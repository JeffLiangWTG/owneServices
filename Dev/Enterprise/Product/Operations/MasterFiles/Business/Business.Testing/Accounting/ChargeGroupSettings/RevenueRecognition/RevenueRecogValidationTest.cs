namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RevenueRecogValidationTest : RevenueRecognitionValidationTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new RevenueRecognition();
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				return new RevenueRecognitionCollection();
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.RecognitionDateOptionCode = "ALL";
			BizObj.Offset = -5;
			BizObj.OffsetType = "ERT";
			BizObj.BrokerCode = "SSS";

			((RevenueRecognition)BizObj).RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.RecognitionDateOptionCodeInfo);
			AssertNoErrors(BizObj.OffsetInfo);
			AssertHasErrors(BizObj.OffsetTypeInfo);
			AssertHasErrors(BizObj.BrokerCodeInfo);

			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Periods;

			AssertHasErrors(BizObj.OffsetInfo);
			AssertNoErrors(BizObj.OffsetTypeInfo);
		}
	}
}
