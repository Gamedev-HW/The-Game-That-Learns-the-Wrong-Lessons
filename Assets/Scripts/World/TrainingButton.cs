using UnityEngine;
using WrongLessons.Game;
namespace WrongLessons.World
{
    // Teaches Mimo its first (incorrectly generalized) button rule.
    public sealed class TrainingButton : Interactable
    {
        private bool learned;
        public override string Prompt => learned ? "Mimo learned: press every blue button" : "E: Demonstrate button press to Mimo";
        public override void Interact()
        {
            if (learned) return;
            learned = true;
            LabEvents.RaiseRuleLearned();
        }
    }
}
