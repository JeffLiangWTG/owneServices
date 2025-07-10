using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsVASOrderValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWVO_CustomerReferenceNo_IsUniquePerClient

		public void TestCheckWVO_CustomerReferenceNo_IsUniquePerClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder1.WVO_CustomerReferenceNo = "ABC";
			Factory.Save();

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertNoErrors("Precondition", vasOrder2.WVO_CustomerReferenceNoInfo);

			vasOrder2.WVO_CustomerReferenceNo = "ABC";
			AssertHasError(vasOrder2.WVO_CustomerReferenceNoInfo, "Customer Reference Number must be unique per Client.");

			vasOrder2.WVO_CustomerReferenceNo = "XYZ";
			AssertNoErrors(vasOrder2.WVO_CustomerReferenceNoInfo);

			vasOrder2.WVO_CustomerReferenceNo = "ABC";
			AssertHasError(vasOrder2.WVO_CustomerReferenceNoInfo, "Customer Reference Number must be unique per Client.");

			var client2 = Helper.CreateClient();
			vasOrder2.WVO_OH_Client = client2.PK;
			AssertNoErrors(vasOrder2.WVO_CustomerReferenceNoInfo);
		}

		#endregion

		#region TestCheckWVO_CustomerReferenceNo_IsUniquePerClient_WithDifferentMainAddress

		public void TestCheckWVO_CustomerReferenceNo_IsUniquePerClient_WithDifferentMainAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder1.WVO_CustomerReferenceNo = "ABC";
			Factory.Save();

			data.Org1.Addresses.AddNewMainAddress();
			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder2.WVO_CustomerReferenceNo = "ABC";

			AssertEquals("Precondition", vasOrder2.WVO_OH_Client, vasOrder1.WVO_OH_Client);
			AssertHasError(vasOrder2.WVO_CustomerReferenceNoInfo, "Customer Reference Number must be unique per Client.");
		}

		#endregion

		#region TestCheckWVO_CustomerReferenceNoIsNotEmpty

		public void TestCheckWVO_CustomerReferenceNoIsNotEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertNoErrors("Precondition", vasOrder.WVO_CustomerReferenceNoInfo);

			vasOrder.Validation.ValidateWVO_CustomerReferenceNo();
			AssertNoErrors("Should not have an Error if the Job is not in the Database.", vasOrder.WVO_CustomerReferenceNoInfo);

			Factory.Save();
			AssertEquals("Precondition: Customer Reference is populated on Save.", false, vasOrder.WVO_CustomerReferenceNo.IsEmpty);
			AssertNoErrors("Precondition", vasOrder.WVO_CustomerReferenceNoInfo);

			vasOrder.WVO_CustomerReferenceNo = "";
			AssertHasError(vasOrder.WVO_CustomerReferenceNoInfo, "Please enter a Customer Reference Number.");
		}

		#endregion

		#region TestCheckWVO_WA_ServiceArea

		public void TestCheckWVO_WA_ServiceArea()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var areaWithLocations = Helper.CreateArea(warehouse, "Locations");
			var areaWithNoLocations = Helper.CreateArea(warehouse, "None");
			var areaWithOnlyPickingLocations = Helper.CreateArea(warehouse, "Pick");
			var row = Helper.CreateRow(warehouse, "AA", 3, 1);
			Factory.Save();

			row.Locations[0].WLV_WA_PickingArea = areaWithOnlyPickingLocations.PK;

			row.Locations[1].WLV_WA_PutawayArea = areaWithLocations.PK;
			row.Locations[2].WLV_WA_PutawayArea = areaWithLocations.PK;

			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNoErrors("Precondition:", vasOrder.WVO_WA_ServiceAreaInfo);

			vasOrder.WVO_WA_ServiceArea = areaWithNoLocations.PK;
			AssertHasError(vasOrder.WVO_WA_ServiceAreaInfo, "Service Area should have at least one Location.");

			vasOrder.WVO_WA_ServiceArea = areaWithOnlyPickingLocations.PK;
			AssertHasError(vasOrder.WVO_WA_ServiceAreaInfo, "Service Area should have at least one Location.");

			vasOrder.WVO_WA_ServiceArea = areaWithLocations.PK;
			AssertNoErrors(vasOrder.WVO_WA_ServiceAreaInfo);
		}

		#endregion

		#region TestCheckWVO_WA_ServiceArea_IsFreeStore

		public void TestCheckWVO_WA_ServiceArea_IsFreeStore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var area = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			area.WA_AreaType = AreaTypes.Codes.Bonded;

			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNoErrors("Precondition:", vasOrder.WVO_WA_ServiceAreaInfo);

			vasOrder.WVO_WA_ServiceArea = area.PK;
			AssertHasError(vasOrder.WVO_WA_ServiceAreaInfo, "Service Area should be a Free Store Area.");

			area.WA_AreaType = AreaTypes.Codes.FreeStore;
			vasOrder.Validation.ValidateWVO_WA_ServiceArea();
			AssertNoErrors(vasOrder.WVO_WA_ServiceAreaInfo);
		}

		#endregion

		#region TestValidateWarehousePK

		public void TestValidateWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNoErrors("Precondition:", vasOrder.WarehousePKInfo);

			vasOrder.WarehousePK = ZGuid.Empty;
			AssertHasError(vasOrder.WarehousePKInfo, "Please enter a Warehouse.");

			vasOrder.WarehousePK = data.Whs1.PK;
			AssertNoErrors(vasOrder.WarehousePKInfo);

			vasOrder.WarehousePK = ZGuid.NewZGuid();
			AssertHasError(vasOrder.WarehousePKInfo, "Enter a valid Warehouse.");

			vasOrder.WarehousePK = data.Whs1.PK;
			AssertNoErrors(vasOrder.WarehousePKInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNoErrors(vasOrder.WVO_OH_ClientInfo);
			AssertNoErrors(vasOrder.WarehousePKInfo);

			vasOrder.WVO_OH_Client = ZGuid.Invalid;
			vasOrder.Validation.ValidateAll();
			AssertHasError(vasOrder.WVO_OH_ClientInfo, "Enter a valid Client.");
			AssertHasError(vasOrder.WarehousePKInfo, "Please enter a Warehouse.");
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var validation = new TestWhsVASOrderValidation(vasOrder);

			foreach (var propertyInfo in vasOrder.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (propertyInfo.Name == WhsVASOrderSchema.Constants.WVO_WA_ServiceArea)
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

		#region TestWhsVASOrderValidation

		class TestWhsVASOrderValidation : WhsVASOrderValidation
		{
			public TestWhsVASOrderValidation(WhsVASOrder parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
