using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TariffDataParser
	{
		public TariffDataParser(string workingDirectory, string excelDirectory)
		{
			this._workingDirectory = workingDirectory;
			this.excelDirectory = excelDirectory;

			HarmonizedTariffs = new Dictionary<string, CAHarmonizedTariff>();
			FinalHarmonizedTariffs = new List<CAHarmonizedTariff>();

			_XmlWriter = new XmlWriter(XMLWriterHelper.GetHarmonizedRefCusTariffConfiguration());
			_XmlWriter.SetDataSource(Constants.DataSource.TariffData);
			_XmlWriter.SetUpdateType(UpdateType.Full);
		}
		readonly string _workingDirectory;
		readonly string excelDirectory;
		readonly IXmlWriter _XmlWriter;
		Dictionary<string, CAHarmonizedTariff> HarmonizedTariffs;
		List<CAHarmonizedTariff> FinalHarmonizedTariffs;

		public void ParseRefCusTariffsIntoXML(string exportFilePath, DateTime publicationTime)
		{
			GetTariffsFromAccdb();
			var conditionDataParser = new ConditionDataParser(excelDirectory);
			conditionDataParser.Parse();

			Console.WriteLine("Start parsing Tariff.");
			Console.WriteLine($"There will be {FinalHarmonizedTariffs.Count} tariffs populated into the final XML");
			foreach (var tariff in FinalHarmonizedTariffs)
			{
				var description = GetDescription(tariff);
				var refCusTariff = new RefCusTariff
				{
					ZZ1_TariffCode = tariff.Tariff,
					ZZ1_Description = description.Substring(0, Math.Min(256, description.Length)),
					ZZ1_StartDate = tariff.EffectiveDate
				};
				var rates = GetRefCusRates(tariff);
				if (rates.Any())
				{
					refCusTariff.RefCusRates = rates.ToArray();
				}

				var conditions = conditionDataParser.GetRefCusConditions(tariff.Tariff);
				if (conditions.Any())
				{
					refCusTariff.RefCusConditions = conditions.ToArray();
				}

				ParserHelper.AddUOM(tariff.Uom, refCusTariff);
				ParserHelper.MarkConveyanceTariffs(refCusTariff);
				_XmlWriter.PopulateData(refCusTariff);
			}

			_XmlWriter.SetPublicationTime(publicationTime);
			_XmlWriter.SaveXml(exportFilePath);
			Console.WriteLine($"Final XML populated {exportFilePath}.");
		}

		string GetDescription(CAHarmonizedTariff tariff)
		{
			List<string> descriptions = null;
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 4), out var tariff4Bits);
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 5), out var tariff5Bits);
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 6), out var tariff6Bits);
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 7), out var tariff7Bits);
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 8), out var tariff8Bits);
			HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, 9), out var tariff9Bits);
			if (tariff9Bits != null)
			{
				descriptions = new List<string>() { tariff9Bits.Description, tariff.Description, "(", tariff4Bits?.Description,
					tariff5Bits?.Description, tariff6Bits?.Description, tariff7Bits?.Description, tariff8Bits?.Description};
			}
			else if (tariff7Bits != null)
			{
				descriptions = new List<string>() { tariff7Bits.Description, tariff8Bits?.Description, tariff.Description,
					"(", tariff4Bits?.Description, tariff5Bits?.Description, tariff6Bits?.Description};
			}
			else
			{
				descriptions = new List<string>() { tariff5Bits?.Description, tariff6Bits?.Description,tariff8Bits?.Description, tariff.Description,
					"(", tariff4Bits?.Description};
			}
			var description = string.Join(" ", descriptions.Distinct().ToList()) + ")";
			return ParserHelper.SanitizeXmlString(Regex.Replace(description, @"\(\s", "("));
		}

		List<RefCusRate> GetRefCusRates(CAHarmonizedTariff tariff)
		{
			List<RefCusRate> refCusRates = new List<RefCusRate>();
			foreach (var ttCode in ParserHelper.TTCodeList)
			{
				var rateValue = string.Empty;
				if (!tariff.TariffRates.ContainsKey(ttCode.Code))
				{
					for (int i = 9; i >= 4; i--)
					{
						HarmonizedTariffs.TryGetValue(tariff.Tariff.Substring(0, i), out var newTariff);
						if (newTariff != null && newTariff.TariffRates.ContainsKey(ttCode.Code))
						{
							rateValue = newTariff.TariffRates[ttCode.Code];
							break;
						}
					}
				}
				else
				{
					rateValue = tariff.TariffRates[ttCode.Code];
				}

				if (!string.IsNullOrEmpty(rateValue))
				{
					var rateFormula = ParserHelper.GetRateFormula(rateValue, ttCode.Code, tariff.Uom);
					if (!string.IsNullOrEmpty(rateFormula))
					{
						var rate = new RefCusRate()
						{
							ZZ2_ZZS_NKPreference = ttCode.Code,
							ZZ2_RateFormula = rateFormula,
							ZZ2_StartDate = tariff.EffectiveDate
						};

						var applicability = new RefCusApplicability()
						{
							ZZT_ZZA_NKTradeGroup = ttCode.Code,
							ZZT_StartDate = tariff.EffectiveDate
						};

						rate.RefCusApplicabilities = new[] { applicability };
						refCusRates.Add(rate);
					}
				}
			}
			return refCusRates;
		}

		void GetTariffsFromAccdb()
		{
			Console.WriteLine("Start parsing Accdb.");
			var tempPath = Path.GetTempPath();
			var accdbFile = ParserHelper.GetAndExtractAccdbFile(tempPath, _workingDirectory);
			if (!string.IsNullOrEmpty(accdbFile))
			{
				var accdbPath = Path.Combine(tempPath, accdbFile);
				using (DataTable results = new DataTable())
				using (var odbcConnection = OdbcConnectionHelper.GetOdbcConnection(accdbPath))
				{
					odbcConnection.Open();
					var sql = @"SELECT TARIFF,EFF_DATE,DESC1,UOM,MFN,[General Tariff] AS GT,AUT,NZT,CCCT,LDCT,GPT,UST,
MXT,CIAT,CT,CRT,IT,NT,SLT,PT,COLT,JT,PAT,HNT,KRT,CEUT,UAT,CPTPT,UKT FROM TPHS WHERE TARIFF IS NOT NULL;";
					var command = new OdbcCommand(sql, odbcConnection);
					var adapter = new OdbcDataAdapter(command);
					adapter.Fill(results);
					adapter.Dispose();
					command.Dispose();
					odbcConnection.Close();

					ConvertDataTableToResults(results);
				}
			}
			Console.WriteLine("End parsing Accdb.");
		}

		void ConvertDataTableToResults(DataTable dataTable)
		{
			Console.WriteLine("Start parsing tariff.");
			Console.WriteLine($"There are {dataTable.Rows.Count} tariffs in the database");
			foreach (DataRow row in dataTable.Rows)
			{
				var description = row["DESC1"];
				var uom = row["UOM"];
				var harmonizedTariff = new CAHarmonizedTariff()
				{
					Tariff = ((string)row["TARIFF"]).Replace(".", ""),
					EffectiveDate = (DateTime)row["EFF_DATE"],
					Description = description.GetType() == typeof(DBNull) ? string.Empty : (string)description,
					Uom = uom.GetType() == typeof(DBNull) ? string.Empty : (string)uom
				};

				var tariffDictionary = new Dictionary<string, string>();
				foreach (var ttCode in ParserHelper.TTCodeList)
				{
					var rate = row[ttCode.Abbreviation].GetType() == typeof(DBNull) ? string.Empty : (string)row[ttCode.Abbreviation];
					if (!string.IsNullOrEmpty(rate))
					{
						tariffDictionary.Add(ttCode.Code, rate);
					}
				}

				harmonizedTariff.TariffRates = tariffDictionary;
				if (harmonizedTariff.Tariff.Length == 10)
				{
					FinalHarmonizedTariffs.Add(harmonizedTariff);
				}
				else
				{
					HarmonizedTariffs.Add(harmonizedTariff.Tariff, harmonizedTariff);
				}
			}
		}
	}
}
