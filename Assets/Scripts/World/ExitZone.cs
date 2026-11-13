using UnityEngine;
using WrongLessons.Game;
using WrongLessons.Player;
namespace WrongLessons.World
{
    // Notifies the game state when the player enters the exit trigger.
    public sealed class ExitZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                FindFirstObjectByType<LabGameState>().ReachExit();
            }
        }
    }
}
