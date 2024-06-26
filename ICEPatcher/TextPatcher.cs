﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using AquaModelLibrary.Data.PSO2.Aqua;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using YamlDotNet.Serialization;
using UnluacNET;

namespace ICEPatcher
{
    public static class TextPatcher
    {
        public static byte[] PatchPSO2Text(byte[] PSO2TextInput, Dictionary<string, Dictionary<string, string>> dataYaml, string language = "en")
        {
            var originalText = new PSO2Text(PSO2TextInput);
            PSO2Text new_text = new PSO2Text();

            for (int i = 0; i < originalText.categoryNames.Count; i++)
            {
                new_text.categoryNames.Add(originalText.categoryNames[i].Trim());
                new_text.text.Add(new List<List<PSO2Text.TextPair>>());
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

        private static bool ContainsJapaneseText(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"[\u3040-\u30FF\u4E00-\u9FFF]");
        }

        private static bool IsUntranslated(string str)
        {
            if (str[0] == '*')
            {
                return true;
            }
            return false;
        }

        public static Dictionary<string, List<string>> ReadCSVFromMemory(byte[] csvData)
        {
            Dictionary<string, List<string>> csvDictionary = new Dictionary<string, List<string>>();

            using (MemoryStream stream = new MemoryStream(csvData))
            using (TextFieldParser parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length >= 2)
                    {
                        string name = fields[0].Split('#')[0]; // Strip after #
                        string value = Regex.Unescape(fields[1].Trim('\"'));

                        if (!csvDictionary.ContainsKey(name))
                        {
                            csvDictionary[name] = new List<string>();
                        }

                        if (!ContainsJapaneseText(value) && !IsUntranslated(value))
                        {
                            csvDictionary[name].Add(value);
                        }
                        else
                        {
                            csvDictionary[name].Add(null);
                        }
                    }
                }
            }

            return csvDictionary;
        }

        public static byte[] PatchPSO2TextUsingCSV(byte[] PSO2TextInput, Dictionary<string, List<string>> csvData, string language = "en")
        {
            var originalText = new PSO2Text(PSO2TextInput);
            PSO2Text new_text = new PSO2Text();

            Dictionary<string, int> nameCounts = new Dictionary<string, int>();

            for (int i = 0; i < originalText.categoryNames.Count; i++)
            {
                new_text.categoryNames.Add(originalText.categoryNames[i].Trim());
                new_text.text.Add(new List<List<PSO2Text.TextPair>>());
                //Logger.Log("    " + originalText.categoryNames[i]);
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
                                //Logger.Log("    " + originalText.text[i][j][k].name);
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
                                        Debug.WriteLine(e.Message);
                                    }
                                }
                                else
                                {
                                    nameCounts.Add(pair.name, (int)0);
                                    if (csvData[pair.name][0] != null)
                                        pair.str = csvData[pair.name][0];
                                }
                                //Logger.Log("    " + " - " + pair.name + ": " + pair.str + " in " + originalText.categoryNames[i]);
                            }

                            new_text.text[i][j].Add(pair);
                        }
                    }
                }
            }

            return new_text.GetBytesNIFL();
        }

        public static byte[] ExtractPSO2Text(byte[] PSO2TextInput, string whichLanguage = "en")
        {
            var text = new PSO2Text(PSO2TextInput);
            var serializer = new SerializerBuilder().Build();


            // if text is empty, exit
            if (text.categoryNames.Count == 0)
            {
                Console.WriteLine("The provided file is empty.");
                return null;
            }

            var textPatchData = new Dictionary<string, Dictionary<string, string>>();

            for (int i = 0; i < text.categoryNames.Count; i++)
            {
                var category = text.categoryNames[i];

                var j = whichLanguage == "jp" ? 0 : 1;
                if (text.text[i][j].Count == 0)
                {
                    continue;
                }

                textPatchData.Add(category, new Dictionary<string, string>());
                for (int k = 0; k < text.text[i][j].Count; k++)
                {
                    var key = text.text[i][j][k].name;
                    var value = text.text[i][j][k].str;

                    textPatchData[category].Add(key, value);
                }

            }

            if (textPatchData.Count == 0)
            {
                // "The output is empty. This means there's no text for the language");
                return null;
            }

            var yaml = serializer.Serialize(textPatchData).ToArray();
            byte[] output = Encoding.UTF8.GetBytes(yaml);

            return output;
        }
    }
}
