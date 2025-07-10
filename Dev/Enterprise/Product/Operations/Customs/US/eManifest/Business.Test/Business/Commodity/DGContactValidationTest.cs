using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class DGContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateContact()
		{
			const string youHaveNotEntered = MandatoryValidation.YouHaveNotEntered + " a UNDG Contact.";
			undg.Validation.ValidateDI_OC_DGContact();
			AssertHasMessageError(undg.DI_OC_DGContactInfo, youHaveNotEntered);
			AssertHasMessageError(commodity.BY_HazardousGoodsContactInfo, youHaveNotEntered);
			const string messageError = @"UNDG Contact is invalid and has following errors:
You have not entered a Contact Name;
You have not entered a Work Phone.";
			var contact = Factory.New<OrgContact>();
			undg.DI_OC_DGContact = contact.PK;
			AssertNoMessageError(undg.DI_OC_DGContactInfo, youHaveNotEntered);
			AssertNoMessageError(commodity.BY_HazardousGoodsContactInfo, youHaveNotEntered);
			AssertHasMessageError("UNDG: Name and phone are mandatory", undg.DI_OC_DGContactInfo, messageError);
			AssertHasMessageError("Commodity: Name and phone are mandatory", commodity.BY_HazardousGoodsContactInfo, messageError);
			commodity.UNDGs.RemoveFromRelationship(undg);
			undg.Validation.ValidateDI_OC_DGContact();
			AssertNoMessageError("UNDG: Validation unhooked if removed from the collection", undg.DI_OC_DGContactInfo, messageError);
			AssertNoMessageError("Commodity: Validation unhooked if removed from the collection", commodity.BY_HazardousGoodsContactInfo, messageError);
			commodity.UNDGs.Add(undg);
			undg.Validation.ValidateDI_OC_DGContact();
			AssertHasMessageError("UNDG: Validation hooked if added to the collection", undg.DI_OC_DGContactInfo, messageError);
			AssertHasMessageError("Commodity: Validation hooked if added to the collection", commodity.BY_HazardousGoodsContactInfo, messageError);
			contact.OC_ContactName = "Contact Name";
			contact.OC_Phone = "132165489798";
			undg.Validation.ValidateDI_OC_DGContact();
			AssertNoMessageError("UNDG: Name and phone entered", undg.DI_OC_DGContactInfo, messageError);
			AssertNoMessageError("Commodity: Name and phone entered", commodity.BY_HazardousGoodsContactInfo, messageError);
		}

		public void TestValidateSubs()
		{
			var invalidCodeError = ListValidation.InvalidCodeError + "DG Substance.";
			undg.Validation.ValidateDI_DG();
			undg.DI_DG = ZGuid.Invalid;
			AssertHasError(undg.DI_DGInfo, invalidCodeError);
			AssertHasError(commodity.BY_HazardousGoodsIdentifierInfo, invalidCodeError);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			AssertNoError(undg.DI_DGInfo, invalidCodeError);
			AssertNoError(commodity.BY_HazardousGoodsIdentifierInfo, invalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			commodity = shipment.Commodities.AddNew();
			undg = commodity.UNDGs.AddNew();
		}

		UNDGDataItem undg;
		Commodity commodity;
	}
}
