using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Zamboni;
using Zamboni.IceFileFormats;
using static Zamboni.IceFileFormats.IceHeaderStructures;
using System.Windows.Forms;
using System.Reflection;
using System.Security.Cryptography;
using SixLabors.ImageSharp.PixelFormats;


namespace ICEPatcher
{
    public class ICEPatcherCommon
    {
        public string OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {   
                Title = "Select ICE file",
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null; // User canceled the dialog
            }
        }

        public string OpenFolder()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
            };

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }
            else
            {
                return null; // User canceled the dialog
            }

        }

        public void SaveFileWithDialog(byte[] rawData, string defaultName = "output.ice")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { FileName = defaultName };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    fileStream.Write(rawData, 0, rawData.Length);
                }
            }
        }

        public bool IsIceFile(string filePath)
        {
            byte[] buffer = System.IO.File.ReadAllBytes(filePath);
            try
            {
                if (buffer.Length <= 127 || buffer[0] != (byte)73 || (buffer[1] != (byte)67 || buffer[2] != (byte)69) || buffer[3] != (byte)0)
                {
                    buffer = null;
                    return false;
                }

                IceFile iceFile = IceFile.LoadIceFile((Stream)new MemoryStream(buffer));

                if (iceFile != null)
                {
                    return true;
                }

                iceFile = null;
            } 
            catch
            {
                string error = filePath + " not an ICE file";
                Logger.Log(filePath + " not an ICE file");
            }

            buffer = null;
            return false;
        }

        public string GetInputFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (Stream inStream = File.OpenRead(filePath))
                {
                    inStream.Close();
                    if (IsIceFile(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        MessageBox.Show("Not an ICE file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }
            else if (filePath == null) ; // Do nothing
            else
            {
                MessageBox.Show("Input file does not exist or not selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return null;
        }

        public Stream LoadIceFileAsStream(object input)
        {
            if (input is string)
            {
                Stream inStream = File.OpenRead((string)input);
                return inStream;
            }
            else
            {
                throw new ArgumentException("Input must be a string");
            }
        }

        public int CountIceContents(string inputFile, string patchDirectory = "")
        {
            Stream inputStream = LoadIceFileAsStream(inputFile);
            int iceFileVersion = GetIceVersion(inputStream);

            IceFile iceFile = IceFile.LoadIceFile(inputStream);
            inputStream.Close();

            if (iceFile.groupOneFiles.Length == 0 && iceFile.groupTwoFiles.Length == 0) { return 0; }

            return iceFile.groupOneFiles.Length + iceFile.groupTwoFiles.Length;
        }

        public int GetIceVersion(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "Stream cannot be null");
            }

            stream.Seek(8L, SeekOrigin.Begin);
            int num = stream.ReadByte();
            return num;
        }

        // Get list of files from the directory patchDir
        public int GetFilesCount(string patchDir)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "Patches", patchDir);

            return Directory.GetFiles(patchPath, "*.*", SearchOption.AllDirectories).ToList().Count;
        }

        bool DoesThisFileExist(string fullPath, string originalExtension, string targetExtension)
        {
            string targetFilePath = fullPath + "." + targetExtension;
            Logger.Log("Does it? Exists: " + targetFilePath);
            string originalExtensionFilePath = Path.ChangeExtension(fullPath, targetExtension);
            Logger.Log("Does it? Exists: " + originalExtensionFilePath);

            if (File.Exists(targetFilePath) || File.Exists(originalExtensionFilePath))
            {
                Logger.Log("Exists: " + fullPath);
                return true;
            }

            return false;
        }

        string DoesThisFileExistAsString(string fullPath, string originalExtension, string targetExtension)
        {
            string targetFilePath = fullPath + "." + targetExtension;
            string originalExtensionFilePath = Path.ChangeExtension(fullPath, targetExtension);

            if (File.Exists(targetFilePath))
            {
                return targetFilePath;
            }
            else
            {
                return originalExtensionFilePath;
            }
        }

        public byte[] PatchIceFile(string inputFile, string patchDirectory, ProgressBar progressBar)
        {
            Logger.Log("Input file: " + inputFile);

            byte[] buffer = System.IO.File.ReadAllBytes(inputFile);
            IceFile iceFile = IceFile.LoadIceFile(new MemoryStream(buffer));
            List<byte[]> patchedGroupOneFiles = PatchGroupFiles(iceFile.groupOneFiles, patchDirectory, progressBar, true);
            List<byte[]> patchedGroupTwoFiles = PatchGroupFiles(iceFile.groupTwoFiles, patchDirectory, progressBar, false);

            IceArchiveHeader header = new();
            byte[] rawData = new IceV4File(header.GetBytes(), patchedGroupOneFiles.ToArray(), patchedGroupTwoFiles.ToArray()).getRawData(false, false);

            return rawData;
        }


        private List<byte[]> PatchGroupFiles(byte[][] groupFiles, string patchDirectory, ProgressBar progressBar, bool isGroupOne)
        {
            List<byte[]> patchedFiles = new List<byte[]>();
            bool isPatched = false;

            if (isGroupOne) Logger.Log("group1 files:");
            else if (!isGroupOne) Logger.Log("group2 files:");

            foreach (var groupFile in groupFiles)
            {
                string fullPath = Path.Combine(patchDirectory, IceFile.getFileName(groupFile));
                if (File.Exists(fullPath))
                {
                    byte[] patchedFile = PatchFile(groupFile, fullPath);
                    patchedFiles.Add(patchedFile);
                    isPatched = true;
                }
                else if (DoesThisFileExist(fullPath, "text", "yaml"))
                {
                    byte[] patchedFile = PatchTextFile(groupFile, fullPath, "yaml");
                    patchedFiles.Add(patchedFile);
                    isPatched = true;
                }
                else if (DoesThisFileExist(fullPath, "text", "csv"))
                {
                    byte[] patchedFile = PatchTextFile(groupFile, fullPath, "csv");
                    patchedFiles.Add(patchedFile);
                    isPatched = true;
                }
                else
                {
                    patchedFiles.Add(groupFile);
                    Logger.Log(" - " + IceFile.getFileName(groupFile));
                }

                progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
            }

            if (!isPatched)
            {
                Logger.Log("Nothing to patch. Skipping...");
            }

            return patchedFiles;
        }

        private byte[] PatchFile(byte[] groupFile, string fullPath)
        {
            List<byte> bytes = new(File.ReadAllBytes(fullPath));
            bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
            Logger.Log(" - " + IceFile.getFileName(groupFile) + " [PATCHED]");

            return bytes.ToArray();
        }

        private byte[] PatchTextFile(byte[] groupFile, string fullPath, string format)
        {
            TextPatcher textPatcher = new();

            if (format == "yaml")
            {
                string yamlPath = DoesThisFileExistAsString(fullPath, "text", "yaml");
                Dictionary<string, Dictionary<string, string>> dataYaml = textPatcher.ReadYAML(yamlPath);
                byte[] textFile = textPatcher.PatchPSO2Text((byte[])groupFile, dataYaml);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());

                return bytes.ToArray();
            }
            else if (format == "csv")
            {
                string csvPath = DoesThisFileExistAsString(fullPath, "text", "csv");
                Dictionary<string, List<string>> csvData = textPatcher.ReadCSV(csvPath);
                byte[] textFile = textPatcher.PatchPSO2TextUsingCSV((byte[])groupFile, csvData);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());

                return bytes.ToArray();
            }
            else { Logger.Log("Format not supported: " + format); }

            return groupFile;
        }

        public string[] GetPatches()
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "Patches");

            try
            {
                string[] subFolders = Directory.GetDirectories(patchPath);
                string[] folderNames = new string[subFolders.Length];
                for (int i = 0; i < subFolders.Length; i++)
                {
                    folderNames[i] = Path.GetFileName(subFolders[i]);
                }

                return folderNames;
            }
            catch (Exception err)
            {
                Logger.Log("Error reading folder: " + err.Message);
            }

            return null;
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

        public void ApplyPatch(string pso2binPath, string patchName, ProgressBar progressBar, bool backup = false)
        {
            string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches", patchName);
            Logger.Log("Applying Patch: " + patchName);

            foreach (var w32path in Directory.GetDirectories(patchesPath))
            {
                string w32folder = Path.GetFileName(w32path);

                bool isReboot = false;

                if (w32folder.Contains("reboot")) isReboot = true;

                foreach (var subpath in Directory.GetDirectories(w32path, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(w32path, subpath);
                    string patchIceFolderPath = Path.Combine(patchesPath, w32path, relativePath);

                    // if subpath contains a folder we skip
                    if (Directory.GetDirectories(subpath).Any()) continue;

                    // if subpath is empty we skip
                    if (!Directory.GetFiles(subpath).Any()) continue;

                    if (File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ||
                        File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ||
                        File.Exists(Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))) ||
                        File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))))
                    {
                        string PSO2IcePath =    File.Exists(Path.Combine(pso2binPath, "data", w32folder, relativePath)) ?
                                                Path.Combine(pso2binPath, "data", w32folder, relativePath) :

                                                File.Exists(Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath)) ?
                                                Path.Combine(pso2binPath, "data", "dlc", w32folder, relativePath) :

                                                File.Exists(Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot))) ?
                                                Path.Combine(pso2binPath, "data", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot)) :
                                                Path.Combine(pso2binPath, "data", "dlc", w32folder, CalculateMD5Hash(relativePath + ".ice", isReboot));

                        byte[] rawData = PatchIceFile(PSO2IcePath, patchIceFolderPath, progressBar);

                        if (rawData != null)
                        {
                            File.WriteAllBytes(PSO2IcePath, rawData);
                            Logger.Log("Applied changes on: " + PSO2IcePath + " from " + patchName);
                        }
                    }
                }
            }
        }
    }
}
