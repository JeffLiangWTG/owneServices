using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(ViewTransportBookingParentLegs))]
	public class ViewTransportBookingParentLegsTest : DtbBookingBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}

		public void TestView_StandaloneBooking()
		{
			var tbConsolNoParent = Helper.CreateConsolidation();
			var booking = CreateBooking(tbConsolNoParent, new[] { new Inst("DLV", "CNE") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 transport booking schedule (BTC).", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.KM_KB_Booking, "BTC", "", "", "");

			// Standalone Booking can add their own Routing, but it is currently not supported.
			// Will be added in a new Feature Request WI.
		}

		public void TestView_AgencyShipment()
		{
			var agencyShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipment[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			agencyShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var sailing = AddJobSailingData((IForwardingShipment)agencyShipment, "AUBNE", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)agencyShipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result[0], booking.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
		}

		public void TestView_AgencyShipment_UsesCorrectSailing()
		{
			var agencyShipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipment1[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment1[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			agencyShipment1[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var sailing1 = AddJobSailingData((IForwardingShipment)agencyShipment1, "AUBNE", "USLAX");

			var tbConsol1 = Helper.CreateConsolidation((IDtbBookingParent)agencyShipment1);
			var booking1 = CreateBooking(tbConsol1, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });

			var agencyShipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipment2[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment2[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			agencyShipment2[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var sailing2 = AddJobSailingData((IForwardingShipment)agencyShipment2, "AUBNE", "USLAX");

			var tbConsol2 = Helper.CreateConsolidation((IDtbBookingParent)agencyShipment2);
			var booking2 = CreateBooking(tbConsol2, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });

			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 2 rows for the two bookings, not four which would result from a cartesian product.", 2, result.Count);
			if ((ZGuid)result[0]["VL_PK"] == booking1.ParentJob.PK)
			{
				AssertViewResultRowData(result[0], booking1.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
				AssertViewResultRowData(result[1], booking2.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
			}
			else
			{
				AssertViewResultRowData(result[1], booking1.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
				AssertViewResultRowData(result[0], booking2.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
			}
		}

		public void TestView_BookingShipment()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var sailing = AddJobSailingData((IForwardingShipment)bookingShipment, "AUBNE", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUBNE", "USLAX", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be true.", ZBool.True, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_CancelledBookingShipment()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			bookingShipment[JobShipmentSchema.JS_IsCancelled] = 1;

			var sailing = AddJobSailingData((IForwardingShipment)bookingShipment, "AUBNE", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return 1 transport booking schedule with BTC type.", 1, results.Count);

			var resultRow = results.Single();
			AssertEquals("Type is BTC", "BTC", resultRow["VL_JobType"]);
		}

		public void TestView_PortOfOriginMatchesSailingLoadingPort_PortOfDestinationDoesNotMatchSailingDischargePort_Import()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			AddJobSailingData((IForwardingShipment)bookingShipment, "AUBNE", "AUMEL");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = "DLV";

			var booking = CreateBooking(tbConsol, new[] { new Inst("DLV", "CFS") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUBNE", "AUMEL", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be false.", ZBool.False, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_PortOfOriginMatchesSailingLoadingPort_PortOfDestinationDoesNotMatchSailingDischargePort_Export()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			AddJobSailingData((IForwardingShipment)bookingShipment, "AUBNE", "AUMEL");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = "PIC";

			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return nothing.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUBNE", "AUMEL", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be true.", ZBool.True, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_PortOfOriginDoesNotMatchSailingLoadingPort_PortOfDestinationMatchesSailingDischargePort_Import()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			AddJobSailingData((IForwardingShipment)bookingShipment, "AUMEL", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = "DLV";

			var booking = CreateBooking(tbConsol, new[] { new Inst("DLV", "CFS") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return nothing.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUMEL", "USLAX", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be true.", ZBool.True, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_PortOfOriginDoesNotMatchSailingLoadingPort_PortOfDestinationMatchesSailingDischargePort_Export()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			AddJobSailingData((IForwardingShipment)bookingShipment, "AUMEL", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = "PIC";

			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUMEL", "USLAX", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be false.", ZBool.False, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_PortOfOriginDoesNotMatchSailingLoadingPort_PortOfDestinationDoesNotMatchSailingDischargePort()
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			AddJobSailingData((IForwardingShipment)bookingShipment, "AUMEL", "AUSYD");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = "PIC";

			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var results = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, results.Count);
			AssertViewResultRowData(results.Single(), booking.ParentJob.PK, "ASH", "AUMEL", "AUSYD", "");
			AssertEquals("VL_PortsMatchBasedOnBookingInstruction should be false.", ZBool.False, results.Single()["VL_PortsMatchBasedOnBookingInstruction"]);
		}

		public void TestView_ForwardingShipment_NoConsol()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";

			var sailing = AddJobSailingData((IForwardingShipment)shipment, "AUBNE", "USLAX");

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.ParentJob.PK, "SHP", "AUBNE", "USLAX", "");
		}

		public void TestView_ForwardingShipment_SingleConsol()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport = AddTransportDataToConsol(consol, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "USLAX");
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol);

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.ParentJob.PK, "SHP", "AUBNE", "USLAX", "001");
		}

		public void TestView_ForwardingShipment_MultipleConsols()
		{
			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport1 = AddTransportDataToConsol(consol1, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			consol1[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol1[JobConsolSchema.JK_RL_NKDischargePort] = "NLAMS";

			var consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport2 = AddTransportDataToConsol(consol2, TransportModes.Sea, "BANOWATI", "002", "NLAMS", "USLAX");
			consol2[JobConsolSchema.JK_RL_NKLoadPort] = "NLAMS";
			consol2[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol1);
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol2);

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.ParentJob.PK, "SHP", "AUBNE", "NLAMS", "001");
		}

		public void TestView_ForwardingShipment_MultipleConsols_UsesCorrectJobConShipLink()
		{
			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport1 = AddTransportDataToConsol(consol1, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			consol1[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol1[JobConsolSchema.JK_RL_NKDischargePort] = "NLAMS";

			var consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport2 = AddTransportDataToConsol(consol2, TransportModes.Sea, "BANOWATI", "002", "NLAMS", "USLAX");
			consol2[JobConsolSchema.JK_RL_NKLoadPort] = "NLAMS";
			consol2[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol2);

			var tbConsol1 = Helper.CreateConsolidation((IDtbBookingParent)shipment1);
			var booking1 = CreateBooking(tbConsol1, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });

			var consol3 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport3 = AddTransportDataToConsol(consol3, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			consol3[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol3[JobConsolSchema.JK_RL_NKDischargePort] = "NLAMS";

			var consol4 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport4 = AddTransportDataToConsol(consol4, TransportModes.Sea, "BANOWATI", "002", "NLAMS", "USLAX");
			consol4[JobConsolSchema.JK_RL_NKLoadPort] = "NLAMS";
			consol4[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment2[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol3);
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol4);

			var tbConsol2 = Helper.CreateConsolidation((IDtbBookingParent)shipment2);
			var booking2 = CreateBooking(tbConsol2, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 2 rows for the two bookings, not a cartesian product of 4.", 2, result.Count);
			if ((ZGuid)result[0]["VL_PK"] == booking1.ParentJob.PK)
			{
				AssertViewResultRowData(result[0], booking1.ParentJob.PK, "SHP", "AUBNE", "NLAMS", "001");
				AssertViewResultRowData(result[1], booking2.ParentJob.PK, "SHP", "AUBNE", "NLAMS", "001");
			}
			else
			{
				AssertViewResultRowData(result[1], booking1.ParentJob.PK, "SHP", "AUBNE", "NLAMS", "001");
				AssertViewResultRowData(result[0], booking2.ParentJob.PK, "SHP", "AUBNE", "NLAMS", "001");
			}
		}

		public void TestView_ForwardingShipment_WasBooking()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_IsForwardRegistered] = true;
			shipment[JobShipmentSchema.JS_IsBooking] = true;
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol);

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result[0], booking.ParentJob.PK, "SHP", "AUBNE", "USLAX", "");
		}

		public void TestView_WhsReceive()
		{
			var whsReceive = (BusinessObject)Factory.New<IWhsReceive>();
			whsReceive.FillWithValidTestData();

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)whsReceive);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			booking.ConsolidationSingleJob.KB_ParentID = whsReceive.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 transport booking schedule with BTC type.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.KM_KB_Booking, "BTC", "", "", "");
		}

		public void TestView_WhsOrder()
		{
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)whsOrder);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			booking.ConsolidationSingleJob.KB_ParentID = whsOrder.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 transport booking schedule with BTC type.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.KM_KB_Booking, "BTC", "", "", "");
		}

		public void TestView_CustomsDeclaration()
		{
			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)customsDeclaration);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			booking.ConsolidationSingleJob.KB_ParentID = customsDeclaration.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 0 transport booking schedules", 0, result.Count);
		}

		public void TestView_CancelledCustomsDeclarationWithoutJobShipmentShouldShowBTC()
		{
			var eta = new ZDateTime(DateTime.Now.Year, 1, 1);
			var etd = new ZDateTime(DateTime.Now.Year, 1, 12);

			var origin = "AUBNE";
			var destination = "USLAX";

			var parentType = "DEC";

			var customsDeclaration = CreateCustomsDeclaration(eta, etd, origin, destination, parentType, "PIC");
			customsDeclaration[JobDeclarationSchema.Constants.JE_IsCancelled] = 1;
			Factory.Save();

			var result = GetViewResultSet();

			AssertEquals("Must return 1 transport booking schedule with BTC type.", 1, result.Count);

			var resultRow = result.Single();
			AssertEquals("Type is BTC", "BTC", resultRow["VL_JobType"]);
		}

		public void TestView_CustomsDeclarationWithoutJobShipment()
		{
			var eta = new ZDateTime(DateTime.Now.Year, 1, 1);
			var etd = new ZDateTime(DateTime.Now.Year, 1, 12);

			var origin = "AUBNE";
			var destination = "USLAX";

			var parentType = "DEC";

			var customsDeclaration = CreateCustomsDeclaration(eta, etd, origin, destination, parentType, "PIC");
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals($"Must return 1 transport booking schedule with {parentType} type.", 1, result.Count);
			var resultRow = result.Single();
			AssertViewResultRowData(resultRow, customsDeclaration.PK, parentType, origin, destination, "TestFlight");

			AssertEquals("ETA", eta, resultRow["VL_ETA"]);
			AssertEquals("ETD", etd, resultRow["VL_ETD"]);
			AssertEquals("TestVessel", "TestVessel", resultRow["VL_Vessel"]);
			AssertEquals("SEA", "SEA", resultRow["VL_TransportMode"]);
		}

		public void TestView_CustomsDeclarationWithoutJobShipmentNonMatchingOrigin()
		{
			var customsDeclaration = CreateCustomsDeclarationBasic("PIC");

			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "AUSYD";

			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals($"Must return 0 transport booking schedules", 0, result.Count);
		}

		public void TestView_CustomsDeclarationWithoutJobShipmentNonMatchingDestination()
		{
			var customsDeclaration = CreateCustomsDeclarationBasic("DLV");

			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "AUSYD";

			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals($"Must return 0 transport booking schedules", 0, result.Count);
		}

		public void TestView_CustomsDeclarationWithoutJobShipmentBlankOrigin()
		{
			var customsDeclaration = CreateCustomsDeclarationBasic("PIC");

			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = string.Empty;

			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals($"Must return 0 transport booking schedules", 0, result.Count);
		}

		public void TestView_CustomsDeclarationWithoutJobShipmentBlankDestination()
		{
			var customsDeclaration = CreateCustomsDeclarationBasic("DLV");

			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = string.Empty;

			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals($"Must return 0 transport booking schedules", 0, result.Count);
		}

		public void TestView_ForwardingConsol()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.ParentJob.PK, "CON", "AUBNE", "USLAX", "");
		}

		public void TestView_ForwardingConsol_NonMatchingOrigin()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking = CreateBooking(tbConsol, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") });
			Factory.Save();

			var jobConsolTransportQuery = new ZQuery(JobConsolTransportSchema.JW_ParentGUID, consol.PK);
			var jobConsolTransport = Factory.LoadTop1<ITransport>(jobConsolTransportQuery);
			jobConsolTransport.JW_RL_NKLoadPort = "AUSYD";
			Factory.Save();

			var result = GetViewResultSet();
			AssertEquals("Must return 1 row for the booking.", 1, result.Count);
			AssertViewResultRowData(result.Single(), booking.ParentJob.PK, "CON", "", "", "");
		}

		BusinessObject CreateCustomsDeclarationBasic(string direction)
		{
			var eta = new ZDateTime(DateTime.Now.Year, 1, 1);
			var etd = new ZDateTime(DateTime.Now.Year, 1, 12);

			var origin = "AUBNE";
			var destination = "USLAX";

			var parentType = "DEC";

			var customsDeclaration = CreateCustomsDeclaration(eta, etd, origin, destination, parentType, direction);
			return customsDeclaration;
		}

		BusinessObject CreateCustomsDeclaration(ZDateTime eta, ZDateTime etd, string origin, string destination, string parentType, string direction)
		{
			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();

			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = origin;
			customsDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = origin;
			customsDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = destination;
			customsDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = destination;

			var bookingConsol = Helper.CreateConsolidation((IDtbBookingParent)customsDeclaration);
			bookingConsol.KB_JobDirection = direction;

			var jobConsolTransport = (BusinessObject)Factory.New<ITransport>();
			jobConsolTransport.FillWithValidTestData();

			((ITransport)jobConsolTransport).ParentType = customsDeclaration.GetType();
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_ParentGUID] = customsDeclaration.PK;
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_ParentType] = parentType;

			jobConsolTransport[JobConsolTransportSchema.Constants.JW_RL_NKLoadPort] = origin;
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_RL_NKDiscPort] = destination;

			jobConsolTransport[JobConsolTransportSchema.Constants.JW_TransportMode] = "SEA";
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_Vessel] = "TestVessel";
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_VoyageFlight] = "TestFlight";

			jobConsolTransport[JobConsolTransportSchema.Constants.JW_ETA] = eta;
			jobConsolTransport[JobConsolTransportSchema.Constants.JW_ETD] = etd;

			return customsDeclaration;
		}

		void AssertViewResultRowData(DynamicBusinessObject resultRow, ZGuid expectedBookingPK, string expectedJobType, string expectedLoadPort, string expectedDischargePort, string expectedFlightVoyage)
		{
			CombineAssertions(() =>
			{
				AssertEquals("The booking parent must correspond with VL_PK.", expectedBookingPK, resultRow["VL_PK"]);
				AssertEquals("Job Type: ", expectedJobType, resultRow["VL_JobType"]);
				AssertEquals("Load Port: ", expectedLoadPort, resultRow["VL_RL_NKLoad"]);
				AssertEquals("Discharge Port: ", expectedDischargePort, resultRow["VL_RL_NKDischarge"]);
				AssertEquals("Flight/Voyage: ", expectedFlightVoyage, resultRow["VL_VoyageFlight"]);
			});
		}

		DynamicBusinessObjectCollection GetViewResultSet()
		{
			var sql = "SELECT * FROM dbo.ViewTransportBookingParentLegs";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		DtbBooking CreateBooking(DtbBookingConsolidation consol, Inst[] instructions, string houseBill = "", string masterBill = "")
		{
			var booking = Helper.CreateBooking(consol);
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, houseBill);
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, masterBill);

			var packageJob = booking.PackageJob;
			var package1 = Helper.CreatePackage("P123", 1, "BOX");
			var package10 = Helper.CreatePackage("", 10, "PLT");
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package10.KP_KJ_ParentPackageJob = packageJob.PK;

			foreach (var instructionSetup in instructions)
			{
				var instruction = Helper.CreateInstruction(booking, instructionSetup.DirectionType, instructionSetup.OrgType, Org.MainAddress);
				Helper.CreatePackageDivot(instruction, package1);
				Helper.CreatePackageDivot(instruction, package10, 6);

				foreach (var confirmationType in instructionSetup.ConfirmationTypes)
				{
					if (!instruction.Confirmations.Any(c => c.KK_ConfirmationType == confirmationType))
					{
						var confirmation = Helper.CreateConfirmation(instruction, confirmationType);
					}
				}
			}

			return booking;
		}

		BusinessObject AddTransportDataToConsol(BusinessObject consol, string transportMode, string vessel, string voyageFlight, string loadPort, string dischargePort, int transportsIndex = 0)
		{
			var now = ZDateTime.Now;
			var transport = (BusinessObject)((IBusinessObjectCollection)consol["Transports"])[transportsIndex];

			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_TransportMode] = transportMode;
			transport[JobConsolTransportSchema.JW_Vessel] = vessel;
			transport[JobConsolTransportSchema.JW_VoyageFlight] = voyageFlight;
			transport[JobConsolTransportSchema.JW_RL_NKLoadPort] = loadPort;
			transport[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport[JobConsolTransportSchema.JW_RL_NKDiscPort] = dischargePort;
			transport[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			return transport;
		}

		IJobSailing AddJobSailingData(IForwardingShipment job, string originPort, string destinationPort)
		{
			var voyage = (BusinessObject)Factory.New<IJobVoyage>();
			var origin = (BusinessObject)Factory.New<IVoyageOrigin>();
			origin[JobVoyOriginSchema.JA_E_DEP] = ZDateTime.Now.AddDays(-5);
			origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = originPort;

			var destination = (BusinessObject)Factory.New<IVoyageDestination>();
			destination[JobVoyDestinationSchema.JB_E_ARV] = ZDateTime.Now;
			destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = destinationPort;

			var sailing = (BusinessObject)Factory.New<IJobSailing>();
			sailing[JobSailingSchema.JX_JA] = origin.PK;
			sailing[JobSailingSchema.JX_JB] = destination.PK;

			var shipment = (BusinessObject)job;
			shipment[JobShipmentSchema.JS_JX] = sailing.PK;

			return (IJobSailing)sailing;
		}

		struct Inst
		{
			public Inst(ZString direction, ZString orgType)
			{
				DirectionType = direction;
				OrgType = orgType;
				ConfirmationTypes = DirectionType != "MLT" ? new[] { DirectionType } : new ZString[] { "DLV", "PIC" };
			}

			public readonly ZString DirectionType;
			public readonly ZString OrgType;
			public readonly ZString[] ConfirmationTypes;
		}

		OrgHeader Org
		{
			get { return org ?? (org = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader org;
	}
}
