using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business;

sealed class ProductMatchScoreProvider
{
	public ProductMatchScoreProvider(OrgSupplierPart product, ZGuid buyerPK, ZGuid supplierPK, bool isExport, bool isExactMatchEnabled)
	{
		Product = product;
		BuyerPK = buyerPK;
		SupplierPK = supplierPK;
		IsExport = isExport;
		IsExactMatchEnabled = isExactMatchEnabled;
	}

	public OrgSupplierPart Product { get; }
	public ZGuid BuyerPK { get; }
	public ZGuid SupplierPK { get; }
	public bool IsExport { get; }
	public bool IsExactMatchEnabled { get; }

	public int GetMatchScore()
	{
		var buyerMatchScore = 0;
		var supplierMatchScore = 0;
		if (!BuyerPK.IsEmpty)
		{
			buyerMatchScore = CalculateMatchScore(GetBuyerMatchScoreProviders());
			if (IsExactMatchEnabled && buyerMatchScore == 0 && ProductHasAnOwner(SupplierPK))
			{
				return 0;
			}
		}

		if (!SupplierPK.IsEmpty)
		{
			supplierMatchScore = CalculateMatchScore(GetSupplierMatchScoreCalculators());
			if (IsExactMatchEnabled && supplierMatchScore == 0 && ProductHasASupplier(BuyerPK))
			{
				return 0;
			}
		}
		return buyerMatchScore + supplierMatchScore;

		static int CalculateMatchScore(IEnumerable<Func<int>> matchScoreCalculators)
		{
			var score = 0;
			foreach (var calculator in matchScoreCalculators)
			{
				score = calculator();
				if (score > 0)
				{
					break;
				}
			}
			return score;
		}
	}

	IEnumerable<Func<int>> GetSupplierMatchScoreCalculators()
	{
		if (IsExport)
		{
			yield return () => GetOrgMatchScore(x => x.MatchesSupplier(SupplierPK), Constants.Export.Exporter_SUP_MatchScore);
			yield return () => GetOrgMatchScore(x => x.MatchesClassificationOrganisation(SupplierPK), Constants.Export.Exporter_CLS_MatchScore);

			if (GetOrgParent(SupplierPK) is { IsEmpty: false } supplierParent)
			{
				yield return () => GetOrgMatchScore(x => x.MatchesSupplier(supplierParent), Constants.Export.ExporterParent_SUP_MatchScore);
				yield return () => GetOrgMatchScore(x => x.MatchesClassificationOrganisation(supplierParent), Constants.Export.ExporterParent_CLS_MatchScore);
			}
		}
		else
		{
			yield return () => GetOrgMatchScore(x => x.MatchesSupplier(SupplierPK), Constants.Import.Exporter_SUP_MatchScore);
		}
	}

	IEnumerable<Func<int>> GetBuyerMatchScoreProviders()
	{
		if (IsExport)
		{
			yield return () => GetOrgMatchScore(x => x.MatchesImporter(BuyerPK), Constants.Export.Importer_OWN_MatchScore);
			if (IsSupplierMatchedOnProductSupplier())
			{
				yield return () => GetOrgMatchScore(x => x.MatchesClassificationOrganisation(BuyerPK), Constants.Export.Importer_CLS_MatchScore);
			}
		}
		else
		{
			yield return () => GetOrgMatchScore(x => x.MatchesImporter(BuyerPK), Constants.Import.Importer_OWN_MatchScore);
			yield return () => GetOrgMatchScore(x => x.MatchesClassificationOrganisation(BuyerPK), Constants.Import.Importer_CLS_MatchScore);

			if (GetOrgParent(BuyerPK) is { IsEmpty: false } buyerParent)
			{
				yield return () => GetOrgMatchScore(x => x.MatchesImporter(buyerParent), Constants.Import.ImporterParent_OWN_MatchScore);
				yield return () => GetOrgMatchScore(x => x.MatchesClassificationOrganisation(buyerParent), Constants.Import.ImporterParent_CLS_MatchScore);
			}
		}

		bool IsSupplierMatchedOnProductSupplier()
			=> Product.RelatedOrganisations.Cast<OrgPartRelation>().Any(x => x.MatchesSupplier(SupplierPK));
	}

	int GetOrgMatchScore(Func<OrgPartRelation, bool> orgMatchingCriteria, int orgMatchScore)
	{
		return Product.RelatedOrganisations.Cast<OrgPartRelation>().Any(orgMatchingCriteria) ? orgMatchScore : 0;
	}

	ZGuid GetOrgParent(ZGuid orgPK)
	{
		var loader = new OrgRelatedParty.Loader(Product.Factory);
		return loader.LoadPartiesWithRelatedOrgAndType(orgPK, RelatedPartyTypeList.Codes.ProductRelationship)
			.OrderByDescending(x => x.PR_SystemCreateTimeUtc)
			.Select(x => x.PR_OH_Parent)
			.FirstOrDefault();
	}

	bool ProductHasAnOwner(ZGuid supplierPK) => HasOwnerSupplierOrg(supplierPK, OrgPartRelation.RelationshipTypes.Owner);

	bool ProductHasASupplier(ZGuid buyerPK) => HasOwnerSupplierOrg(buyerPK, OrgPartRelation.RelationshipTypes.Supplier);

	bool HasOwnerSupplierOrg(ZGuid buyerSupplierOrgPK, string relationshipType)
	{
		return Product.RelatedOrganisations.Cast<OrgPartRelation>().Any(Match);

		bool Match(OrgPartRelation relation)
		{
			return relation.OU_Relationship == relationshipType
					|| (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both
					&& relation.OU_OH != buyerSupplierOrgPK);
		}
	}

	static class Constants
	{
		public static class Import
		{
			public const int Importer_OWN_MatchScore = 16;
			public const int Importer_CLS_MatchScore = 8;
			public const int ImporterParent_OWN_MatchScore = 4;
			public const int ImporterParent_CLS_MatchScore = 2;
			public const int Exporter_SUP_MatchScore = 1;
		}

		public static class Export
		{
			public const int Exporter_SUP_MatchScore = 32;
			public const int Exporter_CLS_MatchScore = 16;
			public const int ExporterParent_SUP_MatchScore = 8;
			public const int ExporterParent_CLS_MatchScore = 4;
			public const int Importer_OWN_MatchScore = 2;
			public const int Importer_CLS_MatchScore = 1;
		}
	}
}
