using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.XmlService;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public class WebContentParser
	{
		public WebContentParser(IWebResponseHandler webResponseHandler, string requestUrl)
		{
			_webResponseHandler = Argument.NotNull(webResponseHandler, nameof(webResponseHandler));
			_requestUrl = Argument.NotNullOrEmpty(requestUrl, nameof(requestUrl));
		}

		public WebContentParser(IXmlService xmlService, IWebResponseHandler webResponseHandler, string requestUrl, IScrappedRecordParser scrappedRecordParser, IPreferenceDataLookup preferenceDataLookup, ILogger logger) : this(webResponseHandler, requestUrl)
		{
			_xmlService = Argument.NotNull(xmlService, nameof(xmlService));
			_scrappedRecordParser = Argument.NotNull(scrappedRecordParser, nameof(scrappedRecordParser));
			_preferenceDataLookup = Argument.NotNull(preferenceDataLookup, nameof(preferenceDataLookup));
			_logger = Argument.NotNull(logger, nameof(logger));
		}

		public async Task GenerateXmlFromWebContent(IEnumerable<string> cusTariffCodes)
		{
			var dtobjects = await Task.WhenAll(cusTariffCodes.Select(async code => await ProcessTariffCode(code)));
			SerializeBusinessObjectsToUniversalXml(dtobjects);
		}

		public DateTime GetPublicationDate()
		{
			var response = _webResponseHandler.GetWebResponseForPublicationDateAsync(_requestUrl);
			if (response?.Result != null && response.Result.IsValidResponse)
			{
				return HtmlDataExtractor.HtmlDataExtractor.GetDataUpdatedOnDate(response.Result.ResponseMessage);
			}

			return MeasuresConstant.DefaultStartDate;
		}

		async Task<ParsingResult> ProcessTariffCode(string tariffCode)
		{
			try
			{
				var responseResult = await _webResponseHandler.GetWebRepsonseForTariffAsync(_requestUrl, tariffCode);

				if (!responseResult.IsValidResponse)
				{
					return new ParsingResult(tariffCode, responseResult.ResponseMessage);
				}

				var tariffDescription = HtmlDataExtractor.HtmlDataExtractor.GetTariffDescription(responseResult.ResponseMessage);
				if (string.IsNullOrEmpty(tariffDescription))
				{
					return new ParsingResult(tariffCode, ErrorMessagesConstant.NoTariffDescription);
				}

				var nationalInformationSection = HtmlDataExtractor.HtmlDataExtractor.GetNationalInformationSection(responseResult.ResponseMessage);
				if (string.IsNullOrEmpty(nationalInformationSection))
				{
					return new ParsingResult(tariffCode, ErrorMessagesConstant.NoNationalSection);
				}

				var parsingResult = ConvertHtmlToDto(tariffCode, tariffDescription, nationalInformationSection);
				return parsingResult;
			}
			catch (Exception exception)
			{
				_logger.Log(tariffCode, exception);
				return new ParsingResult(tariffCode, ErrorMessagesConstant.UnexpectedError);
			}
		}

		ParsingResult ConvertHtmlToDto(string tariffCode, string tariffDescription, string nationalSection)
		{
			var scrappedRecords = HtmlDataExtractor.HtmlDataExtractor.ScrapRecordsFromHtml(nationalSection);

			if (scrappedRecords == null)
			{
				return new ParsingResult(tariffCode, ErrorMessagesConstant.NoMeasureFound, nationalSection);
			}

			var refCusTariff = new RefCusTariff(tariffCode);

			foreach (var scrappedRecord in scrappedRecords.Where(r => r != null))
			{
				if (scrappedRecord.Measure?.Description == null)
				{
					return new ParsingResult(tariffCode, ErrorMessagesConstant.NoMeasureInScrappedRecord, nationalSection);
				}

				if (MeasuresConstant.DescriptionsToIgnore.Any(scrappedRecord.Measure.Description.Contains))
				{
					continue;
				}

				if (ConditionConstants.conditionTypeDictionary.Keys.Any(x => x.StartsWith(scrappedRecord.Measure.Description, StringComparison.OrdinalIgnoreCase)))
				{
					CertificateData certificateData = null;
					var certificateDatas = new List<CertificateData>();
					if (!string.IsNullOrEmpty(scrappedRecord.Measure.Formula) && scrappedRecord.Measure.Formula.StartsWith("Certificato", StringComparison.InvariantCulture))
					{
						var responseResult = _webResponseHandler.GetWebRepsonseForCertificateLinkAsync(_requestUrl, scrappedRecord.RequirementHtml)?.GetAwaiter().GetResult();
						if (responseResult != null && responseResult.ResponseMessage != null)
						{
							var certificateLinks = HtmlDataExtractor.HtmlDataExtractor.GetCertificateParameterString(responseResult.ResponseMessage);
							foreach (var certificateParameter in certificateLinks)
							{
								if (!string.IsNullOrEmpty(certificateParameter))
								{
									var certificateParameterElements = certificateParameter.Split(',').Select(x => x.Replace("'", string.Empty).Trim()).ToList();
									var certificateCacheKey = certificateParameterElements[4] + certificateParameterElements[5]; //take 4th and fifth elements as key example ("5, 1, -2, 100, 'C', '678', '14/12/2019'") = C678
									if (!((WebResponseHandler)_webResponseHandler).cachedCertificateData.ContainsKey(certificateCacheKey))
									{
										var responseResultCertificate = _webResponseHandler.GetWebResponseForCertificateAsync(_requestUrl, certificateParameter, scrappedRecord.RequirementHtml, tariffCode)?.GetAwaiter().GetResult();
										if (responseResultCertificate != null && !string.IsNullOrEmpty(responseResultCertificate.ResponseMessage))
										{
											certificateData = HtmlDataExtractor.HtmlDataExtractor.GetCertificateData(responseResultCertificate.ResponseMessage);
										}
										((WebResponseHandler)_webResponseHandler).cachedCertificateData.Add(certificateCacheKey, certificateData);
									}
									certificateDatas.Add(((WebResponseHandler)_webResponseHandler).cachedCertificateData[certificateCacheKey]);
								}
							}
						}
					}
					var sortedCertificateDatas = certificateDatas.OrderBy(x => x.CertificateNumber).ToList(); //ensure output is deterministic irrespective of certificate order on website
					var conditionResult = _scrappedRecordParser.GetRefCusConditions(scrappedRecord, refCusTariff, sortedCertificateDatas);
					if (conditionResult != null)
					{
						return conditionResult;
					}
					else
					{
						continue;
					}
				}

				if (scrappedRecord.Measure.Description.Contains(MeasuresConstant.ValueAddedTax))
				{
					var vatParsingResult = _scrappedRecordParser.GetRefCusVatApplicabilities(scrappedRecord, refCusTariff);
					if (vatParsingResult != null)
					{
						return vatParsingResult;
					}
					continue;
				}

				var rateParsingResult = _scrappedRecordParser.GetRefCusRates(scrappedRecord, nationalSection, refCusTariff, GetRateGenerationStrategy(tariffDescription));
				if (rateParsingResult != null)
				{
					return rateParsingResult;
				}
			}

			var result = new ParsingResult(tariffCode, refCusTariff);
			return result;
		}

		void SerializeBusinessObjectsToUniversalXml(IEnumerable<ParsingResult> results)
		{
			Argument.NotNull(results, nameof(results));

			foreach (var parsingResult in results.Where(r => r != null))
			{
				if (parsingResult.IsValid)
				{
					_xmlService.SerializeThenAppend(parsingResult.Content);
				}
				else
				{
					_xmlService.SerializeThenAppend(new RefCusTariff(parsingResult.TariffCode));

					if (parsingResult.IsCriticalError)
					{
						_logger.Log(parsingResult);
					}
				}
			}
		}

		IRateGenerationStrategy GetRateGenerationStrategy(string tariffDescription)
		{
			if (tariffDescription.Contains(NoteA148))
			{
				return new NoteA148RateGenerationStrategy(_preferenceDataLookup);
			}

			return new DefaultRateGenerationStrategy();
		}

		readonly IXmlService _xmlService;
		readonly IWebResponseHandler _webResponseHandler;
		readonly string _requestUrl;
		readonly IScrappedRecordParser _scrappedRecordParser;
		readonly IPreferenceDataLookup _preferenceDataLookup;
		readonly ILogger _logger;

		const string NoteA148 = "A148";
	}
}
