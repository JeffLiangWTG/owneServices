using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Services
{
	class OrderManagerConvertServiceTest : TestCaseWithFactory
	{
		public void TestCYContainerLoadListForPlanningShipment()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder("ORD01");
			var orderLine = builder.BuildOrderLine("ORL01", "ORD01");

			var booking = builder.BuildSupplierBooking("JSB01", "PLC");
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			var bookingLine01 = builder.BuildSupplierBookingLine("JSL01", "JSB01", null, "ORL01", 10, 10);
			var bookingLine02 = builder.BuildSupplierBookingLine("JSL02", "JSB01", null, "ORL01", 10, 10);
			var bookingLine03 = builder.BuildSupplierBookingLine("JSL03", "JSB01", null, "ORL01", 10, 10);
			var bookingLine04 = builder.BuildSupplierBookingLine("JSL04", "JSB01", null, "ORL01", 10, 10);

			var consol01 = builder.BuildConsol("JK01");
			builder.BuildContainer("JC01", "JK01", "JSB01", null);
			builder.BuildContainer("JC02", "JK01", "JSB01", null);
			builder.BuildContainer("JC03", "JK01", "JSB01", null);

			Factory.Save();

			var shipment = builder.BuildShipment("JS01", "JK01");

			ForwardingPackLine AddPlannedPackLine(JobSupplierBookingLine bookingLine, decimal quantity, int package)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = package;
				packLine.JL_JSL_BookingLine = bookingLine.PK;
				var product = packLine.Products.AddNew();
				product.D2_JO = orderLine.PK;
				product.D2_ProductQuantity = quantity;
				product.D2_ProductUnitOfQty = "";

				return packLine;
			}

			var packLine01 = AddPlannedPackLine(bookingLine01, 10, 10);
			var packLine02 = AddPlannedPackLine(bookingLine02, 10, 10);
			var packLine03 = AddPlannedPackLine(bookingLine03, 10, 10);
			var packLine04 = AddPlannedPackLine(bookingLine04, 10, 10);

			var loadList = builder.BuildContainerLoadList("CLH01", "JSB01", status: "SHP");

			ContainerLoadListLine AddLoadListLine(string key, string bookingLineKey, string containerKey, decimal quantity, int packageCount)
			{
				var loadListLine = builder.BuildContainerLoadListLine(key, "CLH01", bookingLineKey, containerKey);
				loadListLine.CLL_PackedQuantity = quantity;
				loadListLine.CLL_Packages = packageCount;

				return loadListLine;
			}

			var loadListLine01 = AddLoadListLine("CLL01", "JSL01", "JC01", 3, 3);
			var loadListLine02 = AddLoadListLine("CLL02", "JSL02", "JC02", 13, 13);
			var loadListLine03 = AddLoadListLine("CLL03", "JSL03", "JC03", 10, 10);

			Factory.Save();

			CombineAssertions("Prerequisite: load list lines are separately packed to the planned pack lines", () =>
			{
				AssertEquals(1, consol01.Shipments.Count);
				AssertEquals(4, shipment.OuterPackLines.Count);
			});

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadList.PK, Factory);
			Factory.Save();

			consol01.Shipments.Reload(true);
			shipment.OuterPackLines.Reload(true);
			packLine01.Reload();
			packLine02.Reload();
			packLine03.Reload();
			packLine04.Reload();

			CombineAssertions("load list lines are separately packed to the final pack lines", () =>
			{
				AssertEquals(1, consol01.Shipments.Count);
				AssertEquals(5, shipment.OuterPackLines.Count);

				var splitLine = shipment.OuterPackLines.OfType<ForwardingPackLine>().Except(new ForwardingPackLine[] { packLine01, packLine02, packLine03, packLine04 }).Single();

				AssertEquals(7, packLine01.JL_PackageCount);
				AssertEquals(13, packLine02.JL_PackageCount);
				AssertEquals(10, packLine03.JL_PackageCount);
				AssertEquals(10, packLine04.JL_PackageCount);
				AssertEquals(3, splitLine.JL_PackageCount);

				AssertEquals(null, packLine01.LoadListLine);
				AssertEquals(loadListLine02, packLine02.LoadListLine);
				AssertEquals(loadListLine03, packLine03.LoadListLine);
				AssertEquals(null, packLine04.LoadListLine);
				AssertEquals(loadListLine01, splitLine.LoadListLine);
			});

			AssertMismatchedPackingCompletedEvent(shouldAdd: false, booking);
			AssertEquals("No new shipment is created", shipment.PK, Factory.Load<ForwardingShipment>(new ZQuery()).Single().PK);
		}

		public void TestFinalizingSupplierBookingShipmentCreation()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder("ORD01");
			builder.BuildOrderLine("ORL01", "ORD01");
			var supplierBooking = builder.BuildSupplierBooking("JSB01");
			supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;
			builder.BuildSupplierBookingLine("JSL01", "JSB01", null, "ORL01");
			builder.BuildSupplierBookingLine("JSL02", "JSB01", null, "ORL01");
			builder.BuildConsol("JK01");
			builder.BuildContainer("JC01", "JK01", "JSB01", null);
			builder.BuildContainer("JC02", "JK01", "JSB01", null);
			var containerLoadList1 = builder.BuildContainerLoadList("CLH01", "JSB01");
			containerLoadList1.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Converted;
			builder.BuildContainerLoadListLine("CLL01", "CLH01", "JSL01", "JC01");

			var contaienrLoadList2 = builder.BuildContainerLoadList("CLH02", "JSB01");
			contaienrLoadList2.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Placed;
			builder.BuildContainerLoadListLine("CLL02", "CLH02", "JSL02", "JC02");

			var contaienrLoadList3 = builder.BuildContainerLoadList("CLH03", "JSB01");
			contaienrLoadList3.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Incomplete;
			builder.BuildContainerLoadListLine("CLL03", "CLH03", "JSL02", "JC02");

			Factory.Save();

			new OrderManagerConvertService().FinalizingSupplierBookingShipmentCreation(supplierBooking);
			Factory.Save();

			supplierBooking.Reload();
			containerLoadList1.Reload();
			contaienrLoadList2.Reload();
			contaienrLoadList3.Reload();

			AssertEquals(ContainerLoadListHeaderStatus.Converted, containerLoadList1.CLH_Status);
			AssertEquals(false, containerLoadList1.GetLogs().GetAllLogs().OfType<StmALog>().Any(log => log.SL_SE_NKEvent == "STU" && log.SL_Reference == "|TYP=CLL|OLD=PLC|NEW=CNV"));
			AssertEquals(ContainerLoadListHeaderStatus.Converted, contaienrLoadList2.CLH_Status);
			AssertEquals(true, contaienrLoadList2.GetLogs().GetAllLogs().OfType<StmALog>().Any(log => log.SL_SE_NKEvent == "STU" && log.SL_Reference == "|TYP=CLL|OLD=PLC|NEW=CNV"));
			AssertEquals(ContainerLoadListHeaderStatus.Incomplete, contaienrLoadList3.CLH_Status);
			AssertEquals(false, contaienrLoadList3.GetLogs().GetAllLogs().OfType<StmALog>().Any(log => log.SL_SE_NKEvent == "STU" && log.SL_Reference == "|TYP=CLL|OLD=PLC|NEW=CNV"));
			AssertEquals(SupplierBookingStatus.Converted, supplierBooking.JSB_Status);
		}

		public void TestFinalizingSupplierBookingShipmentCreation_WillNotUpdateShipmentFromContainerLoadListHeader()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder("ORD01");
			builder.BuildOrderLine("ORL01", "ORD01");
			var supplierBooking = builder.BuildSupplierBooking("JSB01");
			supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;

			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_JSB_Booking = supplierBooking.PK;
			containerLoadList.CLH_MarksAndNumbers = "Test marks and numbers";
			containerLoadList.CLH_GoodsDescription = "Test goods description";
			containerLoadList.CLH_DetailedGoodsDescription = "Test detailed goods description";
			containerLoadList.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Placed;
			var loadListLineNonSPT = CreateContainerLoadListLine(containerLoadList);
			var loadListLineFromSPT = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLineFromSPT);
			Factory.Save();

			new OrderManagerConvertService().FinalizingSupplierBookingShipmentCreation(supplierBooking);
			Factory.Save();

			supplierBooking.Reload();
			plannedPackLine.Reload();

			AssertEquals(string.Empty, plannedPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals(string.Empty, plannedPackLine.Shipment.JS_GoodsDescription);
			AssertEquals(string.Empty, plannedPackLine.Shipment.DetailedGoodsDescriptionNoteText);

			var packLines = Factory.Load<ForwardingPackLine>(new ZQuery());
			var nonSPTPackLine = packLines.Single(p => p.PK == loadListLineNonSPT.CLL_JL_PackLine);
			AssertEquals(string.Empty, nonSPTPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals(string.Empty, nonSPTPackLine.Shipment.JS_GoodsDescription);
			AssertEquals(string.Empty, nonSPTPackLine.Shipment.DetailedGoodsDescriptionNoteText);
		}

		[ExpectNoExceptions]
		public void TestConversionOfLooseCargoSupplierBooking()
		{
			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			orderLine.Order.JD_OA_BuyerAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var supplierBooking = Factory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.LooseCargo;
			supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;
			var bookingLine1_1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1_1.JSL_DispatchedQuantity = 1;
			bookingLine1_1.JSL_JO_OrderLine = orderLine.PK;
			var bookingLine1_2 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1_2.JSL_DispatchedQuantity = 1;
			bookingLine1_2.JSL_JO_OrderLine = orderLine.PK;
			var bookingLine1_3 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1_3.JSL_DispatchedQuantity = 1;
			bookingLine1_3.JSL_JO_OrderLine = orderLine.PK;

			var service = new OrderManagerConvertService();

			service.ConvertLooseCargoSupplierBooking(supplierBooking, Factory);

			AssertEquals(Core.Constants.SupplierBookingStatus.Converted, supplierBooking.JSB_Status);
			var convertedShipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;
			AssertNotNull(convertedShipment);
			AssertEquals(3, convertedShipment.OuterPackLines.Count);
		}

		void AddBookingLine(JobSupplierBooking supplierBooking, ZGuid orderLinePK, decimal bookedQty, int bookedPacks, decimal grossWeight, decimal volume, int dispatchedQuantity = 1, int dispatchedPackages = 1, int dispatchedWeight = 2, int dispatchedVolume = 3)
		{
			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_DispatchedQuantity = dispatchedQuantity;
			bookingLine.JSL_JO_OrderLine = orderLinePK;

			bookingLine.JSL_BookedQuantity = bookedQty;
			bookingLine.JSL_BookedPackages = bookedPacks;

			bookingLine.JSL_GrossWeight = grossWeight;
			bookingLine.JSL_GrossWeightUnit = "G";

			bookingLine.JSL_Volume = volume;
			bookingLine.JSL_VolumeUnit = "CY";

			bookingLine.JSL_DispatchedPackages = dispatchedPackages;
			bookingLine.JSL_DispatchedWeight = dispatchedWeight;
			bookingLine.JSL_DispatchedVolume = dispatchedVolume;
		}

		JobSupplierBooking PrepareLooseCargoSupplierBookingData()
		{
			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			orderLine.Order.JD_OA_BuyerAddress = Factory.New<OrgHeader>().MainAddress.PK;
			orderLine.JO_ItemPrice = 1;

			var supplierBooking = Factory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.LooseCargo;
			supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;

			AddBookingLine(supplierBooking, orderLine.PK, bookedQty: 1, bookedPacks: 6, grossWeight: 7, volume: 8);
			AddBookingLine(supplierBooking, orderLine.PK, bookedQty: 2, bookedPacks: 9, grossWeight: 10, volume: 11);
			AddBookingLine(supplierBooking, orderLine.PK, bookedQty: 3, bookedPacks: 12, grossWeight: 13, volume: 14, dispatchedQuantity: 2, dispatchedPackages: 12, dispatchedWeight: 3, dispatchedVolume: 14);

			return supplierBooking;
		}

		[ExpectNoExceptions]
		public void TestConversionOfLooseCargoSupplierBooking_UpdateShipmentFromOuterPackLines()
		{
			var supplierBooking = PrepareLooseCargoSupplierBookingData();

			var service = new OrderManagerConvertService();

			service.ConvertLooseCargoSupplierBooking(supplierBooking, Factory);

			AssertEquals(Core.Constants.SupplierBookingStatus.Converted, supplierBooking.JSB_Status);
			var convertedShipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;

			AssertNotNull(convertedShipment);

			AssertEquals(3, convertedShipment.OuterPackLines.Count);
			AssertEquals("it should be the sum of dispatchedPackages of each packLine: 1 + 1 + 12 = 14", 14, convertedShipment.JS_OuterPacks);

			AssertEquals("it should be the sum of dispatchedWeight of each packLine: 2 + 2 + 3 = 7", 7, (int)convertedShipment.JS_ActualWeight);
			AssertEquals("G", convertedShipment.JS_UnitOfWeight);

			AssertEquals("it should be the sum of dispatchedVolume of each packLine: 3 + 3 + 14 = 20", 20, (int)convertedShipment.JS_ActualVolume);
			AssertEquals("CY", convertedShipment.JS_UnitOfVolume);

			AssertEquals("it should be the sum of linePrice of each packLine(dispatchedQuantity of this line * ItemPrice, note the packline price is already calculated when being generated): 1 + 1 + 2 = 4",
				4,
				(int)convertedShipment.JS_GoodsValue
			);
		}

		[ExpectNoExceptions]
		public void TestConversionOfConvertedLooseCargoSupplierBooking()
		{
			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			var supplierBooking = Factory.New<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.LooseCargo;
			supplierBooking.JSB_Status = Core.Constants.SupplierBookingStatus.Converted;
			var bookingLine1_1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1_1.JSL_DispatchedQuantity = 1;
			bookingLine1_1.JSL_JO_OrderLine = orderLine.PK;

			var service = new OrderManagerConvertService();

			service.ConvertLooseCargoSupplierBooking(supplierBooking, Factory);

			AssertEquals(Core.Constants.SupplierBookingStatus.Converted, supplierBooking.JSB_Status);
			AssertNull(Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)));
		}

		#region Test ConvertContainerLoadListToShipments

		public void TestConvertContainerLoadListToShipments_ShouldHandleSPTAndNonSPTLines()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLineNonSPT = CreateContainerLoadListLine(containerLoadList);
			var loadListLineFromSPT = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLineFromSPT);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);
			Factory.Save();

			var packLines = Factory.Load<ForwardingPackLine>(new ZQuery());
			AssertEquals("Two pack lines are created", 2, packLines.Length);

			Assert("SPT pack line is converted", plannedPackLine.Containers.Count > 0);
			AssertEquals("SPT pack line is linked to CLL's booking line", plannedPackLine.PK, loadListLineFromSPT.CLL_JL_PackLine);

			var nonSPTPackLine = packLines.Single(p => p.PK == loadListLineNonSPT.CLL_JL_PackLine);
			AssertEquals("Non SPT pack line has no linked booking line", ZGuid.Empty, nonSPTPackLine.JL_JSL_BookingLine);
			AssertMismatchedPackingCompletedEvent(shouldAdd: true, loadListLineNonSPT.SupplierBookingLine.SupplierBooking, nonSPTPackLine.Shipment);

			AssertEquals(containerLoadList.CLH_Status, ContainerLoadListHeaderStatus.Converted);
		}

		public void TestConvertContainerLoadListToShipments_IsNotFinalized()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_MarksAndNumbers = "Test marks and numbers";
			containerLoadList.CLH_GoodsDescription = "Test goods description";
			containerLoadList.CLH_DetailedGoodsDescription = "Test detailed goods description";
			var loadListLineNonSPT = CreateContainerLoadListLine(containerLoadList);
			var loadListLineFromSPT = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLineFromSPT);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory, isFinalizing: false);

			AssertEquals("Test marks and numbers", plannedPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals("Test goods description", plannedPackLine.Shipment.JS_GoodsDescription);
			AssertEquals("Test detailed goods description", plannedPackLine.Shipment.DetailedGoodsDescriptionNoteText);

			var packLines = Factory.Load<ForwardingPackLine>(new ZQuery());
			var nonSPTPackLine = packLines.Single(p => p.PK == loadListLineNonSPT.CLL_JL_PackLine);
			AssertEquals("Test marks and numbers", nonSPTPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals("Test goods description", nonSPTPackLine.Shipment.JS_GoodsDescription);
			AssertEquals("Test detailed goods description", nonSPTPackLine.Shipment.DetailedGoodsDescriptionNoteText);
		}

		public void TestConvertContainerLoadListToShipments_IsFinalized()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_MarksAndNumbers = "Test marks and numbers";
			containerLoadList.CLH_GoodsDescription = "Test goods description";
			containerLoadList.CLH_DetailedGoodsDescription = "Test detailed goods description";
			var loadListLineNonSPT = CreateContainerLoadListLine(containerLoadList);
			var loadListLineFromSPT = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLineFromSPT);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory, isFinalizing: true);

			AssertEquals("", plannedPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals("", plannedPackLine.Shipment.JS_GoodsDescription);
			AssertEquals("", plannedPackLine.Shipment.DetailedGoodsDescriptionNoteText);

			var packLines = Factory.Load<ForwardingPackLine>(new ZQuery());
			var nonSPTPackLine = packLines.Single(p => p.PK == loadListLineNonSPT.CLL_JL_PackLine);
			AssertEquals("", nonSPTPackLine.Shipment.JS_MarksAndNumbers);
			AssertEquals("", nonSPTPackLine.Shipment.JS_GoodsDescription);
			AssertEquals("", nonSPTPackLine.Shipment.DetailedGoodsDescriptionNoteText);
		}

		public void TestConvertContainerLoadListToShipments_ShouldAddOnlyOnePKCEventPerBookingOnNonSPTNewShipments()
		{
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			var booking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			var booking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking1.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CFS;
			booking2.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CFS;

			var bookingLine1 = booking1.SupplierBookingLines.AddNew();
			var bookingLine2 = booking1.SupplierBookingLines.AddNew();
			var bookingLine3 = booking2.SupplierBookingLines.AddNew();
			var bookingLine4 = booking2.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine.PK;
			bookingLine2.JSL_JO_OrderLine = orderLine.PK;
			bookingLine3.JSL_JO_OrderLine = orderLine.PK;
			bookingLine4.JSL_JO_OrderLine = orderLine.PK;

			var containerLoadList = Factory.NewWithValidTestData<CFSContainerLoadList>();
			var loadListLine1 = containerLoadList.LoadListLines.AddNew();
			var loadListLine2 = containerLoadList.LoadListLines.AddNew();
			var loadListLine3 = containerLoadList.LoadListLines.AddNew();
			var loadListLine4 = containerLoadList.LoadListLines.AddNew();

			loadListLine1.CLL_JSL_BookingLine = bookingLine1.PK;
			loadListLine2.CLL_JSL_BookingLine = bookingLine2.PK;
			loadListLine3.CLL_JSL_BookingLine = bookingLine3.PK;
			loadListLine4.CLL_JSL_BookingLine = bookingLine4.PK;

			loadListLine1.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingConsol>().Containers.AddNew().PK;
			loadListLine2.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingConsol>().Containers.AddNew().PK;
			loadListLine3.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingConsol>().Containers.AddNew().PK;
			loadListLine4.CLL_JC_Container = Factory.NewWithValidTestData<ForwardingConsol>().Containers.AddNew().PK;

			AssignQuantitiesToContainerLoadListLine(loadListLine1, 100m);
			AssignQuantitiesToContainerLoadListLine(loadListLine2, 100m);
			AssignQuantitiesToContainerLoadListLine(loadListLine3, 100m);
			AssignQuantitiesToContainerLoadListLine(loadListLine4, 100m);

			Factory.Save();

			var orderManagerConvertService = new OrderManagerConvertService();
			orderManagerConvertService.ConvertContainerLoadListToShipments(loadListLine1.CLL_CLH_LoadListHeader, Factory);
			Factory.Save();

			var newPackLine1 = loadListLine1.GetPackLine();
			var newPackLine2 = loadListLine2.GetPackLine();
			var newPackLine3 = loadListLine3.GetPackLine();
			var newPackLine4 = loadListLine4.GetPackLine();

			AssertEquals("Additional shipments created for unplanned bookings",
				expected: 4,
				new[] {
					newPackLine1.Shipment.PK,
					newPackLine2.Shipment.PK,
					newPackLine3.Shipment.PK,
					newPackLine4.Shipment.PK,
				}.Distinct().Count());

			CombineAssertions(() =>
			{
				AssertMismatchedPackingCompletedEvent(true, booking1, newPackLine1.Shipment, newPackLine2.Shipment);
				AssertMismatchedPackingCompletedEvent(true, booking2, newPackLine3.Shipment, newPackLine4.Shipment);
			});
		}

		public void TestConvertContainerLoadListToShipments_ShouldFindPlannedPackLinesInMultipleShipments()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList);
			var loadListLine2 = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var packLine1 = CreatePlannedPackLine(loadListLine1, quantity: 100);
			var packLine2 = CreatePlannedPackLine(loadListLine2, quantity: 100);
			Factory.Save();

			AssertNotEquals("Prerequisite: planned to different shipments", packLine1.JL_JS, packLine2.JL_JS);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);
			AssertContainsExactElementsInAnyOrder("Both planned pack lines are used in conversion",
				new[] { packLine1.PK, packLine2.PK },
				new[] { loadListLine1.GetPackLine().PK, loadListLine2.GetPackLine().PK });
		}

		#endregion

		#region Test ConvertLoadListLineFromSPTToPackLine

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldIgnoreContainerAsPlannedPackLineCriteria()
		{
			var loadListLine = CreateContainerLoadListLine();
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine);
			plannedPackLine.SetContainer(plannedPackLine.Shipment.Consols[0], plannedPackLine.Shipment.Consols[0].Containers[0]);
			Factory.Save();

			Assert("Prerequisite: planned line has container assigned", plannedPackLine.Containers.Count > 0);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);
			AssertEquals("Expect to convert using the planned packline", loadListLine.GetPackLine().PK, plannedPackLine.PK);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ReductionIsEffectiveWithoutWritingToDB()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList, 70);
			var loadListLine2 = CloneContainerLoadListLine(loadListLine1, 20);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine1, quantity: 100);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);
			AssertEquals("Expect to reduce planned quantity by both load list lines", 10m, plannedPackLine.Products[0].D2_ProductQuantity);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldSplitWhenShortPacked()
		{
			var loadListLine = CreateContainerLoadListLine(packedQuantity: 80);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine, quantity: 100);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);
			AssertEquals("Expect to reduce planned quantity", 20m, plannedPackLine.Products[0].D2_ProductQuantity);
			AssertSPTPackLineProperties(loadListLine, expectedQuantity: 80);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldUpdatePlannedPackLineIfQuantityIsFullyPacked()
		{
			var loadListLine = CreateContainerLoadListLine(packedQuantity: 100);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine, quantity: 100);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				"Expect no splitting",
				new[] { plannedPackLine.PK },
				Factory.Load<ForwardingPackLine>(new ZQuery()).Select(x => x.PK));
			AssertSPTPackLineProperties(loadListLine, expectedQuantity: 100);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldUpdatePlannedPackLineIfQuantityIsOverPacked()
		{
			var loadListLine = CreateContainerLoadListLine(packedQuantity: 110);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine, quantity: 100);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);

			AssertContainsExactElementsInAnyOrder(
				"Expect no splitting",
				new[] { plannedPackLine.PK },
				Factory.Load<ForwardingPackLine>(new ZQuery()).Select(x => x.PK));
			AssertSPTPackLineProperties(loadListLine, expectedQuantity: 110);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldClonePackedPackLineIfNoPlannedPackLinesToBeginWith()
		{
			var loadListLine = CreateContainerLoadListLine();
			Factory.Save();

			var packedPackLine = CreatePlannedPackLine(loadListLine, isPacked: true);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);
			var convertedPackLine = loadListLine.GetPackLine();
			AssertEquals("Expect clone to same shipment as packed pack line", convertedPackLine.JL_JS, packedPackLine.JL_JS);
			AssertNotEquals("Expect to clone from packed pack line", convertedPackLine.PK, packedPackLine.PK);
			AssertSPTPackLineProperties(loadListLine, expectedQuantity: 100);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldUpdatePlannedPackLineIfLineProductUnmatch()
		{
			var loadListLine = CreateContainerLoadListLine();
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine);
			plannedPackLine.Products[0].D2_JO = Factory.NewWithValidTestData<OrderLine>().PK;
			Factory.Save();

			AssertNotEquals("Prerequisite", plannedPackLine.Products[0].D2_JO, loadListLine.SupplierBookingLine.JSL_JO_OrderLine);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);

			AssertEquals("Should convert in place", plannedPackLine.PK, loadListLine.GetPackLine().PK);
			AssertSPTPackLineProperties(loadListLine);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldUpdatePlannedPackLineIfLineHasNoProduct()
		{
			var loadListLine = CreateContainerLoadListLine();
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine);
			plannedPackLine.Products.RemoveAll(x => true);
			Factory.Save();

			AssertEquals("Prerequisite", expected: 0, plannedPackLine.Products.Count);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);

			AssertEquals("Should convert in place", plannedPackLine.PK, loadListLine.GetPackLine().PK);
			AssertSPTPackLineProperties(loadListLine);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldPreferPlannedPackLineOverPackedPackLineAsTemplate()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine = CreateContainerLoadListLine(containerLoadList);
			Factory.Save();

			var packedPackLine = CreatePlannedPackLine(loadListLine, quantity: 100, isPacked: true);
			packedPackLine.JL_SystemCreateTimeUtc = new ZDateTime(2023, 12, 31);
			var plannedPackLine = CreatePlannedPackLine(loadListLine, quantity: 100, isPacked: false);
			plannedPackLine.JL_SystemCreateTimeUtc = new ZDateTime(2024, 1, 1);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);
			AssertEquals("Planned line is used", loadListLine.GetPackLine().PK, plannedPackLine.PK);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_ShouldClonePackedPackLineIfAllPlannedPackLinesDepleted()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList, 100);
			var loadListLine2 = CloneContainerLoadListLine(loadListLine1, 1);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine1, quantity: 100);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);

			Factory.Save();
			var packLines = Factory.Load<ForwardingPackLine>(new ZQuery());
			AssertEquals("Expect two new lines are generated", 2, packLines.Length);
			AssertContainsExactElementsInAnyOrder(
				"Generated pack lines from each container load list line",
				packLines,
				new[] { loadListLine1.GetPackLine(), loadListLine2.GetPackLine() });
		}

		public void TestConvertLoadListLineFromSPTToPackLine_UpdateShipmentFromOuterPackLines()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList, 50);
			var loadListLine2 = CloneContainerLoadListLine(loadListLine1, 70);
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine1, quantity: 100);
			Factory.Save();

			AssertEquals(0, plannedPackLine.Shipment.JS_TotalPackageCount);
			AssertEquals(0m, plannedPackLine.Shipment.JS_ActualWeight);
			AssertEquals(0m, plannedPackLine.Shipment.JS_ActualVolume);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);

			AssertEquals(120, plannedPackLine.Shipment.JS_OuterPacks);
			AssertEquals(120m, plannedPackLine.Shipment.JS_ActualWeight);
			AssertEquals(120m, plannedPackLine.Shipment.JS_ActualVolume);
		}

		public void TestConvertPlannedPackLine_UpdateContainerWeightUnitIfNeeded()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList, 100);
			loadListLine1.CLL_Weight = 75m;
			loadListLine1.CLL_WeightUnit = Weight.Kilotonnes;
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine1, quantity: 100);
			plannedPackLine.JL_ActualWeight = 75m;
			plannedPackLine.JL_ActualWeightUQ = Weight.Kilotonnes;

			Factory.Save();

			AssertEquals(0m, loadListLine1.Container.JC_GrossWeight);
			AssertEquals(Weight.Kilograms, loadListLine1.Container.JC_GrossWeightUQ);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);

			AssertEquals(75_000m, loadListLine1.Container.JC_GrossWeight);
			AssertEquals(Weight.Tonnes, loadListLine1.Container.JC_GrossWeightUQ);
		}

		public void TestConvertLoadListLine_UpdateContainerWeightUnitIfNeeded()
		{
			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			var loadListLine1 = CreateContainerLoadListLine(containerLoadList, 50);
			loadListLine1.CLL_Weight = 50m;
			loadListLine1.CLL_WeightUnit = Weight.Kilotonnes;
			Factory.Save();

			AssertEquals(0m, loadListLine1.Container.JC_GrossWeight);
			AssertEquals(Weight.Kilograms, loadListLine1.Container.JC_GrossWeightUQ);

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(containerLoadList.PK, Factory);

			AssertEquals(50_000m, loadListLine1.Container.JC_GrossWeight);
			AssertEquals(Weight.Tonnes, loadListLine1.Container.JC_GrossWeightUQ);
		}

		public void TestConvertLoadListLineFromSPTToPackLine_HandlesSeparateContainerLoadList()
		{
			var loadListLine1 = CreateContainerLoadListLine(packedQuantity: 100);
			Factory.Save();

			var containerLoadList2 = Factory.NewWithValidTestData<CYContainerLoadList>();

			var loadListLine2 = containerLoadList2.LoadListLines.AddNew();
			loadListLine2.CLL_PackedQuantity = 100;
			loadListLine2.CLL_JSL_BookingLine = loadListLine1.CLL_JSL_BookingLine;
			loadListLine2.CLL_JC_Container = loadListLine1.CLL_JC_Container;

			var plannedPackLine = CreatePlannedPackLine(loadListLine1);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine1.CLL_CLH_LoadListHeader, Factory);
			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine2.CLL_CLH_LoadListHeader, Factory);
			AssertEquals("Expect the first load list line is packed to original shipment", plannedPackLine.JL_JS, loadListLine1.GetPackLine().JL_JS);
			AssertEquals("Expect the second load list line is packed to original shipment (sticky)", plannedPackLine.JL_JS, loadListLine2.GetPackLine().JL_JS);
			AssertEquals("Expect the second load list line linked to the same booking line", plannedPackLine.JL_JSL_BookingLine, loadListLine2.GetPackLine().JL_JSL_BookingLine);
		}

		public void TestConvertLoadListLine_ShouldUpdateProductDimensionFromSBKLine()
		{
			var loadListLine = CreateContainerLoadListLine();
			Factory.Save();

			var plannedPackLine = CreatePlannedPackLine(loadListLine);
			Factory.Save();

			new OrderManagerConvertService().ConvertContainerLoadListToShipments(loadListLine.CLL_CLH_LoadListHeader, Factory);

			var packLine = loadListLine.GetPackLine();
			AssertEquals(10m, packLine.JL_Height);
			AssertEquals(20m, packLine.JL_Length);
			AssertEquals(30m, packLine.JL_Width);
			AssertEquals(Core.Constants.Dimension.Feet, packLine.JL_UnitOfDimension);
		}

		#endregion

		#region Test ReleaseUnusedContainersFromCYBooking

		const string OrderKey = "ORD1";
		const string OrderLine1Key = "ORL01";
		const string OrderLine2Key = "ORL02";
		const string BookingKey = "SBK01";
		const string BookingLine1Key = "SBL01";
		const string BookingLine2Key = "SBL02";
		const string ConsolKey = "JK01";
		const string CLH1Key = "CLH01";
		const string CLH2Key = "CLH02";
		const string CLL1Key = "CLL01";
		const string CLL2Key = "CLL02";
		const string Container1Key = "JC01";
		const string Container2Key = "JC02";
		const string Container3Key = "JC03";

		public void TestNoUnusedContainersShouldNotAffectBooking()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder(OrderKey);
			builder.BuildOrderLine(OrderLine1Key, OrderKey);
			builder.BuildOrderLine(OrderLine2Key, OrderKey);

			var booking = builder.BuildSupplierBooking(BookingKey, SupplierBookingStatus.Planned);
			builder.BuildSupplierBookingLine(BookingLine1Key, BookingKey, null, OrderLine1Key);
			builder.BuildSupplierBookingLine(BookingLine2Key, BookingKey, null, OrderLine2Key);

			builder.BuildConsol(ConsolKey);

			var clh1 = builder.BuildContainerLoadList(CLH1Key, BookingKey, status: ContainerLoadListHeaderStatus.Placed);

			var container1 = builder.BuildContainer(Container1Key, ConsolKey, BookingKey, null);
			var container2 = builder.BuildContainer(Container2Key, ConsolKey, BookingKey, null);

			builder.BuildContainerLoadListLine(CLL1Key, CLH1Key, BookingLine1Key, Container1Key);
			builder.BuildContainerLoadListLine(CLL2Key, CLH1Key, BookingLine2Key, Container2Key);

			Factory.Save();

			var service = new OrderManagerConvertService();
			service.FinalizingSupplierBookingShipmentCreation(booking);

			Assert("Container should not be released when it is used properly", container1.JC_JSB_SupplierBooking == booking.PK);
			Assert("Container should not be released when it is used properly", container2.JC_JSB_SupplierBooking == booking.PK);
		}

		public void TestMultipleUnusedCYBookingContainersAreReleasedFromBooking()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder(OrderKey);
			builder.BuildOrderLine(OrderLine1Key, OrderKey);
			builder.BuildOrderLine(OrderLine2Key, OrderKey);

			var booking = builder.BuildSupplierBooking(BookingKey, SupplierBookingStatus.Planned);
			booking.JSB_LoadMode = SupplierBookingLoadMode.ContainerYard;
			builder.BuildSupplierBookingLine(BookingLine1Key, BookingKey, null, OrderLine1Key);
			builder.BuildSupplierBookingLine(BookingLine2Key, BookingKey, null, OrderLine2Key);

			builder.BuildConsol(ConsolKey);

			var clh1 = builder.BuildContainerLoadList(CLH1Key, BookingKey, status: ContainerLoadListHeaderStatus.Placed);

			var container1 = builder.BuildContainer(Container1Key, ConsolKey, BookingKey, null);
			var container2 = builder.BuildContainer(Container2Key, ConsolKey, BookingKey, null);
			var container3 = builder.BuildContainer(Container3Key, ConsolKey, BookingKey, null);

			builder.BuildContainerLoadListLine(CLL1Key, CLH1Key, BookingLine1Key, Container1Key);
			builder.BuildContainerLoadListLine(CLL2Key, CLH1Key, BookingLine1Key, Container1Key);

			Factory.Save();

			var service = new OrderManagerConvertService();
			service.FinalizingSupplierBookingShipmentCreation(booking);

			Assert("Container should not be released when it is used properly", container1.JC_JSB_SupplierBooking == booking.PK);
			Assert("Unused container should have been released from the booking", container2.JC_JSB_SupplierBooking == ZGuid.Empty);
			Assert("Unused container should have been released from the booking", container3.JC_JSB_SupplierBooking == ZGuid.Empty);
		}

		public void TestMultipleUnusedContainersInMultipleCLLsWithDifferentStatusShouldAffectBooking()
		{
			var builder = new OrderManagerMockDataBuilder(Factory);

			builder.BuildOrder(OrderKey);
			builder.BuildOrderLine(OrderLine1Key, OrderKey);
			builder.BuildOrderLine(OrderLine2Key, OrderKey);

			var booking = builder.BuildSupplierBooking(BookingKey, SupplierBookingStatus.Planned);
			builder.BuildSupplierBookingLine(BookingLine1Key, BookingKey, null, OrderLine1Key);
			builder.BuildSupplierBookingLine(BookingLine2Key, BookingKey, null, OrderLine2Key);

			builder.BuildConsol(ConsolKey);

			var clh1 = builder.BuildContainerLoadList(CLH1Key, BookingKey, status: ContainerLoadListHeaderStatus.Placed);
			var clh2 = builder.BuildContainerLoadList(CLH2Key, BookingKey);

			var container1 = builder.BuildContainer(Container1Key, ConsolKey, BookingKey, null);
			var container2 = builder.BuildContainer(Container2Key, ConsolKey, BookingKey, null);
			var container3 = builder.BuildContainer(Container3Key, ConsolKey, BookingKey, null);

			builder.BuildContainerLoadListLine(CLL1Key, CLH1Key, BookingLine1Key, Container1Key);
			builder.BuildContainerLoadListLine(CLL2Key, CLH2Key, BookingLine2Key, Container2Key);

			Factory.Save();

			var service = new OrderManagerConvertService();
			service.FinalizingSupplierBookingShipmentCreation(booking);

			Assert("Container should not be released when it is used properly", container1.JC_JSB_SupplierBooking == booking.PK);
			Assert("Container should not be released when it is used properly", container2.JC_JSB_SupplierBooking == booking.PK);
			Assert("Unused container should have been released from the booking", container3.JC_JSB_SupplierBooking == ZGuid.Empty);
		}

		#endregion

		#region Utility

		void AssertSPTPackLineProperties(ContainerLoadListLine loadListLine, decimal expectedQuantity = 100)
		{
			var convertedPackLine = loadListLine.GetPackLine();
			AssertEquals(convertedPackLine.PK, loadListLine.CLL_JL_PackLine);
			AssertEquals(loadListLine.Container.PK, convertedPackLine.GetContainer(loadListLine.Container.Consol).PK);
			AssertEquals(loadListLine.CLL_Volume, convertedPackLine.JL_ActualVolume);
			AssertEquals(loadListLine.CLL_Weight, convertedPackLine.JL_ActualWeight);
			AssertEquals(loadListLine.CLL_Packages, convertedPackLine.JL_PackageCount);
			Assert(convertedPackLine.Shipment.OuterPackLines.Contains(convertedPackLine));
			var product = convertedPackLine.Products.FirstOrDefault(product => product.D2_JO == loadListLine.SupplierBookingLine.JSL_JO_OrderLine);
			AssertNotNull(product);
			AssertEquals(product.D2_ProductQuantity, expectedQuantity);
		}

		ContainerLoadListLine CreateContainerLoadListLine(CYContainerLoadList containerLoadList = null, decimal packedQuantity = 100)
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.SupplierBooking.JSB_LoadMode = SupplierBookingLoadModeList.Codes.CY;
			bookingLine.JSL_PackHeight = 10;
			bookingLine.JSL_PackLength = 20;
			bookingLine.JSL_PackWidth = 30;
			bookingLine.JSL_PackUnitOfDimension = Core.Constants.Dimension.Feet;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			containerLoadList ??= Factory.NewWithValidTestData<CYContainerLoadList>();

			var loadListLine = containerLoadList.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container.PK;

			AssignQuantitiesToContainerLoadListLine(loadListLine, packedQuantity);

			return loadListLine;
		}

		ContainerLoadListLine CloneContainerLoadListLine(ContainerLoadListLine loadListLine, decimal packedQuantity = 100)
		{
			var clonedLoadListLine = loadListLine.LoadListHeader.LoadListLines.AddNew();
			clonedLoadListLine.CLL_JSL_BookingLine = loadListLine.CLL_JSL_BookingLine;
			clonedLoadListLine.CLL_JC_Container = loadListLine.CLL_JC_Container;

			AssignQuantitiesToContainerLoadListLine(clonedLoadListLine, packedQuantity);

			return clonedLoadListLine;
		}

		void AssignQuantitiesToContainerLoadListLine(ContainerLoadListLine loadListLine, decimal packedQuantity)
		{
			loadListLine.CLL_PackedQuantity = packedQuantity;
			loadListLine.CLL_Packages = (int)packedQuantity;
			loadListLine.CLL_F3_NKPackagesUnit = PkgUnit.Package;
			loadListLine.CLL_Weight = packedQuantity;
			loadListLine.CLL_WeightUnit = Weight.Kilograms;
			loadListLine.CLL_Volume = packedQuantity;
			loadListLine.CLL_VolumeUnit = Volume.CubicDecimetres;
		}

		ForwardingPackLine CreatePlannedPackLine(ContainerLoadListLine loadListLine, decimal quantity = 100, bool isPacked = false)
		{
			return CreatePlannedPackLine(loadListLine, loadListLine.Container.Consol, quantity, isPacked);
		}

		ForwardingPackLine CreatePlannedPackLine(ContainerLoadListLine loadListLine, ForwardingConsol consol, decimal quantity, bool isPacked = false)
		{
			var plannedPackLine = consol.Shipments.AddNew().OuterPackLines.AddNew();

			// In production, user has to remove the container from the pack line.
			// Fix in WI00812174 - GLWOM: Stop assigning continers to OPL planned pack lines
			// After which this can be removed
			plannedPackLine.Containers.RemoveAll();

			if (isPacked)
			{
				loadListLine.CLL_JL_PackLine = plannedPackLine.PK;
			}
			plannedPackLine.JL_JSL_BookingLine = loadListLine.CLL_JSL_BookingLine;
			var product = plannedPackLine.Products.AddNew();
			product.D2_JO = loadListLine.SupplierBookingLine.JSL_JO_OrderLine;
			product.D2_ProductQuantity = quantity;
			return plannedPackLine;
		}

		void AssertMismatchedPackingCompletedEvent(bool shouldAdd, JobSupplierBooking booking, params ForwardingShipment[] shipments)
		{
			var expectedShipmentIDs = string.Join(",", shipments.Select(s => s.JS_UniqueConsignRef).OrderBy(x => x));
			AssertEquals(shouldAdd ? 1 : 0, booking.Logs.GetAllLogs()
				.Where(log => log.SL_SE_NKEvent == "PKC" && log.SL_Reference == "|RFN=" + expectedShipmentIDs + "|TYP=Mismatched")
				.Count());
		}

		#endregion
	}

	static class ContainerLoadListLineExtension
	{
		public static ForwardingPackLine GetPackLine(this ContainerLoadListLine loadListLine)
		{
			return loadListLine.Factory.Load<ForwardingPackLine>(loadListLine.CLL_JL_PackLine);
		}
	}
}
