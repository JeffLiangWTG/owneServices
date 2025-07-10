using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;
using WinzorFramework.JSInterop;

namespace CargoWise.Winzor.AppServer
{
	public class DownloadController : ControllerBase
	{
		[HttpGet]
		[Route("/download/{objectId}")]
		public async Task DownLoadUpgradesAsync(
			string objectId,
			[FromServices] IFileService fileService)
		{
			Response.ContentType = (NoResString)"application/octet-stream";
			var result = fileService.GetAndRemoveDownloadObject(objectId, out var downloadObject);
			if (result == ResultOfGetDownloadObject.NotExist)
			{
				Response.StatusCode = 404;
				return;
			} else
			{
				Response.Headers.ContentDisposition = $"attachment;fileName={UrlEncoder.Default.Encode(downloadObject.Name)}";
				var cts = HttpContext.RequestAborted;

				await Response.StartAsync(cts);
				await downloadObject.DownloadAsync(Response.Body, cts);
				await Response.CompleteAsync();
			}
		}
	}
}
