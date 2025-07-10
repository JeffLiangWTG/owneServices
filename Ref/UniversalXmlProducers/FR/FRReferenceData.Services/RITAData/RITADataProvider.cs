using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RITADataProvider
	{
		public RITADataProvider(RITADataDownloader ritaDataDownloader)
		{
			RitaDataDownloader = ritaDataDownloader;
		}

		public async Task DownloadFRTariffRegulation(string tariff)
		{
			List<string> messageTypes = new List<string> { importMeasure, importCondition, exportMeasure, exportCondition };

			foreach (string messageType in messageTypes)
			{
				var outputXmlText = BuildRequest(tariff, messageType);
				var responseContent = string.Empty;

				var currentAttempt = 0;
				while (string.IsNullOrEmpty(responseContent) || responseContent.Contains("TypeMessage>ERT</"))
				{
					currentAttempt ++;

					if (currentAttempt > ApplicationConfig.Instance.MaxAttemptsCountInCaseOfTechnicalError)
					{
						var techErrorDescription = RITADataParser.GetErrorFromWebServiceResponse(responseContent);
						throw new RITAWebServiceException($"Downloaded file {tariff}_{messageType}.XML reported a technical error: {techErrorDescription}");
					}
					else if (currentAttempt > 1)
					{
						Console.WriteLine($"Downloaded file {tariff}_{messageType}.XML was corrupted. Let's have a pause and try again...");
						Thread.Sleep(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError);
					}

					responseContent = await RitaDataDownloader.PostRequestAndGetResponse(outputXmlText);
				}

				if (responseContent.Contains("TypeMessage>ERM<"))
				{
					var functionalErrorDescription = RITADataParser.GetErrorFromWebServiceResponse(responseContent);
					throw new RITAWebServiceException($"Downloaded file {tariff}_{messageType}.XML reported a functional error: {functionalErrorDescription}");
				}

				responseContent = ResponseReshape(responseContent);
				if (string.IsNullOrEmpty(responseContent))
				{
					throw new RITAWebServiceException($"Downloaded file {tariff}_{messageType}.XML is Empty.");
				}
				else
				{
					ResponseToFile(tariff, responseContent, messageType);
				}
			}

			Thread.Sleep(ApplicationConfig.Instance.MinDelayBetweenRequestsToRITA);
		}

		public static string BuildRequest(string code, string messageType)
		{
			string outputText;

			if (messageType == importMeasure)
			{
				outputText = File.ReadAllText(ApplicationConfig.Instance.WebServiceMeasureRequestTemplate);
				outputText = outputText.Replace("%TARIFF%", code);
				outputText = outputText.Replace("%DIRECTION%", "0");
			}
			else if (messageType == importCondition)
			{
				outputText = File.ReadAllText(ApplicationConfig.Instance.WebServiceConditionRequestTemplate);
				outputText = outputText.Replace("%TARIFF%", code);
				outputText = outputText.Replace("%DIRECTION%", "0");
			}
			else if (messageType == exportMeasure)
			{
				outputText = File.ReadAllText(ApplicationConfig.Instance.WebServiceMeasureRequestTemplate);
				outputText = outputText.Replace("%TARIFF%", code);
				outputText = outputText.Replace("%DIRECTION%", "1");
			}
			else if (messageType == exportCondition)
			{
				outputText = File.ReadAllText(ApplicationConfig.Instance.WebServiceConditionRequestTemplate);
				outputText = outputText.Replace("%TARIFF%", code);
				outputText = outputText.Replace("%DIRECTION%", "1");
			}
			else if (messageType == nomenclature)
			{
				outputText = File.ReadAllText(ApplicationConfig.Instance.WebServiceNomenclatureRequestTemplate);
				outputText = outputText.Replace("%CHAPTER%", code);
			}
			else
			{
				outputText = "";
			}

			outputText = outputText.Replace("%DATE%", DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
			return outputText;
		}

		static string ResponseReshape(string responseContent)
		{
			responseContent = HttpUtility.HtmlDecode(responseContent);
			responseContent = responseContent.Replace("<![CDATA[", "");
			responseContent = responseContent.Replace("]]>", "");
			responseContent = responseContent.Replace("<BR>", Environment.NewLine);
			responseContent = responseContent.Replace("<br>", Environment.NewLine);
			responseContent = responseContent.Replace("<P>", Environment.NewLine + "- ");
			responseContent = responseContent.Replace("&", "+");
			responseContent = responseContent.Replace("<=", "inférieur ou égal à ");
			responseContent = responseContent.Replace(" > ", " supérieur à ");
			responseContent = responseContent.Replace(" < ", " inférieur à ");
			responseContent = responseContent.Replace("<titre", "(titre");
			responseContent = responseContent.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");

			return responseContent;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception will be treated in a report.")]
		static void ResponseToFile(string tariffCode, string responseContent, string messageType)
		{
			string responseFileFullPath = Path.Combine(ApplicationConfig.Instance.DownloadDirectory, tariffCode + "_" + messageType + ".XML");
			try
			{
				File.WriteAllText(responseFileFullPath, responseContent);
			}
			catch (Exception e)
			{
				Console.WriteLine("KO : " + e.Message);
				return;
			}
		}

		public string[] GetUpdatedOrNewFRTariffsThatDay(DateTime thatDateTime)
		{
			var thatDate = thatDateTime.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).Replace(" ", "%2F").Replace("-", "%2F");

			var updatedFRTariffs = new List<string>();
			string[] chapterList = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.TemplateFolder, "ChapterList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			var groupedChapterList = GetGroupedChapters(thatDateTime, 1000);
			groupedChapterList.ForEach(groupedChapters => DomainsToCheckForUpdate.ForEach(domain => updatedFRTariffs.AddRange(GetUpdatedFRTariffs(groupedChapters, thatDate, thatDate, domain))));

			updatedFRTariffs.AddRange(GetTariffsDeclarableChildren(updatedFRTariffs, thatDateTime));

			return updatedFRTariffs.OrderBy(x => x).Distinct().ToArray();
		}

		public static List<string> DomainsToCheckForUpdate => new List<string>() { taxationDomain, antiDumpingDomain, customsDutiesDomain, prohibitionDomain, domFeesDomain, statisticsDomain };

		List<string> GetTariffsDeclarableChildren(List<string> updatedFRTariffs, DateTime dateTime)
		{
			var result = new List<string>();

			var euDeclarableTariffs = GetEUDeclarableTariffsThatDay(dateTime);
			foreach (var updatedFRTariff in updatedFRTariffs)
			{
				result.AddRange(GetTariffDeclarableChildren(updatedFRTariff, euDeclarableTariffs));
			}

			return result;
		}

		static List<string> GetTariffDeclarableChildren(string tariff, List<string> tariffs)
		{
			var result = new List<string>();

			var root = GetTariffRoot(tariff);
			result = tariffs.Where(x => x.StartsWith(root, StringComparison.InvariantCulture)).ToList();

			return result;
		}

		public static string GetTariffRoot(string tariff)
		{
			var result = tariff;

			int[] suffixLengths = { 8, 6, 4, 2 };
			foreach (int length in suffixLengths)
			{
				if (tariff.EndsWith(new string('0', length), StringComparison.InvariantCulture))
				{
					result = tariff.Substring(0, 10 - length);
					break;
				}
			}

			return result;
		}

		public virtual List<string> GetEUDeclarableTariffsThatDay(DateTime thatDateTime)
		{
			if (cachedEuDeclarableTariffs == null || !cachedEuDeclarableTariffs.Any() || cachedEuTariffsQueryDate != thatDateTime)
			{
				cachedEuDeclarableTariffs = GetEUDeclarableTariffs(thatDateTime).Where(x => x.ZZ1_TariffCode.Length == 10).Select(x => x.ZZ1_TariffCode).OrderBy(x => x).Distinct().ToList();
				cachedEuTariffsQueryDate = thatDateTime;
			}

			return cachedEuDeclarableTariffs;
		}
		List<string> cachedEuDeclarableTariffs;
		DateTime cachedEuTariffsQueryDate;

		public virtual IEnumerable<RefCusTariff> GetEUDeclarableTariffs(DateTime thatDateTime)
		{
			IEnumerable<RefCusTariff> result = null;

			int attemptCount = 0;
			while (result == null && attemptCount <= 3 )
			{
				attemptCount++;
				try
				{
					result = new RefCusTariffLoader(new RefDataLoader()).GetEUDeclarableTariffsFromRefDB(thatDateTime).Result;
				}
				catch (Exception e)
				{
					if (attemptCount == 3)
					{
						throw new WebException($"Unable to get a list of EU tariffs from {ApplicationConfig.Instance.RefDbServiceURI}.", e);
					}
				}
			}

			return result;
		}

		public virtual IEnumerable<string> GetUpdatedFRTariffs(string[] groupedChapters, string startDate, string endDate, string domain)
		{
			var content = GetTariffUpdateWebPage(groupedChapters, startDate, endDate, domain);
			return RITADataParser.GetFRTariffsFromHTML(content);
		}

		public List<string[]> GetGroupedChapters(DateTime thatDateTime, int maxTariffCount)
		{
			var result = new List<string[]>();

			var tariffList = GetEUDeclarableTariffsThatDay(thatDateTime).Select(x => x.Substring(0, 2)).ToList();
			var tariffgroups = tariffList.GroupBy(x => x);

			var chapterTariffCountList = new List<ChapterTariffCount>();

			string[] chapters = File.ReadAllText(Path.Combine(ApplicationConfig.Instance.TemplateFolder, "ChapterList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var chapter in chapters)
			{
				var chapterTariffCount = tariffgroups.FirstOrDefault(x => x.Key == chapter)?.Count() ?? 0;
				chapterTariffCountList.Add(new ChapterTariffCount(chapter, chapterTariffCount));
			}

			foreach (var currentChapter in chapterTariffCountList)
			{
				if (!currentChapter.IsUsed)
				{
					var count = currentChapter.TariffCount;
					currentChapter.IsUsed = true;

					var groupedChapter = new List<string> { currentChapter.Chapter  };
					foreach (var candidateForGroupingChapter in chapterTariffCountList.Where(x => x.IsUsed == false))
					{
						if (count + candidateForGroupingChapter.TariffCount < maxTariffCount)
						{
							count += candidateForGroupingChapter.TariffCount;
							groupedChapter.Add(candidateForGroupingChapter.Chapter);
							candidateForGroupingChapter.IsUsed = true;
						}
					}
					result.Add(groupedChapter.ToArray());
				}
			}

			return result;
		}

		public IEnumerable<RefCusTradeGroup> GetFRTradeGroups()
		{
			var result = new List<RefCusTradeGroup>();
			var content = RitaDataDownloader.GetFRTradeGroupsWebPage();

			try
			{
				
				result = RITADataParser.GetFRTradeGroupsFromHTML(this, content);
			}
			catch (Exception e)
			{
				throw new RITAWebSiteException($"Unable to get a list of trade groups from RITA web site.", e);
			}

			return result;
		}

		public IEnumerable<RefCusTradeGroupCountry> GetFRTradeGroupCountries(string tradeGroupCode)
		{
			var result = new List<RefCusTradeGroupCountry>();
			var content = RitaDataDownloader.GetFRTradeGroupCountriesWebPage(tradeGroupCode);

			try
			{
				result = RITADataParser.GetFRTradeGroupCountriesFromHTML(content);
			}
			catch (Exception e)
			{
				throw new RITAWebSiteException($"Unable to get a list of trade group countries from RITA web site.", e);
			}

			return result;
		}

		public IEnumerable<RefCusTradeGroup> GetFRCountriesAsTradeGroups()
		{
			var result = new List<RefCusTradeGroup>();
			var content = RitaDataDownloader.GetFRCountriesWebPage();

			try
			{
				result = RITADataParser.GetCountriesFromHTML(content);
			}
			catch (Exception e)
			{
				throw new RITAWebSiteException($"Unable to get a list of FR trade group countries from RITA web site.", e);
			}

			return result;
		}

		public static List<RefCusTradeGroup> GetApplicationTerritories()
		{
			var result = new List<RefCusTradeGroup>
			{
				new RefCusTradeGroup() { ZZA_TradeGroup = "DPDOM", ZZA_Description = "Overseas Departments", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "METRO", ZZA_Description = "Metropolitan France", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "CONTI", ZZA_Description = "Continental France", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "CORSE", ZZA_Description = "Corsica", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "GUADE", ZZA_Description = "Guadeloupe", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "GUYAN", ZZA_Description = "French Guiana", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "MARTI", ZZA_Description = "Martinique", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "MAYOT", ZZA_Description = "Mayotte", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "REUNI", ZZA_Description = "Reunion", ZZA_StartDate = UniversalDataHelper.MinimumDateTime },
				new RefCusTradeGroup() { ZZA_TradeGroup = "MGPRE", ZZA_Description = "Martinique Guadeloupe Reunion", ZZA_StartDate = UniversalDataHelper.MinimumDateTime }
			};
			return result;
		}

		public string GetTariffUpdateWebPage(string[] groupedChapters, string startDate, string endDate, string domain)
		{
			var requestPayload = string.Format(CultureInfo.InvariantCulture, "expertsMesuresConversation.domaineCritere.description={0}&expertsMesuresConversation.dateDebutPeriode={1}&expertsMesuresConversation.dateFinPeriode={2}", domain, startDate.Replace("-", "%2F"), endDate.Replace("-", "%2F"));

			int count = 0;
			foreach (var chapter in groupedChapters)
			{
				requestPayload += $"&expertsMesuresConversation.listeChapitresCriteres%5B{count}%5D.code={chapter}";
				count++;
			}

			return RetryHelper.RetryWithDelay(
				() => { return RitaDataDownloader.GetWebPage(requestPayload, "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/mesures/ajaxRechercherMesuresModifiees.action", "https://www.douane.gouv.fr/rita-encyclopedie/public/experts/mesures/init.action"); },
				TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
				ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);
		}

		public List<RefCusConditionType> GetFRConditionTypes()
		{
			var result = new List<RefCusConditionType>();
			var content = RitaDataDownloader.GetFRConditionTypesWebPage();

			try
			{
				result = RITADataParser.GetFRConditionTypesFromHTML(content);
			}
			catch (Exception e)
			{
				throw new RITAWebSiteException($"Unable to get a list of condition types from RITA web site.", e);
			}

			return result;
		}

		class ChapterTariffCount
		{
			public ChapterTariffCount(string chapter, int tariffCount)
			{
				Chapter = chapter;
				TariffCount = tariffCount;
			}

			public string Chapter { get; set; }
			public int TariffCount { get; set; }
			public bool IsUsed { get; set; }
		}

		public const string importMeasure = "MI";
		public const string importCondition = "CI";
		public const string exportMeasure = "ME";
		public const string exportCondition = "CE";
		public const string nomenclature = "N";
		public const string taxationDomain = "Domaines+des+fiscalit%C3%A9s+(TVA%2C+taxes...)";
		public const string antiDumpingDomain = "Antidumping+et+mesures+de+r%C3%A9torsions";
		public const string customsDutiesDomain = "Droits+de+douane%2C+droits+additionnels+et+autres+droits";
		public const string prohibitionDomain = "Prohibitions%2FRestrictions";
		public const string domFeesDomain = "Octroi+de+mer";
		public const string statisticsDomain = "Statistiques";

		RITADataDownloader RitaDataDownloader;
	}
}
