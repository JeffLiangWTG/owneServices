using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public class TestLogger : IXmlImportLogger
	{
		public TestLogger(string orgCode)
		{
			TopLevelDataContext.DataProviderForCodeMapping = orgCode;
		}

		public TestLogger() { }

		public bool IsUpdatingConsol { get; set; }

		public bool HasIgnoredModule { get; set; }

		public bool OrgMatchingDisabled => OrgMatchingDisabledCore;

		protected virtual bool OrgMatchingDisabledCore => false;

		public ITopLevelDataObject TopLevelDataObject { get; set; }

		IDataContextDataObject topLevelDataContext;

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		public IEnumerable<ISimpleLog> Logs => logs;

		public string LogOutput => string.Join("\r\n", logs.Select((TestLog log) => log.Type.ToString() + " - " + log.Message));

		public IDataContextDataObject TopLevelDataContext
		{
			get
			{
				topLevelDataContext ??= DataContextFactory.New();
				return topLevelDataContext;
			}
		}

		readonly List<TestLog> logs = new List<TestLog>();

		public void Log(LogType type, string message)
		{
			logs.Add(new TestLog(type, message));
		}

		public void LogBoth(LogType type, string message)
		{
			logs.Add(new TestLog(type, message));
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
		}

		public void LogTopLevelDataContextKey(GetDataContextKey dataContextKey)
		{
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
		}
	}

	class TestLog : ISimpleLog
	{
		internal TestLog(LogType type, string message)
		{
			Type = type;
			Message = message;
		}

		public LogType Type { get; }

		public string Message { get; }

		LogType ISimpleLog.Type => Type;
	}
}
