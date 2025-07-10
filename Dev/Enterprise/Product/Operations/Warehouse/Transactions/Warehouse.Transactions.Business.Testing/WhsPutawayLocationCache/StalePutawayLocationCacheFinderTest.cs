using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.Statistics;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class StalePutawayLocationCacheFinderTest : WhsTestCaseWithFactory
	{
		#region TestFindCacheEntriesThatNeedUpdating

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestFindCacheEntriesThatNeedUpdating_SmartParameterisationIsUsed()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var now = ZDateTime.Now;
				var data = new TestDataSimpleEnvironment(factory, 2, 1);
				var warehouse = data.Whs1;
				var location = warehouse.FindLocation("A-1");
				location.LocationType.WLT_SystemLastEditTimeUtc = now;
				factory.Save();

				connection.ExecuteNonQuery("UPDATE STATISTICS WhsLocationView_DoNotUse WITH FULLSCAN");

				// Add Histogram to Cache
				IDbConnectionInternals internals = connection;
				var retriever = new HistogramsRetriever();
				retriever.SaveAllHistogramsToCache(
					(System.Data.Common.DbConnection)internals.InternalDbConnection,
					(System.Data.Common.DbTransaction)internals.InternalDbTransaction,
					new[] { (WhsLocationViewSchema.Constants.SqlSchemaName, "WhsLocationView_DoNotUse") },
					ObjectFactory.New<IStatisticsPersister>());

				// there is a client side histogram cache, we need to reset it for this test
				using (ParameterSuffixer.Instance.TemporaryUseNewCache_ForTest())
				using (connection.TrackExecutedCommands())
				{
					var staleLocations = FindCacheEntriesThatNeedUpdating(factory, warehouse.PK);
					AssertCollectionContains(location.PK, staleLocations);

					var procCommand = connection.ExecutedCommands.SingleOrDefault(c =>
						c.Contains("EXEC WhsGetLocationsNeedTobeUpdatedForSpecificWhsPutawayLocationCache"));
					var regex = new Regex("WLV_WW_Whs = (?<Value>NOMATCH|[0-9]{1,2})");
					var match = regex.Match(procCommand);
					AssertEquals("Smart parameterisation should be used.", $@"EXEC WhsGetLocationsNeedTobeUpdatedForSpecificWhsPutawayLocationCache @WarehousePK = @WhsPK, @MinCacheEntry = @MinEntry

/* Parameter Stats
WLV_WW_Whs = {match.Groups["Value"].Value}
End Parameter Stats */

Params
@WhsPK: '{warehouse.PK}'
@MinEntry: null
", procCommand);
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestFindCacheEntriesThatNeedUpdating_IsRobustToKillStateException()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var now = ZDateTime.Now;
				var data = new TestDataSimpleEnvironment(factory, 2, 1);
				var warehouse = data.Whs1;
				var location = warehouse.FindLocation("A-1");
				location.LocationType.WLT_SystemLastEditTimeUtc = now;
				factory.Save();

				connection.OnExecute += ThrowException;
				try
				{
					AssertExceptionThrown(typeof(SqlException),
						"Cannot continue the execution because the session is in the kill state.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.",
						() => FindCacheEntriesThatNeedUpdating(factory, warehouse.PK));
				}
				finally
				{
					connection.OnExecute -= ThrowException;
				}
			}

			static void ThrowException(DbCommand command)
			{
				if (command.CommandText.Contains("WhsGetLocationsNeedTobeUpdatedForSpecificWhsPutawayLocationCache"))
				{
					command.DbConnection.OnExecute -= ThrowException;
					command.Connection.Close();
					var sqlException = SqlExceptionBuilder.CreateSqlException(596, 0, 11, Db.ServerName, "Cannot continue the execution because the session is in the kill state.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.", "", 0);
					throw sqlException;
				}
			}
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsLocation_LastAllocatedOrChangedDate()
		{
			var now = ZDateTime.UtcNow;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			location.UpdateWLV_LastAllocatedOrChangedDateUtc(now);
			Factory.Save();
			var firstAllocatedOrChangedDate = location.WLV_LastAllocatedOrChangedDateUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.UpdateWLV_LastAllocatedOrChangedDateUtc(ZDateTime.UtcNow);
			Factory.Save();

			Assert("Precondition: Second allocated or changed time should come after the first.",
				location.WLV_LastAllocatedOrChangedDateUtc > firstAllocatedOrChangedDate);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsLocation_LastEditTime()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location, 5, 5);
			Factory.Save();
			var firstEditTime = location.WLV_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			SetLocationWeightAndCubicConstraints(location, 10, 10);
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.WLV_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsLocationType()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			location.LocationType.WLT_Description = "ZAK";
			Factory.Save();
			var firstEditTime = location.LocationType.WLT_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.LocationType.WLT_Description = "ZAL";
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.LocationType.WLT_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsPickFace()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = pickface.WF_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);

			pickface.WF_ReplenishMinimum = 10;
			pickface.WF_ReplenishMaximum = 20;
			Factory.Save();
			Assert("Precondition: Second edit time should come after the first.",
				pickface.WF_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsPickFace_Multiple()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location);
			location.LocationType.WLT_MaximumNumberOfProducts = 2;
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = pickface1.WF_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);

			pickface1.WF_ReplenishMinimum = 10;
			pickface1.WF_ReplenishMaximum = 20;
			Factory.Save();
			Assert("Precondition: Second edit time should come after the first.",
				pickface1.WF_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsPickFace_LocationFKChanged()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;

			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			location1.LocationType.WLT_SystemLastEditTimeUtc = now;
			location2.WLV_WLT_LocationType = location1.WLV_WLT_LocationType;
			Factory.Save();
			var firstEditTime = pickface.WF_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertEquals("Precondition: No stale locations.", false, staleLocations1.Any());

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			pickface.WF_WL = location2.PK;
			Factory.Save();
			Assert("Precondition: Second edit time should come after the first.",
				pickface.WF_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertContainsExactElementsInAnyOrder("Both locations should be considered stale.",
				new[] { location1.PK, location2.PK }, staleLocations2);

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location1.PK);
			pickface.WF_ReplenishMinimum = 10;
			pickface.WF_ReplenishMaximum = 20;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertContainsExactElementsInAnyOrder("Only the current pick face location should be considered stale.",
				new[] { location2.PK }, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_Fixed_WithNoPickFace()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;

			var locationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC"));
			var location = warehouse.FindLocation("A-1");
			location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			pickface.WF_ReplenishMinimum = 10;
			pickface.WF_ReplenishMaximum = 20;
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsRow()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = location.Row.WR_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.Row.WR_Name = "SAL";
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.Row.WR_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_PickingWhsArea()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = location.PickingArea.WA_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.PickingArea.WA_Name = "CHILLED";
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.PickingArea.WA_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_PutawayWhsArea()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = location.PutawayArea.WA_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.PutawayArea.WA_Name = "CHILLED";
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.PutawayArea.WA_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_WhsWarehouse()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = location.Warehouse.WW_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			location.Warehouse.WW_WarehouseName = "SCRAPYARD";
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				location.Warehouse.WW_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart_Finalised() =>
			TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart(true);

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart_NotFinalised() =>
			TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart(false);

		void TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart(bool finalised)
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			SetPartWeightAndCubicAttributes(data.Part1, 5, 5);
			SetPartWeightAndCubicAttributes(data.Part2, 3, 3);
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 =
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3,
					location); // To show that it can search through a list of OrgSupplierParts
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();
			var firstEditTime = data.Part1.OP_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);
			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				data.Part1.OP_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location.PK, staleLocations2);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_OrgSupplierPart_Cancelled()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			SetPartWeightAndCubicAttributes(data.Part1, 5, 5);
			SetPartWeightAndCubicAttributes(data.Part2, 3, 3);
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 =
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3,
					location); // To show that it can search through a list of OrgSupplierParts
			Factory.Save();

			receive.WD_DocketStatus = DocketStatus.Codes.Entered; // hack
			receive.CancelReactivateDocket();
			AssertEquals("Receive should be successfully cancelled.", true, receive.IsCancelled);
			Factory.Save();
			var firstEditTime = data.Part1.OP_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations1);

			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);
			Factory.Save();

			Assert("Precondition: Second edit time should come after the first.",
				data.Part1.OP_SystemLastEditTimeUtc > firstEditTime);

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Precondition: Returned collection of stale locations should NOT include this location.", location.PK,
				staleLocations2);
		}

		#endregion

		#region TestFindCacheEntriesThatNeedUpdating_MultipleWarehouses

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_MultipleWarehouses()
		{
			TestFindCacheEntriesThatNeedUpdating_MultipleWarehousesCore();
		}

		protected virtual void TestFindCacheEntriesThatNeedUpdating_MultipleWarehousesCore()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse1 = data.Whs1;
			var location1 = warehouse1.FindLocation("A-1");
			location1.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location1, 5, 5);

			var warehouse2 = Helper.CreateWarehouse("WHS2");
			var location2 = Helper.CreateRowAndGenerateLocations(warehouse2, "AA", 1, 1, 1).Locations.Single();
			location2.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location2, 5, 5);

			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location1.PK);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location2.PK);
			var staleLocationInfos = ObjectFactory.Get<IStalePutawayLocationCacheFinder>()
				.FindCacheEntriesThatNeedUpdating(Factory);
			AssertEquals("Returned collection of stale locations should not include both locations.", false,
				staleLocationInfos.Any(staleLocation => staleLocation.LocationPK == location1.PK));
			AssertEquals("Returned collection of stale locations should not include both locations.", false,
				staleLocationInfos.Any(staleLocation => staleLocation.LocationPK == location2.PK));

			SetLocationWeightAndCubicConstraints(location1, 10, 10);
			SetLocationWeightAndCubicConstraints(location2, 10, 10);
			Factory.Save();

			var staleLocationInfos2 = ObjectFactory.Get<IStalePutawayLocationCacheFinder>()
				.FindCacheEntriesThatNeedUpdating(Factory).ToArray();
			Assert("Returned stale locations should have both warehouse locations.",
				staleLocationInfos2.Any(info => info.LocationPK == location1.PK && info.WarehousePK == warehouse1.PK));
			Assert("Returned stale locations should have both warehouse locations.",
				staleLocationInfos2.Any(info => info.LocationPK == location2.PK && info.WarehousePK == warehouse2.PK));
		}

		#endregion

		#region TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer

		[TestDate(2018, 10, 01)]
		public void
			TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_WhsLocation_LastAllocatedOrChangedDate()
		{
			var now = ZDateTime.UtcNow;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			location.UpdateWLV_LastAllocatedOrChangedDateUtc(now);
			Factory.Save();
			var firstAllocatedOrChangedDate = location.WLV_LastAllocatedOrChangedDateUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] >
				firstAllocatedOrChangedDate);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_WhsLocation_LastEditTime()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location, 5, 5);
			Factory.Save();
			var firstEditTime = location.WLV_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Factory.Save();

			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] > firstEditTime);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_WhsLocationType()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			location.LocationType.WLT_Description = "ZAK";
			Factory.Save();
			var firstEditTime = location.LocationType.WLT_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Factory.Save();

			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] > firstEditTime);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_WhsPickFace()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = pickface.WF_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Factory.Save();

			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] > firstEditTime);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_WhsRow()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();
			var firstEditTime = location.Row.WR_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Factory.Save();

			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] > firstEditTime);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_OrgSupplierPart_Finalised() =>
			TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_OrgSupplierPart(true);

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_OrgSupplierPart_NotFinalised() =>
			TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_OrgSupplierPart(false);

		void TestFindCacheEntriesThatNeedUpdating_IgnoresCurrentWithBuffer_OrgSupplierPart(bool finalised)
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			SetPartWeightAndCubicAttributes(data.Part1, 5, 5);
			SetPartWeightAndCubicAttributes(data.Part2, 3, 3);
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 =
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3,
					location); // To show that it can search through a list of OrgSupplierParts
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();
			var firstEditTime = data.Part1.OP_SystemLastEditTimeUtc;

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			var cacheManager = new WhsPutawayLocationCacheManager();
			cacheManager.CreateCache(Factory, location.PK);
			var cache = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Single(c =>
				((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]) == location.PK);
			Factory.Save();

			Assert("Precondition: Cache time should be after location last allocated or changed time.",
				(DateTime)cache[WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name] > firstEditTime);

			var staleLocations = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location.PK, staleLocations);
		}

		#endregion

		#region TestFindCacheEntriesThatNeedUpdating_FindsOnOrganisationMerge

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_FindsOnOrganisationMerge()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var oldOrg = data.Org1;
			var newOrg = Helper.CreateClient("ALS");
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, oldOrg, location1);
			var pickface2 = Helper.CreateProductPickFace(data.Part1, newOrg, location2);
			location1.LocationType.WLT_SystemLastEditTimeUtc = now;
			location2.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains("Precondition: Stale Locations should not include this location.", location1.PK,
				staleLocations1);
			AssertCollectionNotContains("Precondition: Stale Locations should not include this location.", location2.PK,
				staleLocations1);

			var mergeHeader = new MergeOrgHeader(Factory, oldOrg, newOrg);
			var testMerger = new OrganisationMergerForTest(mergeHeader);
			testMerger.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this location.",
				location1.PK, staleLocations2);
			AssertCollectionNotContains("Returned collection of stale locations should NOT include this location.",
				location2.PK, staleLocations2);
		}

		#endregion

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_Voided()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			location.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			SetPartWeightAndCubicAttributes(data.Part1, 5, 5);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			location.WLV_LocationStatus = LocationStatus.Codes.Normal;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_DockDoorLocation()
			=> TestFindCacheEntriesThatNeedUpdating_InvalidLocations_LocationClass(locationClass: LocationClasses.Codes.DDL);

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_PackingStationLocation()
			=> TestFindCacheEntriesThatNeedUpdating_InvalidLocations_LocationClass(locationClass: LocationClasses.Codes.PST);

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_PackingConsolidationLocation()
			=> TestFindCacheEntriesThatNeedUpdating_InvalidLocations_LocationClass(locationClass: LocationClasses.Codes.CON);

		void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_LocationClass(string locationClass)
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			var originalLocationType = location.WLV_WLT_LocationType;
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, locationClass);
			location.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			location.WLV_WLT_LocationType = originalLocationType;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_FixedLocationWithoutPickFace()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			pickface1.Delete();
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location, as cascade delete handled maintenance.",
				location.PK, staleLocations1);

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			location.WLV_LastAllocatedOrChangedDateUtc = now.AddMinutes(10);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			var pickface2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_DynamicLocationWithoutProductParams()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;

			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			var productParams1 = Helper.CreateProductParamsByWhsAndClient(whsProduct.Parent, data.Org1, warehouse);
			productParams1.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			productParams1.Delete();
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			location.WLV_LastAllocatedOrChangedDateUtc = now.AddMinutes(9);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			var productParams2 = Helper.CreateProductParamsByWhsAndClient(whsProduct.Parent, data.Org1, warehouse);
			productParams2.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_TransitWarehouse()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(15).ToDateTime();
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_ContainerYard()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_VirtualWarehouse()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			warehouse.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			warehouse.WW_IsVirtualWarehouse = false;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		[TestDate(2018, 10, 01)]
		public void TestFindCacheEntriesThatNeedUpdating_InvalidLocations_InactiveWarehouse()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_SystemLastEditTimeUtc = now;
			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			warehouse.WW_IsActive = false;
			Factory.Save();

			var staleLocations1 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains(
				"Precondition: Returned collection of stale locations should include this location, to clear it.",
				location.PK, staleLocations1);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location.PK);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			SetLocationWeightAndCubicConstraints(location, 300, 400);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionNotContains(
				"Returned collection of stale locations should *not* include this invalid location.", location.PK,
				staleLocations2);

			warehouse.WW_IsActive = true;
			Factory.Save();

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse.PK);
			AssertCollectionContains("Returned collection of stale locations should include this valid location.",
				location.PK, staleLocations3);
		}

		#region Implementation

		#region PartWeightAndCubic

		void SetPartWeightAndCubicAttributes(OrgSupplierPart part, ZDecimal weight, ZDecimal cubic,
			string weightUQ = "KG", string cubicUQ = "M3")
		{
			SetPartWeightAttributes(part, weight, weightUQ);
			SetPartCubicAttributes(part, cubic, cubicUQ);
		}

		void SetPartWeightAttributes(OrgSupplierPart part, ZDecimal weight, string weightUQ = "KG")
		{
			part.OP_Weight = weight;
			part.OP_WeightUQ = weightUQ;
		}

		void SetPartCubicAttributes(OrgSupplierPart part, ZDecimal cubic, string cubicUQ = "M3")
		{
			part.OP_Cubic = cubic;
			part.OP_CubicUQ = cubicUQ;
		}

		#endregion

		#region LocationWeightAndCubic

		protected void SetLocationWeightAndCubicConstraints(WhsLocation location, ZDecimal weight, ZDecimal cubic,
			string weightUQ = "KG", string cubicUQ = "M3")
		{
			SetLocationWeightConstraints(location, weight, weightUQ);
			SetLocationCubicConstraints(location, cubic, cubicUQ);
		}

		void SetLocationWeightConstraints(WhsLocation location, ZDecimal weight, string weightUQ = "KG")
		{
			location.WLV_MaxWeight = weight;
			location.WLV_MaxWeightUnit = weightUQ;
		}

		void SetLocationCubicConstraints(WhsLocation location, ZDecimal cubic, string cubicUQ = "M3")
		{
			location.WLV_MaxCubic = cubic;
			location.WLV_MaxCubicUnit = cubicUQ;
		}

		#endregion

		void ConditionalFinaliseWithAssertion(WhsReceive receive, bool finalised)
		{
			if (finalised)
			{
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}
		}

		protected virtual IEnumerable<ZGuid> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory,
			ZGuid warehousePK)
		{
			var putawayLocationCacheInfos = ObjectFactory.Get<IStalePutawayLocationCacheFinder>()
				.FindCacheEntriesThatNeedUpdating(factory).ToArray();

			Assert(putawayLocationCacheInfos.All(putawayLocationCacheInfo =>
				putawayLocationCacheInfo.WarehousePK == warehousePK));
			return putawayLocationCacheInfos.Select(putawayLocationCacheInfo => putawayLocationCacheInfo.LocationPK);
		}

		#endregion
	}
}
