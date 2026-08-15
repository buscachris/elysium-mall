using UnityEngine;

namespace MallDemo
{
    public class Billboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 direction = camera.transform.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
