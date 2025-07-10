using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.Services.ServiceHost.WiseTechHealthCheck
{
	public sealed class WiseTechHealthCheckHttpHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContextBase context)
		{
			if (!string.Equals(context.Request.HttpMethod, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase))
			{
				context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
				return;
			}

			var checkDbResult = CheckDbInBackgound(TimeSpan.FromSeconds(10));
			var hasError = !checkDbResult.isOk;

			if (!hasError && RequestIsFromUntrustedNetwork(context.Request))
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				return;
			}

			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.CacheControl = "no-cache"; // HTTP Header - not localizable
			context.Response.ContentType = (NoResString)"text/plain;charset=utf-8"; // HTTP Header - not localizable

			var output = context.Response.OutputStream;
			using (var writer = new StreamWriter(output, Encoding.UTF8))
			{
				writer.Write(checkDbResult.statusMessage);
			}
		}

		/// <summary>
		/// Check DB is OK in a background thread.
		/// Give up if no response from SQL server in the given timeout.
		/// </summary>
		internal static (bool isOk, string statusMessage) CheckDbInBackgound(TimeSpan timeout)
		{
			var taskResult = RunCheckDbTask(timeout);
			return GetStatusFromCheckDbResult(taskResult.taskCompleted, taskResult.ex);
		}

		static (bool taskCompleted, Exception ex) RunCheckDbTask(TimeSpan timeout)
		{
			var task = Task.Run(() => CheckDb(timeout));
			return task.Wait(timeout)
				? (true, task.Result)
				: (false, null);
		}

		internal static (bool isOk, string statusMessage) GetStatusFromCheckDbResult(bool taskCompleted, Exception ex)
		{
			string statusMessage;
			bool isOk;
			if (taskCompleted)
			{
				if (ex != null)
				{
					isOk = false;
					statusMessage = GetStatusMessageFromException(ex);
				}
				else
				{
					isOk = true;
					statusMessage = (NoResString)"INFO(Database): OK"; // no need to be localizable
				}
			}
			else
			{
				isOk = false;
				statusMessage = (NoResString)"ERROR(Database): Database is not responding."; // no need to be localizable
			}

			return (isOk, statusMessage);
		}

		static string GetStatusMessageFromException(Exception ex)
		{
			if (ex is DatabaseUpgradedException)
			{
				return (NoResString)"INFO(Database): The Database has been upgraded and Web Application is being upgraded, please refresh the page in a few minutes."; // no need to be localizable
			}

			if (ex is DatabaseUpgradeInProgressException)
			{
				return (NoResString)"INFO(Database): The Database is currently being upgraded, please refresh the page in a few minutes."; // no need to be localizable
			}

			return (NoResString)"ERROR(Database): Cannot access database.\r\n" + ex.Message; // no need to be localizable
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal static Exception CheckDb(TimeSpan timeout)
		{
			try
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var timeoutSeconds = timeout == System.Threading.Timeout.InfiniteTimeSpan ? 0 : (int)timeout.TotalSeconds;
					using (connection.TemporarySetDefaultCommandTimeOut(timeoutSeconds))
					{
						connection.EnsureIsOpen();
					}
					return null;
				}
			}
			catch (Exception exception)
			{
				return exception;
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			ProcessRequest(new HttpContextWrapper(context));
		}

		public bool IsReusable => true;

		bool RequestIsFromUntrustedNetwork(HttpRequestBase request)
		{
			if (request.ServerVariables == null || !IPAddress.TryParse(request.ServerVariables["LOCAL_ADDR"], out IPAddress hostAddress)) // HTTP ServerVariables - not localizable
			{
				return true;
			}

			var address = HostAddress(request);

			if (!WebDataRegistry.Instance.HealthCheckAccessIPWhitelistRegistryItem.Value.Contains(address))
			{
				return true;
			}

			return false;
		}

		string HostAddress(HttpRequestBase request)
		{
			var userHostAddress = request?.UserHostAddress;
			if (userHostAddress == null || !IPAddress.TryParse(userHostAddress, out var _))
			{
				return string.Empty;
			}

			if (request.Headers == null || !request.Headers.AllKeys.Contains(XForwardedFor, StringComparer.OrdinalIgnoreCase))
			{
				return userHostAddress;
			}

			var values = request.Headers.GetValues(XForwardedFor);
			if (values == null || !values.Any())
			{
				return userHostAddress;
			}

			var xForwardedAddress = GetValidXForwardedForAddress(values.First(), userHostAddress);
			return string.IsNullOrEmpty(xForwardedAddress) ? userHostAddress : xForwardedAddress;
		}

		/// <summary>
		/// The general format of the field is:
		/// X-Forwarded-For: client, proxy1, proxy2
		/// (source: wikipedia, mozilla docs)
		/// </summary>
		string GetValidXForwardedForAddress(string xForwardedForString, string hostAddress)
		{
			if (string.IsNullOrEmpty(xForwardedForString) || string.IsNullOrEmpty(hostAddress))
			{
				return string.Empty;
			}

			var xForwardedAddress = xForwardedForString.Split(new char[] { ',' }, 2)[0];
			if (!IPAddress.TryParse(hostAddress, out var hostIP) ||
				!IPAddress.TryParse(xForwardedAddress, out var xForwardedIP) ||
				!IPAddressExtensions.AddressIsPrivate(hostIP))
			{
				return string.Empty;
			}

			return xForwardedAddress;
		}

		const string XForwardedFor = "X-Forwarded-For";
	}
}
