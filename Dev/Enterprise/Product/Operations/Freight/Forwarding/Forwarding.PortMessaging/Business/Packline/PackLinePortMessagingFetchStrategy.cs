using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Packline
{
	class PackLinePortMessagingFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PackLinePortMessagingFetchStrategy(PackLinePortMessaging businessObject) : base(businessObject)
		{
			packline = businessObject;
		}

		protected PackLinePortMessaging packline;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			foreach (var column in columns)
			{
				if (column.ColumnName == PackLinePortMessaging.Schema.DGTechnicalName)
				{
					Factory.AddFetchHint(JobPackLinesSchema.PK, packline.PK);
					Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, packline.JLM_JL_PackLine);
				}
			}
		}
	}
}
