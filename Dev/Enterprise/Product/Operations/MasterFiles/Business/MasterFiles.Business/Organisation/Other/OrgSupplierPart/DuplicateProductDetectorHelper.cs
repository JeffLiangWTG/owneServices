using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class DuplicateProductDetectorNoBizOHelper
	{
		internal static IEnumerable<RelatedPartyWithCode> GetPartiesOnPart(OrgSupplierPart part)
		{
			var currentPartsRelatedOrgs = new List<RelatedPartyWithCode>();
			foreach (OrgPartRelation relationship in part.RelatedOrganisations)
			{
				if (relationship.Organisation != null)
				{
					currentPartsRelatedOrgs.Add(new RelatedPartyWithCode(relationship.OU_Relationship, relationship.Organisation.OH_Code));
				}
			}
			return currentPartsRelatedOrgs;
		}

		public static IEnumerable<PartAndFriends> GetOtherPartsInFactoryWithSameCode(OrgSupplierPart part)
		{
			var results = new List<PartAndFriends>();
			var query = new ZQuery(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, part.PK);
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.Equal, part.OP_PartNum);
			query.FetchOnlyFromLocalCache = true;
			var allOtherParts = part.Factory.Load<OrgSupplierPart>(query);
			foreach (var p in allOtherParts)
			{
				foreach (OrgPartRelation ou in p.RelatedOrganisations)
				{
					if (ou.Organisation != null)
					{
						results.Add(new PartAndFriends(p.PK, ou.OU_Relationship, ou.Organisation.OH_Code, p.OP_IsActive));
					}
				}
			}
			return results;
		}
	}

	public class PartAndFriends
	{
		public PartAndFriends(ZGuid oPPK, ZString relationshipType, ZString orgCode, ZBool isActive)
		{
			OPPK = oPPK;
			RelationshipType = relationshipType;
			OrgCode = orgCode;
			IsActive = isActive;
		}
		public ZGuid OPPK { get; set; }
		public ZString RelationshipType { get; set; }
		public ZString OrgCode { get; set; }
		public ZBool IsActive { get; set; }
	}
}
