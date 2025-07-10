using System;
using System.Net;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Cud
{
	public static class DownloadEuExRate
	{
		public static XDocument GetEUBankExRateData(string xmlUrl)
		{
			XDocument retv;
			int retry = 0;
			WebException webException = null;

			while (retry < ApplicationConfig.Instance.MaxRetry)
			{
				try
				{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
					using (var webClient = new WebClient())
					{
						retv = XDocument.Load(xmlUrl);

						if (retv == null)
						{
							throw new Exception($"Failed to load data from {xmlUrl}.");
						}
					}
#pragma warning restore SYSLIB0014 // Type or member is obsolete

					return retv;
				}
				catch (WebException ex) // do not handle other exceptions here
				{
					webException = ex;
				}

				retry++;
			}

			if (webException != null)
			{
				Console.Error.WriteLine("Web Exception status : {0}", webException.Status);
				Console.Error.WriteLine($"Web Exception message as follows:");
				Console.Error.WriteLine(webException);
			}
			else
			{
				Console.Error.WriteLine("Could not find CUD xml exchange rate file");
			}

			return null;
		}
	}
}
