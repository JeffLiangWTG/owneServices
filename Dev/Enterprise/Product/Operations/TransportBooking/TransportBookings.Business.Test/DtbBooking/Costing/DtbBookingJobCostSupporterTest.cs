using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingJobCostSupporterTest : TestCaseWithFactory
	{
		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForTBWithConsolParent()
		{
			var (forwardingConsolParent, shipment1, shipment2) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a booking with Forwarding Consol parent should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be true", supporter.HasConsolParent);
				AssertEquals("Supporter PK should match PK of booking", booking.PK, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertContainsExactElementsInAnyOrder("Supporter ShipmentsListPKs should match consol shipments", new[] { shipment1.PK, shipment2.PK }, supporterAsIGenericCostSupporter.ShipmentsListPKs);
				AssertContainsExactElementsInAnyOrder("Supporter ShipmentsList should match consol shipments", new[] { shipment1, shipment2 }, supporterAsIGenericCostSupporter.ShipmentsList.Cast<BusinessObject>().ToArray());
				AssertContainsExactElementsInAnyOrder("Supporter Shipments should match consol shipments", new[] { shipment1, shipment2 }, supporterAsIGenericCostSupporter.Shipments.Cast<BusinessObject>().ToArray());
			});
		}

		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForTBWithForwardingShipmentParent()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a booking with Forwarding Shipment parent should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be false", !supporter.HasConsolParent);
				AssertEquals("Supporter PK should be empty", ZGuid.Empty, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertEquals("Supporter ShipmentsListPKs should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsListPKs.Length);
				AssertEquals("Supporter ShipmentsList should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsList.Length);
				AssertNull("Supporter Shipments should be null", supporterAsIGenericCostSupporter.Shipments);
			});
		}

		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForTBWithCustomsDeclarationParent()
		{
			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)customsDeclaration);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a booking with Customs Declaration parent should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be false", !supporter.HasConsolParent);
				AssertEquals("Supporter PK should be empty", ZGuid.Empty, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertEquals("Supporter ShipmentsListPKs should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsListPKs.Length);
				AssertEquals("Supporter ShipmentsList should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsList.Length);
				AssertNull("Supporter Shipments should be null", supporterAsIGenericCostSupporter.Shipments);
			});
		}

		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForTBWithWhsOrderParent()
		{
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)whsOrder);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a booking with Warehouse Order parent should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be false", !supporter.HasConsolParent);
				AssertEquals("Supporter PK should be empty", ZGuid.Empty, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertEquals("Supporter ShipmentsListPKs should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsListPKs.Length);
				AssertEquals("Supporter ShipmentsList should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsList.Length);
				AssertNull("Supporter Shipments should be null", supporterAsIGenericCostSupporter.Shipments);
			});
		}

		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForTBWithWhsReceiveParent()
		{
			var whsReceive = (BusinessObject)Factory.New<IWhsReceive>();
			whsReceive.FillWithValidTestData();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)whsReceive);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a booking with Warehouse Receive parent should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be false", !supporter.HasConsolParent);
				AssertEquals("Supporter PK should be empty", ZGuid.Empty, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertEquals("Supporter ShipmentsListPKs should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsListPKs.Length);
				AssertEquals("Supporter ShipmentsList should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsList.Length);
				AssertNull("Supporter Shipments should be null", supporterAsIGenericCostSupporter.Shipments);
			});
		}

		public void TestDtbBookingJobCostSupporter_ConsolParentProperties_ForStandaloneTB()
		{
			var bookingConsolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;
			CombineAssertions("DtbBookingJobCostSupporter for a Standalone booking should have correct property values for properties that depend on booking parent being a Forwarding Consol", () =>
			{
				Assert("HasConsol parent should be false", !supporter.HasConsolParent);
				AssertEquals("Supporter PK should be empty", ZGuid.Empty, supporterAsIGenericCostSupporter.PK);
				AssertEquals("Supporter Type should be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, supporterAsIGenericCostSupporter.Type);
				AssertEquals("Supporter ShipmentsListPKs should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsListPKs.Length);
				AssertEquals("Supporter ShipmentsList should be empty", 0, supporterAsIGenericCostSupporter.ShipmentsList.Length);
				AssertNull("Supporter Shipments should be null", supporterAsIGenericCostSupporter.Shipments);
			});
		}

		public void TestDtbBookingJobCostSupporter_TestPropertiesThatDoNotDependOnParentBeingConsol()
		{
			var (forwardingConsolParent, _, _) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;

			CombineAssertions("DtbBookingJobCostSupporter should have correct property values for properties that do not depend on booking parent being a Forwarding Consol", () =>
			{
				AssertEquals("Supporter DocumentSupporter should match Booking DocumentSupporter", ((IDocumentSupportable)booking).DocumentSupporter, supporterAsIGenericCostSupporter.DocumentSupporter);
				AssertEquals("Supporter Master Bill Number should be empty", ZString.Empty, supporterAsIGenericCostSupporter.MasterBillNum);
				AssertEquals("Supporter Transport Mode should be empty", ZString.Empty, supporterAsIGenericCostSupporter.TransportMode);
				AssertEquals("Supporter Direction should be Unknown", Directions.Unknown, supporterAsIGenericCostSupporter.Direction);
				Assert("Supporter IsBuyersConsol should be false", !supporterAsIGenericCostSupporter.IsBuyersConsol);
				AssertEquals("Supporter PortOfLoading should be empty", ZString.Empty, supporterAsIGenericCostSupporter.PortOfLoading);
				AssertEquals("Supporter PortOfDischarge should be empty", ZString.Empty, supporterAsIGenericCostSupporter.PortOfDischarge);
				AssertEquals("Supporter ConsolMode should be empty", ZString.Empty, supporterAsIGenericCostSupporter.ConsolMode);
				AssertNull("Supporter SendingForwarder should be null", supporterAsIGenericCostSupporter.SendingForwarder);
				AssertNull("Supporter ReceivingForwarder should be null", supporterAsIGenericCostSupporter.ReceivingForwarder);
				Assert("Supporter IsApportionmentFilterEnabled should be true", supporterAsIGenericCostSupporter.IsApportionmentFilterEnabled);
				AssertEquals("Supporter ETD should be empty date", ZDateTime.Empty, supporterAsIGenericCostSupporter.ETD);
				AssertEquals("Supporter ETA should be empty date", ZDateTime.Empty, supporterAsIGenericCostSupporter.ETA);
				AssertContainsExactElementsInAnyOrder("Supporter ExcludedApportionmentMethods should be FreeSpaceContribution", new[] { AllocationMethod.FreeSpaceContribution }, supporterAsIGenericCostSupporter.ExcludedApportionmentMethods.ToArray());
				AssertContainsExactElementsInAnyOrder("Supporter DefaultChargeGroups should be Transport, TransportBooking", new[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking }, supporter.DefaultChargeGroups);
			});
		}

		public void TestDtbBookingJobCostSupporter_TestDatabaseStateProperties()
		{
			var (forwardingConsolParent, _, _) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;

			CombineAssertions("Before saving, DtbBookingJobCostSupporter should have correct database saved state property values for unsaved record", () =>
			{
				Assert("Supporter HasChanges should match Booking.HasChanges - true as new booking has not yet been saved", supporterAsIGenericCostSupporter.HasChanges);
				Assert("Supporter IsInDatabase should match Booking.IsInDatabase - false as new booking has not yet been saved", !supporterAsIGenericCostSupporter.IsInDatabase);
			});

			Factory.Save();
			CombineAssertions("After saving, DtbBookingJobCostSupporter should still have correct database saved state property values according to booking saved state", () =>
			{
				Assert("Supporter HasChanges should match Booking.HasChanges - false as changes have been saved", !supporterAsIGenericCostSupporter.HasChanges);
				Assert("Supporter IsInDatabase should match Booking.IsInDatabase - true as booking has now been saved", supporterAsIGenericCostSupporter.IsInDatabase);
			});
		}

		public void TestDtbBookingJobCostSupporter_TestTotalChargeableUnit()
		{
			const string TestWeightUnit = "OT";

			var (forwardingConsolParent, _, _) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestWeightUnit))
			{
				AssertEquals("Supporter Transport Mode is empty", TestWeightUnit, supporterAsIGenericCostSupporter.TotalChargeableUnit);
			}
		}

		public void TestDtbBookingJobCostSupporter_TestGetCreditorPK()
		{
			var (forwardingConsolParent, _, _) = CreateForwardingConsolWithTwoShipments();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolParent);
			var booking = Helper.CreateBooking(bookingConsolidation);
			var organisation = Helper.CreateOrganisation("QWESYD");
			var rateProviderOrg = Helper.CreateOrganisation("RATSYD");
			booking.Address.OrganisationPK = organisation.PK;
			var supporter = new DtbBookingJobCostSupporter(booking);
			var supporterAsIGenericCostSupporter = (IGenericJobCostSupporter)supporter;

			CombineAssertions("Check DtbBookingJobCostSupporter.GetCreditorPK() returns org PK only for specific codes", () =>
			{
				AssertEquals(FormattableString.Invariant($"Supporter GetCreditorPK() returns org PK for code '{ChargeCodeGroupList.Codes.Transport}'"), organisation.PK, supporterAsIGenericCostSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.Transport, rateProviderOrg.PK));
				AssertEquals(FormattableString.Invariant($"Supporter GetCreditorPK() returns org PK for code '{ChargeCodeGroupList.Codes.TransportBooking}'"), organisation.PK, supporterAsIGenericCostSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.TransportBooking, rateProviderOrg.PK));
				foreach (var code in typeof(ChargeCodeGroupList.Codes).GetConstantValues().Where(c => c != ChargeCodeGroupList.Codes.Transport && c != ChargeCodeGroupList.Codes.TransportBooking))
				{
					AssertEquals(FormattableString.Invariant($"Supporter GetCreditorPK() returns org PK for code '{code}'"), ZGuid.Empty, supporterAsIGenericCostSupporter.GetCreditorPK(code, rateProviderOrg.PK));
				}
			});
		}

		(BusinessObject consol, BusinessObject shipment1, BusinessObject shipment2) CreateForwardingConsolWithTwoShipments()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = "Consol1";
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_UniqueConsignRef] = "Shipment1";
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol);

			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_UniqueConsignRef] = "Shipment2";
			shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment2[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol);

			return (consol, shipment1, shipment2);
		}

		TransportBookingTestHelper Helper => helper ?? new TransportBookingTestHelper(Factory);
		readonly TransportBookingTestHelper helper;
	}
}
