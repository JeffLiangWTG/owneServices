using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class TariffUniversalReferenceDataFileGenerator
	{
		public TariffUniversalReferenceDataFileGenerator()
		{
			xmlWriter = new XmlWriter(GetXmlWriterConfiguration());
		}

		public virtual int GenerateURDFiles(string[] tariffList, UpdateType updateType, DateTime updateDay)
		{
			this.updateType = updateType;
			
			var currentTariffIndex = 0;
			var fileCount = 0;
			var currentChapter = string.Empty;
			var processedTariffList = new List<RefCusTariff>();

			tariffList = GetFilteredTariffList(tariffList);
			var tariffListCount = tariffList.Length;
			foreach (var tariff in tariffList)
			{
				currentChapter = tariff.Substring(0, 2);
				var previousTariffChapter = currentTariffIndex > 0 ? tariffList[currentTariffIndex - 1].Substring(0, 2) : string.Empty;

				if (updateType == UpdateType.Full && ((currentTariffIndex > 0 && currentChapter != previousTariffChapter) || currentTariffIndex >= tariffListCount))
				{
					if (processedTariffList.Count > 0)
					{
						CreateOutputFile(updateType, processedTariffList, previousTariffChapter, updateDay);
					}
					processedTariffList.Clear();
					fileCount++;
				}

				if (!string.IsNullOrEmpty(tariff))
				{
					Console.Write("Processing tariff " + tariff + "...");
					var processResult = ProcessMeasuresForTariff(tariff);
					if (processResult != null)
					{
						processedTariffList.Add(processResult);
						Console.WriteLine("Done.");
					}
					else
					{
						Console.WriteLine("No relevant measure found.");
					}
				}

				currentTariffIndex += 1;

				if (currentTariffIndex == tariffListCount)
				{
					if (processedTariffList.Count > 0)
					{
						CreateOutputFile(updateType, processedTariffList, currentChapter, updateDay);
						processedTariffList.Clear();
						fileCount++;
					}
					else
					{
						Console.WriteLine("No data, no UniversalReference data file was created.");
					}
				}
			}

			return fileCount;
		}

		void CreateOutputFile(UpdateType updateType, List<RefCusTariff> processedTariffList, string tariffChapter, DateTime updateDay)
		{
			var dataSource = updateType == UpdateType.Full ? $"{DataSource} chapter {tariffChapter}" : $"{DataSource}";
			UniversalDataHelper.InitializeWriter(xmlWriter, DateTime.Now, dataSource, updateType);
			var outputFileName = updateType == UpdateType.Full ? $"{FilePrefix}_Chapter{tariffChapter}.xml" : $"{FilePrefix}_Update_{updateDay.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture)}.xml";
			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, outputFileName);
			UniversalDataHelper.ExportToXml(xmlWriter, processedTariffList, outputFilePath);
			Console.WriteLine("New UniversalReference data file {0} created.", outputFilePath);
		}

		protected virtual string[] GetFilteredTariffList(string[] tariffList) => tariffList;

		protected static void SubstituteEmptyTaxCode(List<Measure> measures)
		{
			foreach (var measure in measures)
			{
				if (string.IsNullOrEmpty(measure.TaxCode))
				{
					measure.TaxCode = measure.Conditions.FirstOrDefault(c => c.TaxCode != "ATVA" && !string.IsNullOrEmpty(c.TaxCode))?.TaxCode ?? string.Empty;
				}
			}

			foreach (var measure in measures)
			{
				if (string.IsNullOrEmpty(measure.TaxCode))
				{
					measure.TaxCode = measures.FirstOrDefault(m => m.MeasureType == measure.MeasureType && !string.IsNullOrEmpty(m.TaxCode) && m.ApplicationTerritory == measure.ApplicationTerritory)?.TaxCode ?? string.Empty;
				}
			}

			foreach (var measure in measures)
			{
				if (string.IsNullOrEmpty(measure.TaxCode))
				{
					measure.TaxCode = measures.FirstOrDefault(m => m.MeasureType == measure.MeasureType && !string.IsNullOrEmpty(m.TaxCode))?.TaxCode ?? string.Empty;
				}
			}
		}

		XmlWriterConfiguration GetXmlWriterConfiguration() => GetXmlWriterConfigurationCore();
		protected virtual XmlWriterConfiguration GetXmlWriterConfigurationCore() => null;

		protected virtual RefCusTariff ProcessMeasuresForTariff(string tariffCode) => null;

		protected RefCusCondition[] GetImportConditions()
		{
			var conditions = new List<RefCusCondition>();

			foreach (var measure in importTariffMeasuresList.Where(m => m.IsVAT || m.IsExcise || m.IsImportProhibition || m.IsGrantingOfSea))
			{
				if (measure.IsVAT && !string.IsNullOrEmpty(measure.ApplicationTerritory))
				{
					conditions.AddRange(measure.GetConditionsForVat());
				}
				else if (measure.IsImportProhibition)
				{
					conditions.AddRange(measure.GetConditionsForProhibition());
				}
				else if (measure.IsGrantingOfSea)
				{
					conditions.AddRange(measure.GetConditionsForGrantingOfSea());
				}
				else if (!string.IsNullOrEmpty(measure.ApplicationTerritory))
				{
					conditions.AddRange(measure.GetConditionsForExcise());
				}

			}

			return conditions.ToArray();
		}

		protected RefCusCondition[] GetExportConditions()
		{
			var conditions = new List<RefCusCondition>();

			foreach (var measure in exportTariffMeasuresList.Where(m => m.IsExportSupported))
			{
				if (measure.IsExportProhibition)
				{
					conditions.AddRange(measure.GetConditionsForProhibition());
				}
				else if (measure.IsExportRate)
				{
					conditions.AddRange(measure.GetConditionsForExportRate());
				}
			}

			return conditions.ToArray();
		}

		protected virtual RefCusRate[] GetRates(UpdateType updateType) => null;

		protected virtual RefCusTariffUOM[] GetUOMs(string tariffCode) => null;

		protected virtual RefCusTariffAdditionalCode[] GetAdditionalCodes() => null;

		internal List<Measure> importTariffMeasuresList;

		internal List<Measure> exportTariffMeasuresList;

		protected virtual string DataSource => string.Empty;

		protected virtual string FilePrefix => string.Empty;

		internal XmlWriter xmlWriter;

		internal UpdateType updateType;
	}
}

