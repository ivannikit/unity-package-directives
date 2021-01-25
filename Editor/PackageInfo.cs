using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace TeamZero.PackageDirectives
{
    internal static class PackageInfo
    {
        [Serializable]
        private struct PackageManifest
        {
            public string Name => name;
            [SerializeField] private string name;
        }
        
        internal static IEnumerable<string> GetNamesInAssetsFolder()
        {
            string dataPath = Application.dataPath;
            string[] files = Directory.GetFiles(dataPath, "package.json", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                string text = File.ReadAllText(f);
                PackageManifest manifest = JsonUtility.FromJson<PackageManifest>(text);
                yield return manifest.Name;
            }
        }

        private static ListRequest _packagesRequest;
        private static Action<IEnumerable<string>> _packagesRequestCallback;
        internal static void GetNamesInPackageManager(Action<IEnumerable<string>> result)
        {
            _packagesRequestCallback += result;
            if (_packagesRequest == null)
            {
                _packagesRequest = Client.List();
                EditorApplication.update += PackagesRequestProgress;
            }
        }

        private static void PackagesRequestProgress()
        {
            if (_packagesRequest.IsCompleted)
            {
                if (_packagesRequest.Status == StatusCode.Success)
                {
                    IEnumerable<string> packages = _packagesRequest.Result.Select(x => x.name);
                    _packagesRequestCallback?.Invoke(packages);
                }
                else if (_packagesRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_packagesRequest.Error.message);
                    _packagesRequestCallback?.Invoke(Enumerable.Empty<string>());
                }

                EditorApplication.update -= PackagesRequestProgress;
                _packagesRequest = null;
                _packagesRequestCallback = null;
            }
        }
    }
}
