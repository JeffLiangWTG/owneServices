using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs.Parser;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class CARMTariffParser
	{
		public CARMTariffParser(string workingDirectory, string conditionFileName)
		{
			this.workingDirectory = workingDirectory;
			this.conditionFileName = conditionFileName;
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusTariffConfiguration());
			xmlWriter.SetDataSource(Constants.DataSource.TariffData);
			xmlWriter.SetUpdateType(UpdateType.Partial);
			cARMParserHelper = new CARMParserHelper(workingDirectory);
		}
		readonly string workingDirectory;
		readonly string conditionFileName;
		readonly IXmlWriter xmlWriter;
		readonly CARMParserHelper cARMParserHelper;
		DateTime? lastPublishDate;

		public void ParseXMLfilesIntoXML(string exportFilePath, DateTime? lastPublishDate, DateTime currentPublishDate)
		{
			this.lastPublishDate = lastPublishDate;
			var exciseTaxDutiesDic = GetExciseTaxDutiesDicAsync();
			var exciseTaxesDic = GetExciseTaxesDicAsync();
			var tariffFRDic = GetFRTariffs();
			var customsDutiesDic = GetCustomsDutiesDicAsync();
			var conditionDataParser = new ConditionDataParser(conditionFileName);

			cARMParserHelper.ParseEntriesByFilesCore<CARMTariff, RefCusTariff>(Constants.CARMAPIQueryTypes.TariffQueryType, carmTariff =>
			{
				RefCusTariff tariff = null;
				var tariffNumber = carmTariff.TariffNumber;
				exciseTaxDutiesDic.Result.TryGetValue(tariffNumber, out var carmExciseDuties);
				exciseTaxesDic.Result.TryGetValue(tariffNumber, out var carmExciseTaxes);
				tariffFRDic.Result.TryGetValue(tariffNumber, out var carmTariffFRs);
				customsDutiesDic.Result.TryGetValue(tariffNumber.Substring(0, CARMConstants.TariffItemNumberLength), out var carmCustomsDuties);

				bool shouldParseTariff = !IsExpired(carmTariff);

				RefCusTariffLanguage[] tariffLanguages = null;
				if (IsNotEmptyCollection(carmTariffFRs))
				{
					shouldParseTariff = true;
					var desc = ParserHelper.RemoveNewLines(carmTariffFRs[0]);
					var refCusTariffLanguage = new RefCusTariffLanguage();
					refCusTariffLanguage.ZX7_Description = desc.Substring(0, Math.Min(3999, desc.Length));
					tariffLanguages = new RefCusTariffLanguage[] { refCusTariffLanguage };
				}

				var rates = new List<RefCusRate>();
				AppendRates(carmExciseDuties);
				AppendRates(carmExciseTaxes);
				AppendRates(carmCustomsDuties);

				void AppendRates(List<RefCusRate> ratesToBeAppended)
				{
					if (IsNotEmptyCollection(ratesToBeAppended))
					{
						shouldParseTariff = true;
						rates.AddRange(ratesToBeAppended);
					}
				}

				if (shouldParseTariff)
				{
					tariff = carmTariff.ParseToRefCusTariff();
					if (tariff != null)
					{
						tariff.RefCusTariffLanguages = tariffLanguages;
						if (rates.Any())
						{
							tariff.RefCusRates = rates.ToArray();
						}
						var conditions = conditionDataParser.GetRefCusConditions(tariffNumber);
						if (conditions.Any())
						{
							tariff.RefCusConditions = conditions.ToArray();
						}
					}
				}
				return tariff;
			}, (carmTariff, tariff) => xmlWriter.PopulateData(tariff));
			xmlWriter.SetPublicationTime(currentPublishDate);
			xmlWriter.SaveXml(exportFilePath);
		}

		async Task<Dictionary<string, List<string>>> GetFRTariffs()
		{
			return await Task.Run(() => cARMParserHelper.ParseEntriesByFiles<CARMTariff, string>(Constants.CARMAPIQueryTypes.TariffQueryType, tariff => IsExpired(tariff) ? null : tariff.Description, tariff => tariff.TariffNumber, Constants.DefaultValues.FRLanguage));
		}

		async Task<Dictionary<string, List<RefCusRate>>> GetExciseTaxDutiesDicAsync()
		{
			return await Task.Run(() => cARMParserHelper.ParseEntriesByFiles<CARMExciseDuty, RefCusRate>(Constants.CARMAPIQueryTypes.ExciseDutiesQueryType, exciseDuty => IsExpired(exciseDuty) ? null : exciseDuty.ParseToRefCusRate(), exciseDuty => exciseDuty.TariffNumber));
		}

		async Task<Dictionary<string, List<RefCusRate>>> GetExciseTaxesDicAsync()
		{
			return await Task.Run(() =>
			{
				var exciseCodesDic = GetExciseTaxCodesDicAsync();
				return cARMParserHelper.ParseEntriesByFiles<CARMExciseTax, RefCusRate>(Constants.CARMAPIQueryTypes.ExciseTaxesQueryType, exciseTax =>
				{
					RefCusRate result = null;
					if (exciseTax.IsValid)
					{
						exciseCodesDic.Result.TryGetValue(exciseTax.ExciseTaxCode, out var exciseTaxCodes);
						if (IsNotEmptyCollection(exciseTaxCodes))
						{
							var exciseTaxCode = exciseTaxCodes[0];
							if (!IsExpired(exciseTaxCode) || !IsExpired(exciseTax))
							{
								var copyCode = exciseTaxCode.ShallowCopy();
								copyCode.PopulateDateRangeByExciseTax(exciseTax);
								result = copyCode.ParseToRefCusRate();
							}
						}
					}
					return result;
				}, exciseTax => exciseTax.TariffNumber, multiValues: true);
			});
		}

		async Task<Dictionary<string, List<CARMExciseTaxCode>>> GetExciseTaxCodesDicAsync()
		{
			return await Task.Run(() => cARMParserHelper.ParseEntriesByFiles<CARMExciseTaxCode, CARMExciseTaxCode>(Constants.CARMAPIQueryTypes.ExciseTaxCodesQueryType, exciseTaxCode => exciseTaxCode, exciseTaxCode => exciseTaxCode.ExciseTaxCode));
		}

		async Task<Dictionary<string, List<RefCusRate>>> GetCustomsDutiesDicAsync()
		{
			return await Task.Run(() => cARMParserHelper.ParseEntriesByFiles<CARMCustomsDuty, RefCusRate>(Constants.CARMAPIQueryTypes.CustomsDutiesQueryType, customsDuty => IsExpired(customsDuty) ? null : customsDuty.ParseToRefCusRate(), customsDuty => customsDuty.TariffItemNumber, multiValues: true));
		}

		bool IsExpired<T>(T contentProperties) where T : CARMContentProperties
		{
			return lastPublishDate.HasValue && contentProperties.UpdateOn <= lastPublishDate;
		}

		bool IsNotEmptyCollection<T>(List<T> collection) => collection != null && collection.Any();
	}
}
