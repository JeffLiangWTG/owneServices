using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV2ToV1Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_withoutheader()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.C_UniversalInterchangeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.C_UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_ACD()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ACD.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_ACD.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_CIW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DHR_CIW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_CIW.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_DOJ()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DOJ.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GIC()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIC.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIC.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GIM()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIM.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIM.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GIW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIW.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GIY()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIY.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIY.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GOC()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOC.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOC.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GOM()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOM.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOM.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GOW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOW.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_GOY()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOY.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOY.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2ToV1_FMA()
        {
            var mapTester = GetMapTester();
            const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FMA.xml";
            const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_FMA.xml";
            mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV2ToV1_FNA()
        {
            var mapTester = GetMapTester();
            const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FNA.xml";
            const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_FNA.xml";
            mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7A()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7A.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7A.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7E()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7E.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7E.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7P()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7P.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7P.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7R()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7R.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7W()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7W.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7W.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7X()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7X.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7X.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_X7M()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7M.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7M.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XAF()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XAF.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XD1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XD1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XD1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XET()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XET.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XET.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XP1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XP1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XP1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XRW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XRW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XRW.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XX1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XX3()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX3.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX3.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XX4()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX4.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX4.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_XXX()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XXX.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XXX.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MAA2X7A()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAA.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7A.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MAAWithErrors2X7E()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAAWithErrors.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7E.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MAAPartial2X7P()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAAPartial.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7P.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MAAWithoutEventReference2X7A()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAAWithoutEventReference.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7A_WithoutEventReference.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MRJ1Stop2DOJ()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJ1Stop.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MRJForwardAir2X7R()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJForwardAir.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_MRJForwardAirWithoutEventReference2X7R()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJForwardAirWithoutEventReference.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R_WithoutEventReference.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRJFailedValidityTests2X7W()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJFailedValidityTests.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7W.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRJDecryptionFailure2X7X()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJDecryptionFailure.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7X.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRJMACFailed2X7M()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJMACFailed.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7M.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRJWithoutEventReference2X7R()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJWithoutEventReference.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R_WithoutEventReference.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_DEPPickupLocation2XAF()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DEP_PickupLocation.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_FUL2XD1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FUL.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XD1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_ARVOriginalTerminal2XET()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVOriginalTerminal.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XET.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_DEPTerminal2XP1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DEPTerminal.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XP1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_SSO2XRW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_SSO.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XRW.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_ARVDeliveryLocation2XX1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVDeliveryLocation.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_ARVPickupLocation2XX3()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVPickupLocation.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX3.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_ARVDestinationTerminal2XX4()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVDestinationTerminal.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX4.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRA2XXX()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRA.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XXX.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2ToV1_IRAWithoutEventReference2XXX()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRAWithoutEventReference.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XXX.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV2ToV1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeEnvelopeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeEnvelopeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV2ToV1()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeIncludeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeIncludeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1.xml";
			mapTester.Execute<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestSubContext()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.SubContextV2.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.SubContextV1.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestNative()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.Native.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.Native.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransaction()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalTransaction.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalTransaction.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV2ToV1>(sourceFile, expectedFile);
        }

		MapTester GetMapTester()
		{
			var exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='ServerID']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			return new MapTester(Assembly.GetExecutingAssembly(), comparer);
		}
	}
}
