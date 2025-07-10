using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class DummyXmlSessionTracker : IXmlSessionTracker
	{
		public DummyXmlSessionTracker(IDataWritingManager outboundSessionTracker) : base() { this.outboundSessionTracker = outboundSessionTracker; }

		IEnumerable<IImportResult> IXmlSessionTracker.ImportResults
		{
			get { throw new NotImplementedException(); }
		}

		void IXmlSessionTracker.LogChildTopLevelObject(DataContextType dataContextType, Func<string> getDataContextKey)
		{
			throw new NotImplementedException();
		}

		void IXmlSessionTracker.LogLinkCreated(IEntityID parentEntityID, IEntityID childEntityID)
		{
			throw new NotImplementedException();
		}

		IDataWritingManager IXmlSessionTracker.OutboundSessionTracker
		{
			get { return outboundSessionTracker; }
		}
		readonly IDataWritingManager outboundSessionTracker;

		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

		INotificationEmailManager IXmlSessionTracker.NotificationEmailManager
		{
			get { return notificationEmailManager ?? (notificationEmailManager = new NotificationEmailManager()); }
		}
		INotificationEmailManager notificationEmailManager;

		void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
			throw new NotImplementedException();
		}

		bool IXmlImportLogger.OrgMatchingDisabled => false;

		bool IXmlImportLogger.HasIgnoredModule
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		bool IXmlImportLogger.IsUpdatingConsol
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		void IXmlImportLogger.LogBoth(LogType type, string message)
		{
			throw new NotImplementedException();
		}

		void IXmlImportLogger.LogErrorToServiceTaskOnly(string message)
		{
			throw new NotImplementedException();
		}

		void IXmlImportLogger.LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
			throw new NotImplementedException();
		}

		ITopLevelDataObject IXmlImportLogger.TopLevelDataObject
		{
			get { throw new NotImplementedException(); }
		}

		IDataContextDataObject IXmlImportLogger.TopLevelDataContext
		{
			get { throw new NotImplementedException(); }
		}

		public IDisposable SetCurrentMessageContext(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public bool IsCurrentMessageContextSet(ZGuid messagePK)
		{
			throw new NotImplementedException();
		}

		public void StartProcessingASubShipment()
		{
			throw new NotImplementedException();
		}

		public void EndProcessingASubShipment()
		{
			throw new NotImplementedException();
		}

		public void RecordUsedAddressTypeInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			throw new NotImplementedException();
		}

		public void RecordAUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			throw new NotImplementedException();
		}

		public void RecordAnUnknownAddressTypeWarningInCurrentUnknownAddressTypeWarningInfoKeeper(string addressType)
		{
			throw new NotImplementedException();
		}

		void ISimpleLogger.Log(LogType type, string message)
		{
			throw new NotImplementedException();
		}

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs
		{
			get { throw new NotImplementedException(); }
		}

		public IEDIMessage SourceMessage { get; set; }

		public bool HasErrors => throw new NotImplementedException();

		public bool HasWarnings => throw new NotImplementedException();
		public IUserContext SessionUserContext { get; set; }
	}
}
