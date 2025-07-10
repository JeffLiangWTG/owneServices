using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public abstract class HsnTariffEXCListDutyRatesLoaderBase
	{
		protected HsnTariffEXCListDutyRatesLoaderBase(StringBuilder logger = null)
		{
			this.logger = logger;
		}

		public abstract string InputFileName { get; }

		public string InputFilePath => Path.Combine(ApplicationConfig.ResPath, InputFileName);

		protected virtual int TariffNoColumnNumber => 1;

		protected virtual int DescriptionColumnNumber => 2;

		protected virtual int DutyAmountColumnNumber => 3;

		protected virtual int UomCU3ColumnNumber => 5;
		protected virtual int UomCU4ColumnNumber => default;

		protected virtual int UomCU5ColumnNumber => default;

		protected virtual int ExemptedTariffCodesColumnNumber => 6;

		protected virtual int AdditionalCodeColumnNumber => 7;

		protected virtual int RateTypeColumnNumber => 8;

		protected virtual int RateCodeColumnNumber => 9;

		protected virtual int RateFormulaColumnNumber => 10;

		protected virtual int StartDateColumnNumber => 11;

		protected virtual int EndDateColumnNumber => 12;


		public HsnTariffEXCListDutyRate[] GetRates()
		{
			if (!File.Exists(InputFilePath))
			{
				System.Array.Empty<HsnTariffEXCListDutyRate>();
			}

			var xlsFile = new XlsFile(InputFilePath) { ActiveSheet = 1 };
			var rates = new List<HsnTariffEXCListDutyRate>();

			var rowCount = xlsFile.RowCount;
			for (var row = 2; row <= rowCount; row++)
			{
				var hsnTariffExcListDutyRate = GetRate(xlsFile, row);

				if (string.IsNullOrWhiteSpace(hsnTariffExcListDutyRate.TariffNo))
				{
					continue;
				}

				rates.Add(hsnTariffExcListDutyRate);
			}

			return rates.ToArray();
		}

		HsnTariffEXCListDutyRate GetRate(XlsFile xlsFile, int row)
		{
			var exemptedTariffCodesCell = xlsFile.GetTrimmedStringFromCell(row, ExemptedTariffCodesColumnNumber);
			string exemptedTariffCodeSplitter = exemptedTariffCodesCell.Contains(",") ? "," : ";";

			string rateFormula = xlsFile.GetTrimmedStringFromCell(row, RateFormulaColumnNumber);
			if (this is HsnTariffEXCListIIIDutyRatesLoader == false)
			{
				rateFormula = rateFormula.Replace(",", ".");
			}

			return new HsnTariffEXCListDutyRate(
				tariffNo: xlsFile.GetTrimmedStringFromCell(row, TariffNoColumnNumber).Replace(TariffFormatSplitter, string.Empty),
				description: xlsFile.GetTrimmedStringFromCell(row, DescriptionColumnNumber),
				dutyAmount: xlsFile.GetTrimmedStringFromCell(row, DutyAmountColumnNumber).Replace(",", "."),
				uomCU3: UomCU3ColumnNumber == default ? null : xlsFile.GetTrimmedStringFromCell(row, UomCU3ColumnNumber),
				uomCU4: UomCU4ColumnNumber == default ? null : xlsFile.GetTrimmedStringFromCell(row, UomCU4ColumnNumber),
				uomCU5: UomCU5ColumnNumber == default ? null : xlsFile.GetTrimmedStringFromCell(row, UomCU5ColumnNumber),
				exemptedTariffCodes: exemptedTariffCodesCell.Replace(TariffFormatSplitter, string.Empty).ClearWhiteSpaces().Split(new string[] { exemptedTariffCodeSplitter }, StringSplitOptions.None).EmptyIfNoInvalid(),
				additionalCode: xlsFile.GetTrimmedStringFromCell(row, AdditionalCodeColumnNumber),
				rateType: xlsFile.GetTrimmedStringFromCell(row, RateTypeColumnNumber),
				rateCode: xlsFile.GetTrimmedStringFromCell(row, RateCodeColumnNumber),
				rateFormula: rateFormula,
				startDate: xlsFile.GetTrimmedStringFromCell(row, StartDateColumnNumber),
				endDate: xlsFile.GetTrimmedStringFromCell(row, EndDateColumnNumber)
			);
		}

		protected const string TariffFormatSplitter = ".";

		readonly StringBuilder logger;
	}
}
