using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Hawking.UnitTest.Tools.Xml
{
    public static class XmlHelper
    {
        /// <summary>
        /// Create an Xml writer object that can indent the Xml elements and exclude the UTF8 byte order
        /// mark so that any test failure output is easily readable
        /// </summary>
        /// <param name="buffer">Buffer that Xml will be written to</param>
        public static XmlWriter CreateFormattedXmlWriter(Stream buffer)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(false)
            };

            return XmlWriter.Create(buffer, settings);
        }

        /// <summary>
        /// Replaces the required node(s) value using XPath
        /// </summary>
        /// <param name="target">Xml stream to be changed</param>
        /// <param name="changeSets">Location of element or attribute to change value of plus the new value at that xpath</param>
        /// <returns>An edited Xml stream</returns>
        public static Stream ReplaceNodeValue(Stream target, List<KeyValuePair<string, string>> changeSets)
        {
            var editedMemoryStream = target;

            changeSets.ForEach(delegate (KeyValuePair<string, string> changeSet)
            {
                editedMemoryStream = ReplaceNodeValue(editedMemoryStream, changeSet.Key, changeSet.Value);
            });

            return editedMemoryStream;
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
            var targetDocument = new XmlDocument();
            targetDocument.Load(target);
            var nodes = targetDocument.DocumentElement.SelectNodes(xpath);
            if (nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    node.InnerText = replacementValue;
                }

                var editedMemoryStream = new MemoryStream();

                try
                {
                    var settings = new XmlWriterSettings
                    {
                        CloseOutput = false,
                        Indent = true,
                        Encoding = new UTF8Encoding(false)
                    };

                    using (var writer = XmlWriter.Create(editedMemoryStream, settings))
                    {
                        targetDocument.WriteTo(writer);
                        writer.Flush();
                    }

                    editedMemoryStream.Position = 0;

                    return editedMemoryStream;
                }
                catch (Exception)
                {
                    // Tidy up after ourselves
                    editedMemoryStream.Dispose();
                    throw;
                }
            }

            throw new XmlException(string.Format("Node not found at : {0}", xpath));
        }

        /// <summary>
        /// Deserialise an object from and Xml string
        /// </summary>
        /// <typeparam name="T">Type to deserialise to</typeparam>
        /// <param name="xml">Xml to deserialise</param>
        /// <returns>Typed object represented by Xml string</returns>
        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (var reader = XmlReader.Create(ms))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Serialize an object to an Xml string
        /// </summary>
        /// <typeparam name="T">Type to serialize from</typeparam>
        /// <param name="target">Object to serialize</param>
        /// <returns>Object as Xml</returns>
        public static string Serialize<T>(object target)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                using (var writer = XmlHelper.CreateFormattedXmlWriter(ms))
                {
                    serializer.Serialize(writer, target);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
