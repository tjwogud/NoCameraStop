using HarmonyLib;
using System;
using System.Reflection;
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
            private static readonly HarmonyMethod prefix = new HarmonyMethod(typeof(CameraUpdatePatch), "Prefix");

            public static void Patch(bool patch)
            {
                if (patch)
                    Main.harmony.Patch(method, prefix: prefix);
                else
                    Main.harmony.Unpatch(method, HarmonyPatchType.All, Main.harmony.Id);
            }

            public static void Prefix(scrCamera __instance)
            {
                if (RDC.debug
                    || (scnEditor.instance != null && scrController.instance.paused && __instance.followMode)
                    || (!GCS.d_freeroam && !Input.GetKey(KeyCode.Minus) && !Input.GetKey(KeyCode.Equals)))
                    return;
				if (scnEditor.instance != null)
				{
					scrConductor scrConductor = scrConductor.instance;
					float num = scrConductor.bpm * (float)scrController.instance.speed;
					num *= scrConductor.song.pitch;
					float num2 = 60f / num;
					__instance.camspeed = num2 * 2f;
				}
				else if (!__instance.lockedSpeed)
				{
					__instance.camspeed = (float)(scrConductor.instance.crotchet * 2.0);
					if (__instance.speedAffectedByBPMChanges)
					{
						__instance.camspeed /= (float)scrController.instance.speed;
					}
					if (__instance.speedAffectedBySpeedTrial)
					{
						__instance.camspeed /= GCS.currentSpeedTrial;
					}
				}
				__instance.timer += Time.deltaTime;
				switch (__instance.positionState)
				{
					case PositionState.Origin:
						__instance.topos = new Vector3(0f, 0f, -10f);
						break;
					case PositionState.Levels:
						__instance.topos = new Vector3(__instance.topos.x, -2f, -10f);
						break;
					case PositionState.DLC:
						__instance.topos = new Vector3(__instance.topos.x, -12f, -10f);
						break;
					case PositionState.Xtra:
						__instance.topos = new Vector3(-9f, 13f, -10f);
						break;
					case PositionState.HopGem:
						if (!Persistence.GetUnlockedXF())
						{
							__instance.topos = new Vector3(0f, 3f, -10f);
						}
						else
						{
							__instance.positionState = PositionState.Origin;
						}
						break;
					case PositionState.GemToXtra:
						__instance.topos = new Vector3(__instance.topos.x, __instance.topos.y, -10f);
						break;
					case PositionState.ChangingRoom:
						__instance.topos = new Vector3(0f, __instance.topos.y, -10f);
						break;
					case PositionState.XtraIsland:
						__instance.topos = new Vector3(0f, 11f, -10f);
						break;
					case PositionState.CLS:
						__instance.topos = new Vector3(-5.5f, __instance.topos.y, -10f);
						break;
					case PositionState.CrownIsland:
						__instance.topos = new Vector3(0f, 24.5f, -10f);
						break;
					case PositionState.MuseDashIsland:
						__instance.topos = new Vector3(-25f, 24f, -10f);
						break;
				}
				if (__instance.isMoveTweening || scrController.instance.gameworld)
				{
					float num3 = Vector3.Distance(__instance.frompos, __instance.topos);
					float num4;
					if (num3 > 5f)
					{
						num4 = Mathf.InverseLerp(5f, 10f, num3);
						num4 = Mathf.Min(1f, num4);
					}
					else
					{
						num4 = 0f;
					}
					float num5 = num4 * 0.5f + 1f;
					if (!__instance.followMovingPlatforms)
					{
						num5 = 1f;
					}
					__instance.pos = Vector3.Lerp(__instance.frompos, __instance.topos + __instance.offset, __instance.timer / (__instance.camspeed / num5));
					__instance.camobj.transform.localPosition = __instance.pos + __instance.shake.WithZ(0f);
				}
				if (!__instance.editorRotation)
				{
					__instance.Set("rottimer", __instance.Get<float>("rottimer") + Time.deltaTime);
					__instance.rot = Mathf.Lerp(__instance.fromrot, __instance.torot, __instance.Get<float>("rottimer") / __instance.rotdur);
					__instance.camobj.transform.localEulerAngles = new Vector3(__instance.transform.localEulerAngles.x, __instance.transform.localEulerAngles.y, __instance.rot);
				}
				__instance.coltimer += Time.deltaTime;
				__instance.col = Color.Lerp(__instance.fromcol, __instance.tocol, __instance.coltimer / __instance.coldur);
				__instance.camobj.backgroundColor = __instance.col;
			}
        }
    }
}
