using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	sealed class TariffLeviesBuilder : IBuilder<NZTariffProcessingData>
	{
		public TariffLeviesBuilder(IDateProvider dateProvider, ILogger logger, IFileReader fileReader = null)
		{
			formulas = new Dictionary<string, decimal>();
			this.dateProvider = dateProvider;
			Logger = logger;
			FileReader = fileReader ?? new FileReader();
		}

		IFileReader FileReader { get; }

		readonly IDateProvider dateProvider;

		readonly Dictionary<string, decimal> formulas;

		ILogger Logger { get; }

		public bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, NZTariffProcessingData processingData)
		{
			var formulaPath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.LevyFormula);
			var levyPath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.Levy);
			if (formulaPath == null || levyPath == null)
			{
				throw new ArgumentException($"TariffLeviesBuilder needs to have exactly 1 file path for each levy symbol");
			}
			Logger.LogInfo("Start processing Levies file...");

			if (!ReadAllLevyFormulaRates(formulaPath.FilePath))
			{
				return false;
			}

			var count = 0;
			var activeDate = dateProvider.ActiveDate;
			var lines = FileReader.ReadAllLines(levyPath.FilePath);

			foreach (var line in lines.Skip(1))
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				var levyDetails = line.Trim().Split(Constants.TariffSplit);

				if (levyDetails.Length == 9)
				{
					var startDate = Constants.MinSmallDateTime;
					var endDate = Constants.MaxSmallDateTime;
					if (!string.IsNullOrEmpty(levyDetails[7]) && !DateTime.TryParse(levyDetails[7], out startDate))
					{
						throw new InvalidOperationException($"Can't parse start date from line '{line}' in '{levyPath}'.");
					}
					if (!string.IsNullOrEmpty(levyDetails[8]) && !DateTime.TryParse(levyDetails[8], out endDate))
					{
						throw new InvalidOperationException($"Can't parse end date from line '{line}' in '{levyPath}'.");
					}
					startDate = startDate < Constants.MinSmallDateTime ? Constants.MinSmallDateTime : startDate;
					endDate = endDate > Constants.MaxSmallDateTime ? Constants.MaxSmallDateTime : endDate;

					var tariffCodeWithoutCheckDigit = string.Concat(levyDetails.Take(5));
					var tariff = ((ITopLevelDataRepo<RefCusTariff>)dataRepo).Load(tariffCodeWithoutCheckDigit);
					if (endDate > activeDate && tariff != null && tariff.ZZ1_EndDate > activeDate && tariff.ZZ1_StartDate < endDate)
					{
						try
						{
							var levyTypeCode = levyDetails[5];
							FillRateData(tariff, levyTypeCode, levyDetails[6], startDate, endDate);
							count++;
						}
						catch (RefDataParseException ex)
						{
							var errorMessage = $"{ex.Message} from line '{line}' in '{levyPath}'.";
							Logger.LogError(errorMessage);
							Logger.LogError(ex.ToString());
						}
					}
				}
				else
				{
					var errorMessage = $"Can't process data line '{line}' in '{levyPath}'.";
					Logger.LogError(errorMessage);
				}
			}

			if (count == 0)
			{
				throw new InvalidOperationException($"Can't create any tariff rate data from {levyPath}.");
			}

			return true;
		}

		bool ReadAllLevyFormulaRates(string filePath)
		{
			var result = true;
			var count = 0;
			var lines = FileReader.ReadAllLines(filePath);
			foreach (var line in lines.Skip(1))
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				var levyFormula = line.Trim().Split(Constants.TariffSplit);

				if (levyFormula.Length == 2)
				{
					if (decimal.TryParse(levyFormula[1], out var formula))
					{
						formulas.Add(levyFormula[0], formula);
						count++;
					}
					else
					{
						var errorMessage = $"Can't parse formula from line '{line}' in '{filePath}'.";
						Logger.LogError(errorMessage);
						result = false;
					}
				}
				else
				{
					var errorMessage = $"Can't process data line '{line}' in '{filePath}'.";
					Logger.LogError(errorMessage);
					result = false;
				}
			}

			if (count == 0)
			{
				throw new InvalidOperationException($"Can't process any levy formula from {filePath}.");
			}
			return result;
		}

		void FillRateData(RefCusTariff tariff, string levyTypeCode, string formulaCode, DateTime startDate, DateTime endDate)
		{
			var qty = string.Empty;
			var uomType = GetUOMType(levyTypeCode);
			var tariffUOM = tariff.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_Type == uomType);
			if (tariffUOM != null)
			{
				qty = tariffUOM.ZZ8_UOM;
			}
			else
			{
				throw new RefDataParseException("Can't find supplementary Tariff UOM record");
			}

			var formula = string.Empty;
			if (formulas.TryGetValue(formulaCode, out var formulaRate))
			{
				if (formulaRate != decimal.Zero && string.IsNullOrEmpty(qty))
				{
					throw new RefDataParseException("Doesn't have enough infos for generating rate formula");
				}
				else
				{
					formula = formulaRate == decimal.Zero ? "0" : $"{formulaRate}*[{qty}]";
				}
			}
			else
			{
				throw new RefDataParseException("Doesn't have enough infos for generating rate formula");
			}

			var rate = tariff.RefCusRates?.FirstOrDefault(r => r.ZZ2_ZY1_NKRateCode == levyTypeCode
															&& r.ZZ2_ZY1_ZZR_NKRateType == Constants.TariffRateCodes.LVY
															&& r.ZZ2_RateFormula == formula);

			if (rate != null)
			{
				rate.AddNewRefCusApplicability(startDate, endDate, Constants.TariffTradeGroups.NML);
			}
			else
			{
				rate = tariff.AddNewRefCusRate(startDate, endDate, Constants.TariffTradeGroups.NML);
				rate.ZZ2_ZY1_NKRateCode = levyTypeCode;
				rate.ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateCodes.LVY;
				rate.ZZ2_RateFormula = formula;
			}
		}

		static string GetUOMType(string levyType)
		{
			var uomType = Constants.TariffUOMTypes.CU2;
			switch (levyType)
			{
				case Constants.TariffLevyTypes.AC:
				case Constants.TariffLevyTypes.AL:
				case Constants.TariffLevyTypes.GG:
				case Constants.TariffLevyTypes.PF:
				case Constants.TariffLevyTypes.SL:
					uomType = Constants.TariffUOMTypes.CU1;
					break;
			}

			return uomType;
		}
	}
}
