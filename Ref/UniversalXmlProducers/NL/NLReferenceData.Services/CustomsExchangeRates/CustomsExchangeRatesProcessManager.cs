using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class CustomsExchangeRatesProcessManager
	{
		public CustomsExchangeRatesProcessManager(IExchangeRatesBuilder<ExchangeRate> builder)
		{
			HttpClientHelper = new HttpClientHelper();
			this.builder = builder;
		}

		readonly IExchangeRatesBuilder<ExchangeRate> builder;
		public IHttpClientHelper HttpClientHelper { get; set; }

		public void RunProcess(string outputPath, StringBuilder errorCollector, DateTime publicationDate)
		{
			try
			{
				var exchangeRates = GetSOAPResult(DateTime.Now, errorCollector);

				if (exchangeRates.Any())
				{
					builder.BuildXml(publicationDate, exchangeRates, outputPath);
				}
				else
				{
					errorCollector.AppendLine("Downloaded file does not contain Exchange Rates.");
				}
			}
			catch (IOException ex)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failed. Exception: {ex.GetBaseException().Message}");
			}
			catch (ProcessingException ex)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{ApplicationConfig.ExchangeRatesSoapServiceUrl}' Exception: {ex.GetBaseException().Message}");
			}
			catch (Exception ex)
			{
				throw new ProcessingException("Processing failed", ex);
			}
		}

		public static string CreateRequest(DateTime requestDate)
		{
			string dateTimeString = requestDate.ToString(DateFormat, CultureInfo.InvariantCulture);

			XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
			XNamespace ns = "https://exchangerate.douane.nl/ExchangeRateInformation/2014/01/30/";
			XNamespace urn = "urn:wco:datamodel:WCO:RequestForExchangeRateInformation:1";
			var request = new XDocument(
					new XElement(soapenv + "Envelope",
						new XElement(soapenv + "Header"),
						new XElement(soapenv + "Body",
							new XElement(ns + "ExchangeRate",
								new XElement(urn + "Declaration",
									new XElement(urn + "FunctionCode", "91"),
									new XElement(urn + "AdditionalInformation",
										new XElement(urn + "RequestedInspectionDateTime", dateTimeString)
										)
									)
							)
						)
					)
				);
			return request.ToString();
		}

		public IEnumerable<ExchangeRate> GetSOAPResult(DateTime requestDate, StringBuilder errorCollector)
		{
			var soapRequest = CreateRequest(requestDate);
			var result = new List<ExchangeRate>();

			try
			{
				var soapCallResult = HttpClientHelper.PostAndReadAsAsyncString(ApplicationConfig.ExchangeRatesSoapServiceUrl, soapRequest, System.Net.Mime.MediaTypeNames.Text.Xml).GetAwaiter().GetResult();
				var parser = new CustomsExchangeRatesSOAPResponseParser(soapCallResult);
				var soapCallResults = parser.Parse();

				result.AddRange(soapCallResults);
			}
			catch (Exception e)
			{
				errorCollector.Append(CultureInfo.InvariantCulture, $"error getting details via SOAP for exhange rates on date {requestDate} exception: {e.GetBaseException().Message}");
				throw;
			}

			return result;
		}

		const string DateFormat = "yyyy-MM-ddTHH:mm:ss";
	}
}
