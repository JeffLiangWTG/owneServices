using System;
using System.IO;
using System.Net;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Cus
{
	public static class DownloadDataTableContent
	{
		public static StringReader GetDataTableContent(Uri xmlUrl)
		{
			int retry = 0;
			WebException webException = null;

			while (retry < ApplicationConfig.Instance.MaxRetry)
			{
				try
				{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
					using (var webClient = new WebClient())
					{
						var xmlContent = new StringReader(webClient.DownloadString(xmlUrl));

						return xmlContent ?? throw new Exception($"Data table {xmlUrl} error.");
					}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
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
				Console.Error.WriteLine("Could not find CUS xml exchange rate file");
			}

			return null;
		}
	}
}
