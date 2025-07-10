using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DocumentTypeDownloader : BaseJsonDownloader<DocumentTypeDTO>
	{
		public List<DossierDataDTO> Download(HttpClient client, bool isProduction = true)
		{
			var dossierDatas = new DocumentTypeOperationDownloader().Download(client, isProduction);

			foreach (var dossierData in dossierDatas)
			{
				if (dossierData.OperationType == DispatchInstructionDocuments.OperationTypes.LPCO)
				{
					foreach (var dossierTypes in dossierData.DossierTypes)
					{
						GetDocumentsKeywords(dossierTypes.DocumentTypes, client, isProduction);
					}
				}
				else
				{
					GetDocumentsKeywords(dossierData.DocumentTypes, client, isProduction);
				}
			}

			return dossierDatas;
		}

		void GetDocumentsKeywords(IEnumerable<DocumentTypeDTO> docs, HttpClient client, bool isProduction)
		{
			if (docs != null)
			{
				foreach (var documentTypeDTO in docs)
				{
					var url = GetURL(documentTypeDTO.DocumentTypeId.ToString(CultureInfo.InvariantCulture), isProduction);
					var document = DownloadJson(client, url);
					documentTypeDTO.Keywords = document.Keywords;
				}
			}
		}

		static string GetParameterName(bool isProduction) => isProduction ? "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS" : "URL_PORTAL_UNICO_TIPOS_DOCUMENTOS_TEST";

		string GetURL(string documentTypeID, bool isProduction) => GetParameter(GetParameterName(isProduction)).Replace("{idTipoDocumento}", documentTypeID);
	}
}
