using System.IO;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.Traxon.Tests
{
	[TestClass]
	public class TraxonSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TraxonReplyMessageSchema_FSU()
		{
			using (var inputMsg = GetEmbeddedResource("Traxon.TestFiles.TraxonFSU.txt"))
			using (var outputMsg = SchemaTester<TraxonReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("Traxon.TestFiles.TraxonFSU.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TraxonReplyMessageSchema_FMA()
		{
			using (var inputMsg = GetEmbeddedResource("Traxon.TestFiles.TraxonFMAFWB.txt"))
			using (var outputMsg = SchemaTester<TraxonReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("Traxon.TestFiles.TraxonFMAFWB.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TraxonReplyMessageSchema_FNA()
		{
			using (var inputMsg = GetEmbeddedResource("Traxon.TestFiles.TraxonFNAFHL.txt"))
			using (var outputMsg = SchemaTester<TraxonReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("Traxon.TestFiles.TraxonFNAFHL.xml"), sr.ReadToEnd());
		}
	}
}
