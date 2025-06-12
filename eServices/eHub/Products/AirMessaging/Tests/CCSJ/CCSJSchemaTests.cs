using System.IO;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.CCSJ.Tests
{
	[TestClass]
	public class CCSJSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJReplyMessageSchema_FSU()
		{
			using (var inputMsg = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJFSU.txt"))
			using (var outputMsg = SchemaTester<CCSJReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg)) 
				Assert.AreEqual(GetResourceAsString("CCSJ.TestFiles.Text.CCSJFSU.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJReplyMessageSchema_Batch()
		{
			using (var inputMsg = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJ_FMA_FNA_batch.txt"))
			using (var outputMsg = SchemaTester<CCSJReplyMessageBatch>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg)) 
				Assert.AreEqual(GetResourceAsString("CCSJ.TestFiles.Text.CCSJ_FMA_FNA_batch.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCSJReplyMessageSchema_Large()
		{
			using (var inputMsg = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJ_Large.txt"))
			using (var outputMsg = SchemaTester<CCSJReplyMessageBatch>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("CCSJ.TestFiles.Text.CCSJ_Large.xml"), sr.ReadToEnd());
		}
	}
}
