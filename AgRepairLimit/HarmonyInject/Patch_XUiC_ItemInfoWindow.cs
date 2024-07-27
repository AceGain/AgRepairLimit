using HarmonyLib;
using NGUI;
using System;

namespace HarmonyInject
{
    public class Patch_XUiC_ItemInfoWindow
    {
        [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
        [HarmonyPatch(nameof(XUiC_ItemInfoWindow.GetBindingValue))]
        public class Point_GetBindingValue
        {
            [HarmonyPostfix]
            public static void Postfix(XUiC_ItemInfoWindow __instance, ref bool __result, ref string value, string bindingName)
            {
                if (!__result)
                {
                    switch (bindingName)
                    {
                        case "itemstattitle8":
                            value = ((__instance.itemClass != null) ? __instance.GetStatTitle(7) : "");
                            __result = true;
                            break;
                        case "itemstat8":
                            value = ((__instance.itemClass != null) ? __instance.GetStatValue(7) : "");
                            __result = true;
                            break;
                        default:
                            __result = false;
                            break;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
        [HarmonyPatch(nameof(XUiC_ItemInfoWindow.GetStatValue))]
        public class Point_GetStatValue
        {
            [HarmonyPostfix]
            public static void Postfix(XUiC_ItemInfoWindow __instance, ref string __result, int index)
            {
                if (__instance.itemDisplayEntry == null || __instance.itemDisplayEntry.DisplayStats.Count <= index)
                {
                    return;
                }
                DisplayInfoEntry infoEntry = __instance.itemDisplayEntry.DisplayStats[index];
                PassiveEffects passive = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "RepairLimit");
                if (infoEntry.StatType == passive)
                {
                    string itemStackValue = XUiM_ItemStack.GetStatItemValueTextWithModColoring(__instance.itemStack, __instance.xui.playerUI.entityPlayer, infoEntry);
                    if (string.Equals(itemStackValue, "0"))
                    {
                        __result = Localization.Get("statRepairUnlimit");
                    }
                    else if (string.Equals(itemStackValue, "-1"))
                    {
                        __result = Localization.Get("statRepairDisable");
                    }

                    // CompareStack 和 EquippedStack 永远同时有值，此处判断尽量与源代码格式保持一致
                    if (!__instance.CompareStack.IsEmpty() && __instance.CompareStack != __instance.itemStack)
                    {
                        string compareStackValue = XUiM_ItemStack.GetStatItemValueTextWithModColoring(__instance.CompareStack, __instance.xui.playerUI.entityPlayer, infoEntry);
                        if (!string.Equals(compareStackValue, itemStackValue))
                        {
                            if (string.Equals(itemStackValue, "0"))
                            {
                                __result += "([FF0000]→" + (string.Equals(compareStackValue, "-1") ? Localization.Get("statRepairDisable") : compareStackValue) + "[-])";
                            }
                            else if (string.Equals(itemStackValue, "-1"))
                            {
                                __result += "([00FF00]→" + (string.Equals(compareStackValue, "0") ? Localization.Get("statRepairUnlimit") : compareStackValue) + "[-])";
                            }
                            else
                            {
                                if (string.Equals(compareStackValue, "0"))
                                {
                                    __result = itemStackValue + "([00FF00]→" + Localization.Get("statRepairUnlimit") + "[-])";
                                }
                                else if (string.Equals(compareStackValue, "-1"))
                                {
                                    __result = itemStackValue + "([FF0000]→" + Localization.Get("statRepairDisable") + "[-])";
                                }
                            }
                        }
                        return;
                    }

                    if (!__instance.EquippedStack.IsEmpty() && __instance.EquippedStack != __instance.itemStack)
                    {
                        string equippedStackValue = XUiM_ItemStack.GetStatItemValueTextWithModColoring(__instance.EquippedStack, __instance.xui.playerUI.entityPlayer, infoEntry);
                        if (!string.Equals(equippedStackValue, itemStackValue))
                        {
                            if (string.Equals(itemStackValue, "0"))
                            {
                                __result += "([FF0000]→" + (string.Equals(equippedStackValue, "-1") ? Localization.Get("statRepairDisable") : equippedStackValue) + "[-])";
                            }
                            else if (string.Equals(itemStackValue, "-1"))
                            {
                                __result += "([00FF00]→" + (string.Equals(equippedStackValue, "0") ? Localization.Get("statRepairUnlimit") : equippedStackValue) + "[-])";
                            }
                            else
                            {
                                if (string.Equals(equippedStackValue, "0"))
                                {
                                    __result = itemStackValue + "([00FF00]→" + Localization.Get("statRepairUnlimit") + "[-])";
                                }
                                else if (string.Equals(equippedStackValue, "-1"))
                                {
                                    __result = itemStackValue + "([FF0000]→" + Localization.Get("statRepairDisable") + "[-])";
                                }
                            }
                        }
                        return;
                    }

                }
            }

        }
    }
}