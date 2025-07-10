using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalLoader : BusinessObject.Loader
	{
		internal OrgMatchApprovalLoader(BusinessObjectFactory factory) : base(factory)
		{ }

		public OrgMatchApproval[] LoadAll(ZGuid parentPK)
		{
			return LoadFromParentPKAndMatchTypeCode(parentPK);
		}

		public OrgMatchApproval Load(ZGuid parentPK, OrgMatchApprovalType matchApprovalType)
		{
			OrgMatchApproval[] candidateMatchApprovals = LoadFromParentPKAndMatchTypeCode(parentPK, matchApprovalType.Code);
			return (candidateMatchApprovals.Length == 0) ? null : candidateMatchApprovals[0];
		}

		#region LoadOrCreate

		public OrgMatchApproval LoadOrCreate(ZGuid parentPK, OrgMatchApprovalType matchApprovalType)
		{
			return LoadOrCreate(parentPK, null, matchApprovalType);
		}

		public OrgMatchApproval LoadOrCreate(OrgPatternMatchAddress matchAddressAttachedToParent, OrgMatchApprovalType matchApprovalType)
		{
			return LoadOrCreate(matchAddressAttachedToParent.P3_ParentID, matchAddressAttachedToParent, matchApprovalType);
		}

		OrgMatchApproval LoadOrCreate(ZGuid parentPK, OrgPatternMatchAddress matchAddressAttachedToParent, OrgMatchApprovalType matchApprovalType)
			=> Load(parentPK, matchApprovalType)
				?? New(parentPK, matchAddressAttachedToParent, matchApprovalType);

		#endregion

		#region Implementation

		OrgMatchApproval[] LoadFromParentPKAndMatchTypeCode(ZGuid parentPK)
		{
			return LoadFromParentPKAndMatchTypeCode(parentPK, null);
		}

		OrgMatchApproval[] LoadFromParentPKAndMatchTypeCode(ZGuid parentPK, string matchType)
		{
			ZQuery addressDetailsFilter = new ZQuery();
			addressDetailsFilter.AddToFilter(OrgPatternMatchAddressSchema.P3_ParentID, parentPK);

			OrgPatternMatchAddress[] matchApprovalAddressDetails = (OrgPatternMatchAddress[])Factory.Load(typeof(OrgPatternMatchAddress), addressDetailsFilter);
			ZQuery candidateMatchApprovalsFilter = GetMatchApprovalFilterFromAddressListAndMatchType(matchApprovalAddressDetails, matchType);
			OrgMatchApproval[] result = (OrgMatchApproval[])Factory.Load(GetTypeOfBusinessObjectToLoad(), candidateMatchApprovalsFilter);

			return result;
		}

		OrgMatchApproval New(ZGuid parentPK, OrgPatternMatchAddress matchAddressAttachedToParent, OrgMatchApprovalType matchApprovalType)
		{
			if (matchAddressAttachedToParent == null)
			{
				matchAddressAttachedToParent = Factory.New<OrgPatternMatchAddress>();
				matchAddressAttachedToParent.P3_ParentID = parentPK;
				matchAddressAttachedToParent.P3_ParentTableCode = GetTablePrefixForBusinessObjectType(matchApprovalType.ParentType);
				matchAddressAttachedToParent.P3_AddressType = matchApprovalType.Code;
			}

			OrgMatchApproval result = (OrgMatchApproval)Factory.New(matchApprovalType.ApprovalType);
			result.P2_ParentID = matchAddressAttachedToParent.PK;
			result.P2_ParentTableCode = GetTablePrefixForBusinessObjectType(typeof(OrgPatternMatchAddress));
			result.P2_MatchType = matchApprovalType.Code;

			result.CopyDetailsFromParent();
			return result;
		}

		string GetTablePrefixForBusinessObjectType(Type bizObjType)
		{
			string tableName = BusinessObjectFactory.GetTableNameFromType(bizObjType);
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
		}

		ZQuery GetMatchApprovalFilterFromAddressListAndMatchType(OrgPatternMatchAddress[] addresses, string matchTypeCode)
		{
			ZQuery result = ZQuery.NoResultQuery;
			if (addresses.Length > 0)
			{
				result = new ZQuery(OrgMatchApprovalSchema.P2_ParentID, Array.ConvertAll(addresses, address => address.PK));
				if (matchTypeCode != null)
				{
					result.AddToFilter(JoinCondition.And, OrgMatchApprovalSchema.P2_MatchType, SQLComparisonOperator.EndsWith, matchTypeCode);
				}
			}
			return result;
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(OrgMatchApproval);
		}

		#endregion
	}
}
