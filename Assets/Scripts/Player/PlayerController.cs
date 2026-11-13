using UnityEngine;
using WrongLessons.World;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WrongLessons.Player
{
    [RequireComponent(typeof(CharacterController))]
    // Handles player movement, facing, short-range interaction, and box pushing.
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField, Min(0.1f)] private float interactionDistance = 1.4f;
        [SerializeField, Range(0.01f, 0.4f)] private float interactionCastRadius = 0.18f;
        [SerializeField] private float pushImpulse = 5f;
        [SerializeField] private Vector3 initialFacingDirection = Vector3.forward;

        [SerializeField, Min(0f)] private float turnSpeedDegrees = 720f;
        private CharacterController controller;
        private Transform playerVisual;
        private float verticalVelocity;
        private Vector3 lastFacingDirection;
        public string Prompt { get; private set; } = "";

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            lastFacingDirection = initialFacingDirection.sqrMagnitude > 0.001f
                ? initialFacingDirection.normalized : Vector3.forward;
            playerVisual = transform.Find("Player Visual");
        }

        private void Update()
        {
            if (WrongLessons.Game.LabGameState.HasWon) return;
            Vector2 input = ReadMove();
            Vector3 movement = new Vector3(input.x, 0f, input.y);
            if (movement.sqrMagnitude > 1f) movement.Normalize();
            if (movement.sqrMagnitude > 0.001f)
                lastFacingDirection = movement.normalized;

            // Bootstrap adds the visual after AddComponent<PlayerController>() triggers Awake.
            // Resolve it once it exists, including after a generated-level restart.
            if (playerVisual == null)
                playerVisual = transform.Find("Player Visual");

            // Rotate the visual only: CharacterController remains upright.
            if (playerVisual != null)
            {
                Quaternion target = Quaternion.LookRotation(lastFacingDirection, Vector3.up);
                playerVisual.rotation = Quaternion.RotateTowards(
                    playerVisual.rotation, target, turnSpeedDegrees * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((movement * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);

            // A small sphere cast is more forgiving than a zero-width ray, but
            // still requires the object to be directly in front of the player.
            Vector3 origin = transform.position;
            RaycastHit[] hits = Physics.SphereCastAll(
                origin, interactionCastRadius, lastFacingDirection,
                interactionDistance, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            Interactable button = null;
            Rigidbody box = null;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                button = hit.collider.GetComponentInParent<Interactable>();
                if (button != null) break;
                Rigidbody body = hit.rigidbody;
                if (body != null && !body.isKinematic)
                    box = body;
                // Stop at the first non-player solid collider: no interacting through walls.
                break;
            }

            Prompt = button != null ? button.Prompt : box != null ? "E: Push box" : "";
            if (!ReadInteractPressed()) return;
            if (button != null) button.Interact();
            else if (box != null) box.AddForce(lastFacingDirection * pushImpulse, ForceMode.Impulse);
        }

        // Reads horizontal and vertical keyboard movement input.

        private static Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;
            return new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
                (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }
        // Returns true only on the frame the interaction key is pressed.
        private static bool ReadInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
