using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartBOMLookups : AutoOrgPartBOMLookups
	{
		public OrgPartBOMLookups(AutoOrgPartBOM parent)
			: base(parent)
		{
		}

		OrgPartBOM BOM
		{
			get { return (OrgPartBOM)Parent; }
		}

		#region SubParts

		public OrgSupplierPartCollection SubParts
		{
			get
			{
				if (subParts == null || HasMainProductOwnersChanged)
				{
					var subPartQueryFilter = CreateSubPartQueryFilter();
					subParts = new OrgSupplierPartCollection(Factory, subPartQueryFilter);
					cachedMainProductOwners = MainProductOwners;
				}

				return subParts;
			}
		}
		OrgSupplierPartCollection subParts;

		/// <summary>
		/// Has the list of main product owners changed?
		/// </summary>
		bool HasMainProductOwnersChanged
		{
			get
			{
				var hasChanged = true;

				var currentMainProductOwners = MainProductOwners;
				if (currentMainProductOwners.Count == cachedMainProductOwners.Count && !currentMainProductOwners.Except(cachedMainProductOwners).Any())
				{
					hasChanged = false;
				}
				return hasChanged;
			}
		}
		List<ZGuid> cachedMainProductOwners = new List<ZGuid>();

		List<ZGuid> MainProductOwners
		{
			get
			{
				var productOwners = new List<ZGuid>();
				var mainProduct = BOM.MainProduct; // put BOM.MainProduct into a local var (it's a factory load)
				if (mainProduct != null && mainProduct.RelatedOrganisations != null && mainProduct.RelatedOrganisations.BuyerRelations != null)
				{
					var mainProductOwnerPKs =
						from orgRelation in mainProduct.RelatedOrganisations.BuyerRelations
						where orgRelation.OU_OH != ZGuid.Empty
						select orgRelation.OU_OH;

					productOwners = new List<ZGuid>(mainProductOwnerPKs);
				}
				return productOwners;
			}
		}

		/// <summary>
		/// Build a filter to use against the OrgSupplierPartCollection.
		/// </summary>
		ZQuery CreateSubPartQueryFilter()
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, BOM.OE_OP_MainProduct);

			var mainProductOwnerPKs = MainProductOwners;

			if (mainProductOwnerPKs.Count > 0)
			{
				var relationshipTypes = new string[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both };

				var subQueryOwner = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQueryOwner.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, mainProductOwnerPKs);
				subQueryOwner.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, relationshipTypes);
				query.AddSubQuery(subQueryOwner, JoinCondition.And);
			}
			return query;
		}

		#endregion

		#region ProductUQList

		public CodeDescriptionPairList ProductUQList
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion
	}
}
