using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeRevRecOverValidationTest : RevenueRecognitionValidationTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				return new AccChargeRevRecOverrideCollection(chargeCode);
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.RecognitionDateOptionCode = "ALL";
			BizObj.BrokerCode = "SSS";

			((BusinessObject)BizObj).RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.RecognitionDateOptionCodeInfo);
			AssertHasErrors(BizObj.BrokerCodeInfo);
		}

		public new void TestValidateOffset()
		{
			Assert("This test in not actual for AccChargeRevRecOverride", true);
		}

		public new void TestValidateOffsetType()
		{
			Assert("This test in not actual for AccChargeRevRecOverride", true);
		}
	}
}
