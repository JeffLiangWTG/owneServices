using System.IO;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.GLSHK.Tests
{
	[TestClass]
	public class GLSHKSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKReplyMessageSchema_FSU()
		{
			using (var inputMsg = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFSU.txt"))
			using (var outputMsg = SchemaTester<GLSHKReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("GLSHK.TestFiles.GLSHKFSU.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKReplyMessageSchema_FNAFHL()
		{
			using (var inputMsg = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFNAFHL.txt"))
			using (var outputMsg = SchemaTester<GLSHKReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("GLSHK.TestFiles.GLSHKFNAFHL.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKReplyMessageSchema_FNAFWB()
		{
			using (var inputMsg = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFNAFWB.txt"))
			using (var outputMsg = SchemaTester<GLSHKReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
				Assert.AreEqual(GetResourceAsString("GLSHK.TestFiles.GLSHKFNAFWB.xml"), sr.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GLSHKReplyMessageSchema_FSA()
		{
			using (var inputMsg = GetEmbeddedResource("GLSHK.TestFiles.GLSHKFSA.txt"))
			using (var outputMsg = SchemaTester<GLSHKReplyMessage>.ParseFF(inputMsg))
			using (var sr = new StreamReader(outputMsg))
			{
				Assert.AreEqual(GetResourceAsString("GLSHK.TestFiles.GLSHKFSA.xml"), sr.ReadToEnd());
			}
		}
	}
}
