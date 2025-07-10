using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidationJobInvoicingSupporter))]
	public class DtbBookingConsolidationJobInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestIJobInvoicingSupporter()
		{
			var year = ZDateTime.Now.Year;

			var dummy = Factory.New<DummyWithDtbBooking>();
			Helper.CreateConsolidationMultiJob();
			var dummySupproter = ((IJobInvoicingPlugIn)dummy).InvoicingSupporter;
			IJobInvoicingSupporter supporter = new DtbBookingConsolidationJobInvoicingSupporter(new DtbBookingConsolidationJobInvoicingPlugIn(dummy), dummySupproter);

			// Checking that properties are not coming from Parent (dummy) but are calculated.
			AssertNotEquals("IJobInvoicingSupporter.ActualChargeable", 100m, supporter.ActualChargeable);
			AssertNotEquals("IJobInvoicingSupporter.ActualWeight", 3m, supporter.ActualWeight);
			AssertNotEquals("IJobInvoicingSupporter.ContainerCount", 5, supporter.ContainerCount);
			AssertNotEquals("IJobInvoicingSupporter.TEUCount", 6m, supporter.TEUCount);

			// Checking that those properties are mapped to Parent (dummy).
			AssertEquals("IJobInvoicingSupporter.ActualChargeableUnit", "M3", supporter.ActualChargeableUnit);
			AssertEquals("IJobInvoicingSupporter.ActualLoadingMeters", 1m, supporter.ActualLoadingMeters);
			AssertEquals("IJobInvoicingSupporter.ActualVolume", 2m, supporter.ActualVolume);
			AssertEquals("IJobInvoicingSupporter.ActualVolumeUnit", "M3", supporter.ActualVolumeUnit);
			AssertEquals("IJobInvoicingSupporter.ActualWeightUnit", "KG", supporter.ActualWeightUnit);
			AssertEquals("IJobInvoicingSupporter.ATA", new ZDateTime(year, 5, 15), supporter.ATA);
			AssertEquals("IJobInvoicingSupporter.ATD", new ZDateTime(year, 5, 16), supporter.ATD);
			AssertEquals("IJobInvoicingSupporter.AuditSecurity", Env.Security.DtbBookingConsolidation, supporter.AuditSecurity);
			AssertEquals("IJobInvoicingSupporter.ConsolExchangeRate", 4m, supporter.ConsolExchangeRate);
			AssertEquals("IJobInvoicingSupporter.ConsolType", "ConsolType", supporter.ConsolType);
			AssertEquals("IJobInvoicingSupporter.ContainerMode", "ContainerMode", supporter.ContainerMode);
			AssertEquals("IJobInvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			AssertEquals("IJobInvoicingSupporter.DefaultChargeGroup", "DefaultChargeGroup", supporter.DefaultChargeGroup);
			AssertEquals("IJobInvoicingSupporter.EditSecurityCheckpoint", Env.Security.DtbBookingConsolidationEdit, supporter.EditSecurityCheckpoint);
			AssertEquals("IJobInvoicingSupporter.EditSecurityLock", false, supporter.EditSecurityLock);
			AssertEquals("IJobInvoicingSupporter.EditSecurityMessage", "EditSecurityMessage", supporter.EditSecurityMessage);
			AssertEquals("IJobInvoicingSupporter.ETA", new ZDateTime(year, 5, 17), supporter.ETA);
			AssertEquals("IJobInvoicingSupporter.ETD", new ZDateTime(year, 5, 18), supporter.ETD);
			AssertEquals("IJobInvoicingSupporter.GetCustomsClearanceDate", new ZDateTime(year, 5, 19), supporter.GetCustomsClearanceDate());
			AssertEquals("IJobInvoicingSupporter.GetOperationsSignificantDate", new ZDateTime(year, 5, 20), supporter.GetOperationsSignificantDate(""));
			AssertEquals("IJobInvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails", "ReasonNotToAllowChangeOnCostDetails", supporter.GetReasonNotToAllowChangeOnCostDetails(ZGuid.Empty));
			AssertEquals("IJobInvoicingSupporter.GetReasonNotToAllowPosting", "ReasonNotToAllowPosting", supporter.GetReasonNotToAllowPosting());
			AssertEquals("IJobInvoicingSupporter.HouseBillNumber", "HouseBillNumber", supporter.HouseBillNumber);
			AssertEquals("IJobInvoicingSupporter.IncludeInConsolCosting", true, supporter.IncludeInConsolCosting(false));
			AssertEquals("IJobInvoicingSupporter.PaymentTerm", dummySupproter.PaymentTerm, supporter.PaymentTerm);
			AssertEquals("IJobInvoicingSupporter.IsDirectShipment", true, supporter.IsDirectShipment);
			AssertEquals("IJobInvoicingSupporter.IsDomestic", true, supporter.IsDomestic);
			AssertEquals("IJobInvoicingSupporter.IsExport", true, supporter.IsExport);
			AssertEquals("IJobInvoicingSupporter.IsImport", false, supporter.IsImport);
			AssertEquals("IJobInvoicingSupporter.IsPlugInReadOnly", false, supporter.IsPlugInReadOnly);
			AssertEquals("IJobInvoicingSupporter.JobInvoicingSecurity", Env.Security.MaintainShipmentJobInvoicing, supporter.JobInvoicingSecurity);
			AssertEquals("IJobInvoicingSupporter.MasterBillNumber", "MasterBillNumber", supporter.MasterBillNumber);
			AssertEquals("IJobInvoicingSupporter.OverriddenDepartmentPK", ZGuid.Empty, supporter.OverriddenDepartmentPK);
			AssertEquals("IJobInvoicingSupporter.ShipmentNumberOfColoadMaster", "ShipmentNumberOfColoadMaster", supporter.ShipmentNumberOfColoadMaster);
			AssertEquals("IJobInvoicingSupporter.TransportMode", "TransportMode", supporter.TransportMode);
			AssertEquals("IJobInvoicingSupporter.ConsumerType", JobInvoicingConsumerTypes.Consol, supporter.ConsumerType);

			AssertNull("IJobInvoicingSupporter.Broker", supporter.Broker);
			AssertNull("IJobInvoicingSupporter.Consignee", supporter.Consignee);
			AssertNull("IJobInvoicingSupporter.Consignor", supporter.Consignor);
			AssertNull("IJobInvoicingSupporter.ConsolRateCurrency", supporter.ConsolRateCurrency);
			AssertNull("IJobInvoicingSupporter.Destination", supporter.Destination);
			AssertNull("IJobInvoicingSupporter.GetDefaultCreditor", supporter.GetDefaultCreditor(new DefaultCreditorSetting(null, "")));
			AssertNull("IJobInvoicingSupporter.OperationsBranch", supporter.OperationsBranch);
			AssertNull("IJobInvoicingSupporter.Origin", supporter.Origin);
			AssertNull("IJobInvoicingSupporter.ReceivingAgent", supporter.ReceivingAgent);
			AssertNull("IJobInvoicingSupporter.SendingAgent", supporter.SendingAgent);
			AssertNull("IJobInvoicingSupporter.GetTranshipmentPort", supporter.GetTranshipmentPort(CostSell.Cost));
			AssertNull("IJobInvoicingSupporter.GetTranshipmentPort", supporter.GetTranshipmentPort(CostSell.Revenue));
		}

		public void TestIJobInvoicingSupporter_ActualChargeable()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			booking1.KM_OverrideChargeable = true;
			booking2.KM_OverrideChargeable = true;
			booking1.KM_Chargeable = 50m;
			booking2.KM_Chargeable = 75m;

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
			AssertEquals("No bookings added to the plug-in, no Chargeable should be found.", 0m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualChargeable);

			consolidationPlugIn.Bookings.Add(booking1);
			AssertEquals("Should get chargeable from booking1.", 50m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualChargeable);

			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Should sum charteable from both bookings (50 + 75).", 125m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualChargeable);
		}

		public void TestIJobInvoicingSupporter_ActualWeight()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
			AssertEquals("No bookings added to the plug-in, no Weight should be found.", 0m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualWeight);

			consolidationPlugIn.Bookings.Add(booking1);
			AssertEquals("Should count the Max Weight of packages on the pickup or delivery instructions: 5 * 3KG.", 15m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualWeight);

			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Should count the Max Weight of packages on the pickup or delivery instructions: (5 + (3 + 5)) * 3KG.", 39m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualWeight);
		}

		public void TestIJobInvoicingSupporter_ContainerCount()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
			AssertEquals("No bookings added to the plug-in, no containers should be found.", 0, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ContainerCount);

			consolidationPlugIn.Bookings.Add(booking1);
			AssertEquals("Should count the max number of CNTs on the pickup or delivery instructions: 5.", 5, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ContainerCount);

			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Should count the max number of CNTs on the pickup or delivery instructions: 5 + 3.", 8, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ContainerCount);
		}

		public void TestIJobInvoicingSupporter_TEUCount()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
			AssertEquals("No bookings added to the plug-in, no TEU should be found.", 0m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.TEUCount);

			consolidationPlugIn.Bookings.Add(booking1);
			AssertEquals("Should count the max TEU of CNTs on the pickup or delivery instructions: 5 * 1.", 5m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.TEUCount);

			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Should count the max TEU of CNTs on the pickup or delivery instructions: 5 * 1 + 3 * 2.", 11m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.TEUCount);
		}

		public void TestIJobInvoicingSupporter_UseParentSupporter_ActualChargeable()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			booking1.KM_OverrideChargeable = true;
			booking2.KM_OverrideChargeable = true;
			booking1.KM_Chargeable = 50m;
			booking2.KM_Chargeable = 75m;

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy, supporterShouldUseParent: true);
			AssertEquals("Nothing added to the plug-in, Chargeable should be found from DummyWithDtbBookingJobInvoicingSupporter.", 100m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualChargeable);

			consolidationPlugIn.Bookings.Add(booking1);
			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Chargeable should not change when bookings added to plug-in.", 100m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualChargeable);
		}

		public void TestIJobInvoicingSupporter_UseParentSupporter_ActualWeight()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy, supporterShouldUseParent: true);
			AssertEquals("Nothing added to the plug-in, Weight should be found from DummyWithDtbBookingJobInvoicingSupporter.", 3m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualWeight);

			consolidationPlugIn.Bookings.Add(booking1);
			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("Weight should not change when bookings added to plug-in.", 3m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ActualWeight);
		}

		public void TestIJobInvoicingSupporter_UseParentSupporter_ContainerCount()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy, supporterShouldUseParent: true);
			AssertEquals("Nothing added to the plug-in, ContainerCount should be found from DummyWithDtbBookingJobInvoicingSupporter.", 5, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ContainerCount);

			consolidationPlugIn.Bookings.Add(booking1);
			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("ContainerCount should not change when bookings added to plug-in.", 5, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.ContainerCount);
		}

		public void TestIJobInvoicingSupporter_UseParentSupporter_TEUCount()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy, supporterShouldUseParent: true);
			AssertEquals("Nothing added to the plug-in, TEU should be found should be found from DummyWithDtbBookingJobInvoicingSupporter.", 6m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.TEUCount);

			consolidationPlugIn.Bookings.Add(booking1);
			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("TEUCount should not change when bookings added to plug-in.", 6m, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.TEUCount);
		}

		public void TestIJobInvoicingSupporter_UseParentSupporter_OuterPackTotal()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);
			((DummyWithDtbBookingJobInvoicingSupporter)dummy.InvoicingSupporter).OuterPackTotal = 10;
			booking1.PackageJob.Packages.Add(Helper.CreatePackage("OUTER"));
			AssertEquals("Precondition: Booking1 should have outer packages", booking1.PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers().Count, 1);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy, supporterShouldUseParent: true);
			AssertEquals("Nothing added to the plug-in, OuterPackTotal should be found should be found from DummyWithDtbBookingJobInvoicingSupporter.", 10, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.OuterPackTotal);

			consolidationPlugIn.Bookings.Add(booking1);
			consolidationPlugIn.Bookings.Add(booking2);
			AssertEquals("OuterPackTotal should not change when bookings added to plug-in.", 10, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.OuterPackTotal);
		}

		public void TestIJobInvoicingSupporter_AlwaysIncludeInConsolCosting()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);

			var consolidationPlugIn = new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
			AssertEquals("All jobs on the consolidated Bokking should be used for apportionment.", true, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.IncludeInConsolCosting(true));
			AssertEquals("All jobs on the consolidated Bokking should be used for apportionment.", true, ((IJobInvoicingPlugIn)consolidationPlugIn).InvoicingSupporter.IncludeInConsolCosting(false));
		}

		void SetupDummyWithDtbBooking(out DummyWithDtbBooking dummy, out DtbBooking booking1, out DtbBooking booking2)
		{
			dummy = Factory.New<DummyWithDtbBooking>();
			var consolidation1 = Helper.CreateConsolidation(dummy);
			var consolidation2 = Helper.CreateConsolidation(dummy); //#warning David Kill this Consolidation when you fixed the Package_PackageView stuff.

			// Packages
			var container1 = PackingHelper.CreatePackage(consolidation1.PackageJob, 5, Constants.PkgUnit.Container, "20GP");
			var container2 = PackingHelper.CreatePackage(consolidation2.PackageJob, 10, Constants.PkgUnit.Container, "40GP");
			var pallet = PackingHelper.CreatePackage(container2, 9, Constants.PkgUnit.Pallet);

			// Booking1 with 2 instruction (PickUp = 5 CNTs), (Multi = 4 CNTs), (Delivery = 1 CNT)
			booking1 = Helper.CreateBooking(consolidation1);
			var instruction1_Booking1 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp);
			var instruction2_Booking1 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi);
			var instruction3_Booking1 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(instruction1_Booking1, container1, 5);
			Helper.CreatePackageDivot(instruction2_Booking1, container1, 4);
			Helper.CreatePackageDivot(instruction3_Booking1, container1, 1);

			// Booking2 with 2 instructions (PickUp = 1 CNT), (Multi = 2 CNTs), (Delivery = 3 CNTs + 5 PLTs)
			booking2 = Helper.CreateBooking(consolidation2);
			var instruction1_Booking2 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp);
			var instruction2_Booking2 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.Multi);
			var instruction3_Booking2 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(instruction1_Booking2, container2, 1);
			Helper.CreatePackageDivot(instruction2_Booking2, container2, 2);
			Helper.CreatePackageDivot(instruction3_Booking2, container2, 3);
			Helper.CreatePackageDivot(instruction3_Booking2, pallet, 5);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			DummyWithDtbBooking dummy;
			DtbBooking booking1;
			DtbBooking booking2;
			SetupDummyWithDtbBooking(out dummy, out booking1, out booking2);
			return new DtbBookingConsolidationJobInvoicingPlugIn(dummy);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;
	}
}
