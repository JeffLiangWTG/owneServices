using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RoyaltyRetrieverTest : TestCaseWithFactory
	{
		public void TestGetsHighestRoyaltyFromMatchingRelatedOrganisations()
		{
			OrgHeader orgSupplier1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgSupplier2 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgOwner2 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();
			OrgHeader orgBoth1 = GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor();

			OrgSupplierBuyerLink supplier1ToOwner1 = GetNewSupplierLinkAndAddToOrganisation(orgOwner1, orgSupplier1, 10m);
			OrgSupplierBuyerLink supplier1ToOwner2 = GetNewSupplierLinkAndAddToOrganisation(orgOwner2, orgSupplier1, 20m);

			OrgPartRelation relationOwner1 = GetNewRelationToPart(orgOwner1, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationSupplier1 = GetNewRelationToPart(orgSupplier1, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier1)", true, RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier1));
			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 20m, RoyaltyRetriever.RoyaltyPercentage);

			Part.RelatedOrganisations.RemoveAndDeleteAll();

			OrgSupplierBuyerLink supplier2ToBoth1 = GetNewSupplierLinkAndAddToOrganisation(orgBoth1, orgSupplier2, 30m);
			OrgSupplierBuyerLink supplier2ToOwner2 = GetNewSupplierLinkAndAddToOrganisation(orgOwner2, orgSupplier2, 15m);

			OrgPartRelation relationBoth1 = GetNewRelationToPart(orgBoth1, OrgPartRelation.RelationshipTypes.Both);
			relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationSupplier2 = GetNewRelationToPart(orgSupplier2, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2)", true, RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2));
			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 30m, RoyaltyRetriever.RoyaltyPercentage);

			Part.RelatedOrganisations.RemoveAndDeleteAll();

			supplier2ToBoth1 = GetNewBuyerLinkAndAddToOrganisation(orgBoth1, orgSupplier2, 45m);
			supplier2ToOwner2 = GetNewBuyerLinkAndAddToOrganisation(orgOwner2, orgSupplier2, 17m);

			relationBoth1 = GetNewRelationToPart(orgBoth1, OrgPartRelation.RelationshipTypes.Both);
			relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			relationOwner1 = GetNewRelationToPart(orgOwner1, OrgPartRelation.RelationshipTypes.Owner);
			relationSupplier2 = GetNewRelationToPart(orgSupplier2, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2)", true, RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2));
			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 45m, RoyaltyRetriever.RoyaltyPercentage);

			Part.RelatedOrganisations.RemoveAndDeleteAll();

			supplier2ToBoth1 = GetNewBuyerLinkAndAddToOrganisation(orgBoth1, orgSupplier2, 45m);
			OrgSupplierBuyerLink supplier2ToOwner1 = GetNewSupplierLinkAndAddToOrganisation(orgOwner1, orgSupplier2, 57.6m);
			supplier2ToOwner2 = GetNewBuyerLinkAndAddToOrganisation(orgOwner2, orgSupplier2, 17m);

			relationBoth1 = GetNewRelationToPart(orgBoth1, OrgPartRelation.RelationshipTypes.Both);
			relationOwner2 = GetNewRelationToPart(orgOwner2, OrgPartRelation.RelationshipTypes.Owner);
			relationOwner1 = GetNewRelationToPart(orgOwner1, OrgPartRelation.RelationshipTypes.Owner);
			relationSupplier2 = GetNewRelationToPart(orgSupplier2, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2)", true, RoyaltyRetriever.FindRoyalty(Part.RelatedOrganisations, orgSupplier2));
			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 57.6m, RoyaltyRetriever.RoyaltyPercentage);

			Part.RelatedOrganisations.RemoveAndDeleteAll();
		}

		public void TestOtherImportersAndSuppliersDontAffectTheDefaultingOfRoyalties()
		{
			SetUpOrganisationsAsConsigneesAndConsignors();

			OrgHeader orgBothOther = Factory.NewWithValidTestData<OrgHeader>();
			orgBothOther.OH_IsConsignee = true;
			orgBothOther.OH_IsConsignor = true;

			OrgPartRelation relationBothOther = Part.RelatedOrganisations.AddNew();
			relationBothOther.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBothOther.OU_OH = orgBothOther.PK;

			OrgPartRelation relationBoth = GetNewBothRelationsToPart();

			OrgHeader orgBothBunkus = Factory.NewWithValidTestData<OrgHeader>();
			orgBothBunkus.OH_IsConsignee = true;
			orgBothBunkus.OH_IsConsignor = true;

			OrgPartRelation relationBothBunkus = Part.RelatedOrganisations.AddNew();
			relationBothBunkus.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationBothBunkus.OU_OH = orgBothBunkus.PK;

			relationBothOther.OU_RoyaltyPercent = 10m;
			relationBoth.OU_RoyaltyPercent = 5m;
			relationBothBunkus.OU_RoyaltyPercent = 15m;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgBoth, OrgOwner)", true, RoyaltyRetriever.FindRoyalty(Part, OrgBoth, OrgOwner));

			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 5m, RoyaltyRetriever.RoyaltyPercentage);
		}

		public void TestWhenRoyaltyPercentageAndFlatAmountAreSetBothAreAccessible()
		{
			SetUpOrganisationsAsConsigneesAndConsignors();

			OrgPartRelation relationBoth = GetNewBothRelationsToPart();

			relationBoth.OU_RoyaltyPercent = 5m;
			relationBoth.OU_RoyaltyFlatAmount = 35m;
			relationBoth.OU_RX_NKRoyaltyCurrency = Currency.RX_Code;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgBoth)", true, RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgBoth));

			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 5m, RoyaltyRetriever.RoyaltyPercentage);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmount", 35m, RoyaltyRetriever.RoyaltyFlatAmount);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmountCurrency", "ZZZ", RoyaltyRetriever.RoyaltyFlatAmountCurrency);

			relationBoth.OU_RoyaltyPercent = 2.3m;
			relationBoth.OU_RoyaltyFlatAmount = 87m;
			relationBoth.OU_RX_NKRoyaltyCurrency = Currency.RX_Code;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgBoth, OrgOwner)", true, RoyaltyRetriever.FindRoyalty(Part, OrgBoth, OrgOwner));

			AssertEquals("RoyaltyRetriever.RoyaltyPercentage", 2.3m, RoyaltyRetriever.RoyaltyPercentage);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmount", 87m, RoyaltyRetriever.RoyaltyFlatAmount);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmountCurrency", "ZZZ", RoyaltyRetriever.RoyaltyFlatAmountCurrency);
		}

		public void TestProperlySetsRoyaltiesFromBothToOwnerToSupplier()
		{
			SetUpOrganisationsAsConsigneesAndConsignors();

			OrgPartRelation relationSupplier = GetNewSupplierRelationsToPart();

			relationSupplier.OU_RoyaltyFlatAmount = 35m;
			relationSupplier.OU_RX_NKRoyaltyCurrency = Currency.RX_Code;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgOwner)", true, RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgOwner));

			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmount", 35m, RoyaltyRetriever.RoyaltyFlatAmount);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmountCurrency", "ZZZ", RoyaltyRetriever.RoyaltyFlatAmountCurrency);

			OrgPartRelation relationOwner = GetNewOwnerRelationsToPart();

			relationOwner.OU_RoyaltyFlatAmount = 70m;
			relationOwner.OU_RX_NKRoyaltyCurrency = Currency.RX_Code;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgOwner)", true, RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgOwner));

			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmount", 70m, RoyaltyRetriever.RoyaltyFlatAmount);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmountCurrency", "ZZZ", RoyaltyRetriever.RoyaltyFlatAmountCurrency);

			OrgPartRelation relationBoth = GetNewBothRelationsToPart();

			relationBoth.OU_RoyaltyFlatAmount = 105m;
			relationBoth.OU_RX_NKRoyaltyCurrency = Currency.RX_Code;

			AssertEquals("Precondition: RoyaltyRetriever.FindRoyalty(Part, OrgSupplier, OrgOwner)", true, RoyaltyRetriever.FindRoyalty(Part, OrgBoth, OrgOwner));

			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmount", 105m, RoyaltyRetriever.RoyaltyFlatAmount);
			AssertEquals("RoyaltyRetriever.RoyaltyFlatAmountCurrency", "ZZZ", RoyaltyRetriever.RoyaltyFlatAmountCurrency);
		}

		#region Implementation
		OrgSupplierPart Part
		{
			get { return fPart ?? (fPart = Factory.New<OrgSupplierPart>()); }
		}
		OrgSupplierPart fPart;

		OrgHeader OrgBoth
		{
			get { return fOrgBoth ?? (fOrgBoth = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fOrgBoth;

		OrgHeader OrgOwner
		{
			get { return fOrgOwner ?? (fOrgOwner = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fOrgOwner;

		OrgHeader OrgSupplier
		{
			get { return fOrgSupplier ?? (fOrgSupplier = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fOrgSupplier;

		RefCurrency Currency
		{
			get { return fCurrency ?? (fCurrency = GetNewCurrencyWithCode()); }
		}
		RefCurrency fCurrency;
		RefCurrency GetNewCurrencyWithCode()
		{
			RefCurrency result = Factory.NewWithValidTestData<RefCurrency>();
			result.RX_Code = "ZZZ";
			return result;
		}

		RoyaltyRetriever RoyaltyRetriever
		{
			get { return fRoyaltyRetriever ?? (fRoyaltyRetriever = new RoyaltyRetriever()); }
		}
		RoyaltyRetriever fRoyaltyRetriever;

		void SetUpOrganisationsAsConsigneesAndConsignors()
		{
			OrgBoth.OH_IsConsignee = true;
			OrgBoth.OH_IsConsignor = true;
			OrgOwner.OH_IsConsignee = true;
			OrgOwner.OH_IsConsignor = true;
			OrgSupplier.OH_IsConsignee = true;
			OrgSupplier.OH_IsConsignor = true;
		}

		OrgPartRelation GetNewOwnerRelationsToPart()
		{
			OrgPartRelation result = Part.RelatedOrganisations.AddNew();
			result.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			result.OU_OH = OrgOwner.PK;
			return result;
		}

		OrgPartRelation GetNewSupplierRelationsToPart()
		{
			OrgPartRelation result = Part.RelatedOrganisations.AddNew();
			result.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			result.OU_OH = OrgSupplier.PK;
			return result;
		}

		OrgPartRelation GetNewBothRelationsToPart()
		{
			OrgPartRelation result = Part.RelatedOrganisations.AddNew();
			result.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			result.OU_OH = OrgBoth.PK;
			return result;
		}

		OrgHeader GetNewWithValidTestDataOrgHeaderAsConsigneeAndConsignor()
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsConsignee = true;
			result.OH_IsConsignor = true;
			return result;
		}

		OrgSupplierBuyerLink GetNewBuyerLinkAndAddToOrganisation(OrgHeader organisation, OrgHeader organisationToLinkTo, ZDecimal percentage)
		{
			OrgSupplierBuyerLink result = organisation.BuyerLinks.AddNew();
			result.OL_OH_Buyer = organisationToLinkTo.PK;
			result.OL_RoyaltyPercentage = percentage;
			return result;
		}

		OrgSupplierBuyerLink GetNewSupplierLinkAndAddToOrganisation(OrgHeader organisation, OrgHeader organisationToLinkTo, ZDecimal percentage)
		{
			OrgSupplierBuyerLink result = organisation.SupplierLinks.AddNew();
			result.OL_OH_Supplier = organisationToLinkTo.PK;
			result.OL_RoyaltyPercentage = percentage;
			return result;
		}

		OrgPartRelation GetNewRelationToPart(OrgHeader organisation, ZString relationType)
		{
			OrgPartRelation result = Part.RelatedOrganisations.AddNew();
			result.OU_OH = organisation.PK;
			result.OU_Relationship = relationType;
			return result;
		}
		#endregion
	}
}
