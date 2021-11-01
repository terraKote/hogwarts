using Mirror;
using UnityEngine;

namespace OpenHogwarts.Player
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : NetworkBehaviour
    {
        [SerializeField] private CameraController _camera;
        private ThirdPersonCharacter m_Character;
        private Vector3 m_Move;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                _camera.gameObject.SetActive(false);
                return;
            }

            _camera.Initialize(Camera.main);
            m_Character = GetComponent<ThirdPersonCharacter>();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer)
                return;

            Vector3 inputValue = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 transformedInput = _camera.transform.TransformDirection(inputValue);
            m_Move = _camera.transform.right * inputValue.x + Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized * inputValue.z;

            m_Character.Move(m_Move);
        }
    }
}
