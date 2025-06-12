using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace eServices.Configuration.Tests
{
	public class TestBase
	{
		public string GetEmbeddedResourceString(string fileName)
		{
			using (var rdr = new StreamReader(GetEmbeddedResourceStream(fileName)))
				return rdr.ReadToEnd();
		}

		public Stream GetEmbeddedResourceStream(string fileName)
		{
			var resourceName = GetType().Namespace + ".TestFiles." + fileName;
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		}

		public static Guid GetNewGuid(int i)
		{
			return new Guid(Enumerable.Repeat((byte)(i % 16 * 0x11), 16).ToArray());
		}

		public void AssertXmlAreEqual(string expectedTestFileName, XmlDocument actualXmlDocument)
		{
			var xmlExpected = XDocument.Load(GetEmbeddedResourceStream(expectedTestFileName));
			var xmlActual = XDocument.Parse(actualXmlDocument.OuterXml);
			Assert.That(XNode.DeepEquals(xmlExpected, xmlActual), Is.True, "AssertXmlAreEqual failed.");
		}
	}
}
