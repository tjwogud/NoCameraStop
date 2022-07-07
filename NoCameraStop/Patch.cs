using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NoCameraStop
{
    public static class Patch
    {
        //[HarmonyPatch(typeof(scnEditor), "Update")]
        public static class EditorUpdatePatch
        {
            private static readonly MethodInfo method = AccessTools.Method(typeof(scnEditor), "Update");
            private static readonly HarmonyMethod postfix = new HarmonyMethod(typeof(EditorUpdatePatch), "Postfix");

            public static void Patch(bool patch)
            {
                if (patch)
                    Main.harmony.Patch(method, postfix: postfix);
                else
                    Main.harmony.Unpatch(method, HarmonyPatchType.Postfix, Main.harmony.Id);
            }

            public static void Postfix(scnEditor __instance)
            {
                if (Main.Settings.disableEditorZoom && __instance.playMode)
                    scrCamera.instance.userSizeMultiplier = 1;
            }
        }

        //[HarmonyPatch(typeof(scrCamera), "Update")]
        public static class CameraUpdatePatch
        {
            private static readonly MethodInfo method = AccessTools.Method(typeof(scrCamera), "Update");
			private static readonly HarmonyMethod transpiler = new HarmonyMethod(typeof(CameraUpdatePatch), "Transpiler");

			public static void Patch(bool patch)
            {
				if (patch)
					Main.harmony.Patch(method, transpiler: transpiler);
				else
					Main.harmony.Unpatch(method, HarmonyPatchType.All, Main.harmony.Id);
			}

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				bool patch = false;
				int patchOffset = 2;
				int patchLength = 6;
				for (int i = 0; i < instructions.Count(); i++)
                {
					CodeInstruction code = instructions.ElementAt(i);
					if (!patch && (code.operand as FieldInfo)?.Name == "d_freeroam")
                    {
						try
						{
							if (instructions.ElementAt(i + 1).operand is Label
								&& instructions.ElementAt(i + 2).operand is sbyte b1 && b1 == 45
								&& instructions.ElementAt(i + 3).operand is MethodInfo
								&& instructions.ElementAt(i + 4).operand is Label
								&& instructions.ElementAt(i + 5).operand is sbyte b2 && b2 == 61
								&& instructions.ElementAt(i + 6).operand is MethodInfo
								&& instructions.ElementAt(i + 7).operand is Label)
								patch = true;
						}
						catch (Exception) {
						}
                    }
					if (patch && patchOffset > 0)
						patchOffset--;
					else if (patch && patchOffset == 0 && patchLength > 0)
					{
                        patchLength--;
						continue;
					}
					yield return code;
				}
            }
        }
    }
}
