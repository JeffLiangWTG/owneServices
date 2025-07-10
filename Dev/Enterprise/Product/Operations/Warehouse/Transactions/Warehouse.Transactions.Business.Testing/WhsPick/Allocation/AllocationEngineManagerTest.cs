using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Transactions.Facts;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AllocationEngineManagerTest : WhsTestCaseWithFactory
	{
		public void TestConstructor_NullArguments_Throws()
		{
			var rulesEngine = Mock.Of<IUserHaltableProductionRulesEngineService>();
			var factLoader = Mock.Of<IAllocationFactLoader>();
			var resultProcessor = Mock.Of<IAllocationResultProcessor>();

			AssertExceptionThrown<ArgumentNullException>(() =>
				new AllocationEngineManager(null, factLoader, resultProcessor));
			AssertExceptionThrown<ArgumentNullException>(() =>
				new AllocationEngineManager(rulesEngine, null, resultProcessor));
			AssertExceptionThrown<ArgumentNullException>(() =>
				new AllocationEngineManager(rulesEngine, factLoader, null));
		}

		public void TestAllocate_NullPick_Throws()
		{
			var manager = GetNewManager();
			AssertExceptionThrown<ArgumentNullException>(() => manager.Allocate(null, Mock.Of<INotifications>(),
				Mock.Of<IPickStrategy>(), Enumerable.Empty<WhsPickOrderedInventory>()));
		}

		public void TestAllocate_NullOrderedInventory_Throws()
		{
			var manager = GetNewManager();
			AssertExceptionThrown<ArgumentNullException>(() =>
				manager.Allocate(Factory.New<WhsPick>(), Mock.Of<INotifications>(), Mock.Of<IPickStrategy>(), null));
		}

		public void TestAllocate_NullNotifications_Throws()
		{
			var manager = GetNewManager();
			AssertExceptionThrown<ArgumentNullException>(() => manager.Allocate(Factory.New<WhsPick>(), null,
				Mock.Of<IPickStrategy>(), Enumerable.Empty<WhsPickOrderedInventory>()));
		}

		public void TestAllocate() => TestAllocate(useAllOrderedInventories: true);

		public void TestAllocate_SingleOrderedInventory() => TestAllocate(useAllOrderedInventories: false);

		void TestAllocate(bool useAllOrderedInventories)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var orderedInventories = useAllOrderedInventories
				? pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				: new[] { pick.OrderedInventories[0] };
			var resultFact = new AllocationResultFact(
				Guid.NewGuid(),
				"R1",
				null,
				order.Lines[0].PK.ToGuid(),
				pick.OrderedInventories[0].PK.ToGuid(),
				pick.OrderedInventories[0].AvailableInventories[0].PK.ToGuid(),
				5m);

			var inputFacts = new [] { new IInputFact[] { new AllocationLocationFact(data.Whs1.DefaultLocation) } };
			var outputFacts = new IFact[] { WarehouseFactsHelper.GetOrganisationFact(data.Org1), resultFact };

			var pickStrategyMock = Mock.Of<IPickStrategy>();

			var factLoaderMock = new Mock<IAllocationFactLoader>();
			factLoaderMock.Setup(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock)).Returns(inputFacts);

			var mockNotification = Mock.Of<INotification>();
			var resultProcessorMock = new Mock<IAllocationResultProcessor>();
			resultProcessorMock.Setup(rp => rp.ProcessResults(pick, new[] { resultFact }))
				.Returns(new AllocationProcessedResult(mockNotification, true));

			Func<IEnumerable<IEnumerable<IInputFact>>> getFacts = null;
			Func<ProductionRulesEngineResult, INotification> processResults = null;
			INotification resultNotification = null;

			var notifications = new Mock<INotifications>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>();
			engineMock.Setup(em =>
					em.RunRulesEngine(
						Factory,
						notifications.Object,
						RulesContextType.ProductWarehouseAllocation,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == pick.WP_WW_Whs),
						It.IsAny<Func<IEnumerable<IEnumerable<IInputFact>>>>(),
						It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IEnumerable<IInputFact>>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						getFacts = f1;
						processResults = f2;
						resultNotification = processResults(new ProductionRulesEngineResult(outputFacts));
					});

			var engineManager =
				new AllocationEngineManager(engineMock.Object, factLoaderMock.Object, resultProcessorMock.Object);
			var result = engineManager.Allocate(pick, notifications.Object, pickStrategyMock, orderedInventories);
			AssertEquals("Returns correct allocation result.", AllocationResult.AllocatedStock, result);
			AssertNotNull("Called RunRulesEngine.", getFacts);
			AssertNotNull("Called RunRulesEngine.", processResults);

			AssertContainsExactElementsInExactOrder(inputFacts, getFacts());
			factLoaderMock.Verify(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock), Times.Once);

			AssertEquals(mockNotification, resultNotification);
			resultProcessorMock.Verify(rp => rp.ProcessResults(pick, new[] { resultFact }), Times.Once);
		}

		public void TestAllocate_NoStockAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var inputFacts = new[] { new IInputFact[] { new AllocationLocationFact(data.Whs1.DefaultLocation) } };
			var outputFacts = new IFact[] { WarehouseFactsHelper.GetOrganisationFact(data.Org1) };

			var pickStrategyMock = Mock.Of<IPickStrategy>();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var factLoaderMock = new Mock<IAllocationFactLoader>();
			factLoaderMock.Setup(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock)).Returns(inputFacts);

			var mockNotification = Mock.Of<INotification>();
			var resultProcessorMock = new Mock<IAllocationResultProcessor>();
			resultProcessorMock.Setup(rp => rp.ProcessResults(pick, Enumerable.Empty<AllocationResultFact>()))
				.Returns(new AllocationProcessedResult(mockNotification, false));

			Func<IEnumerable<IEnumerable<IInputFact>>> getFacts = null;
			Func<ProductionRulesEngineResult, INotification> processResults = null;
			INotification resultNotification = null;

			var notifications = new Mock<INotifications>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>();
			engineMock.Setup(em =>
					em.RunRulesEngine(
						Factory,
						notifications.Object,
						RulesContextType.ProductWarehouseAllocation,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == pick.WP_WW_Whs),
						It.IsAny<Func<IEnumerable<IEnumerable<IInputFact>>>>(),
						It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IEnumerable<IInputFact>>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						getFacts = f1;
						processResults = f2;
						resultNotification = processResults(new ProductionRulesEngineResult(outputFacts));
					});

			var engineManager =
				new AllocationEngineManager(engineMock.Object, factLoaderMock.Object, resultProcessorMock.Object);
			var result = engineManager.Allocate(pick, notifications.Object, pickStrategyMock, orderedInventories);
			AssertEquals("Returns correct allocation result.", AllocationResult.NoStockAllocated, result);
			AssertNotNull("Called RunRulesEngine.", getFacts);
			AssertNotNull("Called RunRulesEngine.", processResults);

			AssertContainsExactElementsInExactOrder(inputFacts, getFacts());
			factLoaderMock.Verify(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock), Times.Once);

			AssertEquals(mockNotification, resultNotification);
			resultProcessorMock.Verify(rp => rp.ProcessResults(pick, Enumerable.Empty<AllocationResultFact>()),
				Times.Once);
		}

		public void TestAllocate_NoFacts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var inputFacts = Array.Empty<IInputFact[]>();
			var outputFacts = new IFact[] { WarehouseFactsHelper.GetOrganisationFact(data.Org1) };

			var pickStrategyMock = Mock.Of<IPickStrategy>();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var factLoaderMock = new Mock<IAllocationFactLoader>();
			factLoaderMock.Setup(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock)).Returns(inputFacts);

			var mockNotification = Mock.Of<INotification>();
			var resultProcessorMock = new Mock<IAllocationResultProcessor>();
			resultProcessorMock.Setup(rp => rp.ProcessResults(pick, Enumerable.Empty<AllocationResultFact>()))
				.Returns(new AllocationProcessedResult(mockNotification, false));

			var notifications = new Mock<INotifications>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>();
			engineMock.Setup(em =>
					em.RunRulesEngine(
						Factory,
						notifications.Object,
						RulesContextType.ProductWarehouseAllocation,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == pick.WP_WW_Whs),
						It.IsAny<Func<IEnumerable<IEnumerable<IInputFact>>>>(),
						It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IEnumerable<IInputFact>>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						f1();
					});

			var engineManager =
				new AllocationEngineManager(engineMock.Object, factLoaderMock.Object, resultProcessorMock.Object);
			var result = engineManager.Allocate(pick, notifications.Object, pickStrategyMock, orderedInventories);
			AssertEquals("Returns correct allocation result.", AllocationResult.NoStockAllocated, result);

			factLoaderMock.Verify(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock), Times.Once);
			resultProcessorMock.Verify(rp => rp.ProcessResults(pick, It.IsAny<IEnumerable<AllocationResultFact>>()), Times.Never);
		}

		public void TestAllocate_ErrorOrWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var inputFacts = new[] { new IInputFact[] { new AllocationLocationFact(data.Whs1.DefaultLocation) } };
			var outputFacts = new IFact[] { WarehouseFactsHelper.GetOrganisationFact(data.Org1) };

			var pickStrategyMock = Mock.Of<IPickStrategy>();

			var orderedInventories = pick.OrderedInventories.Cast<WhsPickOrderedInventory>();
			var factLoaderMock = new Mock<IAllocationFactLoader>();
			factLoaderMock.Setup(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock)).Returns(inputFacts);

			var mockNotification = Mock.Of<INotification>();
			var resultProcessorMock = new Mock<IAllocationResultProcessor>();
			resultProcessorMock.Setup(rp => rp.ProcessResults(pick, Enumerable.Empty<AllocationResultFact>()))
				.Returns(new AllocationProcessedResult(mockNotification, false));

			Func<IEnumerable<IEnumerable<IInputFact>>> getFacts = null;
			Func<ProductionRulesEngineResult, INotification> processResults = null;

			var notifications = new Mock<INotifications>();
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>();
			engineMock.Setup(em =>
					em.RunRulesEngine(
						Factory,
						notifications.Object,
						RulesContextType.ProductWarehouseAllocation,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == pick.WP_WW_Whs),
						It.IsAny<Func<IEnumerable<IEnumerable<IInputFact>>>>(),
						It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IEnumerable<IInputFact>>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						getFacts = f1;
						processResults = f2;
					});

			var engineManager =
				new AllocationEngineManager(engineMock.Object, factLoaderMock.Object, resultProcessorMock.Object);
			var result = engineManager.Allocate(pick, notifications.Object, pickStrategyMock, orderedInventories);
			AssertEquals("Returns correct allocation result.", AllocationResult.ErrorOrWarning, result);
			AssertNotNull("Called RunRulesEngine.", getFacts);
			AssertNotNull("Called RunRulesEngine.", processResults);
			
			AssertContainsExactElementsInExactOrder(inputFacts, getFacts());
			factLoaderMock.Verify(fl => fl.GetAllocationFacts(orderedInventories, pickStrategyMock), Times.Once);
			resultProcessorMock.Verify(rp => rp.ProcessResults(pick, Enumerable.Empty<AllocationResultFact>()),
				Times.Never);
		}

		AllocationEngineManager GetNewManager(
			IUserHaltableProductionRulesEngineService rulesEngine = null,
			IAllocationFactLoader factLoader = null,
			IAllocationResultProcessor resultProcessor = null)
		{
			return new AllocationEngineManager(
				rulesEngine ?? Mock.Of<IUserHaltableProductionRulesEngineService>(),
				factLoader ?? Mock.Of<IAllocationFactLoader>(),
				resultProcessor ?? Mock.Of<IAllocationResultProcessor>());
		}
	}
}
