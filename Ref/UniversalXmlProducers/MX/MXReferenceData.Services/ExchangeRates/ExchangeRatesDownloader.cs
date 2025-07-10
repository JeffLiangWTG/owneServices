using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentFTP;
using FluentFTP.Exceptions;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
{
	public class ExchangeRatesDownloader : BaseDownloader
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
				foreach (var fileName in ConfigurationProvider.ExchangeRate.FileNamesToDownload)
				{
					if (client.DownloadBytes(out var outBytes, ConfigurationProvider.ExchangeRate.DownloadPath + fileName))
					{
						using (var streamRead = new MemoryStream(outBytes.Reverse().ToArray()))
						{
							xmls.Add(fileName, XMLReverter.RevertAndUnzipXML(streamRead));
						}
					}
				}
			}
			else
			{
				throw new FtpAuthenticationException("404", $"Unable to download from the following host: {ConfigurationProvider.ExchangeRate.Host}");
			}
			return xmls;
		}
	}
}
