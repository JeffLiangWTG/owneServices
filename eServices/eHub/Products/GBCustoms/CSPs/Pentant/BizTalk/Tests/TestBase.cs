using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.BT.Tests
{
    public class TestBase
    {
        public Stream GetEmbeddedResource(string resourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
            if (resource == null)
            {
                throw new Exception($"Could not locate embedded resource '{fullResourceName}'");
            }
            return resource;
        }

        public string GetResourceAsString(string resourceName)
        {
            using (var stream = GetEmbeddedResource(resourceName))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static void AssertXmlEquals(string errorMessage, string expected, string actual)
        {
            if (expected == actual)
            {
                Assert.That(true);
            }
            else
            {
                expected = ProcessXmlStringIfNeeded(expected);
                actual = ProcessXmlStringIfNeeded(actual);

                var expectedFormatted = expected.Replace(">", ">" + Environment.NewLine) + Environment.NewLine + Environment.NewLine + expected;
                var actualFormatted = actual.Replace(">", ">" + Environment.NewLine) + Environment.NewLine + Environment.NewLine + actual;
                Assert.That(actualFormatted, Is.EqualTo(expectedFormatted), errorMessage);
            }
        }

        public static string ProcessXmlStringIfNeeded(string xmlStr)
        {
            if (xmlStr == null)
            {
                return null;
            }

            var str = xmlStr;
            str = str.TrimEnd('\r', '\n', ' ');

            if (!str.Contains(XmlStringFlag))
            {
                return xmlStr;
            }

            if (Is32Bit)
            {
                return str.Replace(XmlString64Bit, XmlString32Bit);
            }
            return str.Replace(XmlString32Bit, XmlString64Bit);
        }

        const string XmlStringFlag = @"http://www.w3.org/2001/XMLSchema-instance";
        const string XmlString32Bit = @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""";
        const string XmlString64Bit = @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""";
        static readonly bool Is32Bit = IntPtr.Size == 4;
    }
}
