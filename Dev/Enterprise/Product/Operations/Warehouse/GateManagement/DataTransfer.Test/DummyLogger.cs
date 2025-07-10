using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	class DummyLogger : IXmlImportLogger
	{
		readonly List<DummyLog> notifications = new ();

		public void Log(LogType type, string message)
		{
			notifications.Add(new DummyLog(type, message));
		}

		public void LogBoth(LogType type, string message)
		{
			notifications.Add(new DummyLog(type, message));
		}

		public string LogsString => string.Join("\r\n", notifications.Select(log => log.Type.ToString() + " - " + log.Message));

		public bool OrgMatchingDisabled => false;

		#region Not Implemented

		public bool IsUpdatingConsol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool HasIgnoredModule { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public IEnumerable<IValidationRule> ValidationRuleCollection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ITopLevelDataObject TopLevelDataObject => throw new NotImplementedException();
		IDataContextDataObject topLevelDataContext;
		public IDataContextDataObject TopLevelDataContext => topLevelDataContext ?? (topLevelDataContext = DataContextFactory.New());
		public IEnumerable<ISimpleLog> Logs => notifications;
		public void FireDataImportedToBusinessObject(BusinessObject targetBO) { }
		public void LogErrorToServiceTaskOnly(string message) => throw new NotImplementedException();
		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey) => throw new NotImplementedException();

		#endregion
	}

	class DummyLog : ISimpleLog
	{
		public DummyLog(LogType type, string message)
		{
			_type = type;
			_message = message;
		}

		readonly LogType _type;
		readonly string _message;

		public LogType Type => _type;

		public string Message => _message;
	}
}
