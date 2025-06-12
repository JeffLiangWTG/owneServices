using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Universal.Tests
{
	[TestClass]
	public class UniversalInterchangeV1ToV2Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_ACD()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_ACD.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ACD.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_ACW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_ACW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ACW.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1ToV2_FMA()
        {
            var mapTester = GetMapTester();
            const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_FMA.xml";
            const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FMA.xml";
            mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalInterchangeV1ToV2_FNA()
        {
            var mapTester = GetMapTester();
            const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_FNA.xml";
            const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FNA.xml";
            mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_CIW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_CIW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_CIW.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_DOJ()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DOJ.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_DOJ_DepartmentHidden()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ_DepartmentHidden.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DOJ.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GIC()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIC.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIC.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GIM()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIM.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIM.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GIW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIW.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GOC()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOC.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOC.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GOM()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOM.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOM.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GIY()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GIY.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GIY.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GOW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOW.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_GOY()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_GOY.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_GOY.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7A()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7A.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7A.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7E()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7E.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7E.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7M()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7M.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7M.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7P()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7P.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7P.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7R()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7R.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7W()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7W.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7W.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7X()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7X.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_X7X.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XAF()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XAF.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XAF_LocAndFacHidden()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF_LocAndFacHidden.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XAF.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XD1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XD1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XD1.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XET()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XET.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XET.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XP1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XP1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XP1.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XRW()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XRW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XRW.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX1()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX1.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX3()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX3.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX3.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX4()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX4.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XX4.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XXX()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XXX.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_XXX.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7A2MAA()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7A.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAA.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7E2MAAWithErrors()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7E.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAAWithErrors.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7P2MAAPartial()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7P.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MAAPartial.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_DOJ2MRJ1Stop()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJ1Stop.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_DOJDepartmentHidden2MRJ1Stop()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_DOJ_DepartmentHidden.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJ1Stop.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7R2MRJForwardAir()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7R.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_MRJForwardAir.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7W2IRJFailedValidityTests()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7W.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJFailedValidityTests.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7X2IRJDecryptionFailure()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7X.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJDecryptionFailure.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_X7M2IRJMACFailed()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_X7M.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRJMACFailed.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XAF2DEPPickupLocation()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DEP_PickupLocation.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XAFLocAndFacHidden2DEPPickupLocation()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XAF_LocAndFacHidden.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DEP_PickupLocation.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XD12FUL()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XD1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_FUL.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XET2ARVOriginalTerminal()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XET.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVOriginalTerminal.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XP12DEPTerminal()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XP1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_DEPTerminal.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XRW2SSO()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XRW.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_SSO.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX12ARVDeliveryLocation()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX1.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVDeliveryLocation.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX32ARVPickupLocation()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX3.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVPickupLocation.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XX42ARVDestinationTerminal()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XX4.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_ARVDestinationTerminal.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV1ToV2_XXX2IRA()
		{
			var mapTester = GetMapTester();
			const string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV1_XXX.xml";
			const string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2_IRA.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeEnvelopeV1ToV2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeEnvelopeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeEnvelopeV2.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeIncludeV1ToV2()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeIncludeV1.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeIncludeV2.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeShipment_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeShipment.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeShipment.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchangeV2_ShouldCopyAsIs()
		{
			MapTester mapTester = GetMapTester();

			string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2.xml";
			string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalInterchangeV2.xml";
			mapTester.Execute<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestSubContext()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.SubContextV1.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.SubContextV2.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestNative()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.Native.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.Native.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransaction()
        {
            MapTester mapTester = GetMapTester();

            string sourceFile = "VersionConversion.TestFiles.V1V2.UniversalTransaction.xml";
            string expectedFile = "VersionConversion.TestFiles.V1V2.UniversalTransaction.xml";
            mapTester.ExecuteCompiled<UniversalInterchangeV1ToV2>(sourceFile, expectedFile);
        }

		MapTester GetMapTester()
		{
			return new MapTester(Assembly.GetExecutingAssembly());
		}
	}
}
