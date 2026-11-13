using UnityEngine;
using WrongLessons.Mimo;
namespace WrongLessons.World
{
    [RequireComponent(typeof(Collider))]
    // Allows Mimo to activate a button when entering its trigger volume.
    public sealed class MimoTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            MimoBrain mimo = other.GetComponent<MimoBrain>();
            if (mimo != null) mimo.TryPressWrongButton();
        }
    }
}
