using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	class ScheduledProductionRuleQueueConsumerTest : TestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<ScheduledProductionRuleQueueConsumer>(ObjectFactory.Get<IScheduledProductionRuleQueueConsumer>());
		}

		public void TestConstructor()
		{
			var scheduledRuleLoader = Mock.Of<IScheduledRuleLoader>();
			var rulesEngine = Mock.Of<IProductionRulesEnginePullService>();
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledProductionRuleQueueConsumer(null, rulesEngine));
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader, null));
		}

		public void TestProcessQueue_NullNotifications()
		{
			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			AssertExceptionThrown<ArgumentNullException>(() => queueConsumer.ProcessQueue(null, CancellationToken.None));
		}

		public void TestProcessQueue_NoQueuedRuleSets()
		{
			var notificationsMock = new Mock<INotifications>();
			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);

			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns((ScheduledRuleLoaderResult)null);

			queueConsumer.ProcessQueue(notificationsMock.Object, CancellationToken.None);
			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Once);
			rulesEngine.VerifyNoOtherCalls();
			Assert(true);
		}

		public void TestProcessQueue() => TestProcessQueue("", RulesContextSubType.None);
		public void TestProcessQueue_WithSubType() => TestProcessQueue("WST", RulesContextSubType.WarehouseStorageJob);

		void TestProcessQueue(string subTypeCode, RulesContextSubType subType)
		{
			IBranch testCurrentBranch = Env.Instance.CurrentBranch;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			ruleSet1.PRS_ContextSubType = subTypeCode;

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			Action assertRunInTemporaryContext = () =>
			{
				AssertEquals("Should be run in a temporary user context.", newBranch.PK, Env.CurrentBranchPK);
			};

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(newBranch.PK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(new[] { mockFact }).Callback(assertRunInTemporaryContext);
			mockProcessor.InSequence(sequence).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						assertRunInTemporaryContext();

						ruleSet1.PRS_ContextSubType = string.Empty; // Undo dodgy subtype

						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					});

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, subType, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Returns(productionRuleResult).Callback(assertRunInTemporaryContext);

			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNull("Should have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);

			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_MultipleQueuedRuleSets()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock1 = new Mock<ISqlApplicationLock>();
			var sqlLock2 = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "A", "A");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact1 = Mock.Of<IInputFact>();
			var facts1 = new[] { mockFact1 };
			var productionRuleResult1 = new ProductionRulesEngineResult(facts1);

			var mockFact2 = Mock.Of<IInputFact>();
			var facts2 = new[] { mockFact2 };
			var productionRuleResult2 = new ProductionRulesEngineResult(facts2);

			var sequence1 = new MockSequence();
			var mockProcessor1 = new Mock<IScheduledRuleProcessor>();
			mockProcessor1.InSequence(sequence1).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(Env.CurrentBranchPK);
			mockProcessor1.InSequence(sequence1).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts1);
			mockProcessor1.InSequence(sequence1).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult1, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = $"NameChanged1";
					});

			var sequence2 = new MockSequence();
			var mockProcessor2 = new Mock<IScheduledRuleProcessor>();
			mockProcessor2.InSequence(sequence2).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet2)).Returns(Env.CurrentBranchPK);
			mockProcessor2.InSequence(sequence2).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet2, cancellationToken)).Returns(facts2);
			mockProcessor2.InSequence(sequence2).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult2, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = $"NameChanged2";
					});

			var loadedRules1 = new[] { rule1_1, rule1_2 };
			var ruleSet1WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock1.Object);
			var ruleLoaderResult1 = new ScheduledRuleLoaderResult(ruleSet1WithLock, loadedRules1, new[] { queue1 }, mockProcessor1.Object);

			var loadedRules2 = new[] { rule2_1 };
			var ruleSet2WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet2, sqlLock2.Object);
			var ruleLoaderResult2 = new ScheduledRuleLoaderResult(ruleSet2WithLock, loadedRules2, new[] { queue2 }, mockProcessor2.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult1, ruleLoaderResult2, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult1.Rules, facts1, cancellationToken)).Returns(productionRuleResult1);
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult2.Rules, facts2, cancellationToken)).Returns(productionRuleResult2);

			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing RuleSet: TEST2 for Context: ProductWarehouseWaveCreation, Scheduled Rules: A."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved RuleSet: TEST2 for Context: ProductWarehouseWaveCreation, Scheduled Rules: A."))));

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(3));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor1.VerifyAll();
			rulesEngine.VerifyAll();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock1.Verify(l => l.Dispose(), Times.Once);
			sqlLock2.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNull("Should have deleted queue1.", queue1_InNewFactory);
			AssertNull("Should have deleted queue2.", queue2_InNewFactory);

			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged2", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_NoResults()
		{
			IBranch testCurrentBranch = Env.Instance.CurrentBranch;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };
			var productionRuleResult = new ProductionRulesEngineResult(Enumerable.Empty<IFact>());

			Action assertRunInTemporaryContext = () =>
			{
				AssertEquals("Should be run in a temporary user context.", newBranch.PK, Env.CurrentBranchPK);
			};

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(newBranch.PK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(new[] { mockFact }).Callback(assertRunInTemporaryContext);
			mockProcessor.InSequence(sequence).Setup(p => p.InformationMessageForNothingProcessed).Returns("Did nothing!");

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Returns(productionRuleResult).Callback(assertRunInTemporaryContext);

			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Did nothing!"))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved RuleSet: TEST1 for Context: ProductWarehouseWaveCreation, Scheduled Rules: 1, 2."))));

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNull("Should have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);
		}

		public void TestProcessQueue_MultipleQueuedRuleSets_Cancelled()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;
			var notificationsMock = new Mock<INotifications>();

			var sqlLock1 = new Mock<ISqlApplicationLock>();
			var sqlLock2 = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact1 = Mock.Of<IInputFact>();
			var facts = new[] { mockFact1 };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(Env.CurrentBranchPK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);
			mockProcessor.InSequence(sequence).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = $"NameChanged";

						cancellationTokenSource.Cancel();
					});

			var loadedRules1 = new[] { rule1_1, rule1_2 };
			var ruleSet1WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock1.Object);
			var ruleLoaderResult1 = new ScheduledRuleLoaderResult(ruleSet1WithLock, loadedRules1, new[] { queue1 }, mockProcessor.Object);

			var loadedRules2 = new[] { rule2_1 };
			var ruleSet2WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet2, sqlLock2.Object);
			var ruleLoaderResult2 = new ScheduledRuleLoaderResult(ruleSet2WithLock, loadedRules2, new[] { queue2 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult1, ruleLoaderResult2, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult1.Rules, facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(1));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			sqlLock1.Verify(l => l.Dispose(), Times.Once);
			sqlLock2.Verify(l => l.Dispose(), Times.Never);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNull("Should have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);

			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_SettingContext_UnexpectedExceptionThrown()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var exceptionToThrow = new Exception();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.Verify(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1), Times.Exactly(1));
			mockProcessor.VerifyNoOtherCalls();
			rulesEngine.VerifyNoOtherCalls();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_SettingContext_CancellationExceptionThrown_ThisToken() => TestProcessQueue_SettingContext_CancellationExceptionThrown(forCurrentCancellationToken: true);
		public void TestProcessQueue_SettingContext_CancellationExceptionThrown_OtherToken() => TestProcessQueue_SettingContext_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_SettingContext_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var cancellationToken = new CancellationTokenSource().Token;
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);

			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessQueue_LoadingFacts_FactLoadingExceptionThrown()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Callback(() => throw new FactLoadingException("Failed to load!"));

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type.Equals(NotificationType.Error) && x.Message == "Failed to load!")));

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyNoOtherCalls();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_LoadingFacts_UnexpectedExceptionThrown()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var exceptionToThrow = new Exception();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyNoOtherCalls();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_LoadingFacts_CancellationExceptionThrown_ThisToken() => TestProcessQueue_LoadingFacts_CancellationExceptionThrown(forCurrentCancellationToken: true);
		public void TestProcessQueue_LoadingFacts_CancellationExceptionThrown_OtherToken() => TestProcessQueue_LoadingFacts_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_LoadingFacts_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var cancellationToken = new CancellationTokenSource().Token;
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);

			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessQueue_LoadingFacts_NoFactsLoaded()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(Enumerable.Empty<IInputFact>());
			mockProcessor.Setup(p => p.InformationMessageForNothingProcessed).Returns("Did nothing!");

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message == "Did nothing!")));

			mockProcessor.VerifyAll();
			rulesEngine.VerifyNoOtherCalls();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNull("Should have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_RunningEngine_ErrorAdded()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken))
				.Returns(new ProductionRulesEngineResult("SOME ERROR!"));

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "SOME ERROR!")));

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_RunningEngine_Halted()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken))
				.Returns(ProductionRulesEngineResult.Halted);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_RunningEngine_UnexpectedExceptionThrown()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var exceptionToThrow = new Exception();
			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Callback(() => throw exceptionToThrow);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_RunningEngine_CancellationExceptionThrown_ThisToken() => TestProcessQueue_RunningEngine_CancellationExceptionThrown(forCurrentCancellationToken: true);
		public void TestProcessQueue_RunningEngine_CancellationExceptionThrown_OtherToken() => TestProcessQueue_RunningEngine_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_RunningEngine_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var cancellationToken = new CancellationTokenSource().Token;
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Callback(() => throw exceptionToThrow);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessQueue_ProcessingResults_ErrorAdded()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);
			mockProcessor.Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						n.AddError("SOME ERROR!");
					});

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "SOME ERROR!")));

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_ProcessingResults_UnexpectedExceptionThrown()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var exceptionToThrow = new Exception();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);
			mockProcessor.Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();
			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
		}

		public void TestProcessQueue_ProcessingResults_CancellationExceptionThrown_ThisToken() => TestProcessQueue_ProcessingResults_CancellationExceptionThrown(forCurrentCancellationToken: true);
		public void TestProcessQueue_ProcessingResults_CancellationExceptionThrown_OtherToken() => TestProcessQueue_ProcessingResults_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_ProcessingResults_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var cancellationToken = new CancellationTokenSource().Token;
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact = Mock.Of<IInputFact>();
			var facts = new[] { mockFact };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1)).Returns(((IWhsWarehouse)warehouse).WW_GB_RelatedCompanyBranch);
			mockProcessor.Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), ruleSet1, cancellationToken)).Returns(facts);
			mockProcessor.Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken)).Callback(() => throw exceptionToThrow);

			var loadedRules = new[] { rule1_1, rule1_2 };
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1, sqlLock.Object);
			var ruleLoaderResult = new ScheduledRuleLoaderResult(ruleSetWithLock, loadedRules, new[] { queue1 }, mockProcessor.Object);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();
			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<ScheduledRuleLoaderResult>(new[] { ruleLoaderResult, null }).Dequeue);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), ruleLoaderResult.Rules, facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);

			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}

	[UseSnapshotProtection]
	public class ScheduledProductionRuleQueueConsumerTest_WithSnapshotProtection : TestCase
	{
		public void TestProcessQueue_FactorySaveFails_SaveFailed()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact1 = Mock.Of<IInputFact>();
			var facts = new[] { mockFact1 };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK))).Returns(Env.CurrentBranchPK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK), cancellationToken)).Returns(facts);
			mockProcessor.InSequence(sequence).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						f.New<ProductionRule>(); // Will fail to save
						scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns((ScheduledRuleLoaderResult)null);
					});

			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns<BusinessObjectFactory>(GetRuleLoaderResult);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), It.IsAny<IEnumerable<IProductionRule>>(), facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			var lastException = ErrorReporter.LastExceptionReported;
			AssertType<ZSaveException>(lastException);
			AssertEquals(true, lastException.Message.Contains("Tablename: ProductionRule"));
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertNotEquals("Factory save failed.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);

			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);

			ScheduledRuleLoaderResult GetRuleLoaderResult(BusinessObjectFactory factory)
			{
				var ruleSet1InFactory = factory.Load<ProductionRuleSet>(ruleSet1.PK);
				var rule1_1InFactory = factory.Load<ProductionRule>(rule1_1.PK);
				var rule1_2InFactory = factory.Load<ProductionRule>(rule1_2.PK);
				var queue1InFactory = factory.Load<ProductionRuleScheduleQueue>(queue1.PK);

				var loadedRules1 = new[] { rule1_1InFactory, rule1_2InFactory };
				var ruleSet1WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1InFactory, sqlLock.Object);
				return new ScheduledRuleLoaderResult(ruleSet1WithLock, loadedRules1, new[] { queue1InFactory }, mockProcessor.Object);
			}
		}

		public void TestProcessQueue_FactorySaveFails_CannotSaveException()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact1 = Mock.Of<IInputFact>();
			var facts = new[] { mockFact1 };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK))).Returns(Env.CurrentBranchPK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK), cancellationToken)).Returns(facts);
			mockProcessor.InSequence(sequence).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = $"NameChanged";

						scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns((ScheduledRuleLoaderResult)null);

						f.Saving += _ => throw new ZCannotSaveException("Some failure!", "Fail");
					});

			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns<BusinessObjectFactory>(GetRuleLoaderResult);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), It.IsAny<IEnumerable<IProductionRule>>(), facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Error occurred while saving the results of the processed rule(s): Some failure!")));

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertNotEquals("Factory save failed.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);

			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);

			ScheduledRuleLoaderResult GetRuleLoaderResult(BusinessObjectFactory factory)
			{
				var ruleSet1InFactory = factory.Load<ProductionRuleSet>(ruleSet1.PK);
				var rule1_1InFactory = factory.Load<ProductionRule>(rule1_1.PK);
				var rule1_2InFactory = factory.Load<ProductionRule>(rule1_2.PK);
				var queue1InFactory = factory.Load<ProductionRuleScheduleQueue>(queue1.PK);

				var loadedRules1 = new[] { rule1_1InFactory, rule1_2InFactory };
				var ruleSet1WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1InFactory, sqlLock.Object);
				return new ScheduledRuleLoaderResult(ruleSet1WithLock, loadedRules1, new[] { queue1InFactory }, mockProcessor.Object);
			}
		}

		public void TestProcessQueue_FactorySaveFails_ConcurrencyError()
		{
			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();

			var sqlLock = new Mock<ISqlApplicationLock>();

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWW", isLive: false, warehousePK: warehouse.PK);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWW", isLive: false, warehousePK: warehouse.PK);

			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2");
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1");

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var mockFact1 = Mock.Of<IInputFact>();
			var facts = new[] { mockFact1 };
			var productionRuleResult = new ProductionRulesEngineResult(facts);

			var scheduledRuleLoader = new Mock<IScheduledRuleLoader>();

			var sequence = new MockSequence();
			var mockProcessor = new Mock<IScheduledRuleProcessor>();
			mockProcessor.InSequence(sequence).Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK))).Returns(Env.CurrentBranchPK);
			mockProcessor.InSequence(sequence).Setup(p => p.LoadInputFacts(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.Is<IProductionRuleSet>(r => r.PK == ruleSet1.PK), cancellationToken)).Returns(facts);
			mockProcessor.InSequence(sequence).Setup(p => p.ProcessResults(It.IsAny<BusinessObjectFactory>(), productionRuleResult, It.IsAny<INotifications>(), cancellationToken))
				.Callback<BusinessObjectFactory, ProductionRulesEngineResult, INotifications, CancellationToken>(
					(f, r, n, c) =>
					{
						var warehouseInResultFactory = f.Load<IWhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = $"NameChanged2";

						var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
						var queueInfactory2 = factory2.Load<IWhsWarehouse>(warehouse.PK);
						queueInfactory2.WW_WarehouseName = $"NameChanged1";
						factory2.Save();

						scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns((ScheduledRuleLoaderResult)null);
					});

			scheduledRuleLoader.Setup(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>())).Returns<BusinessObjectFactory>(GetRuleLoaderResult);

			var rulesEngine = new Mock<IProductionRulesEnginePullService>();
			rulesEngine.Setup(re => re.RunRulesEngine(RulesContextType.ProductWarehouseWaveCreation, RulesContextSubType.None, It.IsAny<ProductionRuleSetFilter>(), It.IsAny<IEnumerable<IProductionRule>>(), facts, cancellationToken)).Returns(productionRuleResult);

			var queueConsumer = new ScheduledProductionRuleQueueConsumer(scheduledRuleLoader.Object, rulesEngine.Object);
			queueConsumer.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Concurrency error occurred while saving the results of the processed rule(s).")));

			scheduledRuleLoader.Verify(srl => srl.GetNextRuleSetToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			scheduledRuleLoader.VerifyNoOtherCalls();

			mockProcessor.VerifyAll();
			rulesEngine.VerifyAll();

			sqlLock.Verify(l => l.Dispose(), Times.Once);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<IWhsWarehouse>(warehouse.PK);
			AssertEquals("Factory save failed.", "NameChanged1", warehouse_InNewFactory.WW_WarehouseName);

			var queue1_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertNotNull("Should *not* have deleted queue1.", queue1_InNewFactory);
			AssertNotNull("Should *not* have deleted queue2.", queue2_InNewFactory);

			ScheduledRuleLoaderResult GetRuleLoaderResult(BusinessObjectFactory factory)
			{
				var ruleSet1InFactory = factory.Load<ProductionRuleSet>(ruleSet1.PK);
				var rule1_1InFactory = factory.Load<ProductionRule>(rule1_1.PK);
				var rule1_2InFactory = factory.Load<ProductionRule>(rule1_2.PK);
				var queue1InFactory = factory.Load<ProductionRuleScheduleQueue>(queue1.PK);

				var loadedRules1 = new[] { rule1_1InFactory, rule1_2InFactory };
				var ruleSet1WithLock = new AppLockedItem<ProductionRuleSet>(ruleSet1InFactory, sqlLock.Object);
				return new ScheduledRuleLoaderResult(ruleSet1WithLock, loadedRules1, new[] { queue1InFactory }, mockProcessor.Object);
			}
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection)); }
		}
		BusinessObjectFactory factory;

		DbConnection testCaseDbConnection;

		protected override void SetUp()
		{
			testCaseDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion
	}
}
