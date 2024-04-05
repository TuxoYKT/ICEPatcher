using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ICEPatcher
{
    static class ZipPatching
    {
        private static FilesToPatch ReadFilesToPatchFromZip(string path)
        {
            FilesToPatch filesToPatch = new();
            foreach (var file in Directory.GetFiles(path))
            {
                filesToPatch.AddFile(file);
            }
            return filesToPatch;
        }

        public static void ApplyPatchFromZip(string patchPath, string exportPath = null)
        {
            string pso2binPath = Patching.GetPSO2BinPath();
            bool isJapaneseClient = Patching.IsJapaneseClient();

            if (pso2binPath == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            string patchName = Path.GetFileName(patchPath);

            Dictionary<string, List<string>> filesToPatchDict = new Dictionary<string, List<string>>();

            using (var archive = ZipFile.OpenRead(patchPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string fullName = entry.FullName;
                    if (fullName.EndsWith("/") || fullName.EndsWith("\\")) continue;

                    string[] directories = fullName.Split('/');
                    int i = 0;
                    while (i < directories.Length && !directories[i].Contains("win32"))
                    {
                        i++;
                    }
                    directories = directories.Skip(i).ToArray();

                    string w32folder = directories[0];
                    string relativePath = string.Join("/", directories[1..^1]);
                    bool isReboot = w32folder.Contains("reboot");

                    if (Patching.DoesIceFileExist(pso2binPath, w32folder, relativePath, isReboot))
                    {
                        string PSO2IcePath = Patching.GetPSO2IcePath(pso2binPath, w32folder, relativePath, isReboot);
                        Debug.WriteLine(PSO2IcePath);
                        Debug.WriteLine(fullName);

                        string relativeICEPath = Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath);

                        if (!filesToPatchDict.ContainsKey(relativeICEPath))
                        {
                            filesToPatchDict.Add(relativeICEPath, new List<string>());
                        }
                        filesToPatchDict[relativeICEPath].Add(fullName);
                    }
                }

                foreach (var iceFolder in filesToPatchDict)
                {
                    string relativePath = iceFolder.Key;
                    Debug.WriteLine(relativePath);

                    if (Patching.DoesIceFileExist(pso2binPath, "", relativePath))
                    {
                        string PSO2IcePath = Patching.GetPSO2IcePath(pso2binPath, "", relativePath);
                        
                        FilesToPatch filesToPatchList = new FilesToPatch();
                        foreach (var filePath in iceFolder.Value)
                        {
                            string fileName = Path.GetFileName(filePath);
                            Debug.WriteLine(fileName);

                            ZipArchiveEntry entry = archive.GetEntry(filePath);
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(memoryStream);
                                    byte[] entryContentBytes = memoryStream.ToArray();
                                    filesToPatchList.AddFileFromMemory(fileName, entryContentBytes);
                                }
                            }
                        }

                        if (Patching.AllowBackup)
                        {
                            string backupRelativeICEPath = Path.Combine(Patching.GetBackupPath(), Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                            if (!File.Exists(backupRelativeICEPath))
                            {
                                Patching.BackupICEFile(PSO2IcePath, backupRelativeICEPath);
                            }
                        }

                        string patchIceFolderPath = Path.Combine(patchPath, relativePath);

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
                }
            }
        }

        public static bool CheckForFilelistFromZip(string patchPath)
        {
            using (var archive = ZipFile.OpenRead(patchPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(".txt") && Regex.IsMatch(entry.Name, @"filelist"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Dictionary<string, List<string>> ProcessFilelistFromZip(string patchPath)
        {
            Dictionary<string, string> fileList = new Dictionary<string, string>();

            using (var archive = ZipFile.OpenRead(patchPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(".txt") && Regex.IsMatch(entry.Name, @"filelist"))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] parts = line.Split(',');
                                fileList[parts[1].Trim().Replace(".text", ".csv")] = parts[0].Trim();
                            }
                        }
                    }
                }
            }

            Dictionary<string, List<string>> output = new Dictionary<string, List<string>>();

            List<string> excludedFolders = new List<string> { "Files", "Dummy", "Empty" };

            using (var archive = ZipFile.OpenRead(patchPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (excludedFolders.Contains(entry.FullName.Split("/")[1])) continue;

                    string file = Path.GetFileName(entry.FullName);

                    if (fileList.ContainsKey(file))
                    {
                        //Debug.WriteLine(file + " in " + fileList[file]);

                        string outputFolderPath = entry.FullName;

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


        public static void ApplyArksLayerPatchFromZip(string patchPath, string exportPath = null)
        {
            string pso2binPath = Patching.GetPSO2BinPath();
            bool isJapaneseClient = Patching.IsJapaneseClient();

            if (pso2binPath == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            string patchName = Path.GetFileName(patchPath);

            Dictionary<string, List<string>> filesToPatchDict = ProcessFilelistFromZip(patchPath);

            using (var archive = ZipFile.OpenRead(patchPath))
            {
                foreach (var iceFolder in filesToPatchDict)
                {
                    string relativePath = iceFolder.Key;

                    string w32folder = "win32";
                    if (relativePath.Length > 2 && relativePath[2] == '\\')
                    {
                        w32folder = "win32reboot";
                    }

                    if (!isJapaneseClient) w32folder += "_na";

                    Debug.WriteLine(relativePath);

                    if (Patching.DoesIceFileExist(pso2binPath, w32folder, relativePath))
                    {
                        string PSO2IcePath = Patching.GetPSO2IcePath(pso2binPath, w32folder, relativePath);
                        
                        FilesToPatch filesToPatchList = new FilesToPatch();
                        foreach (var filePath in iceFolder.Value)
                        {
                            string fileName = Path.GetFileName(filePath);
                            Debug.WriteLine(fileName);

                            ZipArchiveEntry entry = archive.GetEntry(filePath);
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(memoryStream);
                                    byte[] entryContentBytes = memoryStream.ToArray();
                                    filesToPatchList.AddFileFromMemory(fileName, entryContentBytes);
                                }
                            }
                        }

                        if (Patching.AllowBackup)
                        {
                            string backupRelativeICEPath = Path.Combine(Patching.GetBackupPath(), Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                            if (!File.Exists(backupRelativeICEPath))
                            {
                                Patching.BackupICEFile(PSO2IcePath, backupRelativeICEPath);
                            }
                        }

                        string patchIceFolderPath = Path.Combine(patchPath, relativePath);

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
                }
            }
        }
    }
}