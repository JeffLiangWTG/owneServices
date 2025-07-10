using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class SalesRelationActivityFetchStrategyHelper
	{
		public static void AddFetchHintsForView(BusinessObjectFactory factory, ISalesRelationActivity businessObject, TableColumn[] columns)
		{
			bool requiresRelatedActivityData = columns.Any(col => col.ColumnName == "SalesRelationModel.RecentActivityDate" || col.ColumnName == "SalesRelationModel.HasSalesRelation");
			if (requiresRelatedActivityData)
			{
				factory.AddFetchHint(ViewSalesRelationActivityDataSchema.PK, businessObject.PK);
			}
		}
	}
}
