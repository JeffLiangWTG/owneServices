using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DpsExceptionHandler
	{
		public static void Process(Exception ex, bool suppressDeveloperException = false)
		{
			if (ex is AggregateException aggregateException)
			{
				ex = aggregateException.InnerException ?? ex;
			}

			Exception exceptionWithAdditionalInfo = null;

			if (ex is DpsCommunicationException retryFailureException)
			{
				exceptionWithAdditionalInfo = retryFailureException;
				ex = retryFailureException.InnerException ?? ex;
			}

			DeniedPartyScreenerAsync.WriteMessage(ex.ToString());

			var serviceUrl = GetServiceUrlsString();

			string errorNotToReport;

			if (ex is AuthCertNotFoundException)
			{
				errorNotToReport = ConstructExceptionMessage(GetAdministratorSupportMessage(), ex);
			}
			else
			{
				var errorsNotReport = new List<(string[] ExceptionMessages, bool IsInnerException, string ErrorMessage)>()
				{
					(new string[] { (NoResString)"The remote server returned an error: (407)" },
						true,
						Res.GetString("82BA6094-68C6-447E-8851-2AB0041CC730", "Error 407 Proxy Authentication Required. Please contact your network administrator and advise that access is required for {0}.", serviceUrl)),

					(new string[] { (NoResString)"The remote server returned an error: (403) Forbidden" },
						true,
						Res.GetString("55c16f93-18fd-469f-9334-a52a76dfbfcf", "Error 403 Forbidden. Please contact your network administrator and advise that correct ownership of content is granted for {0}.", serviceUrl)),

					(new string[] { (NoResString)"There was no endpoint listening at" },
						false,
						Res.GetString("0e5e4255-b27a-4c0c-8985-8449109c09b9", "There was no endpoint listening at {0} that could accept the message. This is often caused by an incorrect address or SOAP action.", serviceUrl)),

					(new string[] { (NoResString)"The requested service, '" },
						false,
						Res.GetString("32a1b7f7-aa9d-441f-bd3e-a94b53f7cb3a", "The requested service, '{0}' could not be activated. Please try the requested operation later.", serviceUrl)),

					(new string[] { (NoResString)"The maximum message size quota for incoming messages" },
					false,
					Res.GetString("77e4e107-5b43-4df5-8dbd-80f4b8d9e827", "The criteria you have specified result in too many Denied Party matches. Please restrict the screening options.")),

					(new string[]
					{
						(NoResString)"An error occurred while updating the entries",
						(NoResString)"An error occurred while executing the command definition",
						(NoResString)"An error occurred while receiving the HTTP response",
						(NoResString)"One or more errors occurred",
						(NoResString)"The underlying provider failed on Open"
					},
					false,
					GetCommunicationExceptionMessage())
				};

				errorNotToReport = errorsNotReport.Where(o =>
				{
					return (o.IsInnerException && o.ExceptionMessages.Any(m => ex.InnerException?.Message?.StartsWith(m, StringComparison.Ordinal) ?? false))
					|| o.ExceptionMessages.Any(m => ex.Message.StartsWith(m, StringComparison.Ordinal));
				}).Select(o => o.ErrorMessage).FirstOrDefault();
			}

			if (!string.IsNullOrEmpty(errorNotToReport))
			{
				if (!suppressDeveloperException)
				{
					Globals.Message.Show(errorNotToReport);
				}
				else
				{
					Globals.Message.Show(errorNotToReport + GetAdditionalExceptionInfo(exceptionWithAdditionalInfo));
				}
			}
			else if (ex.Message.StartsWith((NoResString)"The formatter threw an exception while trying to deserialize the message", StringComparison.Ordinal))
			{
				string supportMessage = Res.GetString("26a21997-79f1-4863-9be8-6871b16ea2b8", "Unable to retrieve Denied Party Status at this time. Please contact CargoWise support for assistance.");

				var errorMessage = ConstructExceptionMessage(supportMessage, exceptionWithAdditionalInfo ?? ex);
				ShowAndReportExceptionMessage(errorMessage, exceptionWithAdditionalInfo ?? ex, suppressDeveloperException);
			}
			else
			{
				var errorMessage = ConstructExceptionMessage(GetAdministratorSupportMessage(), exceptionWithAdditionalInfo ?? ex);
				ShowAndReportExceptionMessage(errorMessage, exceptionWithAdditionalInfo ?? ex, suppressDeveloperException);
			}
		}

		#region Implementation

		public static string GetServiceUrlsString()
		{
			var isInternal = ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();
			var webServices = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.Value.Cast<DpsWebServiceItem>();
			if (isInternal)
			{
				return string.Join(", ", webServices.Single(x => x.Role == RoleHelper.Code.Staging).WebServiceUrl);
			}
			else
			{
				return string.Join(", ", webServices.Where(x => x.Role != RoleHelper.Code.Staging).Select(r => r.WebServiceUrl));
			}
		}

		static string GetAdditionalExceptionInfo(Exception exceptionWithAdditionalInfo)
		{
			return exceptionWithAdditionalInfo == null ? string.Empty : System.Environment.NewLine + Res.GetString("0EEE8E12-C98C-484C-A904-9C21A80066CD", "Additional Exception Info: {0}", exceptionWithAdditionalInfo.Message);
		}

		static string ConstructExceptionMessage(string supportMessage, Exception ex)
		{
			string message = supportMessage;
			message += System.Environment.NewLine + System.Environment.NewLine;
			message += Res.GetString("46615444-1BEE-4B9D-811D-F28B6829FEB9", "Error details: {0}", ex.Message);

			return message;
		}

		static void ShowAndReportExceptionMessage(string message, Exception ex, bool suppressDeveloperException)
		{
			Globals.Message.Show(message);

			if (!suppressDeveloperException)
			{
				ExceptionReporter.Instance.ReportDeveloperException(message, ex);
			}

			if (EnvProxy.Instance.CurrentUser.IsDeveloper)
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		static MultilingualString GetCommunicationExceptionMessage()
		{
			return ResString.GetMultilingualString("c2e1950e-2cec-45d5-877a-55788df835f8", @"Unable to access the Denied Party Screening service on the CargoWise server.

Please try again. If the issue persists, please submit an eRequest.");
		}

		static string GetAdministratorSupportMessage()
		{
			return Res.GetString("e699363d-c549-4913-9551-f3c0b7bb77e5", "Unable to retrieve Denied Party Status at this time. Please contact your System Administrator for assistance.");
		}
		#endregion
	}
}
