using System;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class ErrorReportController : ControllerBase
	{
		[HttpPost]
		public IActionResult ReportError(WebError error)
		{
			Argument.NotNull(error, nameof(error));
			Exception innerException = null;
			if (!string.IsNullOrEmpty(error.ErrorEventJsonString) && error.ErrorEventJsonString != "{}")
			{
				var jsonContent = JsonConvert.DeserializeObject(error.ErrorEventJsonString).ToString();
				jsonContent = error.Message + "\n" + jsonContent;
				innerException = new InvalidOperationException(jsonContent);
			}
			var exception = new JavascriptException(error.Message, error.StackTrace, innerException);

			using (var errorReporter = new ErrorReportingClientWrapper())
			{
				errorReporter.PostCrashReport(exception, null, null);
			}

			return Ok();
		}
	}
}
