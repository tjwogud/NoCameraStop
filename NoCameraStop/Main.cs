using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace NoCameraStop
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Harmony harmony;
        public static bool IsEnabled = false;
        public static Settings Settings;

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            Logger.Log("Loading Settings...");
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Logger.Log("Load Completed!");
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                if (Settings.disableCameraStop)
                    Patch.CameraUpdatePatch.Patch(true);
                if (Settings.disableEditorZoom)
                    Patch.EditorUpdatePatch.Patch(true);
            }
            else
                harmony.UnpatchAll(modEntry.Info.Id);
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            bool disableCameraStop;
            bool disableEditorZoom;
            if (RDString.language == SystemLanguage.Korean)
            {
                disableCameraStop = GUILayout.Toggle(Settings.disableCameraStop, "+, - 키를 누를 시 카메라 멈춤 비활성화");
                disableEditorZoom = GUILayout.Toggle(Settings.disableEditorZoom, "에디터 플레이중 스크롤 비활성화");
            } else {
                disableCameraStop = GUILayout.Toggle(Settings.disableCameraStop, "Disable camera stop when press +, -");
                disableEditorZoom = GUILayout.Toggle(Settings.disableEditorZoom, "Disable mouse scroll while playing in editor");
            }
            if (Settings.disableCameraStop != disableCameraStop)
            {
                Logger.Log("disableCameraStop " + (disableCameraStop ? "enabled" : "disabled"));
                Settings.disableCameraStop = disableCameraStop;
                Patch.CameraUpdatePatch.Patch(disableCameraStop);
            }
            if (Settings.disableEditorZoom != disableEditorZoom)
            {
                Logger.Log("disableCameraStop " + (disableEditorZoom ? "enabled" : "disabled"));
                Settings.disableEditorZoom = disableEditorZoom;
                Patch.EditorUpdatePatch.Patch(disableEditorZoom);
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Logger.Log("Saving Settings...");
            Settings.Save(modEntry);
            Logger.Log("Save Completed!");
        }
    }
}