using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Text;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;

namespace Cargowise.eHub.Clients.EDI.Tests.FSUFSA12XSDTests
{
	[TestClass]
	public class FSUFSA12XSDTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA12XSD_UsingFSU_BKD_OSI()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSU_BKD_OSI.txt");
			string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU_BKD_OSI.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA12XSD_UsingFSU_MANOnly()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSU_MANOnly.txt");
			string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU_MANOnly.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA12XSD_UsingMultipleCarrierAndDuplicateStatusRecords()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource("FSUFSA12XSDTests.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords.txt");
			string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU12_MultipleCarrierAndDuplicateStatusRecords.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
			File.Delete(inputFlatFilePath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFSUFSA12XSD_UsingOneCarrierAndAllStatusRecords()
		{
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSU12_OneCarrierAndAllStatusRecords.txt");
			string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU12_OneCarrierAndAllStatusRecords.xml";
			TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFSUFSA12XSD_NoDate()
        {
            string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSUNoDate.txt");
            string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSUNoDate.xml";
            TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFSUFSA12XSD_EmptyAirLineCode_PRE()
        {
	        string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSU_EmptyAirLineCode_PRE.txt");
	        string expectedOutputXMLFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU_EmptyAirLineCode_PRE.xml";
	        TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXMLFileResourceName, FSU_FSA12SchemaFilePath);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFSUFSA12XSD_NoDateProvided_DEP()
        {
			string inputFlatFilePath = TestHelper.GetFileWithEmbeddedResource(@"FSUFSA12XSDTests.TestFiles.FSU_NoDepartureDate_DEP.txt");
	        string expectedOutputXmlFileResourceName = "FSUFSA12XSDTests.TestFiles.FSU_NoDepartureDate_DEP.xml";
	        TestHelper.VerifyFlatFile2XMLWithSchema(inputFlatFilePath, expectedOutputXmlFileResourceName, FSU_FSA12SchemaFilePath);
        }

		#region Implementation

		string FSU_FSA12SchemaFilePath
		{
			get
			{
				if (schemaFilePath == null || !File.Exists(schemaFilePath))
				{
					schemaFilePath = Path.Combine(Path.GetTempPath(), "FSU_FSA12.xsd");

					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
						writer.Write(new FSU_FSA12().XmlContent);
					}
				}

				return schemaFilePath;
			}
		}

		string schemaFilePath;

		#endregion
	}
}
