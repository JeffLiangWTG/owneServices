using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgCommissionAgreementFetchStrategy(OrgCommissionAgreement commissionAgreement)
			: base(commissionAgreement)
		{
		}

		#region Fetch For View

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var commissionAgreement = (OrgCommissionAgreement)BusinessObject;

			if (columns.Any(x => x.ColumnName.StartsWith(PropertyNames.Opportunity, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(OrgOpportunitySchema.PK, commissionAgreement.CA0_P8);
			}

			if (columns.Any(x => x.ColumnName.StartsWith(PropertyNames.Customer, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, commissionAgreement.CA0_OH_Customer);
			}

			if (columns.Any(x => x.ColumnName == OrgCommissionAgreement.Schema.Status || x.ColumnName == OrgCommissionAgreement.Schema.StatusDescription))
			{
				Factory.AddFetchHint(OrgCommissionCalculationQueueSchema.CAQ_CA0, commissionAgreement.PK);
				if (!commissionAgreement.CA0_CA0_ParentVersion.IsEmpty)
				{
					Factory.AddFetchHint(OrgCommissionCalculationQueueSchema.CAQ_CA0, commissionAgreement.CA0_CA0_ParentVersion);
				}
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		static class PropertyNames
		{
			public const string Opportunity = "Opportunity";
			public const string Customer = "Customer";
		}

		#endregion
	}
}
