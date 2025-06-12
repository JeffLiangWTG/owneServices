using System.Xml.Linq;
using CargoWise.eHub.Products.ITCustoms.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.eHub.Products.ITCustoms.Tests
{
	[TestClass]
	public class ResultSchemaTests
	{
		public Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
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

		public void AssertTestCase(String testcase)
		{
			var ff_source = "ResultSchemaTests.TestFiles.Text." + testcase + ".txt";
			var xml_source = "ResultSchemaTests.TestFiles.Text." + testcase + ".xml";

			var xml = GetResourceAsString(xml_source);
			var generated_xml = XDocument.Load(SchemaTester<ResultSchema>.ParseFF(GetEmbeddedResource(ff_source))).ToString();
			TestHelper.AssertXmlContentSameAsExpected(xml_source,generated_xml);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResultSchemaTest()
		{
			AssertTestCase("test01");
			AssertTestCase("test02");
		}
	}
}
