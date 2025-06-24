using HarmonyLib;
using Verse;

namespace SoulSerpent
{
    public class SoulSerpentMod : Mod
    {
        public SoulSerpentMod(ModContentPack content) : base(content)
        {
            new Harmony(nameof(SoulSerpentMod)).PatchAll();
        }
    }
}
