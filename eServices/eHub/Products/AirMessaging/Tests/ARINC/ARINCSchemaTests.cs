using System.IO;
using CargoWise.eHub.Common;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.Delta.Tests
{
    [TestClass]
    public class ARINCSchemaTests : BaseSchemaTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void ARINCReplyMessageSchema_FMA()
        {
            ValidateARINCReplyMessageSchema("ARINC.TestFiles.ARINC_FMA.txt", "ARINC.TestFiles.ARINC_FMA.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void ARINCReplyMessageSchema_FNA()
        {
            ValidateARINCReplyMessageSchema("ARINC.TestFiles.ARINC_FNA.txt", "ARINC.TestFiles.ARINC_FNA.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ARINCReplyMessageSchema_FNAMultipleReasonLines()
		{
			ValidateARINCReplyMessageSchema("ARINC.TestFiles.ARINC_FNA_MultiLines.txt", "ARINC.TestFiles.ARINC_FNA_MultiLines.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void ARINCReplyMessageSchema_FSU()
        {
            ValidateARINCReplyMessageSchema("ARINC.TestFiles.ARINC_FSU.txt", "ARINC.TestFiles.ARINC_FSU.xml");
        }

        void ValidateARINCReplyMessageSchema(string inputMsgFile, string outputMsgFile)
        {
            using (var inputMsg = GetEmbeddedResource(inputMsgFile))
            using (var normalizedMsg = new NormalizedLineEndingsStream(inputMsg))
            using (var outputMsg = SchemaTester<ARINCReplyMessage>.ParseFF(normalizedMsg))
            using (var sr = new StreamReader(outputMsg))
            Assert.AreEqual(GetResourceAsString(outputMsgFile), sr.ReadToEnd());
        }

    }
}
