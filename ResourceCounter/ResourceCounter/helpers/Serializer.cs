using System;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace ResourceCounter.helpers {
    public class Settings {
        public string RootPath { get; set; }
        public Settings() {
            RootPath="C:\\";
        }
    }

    public static class Serializer {
        public static T Deserialize<T>(this string xml) where T : new() {
            if (String.IsNullOrEmpty(xml))
                return new T();

            XmlSerializer xs = new XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream(xml.StringToUTF8ByteArray())) {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

                return (T)xs.Deserialize(memoryStream);
            }
        }

        public static string ToXml(this object o) {
            try {
                if (o == null)
                    return "";

                using (var s = new System.IO.MemoryStream()) {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                    //Add an empty namespace and empty value
                    ns.Add("", "");

                    //Create the serializer
                    XmlSerializer slz = new XmlSerializer(o.GetType(), "");

                    var sw = new StringWriter();
                    var writer = new XmlTextWriterFormattedNoDeclaration(sw);
                    slz.Serialize(writer, o, ns);

                    var text = sw.ToString();
                    return text;
                }
            } catch (Exception) {
                return "<unserializable object " + o.GetType().Name + ">";
            }
        }

        public static void SaveSettings(object o) {
            var path = AppDomain.CurrentDomain.BaseDirectory + "settings.xml";
            var xml = Serializer.ToXml(o);
            File.WriteAllText(path, xml);
        }

        public static T LoadSettings<T>() where T : new() {
            var path = AppDomain.CurrentDomain.BaseDirectory + "settings.xml";
            if (File.Exists(path)) {
                var xml = File.ReadAllText(path);
                return  Serializer.Deserialize<T>(xml);
            }
            return new T();
        }

        public static Byte[] StringToUTF8ByteArray(this string pXmlString) {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
    }

    public class XmlTextWriterFormattedNoDeclaration : XmlTextWriter {
        public XmlTextWriterFormattedNoDeclaration(TextWriter w)
            : base(w) {
            Formatting = System.Xml.Formatting.Indented;
        }

        public override void WriteStartDocument() { } 
    }
}
