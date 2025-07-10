using System;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsLuceneTaskSearchTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsLuceneTaskSearch(null));
		}

		#endregion

		#region TestQueryLuceneForTasks

		public void TestQueryLuceneForTasks_BaseQueries()
		{
			// Arrange
			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.UnloadJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			AssertDefaultTaskQueries(
				capturedQuery.GlowQueries[0].ToUrlComponent(),
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());
		}

		public void TestQueryLuceneForTasks_BaseQueries_AllOptionalFieldsPopulated()
		{
			// Arrange
			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);

			var capability1 = Factory.New<GlbCapability>();
			capability1.G4_Code = "CP1";
			var capability2 = Factory.New<GlbCapability>();
			capability2.G4_Code = "CP2";
			var capability3 = Factory.New<GlbCapability>();
			capability3.G4_Code = "CP3";

			staff.Capabilities.Add(capability1);
			staff.Capabilities.Add(capability2);
			staff.Capabilities.Add(capability3);
			Factory.Save();

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			var tasksToIgnore = new[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
			};

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				"ABC",
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.UnloadJob },
				tasksToIgnore);

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			AssertDefaultTaskQueries(
				capturedQuery.GlowQueries[0].ToUrlComponent(),
				"ABC",
				data.Whs1.WW_WarehouseCode,
				staff,
				tasksToIgnore);
		}

		public void TestQueryLuceneForTasks_InvalidArgs()
		{
			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);
			var emptyException = AssertExceptionThrown<ArgumentException>(() => InvokeQueryLuceneForTasks(Array.Empty<string>()));
			Assert(emptyException.Message.StartsWith("At least one form flow type must be specified."));

			var invalidArgException = AssertExceptionThrown<ArgumentException>(() => InvokeQueryLuceneForTasks(new[] { "ABC" }));
			Assert(invalidArgException.Message.StartsWith("Specified Form Flow Type: 'ABC' is not supported by Lucene."));

			void InvokeQueryLuceneForTasks(string[] formFlowTypes)
			{
				luceneTaskSearch.QueryLuceneForTasks(
					string.Empty,
					staff,
					registry,
					data.Whs1,
					formFlowTypes,
					Array.Empty<Guid>());
			}
		}

		#region TestQueryLuceneForTasks_Unload

		public void TestQueryLuceneForTasks_Unload()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.UnloadJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertUnloadTaskQueries(luceneQuery);
		}

		public void TestQueryLuceneForTasks_Unload_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Unload_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Unload_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Unload_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Unload_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Unload_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Unload_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Unload_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Unload_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.UnloadJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertUnloadTaskQueries(
				luceneQuery,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name);
		}

		#endregion

		#region TestQueryLuceneForTasks_Putaway

		public void TestQueryLuceneForTasks_Putaway()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.PutawayJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(luceneQuery, WarehouseTaskFormFlowTypes.PutawayJob);
		}

		public void TestQueryLuceneForTasks_Putaway_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Putaway_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Putaway_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Putaway_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Putaway_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Putaway_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Putaway_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Putaway_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Putaway_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.PutawayJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(
				luceneQuery,
				WarehouseTaskFormFlowTypes.PutawayJob,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name,
				registry.WRR_PickMethodCode,
				registry.WRR_UOMPackType);
		}

		#endregion

		#region TestQueryLuceneForTasks_Transfer

		public void TestQueryLuceneForTasks_Transfer()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.TransferJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(luceneQuery, WarehouseTaskFormFlowTypes.TransferJob);
		}

		public void TestQueryLuceneForTasks_Transfer_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Transfer_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Transfer_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Transfer_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Transfer_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Transfer_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Transfer_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Transfer_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Transfer_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.TransferJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(
				luceneQuery,
				WarehouseTaskFormFlowTypes.TransferJob,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name,
				registry.WRR_PickMethodCode,
				registry.WRR_UOMPackType);
		}

		#endregion

		#region TestQueryLuceneForTasks_Replenishment

		public void TestQueryLuceneForTasks_Replenishment()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.ReplenishmentJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(luceneQuery, WarehouseTaskFormFlowTypes.ReplenishmentJob);
		}

		public void TestQueryLuceneForTasks_Replenishment_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Replenishment_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Replenishment_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Replenishment_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Replenishment_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Replenishment_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Replenishment_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Replenishment_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Replenishment_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.ReplenishmentJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertTransferTaskQueries(
				luceneQuery,
				WarehouseTaskFormFlowTypes.ReplenishmentJob,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name,
				registry.WRR_PickMethodCode,
				registry.WRR_UOMPackType);
		}

		#endregion

		#region TestQueryLuceneForTasks_Pick

		public void TestQueryLuceneForTasks_Pick()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.PickJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertPickTaskQueries(luceneQuery);
		}

		public void TestQueryLuceneForTasks_Pick_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Pick_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Pick_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Pick_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Pick_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Pick_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Pick_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Pick_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Pick_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.PickJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertPickTaskQueries(
				luceneQuery,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name,
				registry.WRR_PickMethodCode,
				registry.WRR_UOMPackType,
				registry.WRR_PickGroupSequence.ToString());
		}

		#endregion

		#region TestQueryLuceneForTasks_DirectedPacking

		public void TestQueryLuceneForTasks_DirectedPacking()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.DirectedPackingJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertDirectedPackingTaskQueries(luceneQuery);
		}

		public void TestQueryLuceneForTasks_DirectedPacking_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_DirectedPacking_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_DirectedPacking_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_DirectedPacking_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_DirectedPacking_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_DirectedPacking_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_DirectedPacking_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_DirectedPacking_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_DirectedPacking_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.DirectedPackingJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertDirectedPackingTaskQueries(
				luceneQuery,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name);
		}

		#endregion

		#region TestQueryLuceneForTasks_CycleCount

		public void TestQueryLuceneForTasks_CycleCount()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.CycleCountJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertCycleCountQueries(luceneQuery);
		}

		public void TestQueryLuceneForTasks_CycleCount_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_CycleCount_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_CycleCount_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_CycleCount_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_CycleCount_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_CycleCount_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_CycleCount_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_CycleCount_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_CycleCount_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.CycleCountJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertCycleCountQueries(
				luceneQuery,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name,
				registry.WRR_PickMethodCode);
		}

		#endregion

		#region TestQueryLuceneForTasks_Load

		public void TestQueryLuceneForTasks_Load()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = Helper.CreateWhsRFRegistry(staff, data.Whs1);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.LoadJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertLoadTaskQueries(luceneQuery);
		}

		public void TestQueryLuceneForTasks_Load_WithOptionalFields_BothAreas()
		{
			TestQueryLuceneForTasks_Load_WithOptionalFieldsCore("PickingArea", "PutawayArea");
		}

		public void TestQueryLuceneForTasks_Load_WithOptionalFields_NoAreas()
		{
			TestQueryLuceneForTasks_Load_WithOptionalFieldsCore(string.Empty, string.Empty);
		}

		public void TestQueryLuceneForTasks_Load_WithOptionalFields_OnlyPickingArea()
		{
			TestQueryLuceneForTasks_Load_WithOptionalFieldsCore("PickingArea", string.Empty);
		}

		public void TestQueryLuceneForTasks_Load_WithOptionalFields_OnlyPutawayArea()
		{
			TestQueryLuceneForTasks_Load_WithOptionalFieldsCore(string.Empty, "PutawayArea");
		}

		void TestQueryLuceneForTasks_Load_WithOptionalFieldsCore(string pickingArea, string putawayArea)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, pickingArea, putawayArea);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				new[] { WarehouseTaskFormFlowTypes.LoadJob },
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertDefaultTaskQueries(
				luceneQuery,
				string.Empty,
				data.Whs1.WW_WarehouseCode,
				staff,
				Array.Empty<Guid>());

			AssertLoadTaskQueries(
				luceneQuery,
				data.Org1.OH_Code,
				registry.PickingArea?.WA_Name,
				registry.PutawayArea?.WA_Name);
		}

		#endregion

		#region TestQueryLuceneForTasks_BooleanExpressions

		// This test allows us to assert and inspect the resulting query
		// It will hit every supported branch in our logic
		public void TestQueryLuceneForTasks_BooleanExpressions()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("US1", "User");
			var registry = CreateOptionalArgumentData(staff, data.Whs1, data.Org1, "pickingArea", "putawayArea");
			Factory.Save();

			var capability1 = Factory.New<GlbCapability>();
			capability1.G4_Code = "CP1";
			var capability2 = Factory.New<GlbCapability>();
			capability2.G4_Code = "CP2";
			var capability3 = Factory.New<GlbCapability>();
			capability3.G4_Code = "CP3";

			staff.Capabilities.Add(capability1);
			staff.Capabilities.Add(capability2);
			staff.Capabilities.Add(capability3);
			Factory.Save();

			var mockEngine = new Mock<IGlowIndexQueryEngine>();

			GlowIndexQueryParam capturedQuery = null;
			var expectedResult = new GlowIndexQueryResultCollection();

			mockEngine
				.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
				.Callback<GlowIndexQueryParam>(q => capturedQuery = q)
				.Returns(expectedResult);

			var luceneTaskSearch = new WhsLuceneTaskSearch(mockEngine.Object);

			// Act
			var result = luceneTaskSearch.QueryLuceneForTasks(
				string.Empty,
				staff,
				registry,
				data.Whs1,
				WhsLuceneTaskSearch.LuceneSupportedTaskTypes.ToArray(),
				Array.Empty<Guid>());

			// Assert
			AssertEquals(expectedResult, result);
			AssertNotNull(capturedQuery);
			AssertEquals(WhsLuceneProcessTaskDefinitions.ProcessTaskEntityType, capturedQuery.EntityType);

			AssertEquals("Resulting query should be singular.", 1, capturedQuery.GlowQueries.Count);
			AssertEquals("Maximum suported results should be returned.", GlowIndexQueryService.Business.Constants.MAXIMUM_QUERY_RESULTS_RETURNED, capturedQuery.MaxQueryResults);
			AssertEquals("Results should include keywords.", false, capturedQuery.OnlyIncludePk);

			var luceneQuery = capturedQuery.GlowQueries[0].ToUrlComponent();
			AssertEquals("(((Warehouse eq '1') and ((CAPABILITY eq 'CP1') or (CAPABILITY eq 'CP2') or (CAPABILITY eq 'CP3') or ((CAPABILITY eq null) or (CAPABILITY eq ''))) and (ISINABUFFER eq true) and (((Staff eq 'US1') and ((Status eq 'ASN') or (Status eq 'WRK') or (Status eq 'SUS'))) or (((Staff eq null) or (Staff eq '')) and ((Status eq 'OPN') or (Status eq 'ASN'))))) and (((FORMFLOWTYPE eq 'WUL') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea'))) or ((FORMFLOWTYPE eq 'WPT') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC')) or ((FORMFLOWTYPE eq 'WRP') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC')) or ((FORMFLOWTYPE eq 'WTR') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC')) or ((FORMFLOWTYPE eq 'WPK') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC') and (PICKGROUP eq '5')) or ((FORMFLOWTYPE eq 'WDP') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea'))) or ((FORMFLOWTYPE eq 'WCC') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1')) or ((FORMFLOWTYPE eq 'WLO') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')))))", luceneQuery);

			/*(
			 *	(
			 *		(Warehouse eq '1')
			 *		and ((CAPABILITY eq 'CP1') or (CAPABILITY eq 'CP2') or (CAPABILITY eq 'CP3') or ((CAPABILITY eq null) or (CAPABILITY eq '')))
			 *		and (ISINABUFFER eq true)
			 *		and 
			 *		(
			 *			((Staff eq 'US1') and ((Status eq 'ASN') or (Status eq 'WRK') or (Status eq 'SUS')))
			 *			or (((Staff eq null) or (Staff eq '')) and ((Status eq 'OPN') or (Status eq 'ASN')))
			 *		)
			 *	) 
			 *	and
			 *	(
			 *		((FORMFLOWTYPE eq 'WUL') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')))
			 *		or ((FORMFLOWTYPE eq 'WPT') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC'))
			 *		or ((FORMFLOWTYPE eq 'WRP') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC'))
			 *		or ((FORMFLOWTYPE eq 'WTR') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC'))
			 *		or ((FORMFLOWTYPE eq 'WPK') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1') and (UOMTYPE eq 'SPC') and (PICKGROUP eq '5'))
			 *		or ((FORMFLOWTYPE eq 'WDP') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')))
			 *		or ((FORMFLOWTYPE eq 'WCC') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea')) and (PICKMETHOD eq 'TS1'))
			 *		or ((FORMFLOWTYPE eq 'WLO') and (CLIENT eq '111') and ((AREANAME eq 'pickingArea') or (AREANAME eq 'putawayArea'))))
			 *	)
			 */
		}

		#endregion

		#endregion

		#region Assertions

		void AssertDefaultTaskQueries(string queryString, string reference, string warehouseCode, GlbStaff staff, Guid[] tasksToIgnore)
		{
			var warehouseQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Warehouse, warehouseCode)).ToUrlComponent();
			Assert("Warehouse filter missing", queryString.Contains(warehouseQuery));

			AssertOptionalFilterExists(queryString, reference, WhsLuceneProcessTaskDefinitions.Reference);

			var capabilities = staff.Capabilities.Cast<GlbCapability>().Select(c => c.G4_Code.ToString());
			foreach (var capability in capabilities)
			{
				var capabilityQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Capability, capability)).ToUrlComponent();
				Assert($"Capability filter for '{capability}' missing", queryString.Contains(capabilityQuery));
			}
			var emptyCapabilityQuery = new IsBlankQuery(new Term(WhsLuceneProcessTaskDefinitions.Capability, string.Empty)).ToUrlComponent();
			Assert(queryString.Contains(emptyCapabilityQuery));

			var isInBufferQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.IsInABuffer, "true"), useQuotes: false).ToUrlComponent();
			Assert("ISINABUFFER filter missing", queryString.Contains(isInBufferQuery));

			var staffAssignedQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Staff, staff.GS_Code));
			var isAssignedQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Assigned));
			var isWorkingQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Working));
			var isSuspendedQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Suspended));
			var staffAssignedStatusQuery = new BooleanQuery(BooleanOperator.Or, isAssignedQuery, isWorkingQuery, isSuspendedQuery);
			var isAssignedToStaffQuery = new BooleanQuery(BooleanOperator.And, staffAssignedQuery, staffAssignedStatusQuery).ToUrlComponent();
			Assert("Assigned To Staff filter missing", queryString.Contains(isAssignedToStaffQuery));

			var staffUnassignedQuery = new IsBlankQuery(new Term(WhsLuceneProcessTaskDefinitions.Staff, string.Empty));
			var isOpenQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.Status, ProcessTaskStatusCodeList.Codes.Open));
			var staffUnassignedStatusQuery = new BooleanQuery(BooleanOperator.Or, isOpenQuery, isAssignedQuery);
			var isUnassignedForAnyStaffQuery = new BooleanQuery(BooleanOperator.And, staffUnassignedQuery, staffUnassignedStatusQuery).ToUrlComponent();
			Assert("Unassigned To Staff filter missing", queryString.Contains(isUnassignedForAnyStaffQuery));

			if (tasksToIgnore.Length > 0)
			{
					var tasksToIgnoreQuery = new BooleanQuery(
						BooleanOperator.And,
						tasksToIgnore.Select(pk => new NotEqualQuery(new Term(WhsLuceneProcessTaskDefinitions.PK, pk.ToString()), useQuotes: false)).ToArray()
					).ToUrlComponent();
				Assert("PK filter missing", queryString.Contains(tasksToIgnoreQuery));
			}
			else
			{
				Assert("PK filter should not exist.", !queryString.Contains($"{WhsLuceneProcessTaskDefinitions.PK} eq"));
				Assert("PK filter should not exist.", !queryString.Contains($"{WhsLuceneProcessTaskDefinitions.PK} ne"));
			}
		}

		void AssertUnloadTaskQueries(
			string queryString,
			string clientCode = null,
			string pickingArea = null,
			string putawayArea = null)
		{
			var unloadJobQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.UnloadJob)).ToUrlComponent();
			Assert("UnloadJob FormFlowType filter missing", queryString.Contains(unloadJobQuery));

			AssertOptionalFilterExists(queryString, clientCode, WhsLuceneProcessTaskDefinitions.Client);
			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
		}

		void AssertTransferTaskQueries(
			string queryString,
			string formFlowType,
			string clientCode = null,
			string pickingArea = null,
			string putawayArea = null,
			string pickMethod = null,
			string packType = null)
		{
			var jobQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, formFlowType)).ToUrlComponent();
			Assert("Job FormFlowType filter missing", queryString.Contains(jobQuery));

			AssertOptionalFilterExists(queryString, clientCode, WhsLuceneProcessTaskDefinitions.Client);
			AssertOptionalFilterExists(queryString, pickMethod, WhsLuceneProcessTaskDefinitions.PickMethod);
			AssertOptionalFilterExists(queryString, packType, WhsLuceneProcessTaskDefinitions.UOMType);
			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
		}

		void AssertPickTaskQueries(
			string queryString,
			string clientCode = null,
			string pickingArea = null,
			string putawayArea = null,
			string pickMethod = null,
			string packType = null,
			string pickGroup = null)
		{
			var jobQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.PickJob)).ToUrlComponent();
			Assert("Job FormFlowType filter missing", queryString.Contains(jobQuery));

			AssertOptionalFilterExists(queryString, clientCode, WhsLuceneProcessTaskDefinitions.Client);
			AssertOptionalFilterExists(queryString, pickMethod, WhsLuceneProcessTaskDefinitions.PickMethod);
			AssertOptionalFilterExists(queryString, packType, WhsLuceneProcessTaskDefinitions.UOMType);
			AssertOptionalFilterExists(queryString, pickGroup, WhsLuceneProcessTaskDefinitions.PickGroup);
			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
		}

		void AssertDirectedPackingTaskQueries(
			string queryString,
			string clientCode = null,
			string pickingArea = null,
			string putawayArea = null)
		{
			var directedPackingQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.DirectedPackingJob)).ToUrlComponent();
			Assert("Directed Packing FormFlowType filter missing", queryString.Contains(directedPackingQuery));

			AssertOptionalFilterExists(queryString, clientCode, WhsLuceneProcessTaskDefinitions.Client);
			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
		}

		void AssertCycleCountQueries(
			string queryString = null,
			string pickingArea = null,
			string putawayArea = null,
			string pickMethod = null)
		{
			var cycleCountQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.CycleCountJob)).ToUrlComponent();
			Assert("Cycle Count FormFlowType filter missing", queryString.Contains(cycleCountQuery));

			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
			AssertOptionalFilterExists(queryString, pickMethod, WhsLuceneProcessTaskDefinitions.PickMethod);
		}

		void AssertLoadTaskQueries(
			string queryString,
			string clientCode = null,
			string pickingArea = null,
			string putawayArea = null)
		{
			var loadQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.FormFlowType, WarehouseTaskFormFlowTypes.LoadJob)).ToUrlComponent();
			Assert("Load FormFlowType filter missing", queryString.Contains(loadQuery));

			AssertOptionalFilterExists(queryString, clientCode, WhsLuceneProcessTaskDefinitions.Client);
			AssertAreaFilterExists(queryString, pickingArea, putawayArea);
		}

		void AssertOptionalFilterExists(string queryString, string variable, string fieldName)
		{
			if (!string.IsNullOrEmpty(variable))
			{
				var equalQuery = new EqualQuery(new Term(fieldName, variable)).ToUrlComponent();
				Assert($"{fieldName} filter missing for form flow.", queryString.Contains(equalQuery));
			}
			else
			{
				Assert($"{fieldName} filter should not exist.", !queryString.Contains(fieldName));
			}
		}

		void AssertAreaFilterExists(string queryString, string pickingArea, string putawayArea)
		{
			if (!string.IsNullOrEmpty(pickingArea) && !string.IsNullOrEmpty(putawayArea))
			{
				var pickingQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.AreaName, pickingArea));
				var putawayQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.AreaName, putawayArea));
				var areaNameQuery = new BooleanQuery(BooleanOperator.Or, pickingQuery, putawayQuery).ToUrlComponent();
				Assert($"Area Name filter missing for form flow", queryString.Contains(areaNameQuery));
			}
			else if (!string.IsNullOrEmpty(pickingArea))
			{
				var pickingQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.AreaName, pickingArea)).ToUrlComponent();
				Assert($"Area Name filter missing for form flow", queryString.Contains(pickingQuery));
			}
			else if (!string.IsNullOrEmpty(putawayArea))
			{
				var putawayQuery = new EqualQuery(new Term(WhsLuceneProcessTaskDefinitions.AreaName, putawayArea)).ToUrlComponent();
				Assert($"Area Name filter missing for form flow", queryString.Contains(putawayQuery));
			}
			else
			{
				Assert($"Area Name filter should not exist.", !queryString.Contains(WhsLuceneProcessTaskDefinitions.AreaName));
			}
		}

		WhsRFRegistry CreateOptionalArgumentData(GlbStaff staff, WhsWarehouse warehouse, OrgHeader client, string pickingArea, string putawayArea)
		{
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			pickMethods.Add("TS1", (NoResString)"Test1", true);

			var pickGroupCollection = new PickGroupCollection();
			pickGroupCollection.Add(new PickGroup() { PickSequence = 5, Description = (NoResString)"ABC" });

			var registry = Helper.CreateWhsRFRegistry(staff, warehouse);

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				registry.WRR_OH_Client = client.PK;
				registry.WRR_UOMPackType = UOMPackTypesList.Codes.SplitCase;
				registry.WRR_PickGroupSequence = pickGroupCollection[0].PickSequence;
				registry.WRR_PickMethodCode = pickMethods[0].Code;

				if (!string.IsNullOrEmpty(pickingArea))
				{
					var area = Helper.CreateArea(warehouse, pickingArea);
					registry.WRR_WA_PickingArea = area.PK;
				}

				if (!string.IsNullOrEmpty(putawayArea))
				{
					var area = Helper.CreateArea(warehouse, putawayArea);
					registry.WRR_WA_PutawayArea = area.PK;
				}

				Factory.Save();
			}

			return registry;
		}

		#endregion
	}
}
