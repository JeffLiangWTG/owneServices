using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgRelatedPartyFetchStrategy(OrgRelatedParty businessObject) : base(businessObject) { }

		protected OrgRelatedParty RelatedParty
		{
			get { return (OrgRelatedParty)BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (string.IsNullOrEmpty(column.TableName))
				{
					if (column.ColumnName == nameof(OrgRelatedParty.ParentName))
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, RelatedParty.PR_OH_Parent);
					}
					else if (column.ColumnName == nameof(OrgRelatedParty.RelatedPartyName))
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, RelatedParty.PR_OH_RelatedParty);
					}
				}
			}
		}
	}
}
