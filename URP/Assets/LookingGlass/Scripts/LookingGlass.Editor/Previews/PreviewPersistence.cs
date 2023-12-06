using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LookingGlass.Editor {
    /// <summary>
    /// A class that ensures our Preview game views close before assembly reloads, and re-open (if needed) after assembly reloads.
    /// </summary>
    [InitializeOnLoad]
    public static class PreviewPersistence {
        /// <summary>
        /// A data class that's serialized to a JSON file to remember whether or not the <see cref="Preview"/> was <see cref="Preview.IsActive">active</see>or not.
        /// </summary>
        [Serializable]
        private class PersistentPreviewData {
            public bool wasPreviewActive;
        }

        private static bool isChangingBetweenPlayMode = false;

        /// <summary>
        /// The JSON file that saves data between
        /// assembly reloads and playmode state changes.
        /// </summary>
        private static string PreviousStateFilePath => Path.Combine(Application.temporaryCachePath, "Preview Data.json");

        static PreviewPersistence() {
            HologramCamera.onListChanged += OnHologramCameraListChanged;
            HologramCamera.onAnyCalibrationReloaded += OnAnyCalibrationReloaded;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            EditorApplication.quitting += DeleteFile;
            EditorSceneManager.sceneOpened += RecheckDisplayTargetOnSceneOpened;
        }

        private static void OnHologramCameraListChanged() {
            if (!Preview.UpdatePreview()) {
                //If the Preview is closed, we still need to force Unity editor to update
                EditorUpdates.ForceUnityRepaint();
            }
        }

        private static void OnAnyCalibrationReloaded(HologramCamera hologramCamera) {
            if (hologramCamera == HologramCamera.Instance)
                EditorUpdates.ForceUnityRepaint();
        }

        //NOTE: The order of operations is:
        //(so the AssemblyReloadEvents take care of ENTERING playmode)

        //The user enters playmode:
        //PlayModeStateChange.ExitingEditMode
        //AssemblyReloadEvents.beforeAssemblyReload
        //AssemblyReloadEvents.afterAssemblyReload
        //PlayModeStateChange.EnteredPlayMode

        //The user exits playmode:
        //PlayModeStateChange.ExitingPlayMode
        //PlayModeStateChange.EnteredEditMode

        //--- --- --- --- --- --- --- --- --- --- --- ---
        //UNITY BUG: Unity throws an internal NullReferenceException in 2019.4+ some time AFTER
        //our code in PlayModeStateChanged(PlayModeStateChange.EnteredEditMode) if we explicitly call
        //      Preview.CloseAllWindowsImmediate()
        //It seems to be because when we have the Preview window open and we return to edit mode, we call:
        //      ConsumeLoadPreview(string) --> Preview.TogglePreview --> Preview.CloseAllWindowsImmediate,
        //So we ALREADY closed our windows..
        //Not sure why this would be an issue though. We'd need to trace through all the calls to see which call even triggers the issue.
        //Lines related to all of this are clearly marked below "//TODO INVESTIGATE: NullReferences upon re-entering edit mode"
        //--- --- --- --- --- --- --- --- --- --- --- ---

        private static void OnBeforeAssemblyReload() {
            SavePreview(PreviousStateFilePath);

            if (!isChangingBetweenPlayMode)
                Preview.CloseAllWindowsImmediate();         //TODO INVESTIGATE: NullReferences upon re-entering edit mode

            EditorUpdates.ForceUnityRepaintImmediate();
        }

        private static void OnAfterAssemblyReload() {
            ConsumeLoadPreview(PreviousStateFilePath);
            EditorUpdates.ForceUnityRepaint();
        }

        private static void PlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.ExitingEditMode:
                    isChangingBetweenPlayMode = true;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    isChangingBetweenPlayMode = false;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    isChangingBetweenPlayMode = true;
                    SavePreview(PreviousStateFilePath);     //TODO INVESTIGATE: NullReferences upon re-entering edit mode
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    isChangingBetweenPlayMode = false;
                    EditorUpdates.Delay(1, () => {
                        ConsumeLoadPreview(PreviousStateFilePath);
                    });
                    break;
            }
        }

        private static void SavePreview(string filePath) {
            //If we're already waiting to consume the previous data, DO NOT overwrite it (upon 2nd attempt to save our state) because we close the preview --
            //It'd ALWAYS write false on every subsequent call to this method!
            if (File.Exists(filePath))
                return;

            string json = JsonUtility.ToJson(new PersistentPreviewData() {
                wasPreviewActive = Preview.IsActive
            }, true);

            File.WriteAllText(filePath, json);

            //TODO INVESTIGATE: NullReferences upon re-entering edit mode
            //      (We used to explicitly close all preview windows here, but no longer)
        }

        private static void ConsumeLoadPreview(string filePath) {
            string json = !File.Exists(filePath) ? "{ }" : File.ReadAllText(filePath);
            PersistentPreviewData data = JsonUtility.FromJson<PersistentPreviewData>(json);
            File.Delete(filePath);

            if (data.wasPreviewActive != Preview.IsActive)
                Preview.TogglePreview();
        }

        private static void DeleteFile() {
            File.Delete(PreviousStateFilePath);
        }

        private static void RecheckDisplayTargetOnSceneOpened(Scene openScene, OpenSceneMode openSceneMode) {
            //NOTE: If we don't wait 1 frame, auto-clicking on the maximize button doesn't seem to work..
            //So let's just wait a frame then! ;)
            EditorUpdates.Delay(1, () => {
                if (!Preview.IsActive)
                    Preview.TogglePreview();
            });
        }
    }
}