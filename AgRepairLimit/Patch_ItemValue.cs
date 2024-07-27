using HarmonyLib;

namespace Patch_ItemValue
{
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch(nameof(ItemValue.Clone))]
    public class Clone_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref ItemValue __result, ItemValue __instance)
        {
            __result.GetType().GetField("RepairTimes").SetValue(__result, __instance.GetType().GetField("RepairTimes").GetValue(__instance));
        }
    }
}
