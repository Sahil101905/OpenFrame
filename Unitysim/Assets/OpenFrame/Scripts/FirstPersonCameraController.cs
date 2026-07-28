using UnityEngine;

namespace OpenFrame.Simulator
{
    public sealed class FirstPersonCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float lookSensitivity = 2f;

        private float pitch;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            float yaw = Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -80f, 80f);

            transform.Rotate(Vector3.up, yaw, Space.World);
            Vector3 eulerAngles = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(pitch, eulerAngles.y, 0f);

            Vector3 input = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                0f,
                Input.GetAxisRaw("Vertical"));

            transform.position += transform.TransformDirection(input.normalized) * moveSpeed * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
