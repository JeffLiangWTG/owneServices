using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV2V2_0_5Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1V2_0_5_FOB()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1_FOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);

			sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2V2_0_5_FOB()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2V2_0_5_SOB()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_SOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_SOB.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2V2_0_5_FUL()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FUL.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FUL.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV2_0_5_FOB_NoParameters()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB_OnlyOneParameterFacility.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV2_0_5_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV2_0_5_UnknownType_NoParameters_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeEnvelopeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeEnvelopeV2_0_5.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeIncludeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeIncludeV2_0_5.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV2_0_5>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
	}
}
