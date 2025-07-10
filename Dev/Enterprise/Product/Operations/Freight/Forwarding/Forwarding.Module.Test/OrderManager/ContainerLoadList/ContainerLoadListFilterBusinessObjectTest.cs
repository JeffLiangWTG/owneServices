using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ContainerLoadListFilterBusinessObject))]
	class ContainerLoadListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerLoadListFilterBusinessObject();
		}

		public void TestCLH_LoadListId()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load List #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CLH002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_BookingId()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Supplier Booking #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "JSB003";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_TransportMode()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_TransportMode = Constants.TransportModes.Sea);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_TransportMode = Constants.TransportModes.Air);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_TransportMode = Constants.TransportModes.Road);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Transport Mode"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.TransportModes.Sea;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB001", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD001", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_IncoTerm()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_IncoTerm = "ABC");
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_IncoTerm = "BCE");
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => booking.JSB_IncoTerm = "CED");

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["INCO Term"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ABC";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB001", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD001", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJK_UniqueConsignRef()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Consol #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CON001";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB001", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD001", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJK_BookingReference()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001", bookingReference: "BKR001"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002", bookingReference: "BKR002"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003", bookingReference: "BKR003"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Booking Reference"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BKR003";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJK_MasterBillNum()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001", masterBillNum: "BLL001"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002", masterBillNum: "BLL002"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003", masterBillNum: "BLL003"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Master Bill"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BLL002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJK_Carrier()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001").Consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002").Consol.JK_OA_ShippingLineAddress = carrier2.MainAddress.PK);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003").Consol.JK_OA_ShippingLineAddress = carrier3.MainAddress.PK);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Carrier"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = carrier1.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH001", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB001", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD001", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJC_ContainerNum()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Container #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CNT002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJC_ContainerType()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001", "20GP"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002", "40GP"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003", "20FR"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Container Type"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "20FR";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJC_SealNum()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001", sealNum: "SN001"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002", sealNum: "SN002"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003", sealNum: "SN003"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Seal Number"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SN002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestCLH_OH_LoadListParty()
		{
			var loadListParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var loadListParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var loadListParty3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (containerLoadList, _, __) => containerLoadList.CLH_OH_LoadListParty = loadListParty1.PK);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (containerLoadList, _, __) => containerLoadList.CLH_OH_LoadListParty = loadListParty2.PK);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (containerLoadList, _, __) => containerLoadList.CLH_OH_LoadListParty = loadListParty3.PK);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load List Party"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = loadListParty3.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestCLH_Status()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Approved);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Placed);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load List Status"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.ContainerLoadListHeaderStatus.Approved;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestControllingCustomerCompanyName()
		{
			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer1.OH_FullName = "SP01";
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer2.OH_FullName = "SP02";
			var controllingCustomer3 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer3.OH_FullName = "SP03";
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer1));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer2));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Controlling Customer Company Name"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SP02";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestControllingCustomer()
		{
			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer3 = Factory.NewWithValidTestData<OrgHeader>();
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer1));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer2));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => FillDocAddress(booking.ControllingCustomerAddress, controllingCustomer3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Controlling Customer"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = controllingCustomer2.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestSupplier()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();

			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier1));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier2));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Supplier"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = supplier3.PK;

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestSupplierCompanyName()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "SP01";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "SP02";
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_FullName = "SP03";

			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier1));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier2));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, order) => FillDocAddress(booking.SupplierAddress, supplier3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Supplier Company Name"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SP02";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestBookLoadAndDischargePort()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => { booking.JSB_RL_NKLoadPort = "THBKK"; booking.JSB_RL_NKDischargePort = "AUSYD"; });
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => { booking.JSB_RL_NKLoadPort = "CNSZX"; booking.JSB_RL_NKDischargePort = "AUSYD"; });
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => { booking.JSB_RL_NKLoadPort = "CNSZX"; booking.JSB_RL_NKDischargePort = "SGSIN"; });

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load / Discharge (Booking)"] as ModuleLocationFilter;
			filter.IsActive = true;
			filter.Property1 = "CNSZX";
			filter.Property2 = "AUSYD";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestConsolLoadAndDischargePort()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON001", "CNT001", loadPort: "THBKK", dischargePort: "AUSYD"));
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON002", "CNT002", loadPort: "CNSZX", dischargePort: "AUSYD"));
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete, (_, booking, __) => CreateContainer(booking, "CON003", "CNT003", loadPort: "CNSZX", dischargePort: "SGSIN"));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load / Discharge (Carrier)"] as ModuleLocationFilter;
			filter.IsActive = true;
			filter.Property1 = "CNSZX";
			filter.Property2 = "SGSIN";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJD_OrderNumber()
		{
			CreateContainerLoadList("ORD001", "", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD002", "", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD003", "", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Order No"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ORD003";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH003", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB003", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD003", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJO_Partno()
		{
			CreateContainerLoadList("ORD001", "PTN001", "JSB001", "CLH001", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD002", "PTN002", "JSB002", "CLH002", Constants.ContainerLoadListHeaderStatus.Incomplete);
			CreateContainerLoadList("ORD003", "PTN003", "JSB003", "CLH003", Constants.ContainerLoadListHeaderStatus.Incomplete);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Part No"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PTN002";

			var loadLists = new CommonContainerLoadListCollection(Factory);
			loadLists.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, loadLists.Count);
			AssertEquals("CLH002", loadLists[0].CLH_LoadListId);
			AssertEquals("JSB002", loadLists[0].Booking.JSB_BookingId);
			AssertEquals("ORD002", loadLists[0].Booking.SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		void FillDocAddress(JobDocAddress docAddress, OrgHeader org)
		{
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = org.Addresses[0].PK;
		}

		public ForwardingContainer CreateContainer(JobSupplierBooking booking, string consolId, string containerNumber, string containerType = "20GP", string bookingReference = "", string masterBillNum = "", string sealNum = "", string loadPort = "", string dischargePort = "")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolId;
			consol.JK_BookingReference = bookingReference;
			consol.JK_MasterBillNum = masterBillNum;

			if (!string.IsNullOrWhiteSpace(loadPort))
			{
				consol.JK_RL_NKLoadPort = loadPort;
			}

			if (!string.IsNullOrWhiteSpace(dischargePort))
			{
				consol.JK_RL_NKDischargePort = dischargePort;
			}

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_SealNum = sealNum;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			container.JC_JSB_SupplierBooking = booking.PK;

			return container;
		}

		public CYContainerLoadList CreateContainerLoadList(string orderNumber, string partNo, string bookingId, string loadListId, string containerLoadListStatus, Action<CYContainerLoadList, JobSupplierBooking, Order> extraInit = null)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			order.JD_OrderNumber = orderNumber;
			orderLine.JO_Quantity = 20;
			orderLine.JO_Partno = partNo;
			orderLine.JO_JD = order.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			booking.JSB_Status = Constants.SupplierBookingStatus.Planned;
			booking.JSB_BookingId = bookingId;
			booking.JSB_LoadMode = Constants.SupplierBookingLoadMode.ContainerYard;

			var containerLoadList = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList.CLH_Status = containerLoadListStatus;
			containerLoadList.CLH_LoadListId = loadListId;
			containerLoadList.CLH_JSB_Booking = booking.PK;

			var containerLoadListLine = containerLoadList.LoadListLines.AddNew();
			containerLoadListLine.CLL_JSL_BookingLine = bookingLine.PK;

			extraInit?.Invoke(containerLoadList, booking, order);

			return containerLoadList;
		}

		#region TestWorkflowFiltersAreNotAddedTwice

		public void TestWorkflowFiltersAreNotAddedTwice()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode;
			template.P0_IsActive = true;
			template.P0_Name = "WFTesting";

			var customColumn = Factory.New<GenCustomColumnDefinition>();
			customColumn.XC_ParentID = template.PK;
			customColumn.XC_ParentTableCode = "P0";
			customColumn.XC_Type = "INT";
			customColumn.XC_Name = "WFCustomField";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			filterBizo.LoadModuleFilters();

			AssertNull("We shouldn't be duplicating this custom field", filterBizo["WFCustomField (WF)"]);
		}

		#endregion
	}
}
