using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CargoIMPPhase2MessageNumberStrategy : IMessageNumberStrategy
	{
		const string GenericMessageReceiverName = "CARGOIMPPHASE2";

		public CargoIMPPhase2MessageNumberStrategy(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			string sender = ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Value;
			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, GenericMessageReceiverName).GetNextFormatted(factory);
		}

		#endregion
	}
}
