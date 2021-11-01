using Mirror;
using UnityEngine;

namespace OpenHogwarts.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonCharacter : NetworkBehaviour
    {
        [SerializeField] private float m_MovingTurnSpeed = 280f;
        [SerializeField] private float m_StationaryTurnSpeed = 90f;
        [SerializeField] private float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] private float m_GravityMultiplier = 2f;
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float m_GroundCheckDistance = 0.1f;

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Animator _animator;

        private bool m_IsGrounded;

        [SyncVar] private float m_ForwardAmount;
        private Vector3 m_GroundNormal;



        public void Move(Vector3 move)
        {
            if (!isLocalPlayer)
                return;

            CheckGroundStatus();

            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            SetForwardAmount(move.magnitude);

            HandleGroundedMovement(move);
            UpdateAnimator(move);
        }

        [Command]
        public void SetForwardAmount(float value)
        {
            m_ForwardAmount = value;
        }

        void UpdateAnimator(Vector3 move)
        {
            _animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            _animator.SetBool("OnGround", m_IsGrounded);

            if (!m_IsGrounded)
            {
                _animator.SetFloat("Jump", _characterController.velocity.y);
            }
        }

        private void HandleGroundedMovement(Vector3 move)
        {
            Debug.DrawRay(transform.position, move, Color.red);

            Vector3 desiredVelocity = move * _moveSpeed * Time.deltaTime;
            _characterController.Move(desiredVelocity);

            if (move.magnitude > 0)
            {
                Quaternion quaternion = Quaternion.LookRotation(move.normalized);
                transform.rotation = quaternion;
            }
        }

        void CheckGroundStatus()
        {
#if UNITY_EDITOR
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif

            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out RaycastHit hitInfo, m_GroundCheckDistance))
            {
                m_GroundNormal = hitInfo.normal;
                m_IsGrounded = true;
            }
            else
            {
                m_GroundNormal = Vector3.up;
                m_IsGrounded = false;
            }
        }

    }
}
