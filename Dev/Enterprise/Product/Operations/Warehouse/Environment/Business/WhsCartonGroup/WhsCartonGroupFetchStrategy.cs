using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonGroupFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsCartonGroupFetchStrategy(WhsCartonGroup cartonGroup)
			: base(cartonGroup)
		{
		}

		#region FetchForViewCore

		// Tested in WhsCartonGroupFilterControl.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(c => c.ColumnName == WhsCartonGroup.Schema.AttachedOrganisationsCodes))
			{
				Factory.AddFetchHint(OrgMiscServSchema.OM_WCG_CartonGroup, CartonGroup.PK);

				var miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_WCG_CartonGroup, CartonGroup.PK);

				var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				orgQuery.AddSubQuery(miscServSubQuery, JoinCondition.And);

				Factory.AddFetchHint(OrgHeaderSchema.Instance, orgQuery);
			}
		}

		#endregion

		#region Implementation

		WhsCartonGroup CartonGroup
		{
			get { return (WhsCartonGroup)BusinessObject; }
		}

		#endregion
	}
}