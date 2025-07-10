using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStocktakeValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestValidateWS_ABCAnalysisCategory

		public void TestValidateWS_ABCAnalysisCategory()
		{
			AssertNoErrors("Precondition", Stocktake.WS_ABCAnalysisCategoryInfo);

			Stocktake.WS_ABCAnalysisCategory = "XXX";
			AssertHasWarning(Stocktake.WS_ABCAnalysisCategoryInfo, "You have not entered a valid code.");
			AssertNoErrors(Stocktake.WS_ABCAnalysisCategoryInfo);

			Stocktake.WS_ABCAnalysisCategory = "A";
			AssertNoErrors(Stocktake.WS_ABCAnalysisCategoryInfo);
			AssertNoWarnings(Stocktake.WS_ABCAnalysisCategoryInfo);

			Stocktake.WS_ABCAnalysisCategory = "B";
			AssertNoErrors(Stocktake.WS_ABCAnalysisCategoryInfo);
			AssertNoWarnings(Stocktake.WS_ABCAnalysisCategoryInfo);

			Stocktake.WS_ABCAnalysisCategory = "A B";
			AssertNoErrors(Stocktake.WS_ABCAnalysisCategoryInfo);
			AssertHasWarning(Stocktake.WS_ABCAnalysisCategoryInfo, "You have not entered a valid code.");
		}

		#endregion

		#region TestCheckWS_StocktakeStatus

		public virtual void TestCheckWS_StocktakeStatus()
		{
			TestCodePairList(Stocktake.WS_StocktakeStatusInfo, ErrorCheckType.HasErrors, false, new StocktakeStatus());
		}

		#endregion

		#region TestValidateWS_CountEmptyLocationsCategory

		public void TestValidateWS_CountEmptyLocationsCategory()
		{
			AssertNoErrors("Precondition", Stocktake.WS_CountEmptyLocationsCategoryInfo);

			Stocktake.WS_CountEmptyLocationsCategory = "XXX";
			AssertHasError(Stocktake.WS_CountEmptyLocationsCategoryInfo, "Enter a valid selection.");

			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations;
			AssertNoErrors(Stocktake.WS_CountEmptyLocationsCategoryInfo);

			Stocktake.WS_OH_Client = Helper.CreateClient().PK;
			AssertCountEmptyLocationsCategoryHasError(Stocktake.WS_OH_ClientInfo.HumanReadableName);
			Stocktake.WS_OH_Client = ZGuid.Empty;

			Stocktake.ProductFilterCollection.AddNew();
			AssertCountEmptyLocationsCategoryHasError("Product");
			Stocktake.ProductFilterCollection.DeleteAll();

			Stocktake.WS_RH_NKCommodityCode = "A";
			AssertCountEmptyLocationsCategoryHasError(Stocktake.WS_RH_NKCommodityCodeInfo.HumanReadableName);
			Stocktake.WS_RH_NKCommodityCode = string.Empty;

			Stocktake.WS_ABCAnalysisCategory = "B";
			AssertCountEmptyLocationsCategoryHasError(Stocktake.WS_ABCAnalysisCategoryInfo.HumanReadableName);
			Stocktake.WS_ABCAnalysisCategory = string.Empty;

			Stocktake.WS_StocktakeCycle = "C";
			AssertCountEmptyLocationsCategoryHasError(Stocktake.WS_StocktakeCycleInfo.HumanReadableName);
			Stocktake.WS_StocktakeCycle = string.Empty;

			Stocktake.WS_StocktakeType = "E";
			AssertCountEmptyLocationsCategoryHasError(Stocktake.WS_StocktakeTypeInfo.HumanReadableName);
			Stocktake.WS_StocktakeType = "STD";
		}

		void AssertCountEmptyLocationsCategoryHasError(string propName)
		{
			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;
			AssertHasError(Stocktake.WS_CountEmptyLocationsCategoryInfo, propName + " " + WhsStocktakeValidation.CannotSelectWhenCountEmptyLocations);

			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations;
			AssertHasError(Stocktake.WS_CountEmptyLocationsCategoryInfo, propName + " " + WhsStocktakeValidation.CannotSelectWhenCountEmptyLocations);

			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.ExcludeEmptyLocations;
			AssertNoErrors(Stocktake.WS_CountEmptyLocationsCategoryInfo);
		}

		#endregion

		#region TestCheckWS_RH_NKCommodityCode

		public void TestCheckWS_RH_NKCommodityCode()
		{
			Stocktake.WS_RH_NKCommodityCode = "";
			AssertNoErrors(Stocktake.WS_RH_NKCommodityCodeInfo);

			Stocktake.WS_RH_NKCommodityCode = "A";
			AssertHasError(Stocktake.WS_RH_NKCommodityCodeInfo, "Enter a valid " + Stocktake.WS_RH_NKCommodityCodeInfo.Description + ".");

			RefCommodityCode commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "C";
			Factory.Save();

			Stocktake.WS_RH_NKCommodityCode = "C";
			AssertNoErrors(Stocktake.WS_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region TestCheckWS_StocktakeCycle

		public void TestCheckWS_StocktakeCycle()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("B", "B");
			WarehouseDataRegistry.Instance.StocktakeCycle.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, result);
			Stocktake.WS_StocktakeCycle = "B";
			AssertNoErrors(Stocktake.WS_StocktakeCycleInfo);

			Stocktake.WS_StocktakeCycle = "";
			AssertNoErrors(Stocktake.WS_StocktakeCycleInfo);

			Stocktake.WS_StocktakeCycle = "A";
			AssertHasError(Stocktake.WS_StocktakeCycleInfo, "Enter a valid " + Stocktake.WS_StocktakeCycleInfo.Description + ".");
		}

		#endregion

		#region TestCheckWS_WW_Whs

		public void TestCheckWS_WW_Whs()
		{
			Stocktake.WS_WW_Whs = ZGuid.Empty;
			AssertHasError(Stocktake.WS_WW_WhsInfo, "Please enter a Warehouse.");

			Stocktake.WS_WW_Whs = Helper.CreateWarehouse("A").PK;
			AssertNoErrors(Stocktake.WS_WW_WhsInfo);

			Stocktake.Warehouse.WW_IsActive = false;
			Stocktake.Validation.ValidateWS_WW_Whs();
			AssertHasError(Stocktake.WS_WW_WhsInfo, WhsStocktakeValidation.CannotSelectInactiveWarehouse);
		}

		#endregion

		#region TestCheckWS_OH_Client

		public void TestCheckWS_OH_Client()
		{
			Stocktake.WS_OH_Client = ZGuid.Empty;
			AssertNoErrors("Client is not mandatory anymore", Stocktake.WS_OH_ClientInfo);

			Stocktake.WS_OH_Client = Helper.CreateClient().PK;
			AssertNoErrors(Stocktake.WS_OH_ClientInfo);

			Stocktake.Client.OH_IsActive = false;
			Stocktake.Validation.ValidateWS_OH_Client();
			AssertHasError(Stocktake.WS_OH_ClientInfo, WhsStocktakeValidation.CannotSelectInactiveClient);
		}

		#endregion

		#region TestValidateWS_CountEmptyLocationsCategorySave

		public void TestValidateWS_CountEmptyLocationsCategorySave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			AssertNoExceptionThrown("Precondition", () => Factory.Save());

			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;
			AssertNoExceptionThrown(() => Factory.Save());
			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.ExcludeEmptyLocations;
			AssertNoExceptionThrown(() => Factory.Save());
			Stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations;
			AssertNoExceptionThrown(() => Factory.Save());
			Stocktake.WS_CountEmptyLocationsCategory = "AAA";
			AssertExceptionThrown(typeof(ZSaveException), () => Factory.Save());
		}

		#endregion

		#region TestValidateLocationString

		public void TestValidateLocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var differentWarehouse = Helper.CreateWarehouse("New Whs");
			Factory.Save();

			// Stocktake doesn't have a warehouse specified.

			Stocktake.LocationString = "";
			AssertNoErrors("Location should not have any errors", Stocktake.LocationStringInfo);
			AssertEquals("WS_WL_Location should be empty", true, Stocktake.WS_WL_Location.IsEmpty);

			Stocktake.LocationString = "A";
			AssertHasError(Stocktake.LocationStringInfo, "A valid warehouse has not been selected for this Stocktake.");
			AssertEquals("WS_WL_Location should be empty", true, Stocktake.WS_WL_Location.IsEmpty);

			// Stocktake with a warehouse specified.

			Stocktake.WS_WW_Whs = data.Whs1.PK;

			Stocktake.LocationString = "";
			AssertNoErrors("Since Location is not mandatory, there should not be any errors.", Stocktake.LocationStringInfo);
			AssertEquals("WL should be empty", true, Stocktake.WS_WL_Location.IsEmpty);

			Stocktake.LocationString = "A";
			AssertNoErrors("Since Location is valid, there should not be any errors.", Stocktake.LocationStringInfo);
			AssertEquals("WS_WL_Location should be the corresponding location guid.", data.Whs1.FindLocation("A").PK, Stocktake.WS_WL_Location);

			Stocktake.LocationString = "A-2";
			AssertHasError(Stocktake.LocationStringInfo, "Row A only has 1 Column so the Column must be 1.");
			AssertEquals("WS_WL_Location should be empty.", true, Stocktake.WS_WL_Location.IsEmpty);

			Stocktake.LocationString = "AAA";
			AssertHasError(Stocktake.LocationStringInfo, "Please enter a valid Location Row.");
			AssertEquals("WS_WL_Location should be empty.", true, Stocktake.WS_WL_Location.IsEmpty);

			Stocktake.LocationString = "A";
			AssertNoErrors("Since Location is valid, there should not be any errors.", Stocktake.LocationStringInfo);

			Stocktake.WS_WW_Whs = differentWarehouse.PK;
			Stocktake.Validation.ValidateLocationString();
			AssertEquals("", Stocktake.LocationString);
			AssertNoErrors("Since warehouse is changed now. Location should be empty and it should not have any errors.", Stocktake.LocationStringInfo);
		}

		#endregion

		#region TestValidateLocationString_DockDoorLocation

		public void TestValidateLocationString_DockDoorLocation()
		{
			var defaultDDLType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var customDDLType = Helper.CreateLocationType("DO2", LocationClasses.Codes.DDL);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoor1 = data.Whs1.DefaultOutboundDockDoorLocation;
			var dockDoor2 = data.Whs1.FindLocation("A-2");
			var dockDoor3 = data.Whs1.FindLocation("A-3");
			dockDoor2.WLV_WLT_LocationType = defaultDDLType.PK;
			dockDoor3.WLV_WLT_LocationType = customDDLType.PK;
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			AssertNoErrors("Precondition.", stocktake.WS_WL_LocationInfo);

			stocktake.WS_WL_Location = dockDoor1.PK;
			stocktake.Validation.ValidateLocationString();
			AssertHasError(stocktake.LocationStringInfo, "You cannot stocktake a Dock Door Location.");

			stocktake.WS_WL_Location = dockDoor2.PK;
			stocktake.Validation.ValidateLocationString();
			AssertHasError(stocktake.LocationStringInfo, "You cannot stocktake a Dock Door Location.");

			stocktake.WS_WL_Location = dockDoor3.PK;
			stocktake.Validation.ValidateLocationString();
			AssertHasError(stocktake.LocationStringInfo, "You cannot stocktake a Dock Door Location.");

			stocktake.WS_WL_Location = data.Whs1.DefaultLocation.PK;
			stocktake.Validation.ValidateLocationString();
			AssertNoErrors("Should be no errors.", stocktake.WS_WL_LocationInfo);
		}

		#endregion

		#region TestStocktakeType

		public void TestStocktakeType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup test data.

			var unloadedStocktakeWithoutStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "", StocktakeStatus.Codes.New);
			var unLoadedStocktakeWithStandardStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "STD", StocktakeStatus.Codes.New);
			var unLoadedStocktakeWithATCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "ATC", StocktakeStatus.Codes.New);
			var unLoadedStocktakeWithAZCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AZC", StocktakeStatus.Codes.New);
			var unLoadedStocktakeWithInvalidStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AAA", StocktakeStatus.Codes.New);

			var loadedStocktakeWithStandardStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "STD", StocktakeStatus.Codes.Loaded);
			var loadedStocktakeWithATCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "ATC", StocktakeStatus.Codes.Loaded);
			var loadedStocktakeWithAZCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AZC", StocktakeStatus.Codes.Loaded);
			var loadedStocktakeWithInvalidStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AAA", StocktakeStatus.Codes.Loaded);// for example a deleted type from the registry

			var finalisedStocktakeWithStandardStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "STD", StocktakeStatus.Codes.Finalised);
			var finalisedStocktakeWithATCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "ATC", StocktakeStatus.Codes.Finalised);
			var finalisedStocktakeWithAZCStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AZC", StocktakeStatus.Codes.Finalised);
			var finalisedStocktakeWithInvalidStocktakeType = Helper.CreateWhsStocktake(data.Org1, data.Whs1, "AAA", StocktakeStatus.Codes.Finalised);// for example a deleted type from the registry

			AssertHasError(unloadedStocktakeWithoutStocktakeType.WS_StocktakeTypeInfo, "Please enter a " + unloadedStocktakeWithoutStocktakeType.WS_StocktakeTypeInfo.HumanReadableName + ".");
			AssertNoErrors(unLoadedStocktakeWithStandardStocktakeType.WS_StocktakeTypeInfo);
			AssertHasError(unLoadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo, "Enter a valid " + unLoadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo.HumanReadableName + ".");
			AssertHasError(unLoadedStocktakeWithAZCStocktakeType.WS_StocktakeTypeInfo, "Enter a valid " + unLoadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo.HumanReadableName + ".");
			AssertHasError(unLoadedStocktakeWithInvalidStocktakeType.WS_StocktakeTypeInfo, "Enter a valid " + unLoadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo.HumanReadableName + ".");

			AssertNoErrors(loadedStocktakeWithStandardStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(loadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(loadedStocktakeWithAZCStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(loadedStocktakeWithInvalidStocktakeType.WS_StocktakeTypeInfo);

			AssertNoErrors(finalisedStocktakeWithStandardStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(finalisedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(finalisedStocktakeWithAZCStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(finalisedStocktakeWithInvalidStocktakeType.WS_StocktakeTypeInfo);
		}

		public void TestStocktakeTypeWithAutomaticStocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup test data.

			var unLoadedStocktakeWithATCStocktakeType = Factory.New<WhsStocktake>();
			var unLoadedStocktakeWithAZCStocktakeType = Factory.New<WhsStocktake>();

			unLoadedStocktakeWithATCStocktakeType.IsAutoCreatingStocktake = true;
			unLoadedStocktakeWithAZCStocktakeType.IsAutoCreatingStocktake = true;
			unLoadedStocktakeWithATCStocktakeType.WS_StocktakeStatus = StocktakeStatus.Codes.New;
			unLoadedStocktakeWithAZCStocktakeType.WS_StocktakeStatus = StocktakeStatus.Codes.New;
			unLoadedStocktakeWithATCStocktakeType.WS_StocktakeType = "ATC";
			unLoadedStocktakeWithAZCStocktakeType.WS_StocktakeType = "AZC";

			// Test stocktake type validation.

			AssertNoErrors(unLoadedStocktakeWithATCStocktakeType.WS_StocktakeTypeInfo);
			AssertNoErrors(unLoadedStocktakeWithAZCStocktakeType.WS_StocktakeTypeInfo);
		}

		#endregion

		#region Test Validate All

		public void TestValidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var duplicatedManuallyLoadedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var uniqueLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Held);

			AssertNoErrors(openLine);
			AssertNoErrors(duplicatedManuallyLoadedLine);
			AssertNoErrors(manuallyAddedLine);
			AssertNoErrors(manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine);
			AssertNoErrors(uniqueLine);

			stocktake.Validation.ValidateAll();

			AssertNoRowErrors(openLine);
			AssertHasRowError(duplicatedManuallyLoadedLine, "Product P1 in Location A with status AVL is already on this stocktake on Line 1. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertHasRowError(manuallyAddedLine, "Product P2 in Location A with status AVL is already on this stocktake on Line 4. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertHasRowError(manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine, "Product P2 in Location A with status AVL is already on this stocktake on Line 3. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertNoRowErrors(uniqueLine);
		}

		public void TestValidateAll_LocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var differentWarehouse = Helper.CreateWarehouse("New Whs");
			Factory.Save();

			// Stocktake with a warehouse specified.

			Stocktake.WS_WW_Whs = data.Whs1.PK;

			// Valid location.

			Stocktake.LocationString = "A";
			Stocktake.Validation.ValidateAll();
			AssertNoErrors("Since Location is valid, there should not be any errors.", Stocktake.LocationStringInfo);
			AssertEquals("WS_WL_Location should be the corresponding location guid.", data.Whs1.FindLocation("A").PK, Stocktake.WS_WL_Location);

			// Change warehouse.

			Stocktake.WS_WW_Whs = differentWarehouse.PK;
			AssertNoErrors("Since location validation is not triggered, there are no errors.", Stocktake.LocationStringInfo);

			Stocktake.Validation.ValidateAll();
			AssertEquals("since warehouse is changed it should reset location string.", "", Stocktake.LocationString);
			AssertNoErrors("Since Location is empty, there should not be errors.", Stocktake.LocationStringInfo);

			Stocktake.LocationString = "A";
			Stocktake.Validation.ValidateAll();
			AssertHasError("Since warehouse is changed now, there should be errors.", Stocktake.LocationStringInfo, "Please enter a valid Location Row.");
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var validation = new TestWhsStocktakeValidation(Stocktake);

			var list = new string[]
			{
				WhsStocktakeSchema.Constants.WS_WR_Row,
				WhsStocktakeSchema.Constants.WS_WA_Area,
				WhsStocktakeSchema.Constants.WS_WL_Location
			};

			foreach (var propertyInfo in Stocktake.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsStocktakeValidation

		class TestWhsStocktakeValidation : WhsStocktakeValidation
		{
			public TestWhsStocktakeValidation(WhsStocktake parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Stocktake = Factory.New<WhsStocktake>();
		}

		WhsStocktake Stocktake;

		#endregion
	}
}
