using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
    public class UniversalInterchangeV2_1ToV2_2Tests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CKC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CKC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CKC.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CKR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CKR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CKR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CUC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CUC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CUC.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CUR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CUR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CUR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CSC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CSC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CSC.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CSR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CSR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CSR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CTC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CTC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CTC.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CTR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CTR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CTR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CWC()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CWC.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CWC.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CWR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CWR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CWR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_FOK()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_FOK.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_FOK.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_FRQ()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_FRQ.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_FRQ.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_QOK()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_QOK.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_QOK.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_QRQ()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_QRQ.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_QRQ.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_CIR()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_CIR.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_CIR.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_NoParameters()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_2_OnlyOneParameter.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_2_UnknownType_NoParameters_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV2_1_UnknownType_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2_1ToV2_2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeEnvelopeV2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeEnvelopeV2_2.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2_1ToV2_2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeIncludeV2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeIncludeV2_2.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_1V2_2.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_2>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
    }
}
