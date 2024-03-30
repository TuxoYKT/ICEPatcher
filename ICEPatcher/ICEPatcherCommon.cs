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
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ProgressBar = System.Windows.Forms.ProgressBar;
using System.IO.Compression;


namespace ICEPatcher
{
    public class ICEPatcherCommon
    {
        private MainForm mainForm;

        public ICEPatcherCommon(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public static string OpenFile()
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
                Debug.WriteLine(filePath + " not an ICE file");
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
            string originalExtensionFilePath = Path.ChangeExtension(fullPath, targetExtension);

            if (File.Exists(targetFilePath) || File.Exists(originalExtensionFilePath))
            {
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

        public byte[] PatchIceFile(string inputFile, string patchDirectory, List<string> filesToPatchList = null)
        {
            // Debug.WriteLine("Input file: " + inputFile);

            byte[] buffer = System.IO.File.ReadAllBytes(inputFile);
            IceFile iceFile = IceFile.LoadIceFile(new MemoryStream(buffer));
            List<byte[]> patchedGroupOneFiles = PatchGroupFiles(iceFile.groupOneFiles, patchDirectory, true, filesToPatchList);
            List<byte[]> patchedGroupTwoFiles = PatchGroupFiles(iceFile.groupTwoFiles, patchDirectory, false, filesToPatchList);

            IceArchiveHeader header = new();
            byte[] rawData = new IceV4File(header.GetBytes(), patchedGroupOneFiles.ToArray(), patchedGroupTwoFiles.ToArray()).getRawData(false, false);

            return rawData;
        }

        private List<byte[]> PatchGroupFiles(byte[][] groupFiles, string patchDirectory, bool isGroupOne, List<string> filesToPatchList = null)
        {
            ProgressBar progressBar = mainForm.MainFormProgressBar;

            List<byte[]> patchedFiles = new List<byte[]>();
            bool isPatched = false;

            // if (isGroupOne) Debug.WriteLine("group1 files:");
            // else Debug.WriteLine("group2 files:");

            foreach (var groupFile in groupFiles)
            {
                int i = 0;
                string fileNameFromIce = IceFile.getFileName(groupFile);
                string fileNameFromIceWithoutExtenstion = Path.GetFileNameWithoutExtension(fileNameFromIce);

                if (filesToPatchList.Count != 0)
                {
                    while (fileNameFromIceWithoutExtenstion != Path.GetFileNameWithoutExtension(filesToPatchList[i]) && i < filesToPatchList.Count - 1) i++;

                    if (fileNameFromIce == Path.GetFileName(filesToPatchList[i]))
                    {
                        byte[] patchedFile = PatchFile(groupFile, filesToPatchList[i]);
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                        filesToPatchList.Remove(filesToPatchList[i]);
                        i++;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else if (DoesThisFileExist(Path.Combine(Path.GetDirectoryName(filesToPatchList[i]), IceFile.getFileName(groupFile)), "text", "yaml"))
                    {
                        byte[] patchedFile = PatchTextFile(groupFile, filesToPatchList[i], "yaml");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                        filesToPatchList.Remove(filesToPatchList[i]);
                        i++;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else if (DoesThisFileExist(Path.Combine(Path.GetDirectoryName(filesToPatchList[i]), IceFile.getFileName(groupFile)), "text", "csv"))
                    {
                        byte[] patchedFile = PatchTextFile(groupFile, filesToPatchList[i], "csv");
                        patchedFiles.Add(patchedFile);
                        isPatched = true;
                        filesToPatchList.Remove(filesToPatchList[i]);
                        i++;
                        progressBar.Invoke((MethodInvoker)delegate { progressBar.PerformStep(); });
                    }
                    else
                    {
                        patchedFiles.Add(groupFile);
                        // Debug.WriteLine(" - " + IceFile.getFileName(groupFile));
                    }
                }
                else
                {
                    patchedFiles.Add(groupFile);
                }
            }

            if (!isPatched)
            {
                // Debug.WriteLine("Nothing to patch in this group. Skipping...");
            }

            return patchedFiles;
        }


        private byte[] PatchFile(byte[] groupFile, string fullPath)
        {
            List<byte> bytes = new(File.ReadAllBytes(fullPath));
            bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
            // Debug.WriteLine(" - " + IceFile.getFileName(groupFile) + " [PATCHED]");

            return bytes.ToArray();
        }

        private byte[] PatchTextFile(byte[] groupFile, string fullPath, string format)
        {
            TextPatcher textPatcher = new();
            string groupFileName = IceFile.getFileName(groupFile);

            if (format == "yaml")
            {
                string yamlPath = DoesThisFileExistAsString(fullPath, "text", "yaml");
                Dictionary<string, Dictionary<string, string>> dataYaml = textPatcher.ReadYAML(yamlPath);
                byte[] textFile = textPatcher.PatchPSO2Text((byte[])groupFile, dataYaml);


                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(groupFileName, (uint)bytes.Count).GetBytes());

                // Debug.WriteLine(" - " + IceFile.getFileName(groupFile) + " [PATCHED WITH YAML]");

                return bytes.ToArray();
            }
            else if (format == "csv")
            {
                string csvPath = DoesThisFileExistAsString(fullPath, "text", "csv");
                Dictionary<string, List<string>> csvData = textPatcher.ReadCSV(csvPath);
                byte[] textFile = textPatcher.PatchPSO2TextUsingCSV((byte[])groupFile, csvData);

                List<byte> bytes = new(textFile);
                bytes.InsertRange(0, new IceFileHeader(groupFileName, (uint)bytes.Count).GetBytes());

                // Debug.WriteLine(" - " + IceFile.getFileName(groupFile) + " [PATCHED WITH CSV]");

                return bytes.ToArray();
            }
            else { Debug.WriteLine("Format not supported: " + format); }

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
                Debug.WriteLine("Error reading folder: " + err.Message);
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

        private Dictionary<string, string> ReadFileList(string patchesPath)
        {
            if (File.Exists(Path.Combine(patchesPath, "filelist.txt")))
            {
                Dictionary<string, string> fileList = new Dictionary<string, string>();

                foreach (string line in File.ReadAllLines(Path.Combine(patchesPath, "filelist.txt")))
                {
                    string[] parts = line.Split(',');
                    fileList[parts[1].Trim().Replace(".text", ".csv")] = parts[0].Trim();
                }

                return fileList;
            }

            return new Dictionary<string, string>();
        }

        private Dictionary<string, List<string>> GetFileList(string patchesPath)
        {
            Dictionary<string, List<string>> output = new Dictionary<string, List<string>>();
            Dictionary<string, string> fileList = ReadFileList(patchesPath);
            List<string> excludedFolders = new List<string> { "Files", "Dummy", "Empty" };

            foreach (string root in Directory.GetDirectories(patchesPath, "*", SearchOption.AllDirectories))
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

        private void ProcessArksLayerPatch(string pso2binPath, string patchesPath, bool isJapaneseClient = false)
        {
            Debug.WriteLine("Found filelist: " + Path.Combine(patchesPath, "filelist.txt"));
            Dictionary<string, List<string>> fileList = GetFileList(patchesPath);

            Parallel.ForEach(fileList, iceFolder =>
            {
                string relativePath = iceFolder.Key;
                //Debug.WriteLine(relativePath);

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

                    List<string> filesToPatchList = new List<string>();
                    foreach (var filePath in iceFolder.Value)
                    {
                        filesToPatchList.Add(filePath);
                        //Debug.WriteLine("    " + filePath);
                    }

                    string patchIceFolderPath = Path.Combine(patchesPath, w32folder, relativePath);

                    byte[] rawData = PatchIceFile(PSO2IcePath, patchIceFolderPath,filesToPatchList);

                    if (rawData != null)
                    {
                        File.WriteAllBytes(PSO2IcePath, rawData);
                        Debug.WriteLine("Applied changes on: " + PSO2IcePath);
                    }
                }
            });
        }

        private void ApplyPatchFromFolder(string patchName, string pso2binPath)
        {
            string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches", patchName);

            Parallel.ForEach(Directory.GetDirectories(patchesPath), w32path =>
            {
                string w32folder = Path.GetFileName(w32path);

                if (!w32folder.Contains("win32")) return;

                bool isReboot = w32folder.Contains("reboot");

                Parallel.ForEach(Directory.GetDirectories(w32path, "*", SearchOption.AllDirectories), subpath =>
                {
                    string relativePath = Path.GetRelativePath(w32path, subpath);
                    string patchIceFolderPath = Path.Combine(patchesPath, w32path, relativePath);

                    if (Directory.GetDirectories(subpath).Any()) return; // if subpath contains a folder we skip

                    if (!Directory.GetFiles(subpath).Any()) return; // if subpath is empty we skip

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

                        List<string> filesToPatchList = Directory.GetFiles(patchIceFolderPath, "*.*", SearchOption.AllDirectories).ToList();

                        byte[] rawData = PatchIceFile(PSO2IcePath, patchIceFolderPath, filesToPatchList);

                        if (rawData != null)
                        {
                            File.WriteAllBytes(PSO2IcePath, rawData);
                            Debug.WriteLine("Applied changes on: " + PSO2IcePath + " from " + patchName);
                        }
                    }
                });
            });
        }

        public void ApplyPatch(string pso2binPath, string patchName, bool backup = false)
        {
            string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches", patchName);
            Debug.WriteLine("Applying Patch: " + patchName);

            // ProcessArksLayerPatch(patchesPath); // this is done for Arks-Layer patch

            if (File.Exists(Path.Combine(patchesPath, "filelist.txt")))
            {
                ProcessArksLayerPatch(pso2binPath, patchesPath, false);
                return;
            }

            ApplyPatchFromFolder(patchName, pso2binPath);
        }
    }
}
