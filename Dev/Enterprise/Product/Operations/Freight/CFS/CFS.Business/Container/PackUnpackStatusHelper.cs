using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public static class PackUnpackStatusHelper
	{
		public enum PackUnpackStatus
		{
			None,
			Pack,
			Unpack,
			InvalidSamePort,
			InvalidBranchHomePort
		}

		public static PackUnpackStatus GetPackUnpackStatus(ZString loadPort, ZString dischargePort, BusinessObjectFactory factory)
		{
			PackUnpackStatus status = PackUnpackStatus.None;

			if (GlbBranch.CurrentBranch.HomePort == null)
			{
				status = PackUnpackStatus.InvalidBranchHomePort;
			}
			else if (dischargePort == loadPort)
			{
				status = PackUnpackStatus.InvalidSamePort;
			}
			else if (CurrentBranchHandlesPacking(loadPort, factory))
			{
				status = PackUnpackStatus.Pack;
			}
			else if (CurrentBranchHandlesUnpacking(dischargePort, factory))
			{
				status = PackUnpackStatus.Unpack;
			}

			return status;
		}

		#region Implementation

		static bool CurrentBranchHandlesPacking(ZString uNLOCO, BusinessObjectFactory factory)
		{
			bool result = uNLOCO == GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			if (!result)
			{
				result = uNLOCO.StartsWith(GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2), StringComparison.Ordinal);
				if (result)
				{
					result &= !PackDepotExistsAtUNLOCO(uNLOCO, factory);
				}
			}
			return result;
		}

		static bool PackDepotExistsAtUNLOCO(ZString uNLOCO, BusinessObjectFactory factory)
		{
			bool result = false;
			ZQuery depotFilter = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, uNLOCO);
			depotFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch[] otherBranches = (GlbBranch[])factory.Load(typeof(GlbBranch), depotFilter);
			foreach (GlbBranch branch in otherBranches)
			{
				if (branch.GB_OH_OrgProxy.IsValid && branch.OrgProxy != null && branch.OrgProxy.OH_IsPackDepot)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		static bool CurrentBranchHandlesUnpacking(ZString uNLOCO, BusinessObjectFactory factory)
		{
			bool result = uNLOCO == GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			if (!result)
			{
				result = uNLOCO.StartsWith(GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2), StringComparison.Ordinal);
				if (result)
				{
					result &= !UnpackDepotExistsAtUNLOCO(uNLOCO, factory);
				}
			}
			return result;
		}

		static bool UnpackDepotExistsAtUNLOCO(ZString uNLOCO, BusinessObjectFactory factory)
		{
			bool result = false;
			ZQuery depotFilter = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, uNLOCO);
			depotFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch[] otherBranches = (GlbBranch[])factory.Load(typeof(GlbBranch), depotFilter);
			foreach (GlbBranch branch in otherBranches)
			{
				if (branch.GB_OH_OrgProxy.IsValid && branch.OrgProxy != null && branch.OrgProxy.OH_IsUnpackDepot)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
