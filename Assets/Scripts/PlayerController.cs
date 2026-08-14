using System.Collections;
using UnityEngine;

namespace ElysiumMall
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float speed = 7f;
        public float acceleration = 18f;
        public float dashSpeed = 18f;
        public float dashDuration = 0.16f;
        public float dashCooldown = 0.8f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 facing = Vector3.forward;
        private Vector3 respawnPoint;
        private float dashReadyAt;
        private bool dead;

        public MallApp App { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            respawnPoint = transform.position;
        }

        private void Update()
        {
            if (dead) return;

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            if (input.sqrMagnitude > 0.01f)
            {
                facing = input.normalized;
                velocity = Vector3.MoveTowards(velocity, input * speed, acceleration * Time.deltaTime);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, acceleration * Time.deltaTime);
            }

            bool dashPressed = Input.GetKeyDown(KeyCode.Space)
                || Input.GetButtonDown("Submit")
                || Input.GetKeyDown(KeyCode.JoystickButton0);
            if (dashPressed && Time.time >= dashReadyAt && facing.sqrMagnitude > 0.01f)
            {
                StartCoroutine(Dash());
            }

            velocity.y = -2f;
            controller.Move(velocity * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
        }

        private IEnumerator Dash()
        {
            dashReadyAt = Time.time + dashCooldown;
            float until = Time.time + dashDuration;
            while (!dead && Time.time < until)
            {
                controller.Move(facing * dashSpeed * Time.deltaTime);
                yield return null;
            }
        }

        public void SetRespawnPoint(Vector3 point)
        {
            respawnPoint = point;
        }

        public void Die()
        {
            if (dead) return;
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            dead = true;
            velocity = Vector3.zero;
            App?.SetStatus("Course fail — respawning...");
            yield return new WaitForSeconds(0.65f);
            controller.enabled = false;
            transform.position = respawnPoint;
            controller.enabled = true;
            dead = false;
            App?.SetStatus("Obstacle course active — reach the goal gate.");
        }
    }
}
