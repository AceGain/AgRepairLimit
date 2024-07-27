using System;
using Platform;
using UnityEngine;

public class XUiC_InjectWindow : XUiController
{
    [PublicizedFrom(EAccessModifier.Private)]
    public static string ID = "";

    [PublicizedFrom(EAccessModifier.Private)]
    public XUiC_SimpleButton btnExit;

    [PublicizedFrom(EAccessModifier.Private)]
    public EApiStatusReason statusReason;

    [PublicizedFrom(EAccessModifier.Private)]
    public string statusReasonAdditionalText;

    [PublicizedFrom(EAccessModifier.Private)]
    public bool wantOffline;

    [PublicizedFrom(EAccessModifier.Private)]
    public Action onLoginComplete;

    public override void Init()
    {
        base.Init();
        ID = base.WindowGroup.ID;
        btnExit = (XUiC_SimpleButton)GetChildById("btnExit");
        btnExit.OnPressed += BtnExit_OnPressed;
    }

    public override bool GetBindingValue(ref string _value, string _bindingName)
    {
        switch (_bindingName)
        {
            case "title":
                _value = string.Format(Localization.Get("xuiInjectTitle"));
                return true;
            case "caption":
                _value = string.Format(Localization.Get("xuiInjectCaption"));
                return true;
            case "reason":
                _value = string.Format(Localization.Get("xuiInjectReason"));
                return true;
            default:
                return base.GetBindingValue(ref _value, _bindingName);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
    {
        Application.Quit();
        String path = Environment.CurrentDirectory + "\\7DaysToDie.exe";
        System.Diagnostics.Process.Start(path);
    }
}
