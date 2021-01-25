using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TeamZero.PackageDirectives
{
    internal class PackageProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            IEnumerable<string> changedAssets = importedAssets.Union(deletedAssets);
            foreach (string assetPath in changedAssets)
            {
                string assetName = Path.GetFileName(assetPath);
                if (assetName == "package.json")
                {
                    Apply(true);
                    break;
                }
            }
        }

        [MenuItem("Tools/Package Directives/Force apply")]
        private static void ForceApply() => Apply(true);
        
        private static void Apply(bool enableLog)
        {
            List<string> packages = new List<string>();
            IEnumerable<string> projectItems = PackageInfo.GetNamesInAssetsFolder();
            packages.AddRange(projectItems);
            
            PackageInfo.GetNamesInPackageManager(result =>
            {
                packages.AddRange(result);
                PackageDirectiveFile.Apply(packages, out IEnumerable<string> fileLines);
                if(enableLog)
                    LogResult(fileLines);
            });
        }
        
        private static void LogResult(IEnumerable<string> lines)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine($"{nameof(PackageProcessor)} out:");
            foreach (string line in lines)
                log.AppendLine(line);
            
            Debug.Log(log);
        }
    }
}
