using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobSupplierBookingFilterBusinessObject))]
	class JobSupplierBookingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobSupplierBookingFilterBusinessObject();
		}

		public void TestJSB_BookingId()
		{
			CreateSupplierBooking("ORD001", "", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD002", "", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD003", "", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Booking #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "JSB002";

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_Status()
		{
			CreateSupplierBooking("ORD001", "", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD002", "", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved);
			CreateSupplierBooking("ORD003", "", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Booking Status"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.SupplierBookingStatus.Rejected;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
			AssertEquals("JSB003", bookings[0].JSB_BookingId);
			AssertEquals("ORD003", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJO_Partno()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved);
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Part No"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PN001";

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
			AssertEquals("JSB001", bookings[0].JSB_BookingId);
			AssertEquals("ORD001", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_LoadMode()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved);
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load Mode"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.SupplierBookingLoadMode.ContainerYard;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_TransportMode()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => x.JSB_TransportMode = Constants.TransportModes.Sea);
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => x.JSB_TransportMode = Constants.TransportModes.Air);
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => x.JSB_TransportMode = Constants.TransportModes.Road);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Transport Mode"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Constants.TransportModes.Road;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
			AssertEquals("JSB003", bookings[0].JSB_BookingId);
			AssertEquals("ORD003", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_RL_NKLoadPortAndJSB_RL_NKDischargePort()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => { x.JSB_RL_NKLoadPort = "THBKK"; x.JSB_RL_NKDischargePort = "AUSYD"; });
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => { x.JSB_RL_NKLoadPort = "CNSZX"; x.JSB_RL_NKDischargePort = "AUSYD"; });
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => { x.JSB_RL_NKLoadPort = "THBKK"; x.JSB_RL_NKDischargePort = "SGSIN"; });

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Load / Discharge"] as ModuleLocationFilter;
			filter.IsActive = true;
			filter.Property1 = "CNSZX";
			filter.Property2 = "AUSYD";

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_BookedOnDate()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => x.JSB_BookedOnDate = new CargoWise.Types.ZDate(2023, 4, 8));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => x.JSB_BookedOnDate = new CargoWise.Types.ZDate(2023, 5, 8));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => x.JSB_BookedOnDate = new CargoWise.Types.ZDate(2023, 6, 8));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Booking Date"] as ModuleDateFilter;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new CargoWise.Types.ZDateTime(2023, 5, 8);
			filter.Property2 = new CargoWise.Types.ZDateTime(2023, 5, 10);

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestControllingCustomerCompanyName()
		{
			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer1.OH_FullName = "SP01";
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer2.OH_FullName = "SP02";
			var controllingCustomer3 = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer3.OH_FullName = "SP03";
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer1));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer2));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer3));
			CreateSupplierBooking("ORD004", "PN004", "JSB004", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => OverrideDocAddress(x.ControllingCustomerAddress, "SP04"));
			CreateSupplierBooking("ORD005", "PN005", "JSB005", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => OverrideDocAddress(x.ControllingCustomerAddress, "SP05"));

			Factory.Save();
			{
				var filterBizo = GetNewFilterStripBusinessObject();
				var filter = filterBizo["Controlling Customer Company Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "SP02";

				var bookings = new JobSupplierBookingCollection(Factory);
				bookings.AdditionalFilter = filterBizo.Filter;
				AssertEquals(1, bookings.Count);
				AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
				AssertEquals("JSB002", bookings[0].JSB_BookingId);
				AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
			}

			{
				var filterBizo = GetNewFilterStripBusinessObject();
				var filter = filterBizo["Controlling Customer Company Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "SP05";

				var bookings = new JobSupplierBookingCollection(Factory);
				bookings.AdditionalFilter = filterBizo.Filter;
				AssertEquals(1, bookings.Count);
				AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
				AssertEquals("JSB005", bookings[0].JSB_BookingId);
				AssertEquals("ORD005", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
			}
		}

		public void TestControllingCustomer()
		{
			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer3 = Factory.NewWithValidTestData<OrgHeader>();
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer1));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer2));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => FillDocAddress(x.ControllingCustomerAddress, controllingCustomer3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Controlling Customer"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = controllingCustomer2.PK;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestBookingParty()
		{
			var bookParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var bookParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var bookParty3 = Factory.NewWithValidTestData<OrgHeader>();
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => x.JSB_OH_BookingParty = bookParty1.PK);
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => x.JSB_OH_BookingParty = bookParty2.PK);
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => x.JSB_OH_BookingParty = bookParty3.PK);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Booking Party"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = bookParty2.PK;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestSupplierCompanyName()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "SP01";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "SP02";
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			supplier3.OH_FullName = "SP03";
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => FillDocAddress(x.SupplierAddress, supplier1));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => FillDocAddress(x.SupplierAddress, supplier2));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => FillDocAddress(x.SupplierAddress, supplier3));
			CreateSupplierBooking("ORD004", "PN004", "JSB004", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => OverrideDocAddress(x.SupplierAddress, "SP04"));
			CreateSupplierBooking("ORD005", "PN005", "JSB005", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => OverrideDocAddress(x.SupplierAddress, "SP05"));

			Factory.Save();
			{
				var filterBizo = GetNewFilterStripBusinessObject();
				var filter = filterBizo["Supplier Company Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "SP02";

				var bookings = new JobSupplierBookingCollection(Factory);
				bookings.AdditionalFilter = filterBizo.Filter;
				AssertEquals(1, bookings.Count);
				AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
				AssertEquals("JSB002", bookings[0].JSB_BookingId);
				AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
			}

			{
				var filterBizo = GetNewFilterStripBusinessObject();
				var filter = filterBizo["Supplier Company Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "SP05";

				var bookings = new JobSupplierBookingCollection(Factory);
				bookings.AdditionalFilter = filterBizo.Filter;
				AssertEquals(1, bookings.Count);
				AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
				AssertEquals("JSB005", bookings[0].JSB_BookingId);
				AssertEquals("ORD005", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
			}
		}

		public void TestSupplier()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => FillDocAddress(x.SupplierAddress, supplier1));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => FillDocAddress(x.SupplierAddress, supplier2));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => FillDocAddress(x.SupplierAddress, supplier3));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Supplier"] as ModuleGuidFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = supplier2.PK;

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_CargoAvailableDate()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => x.JSB_CargoAvailableDate = new CargoWise.Types.ZDate(2023, 4, 8));
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => x.JSB_CargoAvailableDate = new CargoWise.Types.ZDate(2023, 5, 8));
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => x.JSB_CargoAvailableDate = new CargoWise.Types.ZDate(2023, 6, 8));

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Cargo Available Date"] as ModuleDateFilter;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new CargoWise.Types.ZDateTime(2023, 5, 8);
			filter.Property2 = new CargoWise.Types.ZDateTime(2023, 5, 10);

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerYard, bookings[0].JSB_LoadMode);
			AssertEquals("JSB002", bookings[0].JSB_BookingId);
			AssertEquals("ORD002", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJSB_IncoTerm()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete, x => x.JSB_IncoTerm = "ABC");
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved, x => x.JSB_IncoTerm = "BCD");
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected, x => x.JSB_IncoTerm = "CDE");

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["INCO Term"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CDE";

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
			AssertEquals("JSB003", bookings[0].JSB_BookingId);
			AssertEquals("ORD003", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		public void TestJD_OrderNumber()
		{
			CreateSupplierBooking("ORD001", "PN001", "JSB001", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Incomplete);
			CreateSupplierBooking("ORD002", "PN002", "JSB002", Constants.SupplierBookingLoadMode.ContainerYard, Constants.SupplierBookingStatus.Approved);
			CreateSupplierBooking("ORD003", "PN003", "JSB003", Constants.SupplierBookingLoadMode.ContainerFreightStation, Constants.SupplierBookingStatus.Rejected);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var filter = filterBizo["Order #"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ORD003";

			var bookings = new JobSupplierBookingCollection(Factory);
			bookings.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, bookings.Count);
			AssertEquals(Constants.SupplierBookingLoadMode.ContainerFreightStation, bookings[0].JSB_LoadMode);
			AssertEquals("JSB003", bookings[0].JSB_BookingId);
			AssertEquals("ORD003", bookings[0].SupplierBookingLines[0].OrderLine.Order.JD_OrderNumber);
		}

		void FillDocAddress(JobDocAddress docAddress, OrgHeader org)
		{
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = org.Addresses[0].PK;
		}

		void OverrideDocAddress(JobDocAddress docAddress, string companyName)
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = companyName;
		}

		public JobSupplierBooking CreateSupplierBooking(string orderNumber, string partNo, string bookingId, string loadMode, string bookingStatus, Action<JobSupplierBooking> extraInit = null)
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			order.JD_OrderNumber = orderNumber;
			orderLine.JO_Quantity = 20;
			orderLine.JO_Partno = partNo;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_TransportMode = Constants.TransportModes.Sea;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			//bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			booking.JSB_Status = bookingStatus;
			booking.JSB_BookingId = bookingId;
			booking.JSB_LoadMode = loadMode;

			extraInit?.Invoke(booking);

			return booking;
		}
	}
}
