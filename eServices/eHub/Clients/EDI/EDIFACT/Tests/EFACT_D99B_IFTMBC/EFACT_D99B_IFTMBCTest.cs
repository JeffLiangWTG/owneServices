using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.EDIFACT.Tests
{
	[TestClass]
	public class EFACT_D99B_IFTMBCTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTheMaxOccursOfFTX3GreaterThan9()
		{
			XmlSchemaSet schemas = new EFACT_D99B_IFTMBC().SchemaSet;
			XDocument doc = XDocument.Parse(GetResourceAsString("EFACT_D99B_IFTMBC.TestFiles.EFACT_D99B_IFTMBC_FTX3GreaterThan9.xml"));
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
