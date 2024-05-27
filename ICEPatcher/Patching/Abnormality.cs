using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace ICEPatcher
{
    public static class Abnormality
    {
        public static string ComputeSHA256Checksum(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static bool IsBypassed()
        {
            if (Patching.GetPSO2BinPath() == null) return false;

            string sha = "e9dcfa94dc23f86f9b42f7cfb308beac5075aa58bec120f7fe09182e183e5109";
            string fileToCheck = Path.Combine(Patching.GetPSO2BinPath(), "data", "win32", "d4455ebc2bef618f29106da7692ebc1a");

            if (ComputeSHA256Checksum(fileToCheck) == sha)
            {
                return true;
            }

            return false;
        }

        public static void Bypass()
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string checksumFile = Path.Combine(executablePath, "AbnormalityBypass", "d4455ebc2bef618f29106da7692ebc1a");

            File.Copy(checksumFile, Path.Combine(Patching.GetPSO2BinPath(), "data", "win32", "d4455ebc2bef618f29106da7692ebc1a"), true);

        }
    }
}
