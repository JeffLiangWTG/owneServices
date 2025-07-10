using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	abstract class WhsTransitConsignmentConsolDataContextManagerTest<TContextManager, TConsol> : ShipmentDataContextManagerTestCase<TContextManager, TConsol>
		where TContextManager : WhsTransitConsignmentConsolDataContextManager<TConsol>, new()
		where TConsol : NonPersistentBusinessObject, new()
	{
		public void TestDataContextKey()
		{
			AssertEquals("Non persistent consol", new TConsol().GetUniversalDataContextManager().DataContextKey);
		}

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new TContextManager().DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		public void TestDefaultOutputDirectory()
		{
			AssertNull("DefaultOutputDirectory should have no value", new TContextManager().DefaultOutputDirectory);
		}

		public void TestEventContextValues()
		{
			var manager = new TConsol().GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals(0, manager.EventContextValues.Count());
		}

		public void TestManagesEvents()
		{
			AssertEquals("ManagesEvents should be false", false, new TContextManager().ManagesEvents);
		}

		public void TestManagesShipments()
		{
			AssertEquals("ManagesShipments should be true", true, new TContextManager().ManagesShipments);
		}

		public void TestRecipientRoleTargettedToThisModule()
		{
			foreach (var supportedRecipientRoleType in SupportedRecipientRoleTypes)
			{
				AssertDataTargetCollection(DataContextType.TransportConsignmentRunSheet, supportedRecipientRoleType, ExpectedDataContextType.ToString());
				AssertDataTargetCollection(DataContextType.ForwardingShipment, supportedRecipientRoleType, null);
				AssertDataTargetCollection(DataContextType.ForwardingConsol, supportedRecipientRoleType, ExpectedDataContextType.ToString());
			}

			AssertDataTargetCollection(DataContextType.ForwardingShipment, RecipientRoleType.ACR, null);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.DTW, RecipientRoleType.ATW };

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		void AssertDataTargetCollection(DataContextType dataSourceDataContext, RecipientRoleType roleType, string expectedDataContextType)
		{
			IShipmentDataContextManager manager = new TContextManager();
			var runSheetDataSource = DataContextFactory.New();
			runSheetDataSource.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = roleType, ServiceCode = SupportedRecipientServices(roleType).FirstOrDefault() } } });
			runSheetDataSource.AddDataSource(dataSourceDataContext, "");
			manager.DefaultDataTargetFromRecipientRole(runSheetDataSource, new DummyXmlSessionTracker(null));
			AssertEquals(expectedDataContextType, runSheetDataSource.DataTargetCollection?.SingleOrDefault()?.Type);
		}
	}
}
