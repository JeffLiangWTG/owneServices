using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMTariffDataProducer : IProducer
	{
		public CARMTariffDataProducer(IWebServiceCaller serviceCaller, StringBuilder errorBuilder, IFileDownloader downloader, string conditionFileName = null)
		{
			this.serviceCaller = serviceCaller;
			this.errorBuilder = errorBuilder;
			this.downloader = downloader;
			this.conditionFileName = conditionFileName ?? Path.GetTempFileName();
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
		}
		readonly IWebServiceCaller serviceCaller;
		readonly StringBuilder errorBuilder;
		readonly IFileDownloader downloader;
		readonly string conditionFileName;

		public string OutPutFilePath => outPutFilePath ?? (outPutFilePath = Path.Combine(ApplicationConfig.OutputPath, string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string outPutFilePath;

		public string FunctionCode => Constants.ProgramFunctions.CATariff;

		public string WorkingFileOrDirectory => workingFolder ?? (workingFolder = Path.Combine(Path.GetTempPath(), FunctionCode));
		string workingFolder;

		public DateTime PublicationTime => publicationTime.HasValue ? publicationTime.Value : (publicationTime = DateTime.UtcNow.AddHours(-5).Date).Value;
		DateTime? publicationTime;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CA tariff data");
			try
			{
				Directory.CreateDirectory(WorkingFileOrDirectory);
				var preProcessChecker = new PreProcessChecker(FunctionCode);
				publicationTime = QueryLatestUpdateTime();
				var lastPublishDateInfo = preProcessChecker.IsLastPublishDateExpired(publicationTime);
				if (lastPublishDateInfo.Expired)
				{
					var excelModifiedDateAndUrl = downloader.GetConditionExcelModifiedDateAndUrl();
					if (downloader.DownloadFile(excelModifiedDateAndUrl.Item2, conditionFileName))
					{
						QueryAndParseAllRelevantData(new CARMTariffParser(WorkingFileOrDirectory, conditionFileName), lastPublishDateInfo.LastPublishDate);
						preProcessChecker.SavePublishDate(PublicationTime);
					}
					else
					{
						Console.WriteLine($"CARM Tariff: Get condition excel failed. Skip processing this time.");
					}
				}
				else
				{
					Console.WriteLine($"CARM Tariff: Nothing new published since last process. Skip processing this time.");
				}
			}
			catch (Exception ex)
			{
				errorBuilder.AppendLine($"Error processing in '{WorkingFileOrDirectory}':{Environment.NewLine}{ex.Message}");
			}
			finally
			{
				Directory.Delete(WorkingFileOrDirectory, true);
			}

			Console.WriteLine("End processing CA tariff data");
		}

		DateTime? QueryLatestUpdateTime()
		{
			Console.WriteLine("CARM Tariff: Query latest update time from webservice...");
			var queryTypes = new string[] { CARMAPIQueryTypes.TariffQueryType, CARMAPIQueryTypes.ExciseTaxesQueryType, CARMAPIQueryTypes.ExciseTaxCodesQueryType, CARMAPIQueryTypes.ExciseDutiesQueryType, CARMAPIQueryTypes.CustomsDutiesQueryType };
			var tasks = queryTypes.Select(queryType => Task.Run(() => serviceCaller.GetLatestUpdateOnDate(queryType))).ToArray();
			Task.WaitAll(tasks);

			DateTime? latestUpdateTime = tasks.Select(task => task.Result).Where(result => result.HasValue).Max();
			Console.WriteLine("CARM Tariff: Latest update time " + (latestUpdateTime?.ToString(CultureInfo.InvariantCulture) ?? "not found"));
			return latestUpdateTime;
		}

		void QueryAndParseAllRelevantData(CARMTariffParser tariffParser, DateTime? lastPublishDate)
		{
			var lastPublishDateTime = lastPublishDate?.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
			Console.WriteLine("CARM Tariff: Downloading files from webservice, last publish time [{0}]", lastPublishDateTime);

			var filter = string.IsNullOrEmpty(lastPublishDateTime) ? string.Empty : $"{nameof(CARMTariff.UpdateOn)} gt datetime'{HttpUtility.UrlEncode(lastPublishDateTime)}'";
			Parallel.Invoke(
				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.TariffQueryType, new string[] { nameof(CARMTariff.TariffNumber) }, new string[] { nameof(CARMTariff.TariffNumber), nameof(CARMTariff.UnitOfMeasureCode), nameof(CARMTariff.Description), nameof(CARMTariff.TariffNumberValidStartDate), nameof(CARMTariff.TariffNumberValidEndDate), nameof(CARMTariff.UpdateOn) }),

				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.TariffQueryType, new string[] { nameof(CARMTariff.TariffNumber) }, new string[] { nameof(CARMTariff.TariffNumber), nameof(CARMTariff.Description), nameof(CARMTariff.UpdateOn) },
					acceptLanguage: Constants.DefaultValues.FRLanguage, filter: filter),

				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.ExciseTaxesQueryType, new string[] { nameof(CARMExciseTax.TariffNumber) }, new string[] { nameof(CARMExciseTax.TariffNumber), nameof(CARMExciseTax.ExciseTaxCode), nameof(CARMExciseTax.ExciseTaxCodeValidStartDate), nameof(CARMExciseTax.ExciseTaxCodeValidEndDate), nameof(CARMExciseTax.UpdateOn) }),

				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.ExciseTaxCodesQueryType, new string[] { nameof(CARMExciseTaxCode.ExciseTaxCode) }, new string[] { nameof(CARMExciseTaxCode.ExciseTaxCode), nameof(CARMExciseTaxCode.ExciseTaxRateTypeCode), nameof(CARMExciseTaxCode.ExciseTaxRateValue), nameof(CARMExciseTaxCode.UnitOfMeasureCode), nameof(CARMExciseTaxCode.UpdateOn) }),

				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.ExciseDutiesQueryType, new string[] { nameof(CARMExciseDuty.TariffNumber) }, new string[] { nameof(CARMExciseDuty.TariffNumber), nameof(CARMExciseDuty.ExciseDutyValidStartDate), nameof(CARMExciseDuty.ExciseDutyValidEndDate), nameof(CARMExciseDuty.UnitOfMeasureCode), nameof(CARMExciseDuty.ExciseDutyValueType), nameof(CARMExciseDuty.ExciseDutyRateValue), nameof(CARMExciseDuty.UpdateOn) }, filter: filter),

				() => serviceCaller.QueryAndDownloadXmlFiles(WorkingFileOrDirectory, CARMAPIQueryTypes.CustomsDutiesQueryType, new string[] { nameof(CARMCustomsDuty.TariffItemNumber), nameof(CARMCustomsDuty.TariffTreatmentCode) },
					new string[] { nameof(CARMCustomsDuty.TariffItemNumber), nameof(CARMCustomsDuty.TariffTreatmentCode), nameof(CARMCustomsDuty.CustomsDutyValidStartDate), nameof(CARMCustomsDuty.CustomsDutyValidEndDate),
								nameof(CARMCustomsDuty.SpecificRateMinValue), nameof(CARMCustomsDuty.SpecificRateMinQualifierCode), nameof(CARMCustomsDuty.SpecificRateRegValue), nameof(CARMCustomsDuty.SpecificRateRegQualifierCode), nameof(CARMCustomsDuty.SpecificRateMaxValue), nameof(CARMCustomsDuty.SpecificRateMaxQualifierCode),
								nameof(CARMCustomsDuty.AdValoremRateMinValue), nameof(CARMCustomsDuty.AdValoremRateMinQualifierCode), nameof(CARMCustomsDuty.AdValoremRateRegValue), nameof(CARMCustomsDuty.AdValoremRateRegQualifierCode), nameof(CARMCustomsDuty.AdValoremRateMaxValue), nameof(CARMCustomsDuty.AdValoremRateMaxQualifierCode),nameof(CARMCustomsDuty.UnitOfMeasureCode),nameof(CARMCustomsDuty.UpdateOn) }, filter: filter)
			);
			Console.WriteLine("CARM Tariff: All required files downloaded, start parsing...");
			tariffParser.ParseXMLfilesIntoXML(OutPutFilePath, lastPublishDate, PublicationTime);
		}
	}
}
