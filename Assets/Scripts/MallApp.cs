using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElysiumMall
{
    public class MallApp : MonoBehaviour
    {
        [Header("Local API")]
        public string catalogEndpoint = "http://127.0.0.1:8766/catalog.json";
        public string recommendationEndpoint = "http://127.0.0.1:8766/recommendations.json";

        private readonly Color navy = new Color(0.035f, 0.08f, 0.15f);
        private readonly Color blue = new Color(0.08f, 0.28f, 0.58f);
        private readonly Color cyan = new Color(0.1f, 0.75f, 0.78f);
        private readonly Color gold = new Color(1f, 0.72f, 0.12f);
        private readonly Color pink = new Color(0.95f, 0.25f, 0.52f);

        private Transform worldRoot;
        private Transform courseRoot;
        private Transform mallRoot;
        private PlayerController player;
        private Camera mainCamera;
        private Text statusText;
        private Text catalogText;
        private Text recommendationText;
        private Text controllerText;
        private Text productText;
        private Text titleText;
        private GameObject productPanel;
        private GameObject recommendationPanel;
        private Vector3 mallSpawn = new Vector3(27f, 1.25f, 0f);
        private Vector3 courseSpawn = new Vector3(-35f, 1.25f, 0f);
        private CatalogResponse catalog;
        private bool inCourse = true;
        private float nextControllerPoll;

        private void Awake()
        {
            BuildInterface();
            BuildWorld();
            CatalogService service = GetComponent<CatalogService>();
            if (service == null) service = gameObject.AddComponent<CatalogService>();
            StartCoroutine(service.Load(catalogEndpoint, CatalogLoaded));
            StartCoroutine(service.LoadRecommendations(recommendationEndpoint, "explorer", RecommendationsLoaded));
            ConfigureFullscreenForBuild();
            UpdateControllerStatus();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) HideProduct();
            if (Input.GetKeyDown(KeyCode.F11)) ToggleFullscreen();
            if (Time.unscaledTime >= nextControllerPoll)
            {
                nextControllerPoll = Time.unscaledTime + 1f;
                UpdateControllerStatus();
            }
            if (player != null && mainCamera != null)
            {
                Vector3 target = player.transform.position + new Vector3(0f, 18f, -15f);
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, 8f * Time.deltaTime);
                mainCamera.transform.LookAt(player.transform.position + Vector3.up * 0.4f);
            }
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("VirtualMallWorld").transform;
            courseRoot = new GameObject("ObstacleCourseArea").transform;
            courseRoot.SetParent(worldRoot, false);
            mallRoot = new GameObject("ShoppingArea").transform;
            mallRoot.SetParent(worldRoot, false);

            BuildCourse();
            BuildMall();
            BuildPlayer();
            BuildCamera();
        }

        private void BuildCourse()
        {
            Vector3 center = new Vector3(-27f, 0f, 0f);
            RuntimeFactory.Cube("CourseFloor", center + Vector3.down * 0.2f, new Vector3(25f, 0.4f, 18f), new Color(0.08f, 0.16f, 0.2f), courseRoot);
            CreateBoundary(center + new Vector3(0f, 1f, 9f), new Vector3(25f, 2f, 0.5f), courseRoot);
            CreateBoundary(center + new Vector3(0f, 1f, -9f), new Vector3(25f, 2f, 0.5f), courseRoot);
            CreateBoundary(center + new Vector3(-12.5f, 1f, 0f), new Vector3(0.5f, 2f, 18f), courseRoot);
            CreateBoundary(center + new Vector3(12.5f, 1f, 0f), new Vector3(0.5f, 2f, 18f), courseRoot);
            RuntimeFactory.WorldLabel("CourseSign", "SPEED CIRCUIT", center + new Vector3(0f, 4f, 8.4f), cyan, courseRoot, 42);
            RuntimeFactory.WorldLabel("CourseSubSign", "DASH / DODGE / RETURN", center + new Vector3(0f, 3.2f, 8.4f), Color.white, courseRoot, 22);

            CreateCourseHazards(center);
            GameObject goal = RuntimeFactory.Cylinder("CourseGoal", center + new Vector3(10f, 0.25f, 0f), new Vector3(2f, 0.4f, 2f), gold, courseRoot);
            goal.GetComponent<Collider>().isTrigger = true;
            goal.AddComponent<Goal>();
            RuntimeFactory.WorldLabel("GoalLabel", "MALL EXIT", center + new Vector3(10f, 2.4f, 0f), gold, courseRoot, 24);

            GameObject coursePortal = RuntimeFactory.Cube("CourseToMallPortal", center + new Vector3(11f, 1.2f, -6.5f), new Vector3(2.5f, 2.4f, 0.5f), pink, courseRoot);
            coursePortal.GetComponent<Collider>().isTrigger = true;
            coursePortal.AddComponent<MallPortal>().toCourse = false;
            RuntimeFactory.WorldLabel("PortalLabel", "SHOPPING", center + new Vector3(11f, 3.1f, -6.5f), Color.white, courseRoot, 24);
        }

        private void CreateCourseHazards(Vector3 center)
        {
            GameObject patroller = RuntimeFactory.Cube("PatrollingHazard", center + new Vector3(-6f, 1f, 0f), new Vector3(1.2f, 1.2f, 1.2f), pink, courseRoot);
            patroller.AddComponent<Hazard>();
            Patroller patrol = patroller.AddComponent<Patroller>();
            patrol.start = center + new Vector3(-7f, 1f, -6f);
            patrol.end = center + new Vector3(-7f, 1f, 6f);

            GameObject wanderer = RuntimeFactory.Cube("WanderingHazard", center + new Vector3(-1f, 1f, 0f), new Vector3(1f, 1f, 1f), new Color(0.75f, 0.15f, 0.7f), courseRoot);
            wanderer.AddComponent<Hazard>();
            Wanderer wander = wanderer.AddComponent<Wanderer>();
            wander.bounds = new Bounds(center + new Vector3(-1f, 1f, 0f), new Vector3(7f, 0f, 12f));

            GameObject shooter = RuntimeFactory.Cube("ProjectileShooter", center + new Vector3(5f, 1f, 6f), new Vector3(1.2f, 1.2f, 1.2f), new Color(0.95f, 0.4f, 0.1f), courseRoot);
            ProjectileShooter shooting = shooter.AddComponent<ProjectileShooter>();
            shooting.direction = Vector3.back;

            for (int i = 0; i < 3; i++)
            {
                GameObject spikes = RuntimeFactory.Cylinder("SpikeTrap_" + i, center + new Vector3(3f + i * 2f, 0.25f, -4f), new Vector3(0.9f, 0.7f, 0.9f), gold, courseRoot);
                spikes.GetComponent<Collider>().isTrigger = true;
                spikes.AddComponent<Hazard>();
                spikes.AddComponent<SpikeTrap>();
            }

            RuntimeFactory.WorldLabel("HazardLegend", "PATROL   WANDER   PROJECTILES   SPIKES", center + new Vector3(0f, 0.4f, -8f), new Color(0.65f, 0.75f, 0.78f), courseRoot, 16);
        }

        private void BuildMall()
        {
            Vector3 center = new Vector3(27f, 0f, 0f);
            RuntimeFactory.Cube("MallFloor", center + Vector3.down * 0.2f, new Vector3(34f, 0.4f, 20f), new Color(0.78f, 0.8f, 0.76f), mallRoot);
            CreateBoundary(center + new Vector3(0f, 1f, 10f), new Vector3(34f, 2f, 0.5f), mallRoot);
            CreateBoundary(center + new Vector3(0f, 1f, -10f), new Vector3(34f, 2f, 0.5f), mallRoot);
            CreateBoundary(center + new Vector3(17f, 1f, 0f), new Vector3(0.5f, 2f, 20f), mallRoot);
            RuntimeFactory.WorldLabel("MallSign", "ELYSIUM MALL", center + new Vector3(0f, 5f, 9.3f), blue, mallRoot, 46);
            RuntimeFactory.WorldLabel("MallSubSign", "CATALOG / COMPANIONS / SPEED LAB", center + new Vector3(0f, 4.2f, 9.3f), navy, mallRoot, 20);

            CreateShopfront(center + new Vector3(-11f, 0f, 3.7f), "GREEN HILL GOODS", cyan);
            CreateShopfront(center + new Vector3(0f, 0f, 3.7f), "SPEED LAB", gold);
            CreateShopfront(center + new Vector3(11f, 0f, 3.7f), "TECH WORKSHOP", pink);

            GameObject mallPortal = RuntimeFactory.Cube("MallToCoursePortal", center + new Vector3(-14f, 1.2f, -6.5f), new Vector3(3f, 2.4f, 0.5f), blue, mallRoot);
            mallPortal.GetComponent<Collider>().isTrigger = true;
            mallPortal.AddComponent<MallPortal>().toCourse = true;
            RuntimeFactory.WorldLabel("MallPortalLabel", "SPEED CIRCUIT", center + new Vector3(-14f, 3.1f, -6.5f), Color.white, mallRoot, 22);
        }

        private void CreateShopfront(Vector3 position, string label, Color color)
        {
            RuntimeFactory.Cube(label + "Counter", position, new Vector3(8f, 1f, 1.5f), color, mallRoot);
            RuntimeFactory.Cube(label + "BackWall", position + new Vector3(0f, 2.5f, 1f), new Vector3(8f, 5f, 0.5f), new Color(0.1f, 0.12f, 0.18f), mallRoot);
            RuntimeFactory.WorldLabel(label + "Sign", label, position + new Vector3(0f, 5.2f, 0.7f), color, mallRoot, 24);
        }

        private void BuildPlayer()
        {
            GameObject playerObject = new GameObject("Visitor");
            playerObject.transform.SetParent(worldRoot, false);
            playerObject.transform.position = courseSpawn;
            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            player = playerObject.AddComponent<PlayerController>();
            player.App = this;
            player.SetRespawnPoint(courseSpawn);

            RuntimeFactory.SpriteBillboard("VisitorSprite", PixelSpriteFactory.Create("buddy"), playerObject.transform.position + Vector3.up * 1.5f, Vector3.one * 2.6f, playerObject.transform);
        }

        private void BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            mainCamera.fieldOfView = 48f;
            mainCamera.transform.position = courseSpawn + new Vector3(0f, 18f, -15f);
            mainCamera.transform.LookAt(courseSpawn);
        }

        private void BuildInterface()
        {
            Canvas canvas = RuntimeFactory.CreateCanvas("PortfolioUI");
            GameObject panelObject = new GameObject("InfoPanel");
            panelObject.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.offsetMin = new Vector2(18f, 18f);
            panelRect.offsetMax = new Vector2(-18f, -18f);

            titleText = RuntimeFactory.UiText(panelObject.transform, "ELYSIUM MALL // UNITY VERTICAL SLICE", 24, blue, new Vector2(0f, 0.86f), new Vector2(0.65f, 1f), Vector2.zero, Vector2.zero);
            statusText = RuntimeFactory.UiText(panelObject.transform, "Loading world...", 17, navy, new Vector2(0f, 0.78f), new Vector2(0.68f, 0.86f), Vector2.zero, Vector2.zero);
            catalogText = RuntimeFactory.UiText(panelObject.transform, "Catalog: waiting for JSON API...", 15, navy, new Vector2(0f, 0.70f), new Vector2(0.68f, 0.78f), Vector2.zero, Vector2.zero);
            controllerText = RuntimeFactory.UiText(panelObject.transform, "Controller: scanning...", 15, navy, new Vector2(0f, 0.62f), new Vector2(0.68f, 0.70f), Vector2.zero, Vector2.zero);
            RuntimeFactory.UiText(panelObject.transform, "WASD / left stick / D-pad: move    SPACE / button 0: dash    E / button 1: inspect    F11: fullscreen", 14, navy, new Vector2(0f, 0f), new Vector2(0.80f, 0.05f), Vector2.zero, Vector2.zero);

            Button mallButton = RuntimeFactory.UiButton(panelObject.transform, "SHOPPING AREA", new Vector2(0.76f, 0.90f), new Vector2(1f, 0.97f), Vector2.zero, Vector2.zero);
            mallButton.onClick.AddListener(ReturnToMall);
            Button courseButton = RuntimeFactory.UiButton(panelObject.transform, "OBSTACLE COURSE", new Vector2(0.76f, 0.81f), new Vector2(1f, 0.88f), Vector2.zero, Vector2.zero);
            courseButton.onClick.AddListener(EnterCourse);

            recommendationPanel = new GameObject("RecommendationPanel");
            recommendationPanel.transform.SetParent(panelObject.transform, false);
            RectTransform recommendationRect = recommendationPanel.AddComponent<RectTransform>();
            recommendationRect.anchorMin = new Vector2(0.66f, 0.18f);
            recommendationRect.anchorMax = new Vector2(0.98f, 0.76f);
            recommendationRect.offsetMin = Vector2.zero;
            recommendationRect.offsetMax = Vector2.zero;
            Image recommendationBackground = recommendationPanel.AddComponent<Image>();
            recommendationBackground.color = new Color(0.02f, 0.08f, 0.12f, 0.95f);
            recommendationText = RuntimeFactory.UiText(recommendationPanel.transform, "AI CONCIERGE\n\nLoading recommendations...", 17, Color.white, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));

            CreateProfileButton(panelObject.transform, "EXPLORE", "explorer", new Vector2(0.66f, 0.06f), new Vector2(0.76f, 0.13f));
            CreateProfileButton(panelObject.transform, "STYLE", "style", new Vector2(0.77f, 0.06f), new Vector2(0.87f, 0.13f));
            CreateProfileButton(panelObject.transform, "TECH", "tech", new Vector2(0.88f, 0.06f), new Vector2(0.98f, 0.13f));

            productPanel = new GameObject("ProductPanel");
            productPanel.transform.SetParent(panelObject.transform, false);
            RectTransform productRect = productPanel.AddComponent<RectTransform>();
            productRect.anchorMin = new Vector2(0.66f, 0.18f);
            productRect.anchorMax = new Vector2(0.98f, 0.76f);
            productRect.offsetMin = Vector2.zero;
            productRect.offsetMax = Vector2.zero;
            Image productBackground = productPanel.AddComponent<Image>();
            productBackground.color = new Color(0.02f, 0.08f, 0.12f, 0.95f);
            productText = RuntimeFactory.UiText(productPanel.transform, "", 17, Color.white, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));
            productPanel.SetActive(false);
        }

        private void CreateProfileButton(Transform parent, string label, string profile, Vector2 anchorMin, Vector2 anchorMax)
        {
            Button button = RuntimeFactory.UiButton(parent, label, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            button.onClick.AddListener(() => LoadRecommendations(profile));
        }

        private void CreateBoundary(Vector3 position, Vector3 scale, Transform parent)
        {
            RuntimeFactory.Cube("Boundary", position, scale, navy, parent);
        }

        private void CatalogLoaded(CatalogResponse loaded, string source)
        {
            catalog = loaded ?? new CatalogResponse { products = new ProductRecord[0] };
            catalogText.text = "Catalog: " + source + " // " + catalog.products.Length + " products";
            SpawnKiosks();
        }

        private void SpawnKiosks()
        {
            if (catalog == null || catalog.products == null) return;
            Vector3 center = new Vector3(27f, 0f, 0f);
            for (int i = 0; i < catalog.products.Length; i++)
            {
                ProductRecord product = catalog.products[i];
                float x = center.x - 12f + (i % 3) * 6f;
                float z = center.z - 1.5f - (i / 3) * 5f;
                GameObject kiosk = RuntimeFactory.Cube("Kiosk_" + product.id, new Vector3(x, 0.7f, z), new Vector3(4.5f, 1.4f, 2.2f), new Color(0.93f, 0.94f, 0.88f), mallRoot);
                BoxCollider trigger = kiosk.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(5.5f, 2f, 3f);
                Kiosk interaction = kiosk.AddComponent<Kiosk>();
                interaction.product = product;
                RuntimeFactory.SpriteBillboard("ProductSprite_" + product.id, PixelSpriteFactory.Create(product.spriteKind), new Vector3(x, 2.65f, z), Vector3.one * 2.8f, mallRoot);
                RuntimeFactory.WorldLabel("ProductLabel_" + product.id, product.name, new Vector3(x, 1.8f, z - 1.25f), navy, mallRoot, 16);
            }
        }

        private void LoadRecommendations(string profile)
        {
            CatalogService service = GetComponent<CatalogService>();
            if (service == null) return;
            if (recommendationText != null) recommendationText.text = "AI CONCIERGE\n\nRefreshing " + profile + " profile...";
            StartCoroutine(service.LoadRecommendations(recommendationEndpoint, profile, RecommendationsLoaded));
        }

        private void RecommendationsLoaded(RecommendationResponse response, string source)
        {
            if (recommendationText == null || response == null) return;
            string text = "AI CONCIERGE // " + response.profile.ToUpperInvariant() + "\n";
            text += "\n" + response.rationale + "\n\n";
            if (response.products != null)
            {
                for (int i = 0; i < response.products.Length; i++)
                {
                    ProductRecord product = response.products[i];
                    text += (i + 1) + ". " + product.name + "\n   " + product.vendor + "  $" + product.price.ToString("0.00") + "\n";
                }
            }
            text += "\n" + source + "\nmodel: " + response.model;
            recommendationText.text = text;
        }

        public void EnterCourse()
        {
            inCourse = true;
            HideProduct();
            Teleport(courseSpawn, "Obstacle course active — reach the MALL EXIT or use the SHOPPING button.");
        }

        public void ReturnToMall()
        {
            inCourse = false;
            HideProduct();
            Teleport(mallSpawn, "Shopping area active — approach a kiosk and press E.");
        }

        private void Teleport(Vector3 position, string message)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
            player.SetRespawnPoint(inCourse ? courseSpawn : mallSpawn);
            SetStatus(message);
        }

        public void ShowProduct(ProductRecord product)
        {
            if (product == null) return;
            if (recommendationPanel != null) recommendationPanel.SetActive(false);
            productPanel.SetActive(true);
            productText.text = product.name + "\n\n" + product.vendor + "\n" + product.category + "\n\n$" + product.price.ToString("0.00") + "\n\n" + product.description + "\n\n[API-backed product card]";
        }

        public void HideProduct()
        {
            if (productPanel != null) productPanel.SetActive(false);
            if (recommendationPanel != null) recommendationPanel.SetActive(true);
        }

        public void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void ConfigureFullscreenForBuild()
        {
            if (Application.isEditor) return;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }

        private void ToggleFullscreen()
        {
            if (Application.isEditor) return;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = !Screen.fullScreen;
        }

        private void UpdateControllerStatus()
        {
            if (controllerText == null) return;
            string[] names = Input.GetJoystickNames();
            List<string> connected = new List<string>();
            for (int i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(names[i])) connected.Add(names[i]);
            }

            controllerText.text = connected.Count == 0
                ? "Controller: none detected by Unity (keyboard still active)"
                : "Controller: " + string.Join(" | ", connected.ToArray()) + " // button 0 dash, button 1 inspect";
        }
    }
}
