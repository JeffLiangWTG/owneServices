using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using static Enterprise.Warehouse.Transit.Document.TransitDocDataConstants;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class TransitDocDataObjectProvider : ITransitDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (parent is WhsItemReceiveConsignment rcn)
			{
				return GetFromReceiveConsignment(rcn, dataContext);
			}
			else if (parent is WhsItemDispatchConsignment dcn)
			{
				return GetFromDispatchConsignment(dcn, dataContext, parameters);
			}
			return null;
		}

		public CIN750Notification GetFromReceiveConsignment(WhsItemReceiveConsignment consignment, string dataContext)
		{
			switch (dataContext)
			{
				case TransitDocDataContext.CIN750WarehouseIn:
					return new CIN750InNotificationBuilder(consignment).Build();
				case TransitDocDataContext.CIN750WarehouseCor:
					return new CIN750CorNotificationBuilder(consignment).Build();
			}
			return null;
		}

		public CIN750Notification GetFromDispatchConsignment(WhsItemDispatchConsignment consignment, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case TransitDocDataContext.CIN750WarehouseDecons:
					return new CIN750DeconsNotificationBuilder(consignment).Build();
				case TransitDocDataContext.CIN750WarehouseCons:
					return new CIN750ConsNotificationBuilder(consignment, parameters.Data as ConsNotificationAdditionalData).Build();
				case TransitDocDataContext.CIN750WarehouseOut:
					return new CIN750OutNotificationBuilder(consignment, parameters.Data as OutNotificationAdditionalData).Build();
			}

			return null;
		}
	}
}
