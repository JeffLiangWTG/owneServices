using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			Factory.Save();

			IProcessTaskLoadStrategy strategy = new ContainerProcessTaskTypeLoadStrategy();
			Type expectedType = typeof(ContainerProcessTask);
			AssertEquals(expectedType, strategy.GetTypeForLoad(container.TablePrefix, container.PK, Factory));

			container = Factory.New<CommonContainer>();
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;
			shipment.JS_IsShipping = true;

			Factory.Save();

			expectedType = ObjectFactory.GetType<Integration.Agency.IAgencyContainerProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(container.TablePrefix, container.PK, Factory));
		}

		public void TestGetTypeForLoadDoesNotLoadMultipleContainersAroundRow()
		{
			var container = Factory.New(ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>());
			var agencyBooking = Factory.New(ObjectFactory.GetType<Integration.Agency.IAgencyBooking>());
			container[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = agencyBooking.PK;

			Factory.Save();

			IProcessTaskLoadStrategy strategy = new ContainerProcessTaskTypeLoadStrategy();
			Type expectedType = ObjectFactory.GetType<Integration.Agency.IAgencyContainerProcessTask>();

			AssertEquals(expectedType, strategy.GetTypeForLoad(container.TablePrefix, container.PK, Factory));
			AssertContainsExactElementsInAnyOrder("factory didn't create CommonContainer around the same row", new[] { container }, Factory.GetBizOsForPK(container.PK.ToGuid()));

			var otherFactory = new BusinessObjectFactory();
			container = (BusinessObject)otherFactory.Load<Integration.Agency.IAgencyBookingContainer>(container.PK);

			AssertEquals(expectedType, strategy.GetTypeForLoad(container.TablePrefix, container.PK, otherFactory));
			AssertContainsExactElementsInAnyOrder("factory didn't create CommonContainer around the same row", new[] { container }, otherFactory.GetBizOsForPK(container.PK.ToGuid()));
		}

		public void TestAddAdditionalParentFilters()
		{
			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);

			var forwardingContainer = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingContainer>());
			var forwardingShipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());

			var forwardingConsol = forwardingShipment.Consols.AddNew();
			forwardingConsol.Containers.Add(forwardingContainer);

			var forwardingContainerTask = ((IWorkflowProvider)forwardingContainer).WorkflowItems.AddNew();

			var declarationContainer = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingContainer>());
			var declaration = Factory.New<IBaseJobDeclaration>();
			var cusContainer = Factory.New<IBaseCusContainer>() as BusinessObject;
			cusContainer[CusContainerSchema.CO_JC] = declarationContainer.PK;
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			var declarationContainerTask = ((IWorkflowProvider)declarationContainer).WorkflowItems.AddNew();

			var agencyContainer = (CommonContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IAgencyShipmentContainer>());

			var agencyShipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType<Integration.Agency.IAgencyShipment>());
			agencyContainer.JC_JS_FCLBookingOnlyLink = agencyShipment.PK;

			var agencyContainerTask = ((IWorkflowProvider)agencyContainer).WorkflowItems.AddNew();

			Factory.Save();

			ProcessTask[] tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.ContainerWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { forwardingContainerTask, declarationContainerTask }, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode));
			AssertContainsExactElementsInAnyOrder(new[] { agencyContainerTask }, tasks);
		}

		public void TestGetTypeForLoad_FromOtherThread_ShouldNotThrowThreadSentryExceptions()
		{
			TestEntityFrameworkSettings.Get().ReportCrossThreadFactoryAccess = true;

			var container = Factory.NewWithValidTestData<CommonContainer>();
			Factory.Save();

			var factoryThatDoesntHaveTheContainerLoaded = new BusinessObjectFactory();
			var readOnlyFactory = factoryThatDoesntHaveTheContainerLoaded.GetCachedReadOnlyFactory();
			factoryThatDoesntHaveTheContainerLoaded.ThreadSentry.RelinquishThreadOwnership();

			IProcessTaskLoadStrategy strategy = new ContainerProcessTaskTypeLoadStrategy();

			AssertNoExceptionThrown(() =>
			{
				Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						factoryThatDoesntHaveTheContainerLoaded.ThreadSentry.TakeThreadOwnership();
						strategy.GetTypeForLoad(container.TablePrefix, container.PK, factoryThatDoesntHaveTheContainerLoaded);
					}
				}).Wait(TimeSpan.FromSeconds(2));
			});
		}

		public void TestGetTypeForLoad_NoExceptionAfterConcurrencySolving()
		{
			var container = Factory.New(ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>());
			var agencyBooking = Factory.New(ObjectFactory.GetType<Integration.Agency.IAgencyBooking>());
			container[JobContainerSchema.JC_JS_FCLBookingOnlyLink] = agencyBooking.PK;

			Factory.Save();

			var anotherFactory = Factory.CreateNewFactory();
			var anotherContainer = anotherFactory.Load(ObjectFactory.GetType<Integration.Agency.IAgencyBookingContainer>(), container.PK);
			anotherContainer.Delete();

			anotherFactory.Save();

			IProcessTaskLoadStrategy strategy = new ContainerProcessTaskTypeLoadStrategy();
			AssertNoExceptionThrown(() => strategy.GetTypeForLoad(container.TablePrefix, container.PK, Factory));
		}

		ZDBOnlyQuery GetNewQueryForTestAddAdditionalParentFilters(string workflowTypeCode)
		{
			WorkflowDescriptor descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowTypeCode);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CommonContainer), ProcessTasksSchema.P9_ParentID);
			IProcessTaskLoadStrategy strategy = new ContainerProcessTaskTypeLoadStrategy();
			strategy.AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}
	}
}
