using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.eTail.DataTransfer
{
	public class ConvertTracker : IXmlImportLogger, IDataWritingInformationCollector
	{
		public ConvertTracker(int total, CancellationToken token, Action<string, string, int> updateAction, ILogger logger = null)
		{
			this.total = total;
			this.token = token;
			this.updateAction = updateAction;
			this.logger = logger;
		}

		readonly int total;
		readonly CancellationToken token;
		readonly Action<string, string, int> updateAction;
		readonly ILogger logger;

		public void LogBoth(LogType type, string message)
		{
			var both = "[BOTH] ";
			Log(type, both + message);
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
			if (token != default)
			{
				token.ThrowIfCancellationRequested();
			}

			if (proccessedCount < total && targetBO is not BaseJobDeclaration)
			{
				UpdateProgress(Res.GetString(
						"85ec1968-31cc-4bbe-ab06-62f82156df6c", "[{0} / {1}] {2} generated",
						++proccessedCount,
						total,
						targetBO.HumanReadableShortcutName),
						(int)(proccessedCount * 100F / total));
			}
			else
			{
				UpdateProgress(Res.GetString(
						"b113f16d-7d68-4509-995e-832f5e80790a", "{0} generated",
						targetBO.HumanReadableShortcutName),
						100);
			}
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
		}

		public bool IsUpdatingConsol { get; set; }
		public bool HasIgnoredModule { get; set; }
		public bool OrgMatchingDisabled => false;
		public ITopLevelDataObject TopLevelDataObject { get; set; }
		public IDataContextDataObject TopLevelDataContext => TopLevelDataObject.DataContext;

		public bool HasError => Logs.Any(l => l.Type == LogType.Error);

		public void Log(LogType type, string message)
		{
			logs.Add(new SimpleLog(type, message));

			if (logger != null)
			{
				logger.Log(type, message);
			}
		}

		public IEnumerable<ISimpleLog> Logs => logs;
		readonly List<ISimpleLog> logs = new List<ISimpleLog>();

		int proccessedCount;

		void IDataWritingInformationCollector.NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
			UpdateLoadingProgress(businessObject);
		}

		public void UpdateLoadingProgress(BusinessObject businessObject)
		{
			if (token != default)
			{
				token.ThrowIfCancellationRequested();
			}

			if (businessObject is HVLVConsignment)
			{
				UpdateProgress(Res.GetString(
					"f14d1168-014f-4168-b0d4-f1a202af7cc1", "[{0} / {1}] HVLV Consignments loaded",
					++proccessedCount, total),
					(int)(proccessedCount * 100F / total));
			}
		}

		public void UpdateProgress(string message, int progress = 0)
		{
			updateAction?.Invoke(string.Empty, message, progress);
		}

		public void UpdateCaption(string caption)
		{
			updateAction?.Invoke(caption, string.Empty, 0);
		}

		public void ResetCount()
		{
			proccessedCount = 0;
		}

		public string GetErrorDetail()
		{
			return string.Join("\r\n", Logs.Where(n => n.Type == LogType.Error).Select(n => n.Message).ToArray());
		}

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
	}
}
