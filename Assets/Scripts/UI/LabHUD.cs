using UnityEngine;
using WrongLessons.Game;
using WrongLessons.Player;
namespace WrongLessons.UI
{
    // Displays instructions, interaction prompts, and feedback from game events.
    public sealed class LabHUD : MonoBehaviour
    {
        private string message = "Teach Mimo by pressing the blue training button.";
        private void OnEnable()
        {
            LabEvents.RuleLearned += OnLearned;
            LabEvents.PlateChanged += OnPlate;
            LabEvents.RemoteSwitchChanged += OnRemote;
            LabEvents.Won += OnWin;
        }
        private void OnDisable()
        {
            LabEvents.RuleLearned -= OnLearned;
            LabEvents.PlateChanged -= OnPlate;
            LabEvents.RemoteSwitchChanged -= OnRemote;
            LabEvents.Won -= OnWin;
        }
        private void OnLearned() => message = "Mimo misunderstood! It will press the other blue button.";
        private void OnPlate(bool active) => message = active ? "Plate active! Keep the box on it." : "Plate released. Put the box back.";
        private void OnRemote(bool active) => message = active ? "Mimo pressed the remote button. Now push the box onto the plate!" : "Remote button inactive.";
        private void OnWin() => message = "YOU ESCAPED!";
        private void OnGUI()
        {
            GUI.Box(new Rect(12, 12, 590, 108), "THE GAME THAT LEARNS THE WRONG LESSONS");
            GUI.Label(new Rect(24, 40, 560, 25), message);
            GUI.Label(new Rect(24, 67, 560, 25), "WASD move | E interact / push | R restart | Mouse not needed (fixed camera)");
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) GUI.Label(new Rect(24, 91, 560, 25), player.Prompt);
            if (LabGameState.HasWon) GUI.Box(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 45, 300, 90), "WIN! Press R to restart");
        }
    }
}
