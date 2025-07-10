using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.GPS.Business.Testing
{
	[TestedType(typeof(GPSSupporterActivityCollection))]
	sealed class GPSSupporterActivityCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestReadOnly()
		{
			var collection = new GPSSupporterActivityCollection(Factory.NewWithValidTestData<GPSSupporterActivity>());
			AssertEquals("collection should be readonly", true, collection.ReadOnly);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bizO = Factory.New<GPSSupporterActivity>();
			return new GPSSupporterActivityCollection(bizO);
		}

		[ExpectNoExceptions]
		public void TestInvalidActivityDate()
		{
			var collection = new GPSSupporterActivityCollection(Factory.NewWithValidTestData<GPSSupporterActivity>());
			var vehicle = Factory.New<RefEquipment>();
			var gpsClient = new GPSSupporter(vehicle);
			gpsClient.Activities.AddRange(collection);
			gpsClient.Activities.ActivityFilterDateFrom = new ZDateTime(" ");
			gpsClient.Activities.ActivityFilterDateTo = new ZDateTime(" ");
			gpsClient.Activities.Load();
		}

		public void TestAdditionalFilterWhenActivityFilterDateFromIsValid()
		{
			TestAdditionalFilterForActivityFilterDate(ZDateTime.BrettsBirthday, "EN_ActivityTime > '1971-09-18 00:00:00.000'", "ActivityFilterDateFrom");
		}

		public void TestAdditionalFilterWhenActivityFilterDateFromIsInvalid()
		{
			TestAdditionalFilterForActivityFilterDate(new ZDateTime(1899, 12, 31), "EN_ActivityTime > '1899-12-31 00:00:00.000'", "ActivityFilterDateFrom", false);
			TestAdditionalFilterForActivityFilterDate(new ZDateTime(2079, 6, 7), "EN_ActivityTime > '2079-06-07 00:00:00.000'", "ActivityFilterDateFrom", false);
		}

		public void TestAdditionalFilterWhenActivityFilterDateToIsValid()
		{
			TestAdditionalFilterForActivityFilterDate(ZDateTime.BrettsBirthday, "EN_ActivityTime <= '1971-09-18 00:00:00.000'", "ActivityFilterDateTo");
		}

		public void TestAdditionalFilterWhenActivityFilterDateToIsInvalid()
		{
			TestAdditionalFilterForActivityFilterDate(new ZDateTime(1899, 12, 31), "EN_ActivityTime <= '1899-12-31 00:00:00.000'", "ActivityFilterDateTo", false);
			TestAdditionalFilterForActivityFilterDate(new ZDateTime(2079, 6, 7), "EN_ActivityTime <= '2079-06-07 00:00:00.000'", "ActivityFilterDateTo", false);
		}

		void TestAdditionalFilterForActivityFilterDate(ZDateTime testedTime, string sqlCondition, string activityFilterDate, bool shouldContain = true)
		{
			var collection = new GPSSupporterActivityCollection(Factory.NewWithValidTestData<GPSSupporterActivity>());

			if (activityFilterDate == "ActivityFilterDateFrom")
			{
				collection.ActivityFilterDateFrom = testedTime;
			}
			else if (activityFilterDate == "ActivityFilterDateTo")
			{
				collection.ActivityFilterDateTo = testedTime;
			}
			else
			{
				throw new Exception("Invalid ActivityFilterDate.");
			}

			var query = collection.CompleteFilter;
			var queryAsString = query.LiteralTextSqlFormatted;

			if (shouldContain)
			{
				AssertContains("The filter should contain the SQL condition for the " + activityFilterDate + " condition.", sqlCondition, queryAsString);
			}
			else
			{
				AssertNotContains("The filter should not contain the SQL condition for the " + activityFilterDate + " condition.", sqlCondition, queryAsString);
			}
		}
	}
}
