using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.MasterFiles.Business
{
	public abstract class DataLoad
	{
		protected DataLoad()
		{
		}

		public event ProgressChangedEventHandler ProgressChanged;
		public event LogUpdatedEventHandler LogUpdated;
		public event InvalidFileHeaderProcessedEventHandler InvalidFileHeaderProcessed;

		protected virtual void ImportDataCore(string dataLocation, string dataImporting)
		{
			FileName = dataLocation;
			fFileHeaderIsValid = true;
			ProcessCSVData(dataImporting);
			ImportComplete();
		}

		public void ImportData(string dataLocation, string dataImporting)
		{
			ImportDataCore(dataLocation, dataImporting);
		}

		protected virtual void ImportComplete()
		{
		}

		bool ParseHeaderLineSafe(OCsvLine line)
		{
			bool result = false;
			try
			{
				ParseHeaderLine(line);
				result = true;
			}
			catch (ArgumentException ex)
			{
				DisplayLogMessage(ex.Message);
			}
			return result;
		}

		protected virtual void ParseHeaderLine(OCsvLine line)
		{
		}

		public bool FileHeaderIsValid
		{
			get { return fFileHeaderIsValid; }
		}

		bool fFileHeaderIsValid;

		#region Log

		public IReadOnlyList<string> Log => LogInternal;

		List<string> LogInternal => logInternal ??= new List<string>();

		List<string> logInternal;

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory
					{
						RefreshEnabled = false
					};
				}
				return factory;
			}
		}

		/// <summary>
		/// Performance drops as the factory gets thousands of records,
		/// plus we only want to lose an individual record if the save fails, so we need to dump out the factory to DB every record.
		/// </summary>
		protected void SaveAndRenewFactory()
		{
			SaveAndRenewFactory(Guid.Empty, "");
		}

		protected void SaveAndRenewFactory(Guid transPK, string table)
		{
			if (factory != null)
			{
				try
				{
					Factory.Save();
					if ((transPK != Guid.Empty) && (!string.IsNullOrEmpty(table)))
					{
						AddDataImportEvent(transPK, table);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					DisplayLogMessage(ex.Message);
				}

				GCWrapper.ReclaimMemory(ref factory);
			}
		}

		BusinessObjectFactory factory;

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
			public int RecsToUpdate;
			public int RecsCreated;
			public int RecsUpdated;
			public int RecsExcluded;
		}

		public Counters RunCounters
		{
			get
			{
				return fRunCounters ?? (fRunCounters = new Counters());
			}
		}

		public void ClearRunCounters()
		{
			fRunCounters = null;
		}

		Counters fRunCounters;

		#endregion

		string FileName;

		#region ImportFrom .csv file

		protected void ProcessCSVData(string dataType)
		{
			ClearRunCounters();
			SetupBeforeImport();
			SaveAndRenewFactory();
			RunCounters.RecordsToImportPlusHeader = CountRows(dataType);

			using (StreamReader sr = new StreamReader(FileName, System.Text.Encoding.Default, true))
			{
				ProcessCSVDataCore(sr, dataType);
			}

			OutputFinalTotals(dataType);
			RunSubsequentDataParsingIfRequired();
		}

		protected virtual void ProcessCSVDataCore(StreamReader sr, string dataType)
		{
			OCsvLine firstLine = new OCsvLine(sr.ReadLine());

			if (ParseHeaderLineSafe(firstLine))
			{
				fFileHeaderIsValid = IsFileHeaderValid(firstLine);

				if (!fFileHeaderIsValid)
				{
					DisplayLogMessage(Res.GetString("df905324-ddd0-4694-ae1a-b8c54e43918d", "File Header information is incorrect. The import of {0} data requires a specific .CSV format file.", dataType));
					if (InvalidFileHeaderProcessed != null)
					{
						InvalidFileHeaderProcessed(this, EventArgs.Empty);
					}
				}
				else
				{
					string currentLine;
					while ((currentLine = GetNextLine(sr)) != null)
					{
						RunCounters.CurrentRow++;
						ProcessDataForThisLine(new OCsvLine(currentLine));
					}
				}
			}
		}

		protected int CountRows(string dataType)
		{
			int recordCount = 0;

			using (StreamReader sr = new StreamReader(FileName))
			{
				while (GetNextLine(sr) != null)
				{
					recordCount++;
				}
			}

			DisplayLogMessage(Res.GetString("81fdd1df-37bf-48e1-979f-c03d9a4acb92", "{0} to Import = {1}", Grammar.Instance.Pluralize(dataType), RecordsToImport(recordCount).ToString()));

			return recordCount;
		}

		static string GetNextLine(StreamReader sr)
		{
			return ImportWizard.GetNextLine(sr);
		}

		protected virtual int RecordsToImport(int recordCount)
		{
			return recordCount - 1;
		}

		#endregion

		#region Utilities

		protected void UpdateAndDisplayIfRequired(Guid transPK, string table)
		{
			if (RunCounters.RecsToUpdate > 0)
			{
				SaveAndRenewFactory(transPK, table);
				RunCounters.RecsToUpdate = 0;
			}

			if ((RunCounters.RecsExcluded % 500) == 0)
			{
				SaveAndRenewFactory();
			}

			OnProgressChanged();
		}

		protected void OnProgressChanged()
		{
			if (ProgressChanged != null)
			{
				ProgressChanged(this, new ProgressChangedEventArgs(RunCounters.RecordsToImportPlusHeader, RunCounters.CurrentRow, Log));
			}
		}

		public void DisplayLogMessage(string logMessageToAdd)
		{
			LogInternal.Add(logMessageToAdd);

			if (LogUpdated != null)
			{
				LogUpdated(this, new LogUpdatedEventArgs(logMessageToAdd));
			}
		}

		protected void DisplayExcludedRecordMessage(string logMessage)
		{
			RunCounters.RecsExcluded++;
			DisplayLogMessage(Res.GetString("9a6e187d-c48a-4d93-aede-06f7220d68fd", "Row {0}{1}", RunCounters.CurrentRow.ToString(), logMessage));
		}

		protected void AddDataImportEvent(Guid transactionPK, string tableName)
		{
			EventManager eventInserter = new EventManager();
			eventInserter.AddAuditLogEvent(AutoEvents.DataImport.Code, tableName, transactionPK, Env.Time.CurrentLocalDateTime);
		}

		#endregion

		#region Virtual methods

		protected virtual void SetupBeforeImport()
		{
		}

		protected virtual void RunSubsequentDataParsingIfRequired()
		{
		}

		protected virtual void OutputFinalTotals(string dataType)
		{
			DisplayLogMessage("\r\n" + Res.GetString("5b5200a6-5326-4cf0-bc19-970067a6ecc5", "T O T A L : {0} created = {1}, {0} updated = {2}, {0} excluded = {3}", Grammar.Instance.Pluralize(dataType), RunCounters.RecsCreated, RunCounters.RecsUpdated, RunCounters.RecsExcluded) + "\r\n");
		}

		#endregion

		#region Abstract methods

		protected abstract bool IsFileHeaderValid(OCsvLine line);

		public bool HasCSVTemplateHeading
		{
			get { return !string.IsNullOrEmpty(CSVTemplateHeading); }
		}

		public abstract string CSVTemplateHeading { get; }

		public void WriteCSVTemplate(string filePath)
		{
			using (var streamWriter = new StreamWriter(filePath))
			{
				streamWriter.Write(CSVTemplateHeading);
				streamWriter.Flush();
			}
		}

		public void WriteCSVTemplate(Stream stream)
		{
			using (var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8))
			{
				streamWriter.Write(CSVTemplateHeading);
				streamWriter.Flush();
			}
		}

		protected abstract void ProcessDataForThisLine(OCsvLine line);

		#endregion
	}

	#region Event Handlers

	public delegate void ProgressChangedEventHandler(DataLoad sender, ProgressChangedEventArgs e);

	public class ProgressChangedEventArgs : EventArgs
	{
		public ProgressChangedEventArgs(int recordsImporting, int currentRow, IReadOnlyCollection<string> log)
		{
			this.RecordsImporting = recordsImporting;
			this.CurrentRow = currentRow;
			this.Log = log;
		}

		public readonly int RecordsImporting;
		public readonly int CurrentRow;
		public readonly IReadOnlyCollection<string> Log;
	}

	public delegate void LogUpdatedEventHandler(DataLoad sender, LogUpdatedEventArgs e);

	public class LogUpdatedEventArgs : EventArgs
	{
		public LogUpdatedEventArgs(string logMessage)
		{
			this.LogMessage = logMessage;
		}

		public readonly string LogMessage;
	}

	public delegate void InvalidFileHeaderProcessedEventHandler(DataLoad sender, EventArgs e);

	#endregion
}
