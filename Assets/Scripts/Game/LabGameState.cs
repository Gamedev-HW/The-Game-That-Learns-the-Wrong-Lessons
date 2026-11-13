using UnityEngine;
namespace WrongLessons.Game
{
    // Tracks the puzzle conditions and opens the exit when both switches are active.
    public sealed class LabGameState : MonoBehaviour
    {
        public static bool HasWon { get; private set; }
        public bool PlateActive { get; private set; }
        public bool RemoteActive { get; private set; }
        public bool RuleKnown { get; private set; }
        private GameObject gate;
        public void Configure(GameObject exitGate) { gate = exitGate; HasWon = false; }
        private void OnEnable()
        {
            LabEvents.RuleLearned += OnRuleLearned;
            LabEvents.PlateChanged += OnPlate;
            LabEvents.RemoteSwitchChanged += OnRemote;
        }
        private void OnDisable()
        {
            LabEvents.RuleLearned -= OnRuleLearned;
            LabEvents.PlateChanged -= OnPlate;
            LabEvents.RemoteSwitchChanged -= OnRemote;
        }
        private void OnRuleLearned() => RuleKnown = true;
        private void OnPlate(bool active) { PlateActive = active; UpdateGate(); }
        private void OnRemote(bool active) { RemoteActive = active; UpdateGate(); }
        // Keeps the gate closed until the plate and remote switch are both active.
        private void UpdateGate()
        {
            if (gate != null) gate.SetActive(!(PlateActive && RemoteActive));
        }
        // Completes the level only when both exit conditions are satisfied.
        public void ReachExit()
        {
            if (HasWon || !PlateActive || !RemoteActive) return;
            HasWon = true;
            LabEvents.RaiseWon();
        }
    }
}
