using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator))]
	public class OrderSetCarrierAndCarrierServiceLevelActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestShouldUpdateCarrierAndCarrierServiceLevel

		public void TestShouldUpdateCarrierAndCarrierServiceLevel()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition.", ZGuid.Empty, order.TransportCoPK);
			AssertEquals("Precondition.", ZString.Empty, order.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var orders = new[] { order };

			var expectedLogText = @"INFO: Order [HL W00000002] - was successfully updated.";
			ApplyApplicator(orders, expectedLogText, saveFactoryOnSuccess: true);

			var orderInDB = NewFactory().Load<WhsOrder>(order.PK);
			AssertEquals("Should set value.", transportCo.PK, orderInDB.TransportCoPK);
			AssertEquals("Should set value.", OrgCarrierServiceLevel.StandardCode, orderInDB.WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestShouldOnlyUpdateEnteredOrders

		public void TestShouldOnlyUpdateEnteredOrders()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orderEntered = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);

			var orderIsCancelled = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			orderIsCancelled.CancelReactivateDocket();

			var orderATP = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			Helper.CreatePickNew(orderATP);
			Factory.Save();

			AssertEquals("Precondition", DocketStatus.Codes.Entered, orderEntered.WD_DocketStatus);
			AssertEquals("Precondition", DocketStatus.Codes.Cancelled, orderIsCancelled.WD_DocketStatus);
			AssertEquals("Precondition", DocketStatus.Codes.AttachedToPick, orderATP.WD_DocketStatus);
			AssertEquals("Precondition.", ZGuid.Empty, orderEntered.TransportCoPK);
			AssertEquals("Precondition.", ZGuid.Empty, orderIsCancelled.TransportCoPK);
			AssertEquals("Precondition.", ZGuid.Empty, orderATP.TransportCoPK);
			AssertEquals("Precondition", string.Empty, orderEntered.WD_PL_NKCarrierServiceLevel);
			AssertEquals("Precondition", string.Empty, orderIsCancelled.WD_PL_NKCarrierServiceLevel);
			AssertEquals("Precondition", string.Empty, orderATP.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var orders = new[] { orderEntered, orderIsCancelled, orderATP };

			var expectedErrorLogText = @"INFO: Order [HL W00000002] - was successfully updated.
WARNING: Order [HL W00000003] - does not have a status of entered. Carrier Service Level can only be updated on Entered Orders.
WARNING: Order [HL W00000004] - does not have a status of entered. Carrier Service Level can only be updated on Entered Orders.";
			ApplyApplicator(orders, expectedErrorLogText, saveFactoryOnSuccess: true);

			var orderEnteredInDB = NewFactory().Load<WhsOrder>(orderEntered.PK);
			AssertEquals("Should set carrier.", transportCo.PK, orderEnteredInDB.TransportCoPK);
			AssertEquals("Should set carrier service level.", OrgCarrierServiceLevel.StandardCode, orderEnteredInDB.WD_PL_NKCarrierServiceLevel);

			var orderIsCancelledInDB = NewFactory().Load<WhsOrder>(orderIsCancelled.PK);
			AssertEquals("No change.", ZGuid.Empty, orderIsCancelledInDB.TransportCoPK);
			AssertEquals("No change.", string.Empty, orderIsCancelledInDB.WD_PL_NKCarrierServiceLevel);

			var orderATPInDB = NewFactory().Load<WhsOrder>(orderATP.PK);
			AssertEquals("No change.", ZGuid.Empty, orderATPInDB.TransportCoPK);
			AssertEquals("No change.", string.Empty, orderATPInDB.WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestValidation_Carrier

		public void TestValidation_Carrier()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			AssertEquals("Precondition", ZGuid.Empty, order.TransportCoPK);

			Applicator.CarrierPK = ZGuid.Invalid;

			var orders = new[] { order };

			var expectedErrorLogInvalidPK = "ERROR: There are errors that need to be corrected before this action can be run. Error - CarrierPK: Please enter a valid Carrier.";
			ApplyApplicator(orders, expectedErrorLogInvalidPK, saveFactoryOnSuccess: true);
			AssertEquals("Should not set value.", ZGuid.Empty, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);

			Applicator.CarrierPK = data.Org1.PK;
			AssertEquals("Precondition, Should not be carrier.", ZBool.False, data.Org1.OH_IsShippingProvider);
			var expectedErrorLogNotCarrierPK = "ERROR: There are errors that need to be corrected before this action can be run. Error - CarrierPK: Enter a valid Carrier.";
			ApplyApplicator(orders, expectedErrorLogNotCarrierPK, saveFactoryOnSuccess: true);
			AssertEquals("Should not set value.", ZGuid.Empty, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);

			Applicator.CarrierPK = transportCo.PK;
			var expectedSuccessfulLog = "INFO: Order [HL W00000002] - was successfully updated.";
			ApplyApplicator(orders, expectedSuccessfulLog, saveFactoryOnSuccess: true);
			AssertEquals("Should set value.", transportCo.PK, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);
		}

		#endregion

		#region TestValidation_CarrierServiceLevel

		public void TestValidation_CarrierServiceLevel()
		{
			var transportCo = CreateCarrier();
			var carrierServiceLevels = new OrgCarrierServiceLevelCollection(transportCo.MiscServ);
			var newCSL = carrierServiceLevels.AddNew();
			newCSL.PL_Code = "TST";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.TransportCoPK = transportCo.PK;
			Factory.Save();

			AssertEquals("Precondition", string.Empty, order.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;

			var orders = new[] { order };

			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var expectedLogText = "INFO: Order [HL W00000002] - was successfully updated.";
			ApplyApplicator(orders, expectedLogText, saveFactoryOnSuccess: true);
			AssertEquals("Should set value.", OrgCarrierServiceLevel.StandardCode, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierServiceLevel = "AAA";
			var expectedErrorLogTextWithWarning = "ERROR: There are errors that need to be corrected before this action can be run. Error - CarrierServiceLevel: Enter a valid Carrier Service Level.";
			ApplyApplicator(orders, expectedErrorLogTextWithWarning, saveFactoryOnSuccess: true);
			AssertEquals("Should not change value.", OrgCarrierServiceLevel.StandardCode, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierServiceLevel = "TST";
			ApplyApplicator(orders, expectedLogText, saveFactoryOnSuccess: true);
			AssertEquals("Should set value with valid custom value.", "TST", NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestValidation_CarrierServiceLevel_NoCarrier

		public void TestValidation_CarrierServiceLevel_NoCarrier()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			AssertEquals("Precondition", string.Empty, order.WD_PL_NKCarrierServiceLevel);
			AssertEquals("Precondition", ZGuid.Empty, Applicator.CarrierPK);

			var orders = new[] { order };

			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var expectedErrorLogTextWithWarning = "ERROR: There are errors that need to be corrected before this action can be run. Error - CarrierServiceLevel: Enter a valid Carrier Service Level.";
			ApplyApplicator(orders, expectedErrorLogTextWithWarning, saveFactoryOnSuccess: true);
			AssertEquals("Should not change value.", string.Empty, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestClearCarrierAndCarrierServiceLevelValueWhenUserDoesNotEnterValues

		public void TestClearCarrierAndCarrierServiceLevelValueWhenUserDoesNotEnterValues()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.TransportCoPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			Factory.Save();

			AssertEquals("Precondition. check value store in DB.", transportCo.PK, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);
			AssertEquals("Precondition.", ZGuid.Empty, Applicator.CarrierPK);
			AssertEquals("Precondition.", ZString.Empty, Applicator.CarrierServiceLevel);

			var orders = new[] { order };

			var expectedLogText = @"INFO: Order [HL W00000002] - was successfully updated.";
			ApplyApplicator(orders, expectedLogText, saveFactoryOnSuccess: true);

			var orderInDB = NewFactory().Load<WhsOrder>(order.PK);
			AssertEquals("Should should clear value when is run with empty carrier and carrier service level.", ZGuid.Empty, orderInDB.TransportCoPK);
			AssertEquals("Should should clear value when is run with empty carrier and carrier service level.", ZString.Empty, orderInDB.WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestClearCarrierServiceLevelValueWhenUserDoesNotEnterValue

		public void TestClearCarrierServiceLevelValueWhenUserDoesNotEnterValue()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.TransportCoPK = transportCo.PK;
			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			Factory.Save();

			AssertEquals("Precondition", OrgCarrierServiceLevel.StandardCode, order.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = "";

			var orders = new[] { order };

			var expectedLogText = "INFO: Order [HL W00000002] - was successfully updated.";
			ApplyApplicator(orders, expectedLogText, saveFactoryOnSuccess: true);

			var orderInDB = NewFactory().Load<WhsOrder>(order.PK);
			AssertEquals("Should set to empty carrier.", transportCo.PK, orderInDB.TransportCoPK);
			AssertEquals("Should set to empty carrier service level.", string.Empty, orderInDB.WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestCacheCarrierServiceLevel

		public void TestCacheCarrierServiceLevel()
		{
			var transportCo1 = CreateCarrier();
			var carrierServiceLevels = new OrgCarrierServiceLevelCollection(transportCo1.MiscServ);
			var newCSL = carrierServiceLevels.AddNew();
			newCSL.PL_Code = "TST";

			var transportCo2 = CreateCarrier();
			Factory.Save();

			Applicator.Factory.ResetDatabaseLoadCount();
			AssertEquals("Should have empty list when carrier not set.", 0, Applicator.CarrierServiceLevels.Count);
			var expectedNoDBHits = new Dictionary<string, int>();
			AssertDbHits(expectedNoDBHits, Applicator.Factory);

			Applicator.Factory.ClearQueryCache();
			Applicator.Factory.ResetDatabaseLoadCount();
			Applicator.CarrierPK = transportCo1.PK;
			AssertContainsExactElementsInAnyOrder("Should have default STD and custom value TST.", new[] { "STD", "TST" }, Applicator.CarrierServiceLevels.Select(c => c.PL_Code));
			var expectedOneDBHits = new Dictionary<string, int>() {
				{ OrgCarrierServiceLevelSchema.Constants.TableName , 1 }
			};
			AssertDbHits(expectedOneDBHits, Applicator.Factory);

			Applicator.Factory.ClearQueryCache();
			Applicator.Factory.ResetDatabaseLoadCount();
			Applicator.CarrierPK = transportCo2.PK;
			AssertContainsExactElementsInAnyOrder("Should have default STD.", new[] { "STD" }, Applicator.CarrierServiceLevels.Select(c => c.PL_Code));
			//For new Carrier it should load from DB and cache the value.
			AssertDbHits(expectedOneDBHits, Applicator.Factory);

			Applicator.Factory.ClearQueryCache();
			Applicator.Factory.ResetDatabaseLoadCount();
			Applicator.CarrierPK = transportCo1.PK;
			AssertContainsExactElementsInAnyOrder("Should have default STD and custom value TST.", new[] { "STD", "TST" }, Applicator.CarrierServiceLevels.Select(c => c.PL_Code));
			// expect to load from cache and ** no ** db hit
			AssertDbHits(expectedNoDBHits, Applicator.Factory);
		}

		#endregion

		#region TestValidation_PlannedLoad_Carrier

		public void TestValidation_PlannedLoad_Carrier()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			Applicator.CarrierPK = transportCo.PK;
			var orders = new[] { order };

			ApplyApplicator(orders, "WARNING: Order [HL W00000002] - The Carrier Service Level cannot be updated while the order is assigned to a load.", saveFactoryOnSuccess: true);
			AssertEquals("Should not set value.", ZGuid.Empty, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);

			order.WD_WLO_PlannedLoad = ZGuid.Empty;
			Factory.Save();
			ApplyApplicator(orders, "INFO: Order [HL W00000002] - was successfully updated.", saveFactoryOnSuccess: true);
			AssertEquals("Should set value.", transportCo.PK, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);
		}

		public void TestValidation_Carrier_LoadLinkedToLoadThroughPackage()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.WD_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			load.WLO_JobID = "WL00000001";
			Factory.Save();

			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			orderInNewFactory.WD_DocketStatus = DocketStatus.Codes.Entered;
			Applicator.CarrierPK = transportCo.PK;
			var orders = new[] { orderInNewFactory };

			ApplyApplicator(orders, "WARNING: Order [HL W00000002] - The Carrier Service Level cannot be updated while the order is assigned to a load.", saveFactoryOnSuccess: false);
			AssertEquals("Should not set value.", ZGuid.Empty, NewFactory().Load<WhsOrder>(order.PK).TransportCoPK);
		}

		#endregion

		#region TestValidation_PlannedLoad_CarrierServiceLevel

		public void TestValidation_PlannedLoad_CarrierServiceLevel()
		{
			var transportCo = CreateCarrier();
			var carrierServiceLevels = new OrgCarrierServiceLevelCollection(transportCo.MiscServ);
			var newCSL = carrierServiceLevels.AddNew();
			newCSL.PL_Code = "TST";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.TransportCoPK = transportCo.PK;
			Factory.Save();
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			AssertEquals("Precondition", string.Empty, order.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var orders = new[] { order };

			ApplyApplicator(orders, "WARNING: Order [HL W00000002] - The Carrier Service Level cannot be updated while the order is assigned to a load.", saveFactoryOnSuccess: true);
			AssertEquals("Should not set value.", ZString.Empty, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);

			order.WD_WLO_PlannedLoad = ZGuid.Empty;
			Factory.Save();
			ApplyApplicator(orders, "INFO: Order [HL W00000002] - was successfully updated.", saveFactoryOnSuccess: true);
			AssertEquals("Should set value.", OrgCarrierServiceLevel.StandardCode, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);
		}

		public void TestValidation_CarrierServiceLevel_LoadLinkedToLoadThroughPackage()
		{
			var transportCo = CreateCarrier();
			var carrierServiceLevels = new OrgCarrierServiceLevelCollection(transportCo.MiscServ);
			var newCSL = carrierServiceLevels.AddNew();
			newCSL.PL_Code = "TST";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			order.TransportCoPK = transportCo.PK;
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation, startTime: DateTimeOffset.Now);
			load.WLO_JobID = "WL00000001";
			Factory.Save();

			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			orderInNewFactory.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition", string.Empty, orderInNewFactory.WD_PL_NKCarrierServiceLevel);

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var orders = new[] { orderInNewFactory };

			ApplyApplicator(orders, "WARNING: Order [HL W00000002] - The Carrier Service Level cannot be updated while the order is assigned to a load.", saveFactoryOnSuccess: false);
			AssertEquals("Should not set value.", ZString.Empty, NewFactory().Load<WhsOrder>(order.PK).WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region DBHits Test

		public void TestShouldUpdateCarrierAndCarrierServiceLevel_DBHits()
		{
			var transportCo = CreateCarrier();
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var orders = new List<WhsOrder>();
			for (int i = 0; i < 20; i++)
			{
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", data.Part1, 1m);
				if (i % 2 != 1)
				{
					order.WD_WLO_PlannedLoad = load.PK;
				}
				orders.Add(order);
			}

			Factory.Save();

			Assert("Precondition.", orders.All(order => order.TransportCoPK.IsEmpty));
			Assert("Precondition.", orders.All(order => order.WD_PL_NKCarrierServiceLevel.IsEmpty));

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgCarrierServiceLevelSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ RateTransportProviderSchema.Constants.TableName, 1 },
			};

			Applicator.CarrierPK = transportCo.PK;
			Applicator.CarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			var ordersInDB = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			{
				SimulateRun(ordersInDB, true);
			}

			AssertEquals("Should set value on 10 orders.", 10, ordersInDB.Count(order => order.TransportCoPK == transportCo.PK));
			AssertEquals("Should set value on 10 orders.", 10, ordersInDB.Count(order => order.WD_PL_NKCarrierServiceLevel == OrgCarrierServiceLevel.StandardCode));
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		OrgHeader CreateCarrier()
		{
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsShippingProvider = true;
			return transportCo;
		}

		#endregion

		new OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator Applicator => (OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator)base.Applicator;
	}
}
