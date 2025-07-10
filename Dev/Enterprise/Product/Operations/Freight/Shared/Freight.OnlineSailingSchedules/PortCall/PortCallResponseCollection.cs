using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallResponseCollection : NonPersistentBusinessObjectCollection<PortCallResponse>
	{
		public PortCallResponseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Load(PortCallRequest request, INotifications notifications)
		{
			var searchParams = request.GetRequestString(out var errorMessage);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				notifications.AddError(errorMessage);
				return;
			}

			var portCallProvider = GetPortCallProvider();
			var portCall = portCallProvider.GetPortCall(searchParams, new PortCallServiceRequestManager(notifications));

			RemoveAll();

			if (portCall?.Items != null)
			{
				foreach (var item in portCall.Items)
				{
					var response = AddNew();
					response.RequestType = request.RequestType;
					response.SetValues(item);
				}
			}
		}

		protected virtual IPortCallProvider GetPortCallProvider()
		{
			return new PortCallProvider();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortCallResponse();
		}
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
