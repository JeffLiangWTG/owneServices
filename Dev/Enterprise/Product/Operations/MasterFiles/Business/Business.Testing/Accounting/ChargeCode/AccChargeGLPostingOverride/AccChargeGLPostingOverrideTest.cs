using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeGLPostingOverride))]
	sealed class AccChargeGLPostingOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAddIntercompanyGLPostingOverrideToGlobalChargeCodeWontThrowException()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var glPostingOverridesHelper = new AccChargeCodeValidationTest.GLPostingOverridesHelper();
			glPostingOverridesHelper.PopulateCollection(globalChargeCode);
			glPostingOverridesHelper.PopulateCollection(normalChargeCode);

			var glPostingOverride = globalChargeCode.GLPostingOverrides.Cast<AccChargeGLPostingOverride>().First();
			glPostingOverride.Y1_GE = Guid.Empty;
			glPostingOverride.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			AssertNoExceptionThrown("ValidationOnLocalChargeCodeException shouldn't be thrown here", Factory.Save);

			normalChargeCodeLinked.GLPostingOverrides.Load();
			var glPostingOverrideOfLocalChargeCode = normalChargeCodeLinked.GLPostingOverrides.Cast<AccChargeGLPostingOverride>().FirstOrDefault();
			AssertNotNull(glPostingOverrideOfLocalChargeCode);
			AssertEquals(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, glPostingOverrideOfLocalChargeCode.Y1_ConsolidationAccountingCategoryClass);
			AssertEquals(Guid.Empty, glPostingOverrideOfLocalChargeCode.Y1_GE);
		}

		public void TestGlAccountSetupReadOnly()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var glPostingOverridesHelper = new AccChargeCodeValidationTest.GLPostingOverridesHelper();
			glPostingOverridesHelper.PopulateCollection(globalChargeCode);
			glPostingOverridesHelper.PopulateCollection(normalChargeCode);
			Factory.Save();
			normalChargeCodeLinked.GLPostingOverrides.Load();

			bool originalChargeCodesEditGLAccountSetup = Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed;
			bool originalGlobalChargeCodesEditGLAccountSetup = Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed;
			bool originalChargeCodesLTGEditGLAccountSetup = Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed;

			try
			{
				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = true;
				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = true;
				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = true;

				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCode.GLPostingOverrides[0], false);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCodeLinked.GLPostingOverrides[0], false);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(globalChargeCode.GLPostingOverrides[0], false);

				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = false;
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCode.GLPostingOverrides[0], true);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCodeLinked.GLPostingOverrides[0], false);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(globalChargeCode.GLPostingOverrides[0], false);

				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = false;
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCode.GLPostingOverrides[0], true);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCodeLinked.GLPostingOverrides[0], false);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(globalChargeCode.GLPostingOverrides[0], true);

				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = false;
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCode.GLPostingOverrides[0], true);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(normalChargeCodeLinked.GLPostingOverrides[0], true);
				AssertAccChargeGLPostingOverrideFieldsReadOnly(globalChargeCode.GLPostingOverrides[0], true);
			}
			finally
			{
				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = originalChargeCodesEditGLAccountSetup;
				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = originalGlobalChargeCodesEditGLAccountSetup;
				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = originalChargeCodesLTGEditGLAccountSetup;
			}
		}

		public void TestY1_TransportMode()
		{
			ChargeGLPostingOverride.Y1_JobType = "SHP";
			ChargeGLPostingOverride.Y1_TransportMode = "AIR";
			ChargeGLPostingOverride.Y1_JobType = "WKI";

			Assert("Y1_TransportMode should be readonly", ChargeGLPostingOverride.Y1_TransportModeInfo.ReadOnly);
			AssertEquals("ALL", ChargeGLPostingOverride.Y1_TransportMode);

			ChargeGLPostingOverride.Y1_TransportMode = "AIR";
			ChargeGLPostingOverride.Y1_JobType = "FCN";

			Assert("Y1_TransportMode should not be readonly", !ChargeGLPostingOverride.Y1_TransportModeInfo.ReadOnly);
			AssertEquals("AIR", ChargeGLPostingOverride.Y1_TransportMode);

			ChargeGLPostingOverride.Y1_TransportMode = "AIR";
			ChargeGLPostingOverride.Y1_JobType = "BRK";

			Assert("Y1_TransportMode should not be readonly", !ChargeGLPostingOverride.Y1_TransportModeInfo.ReadOnly);
			AssertEquals("AIR", ChargeGLPostingOverride.Y1_TransportMode);
		}

		public void TestY1_Direction()
		{
			ChargeGLPostingOverride.Y1_JobType = "SHP";
			ChargeGLPostingOverride.Y1_Direction = "EXP";
			ChargeGLPostingOverride.Y1_JobType = "WKI";

			Assert("Y1_Direction should be readonly", ChargeGLPostingOverride.Y1_DirectionInfo.ReadOnly);
			AssertEquals("ALL", ChargeGLPostingOverride.Y1_Direction);

			ChargeGLPostingOverride.Y1_Direction = "EXP";
			ChargeGLPostingOverride.Y1_JobType = "FCN";

			Assert("Y1_TransportMode should not be readonly", !ChargeGLPostingOverride.Y1_DirectionInfo.ReadOnly);
			AssertEquals("EXP", ChargeGLPostingOverride.Y1_Direction);

			ChargeGLPostingOverride.Y1_Direction = "IMP";
			ChargeGLPostingOverride.Y1_JobType = "BRK";

			Assert("Y1_Direction should not be readonly", !ChargeGLPostingOverride.Y1_DirectionInfo.ReadOnly);
			AssertEquals("IMP", ChargeGLPostingOverride.Y1_Direction);
		}

		public void TestY1_ConsolContainerMode()
		{
			ChargeGLPostingOverride.Y1_JobType = "SHP";
			ChargeGLPostingOverride.Y1_ConsolContainerMode = "LCL";
			ChargeGLPostingOverride.Y1_JobType = "WKI";

			Assert("Y1_ConsolContainerMode should be readonly", ChargeGLPostingOverride.Y1_ConsolContainerModeInfo.ReadOnly);
			AssertEquals("ALL", ChargeGLPostingOverride.Y1_ConsolContainerMode);

			ChargeGLPostingOverride.Y1_ConsolContainerMode = "LCL";
			ChargeGLPostingOverride.Y1_JobType = "FCN";

			Assert("Y1_ConsolContainerMode should not be readonly", !ChargeGLPostingOverride.Y1_ConsolContainerModeInfo.ReadOnly);
			AssertEquals("LCL", ChargeGLPostingOverride.Y1_ConsolContainerMode);
		}

		public void TestY1_MasterPaymentType()
		{
			ChargeGLPostingOverride.Y1_JobType = "SHP";
			ChargeGLPostingOverride.Y1_MasterPaymentType = "PPD";
			ChargeGLPostingOverride.Y1_JobType = "WKI";

			Assert("Y1_MasterPaymentType should be readonly", ChargeGLPostingOverride.Y1_MasterPaymentTypeInfo.ReadOnly);
			AssertEquals("ALL", ChargeGLPostingOverride.Y1_MasterPaymentType);

			ChargeGLPostingOverride.Y1_MasterPaymentType = "PPD";
			ChargeGLPostingOverride.Y1_JobType = "FCN";

			Assert("Y1_ConsolContainerMode should not be readonly", !ChargeGLPostingOverride.Y1_MasterPaymentTypeInfo.ReadOnly);
			AssertEquals("PPD", ChargeGLPostingOverride.Y1_MasterPaymentType);
		}

		public void TestY1_HousePaymentType()
		{
			ChargeGLPostingOverride.Y1_JobType = "SHP";
			ChargeGLPostingOverride.Y1_HousePaymentType = "PPD";
			ChargeGLPostingOverride.Y1_JobType = "WKI";

			Assert("Y1_HousePaymentType should be readonly", ChargeGLPostingOverride.Y1_HousePaymentTypeInfo.ReadOnly);
			AssertEquals("ALL", ChargeGLPostingOverride.Y1_HousePaymentType);

			ChargeGLPostingOverride.Y1_HousePaymentType = "PPD";
			ChargeGLPostingOverride.Y1_JobType = "SHP";

			Assert("Y1_ConsolContainerMode should not be readonly", !ChargeGLPostingOverride.Y1_HousePaymentTypeInfo.ReadOnly);
			AssertEquals("PPD", ChargeGLPostingOverride.Y1_HousePaymentType);
		}

		void AssertAccChargeGLPostingOverrideFieldsReadOnly(AccChargeGLPostingOverride ovrd, bool expected)
		{
			AssertEquals(expected, ovrd.Y1_AG_REVInfo.ReadOnly);
			AssertEquals(expected, ovrd.Y1_AG_WIPInfo.ReadOnly);
			AssertEquals(expected, ovrd.Y1_AG_CSTInfo.ReadOnly);
			AssertEquals(expected, ovrd.Y1_AG_ACRInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccChargeGLPostingOverride result = Factory.New<AccChargeGLPostingOverride>();
			result.Y1_AC = Factory.New<AccChargeCode>().PK;

			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeGLPostingOverride = chargeCode.GLPostingOverrides.AddNew();

			Factory.Save();
		}
		AccChargeGLPostingOverride ChargeGLPostingOverride;

		#endregion
	}
}
