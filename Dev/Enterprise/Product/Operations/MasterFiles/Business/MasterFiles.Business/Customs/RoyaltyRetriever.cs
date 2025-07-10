using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RoyaltyRetriever
	{
		public ZDecimal RoyaltyPercentage { get { return fRoyaltyPercentage; } }
		ZDecimal fRoyaltyPercentage;

		public ZDecimal RoyaltyFlatAmount { get { return fRoyaltyFlatAmount; } }
		ZDecimal fRoyaltyFlatAmount;

		public ZString RoyaltyFlatAmountCurrency { get { return fRoyaltyFlatAmountCurrency; } }
		ZString fRoyaltyFlatAmountCurrency;

		public ZBool FindRoyalty(OrgPartRelationCollection relatedOrganisations, OrgHeader currentOrg)
		{
			ZDecimal highestSupplierPercentage = ZDecimal.Zero;
			ZDecimal highestOwnerPercentage = ZDecimal.Zero;

			foreach (OrgPartRelation orgRelation in relatedOrganisations)
			{
				if (orgRelation.Organisation != null && orgRelation.Organisation.PK.IsValid && orgRelation.Organisation.PK != currentOrg.PK)
				{
					if (orgRelation.OU_Relationship == OrgPartRelation.RelationshipTypes.Supplier || orgRelation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					{
						highestSupplierPercentage = GetHighestRoyaltyPercentage(highestSupplierPercentage, orgRelation.Organisation.BuyerLinks, currentOrg.PK);
					}

					if (orgRelation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || orgRelation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					{
						highestOwnerPercentage = GetHighestRoyaltyPercentage(highestOwnerPercentage, orgRelation.Organisation.SupplierLinks, currentOrg.PK);
					}
				}
			}

			return SetResultReturningHasRoyalty(Math.Max(highestSupplierPercentage, highestOwnerPercentage), ZDecimal.Zero, ZString.Empty);
		}

		ZDecimal GetHighestRoyaltyPercentage(ZDecimal currentHighest, OrgSupplierBuyerLinkDependentCollection orgLinkCollection, ZGuid organisation)
		{
			ZDecimal result = currentHighest;
			ZGuid linkOrganisation = ZGuid.Empty;

			foreach (OrgSupplierBuyerLink link in orgLinkCollection)
			{
				if (orgLinkCollection.GetType() == typeof(OrgSupplierLinkCollection))
				{
					linkOrganisation = link.OL_OH_Supplier;
				}
				else if (orgLinkCollection.GetType() == typeof(OrgBuyerLinkCollection))
				{
					linkOrganisation = link.OL_OH_Buyer;
				}

				if (linkOrganisation == organisation && link.OL_RoyaltyPercentage > result)
				{
					result = link.OL_RoyaltyPercentage;
				}
			}
			return result;
		}

		public ZBool FindRoyalty(OrgSupplierPart part, OrgHeader supplier, OrgHeader importer)
		{
			ZBool result = false;

			OrgPartRelation bestMatch = FindOrganisationWithRelationship(OrgPartRelation.RelationshipTypes.Both, part, supplier, importer)
				?? FindOrganisationWithRelationship(OrgPartRelation.RelationshipTypes.Owner, part, supplier, importer)
				?? FindOrganisationWithRelationship(OrgPartRelation.RelationshipTypes.Supplier, part, supplier, importer);

			if (bestMatch != null)
			{
				result = SetResultReturningHasRoyalty(bestMatch.OU_RoyaltyPercent, bestMatch.OU_RoyaltyFlatAmount, bestMatch.OU_RX_NKRoyaltyCurrency);
			}

			return result;
		}

		ZBool SetResultReturningHasRoyalty(ZDecimal percentage, ZDecimal flatAmount, ZString currency)
		{
			fRoyaltyPercentage = percentage;
			bool hasFlatAmount = !flatAmount.IsEmpty && !currency.IsEmpty;
			if (hasFlatAmount)
			{
				fRoyaltyFlatAmount = flatAmount;
				fRoyaltyFlatAmountCurrency = currency;
			}
			return !percentage.IsEmpty || hasFlatAmount;
		}

		ZBool HasRoyalty(ZDecimal percentage, ZDecimal flatAmount, ZString currency)
		{
			bool hasFlatAmount = !flatAmount.IsEmpty && !currency.IsEmpty;

			return !percentage.IsEmpty || hasFlatAmount;
		}

		OrgPartRelation FindOrganisationWithRelationship(string relationType, OrgSupplierPart part, OrgHeader supplier, OrgHeader importer)
		{
			foreach (OrgPartRelation orgPartRelation in part.RelatedOrganisations)
			{
				if ((orgPartRelation.OU_OH == importer.PK || orgPartRelation.OU_OH == supplier.PK)
					&& orgPartRelation.OU_Relationship == relationType && HasRoyalty(orgPartRelation.OU_RoyaltyPercent, orgPartRelation.OU_RoyaltyFlatAmount, orgPartRelation.OU_RX_NKRoyaltyCurrency))
				{
					return orgPartRelation;
				}
			}
			return null;
		}
	}
}
