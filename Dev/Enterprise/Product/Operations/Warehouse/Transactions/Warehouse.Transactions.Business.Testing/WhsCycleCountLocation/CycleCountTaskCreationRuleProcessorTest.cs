using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class CycleCountTaskCreationRuleProcessorTest : ScheduledRuleProcessorTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountTaskCreationRuleProcessor(null, Mock.Of<ICycleCountLocationTaskCreationFactLoader>()));
			AssertExceptionThrown<ArgumentNullException>(() => new CycleCountTaskCreationRuleProcessor(Mock.Of<IWhsCycleCountLocationCreator>(), null));
		}

		public void TestLoadInputFacts()
		{
			var warehousePK = ZGuid.BrettsGuid;
			var token = new CancellationToken();
			var cycleCountFacts = new IInputFact[]
			{
				Mock.Of<ICycleCountLocationFact>(),
				Mock.Of<ICycleCountLocationFact>(),
				Mock.Of<ICycleCountLocationFact>(),
			};

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWC";
			ruleSet.PRS_Name = "PWC-TEST";
			ruleSet.PRS_Description = "PWC-TEST";
			ruleSet.PRS_WW_Warehouse = warehousePK;

			var readOnlyFactory = Factory.GetCachedReadOnlyFactory();
			var factLoader = new Mock<ICycleCountLocationTaskCreationFactLoader>();
			factLoader.Setup(fl => fl.LoadInputFacts(readOnlyFactory, warehousePK, token)).Returns(cycleCountFacts);

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor(Mock.Of<IWhsCycleCountLocationCreator>(), factLoader.Object);
			AssertEquals("Should return correct results.", cycleCountFacts, processor.LoadInputFacts(readOnlyFactory, ruleSet, token));
		}

		public void TestProcessResults_SuccessfullyCreatingTask()
		{
			var loc1 = (PK: Guid.NewGuid(), LocString: "A-1-1");
			var loc2 = (PK: Guid.NewGuid(), LocString: "A-1-2");
			var loc3 = (PK: Guid.NewGuid(), LocString: "A-2-1");

			var cycleCountFact1 = new CycleCountTaskFact(loc1.PK, loc1.LocString, CycleCountGranularity.Codes.ProductWithAllAttributes, 3);
			var cycleCountFact2 = new CycleCountTaskFact(loc2.PK, loc2.LocString, CycleCountGranularity.Codes.ProductWithPalletID, 1);
			var cycleCountFact3 = new CycleCountTaskFact(loc3.PK, loc3.LocString, CycleCountGranularity.Codes.PalletCount, 9);
			var cycleCountFacts = new IFact[]
			{
				cycleCountFact1,
				cycleCountFact2,
				cycleCountFact3,
			};

			var createCycleCountLocMock = new Mock<IWhsCycleCountLocationCreator>();
			createCycleCountLocMock.
				Setup(m =>
					m.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsCycleCountLocationInfo>>()))
				.Returns((BusinessObjectFactory f, IEnumerable<WhsCycleCountLocationInfo> infos) =>
				{
					var task1 = f.New<WhsCycleCountLocation>();
					task1.WCL_WL_Location = loc1.PK;
					var task2 = f.New<WhsCycleCountLocation>();
					task2.WCL_WL_Location = loc2.PK;
					var task3 = f.New<WhsCycleCountLocation>();
					task3.WCL_WL_Location = loc3.PK;
					return new[] { task1, task2, task3 };
				});

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor(createCycleCountLocMock.Object);
			processor.ProcessResults(Factory, new ProductionRulesEngineResult(cycleCountFacts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("3 Cycle Count Locations should exist", 3, cycleCountLocs.Length);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc1.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc2.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc3.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == GetInformationMessageForNothingProcessed())), Times.Never);

			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
					new WhsCycleCountLocationInfo(loc1.PK, CycleCountGranularity.Codes.ProductWithAllAttributes, 3),
					new WhsCycleCountLocationInfo(loc2.PK, CycleCountGranularity.Codes.ProductWithPalletID, 1),
					new WhsCycleCountLocationInfo(loc3.PK, CycleCountGranularity.Codes.PalletCount, 9),
			};
			createCycleCountLocMock.Verify(c => c.CreateCycleCountLocations(Factory, cycleCountInfos), Times.Once);
		}

		public void TestProcessResults_SkippingLocationAsTaskAlreadyExists()
		{
			var loc1 = (PK: Guid.NewGuid(), LocString: "B-1-1");
			var loc2 = (PK: Guid.NewGuid(), LocString: "B-2-1");
			var cycleCountFact1 = new CycleCountTaskFact(loc1.PK, loc1.LocString, CycleCountGranularity.Codes.ProductOnly, 2);
			var cycleCountFact2 = new CycleCountTaskFact(loc2.PK, loc2.LocString, CycleCountGranularity.Codes.PalletIDOnly, 4);
			var cycleCountFacts = new IFact[]
			{
				cycleCountFact1,
				cycleCountFact2,
			};

			var createCycleCountLocMock = new Mock<IWhsCycleCountLocationCreator>();
			createCycleCountLocMock.
				Setup(m =>
					m.CreateCycleCountLocations(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<WhsCycleCountLocationInfo>>()))
				.Returns((BusinessObjectFactory f, IEnumerable<WhsCycleCountLocationInfo> infos) =>
				{
					var task1 = f.New<WhsCycleCountLocation>();
					task1.WCL_WL_Location = loc1.PK;
					return new[] { task1 };
				});

			var notificationsMock = new Mock<INotifications>();
			var processor = GetProcessor(createCycleCountLocMock.Object);
			processor.ProcessResults(Factory, new ProductionRulesEngineResult(cycleCountFacts), notificationsMock.Object, new CancellationToken());

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("1 Cycle Count Locations should exist", 1, cycleCountLocs.Length);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Created Cycle Count Task for Location: {loc1.LocString}.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == $"Skipped creating Cycle Count Task for Location: {loc2.LocString} as one already exists.")), Times.Once);
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(notif => !notif.Type.IsFatal && notif.Message == GetInformationMessageForNothingProcessed())), Times.Never);

			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
					new WhsCycleCountLocationInfo(loc1.PK, CycleCountGranularity.Codes.ProductOnly, 2),
					new WhsCycleCountLocationInfo(loc2.PK, CycleCountGranularity.Codes.PalletIDOnly, 4),
			};
			createCycleCountLocMock.Verify(c => c.CreateCycleCountLocations(Factory, cycleCountInfos), Times.Once);
		}

		public void TestProcessResults_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 20);
			Factory.Save();

			var facts = new List<CycleCountTaskFact>();
			for (var i = 1; i <= 10; i++)
			{
				var location1 = data.Whs1.FindLocation($"A-1-{i}");
				facts.Add(new CycleCountTaskFact(location1.PK.ToGuid(), location1.WLV_LocationString, CycleCountGranularity.Codes.ProductWithAllAttributes, 3));

				var location2 = data.Whs1.FindLocation($"A-2-{i}");
				facts.Add(new CycleCountTaskFact(location2.PK.ToGuid(), location2.WLV_LocationString, CycleCountGranularity.Codes.PalletCount, 4));

				var location3 = data.Whs1.FindLocation($"A-3-{i}");
				facts.Add(new CycleCountTaskFact(location3.PK.ToGuid(), location3.WLV_LocationString, CycleCountGranularity.Codes.ProductWithPalletID, 3));
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsCycleCountLocationSchema.Constants.TableName, 1 },
			};

			var testFactory = new BusinessObjectFactory();
			var processor = GetProcessor(new WhsCycleCountLocationCreator());
			var notificationsMock = new Mock<INotifications>();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, testFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				processor.ProcessResults(testFactory, new ProductionRulesEngineResult(facts), notificationsMock.Object, new CancellationToken());
				testFactory.Save();
			}

			VerifyNoErrors(notificationsMock);

			var cycleCountLocs = Factory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("30 Cycle Count Locations should exist", 30, cycleCountLocs.Length);
		}

		protected override string GetInformationMessageForNothingProcessed()
			=> "No cycle count tasks were created in this run.";

		protected override GuidRegistryItem GetErrorContactGroupRegistryItem()
			=> WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup;

		static CycleCountTaskCreationRuleProcessor GetProcessor(
			IWhsCycleCountLocationCreator cycleCountLocationCreator = null,
			ICycleCountLocationTaskCreationFactLoader factLoader = null)
		{
			return new CycleCountTaskCreationRuleProcessor(cycleCountLocationCreator ?? Mock.Of<IWhsCycleCountLocationCreator>(), factLoader ?? Mock.Of<ICycleCountLocationTaskCreationFactLoader>());
		}

		protected override IScheduledRuleProcessor GetProcessor() => new CycleCountTaskCreationRuleProcessor(new WhsCycleCountLocationCreator(), new CycleCountLocationTaskCreationFactLoader());
	}
}
