using HarmonyLib;

namespace Patch_BaseItemActionEntry
{
    [HarmonyPatch(typeof(BaseItemActionEntry))]
    [HarmonyPatch(nameof(BaseItemActionEntry.RefreshEnabled))]
    public class RefreshEnabled_Patch
    {
        [HarmonyReversePatch]
        public static void ReversePatch(BaseItemActionEntry __instance) { }
    }
}
