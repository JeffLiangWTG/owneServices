using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Web.WebService.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Exceptions;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Summary description for BaseService
	/// </summary>
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public abstract class BaseService : System.Web.Services.WebService
	{
		public SecuritySOAPHeader SecurityHeader;

		#region AllowedToRunService

		protected bool AllowedToRunService(WebServiceResponse response, bool requireValidateConcurencyLogin = true)
		{
			if (SecurityHeader == null)
			{
				response.Error = ErrorTypes.LoginFailed;
				response.ErrorMessage = Res.GetString("8eee4499-bf54-4286-bb6f-2947ba1c2f43", "Invalid request. Security Header is not provided.");
			}
			else if (!SecurityHeader.IsAndroidDevice
				&& ZDateTime.UtcNow > WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.Value)
			{
				response.Error = ErrorTypes.LoginFailed;
				response.ErrorMessage = Res.GetString("b0b118dc-5722-438c-b79a-716f14b7cc16", "Windows CE mobile devices are no longer supported. Please migrate to Android solutions to continue supporting Warehouse.RF operations in your warehouse.");
			}
			else if ((SecurityHeader.IsAndroidDevice && WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.Value && SecurityHeader.DeviceVersion != AndroidWebServiceVersion)
				|| (!SecurityHeader.IsAndroidDevice && SecurityHeader.DeviceVersion != WinCEWebServiceVersion))
			{
				response.Error = ErrorTypes.UpgradeRequired;
				response.ErrorMessage = Res.GetString("b486309c-2119-4bb3-ab81-b67443cdda24", "Device version and System version does not match. Upgrade required.");
			}
			else
			{
				AllowedToRunServiceCore(response, requireValidateConcurencyLogin);
			}

			return response.Error == ErrorTypes.None;
		}

		protected virtual void AllowedToRunServiceCore(WebServiceResponse response, bool requireValidateConcurencyLogin = true)
		{
		}

		#endregion

		#region WebServiceVersion

		static Lazy<string> WebServiceVersionCache { get; } = new Lazy<string>(GetWebServiceVersion);

		static string GetWebServiceVersion()
		{
			return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		}

		protected string WebServiceVersion => WebServiceVersionCache.Value;

		#endregion

		#region WinCEWebServiceVersion

		static Lazy<string> WinCEWebServiceVersionCache { get; } = new Lazy<string>(GetWinCEWebServiceVersion);

		static string GetWinCEWebServiceVersion()
		{
			var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var winCEAssembly = Assembly.LoadFrom(Path.Combine(executingDirectory, "Enterprise.Warehouse.RF.Core.dll"));

			var versionAttribute = winCEAssembly
				.GetCustomAttributes()
				.FirstOrDefault(a => a.GetType().Name.Contains("AssemblyFileVersionAttribute"));

			return (string)versionAttribute?.GetType().GetProperty("Version").GetValue(versionAttribute)
				?? throw new InvalidOperationException("Unable to determine local WinCE version");
		}

		internal string WinCEWebServiceVersion => WinCEWebServiceVersionCache.Value;

		#endregion

		#region AndroidWebServiceVersion

		static Lazy<string> AndroidWebServiceVersionCache { get; } = new Lazy<string>(GetAndroidWebServiceVersion);

		const string AndroidVersionFileName = "Warehouse.RF.Version.txt";

		static string GetAndroidWebServiceVersion()
		{
			var androidVersionReader = ObjectFactory.Get<IRFVersionSupporter>();

			var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var androidFilePath = Path.Combine(executingDirectory, AndroidVersionFileName);

			return androidVersionReader.GetAndroidWebServiceVersion(androidFilePath);
		}

		internal string AndroidWebServiceVersion => AndroidWebServiceVersionCache.Value;

		#endregion

		#region HandleException

		protected void HandleException(Exception ex)
		{
			ErrorReporter.ReportOnce(UnHandleExceptionReportMessage + " " + ex.Message, ex);
			throw new SoapException(ex.Message, SoapException.ClientFaultCode, GetExceptionActor(ex));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is not visible to the user")]
		protected const string UnHandleExceptionReportMessage = "An unhandled exception occurred while processing warehouse web requests.";

		protected string GetExceptionActor(Exception exception)
		{
			var result = SoapExceptionActors.Codes.SystemException;

			if (exception is WebServiceLoginException)
			{
				result = SoapExceptionActors.Codes.LoginException;
			}
			else if (exception is WebServiceException)
			{
				result = SoapExceptionActors.Codes.BusinessException;
			}

			return result;
		}

		#endregion

		#region HandleWebServiceRequest

		protected T HandleWebServiceRequest<T>(Action<T> action)
			where T : WebServiceResponse, new()
		{
			return HandleWebServiceRequest(action, (T t) => AllowedToRunService(t));
		}

		protected T HandleWebServiceRequest<T>(Action<T> action, Func<T, bool> checkAllowedToRunService)
			where T : WebServiceResponse, new()
		{
			var result = new T();

			try
			{
				if (checkAllowedToRunService(result))
				{
					action(result);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);
			}

			return result;
		}

		#endregion
	}
}
