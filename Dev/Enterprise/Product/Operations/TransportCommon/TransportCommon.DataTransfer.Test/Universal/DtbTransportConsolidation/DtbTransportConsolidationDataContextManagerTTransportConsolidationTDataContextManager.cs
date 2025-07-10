using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportConsolidationDataContextManagerTest<TTransportConsolidation, TDataContextManager> : ShipmentDataContextManagerTestCase<TDataContextManager, TTransportConsolidation>
			where TTransportConsolidation : DtbTransportConsolidation
			where TDataContextManager : DtbTransportConsolidationDataContextManager<TTransportConsolidation>, new()
	{
		#region Context

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new TDataContextManager().DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		#endregion

		#region TestDataContextKey

		public void TestDataContextKey()
		{
			var consolidation = Factory.New<TTransportConsolidation>();
			consolidation.KB_JobID = "CM00001";
			AssertEquals("CM00001", consolidation.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestGetDataContextKeyMatchingQuery

		public void TestGetDataContextKeyMatchingQuery()
		{
			var consolidation = GetNewConsolidation();
			consolidation.KB_JobID = "CM00001";
			Factory.SaveForTesting();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(ExpectedDataContextType, consolidation.KB_JobID);
			consolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var message = GetQueuedUniversalShipmentMessage(consolidationDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var importedConsolidation = (TTransportConsolidation)importResults.Single(i => i.DataContextType == ExpectedDataContextType).GetBizOForTesting(consolidationDataObject, Factory.BOFactory);
			AssertEquals("Should have matched existing Consolidation by Job Number.", consolidation.PK, importedConsolidation.PK);
			AssertContains("Service Task Log should mention *Update*.", string.Format(@"
Updated {0} from UniversalShipment.
Successfully saved {0}".Trim(), importedConsolidation.HumanReadableName), serviceTaskLog.ToString());
		}

		protected abstract TTransportConsolidation GetNewConsolidation();

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertNull(new TDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#endregion

		#region Shipments

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals(true, new TDataContextManager().ManagesShipments);
		}

		#endregion

		#region TestShipmentDataObjectWriter

		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new TDataContextManager();
			AssertEquals(ExpectedShipmentDataObjectWriterType, manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		protected abstract Type ExpectedShipmentDataObjectWriterType { get; }

		#endregion

		#endregion

		#region Events

		#region TestEventParentFinder

		public void TestEventParentFinder()
		{
			IEventDataContextManager manager = new TDataContextManager();
			AssertNull(manager.GetLogParentsForEvent(new UniversalEvent(), Factory.BOFactory, new TestErrorLogger()));
		}

		#endregion

		#endregion
	}
}
