using HarmonyLib;

namespace Patch_XUiC_MainMenu
{
    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch(nameof(XUiC_MainMenu.Open))]
    public class Patch_XUiC_MainMenu
    {
        [HarmonyPrefix]
        public static bool Prefix(XUiC_MainMenu __instance, XUi _xuiInstance)
        {
            if (AgRepairLimit.toRestart == true)
            {
                _xuiInstance.playerUI.windowManager.Open(XUiC_InjectWindow.ID, true, false, true);
                return false;
            }
            //_xuiInstance.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
            return true;
        }
    }
}
