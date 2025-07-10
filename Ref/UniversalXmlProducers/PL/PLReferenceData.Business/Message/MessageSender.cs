using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message;

sealed class MessageSender(IPuescServiceFactory factory) : IMessageSender
{
	readonly IPuescServiceFactory puescServiceFactory = factory;

	public string SendTariffUpdateRequest(XDocument document)
	{
		AcceptDocumentRequest request = new()
		{
			document = new()
			{
				content = new()
				{
					Value = Encoding.UTF8.GetBytes(document.ToString()),
					filename = Constants.Soap.Puesc.TariffUpdateRequestFileName,
					mime = mimeType.applicationxml
				},
				targetSystems = [systemType.ISZTAR4]
			}
		};
		using var puescService = puescServiceFactory.Create();
		var response = puescService.AcceptDocument(request);
		return response?.result?.sysRef;
	}

	public byte[] GetTariffUpdate(string sysRef)
	{
		GetDocumentsRequest getDocument = new()
		{
			pobrany = "0",
			targetSystem = systemType.ISZTAR4,
			korelacjaSysref = sysRef
		};
		using var puescService = puescServiceFactory.Create();
		var response = puescService.GetDocuments(getDocument);
		var tarrifUpdateFile = response?.document?.FirstOrDefault(x => x.content.filename == Constants.Soap.Puesc.TariffUpdateResponseFilename);
		return tarrifUpdateFile?.content.Value;
	}
}
