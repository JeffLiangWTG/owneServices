using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgPartBOMValidationHelper
	{
		public static bool DoesProductHaveOwner(OrgSupplierPart product)
		{
			return product != null && product.RelatedOrganisations.Cast<OrgPartRelation>().Any(relation => IsProductOwnerRelationship(relation));
		}

		public static bool DoCommonOwnersExist(OrgSupplierPart mainProduct, OrgSupplierPart componentPart)
		{
			if (mainProduct != null
				&& mainProduct.RelatedOrganisations.Count > 0
				&& componentPart != null
				&& componentPart.RelatedOrganisations.Count > 0)
			{
				foreach (OrgPartRelation componentRelation in componentPart.RelatedOrganisations)
				{
					if (IsProductOwnerRelationship(componentRelation)
						&& mainProduct.RelatedOrganisations.Cast<OrgPartRelation>().Any(mainProductRelation => IsProductOwnerRelationship(mainProductRelation) && mainProductRelation.OU_OH == componentRelation.OU_OH))
					{
						return true;
					}
				}
			}

			return false;
		}

		static bool IsProductOwnerRelationship(OrgPartRelation relation)
		{
			return relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both;
		}

		public static bool IsProductComponentOnMainProductWithIsPickOnOrder(OrgSupplierPart product)
		{
			if (product != null)
			{
				var factory = product.Factory;
				var query = new ZQuery(OrgPartBOMSchema.OE_OP_Component, product.PK);
				var partBomsOfProduct = factory.Load<OrgPartBOM>(query);

				foreach (var partBom in partBomsOfProduct)
				{
					var partBomProductQuery = new ZQuery(OrgSupplierPartSchema.PK, partBom.OE_OP_MainProduct);
					partBomProductQuery.AddToFilter(OrgSupplierPartSchema.OP_IsComponentPickedOnSalesOrder, true);
					var partBomMainProductWithIsPickOnOrder = factory.LoadTop1<OrgSupplierPart>(partBomProductQuery);

					if (partBomMainProductWithIsPickOnOrder != null)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
