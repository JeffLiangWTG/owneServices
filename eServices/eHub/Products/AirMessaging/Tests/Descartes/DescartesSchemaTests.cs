using System.IO;
using System.Xml;
using System.Xml.Schema;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.AirMessaging.Tests.Descartes
{
	[TestClass]
	public class DescartesSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DescartesReplyMessageSchema()
		{
			var xml = new XmlDocument();
			xml.Load(GetEmbeddedResource("Descartes.TestFiles.DescartesFWB.xml"));
			xml.Schemas.Add(XmlSchema.Read(new StringReader(new DescartesReplyMessage().XmlContent), null));
			xml.Validate(null);
		}
	}
}
