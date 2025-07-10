using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Module
{
	public static class ReportingUriHelper
	{
		public static Uri GetPerformanceReportingUri(string path = "login", bool isLayoutHidden = false)
		{
			var performanceReportingUrlGenerator = ObjectFactory.Get<IPerformanceReportingUrlGenerator>();

			var token = performanceReportingUrlGenerator.GetToken(ZGuid.NewZGuid().ToString());

			if (!string.IsNullOrEmpty(token.ErrorMessage))
			{
				string tokenErrorMessage = token.ErrorMessage;

				if (!tokenErrorMessage.EndsWith("."))
				{
					tokenErrorMessage += ".";
				}
				Globals.Message.ShowError(
					Res.GetString(
						"CE376653-CDF5-4868-8C4A-5A722405F9DC",
						"While attempting to create an authentication token for global tracking performance reports the following error was encountered. {0}",
						tokenErrorMessage
					)
				);
				return null;
			}

			var (url, errorMessage) = performanceReportingUrlGenerator.Generate(path, token.Token, isLayoutHidden);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.ShowError(Res.GetString("8C9718A3-4A07-4FD3-8E8F-91F3025A86B2", "Unable to create performance report URL : {0}", errorMessage));
				return null;
			}

			return url;
		}
	}
}
