using System.IO;
using System.Text;
using CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Clients.EDI.Tests.CMD_FMA_FNA_XSDTests
{
	[TestClass]
	public class CMD_FMA_FNA_XSDTests
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNACMDShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"CMD_FMA_FNA_XSDTests.TestFiles.FNACMDShort.txt");
            string expectedOutputXMLFileResourceName = "CMD_FMA_FNA_XSDTests.TestFiles.FNACMDShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, CMD_FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNACMDLong()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"CMD_FMA_FNA_XSDTests.TestFiles.FNACMDLong.txt");
            string expectedOutputXMLFileResourceName = "CMD_FMA_FNA_XSDTests.TestFiles.FNACMDLong.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, CMD_FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMACMDShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"CMD_FMA_FNA_XSDTests.TestFiles.FMACMDShort.txt");
            string expectedOutputXMLFileResourceName = "CMD_FMA_FNA_XSDTests.TestFiles.FMACMDShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, CMD_FMA_FNASchemaFilePath);
        }

        #region Implementation

		string CMD_FMA_FNASchemaFilePath
		{
			get
			{
				if (schemaFilePath == null || !File.Exists(schemaFilePath))
				{
					schemaFilePath = Path.Combine(Path.GetTempPath(), "CMD_FMA_FNA.xsd");

					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
                        var xml = new CMD_FMA_FNA();
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
