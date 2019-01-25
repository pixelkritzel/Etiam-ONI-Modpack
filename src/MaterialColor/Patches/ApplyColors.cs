﻿using Harmony;
using MaterialColor.Extensions;
using MaterialColor.Helpers;
using Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using UnityEngine;
using static KInputController;
using System.Reflection;
using MaterialColor.Data;
using MaterialColor.IO;

namespace MaterialColor.Patches
{
    public static class ApplyColors
    {
        [HarmonyPatch(typeof(Game), "OnSpawn")]
        public static class GameStart
        {
            public static void Postfix()
            {
                TryInitMod();
            }

            private static void TryInitMod()
            {
                try
                {
                    State.TileColors = new Color?[Grid.CellCount];
                    Components.BuildingCompletes.OnAdd += Painter.UpdateBuildingColor;
                    Painter.Refresh();
                }
                catch (Exception e)
                {
                    Common.Logger.Log(e);
                }
            }
        }

        [HarmonyPatch(typeof(Ownable), "UpdateTint")]
        public static class Ownable_UpdateTint
        {
            public static void Postfix(Ownable __instance)
            {
                try
                {
                    Color color = ColorHelper.GetComponentMaterialColor(__instance);
                    bool owned = __instance.assignee != null;

                    if (owned)
                    {
                        KAnimControllerBase animBase = __instance.GetComponent<KAnimControllerBase>();
                        if (animBase != null)
                        {
                            animBase.TintColour = color;
                        }
                    }
                }
                catch (Exception e)
                {
                    Common.Logger.LogOnce("Ownable_UpdateTint.Postfix", e);
                }
            }
        }

        [HarmonyPatch(typeof(FilteredStorage), "OnFilterChanged")]
        public static class FilteredStorage_OnFilterChanged
        {
            public static void Postfix(KMonoBehaviour ___root, Tag[] tags)
            {
                try
                {
                    Color color = ColorHelper.GetComponentMaterialColor(___root);
                    bool active = tags != null && tags.Length != 0;

                    if (active)
                    {
                        KAnimControllerBase animBase = ___root.GetComponent<KAnimControllerBase>();
                        if (animBase != null)
                        {
                            animBase.TintColour = color;
                        }
                    }
                }
                catch (Exception e)
                {
                    Common.Logger.LogOnce("FilteredStorage_OnFilterChanged.Postfix", e);
                }
            }
        }

        [HarmonyPatch(typeof(BlockTileRenderer), nameof(BlockTileRenderer.GetCellColour))]
        public static class BlockTileRenderer_GetCellColour
        {
            public static void Postfix(int cell, SimHashes element, BlockTileRenderer __instance, ref Color __result)
            {
                try
                {
                    if
                    (
                        State.Config.Enabled &&
                        State.TileColors[cell].HasValue
                    )
                    {
                        __result *= State.TileColors[cell].Value;
                    }
                }
                catch (Exception e)
                {
                    Common.Logger.LogOnce("EnterCell failed.", e);
                }
            }
        }

        // TODO: run only if deconstructable is a tile
        [HarmonyPatch(typeof(Deconstructable), "OnCompleteWork")]
        public static class Deconstructable_OnCompleteWork_MatCol
        {
            public static void Postfix(Deconstructable __instance)
            {
                try
                {
                    State.TileColors[__instance.GetCell()] = null;
                }
                catch (Exception e)
                {
                    Common.Logger.LogOnce(e);
                }
            }
        }
    }
}