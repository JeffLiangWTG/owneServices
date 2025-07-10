using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public abstract class FiscalExchangeRatesProcessManager
	{
		public void RunProcess(string outputPath, StringBuilder errorCollector, DateTime publicationDate)
		{
			try
			{
				var builder = GetFiscalExchangeRatesBuilder();
				var downloadLink = ApplicationConfig.DownloadUrlFiscalExchangeRates;

				try
				{
					var downloadManager = GetDownloadManager();
					var downloadedXml = downloadManager.DownloadFileAsXmlDocument(downloadLink);
					var fiscalExchangeRates = ReadFiscalExchangeRates(downloadedXml);

					if (fiscalExchangeRates.Count == 0)
					{
						errorCollector.AppendLine("Downloaded file does not contain Fiscal Exchange Rates.");
					}
					else
					{
						builder.BuildXml(publicationDate, fiscalExchangeRates, outputPath);
					}
				}
				catch (IOException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failed. Exception: {ex.GetBaseException().Message}");
				}
				catch (ProcessingException ex)
				{
					errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{downloadLink}' Exception: {ex.GetBaseException().Message}");
				}
			}
			catch (Exception ex)
			{
				throw new ProcessingException("Processing failed", ex);
			}
		}

		public static List<RateNode> ReadFiscalExchangeRates(XmlDocument downloadedXml)
		{
			var fiscalExchangeRates = new List<RateNode>();
			var xmlNamespaceManager = new XmlNamespaceManager(downloadedXml.NameTable);

			xmlNamespaceManager.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
			xmlNamespaceManager.AddNamespace("lo", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
			var rateNodes = downloadedXml.SelectSingleNode("//lo:Cube[@time]", xmlNamespaceManager);

			if (rateNodes != null)
			{
				var rateDate = rateNodes.Attributes["time"]?.Value;
				foreach (XmlNode rate in rateNodes.ChildNodes)
				{
					var exRate = rate.Attributes["rate"]?.Value;
					var parsedRate = decimal.Zero;
					if (decimal.TryParse(exRate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsedRate))
					{
						var rateNode = new RateNode
						{
							Currency = rate.Attributes["currency"]?.Value,
							Rate = parsedRate
						};
						fiscalExchangeRates.Add(rateNode);
					}
				}
			}
			else
			{
				throw new ProcessingException("Downloaded file is not a valid xml for Fiscal Exchange Rates");
			}

			return fiscalExchangeRates;
		}

		public abstract IExchangeRatesBuilder<RateNode> GetFiscalExchangeRatesBuilder();
		public abstract DownloadManager GetDownloadManager();
	}

	public class FiscalExchangeRatesWebClientProcessManager : FiscalExchangeRatesProcessManager
	{
		public FiscalExchangeRatesWebClientProcessManager(IExchangeRatesBuilder<RateNode> fiscalExchangeRatesBuilder)
		{
			FiscalExchangeRatesBuilder = fiscalExchangeRatesBuilder;
		}
		readonly IExchangeRatesBuilder<RateNode> FiscalExchangeRatesBuilder;

		public override IExchangeRatesBuilder<RateNode> GetFiscalExchangeRatesBuilder() => FiscalExchangeRatesBuilder;
		public override DownloadManager GetDownloadManager() => new DownloadManager(new WebClientWrapper());
	}
}
