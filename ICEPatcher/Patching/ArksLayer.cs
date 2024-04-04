using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ICEPatcher
{
    internal class ArksLayer
    {

        public bool CheckForFilelist(string patchPath)
        {
            string[] files = Directory.GetFiles(patchPath, "*.txt");

            foreach (string file in files)
            {
                if (Regex.IsMatch(Path.GetFileName(file), @"filelist"))
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetFilelistFiles(string patchPath)
        {
            List<string> filelistFiles = new();
            string[] files = Directory.GetFiles(patchPath, "*.txt");

            foreach (string file in files)
            {
                if (Regex.IsMatch(Path.GetFileName(file), @"filelist"))
                {
                    filelistFiles.Add(file);
                }
            }
            return filelistFiles;
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

        public void ApplyArksLayerPatchFromFolder(string patchPath, string exportPath)
        {
            if (Patching.GetPSO2BinPath() == null)
            {
                throw new Exception("PSO2BinPath is not set");
            }

            if (!CheckForFilelist(patchPath))
            {
                Debug.WriteLine("No filelist found in " + patchPath);
                return;
            }

            List<string> filelistFiles = GetFilelistFiles(patchPath);


            foreach (var file in filelistFiles)
            {
                string filelistName = Path.GetFileName(file);

            }
        }
    }
}
