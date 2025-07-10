using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemCycleCountLocationCreatorTest : WhsTransitTestCaseWithFactory
	{
		public void TestIWhsItemCycleCountLocationCreator_CreateCycleCountLocations()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsItemCycleCountLocationCreator>(nameof(IWhsItemCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
				new WhsItemCycleCountLocationInfo(location1.PK.ToGuid(), 4),
				new WhsItemCycleCountLocationInfo(location2.PK.ToGuid(), 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("Cycle Counts are created", 2, createdCycleCounts.Count());
			var cycleCountTask1 = createdCycleCounts.First();
			AssertEquals("Cycle Count 1 should be created to the specified location", location1.PK, cycleCountTask1.WIC_WL_Location);
			AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)4, cycleCountTask1.WIC_Priority);

			var cycleCountTask2 = createdCycleCounts.Last();
			AssertEquals("Cycle Count 2 should be created to the specified location", location2.PK, cycleCountTask2.WIC_WL_Location);
			AssertEquals("Cycle Count 2 should be created to the specified priority", (byte)1, cycleCountTask2.WIC_Priority);
		}

		public void TestIWhsItemCycleCountLocationCreator_CreateCycleCountLocations_HasOpenVariance()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(data.Whs1.DefaultLocation, CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateCycleCountLocationVariance(cycleCount, varianceQty: 1);

			AssertEquals("Cycle Count is not finished", false, cycleCount.WIC_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatuses.Codes.Open, locationVariance.WIV_Status);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsItemCycleCountLocationCreator>(nameof(IWhsItemCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
				new WhsItemCycleCountLocationInfo(location1.PK.ToGuid(), 4),
				new WhsItemCycleCountLocationInfo(location2.PK.ToGuid(), 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location2.PK, createdCycleCounts.Single().WIC_WL_Location);
		}

		public void TestIWhsItemCycleCountLocationCreator_CreateCycleCountLocations_EndTimeIsNull()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateCycleCountLocation(data.Whs1.DefaultLocation, CycleCountLocationStatuses.Codes.InProgress, now, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");

			Assert("Cycle Count is not finished", cycleCount.WIC_EndTime.IsEmpty);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsItemCycleCountLocationCreator>(nameof(IWhsItemCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
				new WhsItemCycleCountLocationInfo(location1.PK.ToGuid(), 4),
				new WhsItemCycleCountLocationInfo(location2.PK.ToGuid(), 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location2.PK, createdCycleCounts.Single().WIC_WL_Location);
		}

		public void TestIWhsItemCycleCountLocationCreator_CreateCycleCountLocations_StatusERR()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;

			var cycleCount = Helper.CreateCycleCountLocation(data.Whs1.DefaultLocation, CycleCountLocationStatuses.Codes.Error, now, now.AddHours(1), ZDateTimeOffset.Empty, "TTT");

			AssertEquals("Cycle Count is in Error state", CycleCountLocationStatuses.Codes.Error, cycleCount.WIC_Status);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsItemCycleCountLocationCreator>(nameof(IWhsItemCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
				new WhsItemCycleCountLocationInfo(location1.PK.ToGuid(), 4),
				new WhsItemCycleCountLocationInfo(location2.PK.ToGuid(), 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location2.PK, createdCycleCounts.Single().WIC_WL_Location);
		}

		public void TestIWhsItemCycleCountLocationCreator_CreateCycleCountLocations_StatusPCV()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;

			var cycleCount = Helper.CreateCycleCountLocation(data.Whs1.DefaultLocation, CycleCountLocationStatuses.Codes.ProcessVariance, now, now.AddHours(1), ZDateTimeOffset.Empty, "TTT");

			AssertEquals("Cycle Count is in Processing Variance state", CycleCountLocationStatuses.Codes.ProcessVariance, cycleCount.WIC_Status);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsItemCycleCountLocationCreator>(nameof(IWhsItemCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsItemCycleCountLocationInfo>
			{
				new WhsItemCycleCountLocationInfo(location1.PK.ToGuid(), 4),
				new WhsItemCycleCountLocationInfo(location2.PK.ToGuid(), 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location2.PK, createdCycleCounts.Single().WIC_WL_Location);
		}
	}
}
