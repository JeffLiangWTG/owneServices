using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitCIN750MessageProcessor<T> : IProcessor where T : BusinessObject, IConsignment
	{
		public TransitCIN750MessageProcessor(BusinessObject parent)
		{
			consignment = parent;
		}

		readonly BusinessObject consignment;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var noUISender = ObjectFactory.Get("CIN750WithoutUIMessageSender");
			if (noUISender != null && noUISender is IDocDataObjectWithoutUIMessageSender sender)
			{
				sender.SendMessage(consignment, notifications);
			}
		}
	}
}
