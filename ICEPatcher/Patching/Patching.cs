using AquaModelLibrary.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Zamboni;
using Zamboni.IceFileFormats;
using static Zamboni.IceFileFormats.IceHeaderStructures;
using ProgressBar = System.Windows.Forms.ProgressBar;


namespace ICEPatcher
{
    public class FilesToPatch
    {
        private Dictionary<string, byte[]> files;

        public FilesToPatch()
        {
            files = new Dictionary<string, byte[]>();
        }

        public void AddFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            byte[] data = File.ReadAllBytes(filePath);
            files[fileName] = data;
        }

        public void AddFileFromMemory(string fileName, byte[] data)
        {
            files[fileName] = data;
        }

        public byte[] GetFile(string fileName) 
        {
            if (files.ContainsKey(fileName))
                return files[fileName];
            else
                return null;
        }

        public string GetFileName(string fileName)
        {
            if (files.ContainsKey(fileName))
                return fileName;
            else
                return null;
        }

        public bool FileExists(string fileName)
        { return files.ContainsKey(fileName); }

        public int Count { get { return files.Count; } }
    }

    public static class Patching
    {
        private static string pso2binPath = null;
        private static bool allowBackup = false;
        private static bool isJapaneseClient = false;
        private static string backupPath = null;
        private static ProgressBar ProgressBar = null;
        private static string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches");

        public static ProgressBar progressBar
        {
            get { return ProgressBar; }
            set { ProgressBar = value; }
        }

        public static void SetJapaneseClient(bool isJapanese)
        {
            isJapaneseClient = isJapanese;
        }

        public static bool IsJapaneseClient()
        {
            return isJapaneseClient;
        }

        public static bool IsPSO2BinPath(string path)
        {
            if
            (!path.Contains("pso2_bin")
                || !Directory.Exists(Path.Combine(path, "data"))
                || !File.Exists(Path.Combine(path, "pso2.exe"))
            )
                return false;
            return true;
        }

        public static void SetPSO2BinPath(string path)
        {
            pso2binPath = path;
        }

        public static string GetPSO2BinPath()
        {
            return pso2binPath;
        }

        public static bool AllowBackup
        {
            get { return allowBackup; }
            set { allowBackup = value; }
        }

        public static void SetBackupPath(string path)
        {
            backupPath = path;
        }

        public static string GetBackupPath()
        {
            return backupPath;
        }

        public static void SetPatchesPath(string path)
        {
            patchesPath = path;
        }

        public static string GetPatchesPath()
        {
            return patchesPath;
        }

        private static byte[] PatchFile(byte[] file, string fileName)
        {
            List<byte> bytes = new(file);
            bytes.InsertRange(0, new IceFileHeader(fileName, (uint)bytes.Count).GetBytes());

            return bytes.ToArray();
        }

        private static byte[] PatchTextFile(byte[] groupFile, byte[] textData , string format)
        {
            string groupFileName = IceFile.getFileName(groupFile);

            if (format == "yaml")
            {
                string textContent = Encoding.UTF8.GetString(textData);
                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
                Dictionary<string, Dictionary<string, string>> dataYaml = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(textContent);

                byte[] textFile = TextPatcher.PatchPSO2Text((byte[])groupFile, dataYaml);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(groupFileName, (uint)bytes.Count).GetBytes());
                return bytes.ToArray();
            }
            else if (format == "csv")
            {
                string textContent = Encoding.UTF8.GetString(textData);
                Dictionary<string, List<string>> csvData = TextPatcher.ReadCSVFromMemory(textData);
                byte[] textFile = TextPatcher.PatchPSO2TextUsingCSV((byte[])groupFile, csvData);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(groupFileName, (uint)bytes.Count).GetBytes());

                return bytes.ToArray();
            }

            return groupFile;
        }

        private static List<byte[]> PatchGroupFiles(byte[][] groupFiles, FilesToPatch filesToPatch)
        {
            List<byte[]> patchedFiles = new List<byte[]>();
            bool isPatched = false;

            foreach (var file in groupFiles) 
            {
                string fileName = IceFile.getFileName(file);
                string extension = Path.GetExtension(fileName);

                if (extension == ".text")
                {
                    // handle text patches
                    if (filesToPatch.FileExists(fileName + ".yaml") || filesToPatch.FileExists(Path.GetFileNameWithoutExtension(fileName) + ".yaml"))
                    {
                        // load .yaml into memory and deserialize
                        string yamlFile;
                        if (filesToPatch.FileExists(fileName + ".yaml"))
                            yamlFile = fileName + ".yaml";
                        else
                            yamlFile = Path.GetFileNameWithoutExtension(fileName) + ".yaml";

                        byte[] patchedFile = PatchTextFile(file, filesToPatch.GetFile(yamlFile), "yaml");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else if (filesToPatch.FileExists(fileName + ".csv") || filesToPatch.FileExists(Path.GetFileNameWithoutExtension(fileName) + ".csv"))
                    {
                        // load .csv into memory and deserialize
                        string csvFile;
                        if (filesToPatch.FileExists(fileName + ".csv"))
                            csvFile = fileName + ".csv";
                        else
                            csvFile = Path.GetFileNameWithoutExtension(fileName) + ".csv";

                        byte[] patchedFile = PatchTextFile(file, filesToPatch.GetFile(csvFile), "csv");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else
                    {
                        patchedFiles.Add(file);
                    }
                }
                else
                {
                    if (filesToPatch.FileExists(fileName))
                    {
                        patchedFiles.Add(PatchFile(filesToPatch.GetFile(fileName), fileName));
                        isPatched = true;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else
                    {
                        patchedFiles.Add(file);
                    }
                }
            }
            return patchedFiles;
        }

        public static byte[] PatchIceFileFromMemory(string filePath, FilesToPatch filesToPatch)
        {
            byte[] buffer = System.IO.File.ReadAllBytes(filePath);
            IceFile iceFile = IceFile.LoadIceFile(new MemoryStream(buffer));

            if (filesToPatch.Count == 0)
            {
                return null;
            }

            List<byte[]> patchedGroupOneFiles = PatchGroupFiles(iceFile.groupOneFiles, filesToPatch);
            List<byte[]> patchedGroupTwoFiles = PatchGroupFiles(iceFile.groupTwoFiles, filesToPatch);

            IceArchiveHeader header = new();
            byte[] rawData = new IceV4File(header.GetBytes(), patchedGroupOneFiles.ToArray(), patchedGroupTwoFiles.ToArray()).getRawData(false, false);

            return rawData;
        }

        public static string CalculateMD5Hash(string input, bool isReboot)
        {
            // Replace "\" with "/" in the input
            input = input.Replace("\\", "/");

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                // Add "\" back after 2 characters
                if (isReboot) sb.Insert(2, "\\");

                return sb.ToString();
            }
        }

        private static FilesToPatch ReadFilesToPatchFromFolder(string path)
        {
            FilesToPatch filesToPatch = new();
            foreach (var file in Directory.GetFiles(path))
            {
                filesToPatch.AddFile(file);
            }
            return filesToPatch;
        }

        public static void BackupICEFile(string filePath, string relativePath)
        {
            string backupPath = Path.Combine(GetBackupPath(), relativePath);
            if (!Directory.Exists(Path.GetDirectoryName(backupPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
            }

            File.Copy(filePath, backupPath, true);
        }

        public static bool DoesIceFileExist(string pso2binPath, string w32folder, string relativePath, bool isReboot = false)
        {
            return File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ||
                   File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ||
                   File.Exists(Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))) ||
                   File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot)));
        }

        public static string GetPSO2IcePath(string pso2binPath, string w32folder, string relativePath, bool isReboot = false)
        {
            if (File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)))
            {
                return Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath);
            }
            else if (File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)))
            {
                return Path.Combine(pso2binPath, "data", w32folder, relativePath);
            }
            else if (File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))))
            {
                return Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot));
            }
            else
            {
                return Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot));
            }
        }

        private static void ApplyPatchFromFolder(string patchPath, string exportPath = null)
        {
            string pso2binPath = GetPSO2BinPath();
            bool isJapaneseClient = IsJapaneseClient();

            if (pso2binPath == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            string patchName = Path.GetFileName(patchPath);

            Parallel.ForEach(Directory.GetDirectories(patchPath), w32path =>
            {
                string w32folder = Path.GetFileName(w32path);

                bool isReboot = false;

                if (w32folder.Contains("reboot")) isReboot = true;

                Parallel.ForEach(Directory.GetDirectories(w32path, "*", SearchOption.AllDirectories), subpath =>
                {
                    string relativePath = Path.GetRelativePath(w32path, subpath);
                    string patchIceFolderPath = Path.Combine(patchPath, w32path, relativePath);

                    if (Directory.GetDirectories(subpath).Any()) return; // if subpath contains a folder we skip

                    if (!Directory.GetFiles(subpath).Any()) return; // if subpath is empty we skip

                    if (DoesIceFileExist(pso2binPath, w32folder, relativePath, isReboot))
                    {
                        string PSO2IcePath = GetPSO2IcePath(pso2binPath, w32folder, relativePath, isReboot);

                        FilesToPatch filesToPatchList = ReadFilesToPatchFromFolder(patchIceFolderPath);

                        if (AllowBackup)
                        {
                            string backupRelativeICEPath =  Path.Combine(GetBackupPath(), Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                            if (!File.Exists(backupRelativeICEPath))
                            {
                                BackupICEFile(PSO2IcePath, backupRelativeICEPath);
                            }
                        }

                        byte[] rawData = PatchIceFileFromMemory(PSO2IcePath, filesToPatchList);

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
                                Debug.WriteLine("Exported changes applied on: " + PSO2IcePath + " from " + patchName);
                            }
                        }
                    }
                });
            });
        }

        public static void ApplyPatch(string patchName, string exportPath = null)
        {
            string patchPath = Path.Combine(GetPatchesPath(), patchName);
            string fileExtenstion = Path.GetExtension(patchPath);
            Debug.WriteLine("Applying patch: " + patchName);

            if (fileExtenstion == ".zip")
            {
                if (ZipPatching.CheckForFilelistFromZip(patchPath))
                {
                    ZipPatching.ApplyArksLayerPatchFromZip(patchPath, exportPath);
                    return;
                }

                ZipPatching.ApplyPatchFromZip(patchPath, exportPath);
                return;
            }

            if (ArksLayer.CheckForFilelist(patchPath))
            {
                ArksLayer.ApplyArksLayerPatchFromFolder(patchPath, exportPath);
                return;
            }

            ApplyPatchFromFolder(patchPath, exportPath);
        }
    }
}
