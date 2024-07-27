using HarmonyLib;
using System;
using UnityEngine;

namespace HarmonyInject
{
    public class Patch_ItemActionEntryRepair
    {
        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.OnDisabledActivate))]
        public class Point_OnDisabledActivate
        {
            [HarmonyPostfix]
            public static void Postfix(ItemActionEntryRepair __instance)
            {
                ItemActionEntryRepair.StateTypes overRepairLimit = (ItemActionEntryRepair.StateTypes)Enum.Parse(typeof(ItemActionEntryRepair.StateTypes), "OverRepairLimit");
                if (__instance.state == overRepairLimit)
                {
                    GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairOverLimit"));
                    return;
                }
                ItemActionEntryRepair.StateTypes repairDisable = (ItemActionEntryRepair.StateTypes)Enum.Parse(typeof(ItemActionEntryRepair.StateTypes), "RepairDisable");
                if (__instance.state == repairDisable)
                {
                    GameManager.ShowTooltip(__instance.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairDisable"));
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.RefreshEnabled))]
        public class Point_RefreshEnabled
        {
            public static void Original(ItemActionEntryRepair __instance)
            {
                HarmonyInject.Patch_BaseItemActionEntry.Point_RefreshEnabled.Reverse(__instance);
                __instance.state = ItemActionEntryRepair.StateTypes.Normal;
                XUi xui = __instance.ItemController.xui;
                if (((XUiC_ItemStack)__instance.ItemController).ItemStack.IsEmpty() || ((XUiC_ItemStack)__instance.ItemController).StackLock)
                {
                    return;
                }
                ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue.type);
                __instance.Enabled = __instance.state == ItemActionEntryRepair.StateTypes.Normal;
                if (!__instance.Enabled)
                {
                    __instance.IconName = "ui_game_symbol_book";
                    return;
                }
                ItemValue itemValue = ((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue;
                if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
                {
                    return;
                }

                /*========== Start ==========*/
                // 判断是否存在维修限制
                PassiveEffects passive = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "RepairLimit");
                EntityPlayerLocal entityPlayerLocal = __instance.ItemController.xui.playerUI.entityPlayer;
                int repairLimit = (int)EffectManager.GetValue(passive, itemValue, 0f, entityPlayerLocal, null, default, true, true, true, true, true, 1, false, false);
                // 维修限制小于0，则无法进行维修
                if (repairLimit < 0)
                {
                    ItemActionEntryRepair.StateTypes repairDisable = (ItemActionEntryRepair.StateTypes)Enum.Parse(typeof(ItemActionEntryRepair.StateTypes), "RepairDisable");
                    __instance.state = repairDisable;
                    __instance.Enabled = __instance.state == ItemActionEntryRepair.StateTypes.Normal;
                    return;
                }
                int repairTimes = (int)itemValue.GetType().GetField("RepairTimes").GetValue(itemValue);
                // 维修限制大于0，且维修次数已达限制，则无法进行维修
                if (repairLimit > 0 && repairTimes >= repairLimit)
                {
                    ItemActionEntryRepair.StateTypes overRepairLimit = (ItemActionEntryRepair.StateTypes)Enum.Parse(typeof(ItemActionEntryRepair.StateTypes), "OverRepairLimit");
                    __instance.state = overRepairLimit;
                    __instance.Enabled = __instance.state == ItemActionEntryRepair.StateTypes.Normal;
                    return;
                }
                /*==========  End  ==========*/

                ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
                if (itemClass != null)
                {
                    int b = Convert.ToInt32(Math.Ceiling((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value));
                    if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id)), b) * itemClass.RepairAmount.Value <= 0)
                    {
                        __instance.state = ItemActionEntryRepair.StateTypes.NotEnoughMaterials;
                        __instance.Enabled = __instance.state == ItemActionEntryRepair.StateTypes.Normal;
                    }
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ItemActionEntryRepair __instance)
            {
                Point_RefreshEnabled.Original(__instance);
                return false;
            }

        }

        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
        public class Point_OnActivated
        {
            public static void Original(ItemActionEntryRepair __instance)
            {
                XUi xui = __instance.ItemController.xui;
                XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
                ((XUiC_ItemStack)__instance.ItemController).TimeIntervalElapsedEvent += __instance.ItemActionEntryRepair_TimeIntervalElapsedEvent;
                XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)__instance.ItemController;
                ItemValue itemValue = xUiC_ItemStack.ItemStack.itemValue;
                // 获取待维修工具
                ItemClass forId = ItemClass.GetForId(itemValue.type);
                // 物品槽位
                int sourceToolbeltSlot = ((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) ? xUiC_ItemStack.SlotNumber : (-1));
                // 是否存在指定维修工具
                if (forId.RepairTools == null || forId.RepairTools.Length <= 0)
                {
                    return;
                }

                /*========== Start ==========*/
                // 判断是否存在维修限制
                PassiveEffects passive = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "RepairLimit");
                EntityPlayerLocal entityPlayerLocal = __instance.ItemController.xui.playerUI.entityPlayer;
                int repairLimit = (int)EffectManager.GetValue(passive, itemValue, 0f, entityPlayerLocal, null, default, true, true, true, true, true, 1, false, false);
                // 维修限制小于0，则无法进行维修
                if (repairLimit < 0) { return; }
                int repairTimes = (int)itemValue.GetType().GetField("RepairTimes").GetValue(itemValue);
                Log.Out("AceGame Item Type:{0} RepairLimit:{1} RepairTimes:{2}", itemValue.type, repairLimit, repairTimes);
                // 维修限制大于0，且维修次数已达限制，则无法进行维修
                if (repairLimit > 0 && repairTimes >= repairLimit) { return; }
                /*==========  End  ==========*/

                // 获取维修工具
                ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
                // 不存在维修工具则退出
                if (itemClass == null)
                {
                    return;
                }
                // 计算需使用维修工具数量
                int b = Convert.ToInt32(Math.Ceiling((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value));
                // 计算实际使用维修工具数量
                int num = Mathf.Min(playerInventory.GetItemCount(new ItemValue(itemClass.Id)), b);
                // 计算总计维修值
                int num2 = num * itemClass.RepairAmount.Value;
                // 获取制作UI窗口
                XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();
                if (childByType != null && num2 > 0)
                {
                    // 新建配方
                    Recipe recipe = new Recipe();
                    recipe.count = 1;
                    // 经验值
                    recipe.craftExpGain = Mathf.CeilToInt(forId.RepairExpMultiplier * (float)num);
                    // 维修工具
                    recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id), num));
                    // 物品类型
                    recipe.itemValueType = itemValue.type;
                    // 计算总计维修时间
                    recipe.craftingTime = itemClass.RepairTime.Value * (float)num;
                    // 计算实际维修值
                    num2 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, null, num2, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
                    // 计算实际维修时间
                    recipe.craftingTime = (int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()));
                    // ？？？
                    ItemClass.GetForId(recipe.itemValueType);
                    // 加入维修队列
                    if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), num2, sourceToolbeltSlot))
                    {
                        // 维修队列已满提醒
                        __instance.WarnQueueFull();
                        return;
                    }
                    // 清空物品堆
                    ((XUiC_ItemStack)__instance.ItemController).ItemStack = ItemStack.Empty.Clone();
                    // 移除使用的维修工具
                    playerInventory.RemoveItems(recipe.ingredients);
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ItemActionEntryRepair __instance)
            {
                Point_OnActivated.Original(__instance);
                return false;
            }
        }
    }
}