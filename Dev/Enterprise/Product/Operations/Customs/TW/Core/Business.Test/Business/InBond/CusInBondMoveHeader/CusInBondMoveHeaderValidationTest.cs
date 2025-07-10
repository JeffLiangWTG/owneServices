using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondMoveHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_InBondEntryType()
		{
			CusInBondMoveHeader.BM_InBondEntryType = ZString.Empty;
			AssertHasMessageErrorContaining(CusInBondMoveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			CusInBondMoveHeader.BM_InBondEntryType = EntryTypeList.Codes.T1;
			AssertNoMessageErrorContaining(CusInBondMoveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			CusInBondMoveHeader.BM_InBondEntryType = "T8";
			AssertHasMessageError(CusInBondMoveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_InBondEntryType = EntryTypeList.Codes.T6;
			AssertNoMessageError(CusInBondMoveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_ExportTransportMode()
		{
			CusInBondMoveHeader.BM_ExportTransportMode = "66";
			AssertHasMessageError(CusInBondMoveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_ExportTransportMode = TranshipmentTransportCodeList.Codes.AirPassengerOrCREW;
			AssertNoMessageError(CusInBondMoveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_PlaceOfLoading()
		{
			CusInBondMoveHeader.BM_PlaceOfLoading = "XXXXX";
			AssertHasMessageError(CusInBondMoveHeader.BM_PlaceOfLoadingInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_PlaceOfLoading = "AG222";
			AssertNoMessageError(CusInBondMoveHeader.BM_PlaceOfLoadingInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(CusInBondMoveHeader.BM_PlaceOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
			CusInBondMoveHeader.BM_PlaceOfLoading = ZString.Empty;
			AssertHasMessageErrorContaining(CusInBondMoveHeader.BM_PlaceOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_ForeignDestPortKCode()
		{
			var targetInfo = CusInBondMoveHeader.BM_ForeignDestPortKCodeInfo;
			var errorMessageDestinationAndDestinationUnBothNotEmpty = ValidationConstants.CusInBondMoveHeader.DestinationAndDestinationUnBothNotEmpty;
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "XXXXX";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "003A1090";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "003A1090";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "CNSHA";
			CusInBondMoveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			CusInBondMoveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "003A1090";
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "CNSHA";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
		}

		public void TestCheckBM_RL_NKForeignDestPort()
		{
			var targetInfo = CusInBondMoveHeader.BM_RL_NKForeignDestPortInfo;
			var errorMessageDestinationAndDestinationUnBothNotEmpty = ValidationConstants.CusInBondMoveHeader.DestinationAndDestinationUnBothNotEmpty;
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "XXXXX";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "CNSHA";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "003A1090";
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "CNSHA";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
			CusInBondMoveHeader.BM_RL_NKForeignDestPort = "CNSHA";
			CusInBondMoveHeader.BM_ForeignDestPortKCode = "003A1090";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(targetInfo, errorMessageDestinationAndDestinationUnBothNotEmpty);
		}

		public void TestCheckBM_TransportAtDeparture()
		{
			var targetInfo = CusInBondMoveHeader.BM_TransportAtDepartureInfo;
			CusInBondMoveHeader.BM_TransportAtDeparture = "XXXXX";
			AssertHasMessageError(targetInfo, ValidationConstants.CusInBondMoveHeader.TW_ExportVesselREGLength);
			CusInBondMoveHeader.BM_TransportAtDeparture = "ANADYR";
			AssertNoMessageError(targetInfo, ValidationConstants.CusInBondMoveHeader.TW_ExportVesselREGLength);
			CusInBondMoveHeader.BM_TransportAtDeparture = "!@1111";
			AssertHasMessageError(targetInfo, ValidationConstants.CusInBondMoveHeader.OnlyLettersAndNumbersForExportVesselREG);
			CusInBondMoveHeader.BM_TransportAtDeparture = "\u5176\u4ed6\u904b\u8f38\u65b9\u5f0f";
			AssertHasError(targetInfo, EnglishCharactersValidation.GetNotificationMessage(targetInfo));
		}

		CusInBondMoveHeader CusInBondMoveHeader => fCusInBondMoveHeader ?? (fCusInBondMoveHeader = CusInBondHeader.MovementHeaders.AddNew());
		CusInBondMoveHeader fCusInBondMoveHeader;
		CusInBondHeader CusInBondHeader => fCusInBondHeader ?? (fCusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>());
		CusInBondHeader fCusInBondHeader;
		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AG222", "AG222 XXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "003A1090", "003A1090 XXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
