using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCargoReleaseStatus()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = ZString.Empty;
			AssertHasMessageErrorContaining(bill.CargoReleaseStatusInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CargoReleaseStatus = "XX";
			AssertHasMessageErrorContaining(bill.CargoReleaseStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.CargoReleaseStatus = "1";
			AssertNoNotifications(bill.CargoReleaseStatusInfo);
		}

		public void TestCheckCargoReleaseStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CargoReleaseStatus = AsycudaBill.CargoReleaseStatus_Other;
			bill.CargoReleaseStatusOtherDescription = ZString.Empty;
			AssertHasMessageError(bill.CargoReleaseStatusOtherDescriptionInfo, "Release Status Description Required when Cargo Release Status is 'Other'.");
			bill.CargoReleaseStatusOtherDescription = "Other Desc.";
			AssertNoMessageError(bill.CargoReleaseStatusOtherDescriptionInfo, "Release Status Description Required when Cargo Release Status is 'Other'.");
		}
	}
}
