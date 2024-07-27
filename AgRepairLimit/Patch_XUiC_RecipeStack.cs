using HarmonyLib;
using System;
using UnityEngine;

namespace Patch_XUiC_RecipeStack
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.SetRepairRecipe))]
    public class SetRepairRecipe_Patch
    {
        public static bool Original(XUiC_RecipeStack __instance, float _repairTimeLeft, ItemValue _itemToRepair, int _amountToRepair, int _sourceToolbeltSlot = -1)
        {
            if (__instance.isCrafting || (__instance.originalItem != null && __instance.originalItem.type != 0))
            {
                return false;
            }
            EntityPlayerLocal entityPlayer = __instance.xui.playerUI.entityPlayer;
            __instance.recipeCount = 1;
            __instance.craftingTimeLeft = _repairTimeLeft;
            __instance.originalItem = _itemToRepair.Clone();
            __instance.amountToRepair = _amountToRepair;
            __instance.destinationToolbeltSlot = _sourceToolbeltSlot;
            __instance.totalCraftTimeLeft = _repairTimeLeft;
            __instance.oneItemCraftTime = _repairTimeLeft;
            if (__instance.lockIcon != null && _itemToRepair.type != 0)
            {
                ((XUiV_Sprite)__instance.lockIcon.ViewComponent).SpriteName = "ui_game_symbol_wrench";
            }
            __instance.outputQuality = (int)__instance.originalItem.Quality;
            __instance.StartingEntityId = entityPlayer.entityId;
            __instance.recipe = new Recipe();
            __instance.recipe.craftingTime = _repairTimeLeft;
            __instance.recipe.count = 1;
            __instance.recipe.itemValueType = __instance.originalItem.type;
            __instance.recipe.craftExpGain = Mathf.Clamp(__instance.amountToRepair, 0, 200);
            ItemClass itemClass = __instance.originalItem.ItemClass;
            if (itemClass.RepairTools != null && itemClass.RepairTools.Length > 0)
            {
                /*========== Start ==========*/
                // 判断是否存在维修次数限制
                PassiveEffects passive = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "RepairLimit");
                int repairLimit = (int)EffectManager.GetValue(passive, __instance.originalItem, 0f, entityPlayer, null, default, true, true, true, true, true, 1, false, false);
                // 如果存在，则增加维修次数
                if (repairLimit > 0)
                {
                    int repairTimes = (int)__instance.originalItem.GetType().GetField("RepairTimes").GetValue(__instance.originalItem) + 1;
                    __instance.originalItem.GetType().GetField("RepairTimes").SetValue(__instance.originalItem, repairTimes);
                    GameManager.ShowTooltip(__instance.xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiRepairTimesAdd"),repairTimes));
                }
                /*==========  End  ==========*/

                ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value, false);
                if (itemClass2 != null)
                {
                    int num = Mathf.CeilToInt((float)_amountToRepair / (float)itemClass2.RepairAmount.Value);
                    __instance.recipe.ingredients.Add(new ItemStack(ItemClass.GetItem(itemClass.RepairTools[0].Value, false), num));
                }
            }
            __instance.updateRecipeData();
            return true;
        }

        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, XUiC_RecipeStack __instance, float _repairTimeLeft, ItemValue _itemToRepair, int _amountToRepair, int _sourceToolbeltSlot = -1)
        {
            __result = SetRepairRecipe_Patch.Original(__instance, _repairTimeLeft, _itemToRepair, _amountToRepair, _sourceToolbeltSlot);
            return false;
        }
    }

    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.HandleOnPress))]
    public class HandleOnPress_Patch
    {
        public static void Original(XUiC_RecipeStack __instance, XUiController _sender, int _mouseButton)
        {
            if (__instance.recipe == null)
            {
                return;
            }
            XUiC_WorkstationMaterialInputGrid childByType = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
            XUiC_WorkstationInputGrid childByType2 = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
            EntityPlayerLocal entityPlayer = __instance.xui.playerUI.entityPlayer;
            if (childByType != null)
            {
                for (int i = 0; i < __instance.recipe.ingredients.Count; i++)
                {
                    childByType.SetWeight(__instance.recipe.ingredients[i].itemValue.Clone(), __instance.recipe.ingredients[i].count * __instance.recipeCount);
                }
            }
            else
            {
                if (__instance.originalItem != null && !__instance.originalItem.Equals(ItemValue.None))
                {
                    /*========== Start ==========*/
                    // 判断是否存在维修次数限制
                    PassiveEffects passive = (PassiveEffects)Enum.Parse(typeof(PassiveEffects), "RepairLimit");
                    int repairLimit = (int)EffectManager.GetValue(passive, __instance.originalItem, 0f, entityPlayer, null, default, true, true, true, true, true, 1, false, false);
                    // 如果存在，则回退维修次数
                    if (repairLimit > 0)
                    {
                        int repairTimes = (int)__instance.originalItem.GetType().GetField("RepairTimes").GetValue(__instance.originalItem) - 1;
                        __instance.originalItem.GetType().GetField("RepairTimes").SetValue(__instance.originalItem, repairTimes);
                        GameManager.ShowTooltip(__instance.xui.playerUI.entityPlayer, string.Format(Localization.Get("xuiRepairTimesSub"), repairTimes));
                    }
                    /*==========  End  ==========*/

                    ItemStack itemStack = new ItemStack(__instance.originalItem.Clone(), 1);
                    if (!__instance.xui.PlayerInventory.AddItem(itemStack))
                    {
                        GameManager.ShowTooltip(entityPlayer, __instance.inventoryFullDropping);
                        GameManager.Instance.ItemDropServer(new ItemStack(__instance.originalItem.Clone(), 1), entityPlayer.position, Vector3.zero, entityPlayer.entityId, 120f, false);
                    }
                    __instance.originalItem = ItemValue.None.Clone();
                }
                int[] array = new int[__instance.recipe.ingredients.Count];
                for (int j = 0; j < __instance.recipe.ingredients.Count; j++)
                {
                    array[j] = __instance.recipe.ingredients[j].count * __instance.recipeCount;
                    ItemStack itemStack2 = new ItemStack(__instance.recipe.ingredients[j].itemValue.Clone(), array[j]);
                    bool flag;
                    if (childByType2 != null)
                    {
                        flag = (childByType2.AddToItemStackArray(itemStack2) != -1);
                    }
                    else
                    {
                        flag = __instance.xui.PlayerInventory.AddItem(itemStack2, true);
                    }
                    if (flag)
                    {
                        array[j] = 0;
                    }
                    else
                    {
                        array[j] = itemStack2.count;
                    }
                }
                bool flag2 = false;
                for (int k = 0; k < array.Length; k++)
                {
                    if (array[k] > 0)
                    {
                        flag2 = true;
                        GameManager.Instance.ItemDropServer(new ItemStack(__instance.recipe.ingredients[k].itemValue.Clone(), array[k]), entityPlayer.position, Vector3.zero, entityPlayer.entityId, 120f, false);
                    }
                }
                if (flag2)
                {
                    GameManager.ShowTooltip(__instance.xui.playerUI.entityPlayer, __instance.inventoryFullDropping);
                }
            }
            __instance.isCrafting = false;
            __instance.ClearRecipe();
            XUiC_CraftingQueue owner = __instance.Owner;
            owner?.RefreshQueue();
            __instance.windowGroup.Controller.SetAllChildrenDirty(false);
        }

        [HarmonyPrefix]
        public static bool Prefix(XUiC_RecipeStack __instance, XUiController _sender, int _mouseButton)
        {
            HandleOnPress_Patch.Original(__instance, _sender, _mouseButton);
            return false;
        }
    }
}
