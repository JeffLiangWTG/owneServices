using System.Collections.Generic;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DocumentTypeOperationDownloader : BaseJsonDownloader<DossierDataDTO>
	{
		public List<DossierDataDTO> Download(HttpClient client, bool isProduction = true)
		{
			var dossiesDatsList = new List<DossierDataDTO>();
			foreach (var operationType in GetOperationtypes())
			{
				var url = GetURL(isProduction).Replace("{tipoOperacao}", operationType);

				var dossierDataDTO = DownloadJson(client, url);
				dossierDataDTO.OperationType = operationType;
				dossiesDatsList.Add(dossierDataDTO);
			}

			return dossiesDatsList;
		}

		string GetURL(bool isProduction) => GetParameter(isProduction ? "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_OPERACAO" : "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_OPERACAO_TEST");

		static string[] GetOperationtypes() => DispatchInstructionDocuments.OperationTypes.ToArray;
	}
}
