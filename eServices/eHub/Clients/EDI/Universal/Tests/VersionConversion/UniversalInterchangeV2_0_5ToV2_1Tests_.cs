using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV2_0_5ToV2_1Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_1_FPA()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_FPA.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_FPA.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_1_FPA_NoParameters()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_FPA_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_FPA_OnlyOneParameterType.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_1_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_0_5ToV2_1_UnknownType_NoParameters_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeEnvelopeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeEnvelopeV2_1.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2ToV2_1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeIncludeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeIncludeV2_1.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2_0_5ToV2_1>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
	}
}
