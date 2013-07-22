using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace QuestCompiler
{
    public class Settings
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Profile { get; set; }
        public bool Debug { get; set; }
        public bool Minify { get; set; }
        public bool Gamebook { get; set; }
    }

    internal class SettingsManager
    {
        public Settings LoadSettings(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (var stream = new FileStream(file, FileMode.Open))
            {
                return (Settings)serializer.Deserialize(stream);
            }
        }

        public void SaveSettings(string file, Settings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (var stream = new FileStream(file, FileMode.Create))
            {
                serializer.Serialize(stream, settings);
            }
        }
    }
}
