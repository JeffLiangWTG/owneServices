using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class RefExchangeRateDataImport
	{
		#region Constants

		static class Constants
		{
			public const int MinimumRows = 25;
			public const int MinimumColumns = 9;
			public const string HeadingCurrencyCode = "CURRENCY CODE";
			public const string HeadingExchangeRate = "EXCHANGE RATE";

			public const int CurrencyCodeColumnIndex = 1;
			public const int CurrencyUnitColumnIndex = 4;
			public const int ExchangeRateColumnIndex = 5;
			public const int EffectiveDateColumnIndex = 7;
			public const int ExpiryDateColumnIndex = 8;
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory fFactory;

		#endregion

		#region Counters

		public sealed class Counters
		{
			public Counters()
			{
				CurrentRow = 1;
			}

			public int CurrentRow;
			public int RecordsToImportPlusHeader;
		}

		public Counters RunCounters
		{
			get
			{
				if (fRunCounters == null)
				{
					fRunCounters = new Counters();
				}
				return fRunCounters;
			}
		}

		Counters fRunCounters;

		#endregion

		#region Log

		public StringCollection Log
		{
			get
			{
				if (fLog == null)
				{
					fLog = new StringCollection();
				}
				return fLog;
			}
		}

		StringCollection fLog;

		#endregion

		public event ProgressChangedEventHandler ProgressChanged;
		public event LogUpdatedEventHandler LogUpdated;

		protected void OnProgressChanged()
		{
			if (ProgressChanged != null)
			{
				ProgressChanged(this, new ProgressChangedEventArgs(RunCounters.RecordsToImportPlusHeader, RunCounters.CurrentRow, Log));
			}
		}

		public void ImportData(string fileName)
		{
			if (File.Exists(fileName))
			{
				string[][] data = ImportFile(fileName);
				if (IsImportedDataValid(data))
				{
					ProcessData(data);
				}
				else
				{
					DisplayLogMessage("File information is incorrect. The import of customs exchange rate data requires a specific .xls format file: ");
				}
			}
		}

		#region Import and Validate File

		string[][] ImportFile(string fileName)
		{
			string[][] result = null;
			using (ExcelInterface excelDoc = new ExcelInterface())
			{
				excelDoc.LoadExcelFile(fileName);
				excelDoc.ActiveWorksheet = 0;

				maxRow = excelDoc.WorkSheets[0].RowCount;
				int maxCol = excelDoc.WorkSheets[0].ColumnCount;

				result = new string[maxRow][];

				for (int rowIndex = 0; rowIndex < maxRow; rowIndex++)
				{
					string[] columns = new string[maxCol];
					for (int columnIndex = 0; columnIndex < maxCol; columnIndex++)
					{
						columns[columnIndex] = excelDoc.WorkSheets[0][rowIndex, columnIndex].ToString().Trim();
					}
					result[rowIndex] = columns;
				}
				return result;
			}
		}

		bool IsImportedDataValid(string[][] data)
		{
			return data.Length >= Constants.MinimumRows && data[0].Length >= Constants.MinimumColumns && data[0][1].ToUpper() == Constants.HeadingCurrencyCode && data[0][5].ToUpper() == Constants.HeadingExchangeRate;
		}

		#endregion

		void ProcessData(string[][] data)
		{
			RunCounters.RecordsToImportPlusHeader = maxRow;
			string startDateString = data[1][Constants.EffectiveDateColumnIndex];
			string endDateString = data[1][Constants.ExpiryDateColumnIndex];
			DateTime startDate;
			DateTime endDate;
			if (!DateTime.TryParse(startDateString, out startDate) || !DateTime.TryParse(endDateString, out endDate))
			{
				DisplayLogMessage("The source file is corrupt. Please, verify the effective and expiry dates for the exchange rates.");
			}
			else
			{
				endDate = endDate.Add(new TimeSpan(23, 59, 59));
				DisplayLogMessage("Updating Customs exchange rates effective " + startDate.ToShortDateString() + " expiring " + endDate.ToShortDateString());

				for (int rowIndex = 1; rowIndex < maxRow; rowIndex++)   // 0 row is header fields
				{
					string currencyCode = data[rowIndex][Constants.CurrencyCodeColumnIndex].ToUpper();
					decimal exchangeRate = Convert.ToDecimal(data[rowIndex][Constants.ExchangeRateColumnIndex]);
					int currencyUnit = Convert.ToInt16(data[rowIndex][Constants.CurrencyUnitColumnIndex]);
					if (currencyUnit > 1)
					{
						exchangeRate = exchangeRate / currencyUnit;
					}

					LoadNewRate(currencyCode, exchangeRate, startDate, endDate);
					OnProgressChanged();
				}
			}
		}

		void LoadNewRate(string currencyCode, decimal exchangeRate, ZDateTime startDate, ZDateTime endDate)
		{
			RunCounters.CurrentRow++;
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);

			ZQuery exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_StartDate, startDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, endDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			List<RefExchangeRate> exchangeRates = new List<RefExchangeRate>(currency.ExchangeRates.Find(exchangeRateQuery));
			if (exchangeRates.Count == 0)
			{
				RefExchangeRate refExchangeRate = currency.ExchangeRates.AddNew();
				refExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				refExchangeRate.RE_StartDate = startDate;
				refExchangeRate.RE_ExpiryDate = endDate;
				refExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				refExchangeRate.RE_SellRate = exchangeRate;
				Factory.Save();
				DisplayLogMessage("Currency " + currencyCode + " updated with Customs exchange rate " + exchangeRate.ToString());
			}
			else
			{
				DisplayLogMessage("Currency " + currencyCode + " already has a Customs exchange rate for this period.");
			}
		}

		protected int maxRow;

		protected void DisplayLogMessage(string logMessageToAdd)
		{
			Log.Add(logMessageToAdd);

			if (LogUpdated != null)
			{
				LogUpdated(this, new LogUpdatedEventArgs(logMessageToAdd));
			}
		}
	}

	#region Event Handlers

	public delegate void ProgressChangedEventHandler(RefExchangeRateDataImport sender, ProgressChangedEventArgs e);

	public class ProgressChangedEventArgs : EventArgs
	{
		public ProgressChangedEventArgs(int recordsImporting, int currentRow, StringCollection log)
		{
			this.RecordsImporting = recordsImporting;
			this.CurrentRow = currentRow;
			this.Log = log;
		}

		public readonly int RecordsImporting;
		public readonly int CurrentRow;
		public readonly StringCollection Log;
	}

	public delegate void LogUpdatedEventHandler(RefExchangeRateDataImport sender, LogUpdatedEventArgs e);

	public class LogUpdatedEventArgs : EventArgs
	{
		public LogUpdatedEventArgs(string logMessage)
		{
			this.LogMessage = logMessage;
		}

		public readonly string LogMessage;
	}

	#endregion
}
