using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using FieldAttributes = Mono.Cecil.FieldAttributes;

public class AgRepairLimit : IModApi
{
    public static bool toRestart = false;

    public void InitMod(Mod _modInstance)
    {
        this.AssemblyInject();
        if (GameManager.IsDedicatedServer && toRestart)
        {
            Log.Out("====================================================================================================");
            Log.Error("AceGame RepairLimit has been successfully injected.");
            Log.Error("The program needs to restart, and please restart later");
            TimeSpan interval = new TimeSpan(0, 0, 1);
            for (int i = 5; i > 0; i--)
            {
                Log.Error("Exit the program in {0} seconds.", i);
                Thread.Sleep(interval);
            }
            Log.Out("====================================================================================================");
            Application.Quit();
            string path = Environment.CurrentDirectory + "\\startdedicated.bat";
            System.Diagnostics.Process.Start(path);
            return;
        }

        this.HarmonyPatch();
    }

    private void HarmonyPatch()
    {
        Log.Out("AceGame RepairLimit Harmony Patch: {0}", base.GetType().ToString());
        Harmony harmony = new Harmony(base.GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void AssemblyInject()
    {
        var path1 = Environment.CurrentDirectory + "\\7DaysToDie_Data\\Managed\\Assembly-CSharp.dll";
        var path2 = Environment.CurrentDirectory + "\\7DaysToDieServer_Data\\Managed\\Assembly-CSharp.dll";
        var assemblyPath = File.Exists(path1) ? path1 : path2;
        Log.Out("AceGame RepairLimit Assembly Path: {0}", assemblyPath);

        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters
        {
            ReadWrite = true,
            ReadingMode = ReadingMode.Immediate
        });
        var assemblyModule = assemblyDefinition.MainModule;

        List<bool> injected = new List<bool>
        {
            this.InjectRepairLimit(assemblyModule),
            this.InjectRepariTimes(assemblyModule),
            this.InjectOverRepairLimit(assemblyModule),
            this.InjectRepairDisable(assemblyModule)
        };

        assemblyDefinition.Write();
        assemblyDefinition.Dispose();

        AgRepairLimit.toRestart = injected.Contains(false);
    }

    private bool InjectRepairLimit(ModuleDefinition assemblyModule)
    {
        // 注入维修限制
        bool hasField = false;
        TypeDefinition assemblyType = assemblyModule.GetType(nameof(PassiveEffects));
        foreach (FieldDefinition field in assemblyType.Fields)
        {
            if (string.Equals(field.Name, "RepairLimit"))
            {
                hasField = true;
                break;
            }
        }
        if (!hasField)
        {
            FieldDefinition field = new FieldDefinition("RepairLimit", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, assemblyType) { Constant = (byte)assemblyType.Fields.Count - 1 };
            assemblyType.Fields.Add(field);
        }
        return hasField;
    }

    private bool InjectRepariTimes(ModuleDefinition assemblyModule)
    {
        // 注入维修次数
        bool hasField = false;
        TypeDefinition assemblyType2 = assemblyModule.GetType(nameof(ItemValue));
        foreach (FieldDefinition field in assemblyType2.Fields)
        {
            if (string.Equals(field.Name, "RepairTimes"))
            {
                hasField = true;
                break;
            }
        }
        if (!hasField)
        {
            FieldDefinition field = new FieldDefinition("RepairTimes", FieldAttributes.Public | FieldAttributes.HasDefault, assemblyModule.TypeSystem.Int32);
            assemblyType2.Fields.Add(field);
        }
        return hasField;
    }

    private bool InjectOverRepairLimit(ModuleDefinition assemblyModule)
    {
        // 注入超出维修限制
        bool hasField = false;
        TypeDefinition assemblyType = assemblyModule.GetType(nameof(ItemActionEntryRepair));
        foreach (TypeDefinition type in assemblyType.NestedTypes)
        {
            if (string.Equals(type.Name, "StateTypes"))
            {
                foreach (FieldDefinition field in type.Fields)
                {
                    if (string.Equals(field.Name, "OverRepairLimit"))
                    {
                        hasField = true;
                        break;
                    }
                }
                if (!hasField)
                {
                    FieldDefinition field = new FieldDefinition("OverRepairLimit", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, type) { Constant = (byte)type.Fields.Count - 1 };
                    type.Fields.Add(field);
                    break;
                }
            }
        }
        return hasField;
    }

    private bool InjectRepairDisable(ModuleDefinition assemblyModule)
    {
        // 注入超出维修限制
        bool hasField = false;
        TypeDefinition assemblyType = assemblyModule.GetType(nameof(ItemActionEntryRepair));
        foreach (TypeDefinition type in assemblyType.NestedTypes)
        {
            if (string.Equals(type.Name, "StateTypes"))
            {
                foreach (FieldDefinition field in type.Fields)
                {
                    if (string.Equals(field.Name, "RepairDisable"))
                    {
                        hasField = true;
                        break;
                    }
                }
                if (!hasField)
                {
                    FieldDefinition field = new FieldDefinition("RepairDisable", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, type) { Constant = (byte)type.Fields.Count - 1 };
                    type.Fields.Add(field);
                    break;
                }
            }
        }
        return hasField;
    }
}
