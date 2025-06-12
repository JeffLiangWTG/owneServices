using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV2_1ToV2_0_5Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_0_5_FPA()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_FPA.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_FPA.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_0_5_FPA_OnlyOneParameterType()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_FPA_OnlyOneParameterType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_FPA_NoParameters.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_0_5_FPA_OnlyOneParameterType_UnknowValue()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_FPA_OnlyOneParameterType_UnknowValue.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_FPA_OnlyOneParameterType_UnknowValue.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_0_5_UnknownType_OnlyOneParameterType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_UnknownType_OnlyOneParameterType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_1_UnknownType_OnlyOneParameterType.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_1ToV2_0_5_UnknownType_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV2_UnknownType.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2_1ToV2_0_5()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeEnvelopeV2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeEnvelopeV2.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2_1ToV2_0_5()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeIncludeV2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeIncludeV2.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V2_0_5V2_1.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2_1ToV2_0_5>(sourceFile, expectedFile);
		}

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
	}
}
