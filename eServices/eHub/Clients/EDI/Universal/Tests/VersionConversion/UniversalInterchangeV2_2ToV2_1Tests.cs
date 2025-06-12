using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
	public class UniversalInterchangeV2_2ToV2_1Tests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CKC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CKC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CKC.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CKR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CKR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CKR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CUC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CUC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CUC.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CUR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CUR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CUR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CSC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CSC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CSC.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CSR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CSR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CSR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CTC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CTC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CTC.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CTR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CTR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CTR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CWC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CWC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CWC.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CWR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CWR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CWR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_FOK()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_FOK.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_FOK.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_FRQ()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_FRQ.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_FRQ.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_QOK()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_QOK.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_QOK.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_QRQ()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_QRQ.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_QRQ.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_CIR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CIR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CIR.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_OnlyOneParameterType()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_OnlyOneParameter.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_2ToV2_1_UnknownType_NoParameters_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2_2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeEnvelopeV2_2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeEnvelopeV2_1.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2_2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeIncludeV2_2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeIncludeV2_1.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2_2ToV2_1>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
    }
}
