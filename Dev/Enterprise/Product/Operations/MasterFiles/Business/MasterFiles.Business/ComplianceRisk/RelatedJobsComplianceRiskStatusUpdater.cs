using System;
using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedJobsComplianceRiskStatusUpdater
	{
		IEnumerable<RelatedJobComplianceRiskStatusUpdater> RelatedJobComplianceRiskStatusUpdaters { get; set; } = new List<RelatedJobComplianceRiskStatusUpdater>
		{
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedShipmentsComplianceRiskStatusForOrg2", "UpdateRelatedShipmentsComplianceRiskStatusForVessel2"),
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedConsolsComplianceRiskStatusForOrg2", "UpdateRelatedConsolsComplianceRiskStatusForVessel2"),
			new RelatedJobComplianceRiskStatusUpdater("UpdateRelatedQuotedBookingsComplianceRiskStatusForOrg2", "UpdateRelatedQuotedBookingsComplianceRiskStatusForVessel2")
		};

		public void UpdateForOrg(Guid orgPK, Guid companyPK, Action<DbCommand, DbConnection> excuteCommandAction)
		{
			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				foreach (var updater in RelatedJobComplianceRiskStatusUpdaters)
				{
					updater.UpdateForOrg(orgPK, companyPK, excuteCommandAction);
				}
			}
		}

		public void UpdateForVessel(Guid vesselPK, Guid companyPK, Action<DbCommand, DbConnection> excuteCommandAction)
		{
			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				foreach (var updater in RelatedJobComplianceRiskStatusUpdaters)
				{
					updater.UpdateForVessel(vesselPK, companyPK, excuteCommandAction);
				}
			}
		}
	}
}
