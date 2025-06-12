using System.IO;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.BT.Tests
{
	[TestClass]
	public class BTSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BTReplyMessageSchema_FSU()
		{
			using (var inputMsg = GetEmbeddedResource("BT.TestFiles.Text.BTFSU.txt"))
			using (var outputMsg = SchemaTester<BTReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("BT.TestFiles.Text.BTFSU.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BTReplyMessageSchema_FMA()
		{
			using (var inputMsg = GetEmbeddedResource(@"BT.TestFiles.Text.BTFMA.txt"))
			using (var outputMsg = SchemaTester<BTReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("BT.TestFiles.Text.BTFMA.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BTReplyMessageSchema_FNA()
		{
			using (var inputMsg = GetEmbeddedResource(@"BT.TestFiles.BTFNA.txt"))
			using (var outputMsg = SchemaTester<BTReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("BT.TestFiles.BTFNA.xml"), sr.ReadToEnd());
		}
	}
}
