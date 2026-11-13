using UnityEngine;
using WrongLessons.Game;
namespace WrongLessons.Mimo
{
    // Moves Mimo to the remote button after it learns an incorrect rule.
    public sealed class MimoBrain : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.5f;
        private bool learned;
        private bool pressed;
        private Vector3 destination;
        private bool moving;
        private void OnEnable() => LabEvents.RuleLearned += OnRuleLearned;
        private void OnDisable() => LabEvents.RuleLearned -= OnRuleLearned;
        private void OnRuleLearned()
        {
            learned = true;
            // Mimo incorrectly generalizes: all blue buttons must be pressed.
            destination = new Vector3(3f, 0.75f, 3f);
            moving = true;
        }
        private void Update()
        {
            if (!moving) return;
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, destination) < 0.05f)
            {
                moving = false;
                TryPressWrongButton();
            }
        }
        // Activates the remote switch once Mimo has learned the rule.
        public void TryPressWrongButton()
        {
            if (!learned || pressed) return;
            pressed = true;
            LabEvents.RaiseRemoteSwitchChanged(true);
        }
    }
}
