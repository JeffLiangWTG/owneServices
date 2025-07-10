using CargoWise.Application;
using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.Business;

sealed class MessageLocatorResolver : IMessageLocatorResolver
{
	public Integration.Customs.PL.IMessageLocator GetMessageLocator(MessageTypes messageType) =>
		messageType switch
		{
			MessageTypes.None or MessageTypes.DocumentHandlingPort => null,
			MessageTypes.Common => ObjectFactory.Get<Integration.Customs.PL.IMessageLocator>(name: "PL.Common.IMessageLocator"),
			MessageTypes.AIS or MessageTypes.AES or MessageTypes.AESAIS => ObjectFactory.Get<Integration.Customs.PL.IMessageLocator>(name: "PL.IMessageLocator"),
			MessageTypes.Arrival or MessageTypes.Departure or MessageTypes.ArrivalAndDeparture => ObjectFactory.Get<Integration.Customs.PL.IMessageLocator>(name: "PLNCTS.IMessageLocator"),
			MessageTypes.ExportControl => ObjectFactory.Get<Integration.Customs.PL.IMessageLocator>(name: "PLExitControl.IMessageLocator"),
			_ => null,
		};
}
