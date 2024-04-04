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
    }

    public static class Patching
    {
        private static string pso2binPath = null;
        private static bool allowBackup = false;

        public static void SetPSO2BinPath(string path)
        {
            pso2binPath = path;
        }

        public static string GetPSO2BinPath()
        {
            return pso2binPath;
        }

        public static void AllowBackup()
        {
            allowBackup = true;
        }

        public static void DisableBackup()
        {
            allowBackup = false;
        }
        private static byte[] PatchFile(byte[] file, string fileName)
        {
            List<byte> bytes = new(file);
            bytes.InsertRange(0, new IceFileHeader(fileName, (uint)bytes.Count).GetBytes());

            return bytes.ToArray();
        }

        private static byte[] PatchTextFile(byte[] groupFile, byte[] textData , string format)
        {
            TextPatcher textPatcher = new();
            string groupFileName = IceFile.getFileName(groupFile);

            if (format == "yaml")
            {
                string textContent = Encoding.UTF8.GetString(textData);
                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
                Dictionary<string, Dictionary<string, string>> dataYaml = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(textContent);

                byte[] textFile = textPatcher.PatchPSO2Text((byte[])groupFile, dataYaml);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(groupFileName, (uint)bytes.Count).GetBytes());
                return bytes.ToArray();
            }
            else if (format == "csv")
            {
                string textContent = Encoding.UTF8.GetString(textData);
                Dictionary<string, List<string>> csvData = textPatcher.ReadCSVFromMemory(textData);
                byte[] textFile = textPatcher.PatchPSO2TextUsingCSV((byte[])groupFile, csvData);

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

                if (extension == "text")
                {
                    // handle text patches
                    if (filesToPatch.FileExists(fileName + ".yaml"))
                    {
                        // load .yaml into memory and deserialize
                        string yamlFile = filesToPatch.GetFileName(fileName + ".yaml");
                        byte[] patchedFile = PatchTextFile(file, filesToPatch.GetFile(yamlFile), "yaml");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                    }
                    else if (filesToPatch.FileExists(fileName + ".csv"))
                    {
                        // load .csv into memory and deserialize
                        string csvFile = filesToPatch.GetFileName(fileName + ".csv");
                        byte[] patchedFile = PatchTextFile(file, filesToPatch.GetFile(csvFile), "csv");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
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

        private static void ApplyPatchFromFolder(string patchPath, string exportPath = null)
        {
            if (pso2binPath == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            string patchName = Path.GetFileName(patchPath);

            foreach (var w32path in Directory.GetDirectories(patchPath))
            {
                string w32folder = Path.GetFileName(w32path);

                bool isReboot = false;

                if (w32folder.Contains("reboot")) isReboot = true;

                foreach (var subpath in Directory.GetDirectories(w32path, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(w32folder, subpath);
                    string patchIceFolderPath = Path.Combine(patchPath, w32path, relativePath);

                    if (Directory.GetDirectories(subpath).Any()) return;
                    if (!Directory.GetFiles(subpath).Any()) return;

                    if (File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ||
                       File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ||
                       File.Exists(Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))) ||
                       File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))))
                    {
                        string PSO2IcePath = File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ?
                                                Path.Combine(pso2binPath, "data", w32folder, relativePath) :

                                                File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ?
                                                Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath) :

                                                File.Exists(Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))) ?
                                                Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot)) :
                                                Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot));

                        FilesToPatch filesToPatchList = ReadFilesToPatchFromFolder(patchIceFolderPath);

                        byte[] rawData = PatchIceFileFromMemory(PSO2IcePath, filesToPatchList);

                        if (rawData != null)
                        {
                            if (exportPath != null)
                            {
                                File.WriteAllBytes(PSO2IcePath, rawData);
                                Debug.WriteLine("Applied changes on: " + PSO2IcePath + " from " + patchName);
                            }
                            else
                            {
                                string relativeICEExportPath = Path.Combine(exportPath, Path.GetRelativePath(Path.Combine(pso2binPath, "data"), PSO2IcePath));
                                File.WriteAllBytes(relativeICEExportPath, rawData);
                                Debug.WriteLine("Exported changes on: " + PSO2IcePath + " from " + patchName);
                            }
                        }
                    }
                }
            }
        }

        public static void ApplyPatch(string patchName, string exportPath = null)
        {
            string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches", patchName);
            Debug.WriteLine("Applying patch: " + patchName);

            ArksLayer arksLayer = new ArksLayer();

            if (arksLayer.CheckForFilelist(patchesPath))
            {
                arksLayer.ApplyArksLayerPatchFromFolder(patchesPath, exportPath);
                return;
            }

            ApplyPatchFromFolder(patchesPath, exportPath);
        }
    }
}
