using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	sealed class TariffRatesBuilder : IBuilder<NZTariffProcessingData>
	{
		public TariffRatesBuilder(IDateProvider dateProvider, ILogger logger)
		{
			this.dateProvider = dateProvider;
			Logger = logger;
		}

		readonly IDateProvider dateProvider;
		ILogger Logger { get; }

		public bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, NZTariffProcessingData processingData)
		{
			if (filePaths.Length != 1)
			{
				throw new ArgumentException($"TariffRatesBuilder needs to have exactly 1 file path");
			}
			Logger.LogInfo("Start processing Rates file...");

			var filePath = filePaths.Single().FilePath;

			var count = 0;
			var activeDate = dateProvider.ActiveDate;
			var lines = File.ReadLines(filePath).Skip(1);

			foreach (var line in lines)
			{
				try
				{
					var rate = new TariffRate(line);
					var tariff = ((ITopLevelDataRepo<RefCusTariff>)dataRepo).Load(rate.TariffCode);
					if (IsTariffRateApplicable(rate, tariff, activeDate))
					{
						FillRateData(tariff, rate);
						count++;
					}
				}
				catch (RefDataParseException ex)
				{
					var errorMessage = $"{ex.Message} from line '{line}' in 'Tariff_Rates.csv'.";
					Logger.LogError(errorMessage);
				}
			}

			if (count == 0)
			{
				throw new InvalidOperationException($"Can't create any tariff rate data from 'Tariff_Rates.csv'.");
			}
			return true;
		}

		static void FillRateData(RefCusTariff tariff, TariffRate tariffRate)
		{
			var tariffUOM = GetMainTariffUOM(tariff);
			var formula = GetRateFormula(tariffUOM, tariffRate.FormulaCode,  tariffRate.FactorA, tariffRate.FactorB);
			var existingCusRate = tariff.RefCusRates?.FirstOrDefault(r => r.ZZ2_RateFormula == formula && r.ZZ2_ZZS_NKPreference == tariffRate.RateGroup);
			var isManual = tariffRate.FormulaCode == "2";

			if (existingCusRate != null)
			{
				existingCusRate.AddNewRefCusApplicability(tariffRate.StartDate, tariffRate.EndDate, tariffRate.RateGroup, isManual);
			}
			else
			{
				tariff.AddNewRefCusRate(tariffRate.StartDate, tariffRate.EndDate, tariffRate.RateGroup, formula, tariffUOM, isManual);
			}
		}

		static string GetMainTariffUOM(RefCusTariff tariff) =>
			tariff.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == Constants.TariffUOMTypes.CU1)?.ZZ8_UOM;

		static bool IsTariffRateApplicable(TariffRate rate, RefCusTariff tariff, DateTime activeDate)
		{
			return rate.EndDate > activeDate
			       && tariff != null
			       && tariff.ZZ1_EndDate > activeDate
			       && tariff.ZZ1_StartDate < rate.EndDate;
		}

		static string GetRateFormula(string qty, string formulaCode, string factorA, string factorB)
		{
			var formula = string.Empty;
			var errorMessage = "Doesn't have enough infos for generating rate formula";
			switch (formulaCode)
			{
				case "1":
				case "2":
					formula = "0";
					break;
				case "3":
					if (!string.IsNullOrEmpty(factorA))
					{
						formula = $"{Helper.GetFactorString(factorA, true)}*VFD";
						break;
					}
					else
					{
						throw new RefDataParseException(errorMessage);
					}
				case "4":
					if (!string.IsNullOrEmpty(factorA) && !string.IsNullOrEmpty(qty))
					{
						formula = $"{Helper.GetFactorString(factorA, false)}*[{qty}]";
						break;
					}
					else
					{
						throw new RefDataParseException(errorMessage);
					}
				case "5":
					if (!string.IsNullOrEmpty(factorA) && !string.IsNullOrEmpty(factorB) && !string.IsNullOrEmpty(qty))
					{
						formula = $"({Helper.GetFactorString(factorA, true)}*VFD)+({Helper.GetFactorString(factorB, false)}*[{qty}])";
						break;
					}
					else
					{
						throw new RefDataParseException(errorMessage);
					}
				case "6":
					if (!string.IsNullOrEmpty(factorA) && !string.IsNullOrEmpty(factorB) && !string.IsNullOrEmpty(qty))
					{
						formula = $"({Helper.GetFactorString(factorA, false)}*[{qty}])-({Helper.GetFactorString(factorB, true)}*VFD)";
						break;
					}
					else
					{
						throw new RefDataParseException(errorMessage);
					}
				default:
					throw new RefDataParseException(errorMessage);
			}

			return formula;
		}
	}
}
