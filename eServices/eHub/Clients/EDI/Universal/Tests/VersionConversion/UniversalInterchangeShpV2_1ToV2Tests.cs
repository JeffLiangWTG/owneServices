using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
    [TestClass]
	public class UniversalInterchangeShpV2_1ToV2Tests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShpSample1V2_1ToV2() //Shipment only
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample1V2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample1V2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShpSample2V2_1ToV2() //Sea Consol
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample2V2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample2V2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShpSample3V2_1ToV2() //Air Consol
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample3V2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample3V2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShpSample4V2_1ToV2() //No <Vessel> node
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample4V2_1.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample4V2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMapUniversalInterchangeShpV2_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample1V2.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeShpSample1V2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMapUniversalInterchangeEvt_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeEvt.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeEvt.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMapUniversalInterchangeIncludeV2_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeIncludeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeIncludeV2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMapUniversalInterchangeEnvelopeV2_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeEnvelopeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.ShpV2_1V2.UniversalInterchangeEnvelopeV2.xml";
			mapTester.Execute<UniversalInterchangeShpV2_1ToV2>(sourceFile, expectedFile);
		}
		
		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
    }
}
