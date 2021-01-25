using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TeamZero.PackageDirectives
{
    internal static class PackageDirectiveFile
    {
        internal static void Apply(IEnumerable<string> packages, out IEnumerable<string> result)
        {
            string path = Path.Combine(Application.dataPath, "csc.rsp");
            IEnumerable<string> fileLines = ReadLines(path);
            IEnumerable<string> userFileLines = ClearDirectives(fileLines);
            IEnumerable<string> directiveLines = CreateDirectives(packages);
            
            result = userFileLines.Union(directiveLines);
            File.WriteAllLines(path, result);
            AssetDatabase.Refresh();
        }

        private static IEnumerable<string> ReadLines(string path) 
            => File.Exists(path) ? File.ReadAllLines(path) : Enumerable.Empty<string>();

        private const string START_PACKAGES_MARK = "#start_packages";
        private const string END_PACKAGES_MARK = "#end_packages";
        private const string DEFINE_COMMAND_FORMAT = "-define:{0}";
        private const string DIRECTIVE_PREFIX = "PACKAGE_";
        private static IEnumerable<string> ClearDirectives(IEnumerable<string> lines)
        {
            bool packagesRegion = false;
            List<string> result = new List<string>();
            foreach (string line in lines)
            {
                if (line == START_PACKAGES_MARK)
                    packagesRegion = true;
                else if (line == END_PACKAGES_MARK)
                    packagesRegion = false;
                else if(!packagesRegion)
                    result.Add(line);
            }

            return result;
        }

        private static readonly char[] NOT_SUPPORT_SYMBOLS = {'.', '-'};
        private static IEnumerable<string> CreateDirectives(IEnumerable<string> packages)
        {
            yield return START_PACKAGES_MARK;

            foreach (string packageName in packages)
            {
                string directive = packageName.ToUpper();
                char[] symbols = directive.ToCharArray();
                foreach (char replacingSymbol in NOT_SUPPORT_SYMBOLS)
                    for (int i = 0; i < symbols.Length; i++)
                    {
                        if (symbols[i] == replacingSymbol)
                            symbols[i] = '_';
                    }
                
                directive = $"{DIRECTIVE_PREFIX}{new string(symbols)}";
                yield return string.Format(DEFINE_COMMAND_FORMAT, directive);
            }
            
            yield return END_PACKAGES_MARK;
        }
    }
}
