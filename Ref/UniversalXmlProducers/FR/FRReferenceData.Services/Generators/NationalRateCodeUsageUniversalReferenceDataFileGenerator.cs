using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class NationalRateCodeUsageUniversalReferenceDataFileGenerator : TariffUniversalReferenceDataFileGenerator
	{
		protected override XmlWriterConfiguration GetXmlWriterConfigurationCore()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 01, 01, 00, 00, 00));
			codeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			codeList.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			return xmlWriterConfiguration;
		}

		public override int GenerateURDFiles(string[] tariffList, UpdateType updateType, DateTime updateDay)
		{
			int count = 0;
			tariffList = GetFilteredTariffList(tariffList);
			Console.Write("Getting national rate code usage from tariff measures...");

			var existingCodes = GetExistingRateCodeUsages(ApplicationConfig.Instance.NationalRateCodesRequiringUsageTracking);
			var nationalRateCodesRequiringUsageTracking = ApplicationConfig.Instance.NationalRateCodesRequiringUsageTracking.Split(new char[] { ',' });
			var nationalRateCodeUsageList = new List<RefCusCodeList>();

			foreach (var tariff in tariffList)
			{
				if (!string.IsNullOrEmpty(tariff))
				{
					var ritaDataParser = new RITADataParser();
					importTariffMeasuresList = ritaDataParser.GetMeasuresAndConditions(tariff, UniversalDataHelper.Constants.ImportFile).ToList();
					SubstituteEmptyTaxCode(importTariffMeasuresList);

					foreach (var rateCode in nationalRateCodesRequiringUsageTracking)
					{
						if (importTariffMeasuresList.Any(m => m.TaxCode == rateCode))
						{
							nationalRateCodeUsageList.Add(new RefCusCodeList()
							{
								ZZD_Code = tariff,
								ZZD_Description = tariff,
								ZZD_ZZK_NKCodeType = rateCode,
								ZZD_StartDate = importTariffMeasuresList.Min(m => m.StartDate),
								ZZD_EndDate = UniversalDataHelper.MaximumDateTime,
							});
						}
						else
						{
							var existingCode = existingCodes.FirstOrDefault(x => x.ZZD_ZZK_NKCodeType == rateCode && x.ZZD_Code == tariff);
							if (existingCode != null)
							{
								nationalRateCodeUsageList.Add(new RefCusCodeList()
								{
									ZZD_Code = tariff,
									ZZD_Description = tariff,
									ZZD_ZZK_NKCodeType = rateCode,
									ZZD_StartDate = existingCode.ZZD_StartDate,
									ZZD_EndDate = DateTime.Today.AddDays(-1),
								});
							}
						}
					}
				}
			}

			if (nationalRateCodeUsageList.Count > 0)
			{
				CreateOutputFile(updateType, nationalRateCodeUsageList.OrderBy(x => x.ZZD_ZZK_NKCodeType).ThenBy(x => x.ZZD_Code).ToList(), updateDay);
				count = 1;
			}

			Console.WriteLine("Done.");
			return count;
		}

		void CreateOutputFile(UpdateType updateType, List<RefCusCodeList> nationalRateCodeUsageList, DateTime updateDay)
		{
			UniversalDataHelper.InitializeWriter(xmlWriter, DateTime.Now, DataSource, updateType);
			var outputFileName = updateType == UpdateType.Full ? ApplicationConfig.Instance.FRNationalRateCodeUsageOutputFile : $"{FilePrefix}_Update_{updateDay.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.xml";
			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, outputFileName);
			UniversalDataHelper.ExportToXml(xmlWriter, nationalRateCodeUsageList, outputFilePath);
			Console.WriteLine($"New UniversalReference data file {outputFilePath} created.");
		}

		protected virtual RefCusCodeList[] GetExistingRateCodeUsages(string rateCodes)
		{
			var refCusCodeListLoader = new RefCusCodeListLoader(new RefDataLoader());
			var existingCodes = refCusCodeListLoader.GetRateCodeUsageCodeList(rateCodes).Result.ToArray();
			return existingCodes;
		}

		protected override string DataSource => "FR - National Rate Code Usage";

		protected override string FilePrefix => ApplicationConfig.Instance.FRNationalRateCodeUsageOutputFile.Replace(".xml", string.Empty);
	}
}
