// Copyright 2019 The Gamedev Guru (http://thegamedev.guru)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#pragma warning disable 4014
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace TheGamedevGuru
{
    /// <summary>
    /// This script will patch Unity UI source code to add extended UI profiling capabilities.
    /// What we want here is to find out who's the guilty guy causing UI Canvas Rebuilds.
    /// Check my blog post at https://thegamedev.guru/unity-ui/profiling-canvas-rebuilds/
    /// Questions? E-mail me at ruben@thegamedev.guru
    /// </summary>
    public class UnityUIExtendedProfiler : EditorWindow
    {
        private bool _addLogs;
        
        [MenuItem("Tools/GamedevGuru/Unity UI Extended Profiler")]
        static void Init()
        {
            var window = (UnityUIExtendedProfiler)EditorWindow.GetWindow(typeof(UnityUIExtendedProfiler));
            window.Show();
        }
        
        void OnGUI()
        {
            _addLogs = EditorGUILayout.Toggle("Turn on logging (spam!)", _addLogs);
            if (GUILayout.Button("Buff my Unity UI Profiling XPerience"))
            {
                var targetUIPath = GetUIDirectory();
                if (CheckAssets() == false || string.IsNullOrEmpty(targetUIPath)) return;
                ApplyPatch(targetUIPath);
            }
            else if (GUILayout.Button("Nerf my Unity UI Profiling XPerience"))
            {
                var targetUIPath = GetUIDirectory();
                if (CheckAssets() == false || string.IsNullOrEmpty(targetUIPath)) return;
                UnapplyPatch(targetUIPath);
            }
            if (GUILayout.Button("I need help!"))
            {
                EditorUtility.DisplayDialog("Help", "Ok, ok, send me an e-mail to ruben@thegamedev.guru and I'll see what I can do. Otherwise, post a comment in the blog article", "Ok thanks");
            }
        }

        private bool CheckAssets()
        {
            if (ExistsOnPath("git.exe") == false)
            {
                EditorUtility.DisplayDialog("Error", "GIT executable not found, did you install it and add it to the PATH environment variable?","Ok :(");
                return false;
            }
            if (File.Exists(GetPatchPath("canvasupdateregistry_patch.diff")) == false)
            {
                EditorUtility.DisplayDialog("Error", "Did you move me to another directory? I cannot find canvasupdateregistry_patch.diff where it was supposed to be","Ok :(");
                return false;
            }
            return true;
        }
        private string GetUIDirectory()
        {
            var process = Process.GetCurrentProcess(); // Or whatever method you are using
            var targetPath = Path.Combine(Path.GetDirectoryName(process.MainModule.FileName), "Data", "Resources", "PackageManager", "BuiltInPackages", "com.unity.ugui", "Runtime", "UI", "Core");
            if (Directory.Exists(targetPath) == false)
            {
                EditorUtility.DisplayDialog("Error", "The UI Package path couldn't be found, are you running Unity 2019.2.0+? Path was: " + targetPath, "Ok :(");
                return null;
            }

            return targetPath;
        }
        
        private void ApplyPatch(string baseUIDirectory)
        {
            var patchPath = "";

            patchPath = GetPatchPath("canvasupdateregistry_patch.diff");
            ExecuteCommand($"/C git.exe apply --directory=\"{baseUIDirectory}\" --verbose --ignore-space-change --ignore-whitespace --whitespace=nowarn --unsafe-paths \"{patchPath}\"");
            
            if (_addLogs)
            {
                patchPath = GetPatchPath("graphic_patch.diff");
                ExecuteCommand($"/C git.exe apply --directory=\"{baseUIDirectory}\" --verbose --ignore-space-change --ignore-whitespace --whitespace=nowarn --unsafe-paths \"{patchPath}\"");
            }
        }
        private void UnapplyPatch(string baseUIDirectory)
        {
            var patchPath = "";
            
            patchPath = GetPatchPath("canvasupdateregistry_patch.diff");
            ExecuteCommand($"/C git.exe apply --reverse --directory=\"{baseUIDirectory}\" --verbose --ignore-space-change --ignore-whitespace --whitespace=nowarn --unsafe-paths \"{patchPath}\"");
            
            if (_addLogs)
            {
                patchPath = GetPatchPath("graphic_patch.diff");
                ExecuteCommand($"/C git.exe apply --reverse --directory=\"{baseUIDirectory}\" --verbose --ignore-space-change --ignore-whitespace --whitespace=nowarn --unsafe-paths \"{patchPath}\"");
            }
        }

        private string GetPatchPath(string patchFilename)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Assets", "TheGamedevGuru", "Unity-UI-Extended-Profiler", "Editor", patchFilename);
        }
        private void ExecuteCommand(string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            //Debug.Log(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            Debug.Log("Ready");
        }
            
        public static bool ExistsOnPath(string fileName)
        {
            try
            {
                string result = Environment
                    .GetEnvironmentVariable("PATH")
                    .Split(';')
                    .FirstOrDefault(s => File.Exists(Path.Combine(s, fileName)));
                return string.IsNullOrEmpty(result) == false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
#pragma warning restore 4014
