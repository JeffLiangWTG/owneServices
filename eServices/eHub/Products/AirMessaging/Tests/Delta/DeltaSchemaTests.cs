using System.IO;
using CargoWise.eHub.Common;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.Delta.Tests
{
	[TestClass]
	public class DeltaSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DeltaReplyMessageSchema_FSU()
		{
			using (var inputMsg = GetEmbeddedResource("Delta.TestFiles.DeltaFSU.txt"))
			using (var normalizedMsg = new NormalizedLineEndingsStream(inputMsg))
			using (var outputMsg = SchemaTester<DeltaReplyMessage>.ParseFF(normalizedMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("Delta.TestFiles.DeltaFSU.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DeltaReplyMessageSchema_FNA()
		{
			using (var inputMsg = GetEmbeddedResource("Delta.TestFiles.DeltaFNA.txt"))
			using (var normalizedMsg = new NormalizedLineEndingsStream(inputMsg))
			using (var outputMsg = SchemaTester<DeltaReplyMessage>.ParseFF(normalizedMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("Delta.TestFiles.DeltaFNA.xml"), sr.ReadToEnd());
		}
	}
}
