using HarmonyLib;
using Verse;

namespace SoulWeaver
{
    public class SoulWeaverMod : Mod
    {
        public SoulWeaverMod(ModContentPack content) : base(content)
        {
            new Harmony(nameof(SoulWeaverMod)).PatchAll();
        }
    }
}
