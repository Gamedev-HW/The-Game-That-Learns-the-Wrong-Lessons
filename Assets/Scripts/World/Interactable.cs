using UnityEngine;
namespace WrongLessons.World
{
    // Defines the interaction contract shared by objects the player can activate.
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Interact();
    }
}
