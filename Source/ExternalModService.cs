using Verse;

namespace SoulSerpent
{
    public static class ExternalModService
    {
        private static bool? _isNarutoModInstalled = null;

        public static bool IsNarutoModInstalled()
        {
            if (_isNarutoModInstalled is null)
            {
                _isNarutoModInstalled = LoadedModManager.RunningModsListForReading.Any(mod => mod.PackageId == "LifeisGame.WorldOfNaruto");
            }

            return _isNarutoModInstalled.Value;
        }
    }
}
