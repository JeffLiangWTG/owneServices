using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetJobServiceTypesTest : WhsSecureServiceTestCase
	{
		#region TestGetJobServiceTypes

		[TestDate(2015, 1, 1)]
		public void TestGetJobServiceTypes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var otherClient = Helper.CreateClient("C2");
			var otherWarehouse = Helper.CreateWarehouse("OWH");
			Helper.Factory.Save();

			SetupJobServiceTypes(Helper.Factory);
			SetupClientRates(Helper.Factory, data.Whs1, otherWarehouse, data.Org1, otherClient);

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetWarehouseJobServices()))
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetGenericJobServicesUnderFreight()))
			{
				var addedService = order1.Services.AddNew();
				addedService.ES_ServiceCode = "WH1";
				addedService.ES_ServiceCount = 5;
				addedService.ES_ServiceNote = "ABC";
				Helper.Factory.Save();

				// Whs1 + Client 1 -  Expect WH1, WH2, OTH
				var webService1 = GetNewWebService(data.Whs1);
				var response1 = webService1.GetJobServiceTypes(order1.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, data.Org1.OH_Code);
				AssertSuccessfulResponse(response1, webService1);

				var jobServiceTypes1 = response1.JobServiceTypes;
				CombineAssertions(() =>
				{
					AssertNotNull("Precondition.", jobServiceTypes1);
					AssertEquals("Should be 3 ServiceTypes for this Client + Whs combination.", 3, jobServiceTypes1.Count);
				});

				var wh1_ServiceType = jobServiceTypes1.SingleOrDefault(c => c.Code == "WH1" && c.Description == "Client 1 Warehouse 1");
				CombineAssertions(() =>
				{
					AssertNotNull("Should contain Warehouse Job Service 1 Type.", wh1_ServiceType);
					AssertNotNull("Precondition.", wh1_ServiceType.ExistingJobService);
					AssertEquals("ABC", wh1_ServiceType.ExistingJobService.ServiceNote);
					AssertEquals(5m, wh1_ServiceType.ExistingJobService.Count);
					AssertNotNull("Should contain Client 1 All Warehouses Type.", jobServiceTypes1.SingleOrDefault(c => c.Code == "WH2" && c.Description == "Client 1 All Warehouses"));
					AssertNotNull("Should contain Other Service Client 1 All Warehouses Type.", jobServiceTypes1.SingleOrDefault(c => c.Code == "OTH" && c.Description == "Other Service Client 1 All Warehouses"));
				});

				var webService2 = GetNewWebService(data.Whs1);
				var response_Wh1_OtherOrder = webService2.GetJobServiceTypes(order2.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, data.Org1.OH_Code);
				AssertSuccessfulResponse(response_Wh1_OtherOrder, webService2);

				var jobServiceTypes2 = response_Wh1_OtherOrder.JobServiceTypes;
				CombineAssertions(() =>
				{
					AssertNotNull("Precondition.", jobServiceTypes2);
					AssertEquals("Should be 3 ServiceTypes for this Client + Whs combination.", 3, jobServiceTypes2.Count);
				});

				var wh1_OtherOrder = jobServiceTypes2.SingleOrDefault(c => c.Code == "WH1" && c.Description == "Client 1 Warehouse 1");
				CombineAssertions(() =>
				{
					AssertNotNull("Should contain Client 1 Warehouse 1 Type.", wh1_OtherOrder);
					AssertNull("Should not find an existing Job Service.", wh1_OtherOrder.ExistingJobService);
					AssertNotNull("Should contain Client 1 All Warehouses Type.", jobServiceTypes2.SingleOrDefault(c => c.Code == "WH2" && c.Description == "Client 1 All Warehouses"));
					AssertNotNull("Should contain Other Job Service 1 Type.", jobServiceTypes2.SingleOrDefault(c => c.Code == "OTH" && c.Description == "Other Service Client 1 All Warehouses"));
				});
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGetJobServiceTypes_NoResults()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var otherClient = Helper.CreateClient("C2");
			var otherWarehouse = Helper.CreateWarehouse("OWH");

			Helper.Factory.Save();

			SetupJobServiceTypes(Helper.Factory);
			SetupClientRates(Helper.Factory, data.Whs1, otherWarehouse, data.Org1, otherClient);

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetWarehouseJobServices()))
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetGenericJobServicesUnderFreight()))
			{
				// Whs1 + Client 2 - Expect None
				var webService = GetNewWebService(data.Whs1);

				var response = webService.GetJobServiceTypes(order1.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, otherClient.OH_Code);
				AssertSuccessfulResponse(response, webService);

				var jobServiceTypes = response.JobServiceTypes;
				AssertNotNull("Precondition.", jobServiceTypes);
				AssertEquals("Should be no ServiceTypes for this Client + Whs combination.", false, jobServiceTypes.Any());
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGetJobServiceTypes_PickLineStrategy()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var otherClient = Helper.CreateClient("C2");
			var otherWarehouse = Helper.CreateWarehouse("OWH");

			Helper.Factory.Save();

			SetupJobServiceTypes(Helper.Factory);
			SetupClientRates(Helper.Factory, data.Whs1, otherWarehouse, data.Org1, otherClient);

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetWarehouseJobServices()))
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetGenericJobServicesUnderFreight()))
			{
				// Test Other JobSerivceSupportStrategy - PL1
				var receiveForPick = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
				var inventory = Helper.CreateWhsReceiveInventoryLine(receiveForPick, data.Part1, 10m, data.Whs1.DefaultLocation);
				receiveForPick.FinaliseDocketWithoutUserConfirmation();
				Helper.Factory.Save();
				Helper.CreatePickNew(order1);
				var pickLine = order1.Lines.Single().PickLines.Single();

				var webService = GetNewWebService(data.Whs1);
				var response = webService.GetJobServiceTypes(pickLine.PK.ToGuid(), JobServiceSupporterStrategy.WhsPickLine, data.Org1.OH_Code);
				AssertSuccessfulResponse(response, webService);

				var jobServiceTypes = response.JobServiceTypes;
				CombineAssertions(() =>
				{
					AssertNotNull("Precondition.", jobServiceTypes);
					AssertEquals("Should be 1 Service Type for this warehouse + client", 1, jobServiceTypes.Count);
					AssertNotNull("Should contain Client 1 Warehouse 1 PICKLNE Type.", jobServiceTypes.SingleOrDefault(c => c.Code == "PL1" && c.Description == "Client 1 Warehouse 1 PICKLINE"));
				});
			}
		}

		public void TestGetJobServiceTypes_OtherWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var otherClient = Helper.CreateClient("C2");
			var otherWarehouse = Helper.CreateWarehouse("OWH");

			Helper.Factory.Save();

			SetupJobServiceTypes(Helper.Factory);
			SetupClientRates(Helper.Factory, data.Whs1, otherWarehouse, data.Org1, otherClient);

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetWarehouseJobServices()))
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetGenericJobServicesUnderFreight()))
			{
				// Whs2 + Client 1 - Expect WH3, WH2, OTH
				var webService = GetNewWebService(otherWarehouse);
				var response = webService.GetJobServiceTypes(order1.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, data.Org1.OH_Code);
				AssertSuccessfulResponse(response, webService);

				var jobServiceTypes = response.JobServiceTypes;
				CombineAssertions(() =>
				{
					AssertNotNull("Precondition.", jobServiceTypes);
					AssertNotNull("Should contain Client 1 All Warehouses Type.", jobServiceTypes.SingleOrDefault(c => c.Code == "WH2" && c.Description == "Client 1 All Warehouses"));
					AssertNotNull("Should contain Warehouse Client 1 Warehouse 2 Type.", jobServiceTypes.SingleOrDefault(c => c.Code == "WH3" && c.Description == "Client 1 Warehouse 2"));
					AssertNotNull("Should contain Other Service Client 1 All Warehouses Type.", jobServiceTypes.SingleOrDefault(c => c.Code == "OTH" && c.Description == "Other Service Client 1 All Warehouses"));
				});
			}
		}

		public void TestGetJobServiceTypes_DifferentWarehouseAndClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var otherClient = Helper.CreateClient("C2");
			var otherWarehouse = Helper.CreateWarehouse("OWH");

			Helper.Factory.Save();

			SetupJobServiceTypes(Helper.Factory);
			SetupClientRates(Helper.Factory, data.Whs1, otherWarehouse, data.Org1, otherClient);

			using (WarehouseDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetWarehouseJobServices()))
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetGenericJobServicesUnderFreight()))
			{
				Helper.Factory.Save();

				// Whs2 + Client 2 - Expect WH4
				var webService = GetNewWebService(otherWarehouse);
				var response = webService.GetJobServiceTypes(order1.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, otherClient.OH_Code);
				AssertSuccessfulResponse(response, webService);

				var jobServiceTypes = response.JobServiceTypes;
				AssertNotNull("Precondition.", jobServiceTypes);
				AssertEquals("Should be 1 ServiceTypes for this Client + Whs combination.", 1, jobServiceTypes.Count);
				AssertNotNull("Should contain Client 2 Warehouse 2 Type.", jobServiceTypes.SingleOrDefault(c => c.Code == "WH4" && c.Description == "Client 2 Warehouse 2"));
			}
		}

		#endregion

		#region Implementation

		static void SetupJobServiceTypes(BusinessObjectFactory factory)
		{
			var wh1 = factory.New<AccChargeCode>();
			var duplicate = factory.New<AccChargeCode>();
			var wh2 = factory.New<AccChargeCode>();
			var wh3 = factory.New<AccChargeCode>();
			var wh4 = factory.New<AccChargeCode>();
			var wh5 = factory.New<AccChargeCode>();
			var wh6 = factory.New<AccChargeCode>();
			var wh7 = factory.New<AccChargeCode>();
			var oth = factory.New<AccChargeCode>();
			var ot1 = factory.New<AccChargeCode>();
			var pl1 = factory.New<AccChargeCode>();

			wh1.AC_ChargeSubGroup = "WH1";
			wh1.AC_Code = "WH1";
			wh1.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			duplicate.AC_ChargeSubGroup = "WH1";
			duplicate.AC_Code = "DUP";
			duplicate.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh2.AC_ChargeSubGroup = "WH2";
			wh2.AC_Code = "OTHER";
			wh2.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh3.AC_ChargeSubGroup = "WH3";
			wh3.AC_Code = "IRRELEVANT";
			wh3.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh4.AC_ChargeSubGroup = "WH4";
			wh4.AC_Code = "WH4";
			wh4.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh5.AC_ChargeSubGroup = "WH5";
			wh5.AC_Code = "WH5";
			wh5.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh6.AC_ChargeSubGroup = "WH6";
			wh6.AC_Code = "WH6";
			wh6.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			wh7.AC_ChargeSubGroup = "WH7";
			wh7.AC_Code = "WH7";
			wh7.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			oth.AC_ChargeSubGroup = "OTH";
			oth.AC_Code = "OTH";
			oth.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			ot1.AC_ChargeSubGroup = "OT1";
			ot1.AC_Code = "OT1";
			ot1.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseInwards.Code;

			pl1.AC_ChargeSubGroup = "PL1";
			pl1.AC_Code = "PL1";
			pl1.AC_ChargeGroup = JobInvoicingConsumerTypes.WarehouseOutwards.Code;

			factory.Save();
		}

		static void SetupClientRates(BusinessObjectFactory factory, WhsWarehouse whs1, WhsWarehouse whs2, OrgHeader client1, OrgHeader client2)
		{
			var clientRate = factory.New<ClientRate>();
			clientRate.TH_OH = client1.PK;

			var entry_Client1_Whs1 = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_Client1_Whs1.TI_WW_Warehouse = whs1.PK;
			entry_Client1_Whs1.AddRateLine("WH1");
			entry_Client1_Whs1.AddRateLine("PL1");
			entry_Client1_Whs1.AddRateLine("OTH");

			var entry_Client1_AllWhs = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_Client1_AllWhs.AllWarehouses = true;
			entry_Client1_AllWhs.AddRateLine("OTHER");
			entry_Client1_AllWhs.AddRateLine("OTH");

			var entry_Client1_Whs2 = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_Client1_Whs2.TI_WW_Warehouse = whs2.PK;
			entry_Client1_Whs2.AddRateLine("IRRELEVANT");

			var otherClientRate = factory.New<ClientRate>();
			otherClientRate.TH_OH = client2.PK;

			var entry_Client2_Whs2 = otherClientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_Client2_Whs2.TI_WW_Warehouse = whs2.PK;
			entry_Client2_Whs2.TI_RateStartDate = ZDate.Today.AddDays(-1);
			entry_Client2_Whs2.TI_RateEndDate = ZDate.Today;
			entry_Client2_Whs2.AddRateLine("WH4");

			var entry_StartsTomorrow = otherClientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_StartsTomorrow.TI_WW_Warehouse = whs2.PK;
			entry_StartsTomorrow.TI_RateStartDate = ZDate.Today.AddDays(1);
			entry_StartsTomorrow.AddRateLine("WH5");

			var entry_FinishedYeterday = otherClientRate.AddRateEntry(RatingConstants.RateCategory.WHS);
			entry_FinishedYeterday.TI_WW_Warehouse = whs2.PK;
			entry_FinishedYeterday.TI_RateStartDate = ZDate.Today.AddDays(-3);
			entry_FinishedYeterday.TI_RateEndDate = ZDate.Today.AddDays(-2);
			entry_FinishedYeterday.AddRateLine("WH6");
			factory.Save();
		}

		static SystemDefinableCodeDescriptionBoolCollection GetWarehouseJobServices()
		{
			var warehouseJobServices = new SystemDefinableCodeDescriptionBoolCollection
			{
				{ "WH1", (NoResString)"Client 1 Warehouse 1", false },
				{ "PL1", (NoResString)"Client 1 Warehouse 1 PICKLINE", false },
				{ "WH2", (NoResString)"Client 1 All Warehouses", false },
				{ "WH3", (NoResString)"Client 1 Warehouse 2", false },
				{ "WH4", (NoResString)"Client 2 Warehouse 2", false },
				{ "WH5", (NoResString)"Invalid Starts Tommorrow", false },
				{ "WH6", (NoResString)"Invalid Finished Yesterday", false },
				{ "WH7", (NoResString)"No Client Rates Set Up", false }
			};
			warehouseJobServices.SetDefaultCode("WH1", true);
			return warehouseJobServices;
		}

		static CodeDescriptionBoolCollection GetGenericJobServicesUnderFreight()
		{
			return new CodeDescriptionBoolCollection
			{
				{ "OTH", (NoResString)"Other Service Client 1 All Warehouses", false },
				{ "OT2", (NoResString)"No Client Rates Set Up", false }
			};
		}

		#endregion
	}
}
