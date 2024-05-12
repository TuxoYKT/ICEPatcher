using IniParser.Model;
using IniParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalPatcher
{
    internal static class Configuration
    {
        public static FileIniDataParser parser = new FileIniDataParser();
        public static IniData data = parser.ReadFile("GlobalPatcher.ini");
        public static KeyDataCollection keyConfig = data["Configuration"];
        public static KeyDataCollection keyRepos = data["Repos"];
    }
}
