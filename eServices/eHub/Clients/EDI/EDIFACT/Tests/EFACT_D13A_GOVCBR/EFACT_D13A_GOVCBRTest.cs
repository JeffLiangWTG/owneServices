using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas2.D13A;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.EDIFACT.Tests
{
	[TestClass]
	public class EFACT_D13A_GOVCBRTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTheMaxOccursOfERCLoop1GreaterThan999()
		{
			XmlSchemaSet schemas = new EFACT_D13A_GOVCBR().SchemaSet;
			XDocument doc = XDocument.Parse(GetResourceAsString("EFACT_D13A_GOVCBR.TestFiles.EFACT_D13A_GOVCBR_ERCLoop1GreaterThan999.xml"));
			string msg = "";
			doc.Validate(schemas, (o, e) => {
				msg += e.Message + Environment.NewLine;
			});
			Assert.AreEqual("", msg);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTheMaxOccursOfDOCLoop1GreaterThan99()
		{
			XmlSchemaSet schemas = new EFACT_D13A_GOVCBR().SchemaSet;
			XDocument doc = XDocument.Parse(GetResourceAsString("EFACT_D13A_GOVCBR.TestFiles.EFACT_D13A_GOVCBR_DOCLoop1GreaterThan99.xml"));
			string msg = "";
			doc.Validate(schemas, (o, e) => {
				msg += e.Message + Environment.NewLine;
			});
			Assert.AreEqual("", msg);
		}

		string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}
	}
}
