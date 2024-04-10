using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zamboni;

namespace ICEPatcher.Create
{
    public static class Creation
    {
        private static void WriteGroupToDirectory(byte[][] groupFiles, string outputPath = null)
        {
            if (!Directory.Exists(outputPath) && groupFiles != null && (uint)groupFiles.Length > 0U)
                Directory.CreateDirectory(outputPath);

            for (int index = 0; index < groupFiles.Length; ++index)
            {
                string str = IceFile.getFileName(groupFiles[index], index);
                byte[] file;
                int iceHeaderSize = -1;
                if (str == "namelessFile.bin" || str.Contains("namelessNIFLFile_"))
                {
                    file = groupFiles[index];
                }
                else
                {
                    int iceDataSize = BitConverter.ToInt32(groupFiles[index], 0x8);
                    iceHeaderSize = BitConverter.ToInt32(groupFiles[index], 0xC);
                    file = new byte[iceDataSize];
                    Array.ConstrainedCopy(groupFiles[index], iceHeaderSize, file, 0, iceDataSize);
                }
                Debug.WriteLine($"{str}");
                System.IO.File.WriteAllBytes(str, file);
                file = null;
                groupFiles[index] = null;
            }
        }

        public static void ExtractICE(string path, string outputPath = null)
        {
            if (path == null)
            { 
                throw new ArgumentNullException("path");
            }

            using (Stream stream = File.OpenRead(path))
            {
                IceFile iceFile = IceFile.LoadIceFile(stream);

                if (iceFile != null)
                {
                    if (iceFile.groupOneFiles != null && (uint)iceFile.groupOneFiles.Length > 0U)
                        WriteGroupToDirectory(iceFile.groupOneFiles, outputPath);
                    if (iceFile.groupTwoFiles != null && (uint)iceFile.groupTwoFiles.Length > 0U)
                        WriteGroupToDirectory(iceFile.groupTwoFiles, outputPath);
                }
            }
        }
        public static void ProcessFileInput(string path, string patchName)
        {
            string pso2binPath = Patching.GetPSO2BinPath();
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchesPath = Path.Combine(executablePath, "Patches");
            string patchPath = Path.Combine(patchesPath, patchName);

            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    bool isOriginalName = false;
                    bool isReboot = false;

                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                    if (line.Contains("#")) line = line.Split('#')[0];
                    line = line.Trim();
                    if (line.StartsWith("@"))
                    {
                        isOriginalName = true;
                        line = line.Replace("@", "");
                    }

                    string win32folder = line.Substring(0, line.IndexOf("\\"));

                    if (win32folder.Contains("reboot")) isReboot = true;

                    string relativeSubPath = line.Substring(line.IndexOf("\\") + 1);
                    string iceFileSubPath = relativeSubPath;

                    if (isOriginalName)
                    {
                        iceFileSubPath = Patching.CalculateMD5Hash(relativeSubPath, isReboot);
                    }

                    string fullPath = Path.Combine(pso2binPath, "data", win32folder, iceFileSubPath);
                    string outputPath = Path.Combine(patchPath, win32folder, iceFileSubPath);

                    if (File.Exists(fullPath))
                    {
                        if (Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                        ExtractICE(fullPath, outputPath);
                    }
                }
            }
        }
    }
}
