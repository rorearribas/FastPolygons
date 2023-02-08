using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace FastPolygons
{
    public class Storage : MonoBehaviour
    {
        public static void Save<T>(T data, string filePath, string fileName)
        {
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StreamWriter writer = new StreamWriter(fileName + ".xml");
                
                xmlSerializer.Serialize(writer.BaseStream, data);
                writer.Close();
            }
            catch (IOException ex)
            {
                Debug.LogError("Access to the path is denied: " + ex.Message);
            }
        }

        public static T Load<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                {
                    return (T)xmlSerializer.Deserialize(fileStream);
                }
            }
            return default(T);
        }
    }
}
