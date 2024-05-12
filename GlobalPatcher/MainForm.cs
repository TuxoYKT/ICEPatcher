using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using ICEPatcher;
using IniParser.Model;
using IniParser;
using System.Globalization;

namespace GlobalPatcher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        CultureInfo currentCulture = CultureInfo.CurrentUICulture;
        ResourceManager resourceManager = new ResourceManager("GlobalPatcher.Localization.Strings", Assembly.GetExecutingAssembly());
        bool localPatchesAllowed = Configuration.keyConfig["ApplyLocalPatches"] == "true";
        bool exportPatches = Configuration.keyConfig["Export"] == "true";
        bool allowBackup = Configuration.keyConfig["Backup"] == "true";

        private void MainForm_Load(object sender, EventArgs e)
        {
            browseButton.Text = resourceManager.GetString("button_browse", currentCulture);
            patchButton.Text = resourceManager.GetString("button_patch", currentCulture);
            statusLabel.Text = resourceManager.GetString("status_pso2binNotSet", currentCulture);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    if (!Patching.IsPSO2BinPath(folderBrowserDialog.SelectedPath))
                    {
                        MessageBox.Show(resourceManager.GetString("error_notpso2bin"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        patchButton.Enabled = false;
                        return;
                    }

                    Patching.SetPSO2BinPath(folderBrowserDialog.SelectedPath);

                    browseButton.Enabled = false;
                    statusLabel.Text = resourceManager.GetString("status_pso2binSet", currentCulture);
                    patchButton.Enabled = true;
                }
            }
        }

        public int GetFilesCount(string patchPath)
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

        private void UpdateProgressBar()
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "RemotePatches");
            progressBar.Maximum = 0;

            try
            {
                string[] zipFiles = Directory.GetFiles(patchPath, "*.zip");

                foreach (string zipFile in zipFiles)
                {
                    progressBar.Maximum += GetFilesCount(zipFile);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void ApplyPatches()
        {

            string currentTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup", currentTime);
            string patchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Patches");
            string exportPath = null;

            if (allowBackup && !exportPatches)
            {
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                Patching.SetBackupPath(backupPath);
                Patching.AllowBackup = true;
            }

            if (exportPatches)
            {
                exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
            }

            try
            {
                if (!localPatchesAllowed)
                    return;

                Patching.SetPatchesPath(patchesPath);
                string[] localZipFiles = Directory.GetFiles(patchesPath, "*.zip");

                foreach (string zipFile in localZipFiles)
                {
                    Patching.ApplyPatch(zipFile, exportPath);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private bool isDone = false;

        private void DeleteDownloadedPatches()
        {
            string remotePatchesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RemotePatches");
            string[] zipFiles = Directory.GetFiles(remotePatchesPath, "*.zip");

            foreach (string patchPath in zipFiles)
            {
                if (Directory.Exists(patchPath))
                {
                    Directory.Delete(patchPath, true);
                }
            }
        }

        private async void patchButton_Click(object sender, EventArgs e)
        {
            if (isDone)
            {
                Application.Exit();
                return;
            }

            try
            {
                patchButton.Enabled = false;
                Patching.progressBar = progressBar;

                if (!Abnormality.IsBypassed() && !exportPatches)
                    Abnormality.Bypass();


                progressBar.Value = 0;
                progressBar.Step = 1;

                statusLabel.Text = resourceManager.GetString("status_downloading", currentCulture);

                await Task.Run(() =>
                {
                    Download.DownloadPatches();
                });


                UpdateProgressBar();

                statusLabel.Text = resourceManager.GetString("status_patching", currentCulture);
                await Task.Run(() =>
                {
                    ApplyPatches();
                });

                statusLabel.Text = resourceManager.GetString("status_cleaningup", currentCulture);
                await Task.Run(() =>
                {
                    DeleteDownloadedPatches();
                });

                statusLabel.Text = resourceManager.GetString("status_complete", currentCulture);
                progressBar.Value = progressBar.Maximum;

                isDone = true;
                patchButton.Text = resourceManager.GetString("button_done", currentCulture);
                patchButton.Enabled = true;
            }
            catch (Exception ex)
            {
                statusLabel.Text = resourceManager.GetString("status_patchingError", currentCulture);
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                isDone = true;
                patchButton.Text = resourceManager.GetString("button_done", currentCulture);
            }
        }
    }
}
