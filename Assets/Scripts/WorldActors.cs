using UnityEngine;

namespace MallDemo
{
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player)) player.Die();
        }
    }

    public class Goal : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player)) player.App.ReturnToMall();
        }
    }

    public class MallPortal : MonoBehaviour
    {
        public bool toCourse;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerController player)) return;
            if (toCourse) player.App.EnterCourse();
            else player.App.ReturnToMall();
        }
    }

    public class Patroller : MonoBehaviour
    {
        public Vector3 start;
        public Vector3 end;
        public float speed = 2.5f;
        private float progress;
        private bool forward = true;

        private void Update()
        {
            float delta = speed * Time.deltaTime / Mathf.Max(0.01f, Vector3.Distance(start, end));
            progress += (forward ? delta : -delta);
            if (progress >= 1f) { progress = 1f; forward = false; }
            if (progress <= 0f) { progress = 0f; forward = true; }
            transform.position = Vector3.Lerp(start, end, progress);
        }
    }

    public class Wanderer : MonoBehaviour
    {
        public Bounds bounds;
        public float speed = 2f;
        private Vector3 target;

        private void Start() { ChooseTarget(); }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude < 0.1f) ChooseTarget();
        }

        private void ChooseTarget()
        {
            target = new Vector3(Random.Range(bounds.min.x, bounds.max.x), transform.position.y, Random.Range(bounds.min.z, bounds.max.z));
        }
    }

    public class ProjectileShooter : MonoBehaviour
    {
        public Vector3 direction = Vector3.right;
        public float interval = 2.2f;
        public float speed = 8f;
        private float nextShot;

        private void Update()
        {
            if (Time.time < nextShot) return;
            nextShot = Time.time + interval;
            GameObject projectile = RuntimeFactory.Cube("CourseProjectile", transform.position, new Vector3(0.35f, 0.35f, 0.35f), new Color(1f, 0.3f, 0.1f));
            projectile.AddComponent<Hazard>();
            projectile.AddComponent<Projectile>().direction = direction.normalized;
            Destroy(projectile, 7f);
        }
    }

    public class Projectile : MonoBehaviour
    {
        public Vector3 direction;
        public float speed = 8f;
        private void Update() { transform.position += direction * speed * Time.deltaTime; }
    }

    public class SpikeTrap : MonoBehaviour
    {
        public float cycle = 2.4f;
        private Hazard hazard;
        private Vector3 baseScale;

        private void Start()
        {
            hazard = GetComponent<Hazard>();
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float phase = Mathf.Repeat(Time.time, cycle) / cycle;
            float height = Mathf.SmoothStep(0.1f, 1f, Mathf.PingPong(phase * 2f, 1f));
            transform.localScale = new Vector3(baseScale.x, baseScale.y * height, baseScale.z);
            if (hazard != null) hazard.enabled = height > 0.62f;
        }
    }

    public class Kiosk : MonoBehaviour
    {
        public ProductRecord product;
        private PlayerController nearby;

        private void Update()
        {
            bool inspectPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1);
            if (nearby != null && inspectPressed) nearby.App.ShowProduct(product);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                nearby = player;
                player.App.SetStatus("Press E to inspect " + product.name + ".");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player) && nearby == player)
            {
                nearby = null;
                player.App.SetStatus("Explore the mall or use the COURSE gate.");
            }
        }
    }
}
