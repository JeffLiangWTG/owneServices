using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Web.ExceptionHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewService.Logging
{
	public static class ApiExceptionHandlerExtensions
	{
		public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app)
		{
			return app.UseCommonApiExceptionHandler(false, (context, apiLogException) =>
			{
				var errorReportWrapper = context.RequestServices.GetRequiredService<ErrorReportingClientWrapper>() ?? throw new InvalidOperationException("Fail to get ErrorReportingClientWrapper.");
				var ignore = false;
				var exception = apiLogException.Exception;
				while (exception != null)
				{
					if (IgnoredErrorCodes.Contains(exception.HResult))
					{
						ignore = true;
					}
					if (ignore)
					{
						break;
					}
					exception = exception.InnerException;
				}
				if (!ignore)
				{
					errorReportWrapper.PostCrashReport(apiLogException.Exception, "Delivery Service", null);
				}
			});
		}

		//HttpException.ErrorCode = -2147023667 or -2147024809 or -2147024890 - The remote host closed the connection. The error code is 0x800704CD.
		//HttpException.ErrorCode = -2147023901 or  -2147467259  - An error occurred while communicating with the remote host. The error code is 0x800703E3.
		//HttpException.ErrorCode = -2147467259  - An error occurred while communicating with the remote host. The error code is 0x800704CD.
		//HttpException.ErrorCode = -2147024874 or  -2147467259  - An error occurred while communicating with the remote host. The error code is 0x80070016.
		//HttpException.ErrorCode = -2147023667 - An error occurred while communicating with the remote host. The error code is 0x800704CD.
		//HttpException.ErrorCode = -2147024874 or  -2147467259  - The remote host closed the connection. The error code is 0x80070016.
		//HttpException.ErrorCode = -2147024874 - The underlying provider failed on Open. The error code is 0x80131501
		//DbException -2146232060 : A network-related or instance-specific error occurred while establishing a connection to SQL Server
		//ConnectionResetException -2147024832: The client has disconnected. The specified network name is no longer available. (0x80070040)
		//TaskCanceledException -2146233029: A task was canceled.
		static int[] IgnoredErrorCodes = new[] { -2147023667, -2147023901, -2147024874, -2146233087, -2147467259, -2147024809, -2147024890, -2146232060, -2147024832, -2146233029 };
	}
}
