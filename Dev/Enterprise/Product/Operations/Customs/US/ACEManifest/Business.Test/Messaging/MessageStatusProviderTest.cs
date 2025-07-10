using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public void TestGetMessageStatusList()
		{
			var codeDescriptionPairList = statusProvider.GetMessageStatusList(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(5, codeDescriptionPairList.Count);
			AssertEquals(MessageStatusCodeList.Descriptions.Sent, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Sent));
			AssertEquals(MessageStatusCodeList.Descriptions.Registered, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Registered));
			AssertEquals(MessageStatusCodeList.Descriptions.Cancel, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Cancel));
			AssertEquals(MessageStatusCodeList.Descriptions.Error, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Error));
			AssertEquals(MessageStatusCodeList.Descriptions.Warning, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Warning));
		}

		public void TestGetArrivalStatusList()
		{
			var codeDescriptionPairList = statusProvider.GetArrivalStatusList(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(3, codeDescriptionPairList.Count);
			AssertEquals(MessageStatusCodeList.Descriptions.Sent, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Sent));
			AssertEquals(MessageStatusCodeList.Descriptions.Registered, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Registered));
			AssertEquals(MessageStatusCodeList.Descriptions.Error, codeDescriptionPairList.GetDescriptionFromCode(MessageStatusCodeList.Codes.Error));
		}

		public void TestGetRegistrationStatusListCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z2", "Z2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z3", "Z3 DESC", startDate, endDate);
			Factory.Save();

			var codeDescriptionPairList = statusProvider.GetRegistrationStatusListCore(Factory, Core.Constants.CountryCodes.UnitedStates);

			AssertEquals(3, codeDescriptionPairList.Count);
			AssertEquals(code1.ZZD_Description, codeDescriptionPairList.GetDescriptionFromCode(code1.ZZD_Code));
			AssertEquals(code2.ZZD_Description, codeDescriptionPairList.GetDescriptionFromCode(code2.ZZD_Code));
			AssertEquals(code3.ZZD_Description, codeDescriptionPairList.GetDescriptionFromCode(code3.ZZD_Code));
		}

		public override void TestAllowCancellationMessage()
		{
			Assert("ACE does not currently support cancellation", !statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert("ACE does not currently support cancellation", !statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			Assert("ACE does not currently support modification", !statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			Assert("ACE currently always allow modification", statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			Assert("ACE does not currently support customs acceptance", !statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert("Not submitted if MessageStatus == ''", !statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			Assert("Has been submitted if MessageStatus == 'SNT'", statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			Assert("Not submitted if MessageStatus == 'ERR'", !statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert("ACE does not currently support message status reset", !statusProvider.MessageStatusCanBeReset(header));
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
		{
			return statusProvider;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			statusProvider = header.MessageStatusProvider;
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
	}
}
