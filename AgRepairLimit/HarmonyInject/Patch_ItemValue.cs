using HarmonyLib;
using System;
using System.IO;
using static InvGameItem;

namespace HarmonyInject
{
    public class Patch_ItemValue
    {
        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.Clear))]
        public class Point_Clear
        {
            [HarmonyPostfix]
            public static void Postfix(ItemValue __instance)
            {
                Traverse.Create(__instance).Field("RepairTimes").SetValue(0);
            }
        }

        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.Clone))]
        public class Point_Clone
        {
            [HarmonyPostfix]
            public static void Postfix(ref ItemValue __result, ItemValue __instance)
            {
                Traverse.Create(__result).Field("RepairTimes").SetValue(Traverse.Create(__instance).Field("RepairTimes").GetValue<int>());
            }
        }

        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.ReadData))]
        [HarmonyPatch(new Type[] { typeof(BinaryReader), typeof(int) })]
        public class Point_ReadData
        {
            public static void Original(ItemValue __instance, BinaryReader _br, int version)
            {
                int num = 0;
                if (version >= 8)
                {
                    num = _br.ReadByte();
                }
                __instance.type = _br.ReadUInt16();
                if ((num & 1) > 0)
                {
                    __instance.type += Block.ItemsStartHere;
                }
                if (version < 8 && __instance.type >= 32768)
                {
                    __instance.type += 32768;
                }
                if (version > 5)
                {
                    __instance.UseTimes = _br.ReadSingle();
                }
                else
                {
                    __instance.UseTimes = (int)_br.ReadUInt16();
                }

                /*========== Start ==========*/
                int repairTimes = _br.ReadInt32();
                Traverse.Create(__instance).Field("RepairTimes").SetValue(repairTimes);
                /*==========  End  ==========*/

                __instance.Quality = _br.ReadUInt16();
                __instance.Meta = _br.ReadUInt16();
                if (__instance.Meta >= 65535)
                {
                    __instance.Meta = -1;
                }
                if (version > 6)
                {
                    int num2 = _br.ReadByte();
                    for (int i = 0; i < num2; i++)
                    {
                        string key = _br.ReadString();
                        TypedMetadataValue tmv = TypedMetadataValue.Read(_br);
                        __instance.SetMetadata(key, tmv);
                    }
                }
                if ((version > 4 || __instance.HasQuality) && !(__instance.ItemClass is ItemClassModifier))
                {
                    byte b = _br.ReadByte();
                    __instance.Modifications = new ItemValue[b];
                    if (b != 0)
                    {
                        for (int j = 0; j < b; j++)
                        {
                            if (_br.ReadBoolean())
                            {
                                __instance.Modifications[j] = new ItemValue();
                                __instance.Modifications[j].Read(_br);
                            }
                            else
                            {
                                __instance.Modifications[j] = ItemValue.None.Clone();
                            }
                        }
                    }
                    b = _br.ReadByte();
                    __instance.CosmeticMods = new ItemValue[b];
                    if (b != 0)
                    {
                        for (int k = 0; k < b; k++)
                        {
                            if (_br.ReadBoolean())
                            {
                                __instance.CosmeticMods[k] = new ItemValue();
                                __instance.CosmeticMods[k].Read(_br);
                            }
                            else
                            {
                                __instance.CosmeticMods[k] = ItemValue.None.Clone();
                            }
                        }
                    }
                }
                if (version > 1)
                {
                    __instance.Activated = _br.ReadByte();
                }
                if (version > 2)
                {
                    __instance.SelectedAmmoTypeIndex = _br.ReadByte();
                }
                if (version > 3)
                {
                    __instance.Seed = _br.ReadUInt16();
                    if (__instance.type == 0)
                    {
                        __instance.Seed = 0;
                    }
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ItemValue __instance, BinaryReader _br, int version)
            {
                Point_ReadData.Original(__instance, _br, version);
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.Write))]
        [HarmonyPatch(new Type[] { typeof(BinaryWriter) })]
        public class Point_Write
        {
            public static void Original(ItemValue __instance, BinaryWriter _bw)
            {
                if (__instance.IsEmpty())
                {
                    _bw.Write((byte)0);
                    return;
                }
                _bw.Write((byte)8);
                int num = __instance.type;
                byte value = 0;
                if (__instance.type >= Block.ItemsStartHere)
                {
                    value = 1;
                    num -= Block.ItemsStartHere;
                }
                _bw.Write(value);
                _bw.Write((ushort)num);
                _bw.Write(__instance.UseTimes);

                /*========== Start ==========*/
                int repairTimes = Traverse.Create(__instance).Field("RepairTimes").GetValue<int>();
                _bw.Write(repairTimes);
                /*==========  End  ==========*/

                _bw.Write(__instance.Quality);
                _bw.Write((ushort)__instance.Meta);
                int num2 = ((__instance.Metadata != null) ? __instance.Metadata.Count : 0);
                _bw.Write((byte)num2);
                if (__instance.Metadata != null)
                {
                    foreach (string key in __instance.Metadata.Keys)
                    {
                        if (__instance.Metadata[key]?.GetValue() != null)
                        {
                            _bw.Write(key);
                            TypedMetadataValue.Write(__instance.Metadata[key], _bw);
                        }
                    }
                }
                if (!(__instance.ItemClass is ItemClassModifier))
                {
                    _bw.Write((byte)__instance.Modifications.Length);
                    for (int i = 0; i < __instance.Modifications.Length; i++)
                    {
                        bool flag = __instance.Modifications[i] != null && !__instance.Modifications[i].IsEmpty();
                        _bw.Write(flag);
                        if (flag)
                        {
                            __instance.Modifications[i].Write(_bw);
                        }
                    }
                    _bw.Write((byte)__instance.CosmeticMods.Length);
                    for (int j = 0; j < __instance.CosmeticMods.Length; j++)
                    {
                        bool flag2 = __instance.CosmeticMods[j] != null && !__instance.CosmeticMods[j].IsEmpty();
                        _bw.Write(flag2);
                        if (flag2)
                        {
                            __instance.CosmeticMods[j].Write(_bw);
                        }
                    }
                }
                _bw.Write(__instance.Activated);
                _bw.Write(__instance.SelectedAmmoTypeIndex);
                if (__instance.type == 0)
                {
                    __instance.Seed = 0;
                }
                _bw.Write(__instance.Seed);
                ItemClass itemClass = ItemClass.list[__instance.type];
                if (itemClass == null)
                {
                    if (__instance.type != 0)
                    {
                        Log.Error("No ItemClass entry for type " + __instance.type);
                    }
                }
                else
                {
                    ((!itemClass.IsBlock()) ? ItemClass.nameIdMapping : Block.nameIdMapping)?.AddMapping(__instance.type, itemClass.Name);
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ItemValue __instance, BinaryWriter _bw)
            {
                Point_Write.Original(__instance, _bw);
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.Equals))]
        [HarmonyPatch(new Type[] { typeof(ItemValue) })]
        public class Point_Equals
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, ItemValue __instance, ItemValue _other)
            {
                if (__result)
                {
                    __result = Traverse.Create(__instance).Field("RepairTimes").GetValue<int>() == Traverse.Create(_other).Field("RepairTimes").GetValue<int>();
                }
            }
        }

        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch(nameof(ItemValue.ToString))]
        public class Point_ToString
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result, ItemValue __instance)
            {
                __result = __result + " rt=" + Traverse.Create(__instance).Field("RepairTimes").GetValue<string>();
            }
        }
    }
}