using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetAvailableWhsEquipmentParametersTest : WhsSecureServiceTestCase
	{
		#region TestGetAvailableWhsEquipmentParameters

		#region TestGetAvailableWhsEquipmentParameters_PickMethods

		public void TestGetAvailableWhsEquipmentParameters_PickMethods()
		{
			var webService = GetNewWebService();

			WarehouseDataRegistry.Instance.PickMethod.Value.Clear();
			AssertEquals(1, WarehouseDataRegistry.Instance.PickMethod.Value.Count);
			AssertEquals("ANY", WarehouseDataRegistry.Instance.PickMethod.Value[0].Code);
			webService.AllowedToRunServiceHasBeenCalled = false;
			var response1 = webService.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response1, webService);

			var responsePickMethods = response1.EquipmentParameters.PickMethods;
			AssertNotNull(responsePickMethods);
			AssertEquals(1, responsePickMethods.Count);
			AssertHasPickMethod(responsePickMethods, "ANY", "ANY", true);

			var newValue = new SystemDefinableCodeDescriptionBoolCollection();
			newValue.Add("TS1", (NoResString)"Test1", true);
			newValue.Add("TS2", (NoResString)"Test2", false);

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue))
			{
				webService.AllowedToRunServiceHasBeenCalled = false;
				var response2 = webService.GetAvailableWhsEquipmentParameters();
				AssertSuccessfulResponse(response2, webService);

				var responsePickMethods2 = response2.EquipmentParameters.PickMethods;
				AssertNotNull(responsePickMethods2);
				AssertEquals(3, responsePickMethods2.Count);
				AssertHasPickMethod(responsePickMethods2, "ANY", "ANY", false);
				AssertHasPickMethod(responsePickMethods2, "TS1", "Test1", true);
				AssertHasPickMethod(responsePickMethods2, "TS2", "Test2", false);
			}
		}

		public void TestGetAvailableWhsEquipmentParameters_PickMethods_UpdatesAnyDescToUpper()
		{
			AssertEquals("Precondition", 1, WarehouseDataRegistry.Instance.PickMethod.Value.Count);
			AssertEquals("Precondition", "ANY", WarehouseDataRegistry.Instance.PickMethod.Value[0].Code);
			AssertEquals("Precondition", "Any", WarehouseDataRegistry.Instance.PickMethod.Value[0].Description); // if this changes to upper case in registry, remove hack in GetAvailableWhsPickMethods

			var service = GetNewWebService();
			var response = service.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, service);

			var responsePickMethods = response.EquipmentParameters.PickMethods;
			AssertNotNull(responsePickMethods);
			AssertEquals(1, responsePickMethods.Count);
			AssertHasPickMethod(responsePickMethods, "ANY", "ANY", true);
		}

		void AssertHasPickMethod(WhsPickMethodInfoCollection methods, string code, string description, bool isDefault)
		{
			var methodFound = methods.Any(method => method.Code == code && method.Description == description && method.IsDefault == isDefault);
			Assert($"Pick Method not found: Code: {code} , Description: {description} , Default: {isDefault}", methodFound);
		}

		#endregion

		#region TestGetAvailableWhsEquipmentParameters_PickAreas

		public void TestGetAvailableWhsEquipmentParameters_PickAreas()
		{
			var warehouse = Helper.CreateWarehouse("TST");
			Helper.CreateArea(warehouse, "Area1");
			Helper.CreateArea(warehouse, "Area2");
			Helper.CreateArea(warehouse, "Area3");

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = warehouse.WW_WarehouseCode;
			var response = webService1.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, webService1);

			var responsePickAreas = response.EquipmentParameters.PickAreas;
			AssertNotNull(responsePickAreas);
			AssertEquals(6, responsePickAreas.Count);
			AssertHasArea(responsePickAreas, "ANY", "ANY");
			AssertHasArea(responsePickAreas, "DEFAULT", "DEFAULT");
			AssertHasArea(responsePickAreas, "[DEFAULT DOCK DOOR]", "[DEFAULT DOCK DOOR]");
			AssertHasArea(responsePickAreas, "Area1", "Area1");
			AssertHasArea(responsePickAreas, "Area2", "Area2");
			AssertHasArea(responsePickAreas, "Area3", "Area3");

			Helper.CreateArea(warehouse, "ANY", "ANY");
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse.WW_WarehouseCode;
			response = webService2.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, webService2);
			AssertNotNull(response);

			var responsePickAreas2 = response.EquipmentParameters.PickAreas;
			AssertEquals(6, responsePickAreas2.Count);
			AssertHasArea(responsePickAreas2, "ANY", "ANY");
			AssertHasArea(responsePickAreas2, "DEFAULT", "DEFAULT");
			AssertHasArea(responsePickAreas2, "[DEFAULT DOCK DOOR]", "[DEFAULT DOCK DOOR]");
			AssertHasArea(responsePickAreas2, "Area1", "Area1");
			AssertHasArea(responsePickAreas2, "Area2", "Area2");
			AssertHasArea(responsePickAreas2, "Area3", "Area3");
		}

		public void TestGetAvailableWhsEquipmentParameters_PickAreas_EmptyWarehouseCode()
		{
			var warehouse = Helper.CreateWarehouse("TST");
			Helper.CreateArea(warehouse, "Area1");
			Helper.CreateArea(warehouse, "Area2");
			Helper.CreateArea(warehouse, "Area3");

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = "";
			var response = webService.GetAvailableWhsEquipmentParameters();
			var responsePickAreas = response.EquipmentParameters.PickAreas;
			AssertNotNull(responsePickAreas);
			AssertEquals(1, responsePickAreas.Count);
			AssertHasArea(responsePickAreas, "ANY", "ANY");
		}

		public void TestGetAvailableWhsEquipmentParameters_PickAreas_OnlyPickingAreas()
		{
			var warehouse = Helper.CreateWarehouse("TST");
			Helper.CreateArea(warehouse, "Area1", AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			Helper.CreateArea(warehouse, "Area2", AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			Helper.CreateArea(warehouse, "Area3", AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = warehouse.WW_WarehouseCode;
			var response = webService.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, webService);

			var responsePickAreas = response.EquipmentParameters.PickAreas;
			AssertNotNull(responsePickAreas);
			AssertEquals(5, responsePickAreas.Count);
			AssertHasArea(responsePickAreas, "ANY", "ANY");
			AssertHasArea(responsePickAreas, "DEFAULT", "DEFAULT");
			AssertHasArea(responsePickAreas, "[DEFAULT DOCK DOOR]", "[DEFAULT DOCK DOOR]");
			AssertHasArea(responsePickAreas, "Area1", "Area1");
			AssertHasArea(responsePickAreas, "Area3", "Area3");
		}

		void AssertHasArea(WhsAreaInfoCollection areas, string name, string description)
		{
			bool areaFound = false;
			foreach (var area in areas)
			{
				if (area.Name.ToUpper() == name.ToUpper() && area.Description.ToUpper() == description.ToUpper())
				{
					areaFound = true;
					break;
				}
			}
			Assert($"Area not found: Name: {name}, Description: {description}", areaFound);
		}

		#endregion

		#region TestGetAvailableWhsEquipmentParameters_PickGroups

		public void TestGetAvailableWhsEquipmentParameters_PickGroups()
		{
			var pickGroupCollection = new PickGroupCollection();
			var pickGroup1 = pickGroupCollection.AddNew();
			pickGroup1.Description = (NoResString)"Desc1";

			var pickGroup2 = pickGroupCollection.AddNew();
			pickGroup2.Description = (NoResString)"Desc2";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				var webService = GetNewWebService();
				var response = webService.GetAvailableWhsEquipmentParameters();
				AssertSuccessfulResponse(response, webService);

				var responsePickGroups = response.EquipmentParameters.PickGroups;
				AssertNotNull(responsePickGroups);
				AssertEquals(3, responsePickGroups.Count);
				responsePickGroups.Single(o => o.PickSequence == 0 && o.Description == "ANY");
				responsePickGroups.Single(o => o.PickSequence == 1 && o.Description == "Desc1");
				responsePickGroups.Single(o => o.PickSequence == 2 && o.Description == "Desc2");
			}
		}

		#endregion

		#region TestGetAvailableWhsEquipmentParameters_Printers

		public void TestGetAvailableWhsEquipmentParameters_Printers()
		{
			var printer1 = Helper.CreatePrintQueue("1");
			var printer2 = Helper.CreatePrintQueue("2");
			var printer3 = Helper.CreatePrintQueue("3");
			printer3.SQ_AllowPrinting = false;
			((BusinessObject)printer2)[StmPrintQueueSchema.SQ_QueueDeleted] = ZDateTime.Now.AddYears(-1);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, webService);

			var responsePrinters = response.EquipmentParameters.Printers;
			AssertNotNull(responsePrinters);
			AssertContainsExactElementsInAnyOrder(new[] { Guid.Empty, printer1.PK }, responsePrinters.Select(p => p.PK));
		}

		public void TestGetAvailableWhsEquipmentParameters_Printers_NoPrinters()
		{
			var webService = GetNewWebService();
			PopulateSecurityHeader(webService);

			var response = webService.GetAvailableWhsEquipmentParameters();
			AssertSuccessfulResponse(response, webService);

			var responsePrinters = response.EquipmentParameters.Printers;
			AssertNotNull(responsePrinters);
			AssertEquals("With no Printers Available, the Printers collection should be empty.", 0, responsePrinters.Length);
		}

		#endregion

		#endregion
	}
}
