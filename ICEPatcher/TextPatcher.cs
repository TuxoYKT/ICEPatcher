using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using AquaModelLibrary.Data.PSO2.Aqua;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;

namespace ICEPatcher
{
    public class TextPatcher
    {
        public Dictionary<string, Dictionary<string, string>> ReadYAML(string yamlPath)
        {
            Logger.Log("Reading YAML: " + yamlPath);

            string yamlContent = File.ReadAllText(yamlPath);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            Dictionary<string, Dictionary<string, string>> dataYaml =
            deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(yamlContent);

            return dataYaml;
        }



        public byte[] PatchPSO2Text(byte[] PSO2TextInput, Dictionary<string, Dictionary<string, string>> dataYaml, string language = "en")
        {
            var originalText = new PSO2Text(PSO2TextInput);
            PSO2Text new_text = new PSO2Text();

            for (int i = 0; i < originalText.categoryNames.Count; i++)
            {
                new_text.categoryNames.Add(originalText.categoryNames[i].Trim());
                new_text.text.Add(new List<List<PSO2Text.TextPair>>());
                Logger.Log("    " + originalText.categoryNames[i]);
                for (int j = 0; j < originalText.text[i].Count; j++)
                {
                    new_text.text[i].Add(new List<PSO2Text.TextPair>());
                    if (originalText.text[i][j].Count == 0)
                    {
                        continue;
                    }
                    for (int k = 0; k < originalText.text[i][j].Count; k++)
                    {
                        PSO2Text.TextPair pair = new PSO2Text.TextPair();
                        pair.name = originalText.text[i][j][k].name;
                        pair.str = originalText.text[i][j][k].str;

                        // check if category exists in yaml
                        if (dataYaml.ContainsKey(originalText.categoryNames[i]))
                        {
                            // if we're replacing the english text and the name matches use the replacement from the yaml
                            foreach (var replacement in dataYaml[originalText.categoryNames[i]])
                            {
                                if ((language == "en" && j == 1) || (language != "en" && j == 0))
                                {
                                    if (replacement.Key == originalText.text[i][j][k].name)
                                    {
                                        pair.str = replacement.Value;
                                        Logger.Log("    " + " - " + pair.name + ": " + pair.str + " in " + originalText.categoryNames[i]);
                                    }
                                }
                            }
                        }

                        new_text.text[i][j].Add(pair);
                    }
                }
            }

            return new_text.GetBytesNIFL();
        }

        private bool ContainsJapaneseText(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"[\u3040-\u30FF\u4E00-\u9FFF]");
        }

        private bool IsUntranslated(string str)
        {
            if (str[0] == '*')
            {
                return true;
            }
            return false;
        }


        public Dictionary<string, List<string>> ReadCSV(string csvPath)
        {
            Logger.Log("Reading CSV: " + csvPath);

            Dictionary<string, List<string>> csvData = new Dictionary<string, List<string>>();

            using (TextFieldParser parser = new TextFieldParser(csvPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length >= 2)
                    {
                        string name = fields[0].Split('#')[0]; //Strip after #
                        string str = Regex.Unescape(fields[1].ToString().Trim('\"'));

                        if (!csvData.ContainsKey(name))
                        {
                            csvData[name] = new List<string>();
                        }

                        if (!ContainsJapaneseText(str) && !IsUntranslated(str))
                            csvData[name].Add(str);
                        else
                            csvData[name].Add(null);
                        Logger.Log("    " + name + ": " + str);
                    }
                }
            }

            return csvData;
        }


        public byte[] PatchPSO2TextUsingCSV(byte[] PSO2TextInput, Dictionary<string, List<string>> csvData, string language = "en")
        {
            var originalText = new PSO2Text(PSO2TextInput);
            PSO2Text new_text = new PSO2Text();

            Dictionary<string, int> nameCounts = new Dictionary<string, int>();

            for (int i = 0; i < originalText.categoryNames.Count; i++)
            {
                new_text.categoryNames.Add(originalText.categoryNames[i].Trim());
                new_text.text.Add(new List<List<PSO2Text.TextPair>>());
                Logger.Log("    " + originalText.categoryNames[i]);
                for (int j = 0; j < originalText.text[i].Count; j++)
                {
                    new_text.text[i].Add(new List<PSO2Text.TextPair>());
                    if (originalText.text[i][j].Count == 0)
                    {
                        continue;
                    }
                    for (int k = 0; k < originalText.text[i][j].Count; k++)
                    {
                        PSO2Text.TextPair pair = new PSO2Text.TextPair();
                        pair.name = originalText.text[i][j][k].name;
                        pair.str = originalText.text[i][j][k].str;

                        if ((language == "en" && j == 1) || (language != "en" && j == 0))
                        {

                            // we don't check for category, instead we check if name is in the csv
                            if (csvData.ContainsKey(pair.name))
                            {
                                Logger.Log("    " + originalText.text[i][j][k].name);
                                // we keep track of if that name is repeating
                                if (nameCounts.ContainsKey(pair.name))
                                {
                                    nameCounts[pair.name]++;
                                    try
                                    {
                                        if (csvData[pair.name][(int)nameCounts[pair.name]] != null)
                                            pair.str = csvData[pair.name][(int)nameCounts[pair.name]];
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Log(e.Message);
                                    }
                                }
                                else
                                {
                                    nameCounts.Add(pair.name, (int)0);
                                    if (csvData[pair.name][0] != null)
                                        pair.str = csvData[pair.name][0];
                                }
                                Logger.Log("    " + " - " + pair.name + ": " + pair.str + " in " + originalText.categoryNames[i]);
                            }

                            new_text.text[i][j].Add(pair);
                        }
                    }
                }
            }

            return new_text.GetBytesNIFL();
        }
    }
}
