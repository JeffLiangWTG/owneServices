using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV2_0_5V2Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5V1_FOB()
		{
			MapTester mapTester = GetMapTester();
			
			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);

			sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1_FOB.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5V2_FOB()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5V2_SOB()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_SOB.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB.xml"; //YES, it is FOB(not SOB). On the way back, FLO will become FOB. SOB apparently was a duplicate event that never should have been added.
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5V2_FUL()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FUL.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FUL.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_FOB_NoParameters()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_0_5_FOB_OnlyOneParameterFacility.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_FOB_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_UnknownType_NoParameters_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2_0_5V2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeEnvelopeV2_0_5.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeEnvelopeV2.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2_0_5V2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeIncludeV2_0_5.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeIncludeV2.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2V2_0_5.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
	}
}
