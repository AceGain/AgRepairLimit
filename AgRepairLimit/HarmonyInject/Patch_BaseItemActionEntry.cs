using HarmonyLib;

namespace HarmonyInject
{
    public class Patch_BaseItemActionEntry
    {
        [HarmonyPatch(typeof(BaseItemActionEntry))]
        [HarmonyPatch(nameof(BaseItemActionEntry.RefreshEnabled))]
        public class Point_RefreshEnabled
        {
            [HarmonyReversePatch]
            public static void Reverse(BaseItemActionEntry __instance) { }
        }
    }
}