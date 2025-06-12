using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace CargoWise.BizTalk.UnitTestFX
{
    public static class XmlHelper
    {
        /// <summary>
        /// Replaces the required node(s) value using XPath
        /// </summary>
        /// <param name="target">Xml stream to be changed</param>
        /// <param name="changeSets">Location of element or attribute to change value of plus the new value at that xpath</param>
        /// <returns>An edited Xml stream</returns>
        public static Stream ReplaceNodeValue(Stream target, List<KeyValuePair<string, string>> changeSets)
        {
            Stream edited = target;
            changeSets.ForEach(delegate(KeyValuePair<string, string> changeSet)
            {
                edited = ReplaceNodeValue(edited, changeSet.Key, changeSet.Value);
            });
            return edited;
        }

        /// <summary>
        /// Replaces the required node(s) value using XPath
        /// </summary>
        /// <param name="target">Xml stream to be changed</param>
        /// <param name="xpath">Location of element or attribute to change value of</param>
        /// <param name="replacementValue">New value at <paramref name="xpath"/></param>
        /// <returns>An edited Xml stream</returns>
        public static Stream ReplaceNodeValue(Stream target, string xpath, string replacementValue)
        {
            XmlDocument targetDocument = new XmlDocument();
            targetDocument.Load(target);
            XmlNodeList nodes = targetDocument.DocumentElement.SelectNodes(xpath);
            if (nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    node.InnerText = replacementValue;
                }
                MemoryStream edited = new MemoryStream();
                try
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.CloseOutput = false;
                    settings.Indent = true;
                    settings.Encoding = new UTF8Encoding(false);
                    using (XmlWriter writer = XmlWriter.Create(edited, settings))
                    {
                        targetDocument.WriteTo(writer);
                        writer.Flush();
                    }
                    edited.Position = 0;
                    return edited;
                }
                catch (Exception)
                {
                    // Tidy up after ourselves
                    edited.Dispose();
                    throw;
                }
            }
            else
            {
                throw new XmlException(String.Format("Node not found at : {0}", xpath));
            }
        }

        /// <summary>
        /// Create an Xml writer object that can indent the Xml elements and exclude the UTF8 byte order
        /// mark so that any test failure output is easily readable
        /// </summary>
        /// <param name="buffer">Buffer that Xml will be written to</param>
        public static XmlWriter CreateFormattedXmlWriter(Stream buffer)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = new UTF8Encoding(false);
            return XmlWriter.Create(buffer, settings);
        }

        /// <summary>
        /// Deserialise an object from and Xml string
        /// </summary>
        /// <typeparam name="T">Type to deserialise to</typeparam>
        /// <param name="xml">Xml to deserilase</param>
        /// <returns>Typed object represented by Xml string</returns>
        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Serialise an object to an Xml string
        /// </summary>
        /// <typeparam name="T">Type to serialise from</typeparam>
        /// <param name="target">Object to serialise</param>
        /// <returns>Object as Xml</returns>
        public static string Serialize<T>(object target)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlHelper.CreateFormattedXmlWriter(ms))
                {
                    serializer.Serialize(writer, target);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
