using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsPickFaceValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWF_ReplenishMinimum

		public void TestCheckWF_ReplenishMinimum()
		{
			TestMinDecimal(PickFace.WF_ReplenishMinimumInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestCheckWF_ReplenishMaximum

		public void TestCheckWF_ReplenishMaximum()
		{
			PickFace.WF_ReplenishMinimum = 10;
			TestMinDecimal(PickFace.WF_ReplenishMaximumInfo, ErrorCheckType.HasErrors, PickFace.WF_ReplenishMinimum + 1);

			PickFace.WF_ReplenishMinimum = 0;
			PickFace.WF_ReplenishMaximum = 0;
			AssertHasErrors(PickFace.WF_ReplenishMaximumInfo);

			PickFace.WF_ReplenishMinimum = 1;
			PickFace.WF_ReplenishMaximum = 1;
			AssertHasError(PickFace.WF_ReplenishMaximumInfo, "Please enter an amount greater than minimum.");
		}

		#endregion

		#region TestCheckWF_ReplenishMultiple

		public void TestCheckWF_ReplenishMultiple()
		{
			PickFace.WF_ReplenishMinimum = 1;
			PickFace.WF_ReplenishMaximum = 3;
			PickFace.WF_ReplenishmentMultiple = 0;
			AssertHasError(PickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple greater than or equal to 1.");

			PickFace.WF_ReplenishmentMultiple = -1;
			AssertHasError(PickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple greater than or equal to 1.");

			PickFace.WF_ReplenishmentMultiple = 1;
			AssertNoErrors(PickFace.WF_ReplenishmentMultipleInfo);

			PickFace.WF_ReplenishmentMultiple = 2;
			AssertNoErrors(PickFace.WF_ReplenishmentMultipleInfo);

			PickFace.WF_ReplenishmentMultiple = 3;
			AssertHasError(PickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple less than or equal to 2.");

			PickFace.WF_ReplenishmentMultiple = 4;
			AssertHasError(PickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple less than or equal to 2.");

			PickFace.WF_ReplenishMaximum = 0;
			PickFace.WF_ReplenishmentMultiple = 5;
			PickFace.WF_ReplenishmentMultiple = 4; // revalidate replenishment multiple
			AssertNoErrors("Since replenishment maximum is zero, there shouldn't be any errors", PickFace.WF_ReplenishmentMultipleInfo);

			PickFace.WF_ReplenishMaximum = 2;
			PickFace.WF_ReplenishMinimum = 5;
			PickFace.WF_ReplenishmentMultiple = 5;
			PickFace.WF_ReplenishmentMultiple = 4; // revalidate replenishment multiple
			AssertNoErrors("Since replenishment minimum is greater than replenishment maximum, there shouldn't be any errors", PickFace.WF_ReplenishmentMultipleInfo);

			PickFace.WF_ReplenishMaximum = 3;
			PickFace.WF_ReplenishMinimum = 3;
			PickFace.WF_ReplenishmentMultiple = 5;
			PickFace.WF_ReplenishmentMultiple = 4; // revalidate replenishment multiple
			AssertNoErrors("Since replenishment minimum is equal to replenishment maximum, there shouldn't be any errors", PickFace.WF_ReplenishmentMultipleInfo);
		}

		#endregion

		#region TestCheckWF_ReplenishMultiple_StockKeepingUnitDecimals

		public void TestCheckWF_ReplenishMultiple_StockKeepingUnitDecimals()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var product = Helper.CreateProduct(data.Org1, "O1", 0);
			var pickFace = Helper.CreateProductPickFace(product, data.Org1, data.Whs1.DefaultLocation);

			pickFace.WF_ReplenishMaximum = 3;
			pickFace.WF_ReplenishmentMultiple = 0;
			AssertHasError(pickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple greater than or equal to 1.");

			pickFace.WF_ReplenishmentMultiple = 1;
			AssertNoErrors(pickFace.WF_ReplenishmentMultipleInfo);

			product.OP_CountDecimalPlaces = 1;
			pickFace.WF_ReplenishmentMultiple = 0;
			AssertHasError(pickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple greater than or equal to 0.1.");

			pickFace.WF_ReplenishmentMultiple = 0.1;
			AssertNoErrors(pickFace.WF_ReplenishmentMultipleInfo);

			pickFace.WF_ReplenishmentMultiple = 4;
			AssertHasError(pickFace.WF_ReplenishmentMultipleInfo, "Please enter a Replenishment Multiple less than or equal to 3.");
		}

		#endregion

		#region Client

		public void TestCheckWF_OH_Client()
		{
			PickFace.Validation.ValidateWF_OH_Client();
			AssertHasErrors(PickFace.WF_OH_ClientInfo);

			PickFace.WF_OH_Client = ZGuid.NewZGuid();
			AssertNoErrors(PickFace.WF_OH_ClientInfo);
		}

		#endregion

		#region LocationString

		public void TestValidateLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var locationType = Helper.CreateLocationType("PFT", "PFT Test", false, 1, LocationClasses.Codes.FIX);
			row.Locations[0].WLV_WLT_LocationType = locationType.PK;

			Factory.Save();

			var pickface = Factory.New<WhsPickFace>();
			pickface.LocationWhsGuid = whs.PK;
			pickface.LocationString = "A";
			AssertEquals("Location should not have errors", false, pickface.LocationStringInfo.HasErrors());
			AssertEquals("WL should equal warehouse location", row.Locations[0].PK, pickface.WF_WL);
			pickface.LocationString = "";
			AssertEquals("Location should have errors", true, pickface.LocationStringInfo.HasErrors());
			AssertEquals("WF_WL should be empty", ZGuid.Empty, pickface.WF_WL);
		}

		public void TestCheckLocationString()
		{
			var client = Helper.CreateClient("ABC");
			var whs = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var part = Helper.CreateProduct(client, "P1");
			var locationType = Helper.CreateLocationType("PFT", "PFT Test", false, 1, LocationClasses.Codes.FIX);
			Array.ForEach(row.Locations.ToArray(), l => l.WLV_WLT_LocationType = locationType.PK);

			Factory.Save();

			var pickFace1 = Helper.CreateProductPickFace(part, client, whs, "A-1");
			pickFace1.Validation.ValidateLocationString();
			AssertNoErrors(pickFace1.LocationStringInfo);

			var pickFace2 = Helper.CreateProductPickFace(part, client, whs, "A-1");
			pickFace2.Validation.ValidateLocationString();
			AssertHasError(pickFace2.LocationStringInfo, "There is already a Pick Face with the same Client, Warehouse and Location.");

			pickFace2.LocationString = "A-2";
			AssertNoErrors(pickFace2.LocationStringInfo);
		}

		public void TestCheckLocationString_FixedLocation()
		{
			var client = Helper.CreateClient("ABC");
			var whs = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var part = Helper.CreateProduct(client, "P1");
			var locationType = Helper.CreateLocationType("PFT", "PFT Test", false, 1, LocationClasses.Codes.FIX);
			row.Locations[1].WLV_WLT_LocationType = locationType.PK;

			Factory.Save();

			var pickFace1 = Helper.CreateProductPickFace(part, client, whs, "A-1");
			pickFace1.Validation.ValidateLocationString();
			AssertNoErrors(pickFace1.LocationStringInfo);

			pickFace1.LocationString = "A-2";
			AssertNoErrors(pickFace1.LocationStringInfo);
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType

		public void TestCheckLocationString_CheckFixLocationMaxProductType()
		{
			PickFace.Delete();
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);
			var client2 = Helper.CreateClient("2", "Client2");
			whs2.LocationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			whs2.LocationType.WLT_MaximumNumberOfProducts = 1;
			Factory.Save();

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1, "A-1");
			AssertEquals("Precondition: ", data.Whs1.PK, pickFace1.LocationWhsGuid);
			AssertEquals("Precondition: ", "A-1", pickFace1.LocationString);

			var expectedMessage = "The number of products assigned to this fixed pick face location ({0}) exceeds the maximum allowable ({1})";
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, data.Whs1, "A-1");
			pickFace2.Validation.ValidateLocationString();
			AssertHasError("Cannot put product more than 1 product.", pickFace2.LocationStringInfo, string.Format(expectedMessage, 2, 1));

			var pickFace3 = Helper.CreateProductPickFace(data.Part1, client2, data.Whs1, "A-1");
			pickFace3.Validation.ValidateLocationString();
			AssertHasError("Cannot put product more than 1 product for diffrent client.", pickFace3.LocationStringInfo, string.Format(expectedMessage, 3, 1));

			pickFace1.LocationWhsGuid = whs2.PK;
			AssertEquals(whs2.PK, pickFace1.LocationWhsGuid);
			AssertEquals("When warehouse is changed Location should be cleared.", "", pickFace1.LocationString);
		}

		#endregion

		#region TestDynamicPickFace

		public void TestWF_OH_Client_And_WF_WL_CannotAlsoBeAssignedToDynamicPickArea()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A");
			var client = Helper.CreateClient("CL1");
			var product = Helper.CreateProduct("P1", client);
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var fixedPickFace = Helper.CreateProductPickFace(product, client, row.Locations[0]);
			var dynamicArea = Helper.CreateArea(warehouse, "A2", "DPF");

			fixedPickFace.Validation.ValidateAll();

			CombineAssertions("Parent is only assigned to valid fixed pick face.", () =>
			{
				AssertNoErrors(fixedPickFace.WF_OH_ClientInfo);
				AssertNoErrors(fixedPickFace.LocationWhsGuidInfo);
			});

			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(product.PK, client.PK, warehouse.PK, 10m, 4m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			fixedPickFace.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasError(fixedPickFace.WF_OH_ClientInfo, "Product is also assigned to a dynamic pick face area.");
				AssertHasError(fixedPickFace.LocationWhsGuidInfo, "Product is also assigned to a dynamic pick face area.");
			});
		}

		public void TestWF_OH_Client_And_WF_WL_CanAlsoBeAssignedToDynamicPickArea_WithSameClientButDifferentWarehouse()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse1, "A");
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			var client = Helper.CreateClient("CL1");
			var product = Helper.CreateProduct("P1", client);
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var fixedPickFace = Helper.CreateProductPickFace(product, client, row.Locations[0]);
			var dynamicArea = Helper.CreateArea(warehouse2, "A2", "DPF");

			fixedPickFace.Validation.ValidateAll();

			CombineAssertions("Parent is only assigned to valid fixed pick face", () =>
			{
				AssertNoErrors(fixedPickFace.WF_OH_ClientInfo);
				AssertNoErrors(fixedPickFace.LocationWhsGuidInfo);
			});

			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(product.PK, client.PK, warehouse2.PK, 10m, 4m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			fixedPickFace.Validation.ValidateAll();

			CombineAssertions("Dynamic Pick Face Area is located in different warehouse", () =>
			{
				AssertNoErrors(fixedPickFace.WF_OH_ClientInfo);
				AssertNoErrors(fixedPickFace.LocationWhsGuidInfo);
			});
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var validation = new TestWhsPickFaceValidation(PickFace);

			foreach (var propertyInfo in PickFace.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsPickFaceSchema.Constants.WF_WL)
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

		#region TestWhsCartonGroupSizeLinkValidation

		class TestWhsPickFaceValidation : WhsPickFaceValidation
		{
			public TestWhsPickFaceValidation(WhsPickFace parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		WhsPickFace PickFace => pickFace ?? (pickFace = Factory.New<WhsPickFace>());
		WhsPickFace pickFace;

		#endregion
	}
}
