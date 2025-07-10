using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentFTP;
using FluentFTP.Exceptions;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
{
	public class TariffRatesDownloader : BaseDownloader
	{
		public static Dictionary<string, string> DownloadXmls(IFtpClient client)
		{
			var xmls = new Dictionary<string, string>();

			try
			{
				client.Connect();
			}
			catch (FtpAuthenticationException)
			{
				throw;
			}

			if (client.IsConnected)
			{
				foreach (var fileName in ConfigurationProvider.TariffRate.FileNamesToDownload)
				{
					if (client.DownloadBytes(out var outBytes, ConfigurationProvider.TariffRate.DownloadPath + fileName))
					{
						xmls.Add(fileName, System.Text.Encoding.UTF8.GetString(outBytes.ToArray()));
					}
				}
			}
			else
			{
				throw new FtpAuthenticationException("404", $"Unable to download from the following host: {ConfigurationProvider.TariffRate.Host}");
			}
			return xmls;
		}
	}
}
