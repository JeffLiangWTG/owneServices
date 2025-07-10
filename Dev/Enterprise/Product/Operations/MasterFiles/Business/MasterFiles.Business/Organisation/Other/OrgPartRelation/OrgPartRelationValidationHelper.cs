using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgPartRelationValidationHelper
	{
		static readonly ImmutableList<string> DuplicatedRelationships = ImmutableList.Create(
			OrgPartRelation.RelationshipTypes.Owner + OrgPartRelation.RelationshipTypes.Both,
			OrgPartRelation.RelationshipTypes.Supplier + OrgPartRelation.RelationshipTypes.Both,
			OrgPartRelation.RelationshipTypes.Both + OrgPartRelation.RelationshipTypes.Owner,
			OrgPartRelation.RelationshipTypes.Both + OrgPartRelation.RelationshipTypes.Supplier
		);

		#region HasSameSupplierAndOwner

		public static bool HasSameSupplierAndOwner(IEnumerable<RelatedParty> allRelationships, ZGuid parentOUOH, ZString parentRelationshipType)
		{
			ZString mutualRelationship = GetMutualRelationship(parentRelationshipType);
			return !mutualRelationship.IsEmpty && allRelationships.Any(ou => ou.RelatedOrganisationPK == parentOUOH && ou.RelationshipCode == mutualRelationship);
		}

		public static bool HasSameSupplierAndOwner(IEnumerable<RelatedPartyWithCode> allRelationships, ZString parentRelationshipOrganizationCode, ZString parentRelationshipType)
		{
			ZString mutualRelationship = GetMutualRelationship(parentRelationshipType);
			return !mutualRelationship.IsEmpty && allRelationships.Any(ou => ou.RelatedOrganisationCode == parentRelationshipOrganizationCode && ou.RelationshipCode == mutualRelationship);
		}

		static ZString GetMutualRelationship(ZString parentRelationshipType)
		{
			if (parentRelationshipType == OrgPartRelation.RelationshipTypes.Owner)
			{
				return OrgPartRelation.RelationshipTypes.Supplier;
			}

			if (parentRelationshipType == OrgPartRelation.RelationshipTypes.Supplier)
			{
				return OrgPartRelation.RelationshipTypes.Owner;
			}

			return "";
		}

		#endregion

		#region HasDuplicateRelationship

		public static bool HasDuplicateRelationship(IEnumerable<RelatedParty> allRelationships, ZGuid parentOUOH, ZString parentRelationshipType)
		{
			foreach (var ou in allRelationships)
			{
				if (ou.RelatedOrganisationPK == parentOUOH &&
					(ou.RelationshipCode == parentRelationshipType || DuplicatedRelationships.Contains(ou.RelationshipCode + parentRelationshipType)))
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasDuplicateRelationship(IEnumerable<RelatedPartyWithCode> allRelationships, ZString parentRelationshipOrganizationCode, ZString parentRelationshipType)
		{
			foreach (var ou in allRelationships)
			{
				if (ou.RelatedOrganisationCode == parentRelationshipOrganizationCode &&
					(ou.RelationshipCode == parentRelationshipType || DuplicatedRelationships.Contains(ou.RelationshipCode + parentRelationshipType)))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region GetProductNumbersWithDuplicateBarcode

		public static IEnumerable<ZString> GetProductNumbersWithDuplicateBarcode(BusinessObjectFactory factory, ZGuid productPK, IEnumerable<ZGuid> owners, IEnumerable<ZString> barcodes)
		{
			var result = Enumerable.Empty<ZString>();
			if (owners.Any())
			{
				var sameBarcodesInOtherProducts = factory.Load<OrgSupplierPartBarcode>(GetBarcodeDuplicateBarcodeCheckQuery(productPK, barcodes));

				var query = GetProductDuplicateBarcodeCheckQuery(productPK, barcodes, sameBarcodesInOtherProducts);
				var parts = factory.Load<OrgSupplierPart>(query).Where(p => p.RelatedOrganisations.Cast<OrgPartRelation>().Any(o => owners.Contains(o.OU_OH) && o.IsOwner));
				result = parts.Select(p => p.OP_PartNum).OrderBy(p => p);
			}

			return result;
		}

		public static ZQuery GetBarcodeDuplicateBarcodeCheckQuery(ZGuid productPK, IEnumerable<ZString> barcodes)
		{
			var barcodeQuery = new ZQuery(OrgSupplierPartBarcodeSchema.PH_Barcode, barcodes);
			barcodeQuery.AddToFilter(OrgSupplierPartBarcodeSchema.PH_OP, SQLComparisonOperator.NotEqual, productPK);
			return barcodeQuery;
		}

		public static ZQuery GetProductDuplicateBarcodeCheckQuery(ZGuid productPK, IEnumerable<ZString> barcodes, IEnumerable<OrgSupplierPartBarcode> duplicateBarcodesFromOtherProducts)
		{
			var productQuery = new ZQuery(OrgSupplierPartSchema.PK, duplicateBarcodesFromOtherProducts.Select(b => b.PH_OP));
			productQuery.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_PartNum, barcodes);
			productQuery.AddToFilter(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, productPK);
			var query = new ZQuery(OrgSupplierPartSchema.OP_IsActive, true);
			query.AddToFilter(productQuery);

			return query;
		}

		#endregion

		#region CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife

		public static ZString CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(OrgPartRelation partRelation, ZShort newConsigneeMinShelfLifeAcceptedValue)
		{
			Argument.NotNull(partRelation, "OrgPartRelation");

			var errorMessage = ZString.Empty;
			if (partRelation.IsOwner)
			{
				var query = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, partRelation.OU_OP);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, partRelation.OU_OH);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_MaximumShelfLife, SQLComparisonOperator.GreaterThan, ZShort.Zero);
				query.OrderBy = WhsProductParamsByWhsAndClientSchema.W3_MaximumShelfLife.Name;
				var productParams = partRelation.Factory.LoadTop1<IWhsProductParamsByWhsAndClient>(query);
				var maximumShelfLife = productParams?.W3_MaximumShelfLife ?? ZShort.Zero;
				if (maximumShelfLife > 0 && newConsigneeMinShelfLifeAcceptedValue > maximumShelfLife)
				{
					errorMessage = GetConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLifeErrorMessage(newConsigneeMinShelfLifeAcceptedValue, maximumShelfLife);
				}
			}

			return errorMessage;
		}

		public static ZString GetConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLifeErrorMessage(ZShort consigneeMinShelfLifeAccepted, ZShort maximumShelfLife)
			=> Res.GetString("3665ba4d-5beb-4404-a121-e550602982ee", "Minimum shelf life {0} cannot be greater than Maximum Shelf Life {1}.", consigneeMinShelfLifeAccepted, maximumShelfLife);

		#endregion

		#region CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress

		public static ZString CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(OrgPartRelation partRelation, ZShort newConsigneeMinShelfLifeAcceptedValue)
		{
			Argument.NotNull(partRelation, "OrgPartRelation");

			var errorMessage = ZString.Empty;
			if ((partRelation.IsOwner && partRelation.OU_UseExpiryDate) || partRelation.OU_Relationship == OrgPartRelation.RelationshipTypes.WarehouseConsignee)
			{
				var minimumShelfLifeIncreased = newConsigneeMinShelfLifeAcceptedValue > (ZShort)partRelation.OU_ConsigneeMinShelfLifeAcceptedInfo.OriginalValue;
				if (minimumShelfLifeIncreased && IsAnyUnfinalisedPicksProductExpiryDateExist(partRelation))
				{
					errorMessage = Res.GetString("bfb3e273-c752-4d15-92dd-4ab0fe4be63e", "This product has been ordered and pick is not finalized therefore minimum Shelf Life cannot be increased.");
				}
			}

			return errorMessage;
		}

		static bool IsAnyUnfinalisedPicksProductExpiryDateExist(OrgPartRelation partRelation)
		{
			var relationSQL = partRelation.IsOwner
				? @"OwnerRelation.OU_PK = @PartRelationPK and isnull(OM_MinimumShelfLifeAccepted, 0) = 0 and isnull(ConsigneeRelation.OU_ConsigneeMinShelfLifeAccepted, 0) = 0"
				: @"ConsigneeRelation.OU_PK = @PartRelationPK and OwnerRelation.OU_UseExpiryDate = 1";

			var rawSQL = $@"
SELECT TOP(1) 
	RowExists = convert(bit, NULL) 
FROM
	dbo.WhsPick
	join dbo.WhsDocket on WD_WP = WP_PK
	join dbo.WhsDocketLine on WE_WD = WD_PK
	join dbo.JobDocAddress on E2_ParentID = WD_PK and E2_AddressType = '{DocAddressTypes.Codes.ConsigneeAddress}'
	join dbo.OrgAddress on OA_PK = E2_OA_Address
	join dbo.OrgMiscServ as ConsigneeMiscServ on ConsigneeMiscServ.OM_OH = OA_OH
	join dbo.OrgPartRelation as OwnerRelation on
		OwnerRelation.OU_OP = WE_OP and
		OwnerRelation.OU_OH = WD_OH_Client and
		OwnerRelation.OU_Relationship IN ('{OrgPartRelation.RelationshipTypes.Owner}', '{OrgPartRelation.RelationshipTypes.Both}')
	left join dbo.OrgPartRelation as ConsigneeRelation on
		ConsigneeRelation.OU_OP = WE_OP and
		ConsigneeRelation.OU_OH = OA_OH and
		ConsigneeRelation.OU_Relationship = '{OrgPartRelation.RelationshipTypes.WarehouseConsignee}'
WHERE
	({relationSQL}) and
	WP_PickStatus != 'FIN' and
	WP_PickStatus != 'CAN'";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@PartRelationPK", partRelation.PK, OrgPartRelationSchema.PK }
			};

			var collection = new DynamicBusinessObjectCollection(partRelation.Factory);
			collection.Load(rawSQL, sqlParams);
			return collection.Any();
		}

		#endregion

		#region Check Procedure Names

		public const string WhsCheckOwnerAndBarcodeOfProductAreUnique = "WhsCheckOwnerAndBarcodeOfProductAreUnique";
		public const string UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart = "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart";

		#endregion

		#region Trigger Names

		public const string TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique = "TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique";
		public const string TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique = "TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique";
		public const string TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ = "TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ";

		#endregion
	}
}
