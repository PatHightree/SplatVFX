﻿//NOTE: This is in the runtime assembly, because some code in there needs to run delayed code when running in the UnityEditor

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LookingGlass {
    /// <summary>
    /// A helper class for waiting a given number of Unity editor updates.
    /// </summary>
    public static class EditorUpdates {
        private class WaitingAction {
            public int framesRemaining;
            public Action action;
        }

        /// <summary>
        /// A collection of re-usable <see cref="WaitingAction"/> instances, for performance reasons.
        /// </summary>
        private static Queue<WaitingAction> pool;

        /// <summary>
        /// A collection of actions that are waiting to be called after varying numbers of frames to pass in the Unity editor.
        /// </summary>
        private static List<WaitingAction> waiting;

        static EditorUpdates() {
            EditorApplication.update -= CheckUpdate;
            EditorApplication.update += CheckUpdate;
            AssemblyReloadEvents.beforeAssemblyReload += () => {
                EditorApplication.update -= CheckUpdate;
            };
        }

        public static void Delay(int frames, Action action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (frames <= 0)
                action();

            WaitingAction obj;
            if (pool == null)
                pool = new Queue<WaitingAction>();
            if (pool.Count > 0)
                obj = pool.Dequeue();
            else
                obj = new WaitingAction();

            obj.framesRemaining = frames;
            obj.action = action;

            if (waiting == null)
                waiting = new List<WaitingAction>();
            waiting.Add(obj);
        }

        private static void CheckUpdate() {
            if (waiting == null || waiting.Count <= 0)
                return;
            for (int i = waiting.Count - 1; i >= 0; i--) {
                WaitingAction w = waiting[i];

                w.framesRemaining--;
                if (w.framesRemaining <= 0) {
                    w.action();
                    waiting.RemoveAt(i);
                    pool.Enqueue(w);
                }
            }
        }

        public static void ForceUnityRepaintImmediate() {
            // Spawn an object, then immediately destroy it.
            // This forces Unity to repaint scene, but does not generate a diff in the Unity scene serialization which would require scene to be re-saved
            // Repainting the scene causes Unity to recalculate UI positions for resized GameViewWindow : EditorWindow
            GameObject go = new GameObject();
            GameObject.DestroyImmediate(go);
        }

        public static void ForceUnityRepaint() {
            EditorUpdates.Delay(5, () => {
                ForceUnityRepaintImmediate();
            });
        }
    }
}
#endif
