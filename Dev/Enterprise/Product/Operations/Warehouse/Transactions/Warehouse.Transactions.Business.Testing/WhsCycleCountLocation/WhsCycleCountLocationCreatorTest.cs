using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationCreatorTest : WhsTestCaseWithFactory
	{
		public void TestCreateCycleCount_No_Location()
		{
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			AssertExceptionThrown<ArgumentNullException>("Method should check null value for the location.", () => cycleCountLocationCreator.CreateCycleCountLocation(null));
		}

		public void TestCreateCycleCount_No_Factory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");

			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			AssertExceptionThrown<ArgumentNullException>("Method should check null value for the factory.", () => cycleCountLocationCreator.CreateCycleCountLocation(null, data.Whs1.DefaultLocation.PK, CycleCountGranularity.Codes.ProductWithPalletID));
		}

		public void TestCreateCycleCount_Cycle_Count_Exist()
		{
			TestCreateCycleCount_Cycle_Count_Exist_Core();
		}

		public void TestCreateCycleCount_Cycle_Count_Exist_Overload()
		{
			TestCreateCycleCount_Cycle_Count_Exist_Core(true);
		}

		void TestCreateCycleCount_Cycle_Count_Exist_Core(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "TTT");
			Assert("Cycle Count is not finished", cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount);
			}

			AssertNull("Cycle Count is not created", createdCycleCount);
		}

		public void TestCreateCycleCount_Cycle_Count_Not_Exist()
		{
			TestCreateCycleCount_Cycle_Count_Not_Exist_Core();
		}

		public void TestCreateCycleCount_Cycle_Count_Not_Exist_Overload()
		{
			TestCreateCycleCount_Cycle_Count_Not_Exist_Core(true);
		}

		void TestCreateCycleCount_Cycle_Count_Not_Exist_Core(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			AssertEquals("Cycle Count is finished", true, !cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount);
			}

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", location.PK, createdCycleCount.WCL_WL_Location);
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Core();
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance_Overload()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Core(true);
		}

		void TestCreateCycleCount_Cycle_Count_Open_Variance_Core(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1);

			AssertEquals("Cycle Count is finalised", false, cycleCount.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatus.Codes.Open, locationVariance.WCC_Status);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount);
			}

			AssertNull("Cycle Count is not created", createdCycleCount);
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Rejected()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Rejected_Core();
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Approved()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Approved_Core();
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Rejected_Overload()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Rejected_Core(true);
		}

		public void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Approved_Overload()
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Approved_Core(true);
		}

		void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Rejected_Core(bool overloadMethod = false)
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Core(CycleCountVarianceStatus.Codes.Rejected);
		}

		void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Approved_Core(bool overloadMethod = false)
		{
			TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Core(CycleCountVarianceStatus.Codes.Approved);
		}

		void TestCreateCycleCount_Cycle_Count_Open_Variance_Not_Exist_Variance_Core(ZString variance, bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, variance,
				varianceQty: 0, expectedQty: 10, client: data.Org1, part: data.Part1);

			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(warehouse.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var locationVariance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 0, expectedQty: 10, client: data.Org1, part: data.Part1);

			AssertEquals("Cycle Count is finished", true, !cycleCount.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance status should be Rejected", variance, locationVariance.WCC_Status);

			AssertEquals("Cycle Count is not finished for a second location", false, cycleCount2.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance status should be Open for a second location", CycleCountVarianceStatus.Codes.Open, locationVariance2.WCC_Status);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount);
			}

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", location.PK, createdCycleCount.WCL_WL_Location);
		}

		public void TestCreateCycleCount_Default_Granularity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			var createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(data.Whs1.DefaultLocation);

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", data.Whs1.DefaultLocation.PK, createdCycleCount.WCL_WL_Location);
			AssertEquals("Default granularity should be set", CycleCountGranularity.Codes.ProductWithAttributes, createdCycleCount.WCL_Granularity);
		}

		public void TestCreateCycleCount_Specified_Granularity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			var createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(data.Whs1.DefaultLocation.Factory, data.Whs1.DefaultLocation.PK, CycleCountGranularity.Codes.ProductWithPalletID);

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", data.Whs1.DefaultLocation.PK, createdCycleCount.WCL_WL_Location);
			AssertEquals("Specified granularity should be set", CycleCountGranularity.Codes.ProductWithPalletID, createdCycleCount.WCL_Granularity);
		}

		public void TestCreateCycleCount_Default_Priority()
		{
			TestCreateCycleCount_Default_Priority_Core();
		}

		public void TestCreateCycleCount_Default_Priority_Overload()
		{
			TestCreateCycleCount_Default_Priority_Core(true);
		}

		void TestCreateCycleCount_Default_Priority_Core(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount);
			}

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", location.PK, createdCycleCount.WCL_WL_Location);
			AssertEquals("Priority should be set to zero by default", (ZByte)0, createdCycleCount.WCL_Priority);
		}

		public void TestCreateCycleCount_Specified_Priority()
		{
			TestCreateCycleCount_Specified_Priority_Core();
		}

		public void TestCreateCycleCount_Specified_Priority_Overload()
		{
			TestCreateCycleCount_Specified_Priority_Core(true);
		}

		void TestCreateCycleCount_Specified_Priority_Core(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = new WhsCycleCountLocationCreator();
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location, 5);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount, 5);
			}

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", location.PK, createdCycleCount.WCL_WL_Location);
			AssertEquals("Specified priority should be set", (ZByte)5, createdCycleCount.WCL_Priority);
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocation()
		{
			TestIWhsCycleCountLocationCreator_CreateCycleCountLocationCore();
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocation_Overload()
		{
			TestIWhsCycleCountLocationCreator_CreateCycleCountLocationCore(true);
		}

		void TestIWhsCycleCountLocationCreator_CreateCycleCountLocationCore(bool overloadMethod = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));
			WhsCycleCountLocation createdCycleCount;
			if (overloadMethod)
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location.Factory, location.PK, CycleCountGranularity.Codes.PalletCount, 5);
			}
			else
			{
				createdCycleCount = cycleCountLocationCreator.CreateCycleCountLocation(location);
			}

			AssertNotNull("Cycle Count is created", createdCycleCount);
			AssertEquals("Cycle Count should be created to the specified location", location.PK, createdCycleCount.WCL_WL_Location);

			if (overloadMethod)
			{
				AssertEquals("Cycle Count should be created to the specified granularity", CycleCountGranularity.Codes.PalletCount, createdCycleCount.WCL_Granularity);
				AssertEquals("Cycle Count should be created to the specified priority", (byte)5, createdCycleCount.WCL_Priority);
			}
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
				new WhsCycleCountLocationInfo(location1.PK.ToGuid(), CycleCountGranularity.Codes.PalletCount, 4),
				new WhsCycleCountLocationInfo(location2.PK.ToGuid(), CycleCountGranularity.Codes.ProductWithAllAttributes, 1),
			};
			var createdCycleCounts =
				cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("Cycle Counts is created", 2, createdCycleCounts.Count());
			var cycleCountTask1 = createdCycleCounts.First();
			AssertEquals("Cycle Count 1 should be created to the specified location", location1.PK, cycleCountTask1.WCL_WL_Location);
			AssertEquals("Cycle Count 1 should be created to the specified granularity", CycleCountGranularity.Codes.PalletCount, cycleCountTask1.WCL_Granularity);
			AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)4, cycleCountTask1.WCL_Priority);

			var cycleCountTask2 = createdCycleCounts.Last(); 
			AssertEquals("Cycle Count 2 should be created to the specified location", location2.PK, cycleCountTask2.WCL_WL_Location);
			AssertEquals("Cycle Count 2 should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAllAttributes, cycleCountTask2.WCL_Granularity);
			AssertEquals("Cycle Count 2 should be created to the specified priority", (byte)1, cycleCountTask2.WCL_Priority);
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocations_HasOpenVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");

			var locationVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1);

			AssertEquals("Cycle Count is not finished", false, cycleCount.WCL_EndTime.IsEmpty);
			AssertEquals("Cycle Count Location Variance should be open", CycleCountVarianceStatus.Codes.Open, locationVariance.WCC_Status);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
				new WhsCycleCountLocationInfo(location1.PK.ToGuid(), CycleCountGranularity.Codes.PalletCount, 4),
				new WhsCycleCountLocationInfo(location2.PK.ToGuid(), CycleCountGranularity.Codes.ProductWithAllAttributes, 1),
			};
			var createdCycleCounts =
				cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location2.PK, createdCycleCounts.Single().WCL_WL_Location);
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocations_EndTimeIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var location1 = data.Whs1.DefaultLocation;
			var location2 = data.Whs1.FindLocation("A-2-2");
			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, ZDateTimeOffset.Empty, "TTT");

			Assert("Cycle Count is not finished", cycleCount.WCL_EndTime.IsEmpty);
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));

			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
				new WhsCycleCountLocationInfo(location1.PK.ToGuid(), CycleCountGranularity.Codes.PalletCount, 4),
				new WhsCycleCountLocationInfo(location2.PK.ToGuid(), CycleCountGranularity.Codes.ProductWithAllAttributes, 1),
			};
			var createdCycleCounts =
				cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);

			AssertEquals("1 Cycle Count is created", 1, createdCycleCounts.Count());
			AssertEquals("Cycle Count should be created to the specified location2", location1.PK, createdCycleCounts.Single().WCL_WL_Location);
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocations_LocationPKs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var locationType1 = Helper.CreateLocationType("ECO");
			locationType1.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.PalletCount;
			var locationType2 = Helper.CreateLocationType("TST");
			locationType2.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.PalletIDOnly;
			var locationType3 = Helper.CreateLocationType("RFD");
			locationType3.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.ProductWithAllAttributes;

			var location1 = data.Whs1.DefaultLocation;
			location1.WLV_WLT_LocationType = locationType1.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WLT_LocationType = locationType2.PK;
			var location3 = data.Whs1.FindLocation("A-2-2");
			location3.WLV_WLT_LocationType = locationType3.PK;
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));

			var locPKs = new List<ZGuid>
			{
				location1.PK.ToGuid(),
				location2.PK.ToGuid(),
				location3.PK.ToGuid(),
			};
			var createdCycleCounts =
				cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, locPKs);

			AssertEquals("Cycle Counts is created", 3, createdCycleCounts.Count());
			var cycleCountTask1 = createdCycleCounts.Single(c => c.WCL_WL_Location == location1.PK);
			AssertEquals("Cycle Count 1 should be created to the specified default granularity", CycleCountGranularity.Codes.PalletCount, cycleCountTask1.WCL_Granularity);
			AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)0, cycleCountTask1.WCL_Priority);

			var cycleCountTask2 = createdCycleCounts.Single(c => c.WCL_WL_Location == location2.PK);
			AssertEquals("Cycle Count 2 should be created to the specified default granularity", CycleCountGranularity.Codes.PalletIDOnly, cycleCountTask2.WCL_Granularity);
			AssertEquals("Cycle Count 2 should be created to the specified priority", (byte)0, cycleCountTask2.WCL_Priority);

			var cycleCountTask3 = createdCycleCounts.Single(c => c.WCL_WL_Location == location3.PK);
			AssertEquals("Cycle Count 3 should be created to the specified default granularity", CycleCountGranularity.Codes.ProductWithAllAttributes, cycleCountTask3.WCL_Granularity);
			AssertEquals("Cycle Count 3 should be created to the specified priority", (byte)0, cycleCountTask3.WCL_Priority);
		}

		public void TestIWhsCycleCountLocationCreator_CreateCycleCountLocations_LocationPKs_HighPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var locationType1 = Helper.CreateLocationType("ECO");
			locationType1.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.PalletCount;
			var locationType2 = Helper.CreateLocationType("TST");
			locationType2.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.PalletIDOnly;
			var locationType3 = Helper.CreateLocationType("RFD");
			locationType3.WLT_DefaultCycleCountGranularity = CycleCountGranularity.Codes.ProductWithAllAttributes;

			var location1 = data.Whs1.DefaultLocation;
			location1.WLV_WLT_LocationType = locationType1.PK;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_WLT_LocationType = locationType2.PK;
			var location3 = data.Whs1.FindLocation("A-2-2");
			location3.WLV_WLT_LocationType = locationType3.PK;
			Factory.Save();

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));

			var locPKs = new List<ZGuid>
			{
				location1.PK.ToGuid(),
				location2.PK.ToGuid(),
				location3.PK.ToGuid(),
			};
			var createdCycleCounts =
				cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, locPKs, 1);

			AssertEquals("Cycle Counts is created", 3, createdCycleCounts.Count());
			var cycleCountTask1 = createdCycleCounts.Single(c => c.WCL_WL_Location == location1.PK);
			AssertEquals("Cycle Count 1 should be created to the specified default granularity", CycleCountGranularity.Codes.PalletCount, cycleCountTask1.WCL_Granularity);
			AssertEquals("Cycle Count 1 should be created to the highest priority", (byte)1, cycleCountTask1.WCL_Priority);

			var cycleCountTask2 = createdCycleCounts.Single(c => c.WCL_WL_Location == location2.PK);
			AssertEquals("Cycle Count 2 should be created to the specified default granularity", CycleCountGranularity.Codes.PalletIDOnly, cycleCountTask2.WCL_Granularity);
			AssertEquals("Cycle Count 2 should be created to the highest priority", (byte)1, cycleCountTask2.WCL_Priority);

			var cycleCountTask3 = createdCycleCounts.Single(c => c.WCL_WL_Location == location3.PK);
			AssertEquals("Cycle Count 3 should be created to the specified default granularity", CycleCountGranularity.Codes.ProductWithAllAttributes, cycleCountTask3.WCL_Granularity);
			AssertEquals("Cycle Count 3 should be created to the highest priority", (byte)1, cycleCountTask3.WCL_Priority);
		}

		#region TaskPlanningStatus

		public void TestCreateCycleCountLocation_Location_TaskPlanningStatus()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var location1 = whs1.FindLocation("A-1");
			var location2 = whs2.FindLocation("B-1");

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));
			var cycleCountTask1 = cycleCountLocationCreator.CreateCycleCountLocation(location1);
			var cycleCountTask2 = cycleCountLocationCreator.CreateCycleCountLocation(location2);
			CombineAssertions(() =>
			{
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask1.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask2.WCL_TaskPlanningStatus);
			});
		}

		public void TestCreateCycleCountLocation_LocationPK_TaskPlanningStatus()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var location1 = whs1.FindLocation("A-1");
			var location2 = whs2.FindLocation("B-1");

			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));
			var cycleCountTask1 = cycleCountLocationCreator.CreateCycleCountLocation(location1.Factory, location1.PK, CycleCountGranularity.Codes.PalletCount);
			var cycleCountTask2 = cycleCountLocationCreator.CreateCycleCountLocation(location2.Factory, location2.PK, CycleCountGranularity.Codes.PalletCount);
			CombineAssertions(() =>
			{
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask1.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask2.WCL_TaskPlanningStatus);
			});
		}

		public void TestCreateCycleCountLocations_LocationPKs_TaskPlanningStatus()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var location1 = whs1.FindLocation("A-1");
			var location2 = whs1.FindLocation("A-2");
			var location3 = whs2.FindLocation("B-1");
			var location4 = whs2.FindLocation("B-2");
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, [location1.PK, location2.PK, location3.PK, location4.PK]);
			var cycleCountTask1 = createdCycleCounts.Single(s => s.WCL_WL_Location == location1.PK);
			var cycleCountTask2 = createdCycleCounts.Single(s => s.WCL_WL_Location == location2.PK);
			var cycleCountTask3 = createdCycleCounts.Single(s => s.WCL_WL_Location == location3.PK);
			var cycleCountTask4 = createdCycleCounts.Single(s => s.WCL_WL_Location == location4.PK);
			CombineAssertions(() =>
			{
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask1.WCL_TaskPlanningStatus);
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask2.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask3.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask4.WCL_TaskPlanningStatus);
			});
		}

		public void TestCreateCycleCountLocations_LocationInfos_TaskPlanningStatus()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var whs1 = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 10, 1);
			Factory.Save();

			whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var location1 = whs1.FindLocation("A-1");
			var location2 = whs1.FindLocation("A-2");
			var location3 = whs2.FindLocation("B-1");
			var location4 = whs2.FindLocation("B-2");
			var cycleCountLocationCreator = ObjectFactory.Get<IWhsCycleCountLocationCreator>(nameof(IWhsCycleCountLocationCreator));
			var cycleCountInfos = new List<WhsCycleCountLocationInfo>
			{
				new WhsCycleCountLocationInfo(location1.PK.ToGuid(), CycleCountGranularity.Codes.PalletCount, 4),
				new WhsCycleCountLocationInfo(location2.PK.ToGuid(), CycleCountGranularity.Codes.ProductWithAllAttributes, 1),
				new WhsCycleCountLocationInfo(location3.PK.ToGuid(), CycleCountGranularity.Codes.PalletCount, 4),
				new WhsCycleCountLocationInfo(location4.PK.ToGuid(), CycleCountGranularity.Codes.ProductWithAllAttributes, 1),
			};
			var createdCycleCounts = cycleCountLocationCreator.CreateCycleCountLocations(location1.Factory, cycleCountInfos);
			var cycleCountTask1 = createdCycleCounts.Single(s => s.WCL_WL_Location == location1.PK);
			var cycleCountTask2 = createdCycleCounts.Single(s => s.WCL_WL_Location == location2.PK);
			var cycleCountTask3 = createdCycleCounts.Single(s => s.WCL_WL_Location == location3.PK);
			var cycleCountTask4 = createdCycleCounts.Single(s => s.WCL_WL_Location == location4.PK);
			CombineAssertions(() =>
			{
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask1.WCL_TaskPlanningStatus);
				AssertEquals(TaskPlanningStatus.Codes.Ready, cycleCountTask2.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask3.WCL_TaskPlanningStatus);
				AssertEquals(string.Empty, cycleCountTask4.WCL_TaskPlanningStatus);
			});
		}

		#endregion
	}
}
