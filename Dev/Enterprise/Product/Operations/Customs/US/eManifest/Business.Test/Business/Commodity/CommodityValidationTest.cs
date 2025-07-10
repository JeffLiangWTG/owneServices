using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CommodityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_Description()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_DescriptionInfo);
		}

		public void TestCheckHarmonizedNumbers()
		{
			const string messageError = "Harmonized Numbers are required for Pre-filed In-bond.";
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			var inBond = shipment.InBond;
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			commodity.BY_Description = "Desc";
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("No child collections should be validated if validate all has not been run", commodity.BY_DescriptionInfo, messageError);
			commodity.Validation.ValidateAll();
			AssertHasMessageError("Harmonized Numbers validation when in-bond exportation, and validate all has been run", commodity.BY_DescriptionInfo, messageError);
			inBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("Harmonized Numbers validation when in-bond transportation", commodity.BY_DescriptionInfo, messageError);
			inBond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			commodity.Validation.ValidateBY_Description();
			AssertHasMessageError("Harmonized Numbers validation when in-bond transportation and exportation", commodity.BY_DescriptionInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("Harmonized Numbers validation when NOT in-bond", commodity.BY_DescriptionInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			commodity.HarmonizedNumbers.AddNew();
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("Harmonized Numbers entered", commodity.BY_DescriptionInfo, messageError);
		}

		public void TestCheckC4Codes()
		{
			const string messageError = "C4 Codes are required for Border Release Advance Selectivity Subsystem (BRASS).";
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			commodity.BY_Description = "Desc";
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("No child collections should be validated if validate all has not been run", commodity.BY_DescriptionInfo, messageError);
			commodity.Validation.ValidateAll();
			AssertHasMessageError("C4 Codes validation when BRASS, and validate all has been run", commodity.BY_DescriptionInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("C4 Codes validation when NOT BRASS", commodity.BY_DescriptionInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			commodity.C4Codes.AddNew();
			commodity.Validation.ValidateBY_Description();
			AssertNoMessageError("C4 Codes entered", commodity.BY_DescriptionInfo, messageError);
		}

		public void TestCheckBY_BJ_Equipment()
		{
			commodity.Validation.ValidateAll();
			AssertHasMessageErrorContaining(commodity.BY_BJ_EquipmentInfo, MandatoryValidation.YouHaveNotEntered);
			ValidationTestHelper.AssertErrorIfInvalidPK(commodity.BY_BJ_EquipmentInfo, ZGuid.NewZGuid(), trip.Conveyance.PK);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_BJ_EquipmentInfo);
		}

		public void TestCheckBY_HarmonizedNumbers()
		{
			const string messageError = "Some of the harmonized numbers are invalid. Please check Harmonized Numbers tab for more details.";
			var validCode = Factory.LoadTop1<USCTariff>(new USCTariffCollection(Factory).CompleteFilter).UE_Tariff;
			commodity.BY_HarmonizedNumbers = "8001, " + validCode;
			AssertHasMessageError(commodity.BY_HarmonizedNumbersInfo, messageError);
			commodity.BY_HarmonizedNumbers = validCode + ", " + validCode;
			AssertNoMessageError(commodity.BY_HarmonizedNumbersInfo, messageError);
		}

		public void TestCheckBY_HazardousGoodsIdentifier()
		{
			var undg = commodity.UNDGs.AddNew();
			commodity.Validation.ValidateBY_HazardousGoodsIdentifier();
			commodity.BY_HazardousGoodsIdentifier = Factory.LoadTop1<UNDGSubstance>(new ZQuery()).PK;
			AssertNoErrors(undg.DI_DGInfo);
			AssertNoErrors(commodity.BY_HazardousGoodsIdentifierInfo);
			commodity.BY_HazardousGoodsIdentifier = ZGuid.Empty;
			commodity.UNDGs.AddNew();
			AssertNoNotifications(commodity.BY_HazardousGoodsIdentifierInfo);
			commodity.BY_HazardousGoodsIdentifier = ZGuid.Invalid;
			AssertHasNotifications(commodity.BY_HazardousGoodsIdentifierInfo);
			commodity.UNDGs.DeleteAll();
			AssertNoErrors(commodity.BY_HazardousGoodsIdentifierInfo);
		}

		public void TestCheckBY_HazardousGoodsContact()
		{
			var undg = commodity.UNDGs.AddNew();
			commodity.Validation.ValidateBY_HazardousGoodsContact();
			const string youHaveNotEntered = MandatoryValidation.YouHaveNotEntered + " a UNDG Contact.";
			AssertHasMessageError(undg.DI_OC_DGContactInfo, youHaveNotEntered);
			AssertHasMessageError(commodity.BY_HazardousGoodsContactInfo, youHaveNotEntered);
			const string messageError = @"UNDG Contact is invalid and has following errors:
You have not entered a Contact Name;
You have not entered a Work Phone.";
			var contact = Factory.New<OrgContact>();
			commodity.BY_HazardousGoodsContact = contact.PK;
			AssertNoMessageError(undg.DI_OC_DGContactInfo, youHaveNotEntered);
			AssertNoMessageError(commodity.BY_HazardousGoodsContactInfo, youHaveNotEntered);
			AssertHasMessageError("UNDG: Name and phone are mandatory", undg.DI_OC_DGContactInfo, messageError);
			AssertHasMessageError("Commodity: Name and phone are mandatory", commodity.BY_HazardousGoodsContactInfo, messageError);
			contact.OC_ContactName = "Contact Name";
			contact.OC_Phone = "132165489798";
			commodity.Validation.ValidateBY_HazardousGoodsContact();
			AssertNoMessageError("UNDG: Name and phone entered", undg.DI_OC_DGContactInfo, messageError);
			AssertNoMessageError("Commodity: Name and phone entered", commodity.BY_HazardousGoodsContactInfo, messageError);
			contact.OC_ContactName = ZString.Empty;
			contact.OC_Phone = ZString.Empty;
			commodity.Validation.ValidateBY_HazardousGoodsContact();
			undg = commodity.UNDGs.AddNew();
			undg.DI_OC_DGContact = contact.PK;
			AssertHasMessageError("UNDG: Name and phone are mandatory when second row added", undg.DI_OC_DGContactInfo, messageError);
			AssertNoMessageError("Commodity: No notifications if more than one row", commodity.BY_HazardousGoodsContactInfo, messageError);
			commodity.UNDGs.RemoveFromRelationship(undg);
			AssertHasMessageError("Commodity: Name and phone are mandatory when single UNDG again", commodity.BY_HazardousGoodsContactInfo, messageError);
			commodity.UNDGs.DeleteAll();
			AssertNoMessageError("Commodity: No notifications if no rows", commodity.BY_HazardousGoodsContactInfo, messageError);
		}

		public void TestCheckBY_ManifestUnitCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(commodity.BY_ManifestUnitCodeInfo, "??", PackageTypes.Codes.Pieces);
			commodity.BY_PieceCount = 2;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(commodity.BY_ManifestUnitCodeInfo, "??", PackageTypes.Codes.Pieces);
		}

		public void TestCheckBY_MonetaryValue()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(commodity.BY_MonetaryValueInfo);
			var messageError = "Customs Value is required for Low Value Entries Informal, Pre-filed In-bond and Goods Astray.";
			ValidationTestHelper.AssertFieldIsNotMandatory(commodity.BY_MonetaryValueInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_MonetaryValueInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.GoodsAstray;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_MonetaryValueInfo, messageError);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_MonetaryValueInfo, messageError);
			messageError = "Customs value may not exceed $2500 for a Low Value Entries.";
			commodity.BY_MonetaryValue = 2501;
			AssertHasMessageError(commodity.BY_MonetaryValueInfo, messageError);
			commodity.BY_MonetaryValue = 2400;
			AssertNoMessageError(commodity.BY_MonetaryValueInfo, messageError);
		}

		public void TestCheckBY_PieceCount()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(commodity.BY_PieceCountInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_PieceCountInfo);
		}

		public void TestCheckBY_RN_NKCountryOfOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(commodity.BY_RN_NKCountryOfOriginInfo, "??", Constants.CountryCodes.UnitedStates);
			const string messageError = "Country Of Origin is required for Low Value Entries Informal.";
			shipment.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_RN_NKCountryOfOriginInfo, messageError);
			ValidationTestHelper.AssertInvalidCodeMessageError(commodity.BY_RN_NKCountryOfOriginInfo, "??", Constants.CountryCodes.UnitedStates);
		}

		public void TestCheckBY_GrossWeight_Negative()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(commodity.BY_GrossWeightInfo);
		}

		public void TestCheckBY_GrossWeight_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_GrossWeightInfo);
		}

		public void TestCheckBY_GrossWeightUnit_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(commodity.BY_GrossWeightUnitInfo, "??", Constants.Weight.Kilograms);
		}

		public void TestCheckBY_GrossWeightUnit_Mandatory()
		{
			commodity.BY_GrossWeight = 2;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(commodity.BY_GrossWeightUnitInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = Factory.New<Trip>();
			shipment = trip.Shipments.AddNew();
			commodity = shipment.Commodities.AddNew();
		}

		Commodity commodity;
		Shipment shipment;
		Trip trip;
	}
}
