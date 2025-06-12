using System.Xml.Linq;
using CargoWise.eHub.Products.USCustomsAirAMS.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tests
{
	[TestClass]
	public class SchemaTest
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
			var ff_source = "SchemaTests.TestFiles." + testcase + ".txt";
			var ff = GetResourceAsString(ff_source);
			var generated_xml = XDocument.Load(SchemaTester<USCustomsAirAMSMessage>.ParseFF(GetEmbeddedResource(ff_source))).ToString(); // XDocument is used for formatting

			var xml_source = "SchemaTests.TestFiles." + testcase + ".xml";
			var xml = GetResourceAsString(xml_source);

			Assert.AreEqual(xml, generated_xml); // Above AssembleFF as it provides better error message then exception thrown by AssembleFF
			var generated_ff = (new StreamReader(SchemaTester<USCustomsAirAMSMessage>.AssembleFF(GetEmbeddedResource(xml_source)))).ReadToEnd();
			Assert.AreEqual(ff, generated_ff);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Custom()
		{
			AssertTestCase("Custom01");
			AssertTestCase("Custom02");
			AssertTestCase("Custom03");
			AssertTestCase("Custom04");
			AssertTestCase("Custom05");
			AssertTestCase("Custom06");
			AssertTestCase("Custom07");
			AssertTestCase("Custom08");
			AssertTestCase("Custom09");
			AssertTestCase("Custom10");
		}

		// Example_* testfiles are taken from the examples document

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FDM()
		{
			AssertTestCase("Example_FDM");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FRC()
		{
			AssertTestCase("Example_FRC1");
			AssertTestCase("Example_FRC2");
			AssertTestCase("Example_FRC3");
			AssertTestCase("Example_FRC4");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FRI()
		{
			AssertTestCase("Example_FRI1");
			AssertTestCase("Example_FRI2");
			AssertTestCase("Example_FRI3");
			AssertTestCase("Example_FRI4");
			AssertTestCase("Example_FRI5");
			AssertTestCase("Example_FRI6");
			AssertTestCase("Example_FRI7");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FRX()
		{
			AssertTestCase("Example_FRX1");
			AssertTestCase("Example_FRX2");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FSI()
		{
			AssertTestCase("Example_FSI1");
			AssertTestCase("Example_FSI2");
			AssertTestCase("Example_FSI3");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FSN()
		{
			AssertTestCase("Example_FSN1");
			AssertTestCase("Example_FSN2");
			AssertTestCase("Example_FSN3");
			AssertTestCase("Example_FSN4");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FXC()
		{
			AssertTestCase("Example_FXC1");
			AssertTestCase("Example_FXC2");
			AssertTestCase("Example_FXC3");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FXI()
		{
			AssertTestCase("Example_FXI1");
			AssertTestCase("Example_FXI2");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FXX()
		{
			AssertTestCase("Example_FXX1");
			AssertTestCase("Example_FXX2");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FSC()
		{
			AssertTestCase("Example_FSC1");
			AssertTestCase("Example_FSC2");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FSQ()
		{
			AssertTestCase("Example_FSQ");
		}
	}
}
