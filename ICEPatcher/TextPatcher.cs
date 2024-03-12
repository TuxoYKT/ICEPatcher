using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using AquaModelLibrary.Data.PSO2.Aqua;

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
    }
}
