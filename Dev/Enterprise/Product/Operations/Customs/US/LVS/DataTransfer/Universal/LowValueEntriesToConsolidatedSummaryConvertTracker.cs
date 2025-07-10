using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class LowValueEntriesToConsolidatedSummaryConvertTracker : IXmlImportLogger, IDataWritingInformationCollector
	{
		public LowValueEntriesToConsolidatedSummaryConvertTracker(int totalRecords, Action<ZString, int> updateProgressAction)
		{
			this.totalRecordsToTrack = totalRecords;
			this.updateProgressAction = updateProgressAction;
		}

		readonly int totalRecordsToTrack;
		readonly Action<ZString, int> updateProgressAction;
		int processedCount;
		int importedCount;

		public void NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
			if (businessObject is CusUSLVConsignment)
			{
				UpdateProgress(
					Res.GetString("fcb823c0-e80d-45bf-b207-23a77c9a42d7", "[{0} / {1}] invoice lines processed", ++processedCount, totalRecordsToTrack),
					(int)(processedCount * 100F / totalRecordsToTrack));
			}
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
			if (targetBO is JobComInvoiceHeader)
			{
				UpdateProgress(string.Empty, (int)(++importedCount * 100F / totalRecordsToTrack));
			}
		}

		void UpdateProgress(string message, int progressPercentage)
		{
			updateProgressAction?.Invoke(message, progressPercentage);
		}

		#region Not Implemented

		public bool IsUpdatingConsol { get; set; }
		public bool HasIgnoredModule { get; set; }
		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		public ITopLevelDataObject TopLevelDataObject => null;

		public IDataContextDataObject TopLevelDataContext => null;

		public bool OrgMatchingDisabled => false;

		public IEnumerable<ISimpleLog> Logs => null;

		public void Log(LogType type, string message)
		{
		}

		public void LogBoth(LogType type, string message)
		{
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
		}

		#endregion
	}
}
