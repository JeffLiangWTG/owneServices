using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGPlacesRefCusCodeListTest : TestCaseWithFactory
	{
		public void TestIsFTZ()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.ChangiFTZ);
			Assert(place.IsFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.JurongFTZ);
			Assert(place.IsFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.KeppelFTZ);
			Assert(place.IsFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.PasirPanjangFTZ);
			Assert(place.IsFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.SembawangFTZ);
			Assert(place.IsFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.SailingClub);
			Assert(!place.IsFTZ());
		}

		public void TestShouldUseFTZ()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.ContainerWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.MarinaWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.KeppelWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.JurongWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.PasirPanjangWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.NotToUseForReceiptRelease.SembawangWharves);
			Assert(place.ShouldUseFTZ());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.SailingClub);
			Assert(!place.ShouldUseFTZ());
		}

		public void TestIsValidForGUIUse()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.FreeTradeZones.ChangiFTZ);
			Assert(place.IsValidForGUIUse());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.Others);
			Assert(!place.IsValidForGUIUse());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.SailingClub);
			Assert(!place.IsValidForGUIUse());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.ShipYard);
			Assert(!place.IsValidForGUIUse());
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BWCY1", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BWCY1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			Factory.Save();
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "BW1");
			Assert(place.IsValidForGUIUse());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "BWCY1");
			Assert(place.IsValidForGUIUse());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "LW1");
			Assert(place.IsValidForGUIUse());
		}

		public void TestIsLicencedPremise()
		{
			var place = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			Assert(!place.IsLicencedPremise());
			var attribute = place.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(place.IsLicencedPremise());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard;
			Assert(place.IsLicencedPremise());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.LicensedWarehouse;
			Assert(place.IsLicencedPremise());
		}

		public void TestIsBondedWarehouse()
		{
			var place = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			Assert(!place.IsBondedWarehouse());
			var attribute = place.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(place.IsBondedWarehouse());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard;
			Assert(place.IsBondedWarehouse());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.LicensedWarehouse;
			Assert(!place.IsBondedWarehouse());
		}

		public void TestIsShortPayment()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates);
			Assert(place.IsShortPayment());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates);
			Assert(place.IsShortPayment());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentImportGSTDefermentScheme);
			Assert(place.IsShortPayment());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.LicensedWarehouse);
			Assert(!place.IsShortPayment());
		}

		public void TestIsRecoveryPayment()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.RecoveryPayment.RecoveryPaymentNotInvolvingUpdates);
			Assert(place.IsRecoveryPayment());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.LicensedWarehouse);
			Assert(!place.IsRecoveryPayment());
		}

		public void TestIsApprovedImportGSTSuspensionSchemeExemptionCode()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.LicensedWarehouse);
			Assert(!place.IsApprovedImportGSTSuspensionSchemeExemptionCode());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme);
			Assert(place.IsApprovedImportGSTSuspensionSchemeExemptionCode());
		}

		public void TestIsImportGSTDefermentSchemeExemptionCode()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.LicensedWarehouse);
			Assert(!place.IsImportGSTDefermentSchemeExemptionCode());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ImportGSTDefermentScheme);
			Assert(place.IsImportGSTDefermentSchemeExemptionCode());
		}

		public void TestSupplierExemptPlaceOfReceipt()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.RecoveryPayment.RecoveryPaymentNotInvolvingUpdates);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentImportGSTDefermentScheme);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ImportGSTDefermentScheme);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.SupplierExemptLocation.ApprovedImportSuspensionSchemeLocal);
			Assert(place.SupplierExemptPlaceOfReceipt());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.SupplierExemptPlaceOfReceipt());
		}

		public void TestSupplierExemptPlaceOfRelease()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.SupplierExemptLocation.Embassy);
			Assert(place.IsSupplierExemptPlaceOfRelease());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.SupplierExemptLocation.ExemptionOnMotorVehicle);
			Assert(place.IsSupplierExemptPlaceOfRelease());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.IsSupplierExemptPlaceOfRelease());
		}

		public void TestIsBWCY()
		{
			var place = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			Assert(!place.IsBWCY());
			var attribute = place.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.IsBWCY());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard;
			Assert(place.IsBWCY());
		}

		public void TestIsCFW()
		{
			var place = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			Assert(!place.IsCFW());
			var attribute = place.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.IsCFW());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.ContainerFreightWarehouse;
			Assert(place.IsCFW());
		}

		public void TestIsC2Y()
		{
			var place = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			Assert(!place.IsC2Y());
			var attribute = place.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.IsC2Y());
			attribute.ZZE_Value = SGCPlaces.Constants.PremiseType.Class2Yard;
			Assert(place.IsC2Y());
		}

		public void TestIsExemptPlaceCodePresident()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.BondedWarehouse);
			Assert(!place.IsExemptPlaceCodePresident());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ExemptPlaceCodePresident);
			Assert(place.IsExemptPlaceCodePresident());
		}

		public void TestIsNonSystemNonLicencedPlaces()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BWCY1", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BWCY1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "AAAA", SGCPlaces.Constants.PremiseType.Others, "CONSIGNEE PREMISES");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BBBB", SGCPlaces.Constants.PremiseType.SailingClub, "CONSIGNEE PREMISES");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "CCCC", SGCPlaces.Constants.PremiseType.ShipYard, "CONSIGNEE PREMISES");
			Factory.Save();
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "BW1");
			Assert(!place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "BWCY1");
			Assert(!place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "LW1");
			Assert(!place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.Others);
			Assert(place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.SailingClub);
			Assert(place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.PremiseType.ShipYard);
			Assert(place.IsNonSystemNonLicenced());
			Factory.Save();
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "AAAA");
			Assert(place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "BBBB");
			Assert(place.IsNonSystemNonLicenced());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "CCCC");
			Assert(place.IsNonSystemNonLicenced());
		}

		public void TestIsMajorExporterSchemeExemptionCode()
		{
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.MajorExporterScheme);
			Assert(place.IsMajorExporterSchemeExemptionCode());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates);
			Assert(!place.IsMajorExporterSchemeExemptionCode());
		}

		public void TestIsLocationAddressToBePrinted()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			Factory.Save();
			var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme);
			Assert(!place.IsLocationAddressToBePrinted());
			place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, "LW1");
			Assert(place.IsLocationAddressToBePrinted());
		}

		protected override void SetUp()
		{
			base.SetUp();
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
		}
	}
}
