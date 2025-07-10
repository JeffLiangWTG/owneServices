using System.Collections;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitCRESAMessageProcessor<T> : IProcessor where T : EnterpriseBusinessObject
	{
		public TransitCRESAMessageProcessor(T parent)
		{
			consignment = parent;
		}

		readonly T consignment;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var allMessageSendersRegistration = ObjectFactory.Get<Hashtable>("DocDataObjectWithoutUIMessageSendersProvider");
			if (allMessageSendersRegistration != null
				&& (allMessageSendersRegistration[CRESASender] is ObjectHandle senderRegistration)
				&& (senderRegistration.GetObject() is IDocDataObjectWithoutUIMessageSender messageSender))
			{
				messageSender.SendMessage(consignment, notifications);
			}
		}

		const string CRESASender = "FR_CRESA";
	}
}
