using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DeduplicationOrganisationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DeduplicationOrganisationFetchStrategy(DeduplicationOrganisation deduplicationOrganisation) : base(deduplicationOrganisation) { }

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == DeduplicationOrganisation.Schema.DOH_CreatedUnderBranch ||
					column.ColumnName == DeduplicationOrganisation.Schema.DOH_CreatedUnderCompany)
				{
					Factory.AddFetchHint(StmALogSchema.Instance, new ZQuery(StmALogSchema.SL_Parent, BusinessObject.PK)
						.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AddedARecordToTheSystemCode));
				}
			}
		}

		#endregion
	}
}
