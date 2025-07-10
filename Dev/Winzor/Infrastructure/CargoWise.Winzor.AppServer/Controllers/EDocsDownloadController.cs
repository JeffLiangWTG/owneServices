using System.Text.Encodings.Web;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.Winzor.AppServer
{
	public class EDocsDownloadController : ControllerBase
	{
		[HttpGet]
		[Route("/edoc/download/{fileName}")]
		public void DownLoadEDocs(string fileName, string bizPK, string docPK, [FromServices] DocumentFactory documentFactory)
		{
			Response.ContentType = (NoResString)"application/octet-stream";
			Response.Headers.ContentDisposition = $"attachment;fileName={UrlEncoder.Default.Encode(fileName)}";
			ZGuid.TryParse(bizPK, out var bizId);
			ZGuid.TryParse(docPK, out var docId);
			var strogeMain = documentFactory.GetStorageMainForPK(bizId);
			var eDoc = (StorageDocsBase)strogeMain.eDocs.FindByPK(docId);
			eDoc.SaveToStream(Response.Body);
			Response.Body.Flush();
		}
	}
}
