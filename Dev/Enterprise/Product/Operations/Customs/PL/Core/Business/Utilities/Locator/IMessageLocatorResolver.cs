using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.Business;

interface IMessageLocatorResolver
{
	Integration.Customs.PL.IMessageLocator GetMessageLocator(MessageTypes messageType);
}
