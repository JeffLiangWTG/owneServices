using System;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class ErrorReportingClientWrapper : IErrorReportingClientWrapper
	{
		public ErrorReportingClientWrapper()
		{
			isFromRDU = true;
		}

		public ErrorReportingClientWrapper(bool isFromRDU)
		{
			this.isFromRDU = isFromRDU;
		}

		readonly bool isFromRDU;

		public virtual bool PostCrashReport(Exception ex)
		{
			return PostCrashReport(ex, Environment.MachineName);
		}

		public bool PostCrashReport(Exception exception, string machineName)
		{
			var result = ShouldReport(exception, machineName);
			if (result)
			{
				ErrorReporter.ReportOnce(exception.Message, new RefDataException(exception));
			}
			return result;
		}

		public bool ShouldReport(Exception ex, string machineName)
		{
			if (RemoteDatabaseRegistry.Instance.EnableIssueReport.Value)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(machineName))
			{
				if (!MachineNameHelper.ShouldErrorBeLogged(machineName))
				{
					return false;
				}
			}

			if (isFromRDU)
			{
				var serviceUri = RemoteDatabaseRegistry.Instance.ServerUri;
				var sRDbName = RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.Value;
				if ((!serviceUri.Equals(RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.DefaultValue, StringComparison.OrdinalIgnoreCase))
					|| (!sRDbName.Equals(RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.DefaultValue, StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}
			}
			else
			{
				var serviceUri = RefDataRepoRegistry.Instance.ServerUri;
				if (!serviceUri.Equals(RefDataRepoRegistry.Instance.RefDbRepoServiceUri.DefaultValue, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			var currentException = ex;
			while (currentException != null)
			{
				if (currentException is HttpRequestException || (currentException is RefApplicationException refException && !refException.ReportIssue))
				{
					return false;
				}

				var exceptionMessage = currentException.Message;
				if (!string.IsNullOrEmpty(exceptionMessage))
				{
					foreach (var avoidMessage in ErrorReportingKnownExceptionsMapping.IgnoredExceptionMessages)
					{
						if (exceptionMessage.Contains(avoidMessage))
						{
							return false;
						}
					}
				}
				currentException = currentException.InnerException;
			}
			return true;
		}
	}
}
