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
                Console.WriteLine(filePath + " not an ICE file");
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

        public void ListIceContents(string inputFile, string patchDir = "")
        {
            Logger.Log("Input file: " + inputFile);
            Stream inStream = LoadIceFileAsStream(inputFile);

            int version = GetIceVersion(inStream);
            // Logger.Log("ICE version: " + version.ToString());

            if (patchDir != "") Logger.Log("Patch directory: " + patchDir);

            IceFile iceFile = IceFile.LoadIceFile(inStream);
            inStream.Close();

            if (iceFile.groupOneFiles.Length > 0)
            {
                Logger.Log("group1 files:");
                int i = 0;
                foreach (var file in iceFile.groupOneFiles)
                {
                    if (File.Exists(Path.Combine(patchDir, IceFile.getFileName(file, i))))
                    {
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [TO BE PATCHED]");
                    }
                    else
                    {
                        Logger.Log(" - " + IceFile.getFileName(file, i));
                    }
                    i++;
                }
            }

            if (iceFile.groupTwoFiles.Length > 0)
            {
                Logger.Log("group2 files:");
                int i = 0;
                foreach (var file in iceFile.groupTwoFiles)
                {
                    if (File.Exists(Path.Combine(patchDir, IceFile.getFileName(file, i))))
                    {
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [TO BE PATCHED]");
                    }
                    else
                    {
                        Logger.Log(" - " + IceFile.getFileName(file, i));
                    }
                    i++;
                }
            }
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
        public List<string> GetFiles(string patchDir)
        {
            return Directory.GetFiles(patchDir, "*.*", SearchOption.AllDirectories).ToList();
        }

        public byte[] Patch(string inputFile, string patchDir)
        {
            Logger.Log("Input file: " + inputFile);

            byte[] buffer = System.IO.File.ReadAllBytes(inputFile);
            Stream inStream = new MemoryStream(buffer);
            IceFile iceFile = IceFile.LoadIceFile(inStream);
            bool compress = false;
            bool forceUnencrypted = false;
            bool patched = false;

            // Logger.Log("ICE version: " + version.ToString());

            Logger.Log("Patch directory: " + patchDir);

            List<byte[]> groupOneOut = new List<byte[]>();
            List<byte[]> groupTwoOut = new List<byte[]>();

            if (iceFile.groupOneFiles.Length > 0)
            {
                Logger.Log("group1 files:");
                int i = 0;
                foreach (var file in iceFile.groupOneFiles)
                {
                    string fullPath = Path.Combine(patchDir, IceFile.getFileName(file, i));
                    if (File.Exists(fullPath))
                    {
                        List<byte> bytes = new(File.ReadAllBytes(fullPath));
                        bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
                        groupOneOut.Add(bytes.ToArray());
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [PATCHED]");

                        patched = true;
                    }
                    else if (File.Exists(fullPath + ".yaml"))
                    {
                        TextPatcher textPatcher = new();
                        Dictionary<string, Dictionary<string, string>> yamlData = textPatcher.ReadYAML(fullPath + ".yaml");
                        Logger.Log("    " + " - Patching " + IceFile.getFileName(file, i) + " with YAML");
                        byte[] textfile = textPatcher.PatchPSO2Text((byte[])file, yamlData);

                        List<byte> bytes = new(textfile);
                        bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
                        groupOneOut.Add(bytes.ToArray());
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [PATCHED (TEXT)]");

                        patched = true;
                    }
                    else
                    {
                        groupOneOut.Add((byte[])file);
                        Logger.Log(" - " + IceFile.getFileName(file, i));
                    }
                    i++;
                }
            }

            if (iceFile.groupTwoFiles.Length > 0)
            {
                Logger.Log("group2 files:");
                int i = 0;
                foreach (var file in iceFile.groupTwoFiles)
                {
                    string fullPath = Path.Combine(patchDir, IceFile.getFileName(file, i));
                    if (File.Exists(fullPath))
                    {
                        List<byte> bytes = new(File.ReadAllBytes(fullPath));
                        bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
                        groupTwoOut.Add(bytes.ToArray());
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [PATCHED]");

                        patched = true;
                    }
                    else if (File.Exists(fullPath + ".yaml"))
                    {
                        TextPatcher textPatcher = new();
                        Dictionary<string, Dictionary<string, string>> yamlData = textPatcher.ReadYAML(fullPath + ".yaml");
                        Logger.Log("    " + " - Patching " + IceFile.getFileName(file, i) + " with YAML");
                        byte[] textfile = textPatcher.PatchPSO2Text((byte[])file, yamlData);

                        List<byte> bytes = new(textfile);
                        bytes.InsertRange(0, new IceFileHeader(fullPath, (uint)bytes.Count).GetBytes());
                        groupTwoOut.Add(bytes.ToArray());
                        Logger.Log(" - " + IceFile.getFileName(file, i) + " [PATCHED (TEXT)]");

                        patched = true;
                    }
                    else
                    {
                        groupTwoOut.Add((byte[])file);
                        Logger.Log(" - " + IceFile.getFileName(file, i));
                    }
                    i++;
                }
            }

            if (!patched)
            {
                Logger.Log("Nothing to patch. Skipping...");
                return null;
            }

            IceArchiveHeader header = new();

            byte[] rawData;
            rawData = new IceV4File(header.GetBytes(), groupOneOut.ToArray(), groupTwoOut.ToArray()).getRawData(compress, forceUnencrypted);

            return rawData;

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
                Console.WriteLine("Error reading folder: " + err.Message);
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

        public void ApplyPatch(string pso2binPath, string patchName, bool backup = false)
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

                        byte[] rawData = Patch(PSO2IcePath, patchIceFolderPath);

                        if (rawData != null)
                        {
                            File.WriteAllBytes(PSO2IcePath, rawData);
                        }

                        Logger.Log("Patch applied: " + PSO2IcePath);
                        Logger.Log("");
                    }
                }
            }
        }
    }
}
