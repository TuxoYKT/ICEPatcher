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
            Logger.Clear();
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

        public void Patch(string inputFile, string patchDir)
        {
            Logger.Clear();
            Logger.Log("Input file: " + inputFile);

            byte[] buffer = System.IO.File.ReadAllBytes(inputFile);
            Stream inStream = new MemoryStream(buffer);
            int version = GetIceVersion(inStream);
            IceFile iceFile = IceFile.LoadIceFile(inStream);
            bool compress = false;
            bool forceUnencrypted = false;

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
                    }
                    else
                    {
                        groupTwoOut.Add((byte[])file);
                        Logger.Log(" - " + IceFile.getFileName(file, i));
                    }
                    i++;
                }
            }


            IceArchiveHeader header = new();

            byte[] rawData;
            rawData = new IceV4File(header.GetBytes(), groupOneOut.ToArray(), groupTwoOut.ToArray()).getRawData(compress, forceUnencrypted);

            FileStream fileStream = new FileStream(inputFile + ".patched", FileMode.Create);
            fileStream.Write(rawData, 0, rawData.Length);
            fileStream.Close();
            // Now ask where to save the file rawData

        } 
    }
}
