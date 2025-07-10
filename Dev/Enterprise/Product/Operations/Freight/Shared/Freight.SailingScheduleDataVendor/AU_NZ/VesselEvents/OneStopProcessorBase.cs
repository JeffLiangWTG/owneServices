using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public abstract partial class OneStopProcessorBase
	{
		#region Process

		protected bool ProcessMailItem(ZGuid mailItemPK, ILogger logger)
		{
			try
			{
				var attempts = 5;
				for (int i = attempts - 1; i >= 0; i--)
				{
					try
					{
						return ProcessMailItemCore(mailItemPK, logger);
					}
					catch (ZSaveConcurrencyException)
					{
						if (i == 0)
						{
							throw;
						}
					}
					finally
					{
						FactoryProvider.CreateNewWithoutSave();
					}
				}
			}
			catch (ZSaveConcurrencyException)
			{
				logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "{0} : Failed due to ConcurrencyError. You can reprocess from Emails module", FreightConstants.VesselDataProviderNames.OneStop));
			}
			catch (ZSaveException ex)
			{
				ReportUnexpectedlyDeletedJobSailing(ex);

				throw;
			}

			return false;
		}

		bool ProcessMailItemCore(ZGuid mailItemPK, ILogger logger)
		{
			var mailItem = Factory.Load<MailItem>(mailItemPK);
			if (mailItem == null)
			{
				return false;
			}

			var result = ProcessAttachments(mailItem);
			if (result)
			{
				Factory.Save();
				LogResults(logger);
				FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Now;
			}

			return result;
		}

		bool ProcessAttachments(MailItem itemToProcess)
		{
			var result = true;

			recordsAddedDictionary = [];
			recordsUpdatedDictionary = [];
			recordsRejectedDictionary = [];

			foreach (MailAttachment attachment in itemToProcess.MailAttachments)
			{
				if (attachment.MA_FileName.EndsWith((NoResString)"zip") && !ProcessZipFile(attachment))
				{
					result = false;
				}
			}

			return result;
		}

		bool ProcessZipFile(MailAttachment attachment)
		{
			using (var extractPath = new TempDirectory())
			{
				var fullFilePath = attachment.SaveTo(extractPath.DirectoryName);

				try
				{
					ZipFile.ExtractToDirectory(fullFilePath, extractPath.DirectoryName);
				}
				catch (InvalidDataException invalidDataException)
				{
					ReportErrorIfUAT("Invalid1StopSailingScheduleAttachedItem", (NoResString)"The attached item is invalid: " + invalidDataException.Message);
					return false;
				}
				catch (FileNotFoundException fileNotFoundException)
				{
					string errorReporterMessage = string.Empty;

					if (!Directory.Exists(extractPath))
					{
						errorReporterMessage += ((NoResString)"Directory does not exist: " + extractPath + (NoResString)"\r\n");
					}
					else if (!File.Exists(fullFilePath))
					{
						errorReporterMessage += ((NoResString)"File does not exist: " + fullFilePath + (NoResString)"\r\n");
					}

					ErrorReporter.ReportOnce("The zip file for 1-stop schedules email was not found. Please inform IL team - WI00177419",
						errorReporterMessage + fileNotFoundException.Message,
						fileNotFoundException);

					return false;
				}

				ProcessFiles(extractPath.DirectoryName);

				return true;
			}
		}

		void ProcessFiles(string filePath)
		{
			var filesToProcess = Directory.GetFiles(filePath, "*.tmp");
			Array.Sort(filesToProcess);
			foreach (string file in filesToProcess)
			{
				ProcessFile(file);
			}
		}

		protected void ProcessFile(string fileName)
		{
			using (var reader = new StreamReader(fileName))
			{
				var records = new List<string[]>();
				string csvLine;
				while ((csvLine = reader.ReadLine()) != null)
				{
					records.Add(ConvertCSVStringToArray(csvLine));
				}

				ProcessRecords(records);
			}
		}

		protected abstract void ProcessRecords(List<string[]> records);

		#endregion

		#region Helper Methods

		protected static ZString TrimToMaxLength(SchemaStringColumn column, ZString value)
		{
			int maxLength = column.MaxLength;
			int length = value.Length;

			if (length > maxLength)
			{
				return value.SubstringSafe(0, maxLength);
			}

			return value;
		}

		string[] ConvertCSVStringToArray(string csvRecord)
		{
			var csvLine = new OCsvLine(csvRecord, ',');
			var fieldValues = Array.ConvertAll(csvLine.FieldValues, v => v.Trim());

			return fieldValues;
		}

		#endregion

		#region Implementation

		#region Factory

		protected BusinessObjectFactory Factory => FactoryProvider.Current;

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider();
					factoryProvider.CreateNewWithoutSave();
					factoryProvider.Current.RefreshEnabled = true;
					factoryProvider.Current.NameForDebugging = "One Stop Processor Factory";
				}

				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion

		#region Logging

		void LogResults(ILogger logger)
		{
			if (!recordsAddedDictionary.Keys.Any())
			{
				logger.Log(LogType.Information,
					string.Format(CultureInfo.InvariantCulture, "{0} : No records have been added or updated in any table", FreightConstants.VesselDataProviderNames.OneStop));
			}
			else
			{
				foreach (var tableName in recordsAddedDictionary.Keys)
				{
					logger.Log(LogType.Information,
						string.Format(CultureInfo.InvariantCulture, "{0} : {1} records have been added to the {2} table", FreightConstants.VesselDataProviderNames.OneStop, recordsAddedDictionary[tableName], tableName));
				}

				foreach (var tableName in recordsUpdatedDictionary.Keys)
				{
					logger.Log(LogType.Information,
						string.Format(CultureInfo.InvariantCulture, "{0} : {1} records have been updated in the {2} table", FreightConstants.VesselDataProviderNames.OneStop, recordsUpdatedDictionary[tableName], tableName));
				}

				foreach (var tableName in recordsRejectedDictionary.Keys)
				{
					logger.Log(LogType.Warning,
						string.Format(CultureInfo.InvariantCulture, "{0} : {1} records have been rejected during updating the {2} table", FreightConstants.VesselDataProviderNames.OneStop, recordsRejectedDictionary[tableName], tableName));
				}
			}
		}

		protected void IncreaseAddedRecords(string tableName)
		{
			IncreaseValueInDictionary(tableName, recordsAddedDictionary);
		}

		protected void IncreaseUpdatedRecords(string tableName)
		{
			IncreaseValueInDictionary(tableName, recordsUpdatedDictionary);
		}

		protected void DecreaseAddedRecords(string tableName)
		{
			DecreaseValueInDictionary(tableName, recordsAddedDictionary);
		}

		protected void DecreaseUpdatedRecords(string tableName)
		{
			DecreaseValueInDictionary(tableName, recordsUpdatedDictionary);
		}

		protected void IncreaseRejectedRecords(string tableName)
		{
			IncreaseValueInDictionary(tableName, recordsRejectedDictionary);
		}

		void IncreaseValueInDictionary(string key, Dictionary<string, int> dictionary)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				dictionary[key] = ++value;
			}
			else
			{
				dictionary.Add(key, 1);
			}
		}

		void DecreaseValueInDictionary(string key, Dictionary<string, int> dictionary)
		{
			if (!dictionary.TryGetValue(key, out var value))
			{
				return;
			}
			dictionary[key] = --value;
		}

		Dictionary<string, int> recordsAddedDictionary;
		Dictionary<string, int> recordsUpdatedDictionary;
		Dictionary<string, int> recordsRejectedDictionary;

		#endregion

		#region Error Reporting

		public static void ReportErrorIfUAT(string key, string message)
		{
			if (ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode == "HYE")
			{
				ErrorReporter.ReportOnce(key, message);
			}
		}

		static void ReportUnexpectedlyDeletedJobSailing(ZSaveException ex)
		{
			var row = ex.Row;

			if (row == null)
			{
				return;
			}

			var tableName = row.Table.TableName;
			var rowState = row.RowState;

			if (tableName != AutoJobSailing.Schema.TableName || rowState != DataRowState.Deleted)
			{
				return;
			}

			var pk = ZDataUtils.GetPK(row);
			var deleteStackTrace = JobSailing.GetDeleteStackTrace(pk);

			ErrorReporter.ReportOnce("JobSailing.Delete() Stacktrace for CS00968241",
				string.IsNullOrEmpty(deleteStackTrace)
					? $"No Stacktrace found for JobSailing with PK={pk}."
					: deleteStackTrace);
		}

		#endregion

		protected string DataProvider => FreightConstants.VesselDataProviders.OneStop;
		protected ZDateTime oldestDepartureDate = ZDateTime.Now.AddMonths(-6);
		protected ZDateTime oldestArrivalDate = ZDateTime.Now.AddMonths(-3);

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.SailingDataVendor.Business
{
	partial class OneStopProcessorBase
	{
		public void ProcessMailItemForTest(MailItem itemToProcess)
		{
			try
			{
				itemToProcess.MI_Status = ProcessAttachments(itemToProcess) ? MailStatus.Processed : MailStatus.Failed;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				itemToProcess.MI_Status = MailStatus.Failed;
				throw;
			}
			finally
			{
				itemToProcess.Factory.Save();
				Factory.Save();
			}
		}
	}
}

#endif
#endregion
