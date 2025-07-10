using System;
using System.Net.Mime;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace CargoWise.RefDbRepo.NewService.UploadDownloadService
{
	[ApiController]
	public class UploadController : ControllerBase
	{
		readonly ISourceDataProvider _sourceDataProvider;

		public UploadController(ISourceDataProvider sourceDataProvider)
		{
			Argument.NotNull(sourceDataProvider, nameof(sourceDataProvider));
			_sourceDataProvider = sourceDataProvider;
		}

		[HttpGet]
		[Route("/")]
		public IActionResult View()
		{
			var fileContents = System.IO.File.ReadAllText("index.html");
			return new ContentResult
			{
				Content = fileContents,
				ContentType = "text/html"
			};
		}

		static bool ValidateSendParams(string source, string fileType, string contentType, string contacts)
		{
			if (!string.IsNullOrEmpty(contacts))
			{
				var split = contacts.Split(';');
				foreach (var email in split)
				{
					if (!EmailValidator.IsValidEmail(email))
					{
						return false;
					}
				}
			}

			return source == "UPL"
				&& (fileType == "COM" || fileType == "XML")
				&& contentType == "URD";
		}

		[HttpPost]
		[Route("[controller]/[action]")]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public async Task<IActionResult> Send(string source, string fileType, string contentType, string contacts)
		{
			contacts = contacts ?? string.Empty;
#pragma warning disable CA1308 // Normalize strings to uppercase
			contacts = contacts.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

			if (!ValidateSendParams(source, fileType, contentType, contacts))
			{
				return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
			}

#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;
				var reader = new MultipartReader(boundary, Request.Body);
				var section = await reader.ReadNextSectionAsync();
				var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
				if (hasContentDispositionHeader)
				{
					var fileName = !string.IsNullOrWhiteSpace(contentDisposition.FileName.Value) ? contentDisposition.FileName.Value : "NoName";
					fileName = fileName.Replace("\"", string.Empty);
					if (!FileNameValidator.IsValid(fileName))
					{
						return new ObjectResult("file name is invalid.") { StatusCode = StatusCodes.Status412PreconditionFailed };
					}
					await _sourceDataProvider.UploadFileToServer(source, fileType, contentType, contacts, fileName, section.AsFileSection());
				}
				return new StatusCodeResult(StatusCodes.Status201Created);
			}
			catch (Exception ex)
			{
				return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		[HttpGet]
		[Route("[controller]/[action]")]
		[Authorize(AuthenticationSchemes = AuthType.CombinedAuth)]
		public async Task<IActionResult> GetFile(Guid sourceDataPk)
		{
			var (fileName, fileContent) = await _sourceDataProvider.GetFileFromServer(sourceDataPk);

			if (string.IsNullOrEmpty(fileName) || fileContent == null)
			{
				return NotFound();
			}

			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(fileName, out var contentType))
			{
				contentType = MediaTypeNames.Application.Octet;
			}

			return File(fileContent, contentType, fileName);
		}
	}
}
