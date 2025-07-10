using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class DummyWithWorkflowDataContextManager : ShipmentDataContextManager<DummyWithWorkflow>, IParentEventDataContextManager, IEventDataContextManagerWithTriggeringLog, ITransactionDataContextManager, IScheduleDataContextManager, ITransactionBatchDataContextManager, IActivityDataContextManager
	{
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			IEventDataContextManagerWithTriggeringLog contextManager = this;
			if (contextManager.TriggeringLogForUseInPopulatingEventContext != null
				&& contextManager.TriggeringLogForUseInPopulatingEventContext.SL_Reference == "TestTriggerLogInEventContextPopulation")
			{
				result.Add(new KeyValuePair<TypeWithDescription, IZType>(
					new TypeWithDescription("TriggerLogTime"),
					new ZString(contextManager.TriggeringLogForUseInPopulatingEventContext.SL_EventTime.ToISO8601String())));
			}

			result.AddRange(new[] {
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("CubbyHouseBill"), new ZString("WAWAWA342892382")),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("DummyZDecimal"), new ZDecimal(348.534m)),
			}.Concat(AdditionalContextKeyValuePairs));

			return result;
		}

		protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRoleType, IOrgHeader recipientOrganisation)
		{
			if (ParentBO != null)
			{
				switch (recipientRoleType)
				{
					case RecipientRoleType.FOR:
						return new DummyLink[] { new DummyLink() };
				}
			}

			return base.GetEventDataTargetCore(recipientRoleType, recipientOrganisation);
		}

		#region AdditionalContextKeyValuePairs

		public static IDisposable SetupAdditionalContextKeyValuePairs(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> additionalValues)
		{
			return new DisposableAction(() => AdditionalContextKeyValuePairs = additionalValues, () => AdditionalContextKeyValuePairs = Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>());
		}

		static IEnumerable<KeyValuePair<TypeWithDescription, IZType>> AdditionalContextKeyValuePairs
		{
			get => additionalContextKeyValuePairs ?? (additionalContextKeyValuePairs = Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>());
			set => additionalContextKeyValuePairs = value;
		}

		[ThreadStatic]
		static IEnumerable<KeyValuePair<TypeWithDescription, IZType>> additionalContextKeyValuePairs;

		#endregion

		public static Overridable<Func<BusinessObjectFactory, ZQuery, DummyWithWorkflow[]>> LoadDummy { get; } = new Overridable<Func<BusinessObjectFactory, ZQuery, DummyWithWorkflow[]>>();

		protected override DummyWithWorkflow[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery query)
		{
			if (LoadDummy.IsOverriden)
			{
				return LoadDummy.Value(factory, query);
			}
			else
			{
				return base.LoadBusinessObjects(factory, query);
			}
		}
		protected override IEnumerable<KeyValuePair<IZType, IZType>> GetAdditionalFieldsToUpdateValues()
		{
			return new[] { new KeyValuePair<IZType, IZType>(new ZString("DummyWithWorkflow.Z0_Description"), new ZString("TEST DESCRIPTION")) };
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DummyWithWorkflowEventParentFinder(factory, this, logger);
		}

		class DummyLink : IUniversalJobLink
		{
			public ZString EnterpriseCode => "ABC";

			public ZString ServerCode => "DEF";

			public ZString CompanyCode => "GHI";

			public DataContextType Context => DataContextType.ForwardingShipment;

			public ZString Key => "ShipmentID";
		}

		class DummyWithWorkflowEventParentFinder : EventParentFinder
		{
			internal DummyWithWorkflowEventParentFinder(BusinessObjectFactory factory, DummyWithWorkflowDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				return null;
			}
		}

		static readonly Overridable<int> ThrowExceptionWithDataContextKeyMatchingCount = new Overridable<int>(0);

		public static void SetThrowExceptionWithDataContextKeyMatchingCount(int value)
		{
			ThrowExceptionWithDataContextKeyMatchingCount.Value = value;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			if (ThrowExceptionWithDataContextKeyMatchingCount.Value-- > 0)
			{
				throw new MessageProcessingBusinessFailureException("This message failed because I wanted it to fail.", "Reasons", true);
			}

			return new ZQuery(DummyBizoSchema.Z0_Description, matchingValues.Key);
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.DummyBusinessObject; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.Z0_Description; }
		}

		public override string DefaultOutputDirectory
		{
			get { return DefaultOutputDirectoryForTesting; }
		}

		[ThreadStatic]
		public static string DefaultOutputDirectoryForTesting;

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return false;
		}

		public override bool ManagesShipments => ParentWithUniversalXmlManagement?.ManagesShipments ?? true;

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return DummyWriterDeciderManager.GetWriterDecider().GetWriter();
		}

		public static IDisposable SetWriterDecider(DummyWriterDecider writerDecider)
		{
			return new DummyWriterDeciderManager(writerDecider);
		}

		public class DummyWriterDecider
		{
			public virtual ITopLevelDataObjectWriter GetWriter()
			{
				return new DummyWithWorkflowShipmentDataObjectWriter();
			}

			public virtual IEventDataObjectWriter GetEventWriter(IDataWritingManager writeManager)
			{
				return ObjectFactory.New<IEventDataObjectWriter>(writeManager);
			}
		}

		class DummyWriterDeciderManager : IDisposable
		{
			public DummyWriterDeciderManager(DummyWriterDecider writerDecider)
			{
				WriterDecider = writerDecider;
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			internal static DummyWriterDecider GetWriterDecider()
			{
				return WriterDecider;
			}

			static DummyWriterDecider WriterDecider
			{
				get => writerDecider ?? (writerDecider = new DummyWriterDecider());
				set => writerDecider = value;
			}
			[ThreadStatic]
			static DummyWriterDecider writerDecider;

			void IDisposable.Dispose()
			{
				WriterDecider = new DummyWriterDecider();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		public override bool ManagesEvents => ParentWithUniversalXmlManagement?.ManagesEvents ?? true;

		public IEventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager writeManager)
		{
			return DummyWriterDeciderManager.GetWriterDecider().GetEventWriter(writeManager);
		}

		public bool ManagesTransactions => ParentWithUniversalXmlManagement?.ManagesTransactions ?? true;

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DummyWithWorkflowTransactionDataObjectWriter();
		}

		public bool ManagesSchedules => ParentWithUniversalXmlManagement?.ManagesSchedules ?? true;

		public ITopLevelDataObjectWriter GetScheduleDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DummyWithWorkflowScheduleDataObjectWriter();
		}

		DummyWithWorkflowAndUniversalXmlManagement ParentWithUniversalXmlManagement => ParentBO as DummyWithWorkflowAndUniversalXmlManagement;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		public IEnumerable<IEventDataContextManager> ChildContextManagers
		{
			get { return Enumerable.Empty<IEventDataContextManager>(); }
		}

		BaseStmALog IEventDataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext
		{
			get;
			set;
		}

		#region ITransactionBatchDataContextManager Members

		public ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public bool ManagesTransactionBatches
		{
			get { return true; }
		}

		public bool UseIncomingTransactionBatchData(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			var transactionBatch = (TransactionBatch)dataObject;
			return transactionBatch != null && transactionBatch.BatchType != null && transactionBatch.BatchType.Code.GetValueOrDefault() == "TST";
		}

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			var transactionBatch = (TransactionBatch)dataObject;
			return new KeysResult { IsMatch = transactionBatch != null && transactionBatch.BatchType.Code.GetValueOrDefault() == "TST" };
		}

		class KeysResult : IKeysResult
		{
			public IEnumerable<(string KeyValue, string KeySource)> KeysInfo => Enumerable.Empty<(string KeyValue, string KeySource)>();
			public bool IsMatch { get; set; }
			public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);
		}

		#endregion

		#region IActivityDataContextManager Members

		public bool DoesManageDataContextType(DataContextType type)
		{
			return false;
		}

		public ITopLevelDataObjectWriter GetActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems)
		{
			return new UniversalXmlWorkflowProcessorTest.DummyDataObjectWriterWithDataObjectValidationException();
		}

		public ITopLevelDataObjectReader GetActivityDataObjectReader(ITopLevelDataObject activity, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return null;
		}

		public bool ManagesActivities => ParentWithUniversalXmlManagement?.ManagesActivities ?? true;

		#endregion
	}
}
