using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElysiumMall
{
    public static class RuntimeFactory
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();

        public static Material Material(Color color)
        {
            if (Materials.TryGetValue(color, out Material existing))
            {
                return existing;
            }

            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader) { color = color };
            Materials[color] = material;
            return material;
        }

        public static GameObject Cube(string name, Vector3 position, Vector3 scale, Color color, Transform parent = null)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            item.transform.localScale = scale;
            MeshRenderer renderer = item.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Material(color);
            return item;
        }

        public static GameObject Cylinder(string name, Vector3 position, Vector3 scale, Color color, Transform parent = null)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            item.transform.localScale = scale;
            MeshRenderer renderer = item.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Material(color);
            return item;
        }

        public static Text WorldLabel(string name, string value, Vector3 position, Color color, Transform parent = null, int fontSize = 36)
        {
            GameObject item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            TextMesh textMesh = item.AddComponent<TextMesh>();
            textMesh.text = value;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            item.AddComponent<Billboard>();
            return null;
        }

        public static GameObject SpriteBillboard(string name, Sprite sprite, Vector3 position, Vector3 scale, Transform parent = null)
        {
            GameObject item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            item.transform.localScale = scale;
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;
            item.AddComponent<Billboard>();
            return item;
        }

        public static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Text UiText(Transform parent, string value, int fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject item = new GameObject("Text");
            item.transform.SetParent(parent, false);
            RectTransform rect = item.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Text text = item.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button UiButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject item = new GameObject(label + "Button");
            item.transform.SetParent(parent, false);
            RectTransform rect = item.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Image image = item.AddComponent<Image>();
            image.color = new Color(0.05f, 0.23f, 0.38f, 0.95f);
            Button button = item.AddComponent<Button>();
            Text text = UiText(item.transform, label, 18, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
