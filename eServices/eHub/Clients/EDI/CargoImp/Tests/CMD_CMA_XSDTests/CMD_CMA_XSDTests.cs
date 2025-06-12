using System.IO;
using System.Text;
using CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Clients.EDI.Tests.CMD_CMA_XSDTests
{
    [TestClass]
    public class CMD_CMA_XSDTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CMD_CMA_XSDTests_CMDCMALong()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"CMD_CMA_XSDTests.TestFiles.CMDCMALong.txt");
            string expectedOutputXMLFileResourceName = "CMD_CMA_XSDTests.TestFiles.CMDCMALong.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, CMDCMASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void CMD_CMA_XSDTests_CMDCMAShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"CMD_CMA_XSDTests.TestFiles.CMDCMAShort.txt");
            string expectedOutputXMLFileResourceName = "CMD_CMA_XSDTests.TestFiles.CMDCMAShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, CMDCMASchemaFilePath);
        }


        #region Implementation

        string CMDCMASchemaFilePath
        {
            get
            {
                if (schemaFilePath == null || !File.Exists(schemaFilePath))
                {
                    schemaFilePath = Path.Combine(Path.GetTempPath(), "CMD_CMA.xsd");

                    using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
                    {
                        var xml = new CMD_CMA();
                        writer.Write(xml.XmlContent);
                    }
                }

                return schemaFilePath;
            }
        }

        string schemaFilePath;

        #endregion
    }
}
