using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NLog;

namespace NetworkGameCopier
{
    class SettingsManager
    {
        private static readonly SettingsManager Instance = new SettingsManager();

        public static SettingsManager GetInstance()
        {
            return Instance;
        }

        private enum SettingsKey
        {
            SteamLibKey,
            BlizzardPathKey
        }

        private readonly SerializableDictionary<SettingsKey, string> _settings
            = new SerializableDictionary<SettingsKey, string>();
        private SettingsManager()
        {
            try
            {
                _settings.ReadXml(XmlReader.Create(new StreamReader("settings.xml")));
            }
            catch (FileNotFoundException)
            {
                LogManager.GetCurrentClassLogger().Info("Fresh start!");
                SetDefaultBlizzardPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                SetDefaultSteamLibrary(Path.Combine(SteamOperations.GetInstance().SteamappsPath, "common"));
            }
        }

        public void SetDefaultSteamLibrary(string path)
        {
            _settings[SettingsKey.SteamLibKey] = path;
        }

        public void SetDefaultBlizzardPath(string path)
        {
            _settings[SettingsKey.BlizzardPathKey] = path;
        }

        public string GetDefaultSteamLibrary()
        {
            return _settings[SettingsKey.SteamLibKey];
        }

        public string GetDefaultBlizzardPath()
        {
            return @"C:\teste";
            //return _settings[SettingsKey.BlizzardPathKey];
        }

    }


 
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
