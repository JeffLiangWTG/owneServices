using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalTypeDecider : TypeDecider
	{
		public static OrgMatchApprovalTypeDecider GetInstance()
			=> TypeDecider.GetClientTypeDeciderFromType(typeof(OrgMatchApproval)) as OrgMatchApprovalTypeDecider
				?? TypeDecider.GetTypeDeciderFromType(typeof(OrgMatchApproval)) as OrgMatchApprovalTypeDecider;

		public override Type GetTypeForNew()
		{
			return typeof(EmptyOrgMatchApproval);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(OrgMatchApproval);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string matchType = (row == null) ? "" : row[OrgMatchApprovalSchema.P2_MatchType.Name].ToString();
			OrgMatchApprovalType result = OrgMatchApprovalType.FromCode(matchType);
			return result.ApprovalType;
		}
	}
}
