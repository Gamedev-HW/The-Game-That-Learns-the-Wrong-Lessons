using System;

namespace WrongLessons.Game
{
    // Decouples world interactions, game rules, and presentation.
    // Publishes gameplay notifications without coupling gameplay components to the HUD.
    public static class LabEvents
    {
        public static event Action RuleLearned;
        public static event Action<bool> PlateChanged;
        public static event Action<bool> RemoteSwitchChanged;
        public static event Action Won;
        public static void RaiseRuleLearned() => RuleLearned?.Invoke();
        public static void RaisePlateChanged(bool active) => PlateChanged?.Invoke(active);
        public static void RaiseRemoteSwitchChanged(bool active) => RemoteSwitchChanged?.Invoke(active);
        public static void RaiseWon() => Won?.Invoke();
    }
}
