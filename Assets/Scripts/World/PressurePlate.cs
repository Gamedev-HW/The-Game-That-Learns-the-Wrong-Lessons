using UnityEngine;
using WrongLessons.Game;
namespace WrongLessons.World
{
    [RequireComponent(typeof(Collider))]
    // Reports whether a rigidbody is pressing the plate.
    public sealed class PressurePlate : MonoBehaviour
    {
        private bool active;
        private void OnCollisionEnter(Collision collision) { Check(collision); }
        private void OnCollisionStay(Collision collision) { Check(collision); }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.rigidbody == null) return;
            if (!active) return;
            active = false;
            LabEvents.RaisePlateChanged(false);
        }
        // Activates the plate when a rigidbody makes contact.
        private void Check(Collision collision)
        {
            if (collision.rigidbody == null || active) return;
            active = true;
            LabEvents.RaisePlateChanged(true);
        }
    }
}
