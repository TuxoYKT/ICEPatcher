using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ICEPatcher
{
    public static class ArksLayer
    {

        public static bool CheckForFilelist(string patchPath)
        {

            foreach (string file in Directory.GetFiles(patchPath, "*.txt"))
            {
                if (Regex.IsMatch(Path.GetFileName(file), @"filelist"))
                {
                    return true;
                }
            }
            return false;
        }

        public static Dictionary<string, List<string>> ProcessFilelistFiles(string patchPath)
        {
            Dictionary<string, string> fileList = new Dictionary<string, string>();

            foreach (var file in Directory.GetFiles(patchPath, "*.txt"))
            {
                if (Regex.IsMatch(Path.GetFileName(file), @"filelist"))
                {
                    foreach (string line in File.ReadAllLines(file))
                    {
                        string[] parts = line.Split(',');
                        fileList[parts[1].Trim().Replace(".text", ".csv")] = parts[0].Trim();
                    }
                }
            }

            Dictionary<string, List<string>> output = new Dictionary<string, List<string>>();

            List<string> excludedFolders = new List<string> { "Files", "Dummy", "Empty" };

            foreach (string root in Directory.GetDirectories(patchPath, "*", SearchOption.AllDirectories))
            {
                if (excludedFolders.Contains(Path.GetFileName(root))) continue;

                foreach (string filePath in Directory.GetFiles(root))
                {
                    string file = Path.GetFileName(filePath);

                    if (fileList.ContainsKey(file))
                    {
                        //Debug.WriteLine(file + " in " + fileList[file]);

                        string outputFolderPath = Path.Combine(root, file);

                        if (!output.ContainsKey(fileList[file]))
                        {
                            output.Add(fileList[file], new List<string>());
                        }
                        output[fileList[file]].Add(outputFolderPath);
                    }
                }
            }

            return output;

        }

        public static void ApplyArksLayerPatchFromFolder(string patchPath, string exportPath)
        {
            string pso2binPath = Patching.GetPSO2BinPath();
            bool isJapaneseClient = Patching.IsJapaneseClient();

            if (pso2binPath == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            Dictionary<string, List<string>> fileList = ProcessFilelistFiles(patchPath);

            Parallel.ForEach(fileList, iceFolder =>
            {
                string relativePath = iceFolder.Key;


                string w32folder = "win32";
                if (relativePath.Length > 2 && relativePath[2] == '\\')
                {
                    w32folder = "win32reboot";
                }

                if (!isJapaneseClient) w32folder += "_na";

                if (File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ||
                        File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)))
                {
                    string PSO2IcePath = File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ?
                                             Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath) :
                                             Path.Combine(pso2binPath, "data", w32folder, relativePath);

                    FilesToPatch filesToPatchList = new FilesToPatch();
                    foreach (var filePath in iceFolder.Value)
                    {
                        filesToPatchList.AddFile(filePath);
                    }

                    if (Patching.AllowBackup)
                    {
                        string backupRelativeICEPath = Path.Combine(Patching.GetBackupPath(), Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                        if (!File.Exists(backupRelativeICEPath))
                        {
                            Patching.BackupICEFile(PSO2IcePath, backupRelativeICEPath);
                        }
                    }

                    string patchIceFolderPath = Path.Combine(patchPath, w32folder, relativePath);
                    string patchName = Path.GetFileName(patchPath);

                    byte[] rawData = Patching.PatchIceFileFromMemory(PSO2IcePath, filesToPatchList);

                    if (rawData != null)
                    {
                        if (exportPath == null)
                        {
                            File.WriteAllBytes(PSO2IcePath, rawData);
                            Debug.WriteLine("Applied changes on: " + PSO2IcePath + " from " + patchName);
                        }
                        else
                        {
                            string relativeICEExportPath = Path.Combine(exportPath, Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                            if (!Directory.Exists(Path.GetDirectoryName(relativeICEExportPath))) Directory.CreateDirectory(Path.GetDirectoryName(relativeICEExportPath));
                            File.WriteAllBytes(relativeICEExportPath, rawData);
                            Debug.WriteLine("Exported applied changes on: " + PSO2IcePath + " from " + patchName);
                        }
                    }
                }
            });
        }
    }
}
