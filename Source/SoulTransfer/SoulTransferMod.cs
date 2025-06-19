using HarmonyLib;
using Verse;

namespace SoulTransfer
{
    public class SoulTransferMod : Mod
    {
        public SoulTransferMod(ModContentPack content) : base(content)
        {
            new Harmony(nameof(SoulTransferMod)).PatchAll();
        }
    }
}
