using System.IO;
using CargoWise.eHub.Common;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using CargoWise.eHub.Products.AirMessaging.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.AirMessaging.Delta.Tests
{
    [TestClass]
    public class CargonautSchemaTests : BaseSchemaTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CargonautReplyMessageSchema_FMA()
        {
            ValidateCargonautReplyMessageSchema("Cargonaut.TestFiles.Cargonaut_FMA.txt", "Cargonaut.TestFiles.Cargonaut_FMA.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CargonautReplyMessageSchema_FNA()
        {
            ValidateCargonautReplyMessageSchema("Cargonaut.TestFiles.Cargonaut_FNA.txt", "Cargonaut.TestFiles.Cargonaut_FNA.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CargonautReplyMessageSchema_FSU()
        {
            ValidateCargonautReplyMessageSchema("Cargonaut.TestFiles.Cargonaut_FSU.txt", "Cargonaut.TestFiles.Cargonaut_FSU.xml");
        }

        void ValidateCargonautReplyMessageSchema(string inputMsgFile, string outputMsgFile)
        {
            using (var inputMsg = GetEmbeddedResource(inputMsgFile))
            using (var normalizedMsg = new NormalizedLineEndingsStream(inputMsg))
            using (var outputMsg = SchemaTester<CargonautReplyMessage>.ParseFF(normalizedMsg))
            using (var sr = new StreamReader(outputMsg))
            Assert.AreEqual(GetResourceAsString(outputMsgFile), sr.ReadToEnd());
        }

    }
}
