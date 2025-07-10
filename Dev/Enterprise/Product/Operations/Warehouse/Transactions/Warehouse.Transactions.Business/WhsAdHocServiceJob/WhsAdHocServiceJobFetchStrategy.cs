using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsAdHocServiceJobFetchStrategy(WhsAdHocServiceJob job)
			: base(job)
		{
		}

		#region FetchForViewCore

		// Tested in AdHocServiceJobFilterControl.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var requireJH = false;
			var requireOA = false;
			var requireOH = false;
			var requireWarehouse = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsAdHocServiceJob.Schema.WSJ_CustomerReference:
						requireJH = true;
						requireOA = true;
						requireOH = true;
						break;

					case WhsAdHocServiceJob.Schema.Client + "+" + OrgHeaderSchema.Constants.OH_Code:
					case WhsAdHocServiceJob.Schema.Client + "+" + OrgHeaderSchema.Constants.OH_FullName:
						requireJH = true;
						requireOA = true;
						requireOH = true;
						break;

					case WhsAdHocServiceJob.Schema.Warehouse + "+WW_WarehouseNameMultilingual":
						requireWarehouse = true;
						break;

					default:
						break;
				}
			}

			if (requireJH)
			{
				var queryJH = new ZQuery(JobHeaderSchema.JH_ParentID, Job.PK);
				queryJH.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

				Factory.AddFetchHint(JobHeaderSchema.Instance, queryJH);
			}

			if (requireOA)
			{
				var subQueryJH = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_OA_LocalChargesAddr);
				subQueryJH.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				subQueryJH.AddToFilter(JobHeaderSchema.JH_ParentID, Job.PK);

				var queryOA = new ZDBOnlyQuery(typeof(OrgAddress));
				queryOA.AddSubQuery(subQueryJH, JoinCondition.And);

				Factory.AddFetchHint(OrgAddressSchema.Instance, queryOA);
			}

			if (requireOH)
			{
				var subQueryJH = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_OA_LocalChargesAddr);
				subQueryJH.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				subQueryJH.AddToFilter(JobHeaderSchema.JH_ParentID, Job.PK);

				var subQueryOA = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				subQueryOA.AddSubQuery(subQueryJH, JoinCondition.And);

				var queryOH = new ZDBOnlyQuery(typeof(OrgHeader));
				queryOH.AddSubQuery(subQueryOA, JoinCondition.And);

				Factory.AddFetchHint(OrgHeaderSchema.Instance, queryOH);
			}

			if (requireWarehouse)
			{
				Factory.AddFetchHint(WhsWarehouseSchema.PK, Job.WSJ_WW_Whs);
			}
		}

		#endregion

		#region implementation

		WhsAdHocServiceJob Job => (WhsAdHocServiceJob)BusinessObject;

		#endregion
	}
}
