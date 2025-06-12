using System.IO;
using System.Text;
using CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Clients.EDI.Tests.FMA_FNA_XSDTests
{
	[TestClass]
	public class FMA_FNA_XSDTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFHLLong()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFHLLong.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFHLLong.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFHLShort()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFHLShort.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFHLShort.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFWBLong()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFWBLong.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFWBLong.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFWBShort()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFWBShort.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFWBShort.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMAFWBWithoutOriginDestination()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFWBWithoutOriginDestination.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFWBWithoutOriginDestination.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FNAFHLLong()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFHLLong.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFHLLong.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FNAFHLShort()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFHLShort.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFHLShort.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FNAFWBLong()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFWBLong.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFWBLong.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FNAFWBShort()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFWBShort.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFWBShort.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFHLSelf()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFHLSelf.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFHLSelf.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FMA_FNA_XSDTests_FMAFWBSelf()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFWBSelf.txt");
			string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFWBSelf.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMAISAC()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAISAC.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAISAC.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNAISAC()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAISAC.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAISAC.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNACMDShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNACMDShort.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNACMDShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNACMDLong()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNACMDLong.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNACMDLong.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMACMDShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMACMDShort.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMACMDShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMAFFRLong()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFFRLong.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFFRLong.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMAFFRShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFFRShort.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFFRShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNAFFRLong()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFFRLong.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFFRLong.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FNAFFRShort()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FNAFFRShort.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FNAFFRShort.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void FMA_FNA_XSDTests_FMAFFRSelf()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FMA_FNA_XSDTests.TestFiles.FMAFFRSelf.txt");
            string expectedOutputXMLFileResourceName = "FMA_FNA_XSDTests.TestFiles.FMAFFRSelf.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FMA_FNASchemaFilePath);
        }

		#region Implementation

		string FMA_FNASchemaFilePath
		{
			get
			{
				if (schemaFilePath == null || !File.Exists(schemaFilePath))
				{
					schemaFilePath = Path.Combine(Path.GetTempPath(), "FMA_FNA.xsd");

					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
                        var xml = new FMA_FNA();
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
