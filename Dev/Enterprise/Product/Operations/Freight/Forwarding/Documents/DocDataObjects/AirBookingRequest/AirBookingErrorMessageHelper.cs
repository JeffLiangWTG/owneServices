using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class AirBookingErrorMessageHelper
	{
		public static string GetHumanReadableError(Exception exc)
		{
			switch (exc)
			{
				case TaskCanceledException _:
					return Res.GetString("91a908b0-29e6-49b8-b361-750f5b07e727", "eBookings service didn't respond within required time so the request could not be submitted.");

				case HttpRequestException httpRequestException:
					var requestErrorMessage = httpRequestException.InnerException?.Message
						?? httpRequestException.Message;

					if (FreightDataRegistry.Instance.ShowDetailedRequestFailureForEBookings.Value)
					{
						var requestBody = string.Empty;

						if (httpRequestException.InnerException is WebException webEx
							&& webEx.Response != null)
						{
							using (var reader = new StreamReader(webEx.Response.GetResponseStream()))
							{
								requestBody = reader.ReadToEnd();
							}
						}

						if (!string.IsNullOrWhiteSpace(requestBody))
						{
							requestErrorMessage = string.Join(requestErrorMessage, "\r\n", requestBody);
						}
					}
					return requestErrorMessage;

				case OnSavingCriticalCheckException onSavingCriticalCheckException:
					if (onSavingCriticalCheckException.BusinessEntity is BusinessObject bizObj)
					{
						return Res.GetString("b9630053-5428-4cdd-ac3b-1ee757664370", "Critical validation error has occurred while saving {0}. Please see details below:\r\n{1}", bizObj.HumanReadableName, onSavingCriticalCheckException.Message);
					}
					return onSavingCriticalCheckException.Message;

				default:
					return Res.GetString("1319de2a-0e35-4833-8599-0776d39c1ce8", "An error has occurred while processing the request.");
			}
		}
	}
}
