using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgSupplierBuyerLinkToleranceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOLT_TransportMode()
		{
			var tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();

			tolerance.OLT_TransportMode = ZString.Empty;
			AssertHasError("Transport mode must be entered, and not empty.", tolerance.OLT_TransportModeInfo, "Please enter a Transport Mode.");

			var allowedTransportModes = new string[]
			{
				Constants.TransportModes.All,
				Constants.TransportModes.Air,
				Constants.TransportModes.Sea,
				Constants.TransportModes.Road,
				Constants.TransportModes.Rail
			};
			foreach (var allowedTransportMode in allowedTransportModes)
			{
				tolerance.OLT_TransportMode = allowedTransportMode;
				AssertNoErrors($"Transport mode {allowedTransportMode} should be allowed.", tolerance.OLT_TransportModeInfo);
			}

			tolerance.OLT_TransportMode = Constants.TransportModes.SeaAir;
			AssertHasError(tolerance.OLT_TransportModeInfo, "Enter a valid transport mode.");
		}

		public void TestCheckOLT_PartNumber()
		{
			var currentLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			var otherLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();

			var currentPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			currentPart.OP_PartNum = "PartNum";
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
			currentPart.OP_IsActive = true;

			var otherPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			otherPart.OP_PartNum = "OtherPartNum";
			otherPart.RelatedOrganisations.AddOrganisationIfNotExist(otherLink.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			otherPart.RelatedOrganisations.AddOrganisationIfNotExist(otherLink.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
			otherPart.OP_IsActive = true;

			var tolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			tolerance.OLT_OL_SupplierBuyerLink = currentLink.PK;
			tolerance.OLT_TransportMode = Constants.TransportModes.All;
			tolerance.OLT_PartNumber = ZString.Empty;

			Factory.Save();

			tolerance.OLT_PartNumber = ZString.Empty;
			AssertNoErrors("Empty part number indicates any part.", tolerance.OLT_PartNumberInfo);

			tolerance.OLT_PartNumber = currentPart.OP_PartNum;
			AssertNoErrors("An existing part number that is related to the supplier or buyer should be allowed.", tolerance.OLT_PartNumberInfo);

			tolerance.OLT_PartNumber = otherPart.OP_PartNum;
			AssertHasError("An existing part number that is not related to the supplier or buyer should not be allowed.", tolerance.OLT_PartNumberInfo, "You have not entered an existing product number that is related to the supplier or buyer.");

			tolerance.OLT_PartNumber = "UnknownPartNum";
			AssertHasError("A non-existing part number should not be allowed.", tolerance.OLT_PartNumberInfo, "You have not entered an existing product number that is related to the supplier or buyer.");
		}

		public void TestCheckOrderLineTolerancesAreUnique()
		{
			var currentLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			var otherLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();

			var currentPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			currentPart.OP_PartNum = "PartNum";
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
			currentPart.OP_IsActive = true;

			var currentTolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			currentTolerance.OLT_OL_SupplierBuyerLink = currentLink.PK;
			currentTolerance.OLT_TransportMode = Constants.TransportModes.Air;
			currentTolerance.OLT_PartNumber = currentPart.OP_PartNum;

			Factory.Save();

			var tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();
			tolerance.OLT_OL_SupplierBuyerLink = currentLink.PK;

			tolerance.OLT_TransportMode = currentTolerance.OLT_TransportMode;
			AssertNotEquals(currentTolerance.OLT_PartNumber, tolerance.OLT_PartNumber);
			AssertNoRowErrors("Should have no errors, as PartNumbers are different.", tolerance);

			tolerance.OLT_PartNumber = currentPart.OP_PartNum;
			AssertHasRowError("Transport Mode/Part Number should be unique per SupplierBuyerLink.", tolerance, "An Order Line Tolerance with Transport Mode / Part Number already exists for this supplier-buyer relationship.");

			tolerance.OLT_TransportMode = Constants.TransportModes.Sea;
			AssertNotEquals(currentTolerance.OLT_TransportMode, tolerance.OLT_TransportMode);
			AssertNoRowErrors("Should have no errors, as TransportModes are different.", tolerance);

			tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();
			tolerance.OLT_OL_SupplierBuyerLink = otherLink.PK;

			tolerance.OLT_TransportMode = currentTolerance.OLT_TransportMode;
			AssertNoRowErrors("Should have no errors, as SupplierBuyerLinks are different.", tolerance);

			tolerance.OLT_PartNumber = currentPart.OP_PartNum;
			AssertNoRowErrors("Should have no errors, as SupplierBuyerLinks are different.", tolerance);
		}

		public void TestCheckOLT_UnderQuantityPercentageLimit()
		{
			var tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();

			tolerance.OLT_UnderQuantityPercentageLimit = -1;
			AssertHasError("Allowable under quantity must be greater than or equal to 0.", tolerance.OLT_UnderQuantityPercentageLimitInfo, "Must be between 0 and 99.999.");

			tolerance.OLT_UnderQuantityPercentageLimit = 100;
			AssertHasError("Allowable under quantity must be less than or equal to 99.999.", tolerance.OLT_UnderQuantityPercentageLimitInfo, "Must be between 0 and 99.999.");

			tolerance.OLT_UnderQuantityPercentageLimit = 0;
			AssertNoErrors($"Should have no errors when allowable under quantity is between 0 and 99.999.", tolerance.OLT_UnderQuantityPercentageLimitInfo);

			tolerance.OLT_UnderQuantityPercentageLimit = 99.999m;
			AssertNoErrors($"Should have no errors when allowable under quantity is between 0 and 99.999.", tolerance.OLT_UnderQuantityPercentageLimitInfo);
		}

		public void TestCheckOLT_OverQuantityPercentageLimit()
		{
			var tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();

			tolerance.OLT_OverQuantityPercentageLimit = -1;
			AssertHasError("Allowable over quantity must be greater than or equal to 0.", tolerance.OLT_OverQuantityPercentageLimitInfo, "Must be between 0 and 999.999.");

			tolerance.OLT_OverQuantityPercentageLimit = 1000;
			AssertHasError("Allowable over quantity must be less than or equal to 999.999.", tolerance.OLT_OverQuantityPercentageLimitInfo, "Must be between 0 and 999.999.");

			tolerance.OLT_OverQuantityPercentageLimit = 0;
			AssertNoErrors($"Should have no errors when allowable over quantity is between 0 and 999.999.", tolerance.OLT_OverQuantityPercentageLimitInfo);

			tolerance.OLT_OverQuantityPercentageLimit = 999.999m;
			AssertNoErrors($"Should have no errors when allowable over quantity is between 0 and 999.999.", tolerance.OLT_OverQuantityPercentageLimitInfo);
		}
	}
}
