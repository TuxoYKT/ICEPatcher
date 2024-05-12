using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using IniParser.Model;
using IniParser;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using ICEPatcher;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace GlobalPatcher
{
    public class GithubRepo
    {
        public Commit commit { get; set; }
    }

    public class Commit
    {
        public string sha { get; set; }
    }

    public static class Download
    {
        private static string GetLatestCommitSHA(string organization = "Arks-Layer", string repo = "PSO2ENPatchCSV", string branch = "RU")
        {
            // Get latest commit
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "GlobalPatcher/1.0.0");
                var response = client.GetAsync("https://api.github.com/repos/" + organization + "/" + repo + "/branches/" + branch).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var branches = JsonSerializer.Deserialize<GithubRepo>(json);
                    return branches.commit.sha.Substring(0, 7);
                }
            }

            return null;
        }

        private static async Task DownloadPatchesFromGithub(string organization = "Arks-Layer", string repo = "PSO2ENPatchCSV", string branch = "RU", string sha = null)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePatchesFolderPath = Path.Combine(executablePath, "RemotePatches");
            ProgressBar progressBar = Patching.progressBar;

            if (!Directory.Exists(remotePatchesFolderPath))
            {
                Directory.CreateDirectory(remotePatchesFolderPath);
            }


            // Download patches from GitHub
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", "GlobalPatcher/1.0.0");

                if (sha == null)
                    throw new Exception("Failed to get latest commit");

                string zipFilePath = Path.Combine(remotePatchesFolderPath, organization + "_" + repo + "_" + branch + "_" + sha + ".zip");

                if (!File.Exists(zipFilePath))
                {
                    try
                    {
                        client.DownloadProgressChanged += (sender, args) =>
                        {
                            if (progressBar != null && progressBar.InvokeRequired)
                            {
                                progressBar.Invoke((MethodInvoker)delegate
                                {
                                    progressBar.Value = args.ProgressPercentage;
                                });
                            }
                        };

                        await client.DownloadFileTaskAsync(
                        "https://api.github.com/repos/" + organization + "/" + repo + "/zipball/" + branch,
                        zipFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            return;
        }

        public static int GetFilesCount(string patchPath)
        {
            if (patchPath.EndsWith(".zip"))
            {
                using (ZipArchive archive = ZipFile.OpenRead(patchPath))
                {
                    return archive.Entries.Count;
                }
            }
            else
            {
                return Directory.GetFiles(patchPath, "*.*", SearchOption.AllDirectories).ToList().Count;
            }
        }

        public static void DownloadPatches()
        {
            string remotePatchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RemotePatches");
            bool exportPatches = Configuration.keyConfig["Export"] == "true";
            string exportPath = null;

            if (exportPatches)
            {
                exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
            }

            if (Configuration.keyRepos.Count > 0)
            {
                Patching.SetPatchesPath(remotePatchesPath);

                foreach (KeyData key in Configuration.keyRepos)
                {
                    string organization = key.KeyName;
                    string repo = key.Value.Trim().Split(',')[0];

                    List<string> branches = key.Value.Trim().Split(',').Skip(1).ToList();

                    Task task = Task.Run(() =>
                    {
                        foreach (string branch in branches)
                        {
                            string sha = GetLatestCommitSHA(organization, repo, branch);
                            DownloadPatchesFromGithub(organization, repo, branch, sha).Wait();
                            string zipFile = organization + "_" + repo + "_" + branch + "_" + sha + ".zip";
                            int maximum = GetFilesCount(Path.Combine(remotePatchesPath, zipFile));

                            // Use Invoke to update the progressBar on the UI thread
                            Patching.progressBar.Invoke((MethodInvoker)delegate
                            {
                                Patching.progressBar.Value = 0;
                                Patching.progressBar.Maximum = maximum;
                            });
                            Patching.ApplyPatch(zipFile, exportPath);
                        }
                    });

                    task.Wait();
                }
            }

            return;
        }
    }
}
