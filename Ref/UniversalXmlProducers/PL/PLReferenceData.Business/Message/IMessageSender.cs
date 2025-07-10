using System.Xml.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message;

interface IMessageSender
{
	string SendTariffUpdateRequest(XDocument document);
	byte[] GetTariffUpdate(string sysRef);
}
