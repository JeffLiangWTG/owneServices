using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBooking))]
	public sealed class DtbBookingTest : DtbTransportBusinessObjectTestCase
	{
		public void TestReadOnlyOnDeactivation()
		{
			var consolidation = Helper.CreateConsolidation();
			Helper.CreateBooking(consolidation);
			var booking = Helper.CreateBooking(consolidation);
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			booking.Deactivate();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly for the consolidation should return false", false, consolidation.ReadOnly);
				AssertEquals("ReadOnly for the booking should return true", true, booking.ReadOnly);
				AssertEquals("ReadOnly for the address should return true", true, booking.Address.ReadOnly);
				AssertEquals("ReadOnly for the instructions should return true", true, booking.Instructions[0].ReadOnly);
			});
		}

		public void TestAgentBooking()
		{
			var booking = Helper.CreateBooking();
			AssertNull("By default there should be no Agent Booking attached.", booking.AgentBooking);

			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			AssertEquals("After attaching an Agent Booking, the Property should return the attached Agent Booking.", agentBooking, booking.AgentBooking);
		}

		public void TestParentID()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "123";

			var booking_WithParent = Helper.CreateConsolidation(dummy).Bookings.AddNew();
			AssertEquals("ParentID should come from parent.", "123", booking_WithParent.ParentID);

			var standaloneBooking = Helper.CreateBooking();
			AssertEquals("Precondition.", ZString.Empty, standaloneBooking.ParentID);

			standaloneBooking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "456");
			AssertEquals("ParentID should come from additional references.", "456", standaloneBooking.ParentID);
		}

		public void TestRelatedJobs()
		{
			var booking = Helper.CreateBooking();
			var portTransport1 = Helper.CreatePortTransport(booking);
			var portTransport2 = Helper.CreatePortTransport(booking);

			var portTransport3 = Helper.CreatePortTransport(booking);
			portTransport3.JJ_IsCancelled = true;
			var portTransport4 = Helper.CreatePortTransport(booking);
			portTransport4.JJ_IsCancelled = true;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include both active and inactive transport jobs", new[] { portTransport1, portTransport2, portTransport3, portTransport4 }, booking.RelatedJobs);
		}

		public void TestRelatedJobs_ConsolidationMultiJob()
		{
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var collectionMultiJob = new DtbBookingCollection(consolidationMultiJob);
			var bookingMultiJob = collectionMultiJob.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { consolidationMultiJob }, bookingMultiJob.RelatedJobs);

			// create a second booking and attach consignments and a forwarding shipment (normally done via UXML)
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			AssertContainsExactElementsInAnyOrder("Should only contains shipment", new[] { shipment }, booking.RelatedJobs);
		}

		public void TestRelatedJobs_LandTransportJobs()
		{
			var booking = Helper.CreateBooking();
			var consignment = Factory.New<IDtbConsignment>();
			consignment.LTC_Direction = "ORG";
			consignment.LTC_Status = "BKD";
			consignment.LTC_KM_Booking = booking.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should contains consignment", new[] { consignment }, booking.RelatedJobs);
		}

		public void TestRelatedJobs_DispatchConsignmentJobs()
		{
			var booking = Helper.CreateBooking();
			var transitHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = transitHelper.CreateTRWWarehouse();
			var dispatchConsignment = (BusinessObject)Factory.New<IWhsItemDispatchConsignment>();
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_JobID] = "DCN01";
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_ConsignmentID] = "DC001";
			dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse] = warehouse.PK;

			Factory.Save();

			var universalJobLinkCreator = ObjectFactory.Get<IUniversalJobLinkCreator>("IUniversalJobLinkCreator", Factory, booking, DataContextType.TransitDispatch);
			universalJobLinkCreator.CreateUniversalJobLink(dispatchConsignment);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should contains dispatch consignment", new[] { dispatchConsignment }, booking.RelatedJobs);
		}

		public void TestRelatedJobs_ShouldNotIncludeConsignmentBookings()
		{
			var booking = Helper.CreateBooking();

			var consignmentConsol = Factory.New<IDtbConsignmentConsolidation>();
			consignmentConsol.KB_ParentID = booking.PK;
			consignmentConsol.KB_ParentTableCode = booking.TablePrefix;

			var consignmentBooking = Helper.CreateBooking();
			consignmentBooking.KM_JobType = TransportConsolidationJobTypes.Codes.Consignment;
			consignmentBooking.KM_KB_Booking = consignmentConsol.PK;

			Factory.Save();

			AssertEquals("The Consignment Booking should not be found by RelatedJobs.", 0, booking.RelatedJobs.Count);
		}

		public void TestRelatedJobs_WhenParentIsNull()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyBusinessObject);
			var parent = Factory.New<DummyBusinessObject>();
			consolidation.KB_ParentID = parent.PK;
			consolidation.KB_ParentTableCode = parent.TablePrefix;
			Assert("Precondition", !typeof(IDtbBookingParent).IsAssignableFrom(parent.GetType()));
			Factory.Save();

			AssertEquals("Should load realted object when parent is null!", 0, booking.RelatedJobs.Count);
		}

		public void TestRelatedJobs_ForSubBookingContainingMaster()
		{
			var masterBooking = Helper.CreateBooking();
			var subBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Sub booking should contain its master booking as a related job.", new[] { masterBooking }, subBooking.RelatedJobs);
		}

		public void TestRelatedJobs_ForMasterBookingContainingSubs()
		{
			var masterBooking = Helper.CreateBooking();
			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();
			var normalBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking3.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			AssertEquals("Normal transport booking should not contain any other related jobs.", 0, normalBooking.RelatedJobs.Count);

			AssertContainsExactElementsInAnyOrder("Master booking should contain its sub bookings as related jobs.", new[] { subBooking1, subBooking2, subBooking3 }, masterBooking.RelatedJobs);
		}

		public void TestParentViewDoesNotThrowAndIsNullIfConsolidationSingleJobParentIsNull()
		{
			var booking = Factory.New<DtbBooking>();
			AssertNull(
				"ParentView should be null if ConsolidationSingleJob is null",
				booking.ParentView);
		}

		public void TestScheduleDoesNotThrowAndIsNullIfConsolidationSingleJobParentIsNull()
		{
			var booking = Factory.New<DtbBooking>();
			AssertNull(
				"Schedule should be null if ConsolidationSingleJob is null",
				booking.Schedule);
		}

		public void TestSchedule()
		{
			var now = ZDateTime.Now;

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "NLAMS";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			var consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "GBLON";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "NLAMS";

			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);
			shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment2[JobShipmentSchema.JS_RL_NKDestination] = "GBLON";

			var shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			var transportS3 = ((IBusinessObjectCollection)shipment3["Transports"]).AddNew();
			transportS3[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportS3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "GBLON";
			shipment3[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			shipment3[JobShipmentSchema.JS_RL_NKDestination] = "GBLON";

			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var booking3 = Helper.CreateBooking();

			booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			booking3.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;

			Factory.Save();

			AssertEquals("AUBNE", booking1.Schedule.VL_RL_NKLoad);
			AssertEquals("AUBNE", booking2.Schedule.VL_RL_NKLoad);
			AssertEquals("AUSYD", booking3.Schedule.VL_RL_NKLoad);

			AssertEquals("NLAMS", booking1.Schedule.VL_RL_NKDischarge);
			AssertEquals("GBLON", booking2.Schedule.VL_RL_NKDischarge);
			AssertEquals("GBLON", booking3.Schedule.VL_RL_NKDischarge);
		}

		public void TestWayBillNumber()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Precondition.", ZString.Empty, booking.WayBillNumber);

			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "ABC123");
			booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "DEF123");
			AssertEquals("Should display MasterBill in absence of HouseBill from ConsolidationSingleJob additional reference numbers.", "DEF123", booking.WayBillNumber);

			var houseBill = booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "DEF456");
			var houseBillFromConsolidationSingleJob = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "XCV456");
			AssertEquals("Should display HouseBill.", "XCV456", booking.WayBillNumber);

			houseBillFromConsolidationSingleJob.CE_EntryNum = ZString.Empty;
			AssertEquals("Should fall back to MasterBill if empty HouseBill exists.", "DEF123", booking.WayBillNumber);
		}

		public void TestOriginAndDestination()
		{
			var now = ZDateTime.Now;

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "NLAMS";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "NLAMS";

			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();

			booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;

			Factory.Save();

			AssertEquals("Should display Origin", "AUBNE", booking1.Origin);
			AssertEquals("Should have blank Origin", ZString.Empty, booking2.Origin);

			AssertEquals("Should display Destination", "NLAMS", booking1.Destination);
			AssertEquals("Should have blank Destination", ZString.Empty, booking2.Destination);
		}

		public void TestRelatedNotesFromQuotedBooking()
		{
			var booking = Factory.New<DtbBooking>();
			var consolidation = Factory.New<DtbBookingConsolidation>();

			var builder = ObjectFactory.New<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			var quotedBooking = (BusinessObject)builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);

			consolidation.KB_ParentID = quotedBooking.PK;
			consolidation.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			booking.KM_KB_Booking = consolidation.PK;

			AssertNoExceptionThrown(() =>
			{
				var unused = booking.Notes.HasRelatedNotes;
			});
		}

		public void TestRelatedNotesFromQuotedBooking_schedule()
		{
			var now = ZDateTime.Now;
			var eTD = now.AddDays(-1).ToSmallDateTimeFloor();
			var eTA = now.AddDays(1).ToSmallDateTimeFloor();

			var builder = ObjectFactory.New<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			var quotedBooking = builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var shipment = quotedBooking.ForwardingShipment;
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_ParentID = quotedBooking.ViewPK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "GBLON";

			var voyage = (BusinessObject)Factory.New<IJobVoyage>();
			var sailing = (BusinessObject)Factory.New<IJobSailing>();
			var origin = (BusinessObject)Factory.New<IVoyageOrigin>();
			var destination = (BusinessObject)Factory.New<IVoyageDestination>();
			origin["JA_E_DEP"] = eTD;
			origin["JA_JV"] = voyage.PK;
			destination["JB_E_ARV"] = eTA;
			destination["JB_JV"] = voyage.PK;
			sailing["JX_JA"] = origin.PK;
			sailing["JX_JB"] = destination.PK;
			shipment[JobShipmentSchema.JS_JX] = sailing.PK;
			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = "AUSYD";
			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = "GBLON";

			Factory.Save();

			AssertEquals("AUSYD", booking.Schedule.VL_RL_NKLoad);
			AssertEquals("GBLON", booking.Schedule.VL_RL_NKDischarge);
			AssertEquals(eTD, booking.Schedule.VL_ETD);
			AssertEquals(eTA, booking.Schedule.VL_ETA);
		}

		public void TestCarrierBookingAgent()
		{
			var booking = Helper.CreateBooking();
			AssertNotNull(booking.CarrierBookingAgentDocAddress);
			AssertEquals(booking.CarrierBookingAgentDocAddress.DocAddressType, DocAddressType.CarrierBookingAgent);
			AssertNull(booking.CarrierBookingAgent);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;

			booking.CarrierBookingAgentDocAddress.E2_OA_Address = address.PK;

			AssertEquals(org.PK, booking.CarrierBookingAgentDocAddress.OrganisationPK);
			AssertEquals(org.PK, booking.CarrierBookingAgent.PK);
		}

		[TestDate(2014, 1, 1)]
		public void TestCreatedByUserName()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("", booking.KM_SystemCreateUser);
			AssertEquals("", booking.CreatedByUserName);

			Factory.Save();
			AssertEquals("E", booking.KM_SystemCreateUser);
			AssertEquals("CargoWise Support", booking.CreatedByUserName);
		}

		public void TestTransportBookingPartyReference()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(true, booking.TransportBookingPartyReference.IsEmpty);

			var entryNum = booking.AdditionalReferenceNumbers.AddNew();
			entryNum.CE_EntryType = "XXX";
			entryNum.CE_EntryNum = "123";
			AssertEquals(true, booking.TransportBookingPartyReference.IsEmpty);

			entryNum.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			AssertEquals("123", booking.TransportBookingPartyReference);
		}

		public void TestKM_Status()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			consolidationMultiJob.Bookings.Add(booking);

			AssertEquals(TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("Status should always be readonly", true, booking.KM_StatusInfo.ReadOnly);

			// if status is held, the booking and children should be readonly
			booking.KM_Status = TransportStatuses.Codes.Available;
			AssertEquals(false, booking.ReadOnly);
			AssertEquals(false, instruction.ReadOnly);

			booking.KM_Status = TransportStatuses.Codes.Held;
			AssertEquals(true, booking.ReadOnly);
			AssertEquals(true, instruction.ReadOnly);

			// changing the booking status should recalculate the consolidated booking status
			booking.KM_Status = TransportStatuses.Codes.Available; // to remove the held booking status and recalculate the consolidated booking status
			AssertEquals(TransportStatuses.Codes.Available, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.Available, consolidationMultiJob.KB_Status);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals(TransportStatuses.Codes.ActionRequired, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.ActionRequired, consolidationMultiJob.KB_Status);

			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals(TransportStatuses.Codes.ServiceCommenced, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.ServiceCommenced, consolidationMultiJob.KB_Status);

			booking.KM_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(TransportStatuses.Codes.PickedUp, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.PickedUp, consolidationMultiJob.KB_Status);

			booking.KM_Status = TransportStatuses.Codes.DeliveredEmptyNotReturned;
			AssertEquals(TransportStatuses.Codes.DeliveredEmptyNotReturned, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.DeliveredEmptyNotReturned, consolidationMultiJob.KB_Status);

			booking.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(TransportStatuses.Codes.Delivered, consolidation.KB_Status);
			AssertEquals(TransportStatuses.Codes.Delivered, consolidationMultiJob.KB_Status);
		}

		public void TestKM_Status_ChangeLoggedJustBeforeSaving()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Precondition: booking created with AVL status", TransportStatuses.Codes.Available, booking.KM_Status);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Factory.Save(); // first save
			AssertEquals("Status Update event should be added", 1, booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).Count());
			var stmALog = booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime).First();
			AssertEquals("Old status must be Available", TransportStatuses.Codes.Available, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("New status must be ServiceCommenced", TransportStatuses.Codes.ServiceCommenced, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			booking.KM_Status = TransportStatuses.Codes.Available;
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Factory.Save();
			AssertEquals("No new logs should be added", 1, booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).Count());
			stmALog = booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime).First();
			AssertEquals("Old status must be Available", TransportStatuses.Codes.Available, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("New status must be ServiceCommenced", TransportStatuses.Codes.ServiceCommenced, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			booking.KM_Status = TransportStatuses.Codes.Held;
			booking.KM_Status = TransportStatuses.Codes.PickedUp;
			Factory.Save();
			AssertEquals("Status Update event should be added", 2, booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).Count());
			stmALog = booking.Logs.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime).First();
			AssertEquals("Old status must be ServiceCommenced", TransportStatuses.Codes.ServiceCommenced, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("New status must be PickedUp - the one booking has before saving", TransportStatuses.Codes.PickedUp, stmALog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		public void TestBillingPartyOrLocalClientPK_HasParentJob()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			AssertNotNull("Pre-condition:", booking.ConsolidationSingleJob.Parent);
			var billingParty1 = Helper.CreateOrganisation("O1");
			var billingParty2 = Helper.CreateOrganisation("O2");
			var notify = new TestNotificationBuffer();
			booking.ConsolidationSingleJob.NotificationManager.Push(notify);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			booking.BillingPartyOrLocalClientPK = billingParty1.MainAddress.PK;
			AssertNull(booking.Job);
			AssertNull("First time to assign BillingParty, should not pop up message.", UnitTestUserNotification.Instance.LastMessage.Text);
			Helper.LoadOrCreateJobHeader(booking);
			AssertEquals(billingParty1.MainAddress.PK, booking.Job.JH_OA_LocalChargesAddr);
			AssertEquals(billingParty1.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
			AssertEquals(billingParty1.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);
			notify.Response = false;
			booking.BillingPartyOrLocalClientPK = billingParty2.MainAddress.PK;
			AssertEquals(billingParty1.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
			notify.Response = true;
			booking.BillingPartyOrLocalClientPK = billingParty2.MainAddress.PK;
			AssertEquals(billingParty2.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
		}

		public void TestKM_BookingOfTransportRequestedDate_AddsBKQEventOnSaving()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			AssertEquals("Precondition: No BKQ events on booking.", 0, booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Count());
			AssertEquals("Precondition: Booking request date is empty.", ZDateTime.Empty, booking.KM_BookingOfTransportRequestedDate);
			Factory.Save();

			AssertEquals("There should still be no booking requested events on booking.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			AssertEquals("There should still be no booking requested events on booking as it's not yet saved.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			Factory.Save();
			AssertEquals("There should be booking requested events on booking after saving.", true, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));

			var bkqEvent = booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Single();
			AssertEquals("Event reference parameter type is 'Transport'.", Constants.EventReferenceParameterTypes.Transport, bkqEvent.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event reference is the job id.", "TB1", bkqEvent.ReferenceFreeText);
		}

		public void TestKM_BookingOfTransportRequestedDate_AddsBKQPickupEventOnSaving()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			booking.KM_Direction = Constants.CartageDirection.Origin;
			AssertEquals("Precondition: No BKQ events on booking.", 0, booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Count());
			AssertEquals("Precondition: Booking request date is empty.", ZDateTime.Empty, booking.KM_BookingOfTransportRequestedDate);
			Factory.Save();

			AssertEquals("There should still be no booking requested events on booking.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			AssertEquals("There should still be no booking requested events on booking as it's not yet saved.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			Factory.Save();
			AssertEquals("There should be booking requested events on booking after saving.", true, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));

			var bkqEvent = booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Single();
			AssertEquals("Event reference parameter type is 'PickupTransport'.", Constants.EventReferenceParameterTypes.PickupTransport, bkqEvent.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event reference is the job id.", "TB1", bkqEvent.ReferenceFreeText);
		}

		public void TestKM_BookingOfTransportRequestedDate_AddsBKQDeliveryEventOnSaving()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			booking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: No BKQ events on booking.", 0, booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Count());
			AssertEquals("Precondition: Booking request date is empty.", ZDateTime.Empty, booking.KM_BookingOfTransportRequestedDate);
			Factory.Save();

			AssertEquals("There should still be no booking requested events on booking.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			AssertEquals("There should still be no booking requested events on booking as it's not yet saved.", false, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			Factory.Save();
			AssertEquals("There should be booking requested events on booking after saving.", true, booking.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));

			var bkqEvent = booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Single();
			AssertEquals("Event reference parameter type is 'DeliveryTransport'.", Constants.EventReferenceParameterTypes.DeliveryTransport, bkqEvent.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event reference is the job id.", "TB1", bkqEvent.ReferenceFreeText);
		}

		public void TestKM_BookingOfTransportRequestedDate_AddsBKQEventOnSaving_BKQEventExists()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = "TB1";
			booking.Logs.CreateOrRecreateEventLog(Events.BookingRequested, EstimateActual.Actual, ZDateTimeOffset.Now, booking.KM_JobID,
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));
			Factory.Save();

			AssertEquals("Precondition: There is 1 BKQ event on booking.", 1, booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Count());
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;
			Factory.Save();

			AssertEquals("There is still only 1 BKQ event.", 1, booking.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode).Count());
		}

		public void TestKM_BookingOfTransportRequestedDate_AddsBKQEventOnSaving_BKQEventAddedToParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var booking_WithParent = Helper.CreateConsolidation(dummy).Bookings.AddNew();
			booking_WithParent.KM_JobID = "TB1";
			booking_WithParent.KM_BookingOfTransportRequestedDate = ZDateTime.Now;
			Factory.Save();

			AssertEquals("BKQ event is added to the booking.", true, booking_WithParent.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			AssertEquals("BKQ event is added to the booking parent.", true, dummy.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
		}

		public void TestKM_BookingOfTransportRequestedDate_UpdatesShipmentCartage_Pickup()
		{
			var shipment = Helper.CreateForwardingShipment("123", "", "", "");
			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Today.AddDays(-3);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Today.AddDays(3);

			var consolidatedBooking = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidatedBooking.KB_ParentTableCode = "JS";
			consolidatedBooking.KB_ParentID = shipment.PK;
			consolidatedBooking.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			var booking = Factory.New<DtbBooking>();
			booking.KM_Direction = "ORG";
			booking.KM_KB_Booking = consolidatedBooking.PK;

			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Today.AddDays(-2);

			AssertEquals("shipment pickup cartage advised date changed", ZDateTime.Today.AddDays(-2), shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals("shipment delivery cartage advised date unchanged", ZDateTime.Today.AddDays(3), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestKM_BookingOfTransportRequestedDate_UpdatesShipmentCartage_Delivery()
		{
			var shipment = Helper.CreateForwardingShipment("123", "", "", "");
			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Today.AddDays(-3);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Today.AddDays(3);

			var consolidatedBooking = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidatedBooking.KB_ParentTableCode = "JS";
			consolidatedBooking.KB_ParentID = shipment.PK;
			consolidatedBooking.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			var booking = Factory.New<DtbBooking>();
			booking.KM_Direction = "DST";
			booking.KM_KB_Booking = consolidatedBooking.PK;

			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Today.AddDays(2);

			AssertEquals("shipment pickup cartage advised date unchanged", ZDateTime.Today.AddDays(-3), shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals("shipment delivery cartage advised date changed", ZDateTime.Today.AddDays(2), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestKM_BookingOfTransportRequestedDate_IsDatePopulatedWhenBookingsDirectionIsLOC()
		{
			var shipment = Helper.CreateForwardingShipment("123", "", "", "");
			shipment.DocsAndCartage.JP_PickupCartageAdvised = ZDateTime.Today.AddDays(-3);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = ZDateTime.Today.AddDays(3);

			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(transportBookingParent);
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			consolidation.KB_ParentTableCode = "JS";
			consolidation.KB_ParentID = shipment.PK;

			var booking = Factory.New<DtbBooking>();
			booking.KM_Direction = "LOC";
			booking.KM_KB_Booking = consolidation.PK;

			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Today.AddDays(-2);

			AssertEquals("shipment pickup cartage advised date changed", ZDateTime.Today.AddDays(-2), shipment.DocsAndCartage.JP_PickupCartageAdvised);
			AssertEquals("shipment delivery cartage advised date unchanged", ZDateTime.Today.AddDays(3), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestDefaultPackagesPopulatesTransportBookingInstructionRelaseNumbersByPackageDictionaryCorrectly()
		{
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(transportBookingParent);
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			var pkgContainer = Helper.CreatePackageContainer("");
			var releaseNumber = "EmptyRelease123";

			var booking = consolidation.Bookings.AddNew();

			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.Confirmations.AddNew();
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			var packages = new[] { pkgContainer };

			var releaseNumbersByPackages = new Dictionary<PkgPackage, ZString>()
			{
				{ pkgContainer, releaseNumber }
			};

			booking.DefaultPackages(packages, releaseNumbersByPackages);
			var result = booking.PickupConfirmations.Single().KK_ReferenceNum;

			AssertEquals(releaseNumber, result);
		}

		public void TestDefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructionsAfterAttachingOrDetachingNewSubUpdatesDivots()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();

			masterBooking.KM_KT_NKBookingTemplate = "EFPL";

			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, masterBooking.Instructions.Count);
			var extraNonMatchingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			extraNonMatchingInstruction.KN_Sequence = 5;

			var subBooking1Container = Helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1ContainerInnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);
			subBooking1ContainerInnerLoosePackage.Packages.Add(subBooking1ContainerInnerInnerLoosePackage);

			var subBooking2LoosePackage = Helper.CreatePackage("LOOSE2", 1);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);
			Factory.Save();

			masterBooking.SubBookings.AddRange(new DtbBooking[] { subBooking1, subBooking2 });
			Assert("Stage 1 Precondition: No DtbBookingInstructionPkgDivot records currently exist for any master booking instructions",
				!masterBooking.Instructions.Any(i => i.DivotsWithPackages.Any()));

			masterBooking.DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions();

			var instructionPic1 = masterBooking.Instructions.First(i => i.KN_Sequence == 1);
			var instructionPic2 = masterBooking.Instructions.First(i => i.KN_Sequence == 2);
			var instructionMlt3 = masterBooking.Instructions.First(i => i.KN_Sequence == 3);
			var instructionDlv4 = masterBooking.Instructions.First(i => i.KN_Sequence == 4);
			var instructionDlv5 = masterBooking.Instructions.First(i => i.KN_Sequence == 5);

			var expectedLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
				(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var expectedContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
			};

			var expectedBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var expectedOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			CombineAssertions("Stage 1 Post-Act check: After call to DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions(), DtbBookingInstructionPkgDivot records exist for both master instructions to correct packages", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});

			Factory.Save();

			masterBooking.SubBookings.RemoveFromRelationship(subBooking2);

			CombineAssertions("Stage 2 Precondition: DtbBookingInstructionPkgDivot records have not changed since last call to ResetPackageJobAndDefaultPackagesForAllInstructions()", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});

			masterBooking.DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions();

			expectedLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
			};

			expectedContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
			};

			expectedBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
			};

			expectedOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
			};

			CombineAssertions("Stage 2 Post-Act check: After call to DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions() following the detach of subBooking2, DtbBookingInstructionPkgDivot records are now updated according to updated packages on package job", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});

			Factory.Save();

			masterBooking.SubBookings.RemoveFromRelationship(subBooking1);

			CombineAssertions("Stage 3 Precondition: DtbBookingInstructionPkgDivot records have not changed since last call to ResetPackageJobAndDefaultPackagesForAllInstructions()", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});

			masterBooking.DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions();

			var emptyArrayOfDivotDetails = Array.Empty<(ZGuid packagePK, ZInt packageQty)>();
			CombineAssertions("Stage 3 Post-Act check: After call to DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions() following the detach of final sub subBooking1, DtbBookingInstructionPkgDivot records are now completely removed", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, emptyArrayOfDivotDetails);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, emptyArrayOfDivotDetails);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, emptyArrayOfDivotDetails);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, emptyArrayOfDivotDetails);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(instructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, emptyArrayOfDivotDetails);
			});
		}

		public void TestDefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructionsForNonMasterBookingDoesNotAssignDivots()
		{
			var booking = Helper.CreateBooking();
			booking.KM_KT_NKBookingTemplate = "EFPL";
			AssertEquals("Precondition: Should now have four instructions from template", 4, booking.Instructions.Count);

			var container1 = Helper.CreatePackageContainer("CONT1");
			var container1InnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var container1InnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			booking.PackageJob.Packages.Add(container1);
			container1.Packages.Add(container1InnerLoosePackage);
			container1InnerLoosePackage.Packages.Add(container1InnerInnerLoosePackage);

			var loosePackage1 = Helper.CreatePackage("LOOSE2", 1);
			booking.PackageJob.Packages.Add(loosePackage1);

			Assert("Precondition: No DtbBookingInstructionPkgDivot records currently exist for any instructions on the non-master booking",
				!booking.Instructions.Any(i => i.DivotsWithPackages.Any()));

			var instructionPic1 = booking.Instructions.First(i => i.KN_Sequence == 1);
			var instructionPic2 = booking.Instructions.First(i => i.KN_Sequence == 2);
			var instructionMlt3 = booking.Instructions.First(i => i.KN_Sequence == 3);
			var instructionDlv4 = booking.Instructions.First(i => i.KN_Sequence == 4);

			booking.DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions();

			Assert("After call to DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions() for a non-master booking, no DtbBookingInstructionPkgDivot records were created for any master booking instructions",
				!booking.Instructions.Any(i => i.DivotsWithPackages.Any()));
		}

		public void TestRemovePackagesIfMasterFromSubBookingsForAllInstructions()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();

			masterBooking.KM_KT_NKBookingTemplate = "EFPL";

			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, masterBooking.Instructions.Count);
			var extraNonMatchingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			extraNonMatchingInstruction.KN_Sequence = 5;

			var subBooking1Container = Helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1ContainerInnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);
			subBooking1ContainerInnerLoosePackage.Packages.Add(subBooking1ContainerInnerInnerLoosePackage);

			var subBooking2Container = Helper.CreatePackageContainer("CONT2");
			var subBooking2LoosePackage = Helper.CreatePackage("LOOSE2", 1);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);
			subBooking2.PackageJob.Packages.Add(subBooking2Container);

			masterBooking.SubBookings.Add(subBooking1);
			masterBooking.SubBookings.Add(subBooking2);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);

			var masterInstructionPic1 = masterBooking.Instructions.First(i => i.KN_Sequence == 1);
			var masterInstructionPic2 = masterBooking.Instructions.First(i => i.KN_Sequence == 2);
			var masterInstructionMlt3 = masterBooking.Instructions.First(i => i.KN_Sequence == 3);
			var masterInstructionDlv4 = masterBooking.Instructions.First(i => i.KN_Sequence == 4);
			var masterInstructionDlv5 = masterBooking.Instructions.First(i => i.KN_Sequence == 5);

			var currentLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking2LoosePackage to check detaching does not default anything
			};

			var currentContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					//intentionally missing subBooking1Container to check detaching does not default anything
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var currentBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					//intentionally missing subBooking1ContainerInnerLoosePackage to check detaching does not default anything
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var currentOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstructionPic1, currentLoosePackages);
			AssignDivotsToInstructions(masterInstructionPic2, currentContainerPackages);
			AssignDivotsToInstructions(masterInstructionMlt3, currentBothPackages);
			AssignDivotsToInstructions(masterInstructionDlv4, currentContainerPackages);
			masterInstructionDlv5.PackageCategory = PackageCategories.Codes.Outers;
			AssignDivotsToInstructions(masterInstructionDlv5, currentOuterPackages);

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBooking1, subBooking2);
			Factory.Save();

			CombineAssertions("Precondition: subBookings packages should be correctly assigned to instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, currentLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, currentBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, currentOuterPackages);
			});

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking1
			};
			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			var expectedLoosePackages = currentLoosePackages.Where(lp => lp.packagePK != subBooking1Container.PK && lp.packagePK != subBooking1ContainerInnerLoosePackage.PK).ToArray();

			var expectedContainerPackages = currentContainerPackages.Where(lp => lp.packagePK != subBooking1Container.PK && lp.packagePK != subBooking1ContainerInnerLoosePackage.PK).ToArray();

			var expectedBothPackages = currentBothPackages.Where(lp => lp.packagePK != subBooking1Container.PK && lp.packagePK != subBooking1ContainerInnerLoosePackage.PK).ToArray();

			var expectedOuterPackages = currentOuterPackages.Where(lp => lp.packagePK != subBooking1Container.PK && lp.packagePK != subBooking1ContainerInnerLoosePackage.PK).ToArray();

			CombineAssertions("subBooking1's packages should be correctly removed from instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});
		}

		public void TestRemovePackagesIfMasterFromSubBookingsForAllInstructions_ForAttachAndDetachWithoutSaving()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();

			masterBooking.KM_KT_NKBookingTemplate = "EFPL";
			subBooking1.KM_KT_NKBookingTemplate = "EFPL";
			subBooking2.KM_KT_NKBookingTemplate = "EFPL";

			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, masterBooking.Instructions.Count);
			AssertEquals("Precondition: Should now have four instructions from template", 4, subBooking1.Instructions.Count);
			AssertEquals("Precondition: Should now have four instructions from template", 4, subBooking2.Instructions.Count);
			var masterBookingExtraNonMatchingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var subBooking1ExtraNonMatchingInstruction = subBooking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var subBooking2ExtraNonMatchingInstruction = subBooking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			masterBookingExtraNonMatchingInstruction.KN_Sequence = 5;
			subBooking1ExtraNonMatchingInstruction.KN_Sequence = 5;
			subBooking2ExtraNonMatchingInstruction.KN_Sequence = 5;

			var subBooking1Container = Helper.CreatePackageContainer("CONT1");
			var subBooking1ContainerInnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1ContainerInnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);
			subBooking1ContainerInnerLoosePackage.Packages.Add(subBooking1ContainerInnerInnerLoosePackage);

			var subBooking2Container = Helper.CreatePackageContainer("CONT2");
			var subBooking2LoosePackage = Helper.CreatePackage("LOOSE2", 1);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);
			subBooking2.PackageJob.Packages.Add(subBooking2Container);

			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);

			var masterInstructionPic1 = masterBooking.Instructions.First(i => i.KN_Sequence == 1);
			var masterInstructionPic2 = masterBooking.Instructions.First(i => i.KN_Sequence == 2);
			var masterInstructionMlt3 = masterBooking.Instructions.First(i => i.KN_Sequence == 3);
			var masterInstructionDlv4 = masterBooking.Instructions.First(i => i.KN_Sequence == 4);
			var masterInstructionDlv5 = masterBooking.Instructions.First(i => i.KN_Sequence == 5);
			var subBooking1InstructionPic1 = subBooking1.Instructions.First(i => i.KN_Sequence == 1);
			var subBooking1InstructionPic2 = subBooking1.Instructions.First(i => i.KN_Sequence == 2);
			var subBooking1InstructionMlt3 = subBooking1.Instructions.First(i => i.KN_Sequence == 3);
			var subBooking1InstructionDlv4 = subBooking1.Instructions.First(i => i.KN_Sequence == 4);
			var subBooking1InstructionDlv5 = subBooking1.Instructions.First(i => i.KN_Sequence == 5);
			var subBooking2InstructionPic1 = subBooking2.Instructions.First(i => i.KN_Sequence == 1);
			var subBooking2InstructionPic2 = subBooking2.Instructions.First(i => i.KN_Sequence == 2);
			var subBooking2InstructionMlt3 = subBooking2.Instructions.First(i => i.KN_Sequence == 3);
			var subBooking2InstructionDlv4 = subBooking2.Instructions.First(i => i.KN_Sequence == 4);
			var subBooking2InstructionDlv5 = subBooking2.Instructions.First(i => i.KN_Sequence == 5);

			var subBooking1LoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
			};
			var subBooking2LoosePackages = Array.Empty<(ZGuid packagePK, ZInt packageQty)>(); //intentionally missing subBooking2LoosePackage to check detaching does not default anything

			var subBooking1ContainerPackages = Array.Empty<(ZGuid packagePK, ZInt packageQty)>(); //intentionally missing subBooking1Container to check detaching does not default anything
			var subBooking2ContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var subBooking1BothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					//intentionally missing subBooking1ContainerInnerLoosePackage to check detaching does not default anything
			};
			var subBooking2BothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var subBooking1OuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
			};
			var subBooking2OuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			AssignDivotsToInstructions(subBooking1InstructionPic1, subBooking1LoosePackages);
			AssignDivotsToInstructions(subBooking1InstructionPic2, subBooking1ContainerPackages);
			AssignDivotsToInstructions(subBooking1InstructionMlt3, subBooking1BothPackages);
			AssignDivotsToInstructions(subBooking1InstructionDlv4, subBooking1ContainerPackages);
			subBooking1InstructionDlv5.PackageCategory = PackageCategories.Codes.Outers;
			AssignDivotsToInstructions(subBooking1InstructionDlv5, subBooking1OuterPackages);
			AssignDivotsToInstructions(subBooking2InstructionPic1, subBooking2LoosePackages);
			AssignDivotsToInstructions(subBooking2InstructionPic2, subBooking2ContainerPackages);
			AssignDivotsToInstructions(subBooking2InstructionMlt3, subBooking2BothPackages);
			AssignDivotsToInstructions(subBooking2InstructionDlv4, subBooking2ContainerPackages);
			subBooking2InstructionDlv5.PackageCategory = PackageCategories.Codes.Outers;
			AssignDivotsToInstructions(subBooking2InstructionDlv5, subBooking2OuterPackages);

			Factory.Save();

			var currentLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
				(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var currentContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var currentBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var currentOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var expectedLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var expectedContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var expectedBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			var expectedOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking2Container.PK, packageQty: subBooking2Container.KP_PackageQty),
			};

			CombineAssertions("Precondition: subBookings packages should be correctly assigned to subBookings instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, subBooking1LoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, subBooking1BothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, subBooking1OuterPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, subBooking2LoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking2ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, subBooking2BothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking2ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, subBooking2OuterPackages);
			});

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			var subBookingsToAttach = new List<DtbBooking>() { subBooking1, subBooking2 };

			masterBooking.SubBookings.AddRange(subBookingsToAttach);
			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookingsToAttach);

			CombineAssertions("Precondition: packages should be correctly defaulted to master instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, currentLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, currentBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, currentOuterPackages);
			});

			CombineAssertions("Precondition: packages should be unchanged on sub instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, subBooking1LoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, subBooking1BothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, subBooking1OuterPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, subBooking2LoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking2ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, subBooking2BothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking2ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking2InstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, subBooking2OuterPackages);
			});

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking1
			};
			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			CombineAssertions("subBooking1's packages should be correctly removed from master instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});

			CombineAssertions("subBooking1's packages should be unchanged", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, subBooking1LoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, subBooking1BothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, subBooking1ContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(subBooking1InstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, subBooking1OuterPackages);
			});
		}

		public void TestDefaultPackagesIfMasterFromSubBookingsForAllInstructions()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();

			masterBooking.KM_KT_NKBookingTemplate = "EFPL";
			subBooking3.KM_KT_NKBookingTemplate = "EFPL";

			masterBooking.SubBookings.AddRange(new DtbBooking[] { subBooking1, subBooking2 });
			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, masterBooking.Instructions.Count);
			var extraNonMatchingInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			extraNonMatchingInstruction.KN_Sequence = 5;
			var subBooking3extraNonMatchingInstruction = subBooking3.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			subBooking3extraNonMatchingInstruction.KN_Sequence = 5;

			var subBooking1Container = Helper.CreatePackageContainer("CONT1");
			var sharedContainer = Helper.CreatePackageContainer("CONT2");
			var subBooking1ContainerInnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1ContainerInnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container);
			subBooking1.PackageJob.Packages.Add(sharedContainer);
			subBooking1Container.Packages.Add(subBooking1ContainerInnerLoosePackage);
			subBooking1ContainerInnerLoosePackage.Packages.Add(subBooking1ContainerInnerInnerLoosePackage);

			var subBooking2LoosePackage = Helper.CreatePackage("LOOSE2", 1);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);
			subBooking2.PackageJob.Packages.Add(sharedContainer);

			var subBooking3Container1 = Helper.CreatePackageContainer("CONT3");
			var subBooking3Container1InnerLoosePackage = Helper.CreatePackage("CONT3_IN");
			var subBooking3Container1InnerInnerLoosePackage = Helper.CreatePackage("CONT3_IN_IN");
			subBooking3.PackageJob.Packages.Add(subBooking3Container1);
			subBooking3Container1.Packages.Add(subBooking3Container1InnerLoosePackage);
			subBooking3Container1InnerLoosePackage.Packages.Add(subBooking3Container1InnerInnerLoosePackage);
			var subBooking3Container2 = Helper.CreatePackageContainer("CONT4");
			var subBooking3Container2InnerLoosePackage = Helper.CreatePackage("CONT4_IN");
			var subBooking3Container2InnerInnerLoosePackage = Helper.CreatePackage("CONT4_IN_IN");
			subBooking3.PackageJob.Packages.Add(subBooking3Container2);
			subBooking3Container2.Packages.Add(subBooking3Container2InnerLoosePackage);
			subBooking3Container2InnerLoosePackage.Packages.Add(subBooking3Container2InnerInnerLoosePackage);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);

			var masterInstructionPic1 = masterBooking.Instructions.First(i => i.KN_Sequence == 1);
			var masterInstructionPic2 = masterBooking.Instructions.First(i => i.KN_Sequence == 2);
			var masterInstructionMlt3 = masterBooking.Instructions.First(i => i.KN_Sequence == 3);
			var masterInstructionDlv4 = masterBooking.Instructions.First(i => i.KN_Sequence == 4);
			var masterInstructionDlv5 = masterBooking.Instructions.First(i => i.KN_Sequence == 5);

			var expectedLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					//intentionally missing subBooking1ContainerInnerLoosePackage to check changes on other packages are not overridden by defaulting
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking3Container1InnerLoosePackage.PK, packageQty: subBooking3Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking3Container2InnerLoosePackage to check unassigned packages on a sub are not assigned to a master
			};

			var expectedContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					(packagePK: sharedContainer.PK, packageQty: sharedContainer.KP_PackageQty),
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					//intentionally missing subBooking3Container2 to check unassigned packages on a sub are not assigned to a master
			};

			var expectedBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					//intentionally missing subBooking1Container to check changes on other packages are not overridden by defaulting
					(packagePK: subBooking1ContainerInnerLoosePackage.PK, packageQty: subBooking1ContainerInnerLoosePackage.KP_PackageQty),
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					(packagePK: subBooking3Container1InnerLoosePackage.PK, packageQty: subBooking3Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking3Container2 and related package to check unassigned packages on a sub are not assigned to a master
			};

			var expectedOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container.PK, packageQty: subBooking1Container.KP_PackageQty),
					//intentionally missing subBooking2LoosePackage to check changes on other packages are not overridden by defaulting
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					(packagePK: subBooking3Container1InnerLoosePackage.PK, packageQty: subBooking3Container1InnerLoosePackage.KP_PackageQty), // DefaultPackagesCore code for addTopLevelLoose true doesn't check if a package is top level
					//intentionally missing subBooking3Container2 to check unassigned packages on a sub are not assigned to a master
			};

			var currentLoosePackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking3Container1.PK && lp.packagePK != subBooking3Container1InnerLoosePackage.PK).ToArray();

			var currentContainerPackages = expectedContainerPackages.Where(lp => lp.packagePK != subBooking3Container1.PK && lp.packagePK != subBooking3Container1InnerLoosePackage.PK).ToArray();

			var currentBothPackages = expectedBothPackages.Where(lp => lp.packagePK != subBooking3Container1.PK && lp.packagePK != subBooking3Container1InnerLoosePackage.PK).ToArray();

			var currentOuterPackages = expectedOuterPackages.Where(lp => lp.packagePK != subBooking3Container1.PK && lp.packagePK != subBooking3Container1InnerLoosePackage.PK && lp.packagePK != subBooking3Container1InnerLoosePackage.PK).ToArray();

			AssignDivotsToInstructions(masterInstructionPic1, currentLoosePackages);
			AssignDivotsToInstructions(masterInstructionPic2, currentContainerPackages);
			AssignDivotsToInstructions(masterInstructionMlt3, currentBothPackages);
			AssignDivotsToInstructions(masterInstructionDlv4, currentContainerPackages);
			masterInstructionDlv5.PackageCategory = PackageCategories.Codes.Outers;
			AssignDivotsToInstructions(masterInstructionDlv5, currentOuterPackages);

			CombineAssertions("Precondition: subBookings packages should be correctly assigned to instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, currentLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, currentBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, currentContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, currentOuterPackages);
			});

			var subBooking3InstructionPic1 = subBooking3.Instructions.First(i => i.KN_Sequence == 1);
			var subBooking3InstructionPic2 = subBooking3.Instructions.First(i => i.KN_Sequence == 2);
			var subBooking3InstructionMlt3 = subBooking3.Instructions.First(i => i.KN_Sequence == 3);
			var subBooking3InstructionDlv4 = subBooking3.Instructions.First(i => i.KN_Sequence == 4);
			var subBooking3InstructionDlv5 = subBooking3.Instructions.First(i => i.KN_Sequence == 5);

			var subBooking3LoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking3Container1InnerLoosePackage.PK, packageQty: subBooking3Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking3Container2InnerLoosePackage to check unassigned packages on a sub are not assigned to a master
			};

			var subBooking3ContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					//intentionally missing subBooking3Container2 to check unassigned packages on a sub are not assigned to a master
			};

			var subBooking3BothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					(packagePK: subBooking3Container1InnerLoosePackage.PK, packageQty: subBooking3Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking3Container2 and related package to check unassigned packages on a sub are not assigned to a master
			};

			var subBooking3OuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking3Container1.PK, packageQty: subBooking3Container1.KP_PackageQty),
					//intentionally missing subBooking3Container2 to check unassigned packages on a sub are not assigned to a master
			};

			AssignDivotsToInstructions(subBooking3InstructionPic1, subBooking3LoosePackages);
			AssignDivotsToInstructions(subBooking3InstructionPic2, subBooking3ContainerPackages);
			AssignDivotsToInstructions(subBooking3InstructionMlt3, subBooking3BothPackages);
			AssignDivotsToInstructions(subBooking3InstructionDlv4, subBooking3ContainerPackages);
			subBooking3InstructionDlv5.PackageCategory = PackageCategories.Codes.Outers;
			AssignDivotsToInstructions(subBooking3InstructionDlv5, subBooking3OuterPackages);

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking3);

			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(new List<DtbBooking>() { subBooking3 });

			CombineAssertions("subBooking3's packages should be correctly defaulted to instructions", () =>
			{
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
				AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
			});
		}

		void AssignDivotsToInstructions(DtbBookingInstruction instruction, (ZGuid packagePK, ZInt packageQty)[] divotDetails)
		{
			instruction.PackageDivots.DeleteAll();
			foreach (var divotDetail in divotDetails)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = divotDetail.packagePK;
				packageDivot.KD_Quantity = divotDetail.packageQty;
			}
		}

		public void TestDetachSingleSubBookingFromMasterBookingContainSingularSubBooking()
		{
			var masterBooking = Helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = Helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking.PackageJob.Packages.Add(subBookingPackage);

			var subBookings = new List<DtbBooking>()
			{
				subBooking
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage);

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBookings.ToArray());
			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking = Factory.Load<DtbBooking>(subBooking.PK);

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking
			};

			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking), subBooking, subBookingPackage);
			Assert("Master booking should have no sub bookings attached.", masterBooking.SubBookings.Count == 0);
		}

		public void TestDetachSingleSubBookingFromMasterBookingWithMultipleSubBookings()
		{
			var masterBooking = Helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = Helper.CreateBooking();
			var subBookingPackage1 = Helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = Helper.CreateBooking();
			var subBookingPackage2 = Helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var subBooking3 = Helper.CreateBooking();
			var subBookingPackage3 = Helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			subBooking3.PackageJob.Packages.Add(subBookingPackage3);

			var subBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage3);

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBookings.ToArray());
			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking1
			};

			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking1), subBooking1, subBookingPackage1);
			AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking2), subBooking2, masterBooking, subBookingPackage2);
			AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking3), subBooking3, masterBooking, subBookingPackage3);

			CombineAssertions("Master booking's sub bookings are incorrect", () =>
			{
				AssertCollectionNotContains($"Master booking should not have {nameof(subBooking1)} attached to it.", subBooking1, masterBooking.SubBookings);
				AssertCollectionContains($"Master booking is missing {nameof(subBooking2)}", subBooking2, masterBooking.SubBookings);
				AssertCollectionContains($"Master booking is missing {nameof(subBooking3)}", subBooking3, masterBooking.SubBookings);
			});
		}

		public void TestDetachMultipleSubBookingFromMasterBookingWithMultipleSubBookings()
		{
			var masterBooking = Helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = Helper.CreateBooking();
			var subBookingPackage1 = Helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = Helper.CreateBooking();
			var subBookingPackage2 = Helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var subBooking3 = Helper.CreateBooking();
			var subBookingPackage3 = Helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			subBooking3.PackageJob.Packages.Add(subBookingPackage3);

			var subBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage3);

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBookings.ToArray());
			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2
			};

			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking1), subBooking1, subBookingPackage1);
			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking2), subBooking2, subBookingPackage2);
			AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking3), subBooking3, masterBooking, subBookingPackage3);

			CombineAssertions("Master booking's sub bookings are incorrect", () =>
			{
				AssertCollectionNotContains($"Master booking should not have {nameof(subBooking1)} attached to it.", subBooking1, masterBooking.SubBookings);
				AssertCollectionNotContains($"Master booking should not have {nameof(subBooking2)} attached to it.", subBooking2, masterBooking.SubBookings);
				AssertCollectionContains($"Master booking is missing {nameof(subBooking3)}", subBooking3, masterBooking.SubBookings);
			});
		}

		public void TestDetachAllSubBookingFromMasterBookingWithMultipleSubBookings()
		{
			var masterBooking = Helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = Helper.CreateBooking();
			var subBookingPackage1 = Helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = Helper.CreateBooking();
			var subBookingPackage2 = Helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var subBooking3 = Helper.CreateBooking();
			var subBookingPackage3 = Helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			subBooking3.PackageJob.Packages.Add(subBookingPackage3);

			var subBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage3);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBookings.ToArray());
			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			var subBookingsToDetach = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			DetachSubBookingsFromMaster(subBookingsToDetach.ToArray());
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToDetach);

			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking1), subBooking1, subBookingPackage1);
			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking2), subBooking2, subBookingPackage2);
			AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking3), subBooking3, subBookingPackage3);
			Assert("Master booking should have no sub bookings attached.", masterBooking.SubBookings.Count == 0);
		}

		void AssignDivotsForPackageOnEachInstructionInBooking(DtbBooking booking, PkgPackage package)
		{
			foreach (var instruction in booking.Instructions)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = package.PK;
				packageDivot.KD_Quantity = package.KP_PackageQty;
			}
		}

		void CreateSubBookingInstructionAndLinkToMasterBookingInstruction(DtbBooking masterBooking, params DtbBooking[] subBookings)
		{
			foreach (var subBooking in subBookings)
			{
				foreach (var masterInstruction in masterBooking.Instructions)
				{
					var subInstruction = subBooking.Instructions.AddNew(masterInstruction.KN_InstructionType);
					subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
				}
			}
		}

		void DetachSubBookingsFromMaster(params DtbBooking[] subBookingsToDetach)
		{
			foreach (var subBooking in subBookingsToDetach)
			{
				subBooking.KM_KM_MasterBooking = ZGuid.Empty;
			}
		}

		void AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(string subBookingName, DtbBooking subBooking, PkgPackage package)
		{
			CombineAssertions($"{subBookingName} appears to be invalid", () => {
				Assert($"{subBookingName} no longer has its package", subBooking.PackageJob.Packages.Contains(package));
				Assert($"{subBookingName} no longer has its divots", subBooking.Instructions.All(i => i.PackageDivots.Any(pv => pv.Package.PK == package.PK)));
				Assert($"{subBookingName} Should only have two instruction", subBooking.Instructions.Count == 2);
				Assert($"{subBookingName} should not be associated with a master booking", subBooking.MasterBooking == null);
			});
		}

		void AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(string subBookingName, DtbBooking subBooking, DtbBooking masterBooking, PkgPackage package)
		{
			CombineAssertions($"{subBookingName} appears to be invalid", () =>
			{
				Assert($"{subBookingName} no longer has its package", subBooking.PackageJob.Packages.Contains(package));
				Assert($"{subBookingName} no longer has its proxy divots", subBooking.Instructions.All(i => i.DivotsWithPackages.Any(pv => pv.Package.PK == package.PK)));
				Assert($"{subBookingName} should only have two instruction", subBooking.Instructions.Count == 2);
				Assert($"{subBookingName} is no longer associated with the master booking", subBooking.MasterBooking == masterBooking);
			});
		}

		void AssertInstructionHasDefaultedCorrectPackageCategoryAndDivots(
			DtbBookingInstruction instruction,
			string expectedPackageCategoryCode,
			string expectedPackageCategoryDescription,
			(ZGuid packagePK, ZInt packageQty)[] expectedDivotDetails)
		{
			AssertEquals(
				FormattableString.Invariant($"DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions() should have set Instruction {instruction.KN_InstructionType} Sequence {instruction.KN_Sequence} PackageCategory to '{expectedPackageCategoryCode}' from the template"),
				expectedPackageCategoryCode,
				instruction.PackageCategory);
			AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions() should have assigned {expectedPackageCategoryDescription} packages to Instruction {instruction.KN_InstructionType} Sequence {instruction.KN_Sequence}"),
				DivotDetailDisplayTextProvider,
				expectedDivotDetails,
				instruction.DivotsWithPackages.Select(d => (packagePK: d.KD_KP_Package, packageQty: d.KD_Quantity)).ToArray());
		}

		string DivotDetailDisplayTextProvider((ZGuid packagePK, ZInt packageQty) divotDetail)
		{
			var package = Factory.Load<PkgPackage>(divotDetail.packagePK);
			return $"{package.HumanReadableName}, {divotDetail.packageQty}";
		}

		public void TestTotalPackages_ShouldOnlyCountTopLevelLooseAndContainerisedPackages()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;

			var instruction = booking.Instructions.AddNew();
			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var box = container.Packages.AddNew(Constants.PkgUnit.Box, 5);
			var pallet = container.Packages.AddNew(Constants.PkgUnit.Pallet, 3);

			box.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			pallet.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			Helper.CreatePackageDivot(instruction, container);

			var loosePackage = Factory.New<PkgPackage>();
			loosePackage.KP_PackageQty = 3;
			loosePackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			Helper.CreatePackageDivot(instruction, loosePackage);

			var nonAssignedPackage = Factory.New<PkgPackage>();
			nonAssignedPackage.KP_PackageQty = 5;
			nonAssignedPackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			box.Packages.AddNew(Constants.PkgUnit.Package, 2);
			loosePackage.Packages.AddNew(Constants.PkgUnit.Package, 4);

			AssertEquals("All top level containerised and loose packages are counted", 11, booking.TotalPackages);
		}

		public void TestTotalPackages_WhenDuplicatePackagesAreLooseAndContainerised()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;

			var instruction = booking.Instructions.AddNew();
			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var box = container.Packages.AddNew(Constants.PkgUnit.Box, 5);
			var pallet = container.Packages.AddNew(Constants.PkgUnit.Pallet, 3);
			box.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			pallet.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			Helper.CreatePackageDivot(instruction, container);
			Helper.CreatePackageDivot(instruction, box);
			Helper.CreatePackageDivot(instruction, pallet);

			AssertEquals("Duplicate packages should not be counted", 8, booking.TotalPackages);
		}

		public void TestTotalPackagesType()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			var instruction = booking.Instructions.AddNew();
			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			Helper.CreatePackageDivot(instruction, container);

			var box1 = container.Packages.AddNew(Constants.PkgUnit.Box, 3);
			box1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var box2 = container.Packages.AddNew(Constants.PkgUnit.Box, 2);
			box2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var crate = box1.Packages.AddNew(Constants.PkgUnit.Crate, 10);
			crate.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			AssertEquals(Constants.PkgUnit.Box, booking.TotalPackagesType);

			var pallet = Factory.New<PkgPackage>();
			pallet.KP_F3_NKPackType = Constants.PkgUnit.Pallet;
			pallet.KP_PackageQty = 6;
			pallet.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			AssertEquals(Constants.PkgUnit.Box, booking.TotalPackagesType);

			Helper.CreatePackageDivot(instruction, pallet);

			AssertEquals(Constants.PkgUnit.Package, booking.TotalPackagesType);
		}

		public void TestTotalWeightExcludingDuplicatePackages()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;

			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			container.KP_Weight = 100;

			var box = Factory.New<PkgPackage>();
			box.KP_F3_NKPackType = Constants.PkgUnit.Box;
			box.KP_PackageQty = 1;

			var pallet = Factory.New<PkgPackage>();
			pallet.KP_F3_NKPackType = Constants.PkgUnit.Pallet;
			pallet.KP_PackageQty = 1;

			box.KP_Weight = 200;
			pallet.KP_Weight = 300;

			container.Packages.Add(box);
			container.Packages.Add(pallet);

			box.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			pallet.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			Helper.CreatePackageDivot(instruction, container);
			Helper.CreatePackageDivot(instruction, box);
			Helper.CreatePackageDivot(instruction, pallet);

			AssertEquals("Duplicate packages should not be counted", (ZDecimal)600, booking.TotalWeightExcludingDuplicatePackages.Amount);
		}

		public void TestTotalVolumeExcludingDuplicatePackages()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;

			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var box = container.Packages.AddNew(Constants.PkgUnit.Box, 1);
			var pallet = container.Packages.AddNew(Constants.PkgUnit.Pallet, 1);
			box.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			pallet.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			Helper.CreatePackageDivot(instruction, container);
			Helper.CreatePackageDivot(instruction, box);
			Helper.CreatePackageDivot(instruction, pallet);

			container.KP_Volume = 700;
			box.KP_Volume = 200;
			pallet.KP_Volume = 300;

			AssertEquals("Duplicate packages should not be counted", (ZDecimal)700, booking.TotalVolumeExcludingDuplicatePackages.Amount);
		}

		public void TestJS_Calc_ActualVolumeWeightAndUnit_InvalidConversionsReturnZero()
		{
			var metricFactor = ConversionFactor.Standard.Metric.LoadingMeters;
			var imperialFactor = ConversionFactor.Standard.Imperial.Domestic;
			var factor = new ChargeableFactor(metricFactor, imperialFactor);

			using (TransportRegistry.Instance.TransportBookingChargeableFactor.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, factor))
			{
				Factory.Save();

				var shipment = Factory.New<IForwardingShipment>();
				var consolidation = Helper.CreateConsolidation();
				var booking = consolidation.Bookings.AddNew();
				booking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
				booking.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
				var instruction = booking.Instructions.AddNew();
				instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;

				shipment.JS_RL_NKOrigin = "AUMEL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				shipment.JS_ActualWeight = 100m;
				shipment.JS_ActualVolume = 1m;
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				AssertEquals("Pre-condition: Chargeable Unit", Constants.Weight.Kilograms, booking.KM_ChargeableUnit);
				AssertEquals("Pre-condition: Actual Unit", Constants.Weight.Kilograms, booking.TotalWeight.Unit);
				AssertEquals("No conversion path between M3 to KG/LM so 0 is expected", 0m, booking.KM_Calc_ActualVolumeWeight);
			}
		}

		public void TestKM_Calc_ActualVolumeWeight_WithoutWeightChargeableTransportMode()
		{
			var shipment = Factory.New<IForwardingShipment>();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUPER";
			shipment.JS_TransportMode = Constants.TransportModes.Road;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var instruction = booking.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms))
			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres))
			{
				var loosePackage = Factory.New<PkgPackage>();
				loosePackage.KP_PackageQty = 3;
				loosePackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
				Helper.CreatePackageDivot(instruction, loosePackage);

				loosePackage.KP_Weight = 300m;
				loosePackage.KP_Volume = 10m;

				AssertEquals("Weight Volume: convert actual volume to weight", 3333.333333m, booking.KM_Calc_ActualVolumeWeight);
				AssertEquals("Chargeable Unit", Constants.Weight.Kilograms, booking.KM_Calc_ActualVolumeWeightUnit);
			}
		}

		public void TestKM_ChargeableUnit_ReturnsValueForRoadTransportMode()
		{
			var shipment = Factory.New<IForwardingShipment>();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUPER";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms))
			{
				AssertEquals("Chargeable Unit", Constants.Weight.Kilograms, booking.KM_Calc_ActualVolumeWeightUnit);
			}
		}

		public void TestParentTransportMode()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_ParentID = shipment.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;

			Factory.Save();

			AssertEquals("Doesn't return TransportMode of parent job", Constants.TransportModes.Sea, booking.ParentTransportMode);
		}

		public void TestParentTransportModeWhenParentJobIsNull()
		{
			var booking = Helper.CreateBooking();

			Factory.Save();

			AssertEquals("Doesn't handle null parent job", null, booking.ParentTransportMode);
		}

		public void TestIsPickupCommenced()
		{
			AssertFlag("IsPickupCommenced", DtbBookingSchema.KM_Status, TransportStatuses.Codes.PickUpCommenced, TransportStatuses.Codes.Available);
		}

		public void TestIsPickUpConfirmed()
		{
			AssertFlag("IsPickUpConfirmed", DtbBookingSchema.KM_Status, TransportStatuses.Codes.PickUpConfirmed, TransportStatuses.Codes.Available);
		}

		public void TestIsQuote()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.Quote;
			AssertEquals(true, booking.IsQuote);

			booking.KM_Status = TransportStatuses.Codes.Available;
			AssertEquals(false, booking.IsQuote);
		}

		public void TestDefaultValues()
		{
			var booking = Factory.New<DtbBooking>();
			AssertEquals(TransportConsolidationJobTypes.Codes.Booking, booking.KM_JobType);
		}

		public void TestSetDefaultValues_TransportCompany()
		{
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var booking = Factory.New<DtbBooking>();
			AssertEquals(Guid.Empty, booking.Address.OrganisationPK);
			AssertEquals(Guid.Empty, booking.Address.E2_OA_Address);

			TransportRegistry.Instance.TransportBookingDefaultTransportCompany.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, transportCompany.PK.ToGuid());

			booking = Factory.New<DtbBooking>();
			AssertEquals(transportCompany.PK, booking.Address.OrganisationPK);
			AssertEquals(transportCompany.MainAddress.PK, booking.Address.E2_OA_Address);
		}

		public void TestHasServiceCommencedLogs()
		{
			var booking = Helper.CreateBooking();
			booking.UpdateStatus();

			var pickUpInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			pickUpInstruction1.Confirmations.AddNew();
			pickUpInstruction2.Confirmations.AddNew();
			deliveryInstruction1.Confirmations.AddNew();
			deliveryInstruction2.Confirmations.AddNew();
			Factory.Save();

			var dbCount = Factory.DatabaseLoadCount;
			AssertEquals("No log events, HasServiceCommencedLogs should be false", false, booking.HasServiceCommencedLogs);
			AssertEquals("Should only hit db three times.", dbCount + 3, Factory.DatabaseLoadCount);

			_ = booking.HasServiceCommencedLogs;
			AssertEquals("Should not hit db again in repeated query", dbCount + 3, Factory.DatabaseLoadCount);

			deliveryInstruction2.Confirmations.First().Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now, isEstimate: false);
			AssertEquals("Service commenced log added, HasServiceCommencedLogs should be true", true, booking.HasServiceCommencedLogs);

			deliveryInstruction2.Delete();
			AssertEquals("The confirmation the log was added to is deleted, HasServiceCommencedLogs should be false", false, booking.HasServiceCommencedLogs);

			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now, isEstimate: false);
			AssertEquals("Service commenced log added to booking, HasServiceCommencedLogs should be true", true, booking.HasServiceCommencedLogs);
		}

		public void TestHasServiceCommencedLogs_ChecksForServiceCancelledLogs()
		{
			var booking = Helper.CreateBooking();
			booking.UpdateStatus();

			var pickUpInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			pickUpInstruction1.Confirmations.AddNew();
			pickUpInstruction2.Confirmations.AddNew();
			deliveryInstruction1.Confirmations.AddNew();
			deliveryInstruction2.Confirmations.AddNew();
			Factory.Save();

			deliveryInstruction2.Confirmations.First().Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now, isEstimate: false);
			AssertEquals("Service commenced log added, HasServiceCommencedLogs should be true", true, booking.HasServiceCommencedLogs);

			deliveryInstruction2.Confirmations.First().Logs.AddNew(Events.ServiceCancelled, "", ZDateTimeOffset.Now.AddDays(1), isEstimate: false);
			AssertEquals("Service cancelled log added, HasServiceCommencedLogs should be false", false, booking.HasServiceCommencedLogs);

			deliveryInstruction2.Confirmations.First().Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now.AddDays(2), isEstimate: false);
			AssertEquals("Service commenced log more recent than service cancelled log, HasServiceCommencedLogs should be true", true, booking.HasServiceCommencedLogs);
		}

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingValidation); }
		}

		public void TestEntryTypeShouldBeUnique_ReturnsTrueForTrueRegistryValue()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = true;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBooking>();
				AssertEquals(true, booking.EntryTypeShouldBeUnique(AdditionalReferenceTypes.Codes.OrderNumber, "OTH", "AU"));
			}
		}

		public void TestEntryTypeShouldBeUnique_ReturnsFalseForFalseRegistryValue()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = false;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBooking>();
				AssertEquals(false, booking.EntryTypeShouldBeUnique(AdditionalReferenceTypes.Codes.OrderNumber, "OTH", "AU"));
			}
		}

		public void TestWhenEntryTypeShouldBeUniqueIsFalse_ShouldAllowDuplicateEntries()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = false;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBooking>();

				var entryNum1 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum1.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum1.CE_EntryNum = "AAA";

				var entryNum2 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum2.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum2.CE_EntryNum = "BBB";

				Factory.Save();

				AssertNoNotifications(entryNum2.CE_EntryTypeInfo);
			}
		}

		public void TestWhenEntryTypeShouldBeUniqueIsFalse_ShouldNotAllowDuplicateEntries()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = true;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBooking>();

				var entryNum1 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum1.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum1.CE_EntryNum = "AAA";

				var entryNum2 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum2.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum2.CE_EntryNum = "BBB";

				Factory.Save();

				AssertHasError(entryNum2.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
			}
		}

		public void TestAdditionalReferenceNumbers_CE_EntryType_SystemValue_ShouldAlwaysBeValid()
		{
			var list = new TransportReferenceNumberTypeCollection
			{
				{ "AAA", (NoResString)"A Desc" },
				{ "BBB", (NoResString)"B Desc" },
			};

			TransportRegistry.Instance.AdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var booking = Factory.NewWithValidTestData<DtbBooking>();

			var entryNum1 = booking.AdditionalReferenceNumbers.AddNew();
			entryNum1.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.CarrierBookingReference;
			entryNum1.CE_EntryNum = "123";

			Factory.Save();

			AssertNoNotifications("Should not have any notifications about a system value CE_EntryType", entryNum1.CE_EntryTypeInfo);
		}

		public void TestUpdateStatus_WithEmptyDeliveries()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Precondition: IsDeliveryDirection is false", false, booking.IsDeliveryDirection);

			// booking with no instructions

			booking.KM_Status = "";
			AssertEquals("Precondition", "", booking.KM_Status);

			booking.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Available, booking.KM_Status);

			// booking with pickup instructions

			var pickUpInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			booking.UpdateStatus();
			AssertEquals("PickUp Instructions exists but one is not PickedUp, Status should fallback to Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			pickUpInstruction1.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("First PickUp Instruction is picked up, Status should be Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			pickUpInstruction2.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("Pickup Instructions are all picked up, Status should be PickedUp.", TransportStatuses.Codes.PickedUp, booking.KM_Status);

			var cneMultiConfirmation1 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneMultiConfirmation2 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cydDeliveryConfirmation1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var cydDeliveryConfirmation2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			cydDeliveryConfirmation1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			cydDeliveryConfirmation2.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;

			pickUpInstruction2.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("Pickup Instructions are all picked up, but CNE Multi Instructions exists and are not PickedUp, and IsDeliveryDirection is false, Status should fallback to Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			cneMultiConfirmation1.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals("Pickup Instructions are all picked up, but CNE Multi Instructions exists and are not PickedUp, and IsDeliveryDirection is false, Status should fallback to Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			cneMultiConfirmation2.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals("All instructions except Empties are Delivered, Status should be Delivered Not Returned", TransportStatuses.Codes.DeliveredEmptyNotReturned, booking.KM_Status);

			cydDeliveryConfirmation1.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals("All instructions except Empties are Delivered, Status should be Delivered Not Returned", TransportStatuses.Codes.DeliveredEmptyNotReturned, booking.KM_Status);

			cydDeliveryConfirmation2.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals("All instructions are now delivered, Status should be Delivered", TransportStatuses.Codes.Delivered, booking.KM_Status);
		}

		public void TestSaveFailureDeletesStatusLog()
		{
			var booking = Helper.CreateBooking();
			booking.IsCancelled = true;

			void ThrowException(BusinessObjectFactory f)
			{
				f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f));
			}

			Factory.Saving += ThrowException;

			bool exceptionThrown = false;

			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{
				exceptionThrown = true;
				AssertEquals("Status Log should have been cleared.", 0, booking.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode));
			}

			Assert("Ensure Save failed.", exceptionThrown);
		}

		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			BusinessObjectFactory Factory { get; }

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				throw new ZCannotSaveException("Test", "Test");
			}
		}

		public void TestDeactivateBooking_FiresCancelledEvent()
		{
			int cancelledHitCount = 0;
			var booking = Helper.CreateBooking();
			booking.Cancelled += (sender, e) => cancelledHitCount++;
			AssertEquals("Precondition: Cancelled Event not fited.", 0, cancelledHitCount);

			booking.IsCancelled = true;
			AssertEquals("Cancelled Event should fire when cancelled.", 1, cancelledHitCount);

			booking.IsCancelled = false;
			AssertEquals("Cancelled Event should *not* fire when uncancelled.", 1, cancelledHitCount);
		}

		public void TestDoNotDeleteJobHeaderForTransportBookingWithAParent()
		{
			var shipment = Helper.CreateForwardingShipment("123", "", "", "");
			var shipmentJob = Helper.LoadOrCreateJobHeader((IJobHeaderParent)shipment);
			shipmentJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			Factory.Save();

			var bookingWithParent = Helper.CreateBooking(consolidation);
			AssertNotNull(bookingWithParent.Job);
			AssertEquals(shipmentJob, bookingWithParent.Job);

			bookingWithParent.Delete();
			AssertEquals("Shipment Job shouldn't be deleted because it belongs to a shipment and not the transport booking.", false, shipmentJob.IsDeleted);
		}

		[TestDate(2016, 3, 1, 9, 0, 0)]
		public void TestUpdateDeliveryConfirmationEstimateTimeWhenTransitTimeDoesNotExists()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			Factory.Save();

			var booking = Helper.CreateBooking();

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_Estimated = new ZDateTime(2016, 3, 1, 10, 0, 0, 0); //before 12:00 - 1hr
			AssertEquals(ZDate.Empty, dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);

			dlvInstruction.LastDeliveryConfirmation.KK_Estimated = new ZDateTime(2016, 03, 18, 0, 0, 0);
			AssertEquals(new ZDate(2016, 03, 18), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);

			picInstruction.FirstPickupConfirmation.KK_Estimated = new ZDateTime(2016, 03, 15, 0, 0, 0);
			AssertEquals(new ZDate(2016, 03, 18), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);

			dlvInstruction.LastDeliveryConfirmation.KK_Estimated = ZDateTime.Empty;
			picInstruction.FirstPickupConfirmation.KK_Estimated = new ZDateTime(2016, 3, 3, 10, 0, 0);
			AssertEquals(ZDate.Empty, dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);

			picInstruction.FirstPickupConfirmation.KK_Estimated = ZDateTime.Empty;
			dlvInstruction.LastDeliveryConfirmation.KK_Estimated = new ZDateTime(2016, 03, 14, 0, 0, 0);
			AssertEquals(new ZDate(2016, 03, 14), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		[TestDate(2015, 7, 13, 9, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_SameDay()
		{
			AssertUpdateConfirmationEstimateTime_SameDay(
				() => WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK),
				(booking) => AssertNull("No job header is created therefore current department working hours should be used.", booking.Job));
		}

		[TestDate(2015, 7, 13, 9, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_SameDay_WithJobHeader()
		{
			var departmentForJobHeader = Helper.CreateDepartment("D");
			AssertUpdateConfirmationEstimateTime_SameDay(
				() =>
				{
					// Current department only works on Sundays
					WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Sunday, WorkingDaysTestHelper.NineToFive);

					// Department for job header works on week days 9 to 5
					WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, departmentForJobHeader.PK);
				},
				(booking) =>
				{
					var job = Helper.LoadOrCreateJobHeader(booking);
					AssertEquals("Precondition", job, booking.Job);
					booking.Job.JH_GE = departmentForJobHeader.PK;
				}
				);
		}

		void AssertUpdateConfirmationEstimateTime_SameDay(Action setUpDepartmentInfo, Action<DtbBooking> setupAndAssertJobHeader)
		{
			setUpDepartmentInfo();

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;

			var sydZone = Helper.CreateZone("SYDZONE", zoneSet);
			Helper.AddPostCodesToZone(sydZone, "2001", "3000");
			var sydPortHubSellectionPivot = SetupPortHubZonePivot("PDEP", "PIC", new ZDateTime(2015, 7, 13, 12, 0, 0, 0), sydZone, 60);

			var melZone = Helper.CreateZone("MELZONE", zoneSet);
			Helper.AddPostCodesToZone(melZone, "3001", "4000");
			var melPortHubSellectionPivot = SetupPortHubZonePivot("DDEP", "DLV", ZDateTime.Empty, melZone, 0);

			var serviceLevel = Helper.CreateServiceLevel("TST");
			var transitTime = Helper.CreateTransitTime("TST", 24, sydZone.PK, melZone.PK);  //next day delivery

			Factory.Save();

			var booking = Helper.CreateBooking();
			booking.KM_RS_NKServiceLevel = serviceLevel.RS_Code;
			setupAndAssertJobHeader(booking);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_RequiredFrom = new ZDateTime(2015, 7, 13, 10, 0, 0, 0);//before 12:00 - 1hr

			AssertEquals(new ZDate(2015, 07, 13), picInstruction.FirstPickupConfirmation.KK_Estimated.Date);
			AssertEquals(new ZDate(2015, 07, 14), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		[TestDate(2015, 7, 13, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_NextWorkingDay()
		{
			AssertUpdateConfirmationEstimateTime_NextWorkingDay(
				() => WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK),
				(booking) => AssertNull("No job header is created therefore current department working hours should be used.", booking.Job));
		}

		[TestDate(2015, 7, 13, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_WithJobHeader()
		{
			var departmentForJobHeader = Helper.CreateDepartment("D");
			AssertUpdateConfirmationEstimateTime_NextWorkingDay(
				() =>
				{
					// Current department only works on Sundays
					WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Sunday, WorkingDaysTestHelper.NineToFive);

					// Department for job header works on week days 9 to 5
					WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, departmentForJobHeader.PK);
				},
				(booking) =>
				{
					var job = Helper.LoadOrCreateJobHeader(booking);
					AssertEquals("Precondition", job, booking.Job);
					booking.Job.JH_GE = departmentForJobHeader.PK;
				});
		}

		void AssertUpdateConfirmationEstimateTime_NextWorkingDay(Action setUpDepartmentInfo, Action<DtbBooking> setupAndAssertJobHeader)
		{
			setUpDepartmentInfo();

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var sydZone = Helper.CreateZone("SYDZONE", zoneSet);
			Helper.AddPostCodesToZone(sydZone, "2001", "3000");
			var sydPortHubSellectionPivot = SetupPortHubZonePivot("PDEP", "PIC", new ZDateTime(2015, 7, 13, 14, 0, 0, 0), sydZone, -60);

			var melZone = Helper.CreateZone("MELZONE", zoneSet);
			Helper.AddPostCodesToZone(melZone, "3001", "4000");
			var melPortHubSellectionPivot = SetupPortHubZonePivot("DDEP", "DLV", ZDateTime.Empty, melZone, 0);

			var serviceLevel = Helper.CreateServiceLevel("TST");
			var transitTime = Helper.CreateTransitTime("TST", 24, sydZone.PK, melZone.PK);  //next day delivery

			Factory.Save();

			var booking = Helper.CreateBooking();
			booking.KM_RS_NKServiceLevel = serviceLevel.RS_Code;
			setupAndAssertJobHeader(booking);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_RequiredFrom = new ZDateTime(2015, 7, 13, 14, 0, 0, 0);   //after 14:00 - 1hr

			AssertEquals(new ZDate(2015, 07, 14), picInstruction.FirstPickupConfirmation.KK_Estimated.Date);
			AssertEquals(new ZDate(2015, 07, 15), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		[UseSnapshotProtection]
		public void TestAfterOneUserPickTheOrderAnotherUserChangeProduct()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();

			var secondFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var bookingInSecondFactory = secondFactory.Load<DtbBooking>(booking.PK);
			bookingInSecondFactory.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			secondFactory.Save();
			AssertNoExceptionThrown("There should be no issue if the booking has no changes.", () => Factory.Save());

			booking.HasChanges = true;
			AssertExceptionThrown("Since the status value has changed, exception should be thrown.",
				typeof(ZCannotSaveException), "Consignments have already been created for this Booking, no changes can be saved.", () => Factory.Save());

			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertNoExceptionThrown("There should be no issue saving if both are commenced.", () => Factory.Save());
		}

		[TestDate(2015, 7, 13, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInPast()
		{
			AssertUpdateConfirmationEstimateTime_ReqFromInPast(
				() => WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK),
				(booking) => AssertNull("No job header is created therefore current department working hours should be used.", booking.Job));
		}

		[TestDate(2015, 7, 13, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInPast_WithJobHeader()
		{
			var departmentForJobHeader = Helper.CreateDepartment("D");
			AssertUpdateConfirmationEstimateTime_ReqFromInPast(() =>
			{
				// Current department only works on Saturdays
				WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Saturday, WorkingDaysTestHelper.NineToFive);

				// Department for job header works on week days 9 to 5
				WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, departmentForJobHeader.PK);
			},
				(booking) =>
				{
					var job = Helper.LoadOrCreateJobHeader(booking);
					AssertEquals("Precondition", job, booking.Job);
					booking.Job.JH_GE = departmentForJobHeader.PK;
				});
		}

		void AssertUpdateConfirmationEstimateTime_ReqFromInPast(Action setUpDepartmentInfo, Action<DtbBooking> setupAndAssertJobHeader)
		{
			setUpDepartmentInfo();

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var sydZone = Helper.CreateZone("SYDZONE", zoneSet);
			Helper.AddPostCodesToZone(sydZone, "2001", "3000");
			var sydPortHubSellectionPivot = SetupPortHubZonePivot("PDEP", "PIC", new ZDateTime(2015, 7, 13, 12, 0, 0, 0), sydZone, -60);

			var melZone = Helper.CreateZone("MELZONE", zoneSet);
			Helper.AddPostCodesToZone(melZone, "3001", "4000");
			var melPortHubSellectionPivot = SetupPortHubZonePivot("DDEP", "DLV", ZDateTime.Empty, melZone, 0);

			var serviceLevel = Helper.CreateServiceLevel("TST");
			var transitTime = Helper.CreateTransitTime("TST", 24, sydZone.PK, melZone.PK);  //next day delivery

			Factory.Save();

			var booking = Helper.CreateBooking();
			booking.KM_RS_NKServiceLevel = serviceLevel.RS_Code;
			setupAndAssertJobHeader(booking);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_RequiredFrom = new ZDateTime(2015, 7, 12, 13, 0, 0, 0);   //after 12:00 - 1hr

			AssertEquals(new ZDate(2015, 07, 13), picInstruction.FirstPickupConfirmation.KK_Estimated.Date);
			AssertEquals(new ZDate(2015, 07, 14), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		[TestDate(2015, 11, 24, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInFuture()
		{
			AssertUpdateConfirmationEstimateTime_ReqFromInFuture(
				() => WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK),
				(booking) => AssertNull("No job header is created therefore current department working hours should be used.", booking.Job));
		}

		[TestDate(2015, 11, 24, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInFuture_WithJobHeader()
		{
			var departmentForJobHeader = Helper.CreateDepartment("D");
			AssertUpdateConfirmationEstimateTime_ReqFromInFuture(
				() =>
				{
					// Current department only works on Mondays
					WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, WorkingDaysTestHelper.NineToFive);

					// Department for job header works on week days 9 to 5
					WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, departmentForJobHeader.PK);
				},
				(booking) =>
				{
					var job = Helper.LoadOrCreateJobHeader(booking);
					AssertEquals("Precondition", job, booking.Job);
					booking.Job.JH_GE = departmentForJobHeader.PK;
				});
		}

		void AssertUpdateConfirmationEstimateTime_ReqFromInFuture(Action setUpDepartmentInfo, Action<DtbBooking> setupAndAssertJobHeader)
		{
			setUpDepartmentInfo();

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var sydZone = Helper.CreateZone("SYDZONE", zoneSet);
			Helper.AddPostCodesToZone(sydZone, "2001", "3000");
			var sydPortHubSellectionPivot = SetupPortHubZonePivot("PDEP", "PIC", new ZDateTime(2015, 11, 24, 12, 0, 0, 0), sydZone, -60);

			var melZone = Helper.CreateZone("MELZONE", zoneSet);
			Helper.AddPostCodesToZone(melZone, "3001", "4000");
			var melPortHubSellectionPivot = SetupPortHubZonePivot("DDEP", "DLV", ZDateTime.Empty, melZone, 0);

			var serviceLevel = Helper.CreateServiceLevel("TST");
			var transitTime = Helper.CreateTransitTime("TST", 24, sydZone.PK, melZone.PK);  //next day delivery

			Factory.Save();

			var booking = Helper.CreateBooking();
			booking.KM_RS_NKServiceLevel = serviceLevel.RS_Code;
			setupAndAssertJobHeader(booking);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_RequiredFrom = new ZDateTime(2015, 11, 27, 14, 0, 0, 0);  //after 12:00 - 1hr

			AssertEquals("Since required from is in future, cut off time should be ignored.", new ZDate(2015, 11, 27), picInstruction.FirstPickupConfirmation.KK_Estimated.Date);
			AssertEquals("Since required from is in future, cut off time should be ignored but department working hours must be honoured.", new ZDate(2015, 11, 30), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		[TestDate(2015, 11, 24, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInFutureAndNonWorkingDay()
		{
			AssertUpdateConfirmationEstimateTime_ReqFromInFutureAndNonWorkingDay(
				() => WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK),
				(booking) => AssertNull("No job header is created therefore current department working hours should be used.", booking.Job));
		}

		[TestDate(2015, 11, 24, 12, 0, 0)]
		public void TestUpdateConfirmationEstimateTime_ReqFromInFutureAndNonWorkingDay_WithJobHeader()
		{
			var departmentForJobHeader = Helper.CreateDepartment("D");
			AssertUpdateConfirmationEstimateTime_ReqFromInFutureAndNonWorkingDay(
				() =>
				{
					// Current department only works on Mondays
					WorkingDaysTestHelper.UpdateDepartmentDay(Factory, Env.CurrentDepartment.PK, DayOfWeek.Monday, WorkingDaysTestHelper.NineToFive);

					// Department for job header works on week days 9 to 5
					WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, departmentForJobHeader.PK);
				},
				(booking) =>
				{
					var job = Helper.LoadOrCreateJobHeader(booking);
					AssertEquals("Precondition", job, booking.Job);
					booking.Job.JH_GE = departmentForJobHeader.PK;
				});
		}

		void AssertUpdateConfirmationEstimateTime_ReqFromInFutureAndNonWorkingDay(Action setUpDepartmentInfo, Action<DtbBooking> setupAndAssertJobHeader)
		{
			setUpDepartmentInfo();

			var pickupOrganisation = SetupOrganisation("PORG", "2001");
			var deliveryOrganisation = SetupOrganisation("DORG", "3001");

			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			var sydZone = Helper.CreateZone("SYDZONE", zoneSet);
			Helper.AddPostCodesToZone(sydZone, "2001", "3000");
			var sydPortHubSellectionPivot = SetupPortHubZonePivot("PDEP", "PIC", new ZDateTime(2015, 11, 24, 12, 0, 0, 0), sydZone, -60);

			var melZone = Helper.CreateZone("MELZONE", zoneSet);
			Helper.AddPostCodesToZone(melZone, "3001", "4000");
			var melPortHubSellectionPivot = SetupPortHubZonePivot("DDEP", "DLV", ZDateTime.Empty, melZone, 0);

			var serviceLevel = Helper.CreateServiceLevel("TST");
			var transitTime = Helper.CreateTransitTime("TST", 24, sydZone.PK, melZone.PK);  //next day delivery

			Factory.Save();

			var booking = Helper.CreateBooking();
			booking.KM_RS_NKServiceLevel = serviceLevel.RS_Code;
			setupAndAssertJobHeader(booking);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			picInstruction.FirstPickupConfirmation.KK_RequiredFrom = new ZDateTime(2015, 11, 28, 14, 0, 0, 0);  //after 12:00 - 1hr

			AssertEquals("Eventhough required from is in future, working days and times should be honoured.", new ZDate(2015, 11, 30), picInstruction.FirstPickupConfirmation.KK_Estimated.Date);
			AssertEquals("Eventhough required from is in future, working days and times should be honoured.", new ZDate(2015, 12, 1), dlvInstruction.LastDeliveryConfirmation.KK_Estimated.Date);
		}

		IPortHubZonePivot SetupPortHubZonePivot(ZString depotCode, ZString direction, ZDateTime depotPickupCutOffTime, RateTransportZone zone, ZShort pivotPickupCutOffTimeVariance)
		{
			var depotOrganisation = Helper.CreateOrganisation(depotCode);
			var portHubSellection = Helper.CreatePortHub(depotOrganisation.MainAddress);
			Helper.SetPortHubDirection(portHubSellection, direction);
			var pivot = Helper.CreatePortHubZonePivot(portHubSellection, zone);
			pivot.TX_PickupCutOffTimeVariance = pivotPickupCutOffTimeVariance;
			return pivot;
		}

		OrgHeader SetupOrganisation(ZString code, ZString postCode)
		{
			var organisation = Helper.CreateOrganisation(code);
			organisation.MainAddress.OA_PostCode = postCode;
			organisation.MainAddress.OA_RL_NKRelatedPortCode = UNLOCO.RL_Code;
			return organisation;
		}

		RefUNLOCO UNLOCO
		{
			get { return unloco ?? (unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))); }
		}

		RefUNLOCO unloco;

		public void TestSetIsEmptyContainerOnAllRelatedConfirmations()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";

			var container1 = Helper.CreatePackageContainer("CONT1");
			var container1Package = Helper.CreatePackage("CONT1_IN");
			booking.PackageJob.Packages.Add(container1);
			container1.Packages.Add(container1Package);

			var container2 = Helper.CreatePackageContainer("CONT2");
			var container2Package = Helper.CreatePackage("CONT2_IN");
			booking.PackageJob.Packages.Add(container2);
			container2.Packages.Add(container2Package);

			booking.KM_KT_NKBookingTemplate = "IFUD";

			AssertEquals("Precondition: Adding the template 'IFUD' should have added four instructions", 4, booking.Instructions.Count);

			var pickupInstruction1 = booking.Instructions.Single(i => i.KN_Sequence == 1 && i.KN_InstructionType == "PIC" && i.OrganisationType == "CTO" && i.PackageCategory == "CNT");
			var multiInstructionWithoutConfirmations2 = booking.Instructions.Single(i => i.KN_Sequence == 2 && i.KN_InstructionType == "MLT" && i.OrganisationType == "CFS" && i.PackageCategory == "BTH");
			var deliveryToCNEInstruction3 = booking.Instructions.Single(i => i.KN_Sequence == 3 && i.KN_InstructionType == "DLV" && i.OrganisationType == "CNE" && i.PackageCategory == "LSE");
			var containerReturnDeliveryInstruction4 = booking.Instructions.Single(i => i.KN_Sequence == 4 && i.KN_InstructionType == "DLV" && i.OrganisationType == "CYD" && i.PackageCategory == "CNT");

			var pickupConfirmation = pickupInstruction1.Confirmations.Single();
			AssertEquals($"Precondition: Check that {nameof(pickupConfirmation)}.KK_IsEmptyContainer was initially set to false", false, pickupConfirmation.KK_IsEmptyContainer);

			var deliveryToCNEConfirmation = deliveryToCNEInstruction3.Confirmations.Single();
			AssertEquals($"Precondition: Check that {nameof(deliveryToCNEConfirmation)}.KK_IsEmptyContainer was initially set to false", false, deliveryToCNEConfirmation.KK_IsEmptyContainer);

			var containerReturnDeliveryConfirmation = containerReturnDeliveryInstruction4.Confirmations.Single();
			AssertEquals($"Precondition: Check that {nameof(containerReturnDeliveryConfirmation)}.KK_IsEmptyContainer was initially set to true", true, containerReturnDeliveryConfirmation.KK_IsEmptyContainer);
			containerReturnDeliveryConfirmation.KK_IsEmptyContainer = false;

			booking.SetIsEmptyContainerOnAllRelatedConfirmations();

			CombineAssertions("Check results of SetIsEmptyContainerOnAllRelatedConfirmations()", () =>
			{
				AssertEquals($"Check that {nameof(pickupConfirmation)}.KK_IsEmptyContainer is still set to false", false, pickupConfirmation.KK_IsEmptyContainer);
				AssertEquals($"Check that {nameof(deliveryToCNEConfirmation)}.KK_IsEmptyContainer is still set to false", false, deliveryToCNEConfirmation.KK_IsEmptyContainer);
				AssertEquals($"Check that {nameof(containerReturnDeliveryConfirmation)}.KK_IsEmptyContainer was now set to true again", true, containerReturnDeliveryConfirmation.KK_IsEmptyContainer);
			});
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestShouldAuditLogIfOnlyChildrenHaveChanges()
		{
			TestShouldAuditLogIfOnlyChildrenHaveChanges(DtbChildEditableServiceState.Transport);
			TestShouldAuditLogIfOnlyChildrenHaveChanges(DtbChildEditableServiceState.Consolidation);

			// as done on ZForm SetDataBinding(), should short circuit the DtbChildEditableService state check
			TestShouldAuditLogIfOnlyChildrenHaveChanges(DtbChildEditableServiceState.None, isTopLevel: true);
		}

		void TestShouldAuditLogIfOnlyChildrenHaveChanges(DtbChildEditableServiceState state, bool isTopLevel = false)
		{
			TestDateAttribute.Date = new DateTime(2025, 3, 6, 0, 0, 0);
			DtbChildEditableService.SetState(Factory, state);

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			booking.IsTopLevel = isTopLevel;
			Factory.Save();

			AssertEquals("Precondition - should not have Edit logs.", false,
				booking.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.EditedARecord.Code));

			AssertEquals("Precondition - last edit time should be start date", new ZDateTime(2025, 3, 6, 0, 0, 0), booking.KM_SystemLastEditTimeUtc);

			TestDateAttribute.AddMinutes(15);

			// make an edit to the instruction
			instruction.KN_DropMode = "111";
			Factory.Save();

			// should create a single edit event
			booking.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.EditedARecord.Code);

			AssertEquals("Last edit time should be updated", new ZDateTime(2025, 3, 6, 0, 15, 0), booking.KM_SystemLastEditTimeUtc);
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestShouldntAuditLogIfOnlyChildrenHaveChangesAndNotTopLevelOrHasStateSet()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.None);

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			booking.IsTopLevel = false;
			Factory.Save();

			AssertEquals("Precondition - should not have Edit logs.", false,
				booking.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.EditedARecord.Code));

			AssertEquals("Precondition - last edit time should be start date", new ZDateTime(2025, 3, 6, 0, 0, 0), booking.KM_SystemLastEditTimeUtc);

			// make an edit to the instruction
			instruction.KN_DropMode = "111";
			Factory.Save();

			// should not create a single edit event
			AssertEquals("Should not have Edit logs.", false,
				booking.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.EditedARecord.Code));

			AssertEquals("Last edit time should not be updated", new ZDateTime(2025, 3, 6, 0, 0, 0), booking.KM_SystemLastEditTimeUtc);
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestShouldntAuditLogIfOnlyLogsHaveChangesAndNotInBookingOrConsolidationScreen()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			AssertEquals("Precondition - should not have Edit logs.", false,
				booking.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.EditedARecord.Code));

			AssertEquals("Precondition - last edit time should be start date", new ZDateTime(2025, 3, 6, 0, 0, 0), booking.KM_SystemLastEditTimeUtc);

			AssertEquals("Precondition", false, booking.HasChanges);
			booking.Logs.AddNew(Events.DataExport);
			Factory.Save();

			AssertEquals("Should not have Edit logs.", false,
				booking.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.EditedARecord.Code));

			AssertEquals("Last edit time should not be updated", new ZDateTime(2025, 3, 6, 0, 0, 0), booking.KM_SystemLastEditTimeUtc);
		}

		public void TestFetchStrategy()
		{
			var transportCo1 = Helper.CreateOrganisation("TC1");
			var transportCo2 = Helper.CreateOrganisation("TC2");
			var transportCo3 = Helper.CreateOrganisation("TC3");
			var booking1 = Helper.CreateBooking(transportCo1);
			var booking2 = Helper.CreateBooking(transportCo2);
			var booking3 = Helper.CreateBooking(transportCo3);
			var cnr1 = Helper.CreateOrganisation("CNR1");
			var cnr2 = Helper.CreateOrganisation("CNR2");
			Helper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr1.MainAddress);
			Helper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr2.MainAddress);
			Helper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr1.MainAddress);
			Helper.CreateInstruction(booking2, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr2.MainAddress);
			Helper.CreateInstruction(booking3, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr1.MainAddress);
			Helper.CreateInstruction(booking3, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, cnr2.MainAddress);

			foreach (var instruction in new[] { booking1, booking2, booking3 }.SelectMany(b => b.Instructions))
			{
				Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
				var package = instruction.Booking.ConsolidationSingleJob.PackageJob.Packages.AddNew();
				var divot = Helper.CreatePackageDivot(instruction, package);
				Helper.CreateConfirmation(divot, ConfirmationTypes.Codes.PickUp);
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var otherBooking1 = otherFactory.Load<DtbBooking>(booking1.PK);
			var otherBooking2 = otherFactory.Load<DtbBooking>(booking2.PK);
			var otherBooking3 = otherFactory.Load<DtbBooking>(booking3.PK);

			AssertEquals(typeof(DtbBookingFetchStrategy), otherBooking1.FetchStrategy.GetType());
			AssertEquals(typeof(DtbBookingFetchStrategy), otherBooking2.FetchStrategy.GetType());
			AssertEquals(typeof(DtbBookingFetchStrategy), otherBooking3.FetchStrategy.GetType());

			otherBooking1.FetchStrategy.FetchForView(TableColumnsAddedInFetchForView);
			otherBooking2.FetchStrategy.FetchForView(TableColumnsAddedInFetchForView);
			otherBooking3.FetchStrategy.FetchForView(TableColumnsAddedInFetchForView);

			var databaseHitCountBeforeTest = otherFactory.DatabaseLoadCount;
			var allBookings = new[] { otherBooking1, otherBooking2, otherBooking3 };
			var pokeForTransportCOs = allBookings.Select(b => b.Address).Count();
			AssertEquals("Should be 3 hits.", databaseHitCountBeforeTest + 3, otherFactory.DatabaseLoadCount);

			var allInstructions = allBookings.SelectMany(b => b.Instructions);
			var pokeForInstructions = allInstructions.Count();
			AssertEquals("Should be one hit for Instructions.", databaseHitCountBeforeTest + 3, otherFactory.DatabaseLoadCount);

			var pokeForInstructionAddresses = allInstructions.Select(i => i.Address).Count();
			AssertEquals("Should be one hit for Instruction Addresses.", databaseHitCountBeforeTest + 3, otherFactory.DatabaseLoadCount);

			var pokeForDivots = allInstructions.SelectMany(i => i.PackageDivots).Count();
			AssertEquals("Should be one hit for Package Divots, one hit for Package, one hit for PkgPackageJob", databaseHitCountBeforeTest + 6, otherFactory.DatabaseLoadCount);

			// #warning uncomment this once a fetch hint is added for the divot
			//var pokeForConfirmation = allInstructions.SelectMany(i => i.Confirmations).Concat(allInstructions.SelectMany(i => i.PackageDivots.SelectMany(d => d.Confirmations))).Count();
			//AssertEquals("Should be one hit for Confirmations.", databaseHitCountBeforeTest + 5, otherFactory.DatabaseLoadCount);
		}

		TableColumn[] TableColumnsAddedInFetchForView
		{
			get
			{
				return new TableColumn[]
				{
					new TableColumn(WhsDocketContainerSchema.Constants.TableName, "Address+E2_OA_Address")
				};
			}
		}

		public void TestHumanReadableName()
		{
			var booking1 = Helper.CreateBooking();
			booking1.KM_JobID = "TB123";
			AssertEquals("Transport Booking TB123", booking1.HumanReadableName);
		}

		public void TestHumanReadableName_BookingIsMaster()
		{
			var consolidationBooking = Helper.CreateConsolidation();
			var booking = consolidationBooking.Bookings.AddNew();
			booking.KM_JobID = "TB123";
			booking.KM_IsMaster = true;
			AssertEquals("Should indicate the booking is a master booking", "Master Transport Booking TB123", booking.HumanReadableName);
		}

		public void TestHumanReadableName_BookingIsPickupMaster()
		{
			var consolidationBooking = Helper.CreateConsolidation();
			consolidationBooking.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidationBooking.Bookings.AddNew();
			booking.KM_JobID = "TB123";
			booking.KM_IsMaster = true;
			AssertEquals("Should indicate the booking is a pickup master booking", "Pickup Master Transport Booking TB123", booking.HumanReadableName);
		}

		public void TestHumanReadableName_BookingIsDeliveryMaster()
		{
			var consolidationBooking = Helper.CreateConsolidation();
			consolidationBooking.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking = consolidationBooking.Bookings.AddNew();
			booking.KM_JobID = "TB123";
			booking.KM_IsMaster = true;
			AssertEquals("Should indicate the booking is a delivery master booking", "Delivery Master Transport Booking TB123", booking.HumanReadableName);
		}

		public void TestConsolidationNotesAreShownInBooking()
		{
			var booking = Helper.CreateBooking();
			var note = Helper.CreateNote(booking.ConsolidationSingleJob, "Test NoteOnConsolidation.");
			AssertEquals(note, booking.Notes.GetAllRelatedNotesVisibleToCurrentCompany().Single());
		}

		[ExpectNoExceptions()]
		public void TestBillingPartyShouldNotGetValidatedUponSaveWhenBookingIsDeleted()
		{
			// setup Shipment + Shipment Job Header + Booking + Save
			var shipment = Helper.CreateForwardingShipment("123", "", "", "");
			var shipmentJob = Helper.LoadOrCreateJobHeader((IJobHeaderParent)shipment);
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			Factory.Save();

			// open the multi booking form + add a booking
			var bookingFactory = new BusinessObjectFactory();
			var multiBooking = bookingFactory.Load<DtbBookingConsolidation>(consolidation.PK);
			var newBooking = multiBooking.Bookings.AddNew();

			// change billing party to a valid org + delete the new booking row + save
			newBooking.BillingPartyOrLocalClientPK_ZAddress.OrgPK = bookingFactory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			newBooking.Delete();
			bookingFactory.Save();
		}

		public void TestFirstConsignor()
		{
			var whs = Helper.CreateOrganisation("whs");
			var consignee = Helper.CreateOrganisation("CNE");
			var consignor1 = Helper.CreateOrganisation("CR1");
			var consignor2 = Helper.CreateOrganisation("CR2");
			var noInstructions = Helper.CreateBooking();
			var bookingWithNoConsignor = Helper.CreateBooking();
			Helper.CreateInstruction(bookingWithNoConsignor, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.WHS, whs.MainAddress);

			var bookingWithConsignee = Helper.CreateBooking();
			Helper.CreateInstruction(bookingWithConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee.MainAddress);

			var bookingWithConsignor = Helper.CreateBooking();
			var instructionWithConsignor1 = Helper.CreateInstruction(bookingWithConsignor, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor1.MainAddress);

			var bookingWithTwoConsignors = Helper.CreateBooking();
			var instructionWithTwoConsignor1 = Helper.CreateInstruction(bookingWithTwoConsignors, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor2.MainAddress);
			var instructionWithTwoConsignor2 = Helper.CreateInstruction(bookingWithTwoConsignors, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor1.MainAddress);
			AssertEquals("Precondition", 1, instructionWithTwoConsignor1.KN_Sequence);
			AssertEquals("Precondition", 2, instructionWithTwoConsignor2.KN_Sequence);

			var bookingWithConsignorAndConsignee = Helper.CreateBooking();
			var instruction1 = Helper.CreateInstruction(bookingWithConsignorAndConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee.MainAddress);
			var instruction2 = Helper.CreateInstruction(bookingWithConsignorAndConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor1.MainAddress);
			AssertEquals("Precondition", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition", 2, instruction2.KN_Sequence);

			AssertEquals("First consignor instruction must be null when booking doesn't have any instructions.", null, noInstructions.FirstConsignor);
			AssertEquals("First consignor instruction must be null when booking doesn't have any consignor instructions.", null, bookingWithNoConsignor.FirstConsignor);
			AssertEquals("No instructions must be returned when booking has only consignee instructions.", null, bookingWithConsignee.FirstConsignor);
			AssertEquals("Only consignor instruction must be returned when booking has only one consignor instructions.", instructionWithConsignor1, bookingWithConsignor.FirstConsignor);
			AssertEquals("First consignor instruction must be returned when booking has multiple consignor instructions.", instructionWithTwoConsignor1, bookingWithTwoConsignors.FirstConsignor);
			AssertEquals("Consignor instruction must be returned when booking has consignor and consignee instructions.", instruction2, bookingWithConsignorAndConsignee.FirstConsignor);
		}

		public void TestFirstConsignee()
		{
			var whs = Helper.CreateOrganisation("whs");
			var consignor = Helper.CreateOrganisation("CNR");
			var consignee1 = Helper.CreateOrganisation("CE1");
			var consignee2 = Helper.CreateOrganisation("CE2");
			var noInstructions = Helper.CreateBooking();
			var bookingWithNoConsignee = Helper.CreateBooking();
			Helper.CreateInstruction(bookingWithNoConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.WHS, whs.MainAddress);

			var bookingWithConsignor = Helper.CreateBooking();
			Helper.CreateInstruction(bookingWithConsignor, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor.MainAddress);

			var bookingWithConsignee = Helper.CreateBooking();
			var instructionWithConsignee1 = Helper.CreateInstruction(bookingWithConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee1.MainAddress);

			var bookingWithTwoConsignees = Helper.CreateBooking();
			var instructionWithTwoConsignee1 = Helper.CreateInstruction(bookingWithTwoConsignees, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee2.MainAddress);
			var instructionWithTwoConsignee2 = Helper.CreateInstruction(bookingWithTwoConsignees, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee1.MainAddress);
			AssertEquals("Precondition", 1, instructionWithTwoConsignee1.KN_Sequence);
			AssertEquals("Precondition", 2, instructionWithTwoConsignee2.KN_Sequence);

			var bookingWithConsignorAndConsignee = Helper.CreateBooking();
			var instruction1 = Helper.CreateInstruction(bookingWithConsignorAndConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, consignor.MainAddress);
			var instruction2 = Helper.CreateInstruction(bookingWithConsignorAndConsignee, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, consignee1.MainAddress);
			AssertEquals("Precondition", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition", 2, instruction2.KN_Sequence);

			AssertEquals("First consignee instruction must be null when booking doesn't have any instructions.", null, noInstructions.FirstConsignee);
			AssertEquals("First consignee instruction must be null when booking doesn't have any consignee instructions.", null, bookingWithNoConsignee.FirstConsignee);
			AssertEquals("First consignee instruction must be null when booking has only consignor instructions.", null, bookingWithConsignor.FirstConsignee);
			AssertEquals("Only consignee instruction must be returned when booking has only one consignee instruction.", instructionWithConsignee1, bookingWithConsignee.FirstConsignee);
			AssertEquals("First consignee instruction must be returned when booking has only multiple consignee instructions.", instructionWithTwoConsignee1, bookingWithTwoConsignees.FirstConsignee);
			AssertEquals("Consignee instruction must be returned when booking has only consignor and consignee instructions.", instruction2, bookingWithConsignorAndConsignee.FirstConsignee);
		}

		public void TestTotalPickups()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Destination;
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction3 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction4 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var multiInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			AssertEquals("Precondition: IsDeliveryDirection is true", true, booking.IsDeliveryDirection);
			AssertEquals("TotalPickups should include multis, even if IsDeliveryDirection is true", 6, booking.TotalPickups);
		}

		public void TestTotalDeliveries()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction3 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction4 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var multiInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			AssertEquals("Precondition: IsPickupDirection is true", true, booking.IsPickupDirection);
			AssertEquals("TotalDeliveries should include multis, even if IsPickupDirection is true", 3, booking.TotalDeliveries);
		}

		public void TestAdditionalReferenceNumbersSingleLine()
		{
			var booking = Helper.CreateBooking();

			var cusNumber1 = booking.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "XXXABILL1";
			AssertEquals("AMS: XXXABILL1", booking.AdditionalReferenceNumbersSingleLine);

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("AMS: XXXABILL1/AU", booking.AdditionalReferenceNumbersSingleLine);

			var cusNumber2 = booking.AdditionalReferenceNumbers.AddNew();
			cusNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusNumber2.CE_EntryNum = "CV00001";
			cusNumber2.CE_RN_NKCountryCode = "CN";

			AssertEquals("AMS: XXXABILL1/AU, BKG: CV00001/CN", booking.AdditionalReferenceNumbersSingleLine);
		}

		public void TestFirstRoutingLeg_Shipment()
		{
			var now = ZDateTime.Now;
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol);

			var consolidationForShipment = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var bookingWithShipment = Helper.CreateBooking(consolidationForShipment);
			var consolTransports = ((BusinessObjectCollection)consol["Transports"]);
			consolTransports.Load();
			AssertEquals("Precondition", 1, consolTransports.Count);
			var expectedLegForConsol = ((ITransport)consolTransports.ToArray()[0]);
			expectedLegForConsol.JW_ATA = now.AddDays(-6);
			((BusinessObject)expectedLegForConsol)[JobConsolTransportSchema.JW_LegOrder] = (ZByte)0;
			AssertEquals("Precondition", (ZByte)0, expectedLegForConsol.JW_LegOrder);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-5), Constants.TransportParentTypes.Shipment);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-4), Constants.TransportParentTypes.Shipment);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-3), Constants.TransportParentTypes.Shipment);
			((BusinessObjectCollection)consol["Transports"]).Load();

			AssertEquals(expectedLegForConsol, bookingWithShipment.FirstRoutingLeg);
		}

		public void TestFirstRoutingLeg_StandaloneBooking()
		{
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var bookingWithoutShipment = Helper.CreateBooking(consolidation);
			var expectedLegForConsolidation = CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 1, now.AddDays(-5));
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 1, now.AddDays(-4));
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 2, now.AddDays(-3));
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 3, now.AddDays(-2));
			Factory.Save();

			AssertEquals(expectedLegForConsolidation, bookingWithoutShipment.FirstRoutingLeg);
		}

		ITransport CreateTransport(Type parentType, ZGuid parentPK, ZByte legOrder, ZDateTime arrivalTime, string transportParentType = Constants.TransportParentTypes.TransportBooking)
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = parentType;
			transport.JW_ParentGUID = parentPK;
			((BusinessObject)transport)[JobConsolTransportSchema.JW_LegOrder] = legOrder;
			((BusinessObject)transport)[JobConsolTransportSchema.JW_ATA] = arrivalTime;
			((BusinessObject)transport)[JobConsolTransportSchema.JW_ParentType] = transportParentType;
			return transport;
		}

		public void TestLastRoutingLeg_Shipment()
		{
			var now = ZDateTime.Now;
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment["Consols"]).Add(consol);
			var consolidationForShipment = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var bookingWithShipment = Helper.CreateBooking(consolidationForShipment);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-6), Constants.TransportParentTypes.Shipment);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-5), Constants.TransportParentTypes.Shipment);
			CreateTransport(shipment.GetType(), shipment.PK, 0, now.AddDays(-4), Constants.TransportParentTypes.Shipment);
			var consolTransports = ((BusinessObjectCollection)consol["Transports"]);
			consolTransports.Load();
			AssertEquals("Precondition", 1, consolTransports.Count);
			var expectedLegForConsol = ((ITransport)consolTransports.ToArray()[0]);
			expectedLegForConsol.JW_ATA = now.AddDays(-1);
			((BusinessObject)expectedLegForConsol)[JobConsolTransportSchema.JW_LegOrder] = (ZByte)0;
			AssertEquals("Precondition", (ZByte)0, expectedLegForConsol.JW_LegOrder);

			AssertEquals(expectedLegForConsol, bookingWithShipment.LastRoutingLeg);
		}

		public void TestLastRoutingLeg_StandaloneBooking()
		{
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var bookingWithoutShipment = Helper.CreateBooking(consolidation);
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 1, now.AddDays(-6));
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 2, now.AddDays(-5));
			CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 3, now.AddDays(-4));
			var expectedLegForConsolidation = CreateTransport(typeof(DtbBookingConsolidation), consolidation.PK, 3, now.AddDays(-3));
			Factory.Save();

			AssertEquals(expectedLegForConsolidation, bookingWithoutShipment.LastRoutingLeg);
		}

		public void TestSetContainerLinksAndNumbers()
		{
			var booking = Factory.New<DtbBooking>();
			var testContainerLinksAndRefs = new List<UniversalDataBuss.DataObjects.Universal.Container>()
			{
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = null, ContainerNumber = null },
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = 1, ContainerNumber = "CN1" },
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = 2, ContainerNumber = null },
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = 3, ContainerNumber = ZString.Empty },
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = null, ContainerNumber = "CN4" },
				new UniversalDataBuss.DataObjects.Universal.Container() { Link = 0, ContainerNumber = "CN0" },
			};
			var expectedLinks = new List<ZInt>() { 1, 2, 3, 0 };
			var expectedContainerNumbers = new List<ZString>() { "CN1", "CN4", "CN0" };

			booking.SetContainerLinksAndNumbers(testContainerLinksAndRefs);

			AssertEquals("booking's Links should be equivalent to expected links", true, expectedLinks.SequenceEqual(booking.ContainerLinks.ToList()));
			AssertEquals("booking's Links should be equivalent to expected links", true, expectedContainerNumbers.SequenceEqual(booking.ContainerNumbers.ToList()));
		}

		public void TestSetContainerLinksAndNumbersToNullReturnsEmptyEnumerablesAndDoesNotThrow()
		{
			var booking = Factory.New<DtbBooking>();
			booking.SetContainerLinksAndNumbers(null);

			AssertEquals("booking's Links should be equivalent to expected links", true, Enumerable.Empty<ZInt>().SequenceEqual(booking.ContainerLinks.ToList()));
			AssertEquals("booking's Links should be equivalent to expected links", true, Enumerable.Empty<ZString>().SequenceEqual(booking.ContainerNumbers.ToList()));
		}

		// interface members

		public void TestICreditControlledDocumentDelivery_GetDocumentLogin_WithParent()
		{
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();

			int getDocumentHitCount = 0;
			void GetDocumentLogin(object sender, SecurityLoginEventArgs e) => getDocumentHitCount++;

			shipmentCreditControlledDocumentDelivery.GetDocumentLogin += GetDocumentLogin;
			bookingCreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(null);
			AssertEquals("GetDocumentLogin should be called", 1, getDocumentHitCount);

			shipmentCreditControlledDocumentDelivery.GetDocumentLogin -= GetDocumentLogin;
			bookingCreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(null);
			AssertEquals("GetDocumentLogin should not be called", 1, getDocumentHitCount);

			bookingCreditControlledDocumentDelivery.GetDocumentLogin += GetDocumentLogin;
			shipmentCreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(null);
			AssertEquals("GetDocumentLogin should be called", 2, getDocumentHitCount);

			bookingCreditControlledDocumentDelivery.GetDocumentLogin -= GetDocumentLogin;
			shipmentCreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(null);
			AssertEquals("GetDocumentLogin should not be called", 2, getDocumentHitCount);
		}

		public void TestICreditControlledDocumentDelivery_GetDocumentLogin_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			int getDocumentHitCount = 0;
			void GetDocumentLogin(object sender, SecurityLoginEventArgs e) => getDocumentHitCount++;

			bookingCreditControlledDocumentDelivery.GetDocumentLogin += GetDocumentLogin;
			bookingCreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(null);
			AssertEquals("GetDocumentLogin should not be called", 0, getDocumentHitCount);
		}

		public void TestICreditControlledDocumentDelivery_IsDPSFreightMovementRestricted_WithParent()
		{
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();
			new string[]
			{
				DPSFreightMovementRestrictionsOptions.Codes.All,
				DPSFreightMovementRestrictionsOptions.Codes.Exp,
				DPSFreightMovementRestrictionsOptions.Codes.No
			}.ForEach(option =>
			{
				using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option))
				{
					AssertEquals(shipmentCreditControlledDocumentDelivery.IsDPSFreightMovementRestricted, bookingCreditControlledDocumentDelivery.IsDPSFreightMovementRestricted);
				}
			});
		}

		public void TestICreditControlledDocumentDelivery_IsDPSFreightMovementRestricted_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			Assert(!bookingCreditControlledDocumentDelivery.IsDPSFreightMovementRestricted);
		}

		public void TestICreditControlledDocumentDelivery_GetScreeningParties_WithParent()
		{
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();
			var bookingScreeningParties = bookingCreditControlledDocumentDelivery.GetScreeningParties();
			var shipmentScreeningParties = shipmentCreditControlledDocumentDelivery.GetScreeningParties();
			AssertNotEquals("Precondition: Parent has Screening Parties.", 0, shipmentScreeningParties.Length);
			AssertArray(shipmentScreeningParties, bookingScreeningParties, (expected, actural) => expected.Parent.PK == actural.Parent.PK);
		}

		public void TestICreditControlledDocumentDelivery_GetScreeningParties_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			AssertEquals(0, bookingCreditControlledDocumentDelivery.GetScreeningParties().Length);
		}

		public void TestICreditControlledDocumentDelivery_OrganisationsForCreditChecks_WithParent()
		{
			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();
			var bookingOrganisationsForCreditChecks = bookingCreditControlledDocumentDelivery.OrganisationsForCreditChecks;
			var shipmentOrganisationsForCreditChecks = shipmentCreditControlledDocumentDelivery.OrganisationsForCreditChecks;
			AssertNotEquals("Precondition: Parent has Organisations For Credit Checks.", 0, shipmentOrganisationsForCreditChecks.Length);
			AssertArray(shipmentOrganisationsForCreditChecks, bookingOrganisationsForCreditChecks, (expected, actural) => expected.PK == actural.PK);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestICreditControlledDocumentDelivery_OrganisationsForCreditChecks_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			AssertEquals(0, bookingCreditControlledDocumentDelivery.OrganisationsForCreditChecks.Length);
		}

		public void TestICreditControlledDocumentDelivery_DescriptionOfOrganisationBeingCheckedForCredit_WithParent()
		{
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();
			var bookingDescriptionOfOrganisationBeingCheckedForCredit = bookingCreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit;
			var shipmentDescriptionOfOrganisationBeingCheckedForCredit = shipmentCreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit;
			AssertNotEquals("Precondition: Parent has Description of Organisation Being Checked For Credit.", 0, shipmentDescriptionOfOrganisationBeingCheckedForCredit.Length);
			AssertEquals(shipmentDescriptionOfOrganisationBeingCheckedForCredit, bookingDescriptionOfOrganisationBeingCheckedForCredit);
		}

		public void TestICreditControlledDocumentDelivery_DescriptionOfOrganisationBeingCheckedForCredit_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			AssertEquals(string.Empty, bookingCreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		public void TestICreditControlledDocumentDelivery_JobNumber_WithParent()
		{
			var (bookingCreditControlledDocumentDelivery, shipmentCreditControlledDocumentDelivery) = CreateICreditControlledDocumentDeliveryWithParent();
			var bookingJobNumber = bookingCreditControlledDocumentDelivery.JobNumber;
			var shipmentJobNumber = shipmentCreditControlledDocumentDelivery.JobNumber;
			AssertNotEquals("Precondition: Parent has Job Number.", 0, shipmentJobNumber.Length);
			AssertArrayEqualsByElements(shipmentJobNumber, bookingJobNumber);
		}

		public void TestICreditControlledDocumentDelivery_JobNumber_WithoutParent()
		{
			var bookingCreditControlledDocumentDelivery = CreateICreditControlledDocumentDeliveryWithoutParent();
			AssertEquals(0, bookingCreditControlledDocumentDelivery.JobNumber.Length);
		}

		(ICreditControlledDocumentDelivery BookingCreditControl, ICreditControlledDocumentDelivery ShipmentCreditControl) CreateICreditControlledDocumentDeliveryWithParent()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			return (booking, (ICreditControlledDocumentDelivery)shipment);
		}

		ICreditControlledDocumentDelivery CreateICreditControlledDocumentDeliveryWithoutParent()
		{
			return Helper.CreateBooking();
		}

		void AssertArray<T>(T[] expected, T[] actural, Func<T, T, bool> comparer)
		{
			AssertEquals(expected.Length, actural.Length);
			var index = 0;
			expected.ForEach(expectedItem => Assert(comparer(expectedItem, actural[index++])));
		}

		public void TestJobHeaderPK()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(ZGuid.Empty, booking.JobHeaderPK);

			var job = Helper.LoadOrCreateJobHeader(booking);
			AssertNotNull(booking.Job);
			AssertEquals(booking.Job.PK, booking.JobHeaderPK);
		}

		public void TestErrorReportedWhenJobCreatedOnBookingWhenAlreadyAttachedToParent()
		{
			var parent = Factory.NewWithValidTestData<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation();
			Helper.AttachConsolidationToParent(consol, parent);
			var booking = Helper.CreateBooking(consol);

			AssertEquals("Precon: No error reported", "", ErrorReporter.LastMessageReported);

			var bookingJob = Factory.NewJobForTesting<JobHeader>();
			bookingJob.JH_ParentTableCode = booking.TablePrefix;
			bookingJob.JH_ParentID = booking.PK;
			((IJobHeaderParent)booking).OnJobCreated(bookingJob);

			AssertEquals("An error should be reported", "JobHeader has just been created on this Booking record while it is attached to a parent. When attached to a parent the booking should use the parent's JobHeader.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNoErrorReportedWhenJobCreatedOnBookingWhenNotAttachedToParent()
		{
			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);

			var bookingJob = Factory.NewJobForTesting<JobHeader>();
			bookingJob.JH_ParentTableCode = booking.TablePrefix;
			bookingJob.JH_ParentID = booking.PK;
			((IJobHeaderParent)booking).OnJobCreated(bookingJob);

			AssertEquals("No error reported", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPkgPackageJob()
		{
			var booking = Factory.New<DtbBooking>();
			AssertNull(((IDtbBooking)booking).PackageJob);

			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);
			consolidation.Bookings.Add(booking);
			AssertEquals(packageJob, ((IDtbBooking)booking).PackageJob);
		}

		public void TestInvoicingPlugIn_WhenParentIsHidden()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			var plugin = booking.InvoicingPlugIn;
			AssertType<DtbAgentBooking>(plugin);
		}

		public void TestInvoicingSupporter_WhenParentIsHidden()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			IJobInvoicingPlugIn plugin = booking;
			AssertType<DtbBookingParentInvoicingSupporter>(plugin.InvoicingSupporter);
		}

		public void TestOnJobCreating_LinkCartageJobToJob_ShipmentParent()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);

			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.JJ_ParentID] = booking.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = booking.TablePrefix;

			Factory.Save();

			Helper.LoadOrCreateJobHeader((IJobHeaderParent)cartage);
			AssertNotNull(((ICommonCartage)cartage).Job);
			Assert(((ICommonCartage)cartage).Job.JH_JH_ParentJob.IsEmpty);

			AssertNull(booking.Job);
			var bookingJob = Helper.LoadOrCreateJobHeader(booking);
			AssertNotNull(booking.Job);
			AssertEquals(bookingJob.PK, ((ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestOnJobCreating_LinkCartageJobToJob_StandaloneBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.JJ_ParentID] = booking.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = booking.TablePrefix;

			Factory.Save();

			Helper.LoadOrCreateJobHeader((IJobHeaderParent)cartage);
			AssertNotNull(((ICommonCartage)cartage).Job);
			Assert(((ICommonCartage)cartage).Job.JH_JH_ParentJob.IsEmpty);

			AssertNull(booking.Job);
			var bookingJob = Helper.LoadOrCreateJobHeader(booking);
			AssertNotNull(booking.Job);
			AssertEquals(bookingJob.PK, ((ICommonCartage)cartage).Job.JH_JH_ParentJob);
		}

		public void TestIJobNumberCore()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			parent.Z0_Description = "D00000123";
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "FOF12341298";
			AssertEquals("Should be parent Job Number", "D00000123", ((IJobNumber)booking).JobNumber);
		}

		public void TestIRelatedJob()
		{
			var booking = Helper.CreateBooking();
			var relatedJob = (IRelatedJob)booking;
			AssertEquals("PK", booking.PK, relatedJob.BusinessObjectPK);
			AssertEquals("ControllerID", ControllerIDs.DtbBooking, relatedJob.ControllerID);
			AssertEquals("JobDescription", "Transport Booking", relatedJob.JobDescription);
			AssertEquals("JobNumber", "", relatedJob.JobNumber);
			AssertEquals("JobStatus", TransportStatuses.Descriptions.Available, relatedJob.JobStatus);

			Factory.Save();
			booking.KM_Description = "Hello";
			AssertEquals("PK", booking.PK, relatedJob.BusinessObjectPK);
			AssertEquals("ControllerID", ControllerIDs.DtbBooking, relatedJob.ControllerID);
			AssertEquals("JobDescription", "Transport Booking - Hello", relatedJob.JobDescription);
			AssertEquals("JobNumber", "TB00000001", relatedJob.JobNumber);
			AssertEquals("JobStatus", TransportStatuses.Descriptions.Available, relatedJob.JobStatus);
		}

		public void TestLandTransportConsignment()
		{
			var booking = Helper.CreateBooking();
			Factory.Save();
			AssertNull(booking.LandTransportConsignment);

			var consignment = Factory.New<IDtbConsignment>();
			consignment.LTC_Direction = "ORG";
			consignment.LTC_Status = "BKD";
			consignment.LTC_KM_Booking = booking.PK;
			Factory.Save();
			AssertEquals(consignment, booking.LandTransportConsignment);
		}

		public void TestJobDirectionImport()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var consolBooking = Helper.CreateConsolidation();
			var booking = consolBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Does not return the correct JobDirection for an import", MasterFiles.Business.Directions.Import, booking.JobDirection);
		}

		public void TestJobDirectionExport()
		{
			var template = Helper.CreateTransportBookingTemplate("EXP", "FCL Export", Constants.CartageDirection.Export);
			var consolBooking = Helper.CreateConsolidation();
			var booking = consolBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Does not return the correct JobDirection for an export", MasterFiles.Business.Directions.Export, booking.JobDirection);
		}

		public void TestJobDirectionUnknown()
		{
			var template = Helper.CreateTransportBookingTemplate("DLCX", "Warehouse release", Constants.CartageDirection.Destination);
			var consolBooking = Helper.CreateConsolidation();
			var booking = consolBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Does not return the correct JobDirection for something with an unknown direction", MasterFiles.Business.Directions.Unknown, booking.JobDirection);
		}

		public void TestJobDirection_DoesNotUseCartageDirection()
		{
			var template = Helper.CreateTransportBookingTemplate("ILDX", "Import LCL/LSE/LTL Delivery", Constants.CartageDirection.Destination);
			var consolBooking = Helper.CreateConsolidation();
			var booking = consolBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Uses cartage direction attribute when code or description more accurately describes whether something is an import, export, or neither", MasterFiles.Business.Directions.Import, booking.JobDirection);
		}

		public void TestAccessingNoteContextsForRelatedNotesOnDtbTransportWhileCollectionListChangedEventsDelayed()
		{
			var booking = GetSaveableTransportJob();
			booking.PackageJob.FillWithValidTestData();
			var instructionPic1 = booking.Instructions.AddNew();
			instructionPic1.KN_InstructionType = "PIC";
			var packageCnt1 = Helper.CreatePackage("PKG1", 1, "CNT");
			packageCnt1.Container.FillWithValidTestData();
			var divot1 = Helper.CreatePackageDivot(instructionPic1, packageCnt1, 1);
			packageCnt1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			var packagePlt2 = Helper.CreatePackage("PKG2", 1, "PLT");
			var divot2 = Helper.CreatePackageDivot(instructionPic1, packagePlt2, 1);
			packagePlt2.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			Factory.Save();

			var assignToEnsurePackagesPackageViewIsInstantiated = booking.Packages_PackageView;
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				divot2.Delete();
				packagePlt2.Delete();
				var noteContexts = ((IStmNoteParent)booking).NoteContextsForRelatedNotes;
			}
			AssertEquals(
				"Should not try to access deleted record package data - when Factory.AreCollectionListChangedEventsDelayed (eg in some circumstances when saving) and accessing NoteContextsForRelatedNotes (which looks at IsContainerised and IsLoose, which in turn accesses PkgPackage.KP_F3_NKPackType) - should ensure that PackageCollection_PackageView is up to date and deleted/unassigned packages have been removed",
				false,
				ErrorReporter.LastMessageReported.Contains(@"Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Should supply correct type based on the table prefix", typeof(DtbBooking), businessObjectType);
			AssertNotEquals("Should not supply deprecated abstract type", typeof(DtbTransport), businessObjectType);
		}

		public void TestGetTypeFromObjectFactory()
		{
			var suppliedType = typeof(IDtbTransport);
			var expectedType = typeof(DtbBooking);
			var actualType = ObjectFactory.GetType(suppliedType);
			AssertEquals("Should be able to get the correct type from the interface", expectedType, actualType);
		}

		public void TestCreateFromInterface()
		{
			var type = typeof(IDtbTransport);
			var businessObject = Factory.New(ObjectFactory.GetType(type));
			var expectedType = typeof(DtbBooking);
			AssertEquals("Should create correct business object when using Factory.New", expectedType, businessObject.GetType());
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenBookingHasInstruction_ShouldFindInstruction()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			Factory.Save();

			AssertEquals("Expected Instruction in business objects.", instruction.PK, booking.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenBookingHasConfirmation_ShouldFindConfirmation()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var confirmation = instruction.Confirmations.AddNew();
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Expected instruction and confirmation records in business objects.", new[] { instruction.PK.ToString(), confirmation.PK.ToString() }, booking.BusinessObjectsWithRelatedEvents.Select(o => o.PK.ToString()));
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenBookingHasJobHeader_ShouldFindJobHeader()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			var jobHeader = Helper.LoadOrCreateJobHeader(booking);
			Factory.Save();

			AssertEquals("Expected JobHeader in business objects.", jobHeader.PK, booking.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenBookingHasShipment_ShouldFindShipment()
		{
			var shipment = Helper.CreateForwardingShipment();
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = consolidation.Bookings.AddNew();

			Factory.Save();

			AssertEquals("Expected Shipment in business objects.", shipment.PK, booking.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenBookingHasConsignment_ShouldFindConsignment()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			var consignment = Helper.CreateConsignment(booking.PK);
			Factory.Save();

			AssertEquals("Expected Consignment in business objects.", consignment.PK, booking.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenMasterBooking_ShouldFindSubBookings()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			Factory.Save();
			masterBooking.SubBookings.Add(subBooking1);
			masterBooking.SubBookings.Add(subBooking2);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Expected sub booking records in business objects with related events for master booking.", new[] { subBooking1.PK.ToString(), subBooking2.PK.ToString() }, masterBooking.BusinessObjectsWithRelatedEvents.Select(o => o.PK.ToString()));
		}

		public void TestBusinessObjectsWithRelatedEventsCore_WhenSubBooking_ShouldFindMasterBooking()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();
			var subBooking = Helper.CreateBooking();
			Factory.Save();
			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertEquals("Expected master booking records in business objects with related events for sub booking.", masterBooking.PK, subBooking.BusinessObjectsWithRelatedEvents.Single().PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;

			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateBooking();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		TransportBookingTestData Data
		{
			get { return data ?? (data = new TransportBookingTestData(Factory)); }
		}
		TransportBookingTestData data;

		// DtbBookingTest is the result of merging itself and a former class called DtbBooking_OLD_Test. This is where DtbBooking_OLD_Test previously started, hence duplicate regions such as "Related Entities 1" and "Related Entities 2".

		public void TestAddress()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			AssertNotNull(booking.Address);
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, booking.Address.DocAddressType);

			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			booking.Address.OrganisationPK = organisation.PK;
			AssertEquals("Should default to office address", officeAddress.PK, booking.Address.E2_OA_Address);
		}

		public void TestAddress_IsReadOnlyOnMultiJobConsolidation()
		{
			var consolidation = Helper.CreateConsolidation();
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();

			var booking = consolidation.Bookings.AddNew();
			booking.ReadOnly = true;
			AssertEquals(true, booking.Address.ReadOnly);

			booking.ReadOnly = false;
			AssertEquals(false, booking.Address.ReadOnly);

			consolidationMultiJob.Bookings.Add(booking);
			AssertEquals(true, booking.Address.ReadOnly);
		}

		public void TestConsolidationSingleJob_AdditionalTest()
		{
			var booking = Factory.New<DtbBooking>();
			AssertNull(booking.ConsolidationSingleJob);

			var consolidation = Helper.CreateConsolidation();
			AssertEquals("Precondition", TransportConsolidationJobTypes.Codes.Booking, consolidation.KB_JobType);

			consolidation.Bookings.Add(booking);
			AssertEquals(consolidation, booking.ConsolidationSingleJob);
		}

		public void TestConsolidationMultiJob()
		{
			var booking = Factory.New<DtbBooking>();
			AssertNull(booking.ConsolidationMultiJob);

			var consolidation = Helper.CreateConsolidationMultiJob();
			AssertEquals("Precondition", TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, consolidation.KB_JobType);

			consolidation.Bookings.Add(booking);
			AssertEquals(consolidation, booking.ConsolidationMultiJob);
		}

		public void TestAddInstructionChangesStatus()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.Available;

			// add an instruction and deliver it
			var instruction = Helper.CreateInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			instruction.KN_Status = TransportStatuses.Codes.Delivered;
			booking.Instructions.Add(instruction);
			AssertEquals("Wrong booking status after instruction is delivered.", TransportStatuses.Codes.Delivered, booking.KM_Status);

			// add an available instruction
			instruction = Helper.CreateInstruction();
			instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			booking.Instructions.Add(instruction);
			AssertEquals("Wrong booking status", TransportStatuses.Codes.Available, booking.KM_Status);
		}

		public void TestContainers()
		{
			var booking = Factory.New<DtbBooking>();
			AssertEquals(0, booking.Containers.Count());

			var package = Factory.New<PkgPackage>();
			var instruction = booking.Instructions.AddNew();
			Helper.CreatePackageDivot(instruction, package);
			AssertEquals(0, booking.Containers.Count());

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction, container);
			AssertContainsExactElementsInAnyOrder(new[] { container }, Array.ConvertAll(booking.Containers.ToArray(), p => p.Package));

			var instruction2 = booking.Instructions.AddNew();
			var container2 = Factory.New<PkgPackage>();
			container2.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction2, container2);
			AssertContainsExactElementsInAnyOrder(new[] { container, container2 }, Array.ConvertAll(booking.Containers.ToArray(), p => p.Package));
		}

		public void TestLoosePackages()
		{
			var booking = Factory.New<DtbBooking>();
			AssertEquals(0, booking.Containers.Count());

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = booking.Instructions.AddNew();
			Helper.CreatePackageDivot(instruction, container);
			AssertEquals(0, booking.LoosePackages.Count());

			var package = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction, package);
			AssertContainsExactElementsInAnyOrder(new[] { package }, Array.ConvertAll(booking.LoosePackages.ToArray(), p => p.Package));

			var instruction2 = booking.Instructions.AddNew();
			var package2 = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction2, package2);
			AssertContainsExactElementsInAnyOrder(new[] { package, package2 }, Array.ConvertAll(booking.LoosePackages.ToArray(), p => p.Package));
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);

			var relatedJobHeader = Helper.LoadOrCreateJobHeader(parent);
			Assert(booking.BusinessObjectsWithRelatedEvents.Contains(relatedJobHeader));

			var standaloneBooking = Helper.CreateBooking();
			var standaloneJobHeader = Helper.LoadOrCreateJobHeader(standaloneBooking);
			Assert(standaloneBooking.BusinessObjectsWithRelatedEvents.Contains(standaloneJobHeader));
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);
			var org = Helper.CreateOrganisation("TransportCo");
			booking.Address.OrganisationPK = org.PK;

			var consignor = Helper.CreateOrganisation("CNR");
			var consignee = Helper.CreateOrganisation("CNE");
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, consignor.MainAddress);
			Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, consignee.MainAddress);

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { org, consignor, consignee, parent, consolidation }, booking.BusinessObjectsWithRelatedNotes);
		}

		public void TestSubHasMasterInBusinessObjectsWithRelatedNotes()
		{
			var masterBooking = Helper.CreateBooking();
			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { masterBooking, subConsolidation }, subBooking.BusinessObjectsWithRelatedNotes);
		}

		public void TestMasterHasSubsInBusinessObjectsWithRelatedNotes()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking3.KM_KM_MasterBooking = masterBooking.PK;

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { subBooking1, subBooking2, subBooking3, masterConsolidation }, masterBooking.BusinessObjectsWithRelatedNotes);
		}

		// persistent

		public void TestKM_KT_NKBookingTemplate_DropMode()
		{
			var template = Helper.CreateTransportBookingTemplate("TEST", "Test Templace", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			var instructionTemplateWithDropMode = Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.PickUp);
			var instructionTemplateWithoutDropMode = Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			instructionTemplateWithDropMode.K2_DropMode = "DM1";

			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_ParentID = transportBookingParent.PK;
			consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;
			consolidation.KB_JobDirection = "DLV";
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_KT_NKBookingTemplate = "TEST";

			var pickupInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
			var deliveryInstruction = booking.Instructions.Single(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);
			AssertEquals("Default drop mode from parent always", "N34", pickupInstruction.KN_DropMode);
			AssertEquals("Default drop mode from parent always", "N34", deliveryInstruction.KN_DropMode);
		}

		public void TestKM_KT_NKBookingTemplate_DoesNotCreateInstructionsOnSubBooking()
		{
			var template = Helper.CreateTransportBookingTemplate("TEST", "Test Templace", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			var instructionTemplateWithDropMode = Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.PickUp);
			var instructionTemplateWithoutDropMode = Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			instructionTemplateWithDropMode.K2_DropMode = "DM1";

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			Factory.Save();
			var subBooking = Helper.CreateBooking();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_KT_NKBookingTemplate = "TEST";

			Assert("Setting template on sub booking should not create instructions (by calling DefaultTemplate())", !subBooking.Instructions.Any());
		}

		public void TestToggleHeldStatus()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.Available;

			// toggle from available to held
			booking.ToggleHeldStatus(Notify);
			AssertEquals(TransportStatuses.Codes.Held, booking.KM_Status);
			AssertNull(Notify.LastEvent);

			// toggle from held to available
			booking.ToggleHeldStatus(Notify);
			AssertEquals(TransportStatuses.Codes.Available, booking.KM_Status);
			AssertNull(Notify.LastEvent);

			// change the status to delivered
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			confirmation.KK_Actual = ZDateTime.Now;
			AssertEquals("Precondition", TransportStatuses.Codes.Delivered, booking.KM_Status);

			// attempt to toggle from delivered to held (resulting in an error)
			booking.ToggleHeldStatus(Notify);
			AssertEquals("Should not be able to Hold a Booking whose status is not Available.", TransportStatuses.Codes.Delivered, booking.KM_Status);

			AssertEquals(string.Format(
					"This Booking cannot be put on hold because the status is {0}. Only Bookings with a status of {1} can be held.",
					booking.StatusDescription, TransportStatuses.Descriptions.Available), Notify.LastEvent.Message);
		}

		public void TestKM_Chargeable()
		{
			TransportRegistry.Instance.TransportBookingChargeableFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6060m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(170m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			var consolidation = Helper.CreateConsolidation();
			var metricBooking1 = consolidation.Bookings.AddNew();
			var metricBooking2 = consolidation.Bookings.AddNew();
			var imperialBooking = consolidation.Bookings.AddNew();
			var metricPickup1 = metricBooking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var metricDelivery1 = metricBooking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var metricPickup2 = metricBooking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var metricDelivery2 = metricBooking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var imperialPickup = imperialBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var imperialDelivery = imperialBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			// add a complete delivery instruction to a package
			var metricBox1 = consolidation.PackageJob.Packages.AddNew("BOX");
			var metricBox2 = consolidation.PackageJob.Packages.AddNew("BOX");
			var imperialBox = consolidation.PackageJob.Packages.AddNew("BOX");
			metricBox1.KP_Weight = 800;
			metricBox1.KP_Volume = 5;
			metricBox1.KP_VolumeUQ = Constants.Volume.CubicMetres;
			metricBox1.KP_WeightUQ = Constants.Weight.Kilograms;
			metricBox2.KP_Weight = 827;
			metricBox2.KP_Volume = 5;
			metricBox2.KP_VolumeUQ = Constants.Volume.CubicMetres;
			metricBox2.KP_WeightUQ = Constants.Weight.Kilograms;
			Helper.CreatePackageDivot(metricPickup1, metricBox1, 1);
			Helper.CreatePackageDivot(metricDelivery1, metricBox1, 1);
			Helper.CreatePackageDivot(metricPickup2, metricBox2, 1);
			Helper.CreatePackageDivot(metricDelivery2, metricBox2, 1);
			metricBooking1.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			metricBooking2.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			imperialBox.KP_Weight = Constants.Weight.Convert(metricBox1.KP_Weight, metricBox1.KP_WeightUQ, Constants.Weight.Pounds);
			imperialBox.KP_WeightUQ = Constants.Weight.Pounds;
			imperialBox.KP_Volume = Constants.Volume.Convert(metricBox1.KP_Volume, metricBox1.KP_VolumeUQ, Constants.Volume.CubicFeet);
			imperialBox.KP_VolumeUQ = Constants.Volume.CubicFeet;
			Helper.CreatePackageDivot(imperialPickup, imperialBox, 1);
			Helper.CreatePackageDivot(imperialDelivery, imperialBox, 1);
			imperialBooking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;

			AssertEquals("Chargeable (Road)", 825.08M, ZArchitecture.Core.Utilities.Round(metricBooking1.KM_Chargeable, 2));
			AssertEquals("Chargeable Unit (Road)", "KG", metricBooking1.ChargeableUnits);
			AssertEquals("Chargeable (Road)", 827M, ZArchitecture.Core.Utilities.Round(metricBooking2.KM_Chargeable, 2));
			AssertEquals("Chargeable Unit (Road)", "KG", metricBooking2.ChargeableUnits);

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Pounds);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicFeet);

			AssertEquals("Chargeable (Road)", 1794.813M, ZArchitecture.Core.Utilities.Round(imperialBooking.KM_Chargeable, 3));
			AssertEquals("Chargeable Unit (Road)", "LB", imperialBooking.ChargeableUnits);
		}

		public void TestKM_Chargeable_EmptyConversionFactorProducesEmptyChargeable()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			var box = consolidation.PackageJob.Packages.AddNew(Constants.PkgUnit.Box);
			box.KP_Weight = 800;
			box.KP_Volume = 5;
			box.KP_VolumeUQ = Constants.Volume.CubicMetres;
			box.KP_WeightUQ = Constants.Weight.Kilograms;

			Helper.CreatePackageDivot(booking.Instructions.AddNew(InstructionTypes.Codes.PickUp), box, 1);
			Helper.CreatePackageDivot(booking.Instructions.AddNew(InstructionTypes.Codes.Delivery), box, 1);

			AssertEquals(Constants.Weight.Kilograms, booking.ChargeableUnits);
			AssertEquals(1666.667m, booking.KM_Chargeable);

			var conversionFactor1 = new ConversionFactor(0m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var conversionFactor2 = new ConversionFactor(0m, Constants.Volume.CubicInches, Constants.Weight.Pounds);
			var chargeableFactor = new ChargeableFactor(conversionFactor1, conversionFactor2);
			TransportRegistry.Instance.TransportBookingChargeableFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeableFactor);

			AssertEquals("Should simply use metic weight amount as volume cannot be calculated", 800m, booking.KM_Chargeable);

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Pounds);
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicFeet);

			AssertEquals("Should convert only the weight to imperial", 1763.698m, booking.KM_Chargeable);
		}

		public void TestKM_Calc_RoundedChargeable()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package = consolidation.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 800;
			package.KP_Volume = 5;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package.KP_WeightUQ = Constants.Weight.Kilograms;

			Helper.CreatePackageDivot(pickupInstruction, package, 1);

			setRegistry(RatingRoundingTypes.Bankers);
			AssertEquals(1667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.NoRounding);
			AssertEquals(1666.66666666667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.UpTo1);
			AssertEquals(1667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.UpTo1IfLessThanOne);
			AssertEquals(1666.66666666667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.UpToHalf);
			AssertEquals(1667.0M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.Chargeable);
			AssertEquals(1666.66666666667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.Custom);
			AssertEquals(1667M, booking.KM_Calc_RoundedChargeable);

			setRegistry(RatingRoundingTypes.Custom, 0.01);
			AssertEquals(1666.67M, booking.KM_Calc_RoundedChargeable);

			static void setRegistry(ZString roundingType, double customRoundingFactor = 1.0)
			{
				var roundingObj = new DefaultRoundings() { Code = "TBC", RoundingType = roundingType };

				if (roundingType == RatingRoundingTypes.Custom)
				{
					roundingObj.RoundingFactor = customRoundingFactor;
				}

				DataRegistryRating.Instance.DefaultRounding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultRoundingsCollection() { roundingObj });
			}
		}

		public void TestKM_Calc_RoundedChargeableOverride()
		{
			DataRegistryRating.Instance.DefaultRounding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultRoundingsCollection() { new DefaultRoundings() { Code = "TBC", RoundingType = RatingRoundingTypes.Bankers } });

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var package = consolidation.PackageJob.Packages.AddNew("BOX");
			package.KP_Weight = 800;
			package.KP_Volume = 5;
			package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package.KP_WeightUQ = Constants.Weight.Kilograms;

			Helper.CreatePackageDivot(pickupInstruction, package, 1);

			AssertEquals("Without override derives KM_Calc_RoundedChargeable", 1667M, booking.KM_Calc_RoundedChargeable);

			booking.KM_OverrideChargeable = true;
			booking.KM_Calc_RoundedChargeable = 1500.8;

			AssertEquals("While OverrideChargeable is true uses input value and rounds it based on registry setting", 1501M, booking.KM_Calc_RoundedChargeable);
		}

		// calculated

		public void TestDefaultTransportBookingCoFromParent()
		{
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_ParentID = transportBookingParent.PK;
			consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;

			var booking = consolidation.Bookings.AddNew();
			AssertEquals("Company123", booking.Address.E2_CompanyName);
		}

		public void TestDefaultClientServiceLevelFromParent()
		{
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_ParentID = transportBookingParent.PK;
			consolidation.KB_ParentTableCode = transportBookingParent.TablePrefix;

			var booking = consolidation.Bookings.AddNew();
			AssertEquals("123", booking.KM_PL_NKCarrierServiceLevel);
		}

		public void TestFirstPickup()
		{
			var booking = Factory.New<DtbBooking>();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_InstructionType = ConfirmationTypes.Codes.PickUp;
			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_InstructionType = ConfirmationTypes.Codes.PickUp;

			AssertEquals(instruction1, booking.FirstPickup);
		}

		public void TestLatestDeliveryNonDehireConfirmationDate()
		{
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			AssertEquals(ZDateTime.Empty, booking.LatestDeliveryNonDehireConfirmationDate);

			// add a complete pickup confirmation for tomorrow
			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = new ZDateTime(now).AddDays(1);
			AssertEquals(ZDateTime.Empty, booking.LatestDeliveryNonDehireConfirmationDate);

			// add an incomplete delivery confirmation
			deliveryInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals(ZDateTime.Empty, booking.LatestDeliveryNonDehireConfirmationDate);

			// complete the delivery confirmation for today
			deliveryInstruction1.Confirmations[0].KK_Actual = now;
			AssertEquals(now, booking.LatestDeliveryNonDehireConfirmationDate);

			// add a complete delivery instruction to a package
			var box = consolidation.PackageJob.Packages.AddNew("BOX");
			var divot = Helper.CreatePackageDivot(deliveryInstruction2, box, 1);
			divot.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = new ZDateTime(now).AddDays(1);
			AssertEquals(new ZDateTime(now).AddDays(1), booking.LatestDeliveryNonDehireConfirmationDate);
		}

		public void TestLatestDeliveryNonDehireWithDateAndSignedByConfirmation()
		{
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pickupInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			AssertNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);

			// add a complete pickup confirmation for tomorrow
			pickupInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(1);
			AssertNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);

			// add an incomplete delivery confirmation
			var delivery1Confirm = deliveryInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);

			// complete the delivery confirmation for today
			delivery1Confirm.KK_Actual = now.AddDays(-4);
			AssertNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);

			// add a complete confirmation for delivery 2
			var delivery2Confirm = deliveryInstruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			delivery2Confirm.KK_Actual = now.AddDays(-3);
			AssertNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);

			// sign delivery 1
			delivery1Confirm.KK_ReceivedBy = "Bob";
			AssertNotNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);
			AssertEquals("Bob", booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation.KK_ReceivedBy);
			AssertEquals(now.AddDays(-4), booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation.KK_Actual);

			// sign delivery 2 and change date
			delivery2Confirm.KK_Actual = now.AddDays(-2);
			delivery2Confirm.KK_ReceivedBy = "Ted";
			AssertNotNull(booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation);
			AssertEquals("Ted", booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation.KK_ReceivedBy);
			AssertEquals(now.AddDays(-2), booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation.KK_Actual);
		}

		public void TestLatestEmptyReturnedConfirmationDate()
		{
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pickupInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var multiInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			deliveryInstruction1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			deliveryInstruction2.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("No CYD Delivery Confirmations", ZDateTime.Empty, booking.LatestEmptyReturnedConfirmationDate);

			pickupInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-6);
			pickupInstruction2.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-5);
			multiInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddDays(-4);
			multiInstruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddDays(-3);
			AssertEquals("No CYD Delivery Confirmations", ZDateTime.Empty, booking.LatestEmptyReturnedConfirmationDate);

			deliveryInstruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddDays(-2);
			AssertEquals("Has 1 CYD Delivery Confirmation", now.AddDays(-2), booking.LatestEmptyReturnedConfirmationDate);

			deliveryInstruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddDays(-1);
			AssertEquals("Has 2 CYD Delivery Confirmations", now.AddDays(-1), booking.LatestEmptyReturnedConfirmationDate);
		}

		public void TestLatestPickedUpConfirmationDate_IsDeliveryDirectionFalse()
		{
			var now = ZDateTime.Now;
			var booking = Helper.CreateBooking();
			AssertEquals("Precondition: IsDeliveryDirection is false", false, booking.IsDeliveryDirection);
			var cydInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			cydInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("No Pickup Confirmations", ZDateTime.Empty, booking.LatestPickedUpConfirmationDate);

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-6);
			AssertEquals("No Pickup Confirmations", ZDateTime.Empty, booking.LatestPickedUpConfirmationDate);

			cydInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-6);
			AssertEquals("Supplies the Pickup Confirmation", now.AddDays(-6), booking.LatestPickedUpConfirmationDate);

			multiInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-3);
			AssertEquals("Supplies the most recent Pickup Confirmation, even if it's from a multi instruction, as IsDeliveryDirection is false", now.AddDays(-3), booking.LatestPickedUpConfirmationDate);

			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-4);
			AssertEquals("Supplies the most recent Pickup Confirmation", now.AddDays(-3), booking.LatestPickedUpConfirmationDate);
		}

		public void TestLatestPickedUpConfirmationDate_IsDeliveryDirectionTrue()
		{
			var now = ZDateTime.Now;
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is true", true, booking.IsDeliveryDirection);
			var cydInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var multiInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			cydInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("No Pickup Confirmations", ZDateTime.Empty, booking.LatestPickedUpConfirmationDate);

			deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-6);
			AssertEquals("No Pickup Confirmations", ZDateTime.Empty, booking.LatestPickedUpConfirmationDate);

			cydInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-6);
			AssertEquals("Supplies the Pickup Confirmation", now.AddDays(-6), booking.LatestPickedUpConfirmationDate);

			multiInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddDays(-3);
			AssertEquals("Ignores Pickup Confirmation from multi instruction, as IsDeliveryDirection is true", now.AddDays(-6), booking.LatestPickedUpConfirmationDate);
		}

		public void TestSplitBookingDescription()
		{
			// Ensure adding more inactive bookings correctly updates the count
			var now = ZDateTime.Now;
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			AssertEquals("1 of 1", booking.SplitBookingDescription);

			var booking2 = consolidation.Bookings.AddNew();
			AssertEquals("1 of 2", booking.SplitBookingDescription);
			AssertEquals("1 of 2", booking2.SplitBookingDescription);

			var booking3 = consolidation.Bookings.AddNew();
			AssertEquals("1 of 3", booking.SplitBookingDescription);
			AssertEquals("1 of 3", booking2.SplitBookingDescription);
			AssertEquals("1 of 3", booking3.SplitBookingDescription);

			// Ensure DEACTIVATING bookings correctly updates the count
			booking3.KM_IsActive = false;
			AssertEquals("1 of 2", booking.SplitBookingDescription);
			AssertEquals("1 of 2", booking2.SplitBookingDescription);
			AssertEquals("1 of 2", booking3.SplitBookingDescription);

			booking2.KM_IsActive = false;
			AssertEquals("1 of 1", booking.SplitBookingDescription);
			AssertEquals("1 of 1", booking2.SplitBookingDescription);
			AssertEquals("1 of 1", booking3.SplitBookingDescription);

			booking.KM_IsActive = false;
			AssertEquals("1 of 0", booking.SplitBookingDescription);
			AssertEquals("1 of 0", booking2.SplitBookingDescription);
			AssertEquals("1 of 0", booking3.SplitBookingDescription);
		}

		public void TestSplitBookingDescriptionDoesNotThrowAndIsEmptyIfConsolidationSingleJobParentIsNull()
		{
			var booking = Factory.New<DtbBooking>();
			AssertEquals(
				"SplitBookingDescription should be empty if ConsolidationSingleJob is null",
				ZString.Empty,
				booking.SplitBookingDescription);
		}

		// rating

		[TestDate(2013, 05, 06)]
		public void TestIsCustomsCleared()
		{
			// transport booking without parent
			var bookingWithoutParent = Helper.CreateBooking();

			// dummy transport booking parent with CLR event types
			var bookingForDummyParentWithCLREvent = Helper.CreateBooking();
			CreateDummyTransportBookingParentWithEvents(bookingForDummyParentWithCLREvent, ZDateTime.Now, "Z1", "CLR");

			// dummy transport booking parent with CLR event types
			var bookingForDummyParentWithEstimatedCLREvent = Helper.CreateBooking();
			CreateDummyTransportBookingParentWithEvents(bookingForDummyParentWithEstimatedCLREvent, ZDateTime.Now, "Z1", "CLR", true);

			// dummy transport booking parent with CCC event types
			var bookingForDummyParentWithLastCCCEvent = Helper.CreateBooking();
			CreateDummyTransportBookingParentWithEvents(bookingForDummyParentWithLastCCCEvent, ZDateTime.Now, "Z1", "CCC");

			// dummy transport booking parent with triggers and without events
			var bookingForDummyParentWithTrigger = Helper.CreateBooking();
			var dummy = CreateDummyTransportBooking(bookingForDummyParentWithTrigger, "Z1");
			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsCleared.Code;

			Factory.Save();

			AssertEquals(false, bookingWithoutParent.IsCustomsCleared);
			AssertEquals(true, bookingForDummyParentWithCLREvent.IsCustomsCleared);
			AssertEquals(false, bookingForDummyParentWithEstimatedCLREvent.IsCustomsCleared);
			AssertEquals(false, bookingForDummyParentWithLastCCCEvent.IsCustomsCleared);
			AssertEquals(false, bookingForDummyParentWithTrigger.IsCustomsCleared);

			bookingForDummyParentWithCLREvent.ConsolidationSingleJob.Parent.ParentWithWorkflow.GetLogs().Find(l => l.SL_SE_NKEvent == "CLR").FirstOrDefault().Cancel();
			AssertEquals(false, bookingForDummyParentWithCLREvent.IsCustomsCleared);
		}

		DummyWithDtbBooking CreateDummyTransportBookingParentWithEvents(DtbBooking booking, ZDateTime eventTime, ZString description, ZString eventType, bool isEstimated = false)
		{
			var dummy = CreateDummyTransportBooking(booking, description);
			var log = dummy.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = eventType;
				log.SL_EventTime = eventTime;
				log.SL_IsEstimate = isEstimated;
			}
			return dummy;
		}

		DummyWithDtbBooking CreateDummyTransportBooking(DtbBooking booking, ZString description)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = description;
			booking.ConsolidationSingleJob.KB_ParentID = dummy.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = dummy.TablePrefix;
			return dummy;
		}

		public void TestOnConsolidationIsOverrideChanged_WhenBookingIsSetToInactive()
		{
			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);

			Factory.Save();

			var consolidation = Helper.CreateConsolidation();
			var transportCo = Helper.CreateOrganisation("SAM");
			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCo.PK;
			var instruction = booking.Instructions.AddNew();
			var job = Helper.LoadOrCreateJobHeader(booking);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew();
			Helper.CreateAndAssignPackageDivots(booking, package);
			var packageView = booking.Packages_PackageView[0];
			var transportBookingProvider = (ICustomFieldProvider)booking;
			var transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			var transportBookingDynamicBizo = (IDynamicBusinessObject)transportBookingCustomBizo;

			CombineAssertions("Nothing related to the booking or the consolidation should be read only prior to calling OnConsolidationIsOverrideChanged.", () =>
			{
				AssertEquals("Precondition", false, booking.ReadOnly);
				AssertEquals("Precondition", false, booking.DocAddresses.ReadOnly);
				AssertEquals("Precondition", false, booking.Address.ReadOnly);
				AssertEquals("Precondition", false, booking.Instructions.ReadOnly);
				AssertEquals("Precondition", false, instruction.ReadOnly);
				AssertEquals("Precondition", false, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Precondition", false, packageView.ReadOnly);
				AssertEquals("Precondition", false, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Precondition", false, booking.ConsolidationSingleJob.ReadOnly);
				//No precondition for PackageJob here because the PackageJob only sets its ReadOnly status based on the consolidation when it is *first* retrieved.
			});

			Factory.Save();

			booking.KM_IsActive = false;

			booking.OnConsolidationIsOverrideChanged();

			CombineAssertions("Only things related to the booking itself should be read only.", () =>
			{
				AssertEquals("Booking should be ReadOnly.", true, booking.ReadOnly);
				AssertEquals("DocAddress collection should be ReadOnly.", true, booking.DocAddresses.ReadOnly);
				AssertEquals("Transport Co Address should be ReadOnly.", true, booking.Address.ReadOnly);
				AssertEquals("Instruction collection should be ReadOnly.", true, booking.Instructions.ReadOnly);
				AssertEquals("Instruction should be ReadOnly.", true, instruction.ReadOnly);
				AssertEquals("Package View collection should be ReadOnly.", true, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Package view should be ReadOnly.", true, packageView.ReadOnly);
				AssertEquals("Custom Fields should be ReadOnly.", true, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Consolidation should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.ReadOnly);
				AssertEquals("Package Job should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.PackageJob.ReadOnly);
			});
		}

		public void TestOnConsolidationIsOverrideChanged_WhenConsolidationIsSetToReadOnly()
		{
			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);

			Factory.Save();

			var consolidation = Helper.CreateConsolidation();
			var transportCo = Helper.CreateOrganisation("SAM");
			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCo.PK;
			var instruction = booking.Instructions.AddNew();
			var job = Helper.LoadOrCreateJobHeader(booking);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew();
			Helper.CreateAndAssignPackageDivots(booking, package);
			var packageView = booking.Packages_PackageView[0];
			var transportBookingProvider = (ICustomFieldProvider)booking;
			var transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			var transportBookingDynamicBizo = (IDynamicBusinessObject)transportBookingCustomBizo;

			CombineAssertions("Nothing related to the booking or the consolidation should be read only prior to calling OnConsolidationIsOverrideChanged.", () =>
			{
				AssertEquals("Precondition", false, booking.ReadOnly);
				AssertEquals("Precondition", false, booking.DocAddresses.ReadOnly);
				AssertEquals("Precondition", false, booking.Address.ReadOnly);
				AssertEquals("Precondition", false, booking.Instructions.ReadOnly);
				AssertEquals("Precondition", false, instruction.ReadOnly);
				AssertEquals("Precondition", false, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Precondition", false, packageView.ReadOnly);
				AssertEquals("Precondition", false, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Precondition", false, booking.ConsolidationSingleJob.ReadOnly);
				//No precondition for PackageJob here because the PackageJob only sets its ReadOnly status based on the consolidation when it is *first* retrieved.
			});

			Factory.Save();

			consolidation.ReadOnly = true;

			booking.OnConsolidationIsOverrideChanged();

			CombineAssertions("Everything related to both the booking and the consolidation should be read only.", () =>
			{
				AssertEquals("Booking should be ReadOnly.", true, booking.ReadOnly);
				AssertEquals("DocAddress collection should be ReadOnly.", true, booking.DocAddresses.ReadOnly);
				AssertEquals("Transport Co Address should be ReadOnly.", true, booking.Address.ReadOnly);
				AssertEquals("Instruction collection should be ReadOnly.", true, booking.Instructions.ReadOnly);
				AssertEquals("Instruction should be ReadOnly.", true, instruction.ReadOnly);
				AssertEquals("Package View collection should be ReadOnly.", true, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Package view should be ReadOnly.", true, packageView.ReadOnly);
				AssertEquals("Custom Fields should be ReadOnly.", true, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Consolidation should be ReadOnly.", true, booking.ConsolidationSingleJob.ReadOnly);
				AssertEquals("Package Job should be ReadOnly.", true, booking.ConsolidationSingleJob.PackageJob.ReadOnly);
			});
		}

		public void TestOnConsolidationIsOverrideChanged_WhenBookingIsSetToBeASub()
		{
			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);

			Factory.Save();

			var consolidation = Helper.CreateConsolidation();
			var transportCo = Helper.CreateOrganisation("SAM");
			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCo.PK;
			var instruction = booking.Instructions.AddNew();
			var job = Helper.LoadOrCreateJobHeader(booking);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew();
			Helper.CreateAndAssignPackageDivots(booking, package);
			var packageView = booking.Packages_PackageView[0];
			var transportBookingProvider = (ICustomFieldProvider)booking;
			var transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			var transportBookingDynamicBizo = (IDynamicBusinessObject)transportBookingCustomBizo;

			CombineAssertions("Nothing related to the booking or the consolidation should be read only prior to calling OnConsolidationIsOverrideChanged.", () =>
			{
				AssertEquals("Precondition", false, booking.ReadOnly);
				AssertEquals("Precondition", false, booking.DocAddresses.ReadOnly);
				AssertEquals("Precondition", false, booking.Address.ReadOnly);
				AssertEquals("Precondition", false, booking.Instructions.ReadOnly);
				AssertEquals("Precondition", false, instruction.ReadOnly);
				AssertEquals("Precondition", false, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Precondition", false, packageView.ReadOnly);
				AssertEquals("Precondition", false, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Precondition", false, booking.ConsolidationSingleJob.ReadOnly);
				//No precondition for PackageJob here because the PackageJob only sets its ReadOnly status based on the consolidation when it is *first* retrieved.
			});

			Factory.Save();

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			booking.KM_KM_MasterBooking = masterBooking.PK;
			AssertEquals("Precondition: Booking should now be a Sub Booking.", true, booking.IsSub);

			booking.OnConsolidationIsOverrideChanged();

			CombineAssertions("Only things related to the booking itself should be read only, with the exception of the package views.", () =>
			{
				AssertEquals("Booking should be ReadOnly.", true, booking.ReadOnly);
				AssertEquals("DocAddress collection should be ReadOnly.", true, booking.DocAddresses.ReadOnly);
				AssertEquals("Transport Co Address should be ReadOnly.", true, booking.Address.ReadOnly);
				AssertEquals("Instruction collection should be ReadOnly.", true, booking.Instructions.ReadOnly);
				AssertEquals("Instruction should be ReadOnly.", true, instruction.ReadOnly);
				AssertEquals("Package View collection should be ReadOnly.", true, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Package view should *not* be ReadOnly.", false, packageView.ReadOnly);
				AssertEquals("Custom Fields should be ReadOnly.", true, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Consolidation should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.ReadOnly);
				AssertEquals("Package Job should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.PackageJob.ReadOnly);
			});
		}

		public void TestOnConsolidationIsOverrideChanged_WhenBookingHasABookingConfirmedEventAttached()
		{
			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);

			Factory.Save();

			var consolidation = Helper.CreateConsolidation();
			var transportCo = Helper.CreateOrganisation("SAM");
			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCo.PK;
			var instruction = booking.Instructions.AddNew();
			var job = Helper.LoadOrCreateJobHeader(booking);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew();
			Helper.CreateAndAssignPackageDivots(booking, package);
			var packageView = booking.Packages_PackageView[0];
			var transportBookingProvider = (ICustomFieldProvider)booking;
			var transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			var transportBookingDynamicBizo = (IDynamicBusinessObject)transportBookingCustomBizo;

			CombineAssertions("Nothing related to the booking or the consolidation should be read only prior to calling OnConsolidationIsOverrideChanged.", () =>
			{
				AssertEquals("Precondition", false, booking.ReadOnly);
				AssertEquals("Precondition", false, booking.DocAddresses.ReadOnly);
				AssertEquals("Precondition", false, booking.Address.ReadOnly);
				AssertEquals("Precondition", false, booking.Instructions.ReadOnly);
				AssertEquals("Precondition", false, instruction.ReadOnly);
				AssertEquals("Precondition", false, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Precondition", false, packageView.ReadOnly);
				AssertEquals("Precondition", false, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Precondition", false, booking.ConsolidationSingleJob.ReadOnly);
				//No precondition for PackageJob here because the PackageJob only sets its ReadOnly status based on the consolidation when it is *first* retrieved.
			});

			Factory.Save();

			Helper.AddBookingConfirmedEventToBooking(booking, "BLUME_EAD");
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);

			booking.OnConsolidationIsOverrideChanged();

			CombineAssertions("Only things related to the booking itself should be read only.", () =>
			{
				AssertEquals("Booking should be ReadOnly.", true, booking.ReadOnly);
				AssertEquals("DocAddress collection should be ReadOnly.", true, booking.DocAddresses.ReadOnly);
				AssertEquals("Transport Co Address should be ReadOnly.", true, booking.Address.ReadOnly);
				AssertEquals("Instruction collection should be ReadOnly.", true, booking.Instructions.ReadOnly);
				AssertEquals("Instruction should be ReadOnly.", true, instruction.ReadOnly);
				AssertEquals("Package View collection should be ReadOnly.", true, booking.Packages_PackageView.ReadOnly);
				AssertEquals("Package view should be ReadOnly.", true, packageView.ReadOnly);
				AssertEquals("Custom Fields should be ReadOnly.", true, transportBookingCustomBizo.ReadOnly);

				AssertEquals("Consolidation should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.ReadOnly);
				AssertEquals("Package Job should *not* be ReadOnly.", false, booking.ConsolidationSingleJob.PackageJob.ReadOnly);
			});
		}

		public void TestSetReadOnly_BookingForm()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var consolidation = Helper.CreateConsolidation();
			var transportCo = Helper.CreateOrganisation("SAM");
			var booking = Helper.CreateBooking(consolidation);
			booking.Address.OrganisationPK = transportCo.PK;
			var instruction = booking.Instructions.AddNew();
			var job = Helper.LoadOrCreateJobHeader(booking);
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew();
			Helper.CreateAndAssignPackageDivots(booking, package);
			var packageView = booking.Packages_PackageView[0];
			AssertEquals("Precondition", false, booking.ReadOnly);
			AssertEquals("Precondition", false, booking.DocAddresses.ReadOnly);
			AssertEquals("Precondition", false, booking.Address.ReadOnly);
			AssertEquals("Precondition", false, booking.Instructions.ReadOnly);
			AssertEquals("Precondition", false, instruction.ReadOnly);
			AssertEquals("Precondition", false, job.ReadOnly);
			AssertEquals("Precondition", false, booking.Packages_PackageView.ReadOnly);
			AssertEquals("Precondition", false, packageView.ReadOnly);
			Factory.Save();

			booking.KM_IsActive = false;
			booking.UpdateReadOnly_SingleBookingForm();
			AssertEquals("Booking should be ReadOnly.", true, booking.ReadOnly);
			AssertEquals("DocAddress collection should be ReadOnly.", true, booking.DocAddresses.ReadOnly);
			AssertEquals("Transport Co Address should be ReadOnly.", true, booking.Address.ReadOnly);
			AssertEquals("Instruction collection should be ReadOnly.", true, booking.Instructions.ReadOnly);
			AssertEquals("Instruction should be ReadOnly.", true, instruction.ReadOnly);
			AssertEquals("Packages View should be ReadOnly.", true, booking.Packages_PackageView.ReadOnly);
			AssertEquals("Package should be ReadOnly.", true, packageView.ReadOnly);
			AssertEquals("Package Job should be ReadOnly.", true, booking.ConsolidationSingleJob.PackageJob.ReadOnly);
			AssertEquals("Job Header should be Deleted.", true, !job.IsDeleted);
			AssertEquals("Consolidation should be ReadOnly.", true, booking.ConsolidationSingleJob.ReadOnly);

			Factory.Save();
			AssertEquals("Job Header should be cancelled.", true, job.IsCancelled);
		}

		// overrides

		public void TestReadOnly()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			AssertEquals("Precondition", true, booking.KM_IsActive);
			AssertEquals("Precondition", false, booking.ReadOnly);
			AssertEquals("Precondition", false, booking.Address.ReadOnly);
			AssertEquals("Precondition", false, booking.Instructions[0].ReadOnly);

			booking.KM_IsActive = false;
			AssertEquals(true, booking.ReadOnly);
			AssertEquals(true, booking.Address.ReadOnly);
			AssertEquals(true, booking.Instructions[0].ReadOnly);
		}

		// status

		public void TestIsAvailable()
		{
			AssertFlag("IsAvailable", DtbBookingSchema.KM_Status, TransportStatuses.Codes.Available, TransportStatuses.Codes.Held);
		}

		public void TestIsDeliveredEmptyNotReturned()
		{
			AssertFlag("IsDeliveredEmptyNotReturned", DtbBookingSchema.KM_Status, TransportStatuses.Codes.DeliveredEmptyNotReturned, TransportStatuses.Codes.Delivered);
		}

		public void TestIsPickedUp()
		{
			AssertFlag("IsPickedUp", DtbBookingSchema.KM_Status, TransportStatuses.Codes.PickedUp, TransportStatuses.Codes.Available);
		}

		public void TestIsHeld()
		{
			AssertFlag("IsHeld", DtbBookingSchema.KM_Status, TransportStatuses.Codes.Held, TransportStatuses.Codes.Available);
		}

		public void TestIsServiceCommenced()
		{
			AssertFlag("IsServiceCommenced", DtbBookingSchema.KM_Status, TransportStatuses.Codes.ServiceCommenced, TransportStatuses.Codes.Available);
		}

		public void TestIsActionRequired()
		{
			AssertFlag("IsActionRequired", DtbBookingSchema.KM_Status, TransportStatuses.Codes.ActionRequired, TransportStatuses.Codes.Available);
		}

		// considering status

		public void TestIsConsideredDelivered()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("No Delivery Instructions exist, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			pickupInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("No Delivery Instructions exist, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			AssertEquals("An incomplete Delivery Instruction exists, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			deliveryInstruction2.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("2 Delivery Instructions exist however one is incomplete, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			deliveryInstruction1.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(true, booking.IsConsideredDelivered);
		}

		public void TestIsConsideredDelivered_IncludesMulti()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("No Delivery Instructions exist, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			var multiInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			AssertEquals("An incomplete Multi Instruction exists, delivery should be incomplete.", false, booking.IsConsideredDelivered);

			multiInstruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("The Multi Instruction is Delivered, so should be complete.", true, booking.IsConsideredDelivered);
		}

		public void TestIsConsideredDelivered_EmptiesNotReturned()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("No Delivery Instructions exist, delivery should be incomplete.", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			pickupInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("No Delivery Instructions exist, delivery should be incomplete.", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			var deliveryInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			AssertEquals("An incomplete Delivery Instruction exists, delivery should be incomplete.", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			deliveryInstruction1.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("Delivery Status is delivered, but Delivery but not returned requires at least 1 regular delivery and all empties not to be complete.", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			deliveryInstruction1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("Delivery Status is delivered and it's a CYD, but we still require a non cyd instruction to be delivered.", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			deliveryInstruction1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CNE;
			var deliveryInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			deliveryInstruction2.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("2 Delivery Instructions exist, non cyd is complete, and cyd is not complete, should be true.", true, booking.IsConsideredDelivered_EmptiesNotReturned);

			deliveryInstruction2.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("Empty is now complete, this should return false", false, booking.IsConsideredDelivered_EmptiesNotReturned);

			var deliveryInstruction3 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			deliveryInstruction3.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("3rd cyd instruction is added, and not complete, should be true again", true, booking.IsConsideredDelivered_EmptiesNotReturned);
		}

		public void TestIsConsideredPickedUp()
		{
			var originBooking = Factory.New<DtbBooking>();
			var destinationBooking = Factory.New<DtbBooking>();
			originBooking.KM_Direction = Constants.CartageDirection.Origin;
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Origin Booking: No PickUp Instructions exist, pickup should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: No PickUp Instructions exist, pickup should be incomplete.", false, destinationBooking.IsConsideredPickedUp);

			originBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery).KN_Status = TransportStatuses.Codes.Delivered;
			destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery).KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("Origin Booking: No PickUp Instructions exist, pickup should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: No PickUp Instructions exist, pickup should be incomplete.", false, destinationBooking.IsConsideredPickedUp);

			var originPickupInstruction1 = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var destinationPickupInstruction1 = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			AssertEquals("Origin Booking: An incomplete PickUp Instruction exists, pickup should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: An incomplete PickUp Instruction exists, pickup should be incomplete.", false, destinationBooking.IsConsideredPickedUp);

			var originPickupInstruction2 = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var destinationPickupInstruction2 = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			originPickupInstruction2.KN_Status = TransportStatuses.Codes.PickedUp;
			destinationPickupInstruction2.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("Origin Booking: 2 PickUp Instructions exist however one of them is incomplete, pickup should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: 2 PickUp Instructions exist however one of them is incomplete, pickup should be incomplete.", false, destinationBooking.IsConsideredPickedUp);

			originPickupInstruction1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			destinationPickupInstruction1.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals("Origin Booking: First Pickup is an Empty but should still be included, so therefore pickups are not all complete", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: First Pickup is an Empty but should still be included, so therefore pickups are not all complete", false, destinationBooking.IsConsideredPickedUp);

			originPickupInstruction1.KN_Status = TransportStatuses.Codes.PickedUp;
			destinationPickupInstruction1.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("Origin Booking: First Pickup is an Empty but picked up, so therefore pickups are all complete", true, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: First Pickup is an Empty but picked up, so therefore pickups are all complete", true, destinationBooking.IsConsideredPickedUp);

			var originPickupInstruction3 = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var destinationPickupInstruction3 = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			AssertEquals("Origin Booking: First Pickup is complete, 2nd is complete, 3rd is not complete, so therefore pickups are not all complete", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: First Pickup is complete, 2nd is complete, 3rd is not complete, so therefore pickups are not all complete", false, destinationBooking.IsConsideredPickedUp);

			originPickupInstruction3.KN_Status = TransportStatuses.Codes.PickedUp;
			destinationPickupInstruction3.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("Origin Booking: First Pickup is complete, 2nd is complete, 3rd is complete, so therefore pickups are complete", true, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: First Pickup is complete, 2nd is complete, 3rd is complete, so therefore pickups are complete", true, destinationBooking.IsConsideredPickedUp);
		}

		public void TestIsPickedUp_IncludesMulti()
		{
			var originBooking = Factory.New<DtbBooking>();
			var destinationBooking = Factory.New<DtbBooking>();
			originBooking.KM_Direction = Constants.CartageDirection.Origin;
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is false for originBooking", false, originBooking.IsDeliveryDirection);
			AssertEquals("Precondition: IsDeliveryDirection is true for destinationBooking", true, destinationBooking.IsDeliveryDirection);
			AssertEquals("No PickUp Instructions exist, the booking's pickups should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("No PickUp Instructions exist, the booking's pickups should be incomplete.", false, destinationBooking.IsConsideredPickedUp);

			var originPickupInstruction = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var destinationPickupInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			originPickupInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			destinationPickupInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("Origin Booking: A complete PickedUp Instruction exists, so the booking's pickups should be complete.", true, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: A complete PickedUp Instruction exists, so the booking's pickups should be complete.", true, destinationBooking.IsConsideredPickedUp);

			var originMultiInstruction = originBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var destinationMultiInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			AssertEquals("Origin Booking: An incomplete Multi Instruction exists, and IsDeliveryDirection is false, so the booking's pickups should be incomplete.", false, originBooking.IsConsideredPickedUp);
			AssertEquals("Destination Booking: IsDeliveryDirection is true, so multi instructions are ignored, so the booking's pickups should still be complete", true, destinationBooking.IsConsideredPickedUp);

			originMultiInstruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("Origin Booking: Multi Instruction remains incomplete when its status is Delivered, the booking's pickups should remain incomplete", false, originBooking.IsConsideredPickedUp);

			originMultiInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals("Origin Booking: The Multi Instruction is Picked Up, so the booking's pickups should be complete.", true, originBooking.IsConsideredPickedUp);
		}

		public void TestIsConsideredAvailable()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("No LTs or PTs exist, but current status is not service commenced, should not be considered available.", false, booking.IsConsideredAvailable);

			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals("Status is service commenced, but no transport jobs have been added, should not be considered available.", false, booking.IsConsideredAvailable);

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_Direction = "ORG";
			landTransportConsignment.LTC_Status = "BKD";
			landTransportConsignment.LTC_KM_Booking = booking.PK;
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals("LT attached and status is service commenced, should not be considered available", false, booking.IsConsideredAvailable);

			landTransportConsignment.LTC_IsActive = false;
			AssertEquals("LT is no longer active and status is service commenced, should be considered available", true, booking.IsConsideredAvailable);

			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now, isEstimate: false);
			AssertEquals("Inactive LT, but TB is service commenced for a reason other than having a transport job (ServiceCommenced Log), should not be considered available", false, booking.IsConsideredAvailable);

			booking.Logs.RemoveAndDeleteAll();
			var carrierBookingAgent = Factory.New<OrgHeader>();
			var commMode = carrierBookingAgent.EDICommunicationsModes.AddNew();
			commMode.EK_Module = RelatableActivityTypeList.Codes.TransportBooking;
			commMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			commMode.EK_Destination = DtbAgentBooking.ContainerTransportOptimizationCBA;
			booking.CarrierBookingAgentDocAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;
			booking.UpdateStatus();
			AssertEquals("Inactive LT, but TB is service commenced for a reason other than having a transport job (Booking is being managed by authorised CarrierBookingAgent), should not be considered available", false, booking.IsConsideredAvailable);

			booking.CarrierBookingAgentDocAddress.E2_OA_Address = ZGuid.Empty;
			var portTransport = Helper.CreatePortTransport(booking);
			AssertEquals("PT attached and status is service commenced, should not be considered available", false, booking.IsConsideredAvailable);

			portTransport.JJ_IsCancelled = true;
			AssertEquals("PT is no longer active and status is service commenced, should be considered available", true, booking.IsConsideredAvailable);
		}

		public void TestIsConsideredServiceCommenced()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Current status is available, but no LTs or PTs exist, should not be considered service commenced.", false, booking.IsConsideredServiceCommenced);

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_Direction = "ORG";
			landTransportConsignment.LTC_Status = "BKD";
			landTransportConsignment.LTC_KM_Booking = booking.PK;
			AssertEquals("LT attached and status is available, should be considered service commenced", true, booking.IsConsideredServiceCommenced);

			landTransportConsignment.LTC_IsActive = false;
			AssertEquals("Status is available, but LT is inactive, should not be considered service commenced", false, booking.IsConsideredServiceCommenced);

			var portTransport = Helper.CreatePortTransport(booking);
			AssertEquals("PT attached and status is available, should be considered service commenced", true, booking.IsConsideredServiceCommenced);

			portTransport.JJ_IsCancelled = true;
			AssertEquals("Status is available, but PT is inactive, should not be considered service commenced", false, booking.IsConsideredServiceCommenced);
		}

		public void TestHasEmptyDehireInstructions()
		{
			var booking = Factory.New<DtbBooking>();
			AssertEquals(false, booking.HasEmptyDehireInstructions);

			var pickupInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			AssertEquals(false, booking.HasEmptyDehireInstructions);

			pickupInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals(false, booking.HasEmptyDehireInstructions);

			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			AssertEquals(false, booking.HasEmptyDehireInstructions);

			deliveryInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			AssertEquals(true, booking.HasEmptyDehireInstructions);
		}

		// other

		public void TestIsSendingXUSToCTO_WhenIsCurrentlyValidatingSendingXUSToCTOIsTrue()
		{
			var booking = Helper.CreateBooking();

			booking.KM_Status = TransportStatuses.Codes.Available;
			booking.IsValidatingSendingXUSToCTO = true;

			AssertEquals("IsSendingXUSToCTO should return true.", true, booking.IsSendingXUSToCTO);
		}

		public void TestIsSendingXUSToCTO_WhenBookingStatusIsActionRequired()
		{
			var booking = Helper.CreateBooking();

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			booking.IsValidatingSendingXUSToCTO = false;

			AssertEquals("IsSendingXUSToCTO should return true.", true, booking.IsSendingXUSToCTO);
		}

		public void TestIsSendingXUSToCTO_WhenBookingStatusIsNotActionRequiredAndIsCurrentlyValidatingSendingXUSToCTOIsFalse()
		{
			var booking = Helper.CreateBooking();

			booking.KM_Status = TransportStatuses.Codes.Available;
			booking.IsValidatingSendingXUSToCTO = false;

			AssertEquals("IsSendingXUSToCTO should return false.", false, booking.IsSendingXUSToCTO);
		}

		public void TestIsOnMultiJobConsolidation()
		{
			var consolidationSingleJob = Helper.CreateConsolidation();
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();

			var booking = Factory.New<DtbBooking>();
			AssertEquals(false, booking.IsOnMultiJobConsolidation);

			consolidationSingleJob.Bookings.Add(booking);
			AssertEquals(false, booking.IsOnMultiJobConsolidation);

			consolidationMultiJob.Bookings.Add(booking);
			AssertEquals(true, booking.IsOnMultiJobConsolidation);
		}

		public void TestHasPackages()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			AssertEquals(false, booking.HasPackages);

			var package = Factory.New<PkgPackage>();
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals(true, booking.HasPackages);
		}

		public void TestLoad()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.Held;
			Factory.Save();

			var bookingInOtherFactory = new BusinessObjectFactory().Load<DtbBooking>(booking.PK);
			AssertEquals("A loaded Booking that is Held should be ReadOnly.", true, bookingInOtherFactory.ReadOnly);
		}

		public void TestSave_FireEvents_OriginBooking()
		{
			var now = ZDateTime.Now;

			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			AssertEquals("Precondition: IsDeliveryDirection false", false, booking.IsDeliveryDirection);

			var hireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			dehireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;

			// hire empty container
			Helper.GetOrCreateConfirmation(hireInstruction, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);
			Factory.Save();
			AssertLogCounts(booking, 0, 0, 0, 0);

			// pickup goods
			cnrInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);
			Factory.Save();
			AssertLogCounts(booking, 0, 0, 0, 0);

			// deliver goods
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);
			Factory.Save();
			AssertLogCounts(booking, 0, 0, 0, 0);

			// complete the pickup of goods (as IsDeliveryDirection is false, pickup instructions and multi instructions must have a status of picked up)
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-3);
			Factory.Save();
			AssertLogCounts(booking, 1, 0, 0, 0);
			AssertLog(booking, Events.PickupCartageCompleteFinalised, now.AddHours(-3));

			// complete the delivery of goods
			cneInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);
			Factory.Save();
			AssertLogCounts(booking, 1, 1, 0, 0);
			AssertLog(booking, Events.DeliveryCartageCompleteFinalised, now.AddHours(-2));

			// dehire the empty container
			Helper.GetOrCreateConfirmation(dehireInstruction, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);
			Factory.Save();
			AssertLogCounts(booking, 1, 1, 1, 1);
			AssertLog(booking, Events.CartageCompleteFinalised, now.AddHours(-1));
			AssertLog(booking, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_OriginBooking_AllEnteredAtOnce()
		{
			var now = ZDateTime.Now;

			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Origin;
			AssertEquals("Precondition: IsDeliveryDirection false", false, booking.IsDeliveryDirection);

			var hireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			dehireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;

			// hire empty container
			Helper.GetOrCreateConfirmation(hireInstruction, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);

			// pickup goods
			cnrInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);

			// deliver goods
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);

			// complete the pickup of goods (as IsDeliveryDirection is false, pickup instructions and multi instructions must have a status of picked up)
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-3);

			// complete the delivery of goods
			cneInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);

			// dehire the empty container
			Helper.GetOrCreateConfirmation(dehireInstruction, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);

			Factory.Save();
			AssertLogCounts(booking, 1, 1, 1, 1);
			AssertLog(booking, Events.CartageCompleteFinalised, now.AddHours(-1));
			AssertLog(booking, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_DestinationBooking()
		{
			var now = ZDateTime.Now;

			// create destination Booking
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is true", true, booking.IsDeliveryDirection);

			var hireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			dehireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;

			// hire empty container
			Helper.GetOrCreateConfirmation(hireInstruction, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);
			Factory.Save();
			AssertLogCounts(booking, 0, 0, 0, 0);

			// complete the pickup of goods (as IsDeliveryDirection is true, only pickup instructions need to have a status of picked up)
			cnrInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);
			Factory.Save();
			AssertLogCounts(booking, 1, 0, 0, 0);
			AssertLog(booking, Events.PickupCartageCompleteFinalised, now.AddHours(-5)); // latest pickup

			// deliver goods
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);
			Factory.Save();
			AssertLogCounts(booking, 1, 0, 0, 0);

			// pickup at multi
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-3);
			Factory.Save();
			AssertLogCounts(booking, 1, 0, 0, 0);
			AssertLog(booking, Events.PickupCartageCompleteFinalised, now.AddHours(-5)); // ignore pickup at multi

			// complete the delivery of goods
			cneInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);
			Factory.Save();
			AssertLogCounts(booking, 1, 1, 0, 0);
			AssertLog(booking, Events.DeliveryCartageCompleteFinalised, now.AddHours(-2));

			// dehire the empty container
			Helper.GetOrCreateConfirmation(dehireInstruction, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);
			Factory.Save();
			AssertLogCounts(booking, 1, 1, 1, 1);
			AssertLog(booking, Events.CartageCompleteFinalised, now.AddHours(-1));
			AssertLog(booking, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_DestinationBooking_AllEnteredAtOnce()
		{
			var now = ZDateTime.Now;

			// create destination Booking
			var booking = Helper.CreateBooking();
			booking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is true", true, booking.IsDeliveryDirection);

			var hireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;
			dehireInstruction.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD;

			// hire empty container
			Helper.GetOrCreateConfirmation(hireInstruction, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);

			// complete the pickup of goods (as IsDeliveryDirection is true, only pickup instructions need to have a status of picked up)
			cnrInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);

			// deliver goods
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);

			// pickup at multi
			cfsInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-3);

			// complete the delivery of goods
			cneInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);

			// dehire the empty container
			Helper.GetOrCreateConfirmation(dehireInstruction, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);

			Factory.Save();
			AssertLogCounts(booking, 1, 1, 1, 1);
			AssertLog(booking, Events.CartageCompleteFinalised, now.AddHours(-1));
			AssertLog(booking, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_WithoutLoadingConsolidation()
		{
			var now = ZDateTime.Now;

			var consolidation = Helper.CreateConsolidation();
			var destinationBooking = Helper.CreateBooking(consolidation);
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;

			var pickupInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			var pickupConfirmation = pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickupConfirmation_OtherFactory = otherFactory.Load<DtbBookingConfirmation>(pickupConfirmation.PK);
			var deliveryConfirmation_OtherFactory = otherFactory.Load<DtbBookingConfirmation>(deliveryConfirmation.PK);

			pickupConfirmation_OtherFactory.KK_Actual = now.AddHours(-4);
			deliveryConfirmation_OtherFactory.KK_Actual = now.AddHours(-2);

			otherFactory.Save();

			var booking_OtherFactory = otherFactory.Load<DtbBooking>(destinationBooking.PK);
			AssertLogCounts(booking_OtherFactory, 1, 1, 1, 0);
			AssertLog(booking_OtherFactory, Events.CartageCompleteFinalised, now.AddHours(-2));
		}

		void AssertLogCounts(DtbBooking booking, ZInt pickup, ZInt delivery, ZInt complete, ZInt emptyReturned)
		{
			AssertEquals("PickupCartageCompleteFinalised Count.", pickup, GetLogs(booking.Logs, Events.PickupCartageCompleteFinalised).Count());
			AssertEquals("DeliveryCartageCompleteFinalised Count.", delivery, GetLogs(booking.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			AssertEquals("CartageCompleteFinalised Count.", complete, GetLogs(booking.Logs, Events.CartageCompleteFinalised).Count());
			AssertEquals("GateIn Count.", emptyReturned, GetLogs(booking.Logs, Events.GateIn).Count(c => c.SL_Reference == "|FAC=CY"));
		}

		void AssertLog(DtbBooking booking, Event eventType, ZDateTime date, string reference = "")
		{
			var onlyLog = GetLogs(booking.Logs, eventType).SingleOrDefault(l => l.SL_EventTime == date);
			AssertNotNull(string.Format("Could not find log of Event Type '{0}' and Date '{1}'", eventType.Description, date.ToLongTimeString()), onlyLog);

			if (!string.IsNullOrWhiteSpace(reference))
			{
				AssertEquals(onlyLog.SL_Reference, (ZString)reference);
			}
		}

		void AssertLog(IStmALogParent parent, Event eventType, ZString paramtype, ZString jobID)
		{
			var lastEventLog = GetLogs(parent.Logs, eventType).OrderBy(l => l.SL_EventTime).LastOrDefault();
			AssertNotNull("Event log should not be null", lastEventLog);
			AssertEquals("paramtype should be correct", paramtype, lastEventLog.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type));
			AssertEquals("ReferenceFreeText should be correct", jobID, lastEventLog.ReferenceFreeText);
		}

		IEnumerable<StmALog> GetLogs(Logs logs, Event eventType)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == eventType.Code);
		}

		public void TestInstructionView()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(TransportBookingInstructionView.Instruction, booking.InstructionView);
			AssertNull(booking.SelectedPackage_PackageView);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, null);
			AssertEquals(TransportBookingInstructionView.Package, booking.InstructionView);
			AssertNull(booking.SelectedPackage_PackageView);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.CustomFields, null);
			AssertEquals(TransportBookingInstructionView.CustomFields, booking.InstructionView);
			AssertNull(booking.SelectedPackage_PackageView);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.AdditionalReferences, null);
			AssertEquals(TransportBookingInstructionView.AdditionalReferences, booking.InstructionView);
			AssertNull(booking.SelectedPackage_PackageView);

			var packageView = new DtbBookingPackage_PackageView(Factory.New<PkgPackage>(), booking);
			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, packageView);
			AssertEquals(TransportBookingInstructionView.Package, booking.InstructionView);
			AssertEquals(packageView, booking.SelectedPackage_PackageView);

			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Instruction, null);
			AssertEquals(TransportBookingInstructionView.Instruction, booking.InstructionView);
			AssertNull(booking.SelectedPackage_PackageView);
		}

		public void TestDeactivate()
		{
			var booking = Helper.CreateBooking();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var package1 = Helper.CreatePackage("p1");
			var package2 = Helper.CreatePackage("p2");
			var package3 = Helper.CreatePackage("p3");
			var divot_instruction1_package1 = Helper.CreatePackageDivot(instruction1, package1, 1);
			var divot_instruction1_package2 = Helper.CreatePackageDivot(instruction1, package2, 1);
			var divot_instruction2_package3 = Helper.CreatePackageDivot(instruction2, package3, 1);
			AssertEquals("Precondition", true, booking.KM_IsActive);
			AssertEquals("Precondition", 2, booking.Instructions[0].PackageDivots.Count);
			AssertEquals("Precondition", 1, booking.Instructions[1].PackageDivots.Count);

			var notify = Notify;
			notify.Response = true;
			booking.Deactivate(notify);
			AssertEquals("Movement should be deactivated", false, booking.KM_IsActive);
			AssertEquals("Movement should have deactivated status", TransportStatuses.Codes.Deactivated, booking.KM_Status);
			AssertEquals("Movement should have deactivated status", TransportStatuses.Descriptions.Deactivated, booking.StatusDescription);
			AssertEquals("Package divots should be deleted from all instructions", 0, booking.Instructions[0].PackageDivots.Count);
			AssertEquals("Package divots should be deleted from all instructions", 0, booking.Instructions[1].PackageDivots.Count);
		}

		public void TestActivate()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsActive = false;
			booking.Activate();
			AssertEquals("Movement should be activated", true, booking.KM_IsActive);
		}

		public void TestDelete()
		{
			// blank
			var booking = Helper.CreateBooking();
			booking.Delete();
			AssertEquals(true, booking.IsDeleted);

			// with package
			var consolidation = Helper.CreateConsolidation();
			var bookingWithPackages = consolidation.Bookings.AddNew();
			var pickupInstruction = bookingWithPackages.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var box = consolidation.PackageJob.Packages.AddNew("BOX");
			Helper.CreatePackageDivot(pickupInstruction, box, 1);
			bookingWithPackages.Delete();
			AssertEquals(true, bookingWithPackages.IsDeleted);

			// with dates and reference
			consolidation = Helper.CreateConsolidation();
			var bookingWithDatesAndReferences = consolidation.Bookings.AddNew();
			pickupInstruction = bookingWithDatesAndReferences.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = ZDateTime.Now.AddDays(1);
			bookingWithDatesAndReferences.Delete();
			AssertEquals(true, bookingWithDatesAndReferences.IsDeleted);

			EventHandler<CancelEventArgs> cancel = (object sender, CancelEventArgs e) => { e.Cancel = true; };
			EventHandler<CancelEventArgs> dontCancel = (object sender, CancelEventArgs e) => { e.Cancel = false; };

			// blank, but cancel
			var bookingButCancel = Helper.CreateBooking();
			bookingButCancel.CancelBookingDelete += cancel;
			bookingButCancel.Delete();
			AssertEquals("Should have no affect", true, bookingButCancel.IsDeleted);

			// with package,  but cancel
			consolidation = Helper.CreateConsolidation();
			var bookingWithPackagesButCancel = consolidation.Bookings.AddNew();
			pickupInstruction = bookingWithPackagesButCancel.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			box = consolidation.PackageJob.Packages.AddNew("BOX");
			Helper.CreatePackageDivot(pickupInstruction, box, 1);
			bookingWithPackagesButCancel.CancelBookingDelete += cancel;
			bookingWithPackagesButCancel.Delete();
			AssertEquals(false, bookingWithPackagesButCancel.IsDeleted);

			bookingWithPackagesButCancel.CancelBookingDelete -= cancel;
			bookingWithPackagesButCancel.CancelBookingDelete += dontCancel;
			bookingWithPackagesButCancel.Delete();
			AssertEquals(true, bookingWithPackagesButCancel.IsDeleted);

			// with dates and reference, but cancel
			consolidation = Helper.CreateConsolidation();
			var bookingWithDatesAndReferencesButCancel = consolidation.Bookings.AddNew();
			pickupInstruction = bookingWithDatesAndReferencesButCancel.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = ZDateTime.Now.AddDays(1);
			bookingWithDatesAndReferencesButCancel.CancelBookingDelete += cancel;
			bookingWithDatesAndReferencesButCancel.Delete();
			AssertEquals(false, bookingWithDatesAndReferencesButCancel.IsDeleted);

			bookingWithDatesAndReferencesButCancel.CancelBookingDelete -= cancel;
			bookingWithDatesAndReferencesButCancel.CancelBookingDelete += dontCancel;
			bookingWithDatesAndReferencesButCancel.Delete();
			AssertEquals(true, bookingWithDatesAndReferencesButCancel.IsDeleted);
		}

		public void TestCanDelete()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(true, booking.CanDelete);

			Factory.Save();
			AssertEquals(false, booking.CanDelete);
		}

		public void TestReasonForNotAbleToDelete()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Saved Bookings cannot be deleted, however you may Deactivate the Booking instead.", booking.ReasonForNotAbleToDelete);
		}

		public void TestCanCancel()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);
		}

		public void TestDeactivateBooking_UnAssignPackages()
		{
			var booking = Helper.CreateBooking();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var package1 = Helper.CreatePackage("p1");
			var package2 = Helper.CreatePackage("p2");
			var package3 = Helper.CreatePackage("p3");
			var divot_instruction1_package1 = Helper.CreatePackageDivot(instruction1, package1, 1);
			var divot_instruction1_package2 = Helper.CreatePackageDivot(instruction1, package2, 1);
			var divot_instruction2_package3 = Helper.CreatePackageDivot(instruction2, package3, 1);
			AssertEquals("Precondition", true, booking.KM_IsActive);
			AssertEquals("Precondition", 2, instruction1.PackageDivots.Count);
			AssertEquals("Precondition", 1, instruction2.PackageDivots.Count);

			booking.IsCancelled = true;
			AssertEquals("Movement should be deactivated", false, booking.KM_IsActive);
			AssertEquals("Movement should have deactivated status", TransportStatuses.Codes.Deactivated, booking.KM_Status);
			AssertEquals("Movement should have deactivated status", TransportStatuses.Descriptions.Deactivated, booking.StatusDescription);
			AssertEquals("Package divots should be deleted from all instructions", 0, instruction1.PackageDivots.Count);
			AssertEquals("Package divots should be deleted from all instructions", 0, instruction2.PackageDivots.Count);
		}

		public void TestCanCancelBooking_ActivePortTransport()
		{
			var booking = Helper.CreateBooking();
			var portTransport = Helper.CreatePortTransport(booking);
			portTransport.JJ_IsCancelled = false;

			AssertEquals("Shouldn't be able to deactivate as there's an attached active Transport Job", false, ((ZString)booking.CanCancel()).IsEmpty);

			booking = Helper.CreateBooking();
			portTransport = Helper.CreatePortTransport(booking);
			portTransport.JJ_IsCancelled = true;

			AssertEquals("Should be able to deactivate as no active Transport Jobs", true, ((ZString)booking.CanCancel()).IsEmpty);
		}

		public void TestCanCancelBooking_ActiveLandTransport()
		{
			var booking = Helper.CreateBooking();
			var landTransport = Helper.CreateConsignment(booking.PK);
			landTransport.LTC_IsActive = true;

			AssertEquals("Shouldn't be able to deactivate as there's an attached active Transport Job", false, ((ZString)booking.CanCancel()).IsEmpty);

			booking = Helper.CreateBooking();
			landTransport = Helper.CreateConsignment(booking.PK);
			landTransport.LTC_IsActive = false;

			AssertEquals("Should be able to deactivate as no active Transport Jobs", true, ((ZString)booking.CanCancel()).IsEmpty);
		}

		public void TestCanCancel_DoesNotAllowCancelIfThereAreAnyRelatedCharges()
		{
			// no job header -- can cancel
			var booking = Helper.CreateBooking();
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);

			// job header but no charges -- can cance
			var job = Helper.LoadOrCreateJobHeader(booking);
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			Factory.Save();
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);

			// job header with charges -- *cannot* cancel
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_GE = job.JH_GE;
			charge.JR_GB = job.JH_GB;
			charge.JR_AC = chargeCode.PK;
			charge.JR_LocalSellAmt = 1;
			charge.JR_OSSellAmt = 1;
			Factory.Save();
			AssertEquals("Expected not to cancel.", false, ((ZString)booking.CanCancel()).IsEmpty);
		}

		public void TestCanCancel_MasterBookingDoesNotAllowCancelIfThereAreAnyAttachedSubBookings()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking = Helper.CreateBooking();
			masterBooking.SubBookings.Add(subBooking);

			AssertEquals("CanCancel should return an error message for a master booking with an attached Sub Booking.", "Cannot Deactivate this Master Booking as it has one or more attached Sub Bookings.", masterBooking.CanCancel());
		}

		public void TestCanCancel_MasterBookingAllowsCancelIfThereAreNoAttachedSubBookings()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			AssertEquals("Precondition: There should be no Sub Bookings attached to the Master Booking.", 0, masterBooking.SubBookings.Count);

			AssertEquals("CanCancel should return no error message.", string.Empty, masterBooking.CanCancel());
		}

		public void TestDeleteAllInstructionsDoesNotResequenceInstructions()
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_JobDirection = "DLV";
			booking.KM_Direction = "DST";
			booking.KM_KT_NKBookingTemplate = "IFUD";

			var bookingInstructionsListChangedOccurrences = new List<ListChangedType>();

			((IBindingList)booking.Instructions).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				bookingInstructionsListChangedOccurrences.Add(e.ListChangedType);
			};

			AssertEquals("Precondition: Adding the template should have added four instructions", 4, booking.Instructions.Count);

			booking.DeleteAllInstructions();
			var expectedBookingInstructionsListChangedOccurrences = new[] { ListChangedType.Reset };

			CombineAssertions("Check that instructions have been deleted and no re-sequencing took place", () =>
			{
				Assert("All booking.Instructions should have been deleted", !booking.Instructions.Any());
				AssertContainsExactElementsInAnyOrder($"booking.Instructions should only have had one occurrence of the Reset ListChanged event (from the call to DeleteAll() - see ActiveBusinessObjectCollectionIndex.DeleteAll()), no re-sequencing should occur after each delete (indicated by extra Reset ListChanged events)", expectedBookingInstructionsListChangedOccurrences, bookingInstructionsListChangedOccurrences);
			});
		}

		// interface members

		[TestedType(typeof(DtbBooking))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
			protected override BusinessObject GetBizo()
			{
				var helper = new TransportBookingTestHelper(Factory);
				var template = helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
				helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CTO, InstructionTypes.Codes.PickUp);
				helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
				helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CYD, InstructionTypes.Codes.Delivery);

				var transportBooking = helper.CreateConsolidation();
				var booking = transportBooking.Bookings.AddNew();
				booking.KM_KT_NKBookingTemplate = template.KT_Code;
				return booking;
			}
		}

		public void TestICustomFieldProvider_GetCustomBusinessObject()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, LocalCartageJobOrgTypeList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var consolBooking = Helper.CreateConsolidation();
			var booking = consolBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;

			var workflowTemplate = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode);
			Helper.AddCustomField(workflowTemplate, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(workflowTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(workflowTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(workflowTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var transportBookingProvider = (ICustomFieldProvider)booking;
			var transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			var transportBookingDynamicBizo = (IDynamicBusinessObject)transportBookingCustomBizo;
			AssertNotNull(transportBookingDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
			AssertNotNull(transportBookingDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
			AssertNotNull(transportBookingDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
			AssertNotNull(transportBookingDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
			AssertEquals(false, transportBookingCustomBizo.ReadOnly);

			var newFactory = new BusinessObjectFactory();
			var booking_NewFactory = newFactory.Load<DtbBooking>(booking.PK);
			booking_NewFactory.ReadOnly = true;
			transportBookingProvider = booking_NewFactory;
			transportBookingCustomBizo = transportBookingProvider.GetCustomBusinessObject();
			AssertEquals(true, transportBookingCustomBizo.ReadOnly);
		}

		public void TestCustomBusinessObject_SubDoesNotLookToMasterForCustomFields()
		{
			var subBooking = Factory.NewWithValidTestData<DtbBooking>();
			var masterBooking = Factory.NewWithValidTestData<DtbBooking>();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			AssertEquals("Precondition: subBooking is recognized as a sub.", true, subBooking.IsSub);
			AssertEquals("The Instruction should be the parent of its own Custom Fields.", subBooking.PK, subBooking.CustomBusinessObject_ForTest.Parent.PK);
		}

		public void IJobNumberForWorkflow_JobNumber()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_JobID = "FOF12341298";
			AssertEquals("((IJobNumber)booking).JobNumber", "FOF12341298", ((IJobNumberForWorkflow)booking).JobNumber);

			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "FOF12341298";
			AssertEquals("Should STILL be Booking Job Number", "FOF12341298", ((IJobNumberForWorkflow)booking).JobNumber);
		}

		public void TestIJobInvoicingPlugIn_InvoicingSupporter()
		{
			var booking = Factory.New<DtbBooking>();
			var iBooking = (IJobInvoicingPlugIn)booking;
			var invoicingSupporter = iBooking.InvoicingSupporter;
			AssertNotNull(invoicingSupporter);
			AssertEquals(typeof(DtbBookingInvoicingSupporter), invoicingSupporter.GetType());
			AssertEquals("Cache InvoicingSupporter", invoicingSupporter, iBooking.InvoicingSupporter);
		}

		public void TestIJobInvoicingPlugIn_AllowInvoiceDeletion()
		{
			var booking = Factory.New<DtbBooking>();
			var iBooking = (IJobInvoicingPlugIn)booking;
			AssertEquals("Allow", true, iBooking.AllowInvoiceDeletion);
		}

		[ExpectNoExceptions()]
		public void TestIJobInvoicingPlugIn_OnJobCreating()
		{
			var booking = Factory.New<DtbBooking>();
			var iBooking = (IJobInvoicingPlugIn)booking;
			iBooking.OnJobCreating(null);
		}

		[ExpectNoExceptions()]
		public void TestIJobInvoicingPlugIn_OnJobDeleting()
		{
			var booking = Factory.New<DtbBooking>();
			var iBooking = (IJobInvoicingPlugIn)booking;
			iBooking.OnJobDeleting(null);
		}

		public void TestIJobInvoicingPlugIn_SetJobNumberFieldOnSaving()
		{
			var booking = Factory.New<DtbBooking>();
			var iBooking = (IJobInvoicingPlugIn)booking;
			AssertEquals("", booking.KM_JobID);

			iBooking.SetJobNumberFieldOnSaving();
			AssertEquals("TB00000001", booking.KM_JobID);
		}

		public void TestIAutoRatingProvider_AutoRating()
		{
			Data.CreateTransportBookings();

			AssertEquals(FreightMode.FRO, Data.ExportFCLTransportBooking.GetFirstAdapter().FreightMode);
			AssertEquals(FreightMode.LRO, Data.ImportLCLTransportBooking.GetFirstAdapter().FreightMode);

			var invalidBooking = Factory.New<DtbBooking>();
			invalidBooking.KM_RatingFreightMode = "";
			AssertNull(invalidBooking.GetFirstAdapter());
		}

		public void TestAutoRating_WithOverridenChargeableWeight()
		{
			Data.CreateTransportBookings();

			AssertEquals(FreightMode.FRO, Data.ExportFCLTransportBooking.GetFirstAdapter().FreightMode);
			AssertEquals(FreightMode.LRO, Data.ImportLCLTransportBooking.GetFirstAdapter().FreightMode);

			var invalidBooking = Factory.New<DtbBooking>();
			invalidBooking.KM_RatingFreightMode = "";
			AssertNull(invalidBooking.GetFirstAdapter());
		}

		public void IAutoRatingAdditionalJobs_AdditionalJobs()
		{
			Data.CreateTransportBookings();

			AssertEquals(1, Data.ExportFCLTransportBooking.GetRatingAdapters().Count);
			AssertEquals(1, Data.ImportFCLTransportBooking.GetRatingAdapters().Count);
			AssertEquals(2, Data.ImportMixedTransportBooking.GetRatingAdapters().Count);
		}

		public void TestLoadingAsWrongTypeWithoutFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBooking>(), ObjectFactory.GetType<IDtbBookingConsignment>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: false);
		}

		public void TestLoadingAsWrongTypeWithFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBooking>(), ObjectFactory.GetType<IDtbBookingConsignment>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: true);
		}

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.DtbTransport);
			}
		}

		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			var instructionA = booking.Instructions.AddNew();
			var instructionB = booking.Instructions.AddNew();
			var confirmationA1 = instructionA.Confirmations.AddNew();
			var confirmationA2 = instructionA.Confirmations.AddNew();
			var confirmationB1 = instructionB.Confirmations.AddNew();
			var confirmationB2 = instructionB.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { confirmationA1, confirmationA2, confirmationB1, confirmationB2, instructionA, instructionB }, booking.BusinessObjectsWithRelatedEvents);
		}

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var booking = (DtbBooking)GetNewBusinessObject();

			var additionalReferenceNumberLookups = booking.AdditionalReferenceNumbers.AddNew().Lookups;
			var additionalReferenceNumberTypes = additionalReferenceNumberLookups.GetType().GetProperty("AdditionalReferenceNumberTypes").GetValue(additionalReferenceNumberLookups, null);

			AssertContainsExactElementsInAnyOrder(
				"Should have correct additional reference number types",
				new[]
				{
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"CLR|Customer Reference Number|Y",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"ORD|Order Number|N",
					"BPR|Booking Party Reference|Y",
					"CLN|Client|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|Y",
					"CBK|Carrier Booking Reference|Y",
				},
				((CodeDescriptionPairList)additionalReferenceNumberTypes)
				.Cast<TransportReferenceNumberType>()
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		public void TestIAdditionalReferenceNumberTypeProvider_WhenValuesRetrievedFromDatabase()
		{
			var list = new TransportReferenceNumberTypeCollection();
			list.Add("AAA", (NoResString)"A Desc", isUnique: false);
			list.Add("BBB", (NoResString)"B Desc", isUnique: true);

			TransportRegistry.Instance.AdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var booking = (DtbBooking)GetNewBusinessObject();

			var additionalReferenceNumberLookups = booking.AdditionalReferenceNumbers.AddNew().Lookups;
			var additionalReferenceNumberTypes = additionalReferenceNumberLookups.GetType().GetProperty("AdditionalReferenceNumberTypes").GetValue(additionalReferenceNumberLookups, null);

			AssertContainsExactElementsInAnyOrder(
				"Should include system values when the default has been overridden and the saved value does not include them",
				new[]
				{
					"AAA|A Desc|N",
					"BBB|B Desc|Y",
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"CLR|Customer Reference Number|Y",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"ORD|Order Number|N",
					"BPR|Booking Party Reference|Y",
					"CLN|Client|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|Y",
					"CBK|Carrier Booking Reference|Y",
				},
				((CodeDescriptionPairList)additionalReferenceNumberTypes)
				.Cast<TransportReferenceNumberType>()
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		public void TestBookingTemplate()
		{
			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = (DtbBooking)GetNewBusinessObject();
			booking.KM_KB_Booking = consolidation.PK;
			AssertNull(booking.BookingTemplate);

			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals(template, booking.BookingTemplate);
		}

		public void TestConsolidationSingleJob()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = Factory.New<DtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			AssertEquals(consolidation, booking.ConsolidationSingleJob);
			AssertEquals(ExpectedConsolidationType, booking.ConsolidationSingleJob.GetType());
		}

		Type ExpectedConsolidationType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		public void TestInstructions()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			booking.Instructions.AddNew();
			AssertEquals(typeof(DtbBookingInstructionCollection), booking.Instructions.GetType());
			AssertEquals("Instructions should be registered editable on Transport.", true, booking.IsRegisteredEditableChildObject(booking.Instructions));
		}

		public void TestJob()
		{
			var transportJob = GetSaveableTransportJob();
			AssertNull(transportJob.Job);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = transportJob.PK;
			job.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			AssertNotNull(transportJob.Job);
		}

		public void TestCancelDeactivatesJobHeader()
		{
			var booking = GetSaveableTransportJob();

			var job = Helper.LoadOrCreateJobHeader(booking);
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			AssertEquals(true, ((ZString)booking.CanCancel()).IsEmpty);
			AssertNotNull(booking.Job);
			Factory.Save();

			booking.IsCancelled = true;
			Factory.Save();

			AssertNull("Expected JobHeader is removed.", booking.Job);
			AssertEquals(true, job.IsCancelled);
		}

		public void TestPackages()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(0, booking.AssignedPackages.Count);

			var instruction = booking.Instructions.AddNew();
			AssertEquals(0, booking.AssignedPackages.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals(0, booking.AssignedPackages.Count);

			var package = Factory.New<PkgPackage>();
			AssertEquals(0, booking.AssignedPackages.Count);

			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package }, booking.AssignedPackages);

			var instruction2 = booking.Instructions.AddNew();
			var packageDivot2 = instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package, package2 }, booking.AssignedPackages);
		}

		public void TestPackages_PackageView()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(0, booking.Packages_PackageView.Count);
			AssertEquals(true, booking.IsRegisteredEditableChildObject(booking.Packages_PackageView));

			var instruction = booking.Instructions.AddNew();
			AssertEquals(0, booking.Packages_PackageView.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals(0, booking.Packages_PackageView.Count);

			var package = Factory.New<PkgPackage>();
			AssertEquals(0, booking.Packages_PackageView.Count);

			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package }, Array.ConvertAll(booking.Packages_PackageView.Cast<DtbBookingPackage_PackageView>().ToArray(), p => p.Package));

			var instruction2 = booking.Instructions.AddNew();
			var packageDivot2 = instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package, package2 }, Array.ConvertAll(booking.Packages_PackageView.Cast<DtbBookingPackage_PackageView>().ToArray(), p => p.Package));
		}

		public void TestPackageDivots()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(0, booking.PackageDivots.Count);

			var instruction = booking.Instructions.AddNew();
			AssertEquals(0, booking.PackageDivots.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbBookingInstructionPkgDivot[] { packageDivot }, booking.PackageDivots);

			var instruction2 = booking.Instructions.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbBookingInstructionPkgDivot[] { packageDivot }, booking.PackageDivots);

			var packageDivot2 = instruction2.PackageDivots.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbBookingInstructionPkgDivot[] { packageDivot, packageDivot2 }, booking.PackageDivots);
		}

		public void TestNoteTypes()
		{
			var booking = (DtbBooking)GetNewBusinessObject();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation,
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes
			}, booking.NoteTypes);
		}

		public void TestNoteContextsForRelatedNotes_Module()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			var noteContexts = booking.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("Cartage should load notes for 'Transport' module only.", StmNoteContextModule.A | StmNoteContextModule.T, noteContexts.Module);
		}

		public void TestNoteContextsForRelatedNotes_Direction()
		{
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Export, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Origin, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Import, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Destination, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.LineHaul, false, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Local, false, false);
			CreateAndAssertNoteContextDirection("", false, false);
		}

		void CreateAndAssertNoteContextDirection(ZString direction, bool hasExport, bool hasImport)
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			booking.KM_Direction = direction;
			var noteContexts = booking.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("Direction: All", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.A));
			AssertEquals("Direction: Import", hasImport, noteContexts.Direction.HasFlag(StmNoteContextDirection.I));
			AssertEquals("Direction: Export", hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.E));
			AssertEquals("Direction: Import and Export", hasImport || hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.B));
			AssertEquals("Direction: Domestic", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.D));
			AssertEquals("Direction: Cross Trade", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.X));
			AssertEquals("Direction: All Forwarding", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.F));
			AssertEquals("Direction: Other / Warehouse Out", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.O));
			AssertEquals("Direction: Warehouse In", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.R));
		}

		public void TestNoteContextsForRelatedNotes_FreightMode()
		{
			CreateAndAssertNoteContextFreightMode(true, false);
			CreateAndAssertNoteContextFreightMode(false, true);
			CreateAndAssertNoteContextFreightMode(true, true);
			CreateAndAssertNoteContextFreightMode(false, false);
		}

		void CreateAndAssertNoteContextFreightMode(bool hasContainers, bool hasLoose)
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			var instruction = booking.Instructions.AddNew();
			if (hasContainers)
			{
				var package = Helper.CreatePackage("", 1, "CNT");
				Helper.CreatePackageDivot(instruction, package, 1);
			}

			if (hasLoose)
			{
				var package = Helper.CreatePackage("", 1, "PLT");
				Helper.CreatePackageDivot(instruction, package, 1);
			}

			var noteContexts = booking.GetNoteContextsForRelatedNotesForTest();
			AssertEquals("FreightMode: All", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.A));
			AssertEquals("FreightMode: Sea", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.S));
			AssertEquals("FreightMode: FCL", hasContainers, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.F));
			AssertEquals("FreightMode: LCL", hasLoose, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.L));
			AssertEquals("FreightMode: Air", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.I));
			AssertEquals("FreightMode: Road", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.R));
			AssertEquals("FreightMode: Rail", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.W));
			AssertEquals("FreightMode: Air and Sea", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.B));
			AssertEquals("FreightMode: Warehouse Orders", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.O));
			AssertEquals("FreightMode: Warehouse Transfers", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.T));
			AssertEquals("FreightMode: Warehouse Adjustments", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.D));
			AssertEquals("FreightMode: Warehouse Periodic Billing", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.P));
		}

		public void TestSetDefaultValues()
		{
			var transportJob = (DtbBooking)GetNewBusinessObject();
			AssertEquals(TransportStatuses.Codes.Available, transportJob.KM_Status);

			AssertEquals(GlbBranch.CurrentBranch.PK, transportJob.KM_GB_Branch);
		}

		public void TestDefaultConfirmations()
		{
			// template
			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);

			// consol and booking
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = consolidation.Bookings.AddNew();

			// assign packages first
			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);
			packageJob.Packages.Add(container);
			packageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			// set template
			booking.KM_KT_NKBookingTemplate = "IFCL";

			// assert Packages and Confirmations assigned
			AssertEquals("FCL Import", booking.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, booking.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, booking.KM_RatingFreightMode);

			var instructions = booking.Instructions.Cast<DtbBookingInstruction>();
			var cto = instructions.First(i => i.KN_Sequence == 1);
			var cne = instructions.First(i => i.KN_Sequence == 2);
			var cyd = instructions.First(i => i.KN_Sequence == 3);
			AssertContainsExactElementsInAnyOrder(new[] { container }, cto.DivotsWithPackages.Packages);
			AssertContainsExactElementsInAnyOrder(new[] { container, containerInnerPackage, topLevelPackage }, cne.DivotsWithPackages.Packages);
			AssertContainsExactElementsInAnyOrder(new[] { container, topLevelPackage }, cyd.DivotsWithPackages.Packages);
			AssertNotNull(cto.Confirmations.Cast<DtbBookingConfirmation>().Single(c => c.IsPickUp));
			AssertNotNull(cne.Confirmations.Cast<DtbBookingConfirmation>().Single(c => c.IsDelivery));
			AssertNotNull(cyd.Confirmations.Cast<DtbBookingConfirmation>().Single(c => c.IsDelivery));
		}

		// persistent

		public void TestKM_IsActive()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals("Precondition", TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("Precondition", true, booking.KM_IsActive);

			booking.KM_IsActive = false;
			AssertEquals("Precondition", TransportStatuses.Codes.Deactivated, booking.KM_Status);
			AssertEquals("Precondition", false, booking.KM_IsActive);

			booking.KM_IsActive = true;
			AssertEquals("Precondition", TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("Precondition", true, booking.KM_IsActive);
		}

		public void TestKM_KT_NKBookingTemplate()
		{
			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			cneInstructionTemplate.K2_DropMode = "SDL";

			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = consolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals("FCL Import", booking.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, booking.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, booking.KM_RatingFreightMode);
			AssertInstructionAndTemplate(ctoInstructionTemplate, booking.Instructions[0]);
			AssertInstructionAndTemplate(cneInstructionTemplate, booking.Instructions[1]);
			AssertInstructionAndTemplate(cydInstructionTemplate, booking.Instructions[2]);

			booking.KM_KT_NKBookingTemplate = "";
			AssertEquals("", booking.KM_Description);
		}

		void AssertInstructionAndTemplate(DtbBookingInstructionTmpl template, DtbBookingInstruction instruction)
		{
			AssertEquals(template.K2_Sequence, instruction.KN_Sequence);
			AssertEquals(template.K2_InstructionType, instruction.KN_InstructionType);
			AssertEquals(template.K2_OrgType, instruction.OrganisationType);
			AssertEquals(template.K2_PackageType, instruction.PackageCategory);
			AssertEquals(template.K2_DropMode, instruction.KN_DropMode);
			AssertEquals(template.K2_IsContainerRateable, instruction.KN_IsContainerRateable);
			AssertEquals(template.K2_IsLooseRateable, instruction.KN_IsLooseRateable);
		}

		public void TestKM_KT_NKBookingTemplate_CallsRefreshBindingAfterCreationOfInstructions()
		{
			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true);

			bool instructionsWereCreatedBeforeRefreshBinding = false;
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = consolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplateInfo.ValueChanged += (sender, e) => instructionsWereCreatedBeforeRefreshBinding = booking.Instructions.Count == 3;
			booking.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals("Should have created the Instructions from the new Template before Refresh Binding", true, instructionsWereCreatedBeforeRefreshBinding);
			AssertEquals("IFCL", booking.KM_KT_NKBookingTemplate);
			AssertEquals("FCL Import", booking.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, booking.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, booking.KM_RatingFreightMode);
			AssertInstructionAndTemplate(ctoInstructionTemplate, booking.Instructions[0]);
			AssertInstructionAndTemplate(cneInstructionTemplate, booking.Instructions[1]);
			AssertInstructionAndTemplate(cydInstructionTemplate, booking.Instructions[2]);
		}

		public void TestKM_KT_NKBookingTemplate_DoesNotCreateInstructionsIfSemaphoreIsSuspended()
		{
			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);

			var ctoInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false);
			var cneInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true);
			var cydInstructionTemplate = Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true);

			var booking = (DtbBooking)GetNewBusinessObject();
			using (new SemaphoreManager(booking.AddingInstructionsFromTemplateSemaphore))
			{
				booking.KM_KT_NKBookingTemplate = "IFCL";
			}

			AssertEquals("IFCL", booking.KM_KT_NKBookingTemplate);
			AssertEquals("FCL Import", booking.KM_Description);
			AssertEquals(Constants.CartageDirection.Import, booking.KM_Direction);
			AssertEquals(RatingFreightModes.Codes.Containerised, booking.KM_RatingFreightMode);
			AssertEquals(0, booking.Instructions.Count);
		}

		public void TestKM_KT_NKBookingTemplateChange_PopupMessageIfNoPackageCanBeAssignedForNewTemplate()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = consolidation.Bookings.AddNew();
			var notify = new TestNotify();
			booking.ConsolidationSingleJob.NotificationManager.Push(notify);
			AssertEquals(0, booking.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var packageJob = booking.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var templateLoose = CreateBookingTemplate("ILLL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			Helper.AddInstructionToTemplate(templateLoose, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Outers);
			Helper.AddInstructionToTemplate(templateLoose, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			booking.KM_KT_NKBookingTemplate = "ILLL";

			AssertEquals(3, booking.AssignedPackages.Count);

			var template = CreateBookingTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			booking.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(0, booking.AssignedPackages.Count);
			AssertNotNull("Warning message should be raised.", booking.ConsolidationSingleJob.NotificationSubscriber);
			Assert(notify.LastNotification.Message.Contains("Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab."));
		}

		public void TestKM_KT_NKBookingTemplateChange_DoNotPopupMessageIfNoPackageAttachedToConsolidationPackageJob()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var booking = consolidation.Bookings.AddNew();
			var notify = new TestNotify();
			booking.ConsolidationSingleJob.NotificationManager.Push(notify);
			AssertEquals(0, booking.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var template = CreateBookingTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			booking.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(0, booking.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);

			var templateIFCL = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			Helper.AddInstructionToTemplate(templateIFCL, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			booking.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals(0, booking.AssignedPackages.Count);
			AssertNull("No Warning message to be raised.", notify.LastNotification);
		}

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var booking = consolidation.Bookings.AddNew();
			AssertEquals(0, booking.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, booking.AssignedPackages.Count);

			Helper.CreatePackageDivot(picInstruction, package3, 1);
			Helper.CreatePackageDivot(dlvInstruction, package3, 1);
			AssertEquals(2, booking.AssignedPackages.Count);

			var template = CreateBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp, true, false, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, true, true, PackageCategories.Codes.Both);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Outers);
			booking.KM_KT_NKBookingTemplate = "IFCL";

			AssertEquals(2, booking.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package1, package3 }, booking.AssignedPackages);
		}

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages_ContainersToLoose()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var container1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0001");
			var container2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0002");

			var package1 = container1.Packages.AddNew();
			var package2 = container2.Packages.AddNew();
			package1.KP_PackageID = "PKG00002";
			package2.KP_PackageID = "PKG00003";

			var booking = consolidation.Bookings.AddNew();
			AssertEquals(0, booking.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container1, 1);
			Helper.CreatePackageDivot(dlvInstruction, container1, 1);
			AssertEquals(1, booking.AssignedPackages.Count);

			var template = CreateBookingTemplate("ILLL", "LCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Loose);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Loose);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Loose);
			booking.KM_KT_NKBookingTemplate = "ILLL";

			AssertEquals(1, booking.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package1 }, booking.AssignedPackages);
		}

		public void TestKM_KT_NKBookingTemplateChange_OnlyReAssignPreExistingPackages_LooseToContainer()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;

			var container1 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0001");
			var container2 = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CONT0002");

			var package1 = container1.Packages.AddNew();
			var package2 = container2.Packages.AddNew();
			package1.KP_PackageID = "PKG00002";
			package2.KP_PackageID = "PKG00003";

			var booking = consolidation.Bookings.AddNew();
			AssertEquals(0, booking.AssignedPackages.Count);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, booking.AssignedPackages.Count);

			var template = CreateBookingTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);
			booking.KM_KT_NKBookingTemplate = "IFFF";

			AssertEquals(1, booking.AssignedPackages.Count);
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { container1 }, booking.AssignedPackages);
		}

		// calculated

		public void TestStatusDescription()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(TransportStatuses.Descriptions.Available, transportJob.StatusDescription);

			transportJob.KM_Status = TransportStatuses.Codes.Held;
			AssertEquals(TransportStatuses.Descriptions.Held, transportJob.StatusDescription);

			transportJob.KM_Status = "xXx";
			AssertEquals("", transportJob.StatusDescription);
		}

		public void TransportBookingPartyReference()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals("", transportJob.TransportBookingPartyReference);

			var reference = transportJob.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryNum = "ABC";
			AssertEquals("", transportJob.TransportBookingPartyReference);

			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			AssertEquals("Use Additional Reference over own TB, we send events directly to sending tb.", "ABC", transportJob.TransportBookingPartyReference);
		}

		public void TestBookedByOrganisationPK()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals(ZGuid.Empty, booking.BookedByOrganisationPK);

			var org = Factory.New<OrgHeader>();
			booking.ConsolidationSingleJob.BookedByAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK, booking.BookedByOrganisationPK);
		}

		public void TestLocalClient()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals("Neither Billing Client nor ClientReqBillToParty exist, Local Client should be empty.", true, booking.LocalClient.IsEmpty);

			var bOrg = Factory.New<OrgHeader>();
			booking.ConsolidationSingleJob.BookedByAddress.OrganisationPK = bOrg.PK;
			Assert(booking.LocalClient.IsEmpty);

			var clientReqBillToPartyOrg = Helper.CreateOrganisation("CRBP");
			var billingParty = booking.BillingPartyAddress; // Poke it to create CRB
			var clientReqBillToPartyAddress = booking.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertEquals(false, clientReqBillToPartyAddress.Requirement.CanOverride);
			clientReqBillToPartyAddress.E2_OA_Address = clientReqBillToPartyOrg.MainAddress.PK;
			AssertEquals("No Billing Client exists, Local Client should fallback to the ClientReqBillToParty Org.", clientReqBillToPartyOrg.PK, booking.LocalClient);

			var billingOrg = Helper.CreateOrganisation("BILLC");
			var job = Helper.LoadOrCreateJobHeader(booking);
			job.LocalChargesAddr.OA_OH = billingOrg.PK;
			AssertEquals("Local Client should come from the Billing Job.", billingOrg.PK, booking.LocalClient);
		}

		public void TestBillingPartyOrLocalClientPK()
		{
			var org1 = Helper.CreateOrganisation("O1");
			var org2 = Helper.CreateOrganisation("O2");
			var booking = GetSaveableTransportJob();
			AssertEquals("Precondition", ZGuid.Empty, booking.BillingPartyAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, booking.BillingPartyOrLocalClientPK);

			booking.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);
			AssertEquals(org1.MainAddress.PK, booking.BillingPartyOrLocalClientPK);

			booking.BillingPartyOrLocalClientPK = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyOrLocalClientPK);

			Helper.LoadOrCreateJobHeader(booking);
			AssertEquals(org2.MainAddress.PK, booking.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);

			booking.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, booking.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org1.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);

			booking.BillingPartyOrLocalClientPK = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, booking.Job.JH_OA_LocalChargesAddr);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyOrLocalClientPK);
			AssertEquals(org2.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);
		}

		public void TestBillingPartyOrLocalClientAddress()
		{
			var org1 = Helper.CreateOrganisation("O1");
			var booking = GetSaveableTransportJob();
			AssertEquals("Precondition", ZGuid.Empty, booking.BillingPartyAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, booking.BillingPartyOrLocalClientPK);

			// Job is null
			booking.BillingPartyOrLocalClientPK = org1.MainAddress.PK;
			AssertEquals("Since job is null, BillingPartyOrLocalClientAddress should be equal to BillingPartyAddress.Address", booking.BillingPartyAddress.Address, booking.BillingPartyOrLocalClientAddress);

			// Create job
			var job = Helper.LoadOrCreateJobHeader(booking);
			job.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			AssertEquals("Since job is not null, BillingPartyOrLocalClientAddress should be equal to job.LocalChargesAddr", job.LocalChargesAddr, booking.BillingPartyOrLocalClientAddress);
		}

		public void TestBillingPartyOrLocalClientPK_ZAddress()
		{
			var booking = GetSaveableTransportJob();
			var zAddressForTransportWithoutJobHeader = booking.BillingPartyOrLocalClientPK_ZAddress;
			Assert(zAddressForTransportWithoutJobHeader.IsOrgVisible);
			AssertEquals(AddressType.ARM, zAddressForTransportWithoutJobHeader.DefaultAddressType);
		}

		public void TestBillingPartyOrLocalClientPKInfo()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals(booking.BillingPartyAddress.E2_OA_AddressInfo, ((ZWrappedPropertyInfo)booking.BillingPartyOrLocalClientPKInfo).InnerInfo);

			var job = Helper.LoadOrCreateJobHeader(booking);
			AssertEquals(booking.Job.JH_OA_LocalChargesAddrInfo, ((ZWrappedPropertyInfo)booking.BillingPartyOrLocalClientPKInfo).InnerInfo);
		}

		// rating

		public void TestGetTotalLoosePackageQuantity()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0, transportJob.GetTotalLoosePackageQuantity());

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(30, transportJob.GetTotalLoosePackageQuantity());

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(30, transportJob.GetTotalLoosePackageQuantity());
		}

		public void TestGetTotalLoosePackageUnit()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals("", transportJob.GetTotalLoosePackageUnit());

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package1 = Helper.CreatePackage("", 1, Constants.PkgUnit.Keg);
			var package2 = Helper.CreatePackage("", 1, Constants.PkgUnit.Keg);

			Helper.CreatePackageDivot(frmInstruction, package1, 1);
			Helper.CreatePackageDivot(frmInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package2, 1);
			Helper.CreatePackageDivot(dlvInstruction, package2, 1);

			AssertEquals(Constants.PkgUnit.Keg, transportJob.GetTotalLoosePackageUnit());

			var package30 = Helper.CreatePackage("", 1, Constants.PkgUnit.Pallet);
			Helper.CreatePackageDivot(frmInstruction, package30, 1);
			AssertEquals(Constants.PkgUnit.Package, transportJob.GetTotalLoosePackageUnit());
		}

		public void TestGetTotalLoosePackageWeight()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_WeightUQ = Constants.Weight.Kilograms;
			package20.KP_WeightUQ = Constants.Weight.Pounds;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(20.52544m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(20.52544m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);
		}

		public void TestGetTotalLoosePackageWeightWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalLoosePackageWeight().Unit);

			CreatePackageDivots(transportJob, false);

			AssertNoExceptionThrown("All weights used in calculation of TotalLoosePackageWeight should be valid", () => { var weight = transportJob.GetTotalLoosePackageWeight(); });
			AssertEquals(new ZWeight(50, "KG"), transportJob.GetTotalLoosePackageWeight());
		}

		public void TestGetTotalLoosePackageVolume()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			var package10 = Helper.CreatePackage("", null, 10, 11, 12);
			var package20 = Helper.CreatePackage("", null, 20, 21, 22);
			var package30 = Helper.CreatePackage("", null, 30, 31, 32);

			package10.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package20.KP_VolumeUQ = Constants.Volume.CubicFeet;

			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(frmInstruction, package10, 10);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);
			Helper.CreatePackageDivot(dlvInstruction, package20, 20);

			AssertEquals(12.622971m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			AssertEquals(12.622971m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);
		}

		public void TestGetTotalLoosePackageVolumeWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalLoosePackageVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalLoosePackageVolume().Unit);

			CreatePackageDivots(transportJob, false);

			AssertNoExceptionThrown("All volumes used in calculation of TotalLoosePackageVolume should be valid", () => transportJob.GetTotalLoosePackageVolume());
			AssertEquals(new ZVolume(100, "M3"), transportJob.GetTotalLoosePackageVolume());
		}

		public void TestGetTotalContainerisedWeight()
		{
			var transportJob = GetSaveableTransportJob();
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);
			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			var cont2 = Helper.CreatePackageContainer("CONT2", null, 1, 50, 20, "40GP");

			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			Helper.CreatePackageDivot(frmInstruction, cont2, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont1, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont2, 1);

			AssertEquals(90m, transportJob.GetTotalContainerisedWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalContainerisedWeight().Unit);
		}

		public void TestGetTotalContainerisedWeightWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalContainerisedWeight().Amount);
			AssertEquals(Constants.Weight.Kilograms, transportJob.GetTotalContainerisedWeight().Unit);

			CreatePackageDivots(transportJob, true);

			AssertNoExceptionThrown("All weights used in calculation of TotalContainerisedWeight should be valid", () => transportJob.GetTotalContainerisedWeight());
			AssertEquals(new ZWeight(50, "KG"), transportJob.GetTotalContainerisedWeight());
		}

		public void TestGetTotalContainerisedVolume()
		{
			var transportJob = GetSaveableTransportJob();
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);
			var cont1 = Helper.CreatePackageContainer("CONT1", null, 1, 40);
			var cont2 = Helper.CreatePackageContainer("CONT2", null, 1, 50, 20, "40GP");

			Helper.CreatePackageDivot(frmInstruction, cont1, 1);
			Helper.CreatePackageDivot(frmInstruction, cont2, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont1, 1);
			Helper.CreatePackageDivot(dlvInstruction, cont2, 1);

			AssertEquals(20m, transportJob.GetTotalContainerisedVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalContainerisedVolume().Unit);
		}

		public void TestGetTotalContainerisedVolumeWithInvalidUnits()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(0m, transportJob.GetTotalContainerisedVolume().Amount);
			AssertEquals(Constants.Volume.CubicMetres, transportJob.GetTotalContainerisedVolume().Unit);

			CreatePackageDivots(transportJob, true);

			AssertNoExceptionThrown("All volumes used in calculation of TotalContainerisedVolume should be valid", () => transportJob.GetTotalContainerisedVolume());
			AssertEquals(new ZVolume(100, "M3"), transportJob.GetTotalContainerisedVolume());
		}

		void CreatePackageDivots(DtbBooking transportJob, bool containerised)
		{
			var frmInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.Delivery);

			List<PkgPackage> packages = CreatePackagesWithInvalid(containerised);

			foreach (PkgPackage p in packages)
			{
				Helper.CreatePackageDivot(frmInstruction, p, 1);
				Helper.CreatePackageDivot(dlvInstruction, p, 1);
			}
		}

		List<PkgPackage> CreatePackagesWithInvalid(bool containerised)
		{
			var packages = new List<PkgPackage>();
			if (containerised)
			{
				packages.Add(MakeInvalid(Helper.CreatePackageContainer("ContainerPackage1 (Invalid)", null, 1, 100, 200)));
				packages.Add(Helper.CreatePackageContainer("ContainerPackage2", null, 1, 50, 100));
			}
			else
			{
				packages.Add(MakeInvalid(Helper.CreatePackage("Package1 (Invalid)", null, 1, 100, 200)));
				packages.Add(Helper.CreatePackage("Package2", null, 1, 50, 100));
			}
			return packages;
		}

		PkgPackage MakeInvalid(PkgPackage package)
		{
			package.KP_VolumeUQ = Constants.Weight.MetricCarat;  //'MC'
			package.KP_WeightUQ = Constants.Volume.CubicMetres;  //'M3'

			return package;
		}

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<DtbBooking>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		// status

		public void TestIsDelivered()
		{
			AssertFlag("IsDelivered", DtbBookingSchema.KM_Status, TransportStatuses.Codes.Delivered, TransportStatuses.Codes.Available);
		}

		// direction

		public void TestIsPickupDirection()
		{
			AssertFlag("IsPickupDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Origin, Constants.CartageDirection.Destination);
			AssertFlag("IsPickupDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Export, Constants.CartageDirection.Import);
		}

		public void TestIsDeliveryDirection()
		{
			AssertFlag("IsDeliveryDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Destination, Constants.CartageDirection.Origin);
			AssertFlag("IsDeliveryDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Import, Constants.CartageDirection.Export);
		}

		public void TestIsLocalDirection()
		{
			AssertFlag("IsLocalDirection", DtbBookingSchema.KM_Direction, Constants.CartageDirection.Local, Constants.CartageDirection.LineHaul);
		}

		public void TestUpdateIsHazardous()
		{
			var booking = GetSaveableTransportJob();

			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);

			var containerInnerPackageUNDG = containerInnerPackage.UNDGs.AddNew();

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			CombineAssertions("Precondition: Checks for test setup", () =>
			{
				AssertEquals("Precondition: booking has 1 assigned package", 1, booking.AssignedPackages.Count);
				AssertEquals("Precondition: booking.IsAnyPackageHazardous is true", true, booking.IsAnyPackageHazardous);
				AssertEquals("Precondition: booking.KM_IsHazardous is still false", false, booking.KM_IsHazardous);
			});

			booking.UpdateIsHazardous();
			AssertEquals("booking.KM_IsHazardous should now be true", true, booking.KM_IsHazardous);

			containerInnerPackageUNDG.Delete();
			CombineAssertions("Precondition: Checks for Act#2 test setup", () =>
			{
				Assert("Precondition: containerInnerPackage should now not have any UNDGs", !containerInnerPackage.UNDGs.Any());
				AssertEquals("Precondition: booking.IsAnyPackageHazardous is now false", false, booking.IsAnyPackageHazardous);
				AssertEquals("Precondition: booking.KM_IsHazardous should still be true", true, booking.KM_IsHazardous);
			});

			booking.UpdateIsHazardous();
			AssertEquals("booking.KM_IsHazardous should now be re-set to false", false, booking.KM_IsHazardous);
		}

		public void TestIsAnyPackageHazardous()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals(false, booking.IsAnyPackageHazardous);

			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);
			AssertEquals(false, booking.IsAnyPackageHazardous);

			containerInnerPackage.UNDGs.AddNew();
			AssertEquals("Package is not assigned yet.", false, booking.IsAnyPackageHazardous);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			AssertEquals("Assigned Package.", 1, booking.AssignedPackages.Count);

			AssertEquals(true, booking.IsAnyPackageHazardous);
		}

		public void TestIsAnyPackageRequiresRefrigeration()
		{
			var booking = GetSaveableTransportJob();
			AssertEquals("Precondition: RequiresRefridgeration should be false", false, booking.IsAnyPackageRequiresRefridgeration);

			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);
			AssertEquals("RequiresRefridgeration still false.", false, booking.IsAnyPackageRequiresRefridgeration);

			containerInnerPackage.KP_RequiresTemperatureControl = true;
			AssertEquals("Package is not assigned yet, RequiresRefridgeration still false", false, booking.IsAnyPackageRequiresRefridgeration);

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			AssertEquals("Assigned Package.", 1, booking.AssignedPackages.Count);

			AssertEquals("Package now assigned, RequiresRefridgeration should be true.", true, booking.IsAnyPackageRequiresRefridgeration);
		}

		public void TestUpdateRequiresRefrigeration()
		{
			var booking = GetSaveableTransportJob();

			var packageJob = booking.PackageJob;
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			packageJob.Packages.Add(container);
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			container.Packages.Add(containerInnerPackage);

			containerInnerPackage.KP_RequiresTemperatureControl = true;

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, container, 1);
			Helper.CreatePackageDivot(dlvInstruction, container, 1);
			CombineAssertions("Precondition: Checks for test setup", () =>
			{
				AssertEquals("Precondition: booking has 1 assigned package", 1, booking.AssignedPackages.Count);
				AssertEquals("Precondition: booking.IsAnyPackageRequiresRefrigeration is true", true, booking.IsAnyPackageRequiresRefridgeration);
				AssertEquals("Precondition: booking.KM_RequiresRefrigeration is still false", false, booking.KM_RequiresRefrigeration);
			});

			booking.UpdateRequiresRefrigeration();
			AssertEquals("booking.KM_RequiresRefrigeration should now be true", true, booking.KM_RequiresRefrigeration);

			containerInnerPackage.KP_RequiresTemperatureControl = false;
			CombineAssertions("Precondition: Checks for Act#2 test setup", () =>
			{
				AssertEquals("Precondition: booking.KM_RequiresRefrigeration should still be true", true, booking.KM_RequiresRefrigeration);
				AssertEquals("Precondition: booking.IsAnyPackageRequiresRefrigeration is now false", false, booking.IsAnyPackageRequiresRefridgeration);
			});

			booking.UpdateRequiresRefrigeration();
			AssertEquals("booking.KM_RequiresRefrigeration should now be re-set to false", false, booking.KM_RequiresRefrigeration);
		}

		void AssertFlag(ZString flagPropertyName, SchemaColumn codeColumn, ZString validCode, ZString invalidCode)
		{
			var booking = (DtbBooking)GetNewBusinessObject();

			booking[codeColumn.Name] = "";
			AssertEquals(false, booking[flagPropertyName]);

			booking[codeColumn.Name] = validCode;
			AssertEquals(true, booking[flagPropertyName]);

			booking[codeColumn.Name] = invalidCode;
			AssertEquals(false, booking[flagPropertyName]);
		}

		// rating

		public void TestIsContainerisedOnly()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsContainerisedOnly);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerisedOnly);

			var package = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(false, transportJob.IsContainerisedOnly);
		}

		public void TestIsLooseOnly()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsLooseOnly);

			var package = Factory.New<PkgPackage>();
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(true, transportJob.IsLooseOnly);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsLooseOnly);
		}

		public void TestIsContainerised()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsContainerised);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerised);

			var package = Factory.New<PkgPackage>();
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsContainerised);
		}

		public void TestIsLoose()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsLoose);

			var package = Factory.New<PkgPackage>();
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, package, 1);
			AssertEquals(true, transportJob.IsLoose);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsLoose);
		}

		public void TestIsFTL()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFTL);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsFTL);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL.", true, transportJob.IsFTL);

			container.Container.K0_ContainerMode = "";
			AssertEquals(false, transportJob.IsFTL);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(false, transportJob.IsFTL);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road.", true, transportJob.IsFTL);
		}

		public void TestIsFCL()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFCL);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(true, transportJob.IsFCL);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL so is not FCL", false, transportJob.IsFCL);

			container.Container.K0_ContainerMode = "";
			AssertEquals(true, transportJob.IsFCL);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(true, transportJob.IsFCL);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road so is not FCL", false, transportJob.IsFCL);
		}

		public void TestIsPort()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsPort);

			var cto = transportJob.Instructions.AddNew();
			cto.OrganisationType = "CTO";
			AssertEquals(true, transportJob.IsPort);

			cto.OrganisationType = "CNR";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CNE";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CFS";
			AssertEquals(false, transportJob.IsPort);

			cto.OrganisationType = "CYD";
			AssertEquals(true, transportJob.IsPort);

			cto.OrganisationType = "WHS";
			AssertEquals(false, transportJob.IsPort);
		}

		public void TestIsFCLPort()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsFCLPort);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals(false, transportJob.IsFCLPort);

			var cto = transportJob.Instructions.AddNew();
			cto.OrganisationType = "CTO";
			AssertEquals(true, transportJob.IsFCLPort);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has Container marked as FTL so is not FCL", false, transportJob.IsFCLPort);

			container.Container.K0_ContainerMode = "";
			AssertEquals(true, transportJob.IsFCLPort);

			var roadContainerType = Factory.New<RefContainer>();
			roadContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Road;
			var seaContainerType = Factory.New<RefContainer>();
			seaContainerType.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			AssertEquals(true, transportJob.IsFCLPort);

			container.Container.K0_RC_ContainerType = roadContainerType.PK;
			AssertEquals("Has container type that is of type Road so is not FCL", false, transportJob.IsFCLPort);

			container.Container.K0_RC_ContainerType = seaContainerType.PK;
			cto.OrganisationType = "CFS";
			AssertEquals(false, transportJob.IsFCLPort);

			cto.OrganisationType = "CYD";
			AssertEquals(true, transportJob.IsFCLPort);
		}

		public void TestIsMixedCargo()
		{
			var transportJob = GetSaveableTransportJob();
			AssertEquals(false, transportJob.IsMixedCargo);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			var instruction = Helper.CreateInstruction(transportJob);
			Helper.CreatePackageDivot(instruction, container, 1);
			AssertEquals("Only has one container, cannot be Mixed", false, transportJob.IsMixedCargo);

			var container2 = Factory.New<PkgPackage>();
			container2.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container2.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			AssertEquals("Has two containers marked as FTL so is not Mixed", false, transportJob.IsMixedCargo);

			container2.Container.K0_ContainerMode = Constants.ContainerModes.Bulk;
			container.Container.K0_ContainerMode = Constants.ContainerModes.Bulk;
			AssertEquals("Has two containers marked as Non-FCL so is not Mixed", false, transportJob.IsMixedCargo);

			container2.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			Helper.CreatePackageDivot(instruction, container2, 1);
			AssertEquals("Has one FTL container and one Non-FCL container, so it is Mixed", true, transportJob.IsMixedCargo);

			container2.KP_F3_NKPackType = Constants.PkgUnit.Package;
			AssertEquals("Has one container and one loose, so it is Mixed", true, transportJob.IsMixedCargo);

			container.KP_F3_NKPackType = Constants.PkgUnit.Package;
			AssertEquals("Has two loose, so it is not Mixed", false, transportJob.IsMixedCargo);
		}

		public void TestUpdateStatus()
		{
			var booking = Helper.CreateBooking();

			// booking with no instructions

			booking.KM_Status = "";
			AssertEquals("Precondition", "", booking.KM_Status);

			booking.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Available, booking.KM_Status);

			// booking with pickup instructions

			var pickUpInstruction1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var pickUpInstruction2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			booking.UpdateStatus();
			AssertEquals("PickUp Instructions exists but are not PickedUp, Status should fallback to Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			pickUpInstruction1.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("PickUp Instructions exists but not all are PickedUp, Status should be Available.", TransportStatuses.Codes.Available, booking.KM_Status);

			booking.KM_Status = TransportStatuses.Codes.PickUpConfirmed;
			booking.UpdateStatus();
			AssertEquals("Booking is Pick Up Confirmed so it should not change.", TransportStatuses.Codes.PickUpConfirmed, booking.KM_Status);

			booking.KM_Status = TransportStatuses.Codes.PickUpCommenced;
			booking.UpdateStatus();
			AssertEquals("Booking is Pick Up Commenced so it should not change.", TransportStatuses.Codes.PickUpCommenced, booking.KM_Status);

			booking.KM_Status = TransportStatuses.Codes.Available;
			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Now, isEstimate: false);
			booking.UpdateStatus();
			AssertEquals("Service Commenced log added, status should be Service Commenced.", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);
			booking.UpdateStatus();
			AssertEquals("Status should not change when no changes are made to a TB, status should still be service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_Direction = "ORG";
			landTransportConsignment.LTC_Status = "BKD";
			landTransportConsignment.LTC_KM_Booking = booking.PK;
			landTransportConsignment.LTC_IsActive = false;
			booking.UpdateStatus();
			AssertEquals("Inactive transport job added to a service commenced TB, but TB is service commenced for a reason other than having a transport job (ServiceCommenced Log), status should still be service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			booking.Logs.RemoveAndDeleteAll();
			var carrierBookingAgent = Factory.New<OrgHeader>();
			var commMode = carrierBookingAgent.EDICommunicationsModes.AddNew();
			commMode.EK_Module = RelatableActivityTypeList.Codes.TransportBooking;
			commMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			commMode.EK_Destination = DtbAgentBooking.ContainerTransportOptimizationCBA;
			booking.CarrierBookingAgentDocAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;
			booking.UpdateStatus();
			AssertEquals("Inactive transport job added to a service commenced TB, but TB is service commenced for a reason other than having a transport job (Booking is being managed by authorised CarrierBookingAgent), status should still be service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			booking.CarrierBookingAgentDocAddress.E2_OA_Address = ZGuid.Empty;
			booking.UpdateStatus();
			AssertEquals("Inactive transport job added to a service commenced TB and no other reason for the TB being service commenced, status should be available", TransportStatuses.Codes.Available, booking.KM_Status);

			landTransportConsignment.LTC_IsActive = true;
			booking.UpdateStatus();
			AssertEquals("Transport job reactivated on an active TB, status should be service commenced", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			pickUpInstruction2.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("PickUp Instructions exists and all are PickedUp, Status should be PickUp.", TransportStatuses.Codes.PickedUp, booking.KM_Status);

			// booking with pickup and delivery instructions

			var deliveryConfirmation1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var deliveryConfirmation2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			booking.UpdateStatus();
			AssertEquals("Delivery Instructions exist but are not Delivered, Status should fallback to PickUp.", TransportStatuses.Codes.PickedUp, booking.KM_Status);

			deliveryConfirmation1.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals("Delivery Instructions exists but not all are Delivered, Status should fallback to PickUp.", TransportStatuses.Codes.PickedUp, booking.KM_Status);

			deliveryConfirmation2.KN_Status = TransportStatuses.Codes.Delivered;
			booking.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Delivered, booking.KM_Status);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			booking.UpdateStatus();
			AssertEquals("Action Required log added, status should be Action Required.", TransportStatuses.Codes.ActionRequired, booking.KM_Status);

			booking.KM_Status = TransportStatuses.Codes.Quote;
			booking.UpdateStatus();
			AssertEquals("Should remain as a Quote.", TransportStatuses.Codes.Quote, booking.KM_Status);
		}

		public void TestSuspendSettingPackages()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(false, booking.IsSettingPackagesSuspended);

			using (booking.SuspendSettingPackages())
			{
				AssertEquals(true, booking.IsSettingPackagesSuspended);
			}

			AssertEquals(false, booking.IsSettingPackagesSuspended);
		}

		public void TestSave_PopulateUnqiueIDIfNeeded()
		{
			var consolidation = (DtbBookingConsolidation)Factory.New(ExpectedConsolidationType);
			var booking = consolidation.Bookings.AddNew();
			AssertEquals("Precondition", "", booking.KM_JobID);

			Factory.Save();
			AssertEquals("TB00000001", booking.KM_JobID);
		}

		public void TestUniversalCopyAttributes()
		{
			var booking = Factory.New<DtbBooking>();
			var componentType = booking.GetType();
			var km_kb_bookingInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == DtbBookingSchema.Constants.KM_KB_Booking);
			AssertEquals("KM_KB_Booking should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, km_kb_bookingInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));
		}

		public void TestDocAddresses_WhenBookingIsSub()
		{
			var subBooking = Factory.NewWithValidTestData<DtbBooking>();
			var masterBooking = Factory.NewWithValidTestData<DtbBooking>();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			var masterAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var masterAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			masterAddress1.E2_ParentID = masterBooking.PK;
			masterAddress2.E2_ParentID = masterBooking.PK;
			masterAddress1.E2_ParentTableCode = masterBooking.TablePrefix;
			masterAddress2.E2_ParentTableCode = masterBooking.TablePrefix;

			var subAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var subAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			subAddress1.E2_ParentID = subBooking.PK;
			subAddress2.E2_ParentID = subBooking.PK;
			subAddress1.E2_ParentTableCode = subBooking.TablePrefix;
			subAddress2.E2_ParentTableCode = subBooking.TablePrefix;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Sub Booking DocAddresses should only contain addresses attached to the Master.", new ZGuid[] { masterAddress1.PK, masterAddress2.PK }, subBooking.DocAddresses.Select(a => a.PK));
		}

		public void TestDocAddresses_WhenBookingIsNotSub()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			var address1 = Factory.NewWithValidTestData<JobDocAddress>();
			var address2 = Factory.NewWithValidTestData<JobDocAddress>();

			address1.E2_ParentID = booking.PK;
			address2.E2_ParentID = booking.PK;
			address1.E2_ParentTableCode = booking.TablePrefix;
			address2.E2_ParentTableCode = booking.TablePrefix;

			Factory.Save();

			AssertEquals("Precondition: Booking is not a sub booking.", false, booking.IsSub);

			AssertEquals(typeof(JobDocAddressDependentCollection), booking.DocAddresses.GetType());
			AssertContainsExactElementsInAnyOrder("DocAddresses should contain addresses attached to the Booking itself.", new ZGuid[] { address1.PK, address2.PK }, booking.DocAddresses.Select(a => a.PK));
		}

		public void TestDelete_ShouldNotDeleteFromDb_WhenSubBookingExist()
		{
			var subBooking = Factory.NewWithValidTestData<DtbBooking>();
			var masterBooking = Factory.NewWithValidTestData<DtbBooking>();
			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			masterBooking.Delete();

			AssertEquals(false, masterBooking.IsDeleted);
		}

		public void TestDocAddresses_WhenMasterBookingChanges()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();

			var bookingAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			var bookingAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			bookingAddress1.E2_ParentID = booking.PK;
			bookingAddress2.E2_ParentID = booking.PK;
			bookingAddress1.E2_ParentTableCode = booking.TablePrefix;
			bookingAddress2.E2_ParentTableCode = booking.TablePrefix;

			Factory.Save();

			AssertEquals("Precondition: KM_KM_MasterBooking should be an empty Guid.", ZGuid.Empty, booking.KM_KM_MasterBooking);
			AssertContainsExactElementsInAnyOrder("DocAddresses should contain addresses attached to the Booking itself.", new ZGuid[] { bookingAddress1.PK, bookingAddress2.PK }, booking.DocAddresses.Select(a => a.PK));

			var masterBooking1 = Factory.NewWithValidTestData<DtbBooking>();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking1.PK;
			booking.KM_MasterBookingVersion = 1;

			var master1Address1 = Factory.NewWithValidTestData<JobDocAddress>();
			var master1Address2 = Factory.NewWithValidTestData<JobDocAddress>();
			master1Address1.E2_ParentID = masterBooking1.PK;
			master1Address2.E2_ParentID = masterBooking1.PK;
			master1Address1.E2_ParentTableCode = masterBooking1.TablePrefix;
			master1Address2.E2_ParentTableCode = masterBooking1.TablePrefix;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Booking DocAddresses should only contain addresses attached to its Master.", new ZGuid[] { master1Address1.PK, master1Address2.PK }, booking.DocAddresses.Select(a => a.PK));

			var masterBooking2 = Factory.NewWithValidTestData<DtbBooking>();
			masterBooking2.KM_IsMaster = true;
			masterBooking2.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking2.PK;

			var master2Address1 = Factory.NewWithValidTestData<JobDocAddress>();
			var master2Address2 = Factory.NewWithValidTestData<JobDocAddress>();
			master2Address1.E2_ParentID = masterBooking2.PK;
			master2Address2.E2_ParentID = masterBooking2.PK;
			master2Address1.E2_ParentTableCode = masterBooking2.TablePrefix;
			master2Address2.E2_ParentTableCode = masterBooking2.TablePrefix;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Booking DocAddresses should only contain addresses attached to its Master.", new ZGuid[] { master2Address1.PK, master2Address2.PK }, booking.DocAddresses.Select(a => a.PK));
		}

		public void TestIDocAddresses_CanDeleteAddress()
		{
			IDocAddresses booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(false, booking.CanDeleteAddress(null));
		}

		public void TestIDocAddresses_DocAddresses()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(typeof(JobDocAddressDependentCollection), booking.DocAddresses.GetType());
			AssertEquals(true, booking.IsRegisteredEditableChildObject(booking.DocAddresses));
		}

		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			IDocAddresses booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(Env.Security.TransportJobMISCDetails, booking.GetCanOverrideCheckpoint(null));
		}

		public void TestIDocAddresses_GetDocAddressRequirement()
		{
			var forwardingShipment = Helper.CreateForwardingShipment();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingShipment);
			IDocAddresses booking = Helper.CreateBooking();
			var requirementForTransportCompany = booking.GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress);

			var requirementForCRB = booking.GetDocAddressRequirement(DocAddressType.ClientRequestedBillingParty);

			var requirementForSLA = booking.GetDocAddressRequirement(DocAddressType.ShippingLineAddress);

			CombineAssertions("Check IDocAddresses.GetDocAddressRequirement() for booking", () =>
			{
				AssertNotNull("Should have a requirement for Transport Company address", requirementForTransportCompany);
				if (requirementForTransportCompany != null)
				{
					AssertEquals("Requirement for Transport Company address should have address type OFC", AddressType.OFC, requirementForTransportCompany.DefaultAddressType);
					AssertEquals("Requirement for Transport Company address should have Contact Type LocalTransport", ContactType.LocalTransport, requirementForTransportCompany.DefaultContactType);
					AssertEquals("Requirement for Transport Company address should have Doc Address Type TransportCompanyDocumentaryAddress", DocAddressType.TransportCompanyDocumentaryAddress, requirementForTransportCompany.DefaultDocAddressType);
					AssertEquals("Requirement for Transport Company address should have SaveEvenIfBlank true", true, requirementForTransportCompany.SaveEvenIfBlank);
				}

				AssertNotNull("Should have a requirement for Client Requested Booking address", requirementForCRB);
				if (requirementForCRB != null)
				{
					AssertEquals("Requirement for Client Requested Booking address should not be mandatory", false, requirementForCRB.IsMandatory);
				}

				AssertNotNull("Should have a requirement for Shipping Line address", requirementForSLA);
				if (requirementForSLA != null)
				{
					AssertEquals("Requirement for Shipping Line address should not be able to be overridden", false, requirementForSLA.CanOverride);
					AssertEquals("Requirement for Shipping Line address should have maximum number of 1", 1, requirementForSLA.DefaultMax);
				}
			});
		}

		public void TestIDocAddresses_GetOrgHeaderList()
		{
			IDocAddresses booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(typeof(DebtorCollection), booking.GetOrgHeaderList(DocAddressType.ClientRequestedBillingParty).GetType());
			IDocAddresses iBooking = Helper.CreateBooking();
			AssertEquals(typeof(LocalTransportCollection), iBooking.GetOrgHeaderList(DocAddressType.TransportCompanyDocumentaryAddress).GetType());
		}

		public void TestIDocAddresses_SupportedAddressTypes_StandaloneBooking()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			IDocAddresses_SupportedAddressTypes_CoreTest(booking, expectShippingLineAddress: true);
		}

		public void TestIDocAddresses_SupportedAddressTypes_BookingWithForwardingConsolParent()
		{
			var forwardingShipment = Helper.CreateForwardingShipment();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingShipment);
			var booking = Helper.CreateBooking(bookingConsolidation);

			IDocAddresses_SupportedAddressTypes_CoreTest(booking, expectShippingLineAddress: true);
		}

		public void TestIDocAddresses_SupportedAddressTypes_BookingWithForwardingShipmentParent()
		{
			var forwardingShipment = Helper.CreateForwardingShipment();
			var forwardingConsolidation = Helper.CreateForwardingConsol(forwardingShipment, "C0004321", "SEA", "MB99876543");
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsolidation);
			var booking = Helper.CreateBooking(bookingConsolidation);

			IDocAddresses_SupportedAddressTypes_CoreTest(booking, expectShippingLineAddress: true);
		}

		public void TestIDocAddresses_SupportedAddressTypes_BookingWithNonForwardingShipmentOrConsolParent()
		{
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)whsOrder);
			var booking = Helper.CreateBooking(bookingConsolidation);

			IDocAddresses_SupportedAddressTypes_CoreTest(booking, expectShippingLineAddress: false);
		}

		void IDocAddresses_SupportedAddressTypes_CoreTest(DtbBooking booking, bool expectShippingLineAddress)
		{
			AssertEquals("Precondition: Booking should not be master.", false, booking.KM_IsMaster);

			AssertContainsExactElementsInAnyOrder("IDocAddresses.SupportedAddressTypes for non-master booking are not correct", GetExpectedSupportedDocAddressTypes(expectShippingLineAddress, bookingIsMaster: false), ((IDocAddresses)booking).SupportedAddressTypes);

			booking.KM_IsMaster = true;

			AssertContainsExactElementsInAnyOrder("IDocAddresses.SupportedAddressTypes for master booking are not correct", GetExpectedSupportedDocAddressTypes(expectShippingLineAddress, bookingIsMaster: true), ((IDocAddresses)booking).SupportedAddressTypes);
		}

		DocAddressType[] GetExpectedSupportedDocAddressTypes(bool expectShippingLineAddress, bool bookingIsMaster)
		{
			var result = new List<DocAddressType>
			{
				DocAddressType.NotifyParty,
				DocAddressType.FinalConsigneeAddress,
				DocAddressType.OriginatingConsignorAddress,
				DocAddressType.TransportCompanyDocumentaryAddress,
				DocAddressType.CarrierBookingAgent
			};

			if (expectShippingLineAddress)
			{
				result.Add(DocAddressType.ShippingLineAddress);
			}

			if (!bookingIsMaster)
			{
				result.Add(DocAddressType.ClientRequestedBillingParty);
			}

			return result.ToArray();
		}

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var booking = Factory.New<DtbBooking>();
			var iConsignmentDocManager = (IDocManagerSupport)booking;
			AssertNotNull(iConsignmentDocManager.DocManagerInfo);
			AssertEquals(ExpectedDocManagerInfoType, iConsignmentDocManager.DocManagerInfo.GetType());
			AssertEquals(ExpectedDocManagerCode, iConsignmentDocManager.DocManagerInfo.DocManagerCode);
		}

		Type ExpectedDocManagerInfoType
		{
			get { return typeof(DtbBookingDocManagerInfo); }
		}

		string ExpectedDocManagerCode
		{
			get { return Constants.DocManagerCodes.DomesticTransportBooking; }
		}

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var booking = Factory.New<DtbBooking>();
			var iConsignmentDocumentSupportable = (IDocumentSupportable)booking;
			AssertNotNull(iConsignmentDocumentSupportable.DocumentSupporter);
			AssertEquals(ExpectedDocumentSupporterType, iConsignmentDocumentSupportable.DocumentSupporter.GetType());
		}

		Type ExpectedDocumentSupporterType
		{
			get { return typeof(DtbBookingDocumentSupporter); }
		}

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var booking = Factory.New<DtbBooking>();
			var iConsignmentEDocsProvider = (IEDocsProvider)booking;
			AssertNotNull(iConsignmentEDocsProvider.GetEDocsProviderSupporter());
			AssertEquals(typeof(JobInvoicingEDocsProviderSupporter), iConsignmentEDocsProvider.GetEDocsProviderSupporter().GetType());
		}

		public void TestIJobInvoicingPlugIn()
		{
			var booking = GetSaveableTransportJob();
			IJobInvoicingPlugIn invoicingPlugIn = booking;
			AssertEquals(ExpectedInvoicingSupporterType, invoicingPlugIn.InvoicingSupporter.GetType());
		}

		Type ExpectedInvoicingSupporterType
		{
			get { return typeof(DtbBookingInvoicingSupporter); }
		}

		public void TestIJobHeaderParent()
		{
			IJobHeaderParent parent = GetSaveableTransportJob();
			AssertEquals(true, parent.AllowInvoiceDeletion);
			AssertEquals("", parent.JobNumber);
			AssertNoExceptionThrown(() => parent.OnJobCreating(null));
			AssertNoExceptionThrown(() => parent.OnJobDeleting(null));

			parent.SetJobNumberFieldOnSaving();
			AssertNotEquals("", parent.JobNumber);
		}

		public void JobCreatedEventHandler()
		{
			var transportJob = GetSaveableTransportJob();
			bool isJobCreatedEventCalled = false;
			transportJob.JobCreated += delegate
			{ isJobCreatedEventCalled = true; };
			AssertEquals("Precondition", false, isJobCreatedEventCalled);

			((IJobHeaderParent)transportJob).OnJobCreated(null);
			AssertEquals(true, isJobCreatedEventCalled);
		}

		public void TestIJobHeaderParentCore()
		{
			var booking = GetSaveableTransportJob();
			var parentCore = booking as IJobHeaderParentCore;
			AssertEquals(booking.PK, parentCore.PK);
			AssertEquals(booking.TableName, parentCore.TableName);
			AssertEquals(false, parentCore.IsInDatabase);
			AssertEquals(booking.Factory, parentCore.Factory);

			Factory.Save();
			AssertEquals(true, parentCore.IsInDatabase);
		}

		public void TestIJobNumber()
		{
			var booking = GetSaveableTransportJob();
			var jobNumber = booking as IJobNumber;
			Factory.Save();
			AssertEquals(false, booking.KM_JobID.IsEmpty);
			AssertEquals(booking.KM_JobID, jobNumber.JobNumber);
		}

		DtbBooking GetSaveableTransportJob()
		{
			return Helper.CreateBooking();
		}

		public void TestGetAddress()
		{
			var booking = GetSaveableTransportJob();
			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			SetUpAddress(organisation, booking);
			var address = booking.GetAddress(DocAddressType);

			AssertEquals(officeAddress.PK, address.E2_OA_Address);
		}

		DocAddressType DocAddressType { get { return DocAddressType.TransportCompanyDocumentaryAddress; } }

		void SetUpAddress(OrgHeader organization, DtbBooking booking)
		{
			booking.Address.OrganisationPK = organization.PK;
		}

		DtbBookingTmpl CreateBookingTemplate(ZString code, ZString description, ZString direction, string ratingFreightMode = "")
		{
			var template = Factory.New<DtbBookingTmpl>();
			template.KT_Code = code;
			template.KT_Description = description;
			template.KT_Direction = direction;
			template.KT_RatingFreightMode = ratingFreightMode;
			return template;
		}

		class TestNotify : INotifications
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
				if (notification != null)
				{
					LastNotification = notification;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public INotification LastNotification { get; private set; }
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var booking = GetSaveableTransportJob();
			var job = Helper.LoadOrCreateJobHeader(booking);
			Factory.Save();

			booking.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("booking {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating booking, IsCancelled flag should be set to true", booking.IsCancelled);
			Assert("Deactivating booking, IsCancelledInfo should have changes", booking.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, booking.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		public void TestIsNotHazardousWhenBookingIsDeactivated()
		{
			var booking = GetSaveableTransportJob();
			booking.KM_IsHazardous = true;
			Factory.Save();

			booking.Deactivate();
			AssertEquals("Booking should no longer be hazardous", false, booking.KM_IsHazardous);
		}

		public void TestIsNotRequiresRefrigerationWhenBookingIsDeactivated()
		{
			var booking = GetSaveableTransportJob();
			booking.KM_RequiresRefrigeration = true;
			Factory.Save();

			booking.Deactivate();
			AssertEquals("Booking should no longer require refrigeration", false, booking.KM_RequiresRefrigeration);
		}

		public void TestIsSub()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();

			AssertEquals("Precondition: KM_KM_MasterBooking should be an empty Guid.", ZGuid.Empty, booking.KM_KM_MasterBooking);
			AssertEquals("IsSub should return false.", false, booking.IsSub);

			booking.KM_KM_MasterBooking = ZGuid.NewZGuid();
			AssertEquals("IsSub should return true.", true, booking.IsSub);
		}

		public void TestPackagesOnMasterBookingAreRefreshedWhenSubBookingIsAttached()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = subConsolidation.Bookings.AddNew();
			var subInstruction = subBooking.Instructions.AddNew();
			var package = subConsolidation.PackageJob.Packages.AddNew();

			var subInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: package.PK, packageQty: package.KP_PackageQty),
			};
			AssignDivotsToInstructions(subInstruction, subInstructionAssignedPackages);

			Factory.Save();

			AssertEquals("Master Consolidation should not have any packages yet.", 0, masterConsolidation.PackageJob.Packages.Count);

			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			AssertEquals("ShouldReloadPackagesCollection should have been set to true on the Master Consolidation's PackageJob.", true, masterConsolidation.PackageJob.ShouldReloadPackagesCollection);

			AssertEquals("Master Consolidation should now have 1 package.", 1, masterConsolidation.PackageJob.Packages.Count);
		}

		public void TestPackagesOnMasterBookingAreRefreshedWhenSubBookingIsDetached()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;

			var subBooking = subConsolidation.Bookings.AddNew();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			var package = subConsolidation.PackageJob.Packages.AddNew();

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: package.PK, packageQty: package.KP_PackageQty),
			};
			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			AssertEquals("Master Consolidation should have 1 package.", 1, masterConsolidation.PackageJob.Packages.Count);

			subBooking.KM_KM_MasterBooking = ZGuid.Empty;

			AssertEquals("ShouldReloadPackagesCollection should have been set to true on the Master Consolidation's PackageJob.", true, masterConsolidation.PackageJob.ShouldReloadPackagesCollection);
			AssertEquals("Master Consolidation should now have 0 packages.", 0, masterConsolidation.PackageJob.Packages.Count);
		}

		public void TestSubBookingPKsToIncludeAndSubConsolidationPKsToExcludeIsUpdatedOnMasterConsolidationPackageJob_WhenSubBookingIsDetached()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;

			var subBooking = subConsolidation.Bookings.AddNew();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subConsolidation.PackageJob.Packages.AddNew();

			Factory.Save();

			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude.Add(subBooking.PK);
			AssertEquals("SubConsolidationPKsToExclude on Master Consolidation's PackageJob should be empty.", 0, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude.Count);

			subBooking.KM_KM_MasterBooking = ZGuid.Empty;

			AssertContainsExactElementsInAnyOrder("SubConsolidationPKsToExclude on Master Consolidation's PackageJob should include the detached sub booking's consolidation.", new List<ZGuid>() { subConsolidation.PK }, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude);
			AssertEquals("SubBookingPKsToInclude should now be empty.", 0, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude.Count);
		}

		public void TestSubBookingPKsToIncludeAndSubConsolidationPKsToExcludeIsUpdatedOnMasterConsolidationPackageJob_WhenSubBookingIsAttached()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = subConsolidation.Bookings.AddNew();
			subConsolidation.PackageJob.Packages.AddNew();

			Factory.Save();

			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude.Add(subConsolidation.PK);
			AssertEquals("SubBookingPKsToInclude on Master Consolidation's PackageJob should be empty.", 0, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude.Count);

			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			AssertContainsExactElementsInAnyOrder("SubBookingPKsToInclude on Master Consolidation's PackageJob should include the attached sub booking.", new List<ZGuid>() { subBooking.PK }, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude);
			AssertEquals("SubConsolidationPKsToExclude should now be empty.", 0, ((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude.Count);
		}

		public void TestMasterBooking()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			Factory.Save();

			AssertEquals("Precondition: KM_KM_MasterBooking should be an empty Guid.", ZGuid.Empty, booking.KM_KM_MasterBooking);
			AssertEquals("MasterBooking should return null.", null, booking.MasterBooking);

			var masterBooking1 = Factory.NewWithValidTestData<DtbBooking>();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking1.PK;
			booking.KM_MasterBookingVersion = 1;
			Factory.Save();

			AssertEquals("MasterBooking should return the master booking.", masterBooking1.PK, booking.MasterBooking.PK);

			var masterBooking2 = Factory.NewWithValidTestData<DtbBooking>();
			masterBooking2.KM_IsMaster = true;
			masterBooking2.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking2.PK;
			Factory.Save();

			AssertEquals("MasterBooking should return the second master booking.", masterBooking2.PK, booking.MasterBooking.PK);

			booking.KM_KM_MasterBooking = ZGuid.Empty;

			AssertEquals("MasterBooking should return null.", null, booking.MasterBooking);
		}

		public void TestMasterBookingFieldsUpdatedOnBookingAndParentConsolidationIfNewAndMasterFlagSet()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsMaster = true;

			Factory.Save();

			CombineAssertions("If KM_Master is set, master booking fields on booking and consolidation should be set also", () =>
			{
				AssertEquals("booking.KM_MasterBookingVersion should be 1 if KM_IsMaster was set", (short)1, booking.KM_MasterBookingVersion);
				AssertEquals("consolidation.KB_IsMaster should be true if KM_IsMaster was set", true, consolidation.KB_IsMaster);
				AssertEquals("consolidation.KB_MasterBookingVersion should be 1 if KM_IsMaster was set", (short)1, consolidation.KB_MasterBookingVersion);
			});
		}

		public void TestMasterBookingFieldsUpdatedOnBookingAndParentConsolidationIfNewAndMasterFlagNotSetAndNotSub()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			CombineAssertions("Preconditions: booking is not master or sub", () =>
			{
				AssertEquals("Precondition: booking.KM_IsMaster is false (by default)", false, booking.KM_IsMaster);
				AssertEquals("Precondition: booking.KM_KM_MasterBooking is empty", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});

			Factory.Save();

			CombineAssertions("If KM_Master is not set, master booking fields on booking and consolidation should be set to blank also", () =>
			{
				AssertEquals("booking.KM_MasterBookingVersion should be 0 if KM_IsMaster was not set", (short)0, booking.KM_MasterBookingVersion);
				AssertEquals("consolidation.KB_IsMaster should be false if KM_IsMaster was not set", false, consolidation.KB_IsMaster);
				AssertEquals("consolidation.KB_MasterBookingVersion should be 0 if KM_IsMaster was not set", (short)0, consolidation.KB_MasterBookingVersion);
			});
		}

		public void TestMasterBookingFieldsUpdatedOnBookingAndParentConsolidationIfNewAndMasterFlagNotSetAndNotSubEvenIfOtherwiseSet()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = (short)10;
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_MasterBookingVersion = (short)20;

			CombineAssertions("Preconditions: booking is not master or sub", () =>
			{
				AssertEquals("Precondition: booking.KM_IsMaster is false (by default)", false, booking.KM_IsMaster);
				AssertEquals("Precondition: booking.KM_KM_MasterBooking is empty", ZGuid.Empty, booking.KM_KM_MasterBooking);
			});

			Factory.Save();

			AssertEquals("booking.KM_MasterBookingVersion should be 0 if KM_IsMaster was not set", (short)0, booking.KM_MasterBookingVersion);
		}

		public void TestSubBookingUpdatesMasterBookingVersionIfNewAndNotYetSet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;

			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			AssertEquals("Precondition: sub booking MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBooking.KM_MasterBookingVersion);
			Factory.Save();

			AssertEquals("Sub booking MasterBookingVersion should have updated to 1", (short)1, subBooking.KM_MasterBookingVersion);
		}

		public void TestSubBookingDoesNotUpdateMasterBookingVersionIfNewButAlreadySet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)1;

			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = (short)2;

			Factory.Save();

			AssertEquals("Sub booking MasterBookingVersion should have stayed at 2 (process that creates sub probably populated it from the master)", (short)2, subBooking.KM_MasterBookingVersion);
		}

		public void TestSaveBookingReportsErrorIfTryToSaveNewBookingWithInconsistentMasterFlag()
		{
			ErrorReporter.Clear();
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			booking1.KM_JobID = "XXX1";
			Factory.Save();

			var booking2 = Helper.CreateBooking(consolidation);
			booking2.KM_JobID = "XXX2";
			booking2.KM_IsMaster = true;

			AssertEquals("Precondition: no error reported yet", 0, ErrorReporter.TotalErrorCount);

			Factory.Save();

			AssertEquals("Should report inconsistent master booking flag error", "All bookings under a booking consolidation should have the same value of KM_IsMaster, you are trying to save booking 'XXX2' with master flag value 'true' where consolidation parent has other booking(s) with master flag value 'false'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSubBookingUpdatesMasterBookingVersionIfNotNew()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 42;
			masterConsolidation.KB_MasterBookingVersion = 20;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			masterBooking.KM_MasterBookingVersion = 42;
			masterConsolidation.KB_MasterBookingVersion = 20;
			Factory.Save();

			AssertEquals("Precondition: sub booking MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBooking.KM_MasterBookingVersion);
			AssertEquals("Precondition: MasterBookingVersion has been set on master booking", (short)42, masterBooking.KM_MasterBookingVersion);
			AssertEquals("Precondition: MasterBookingVersion has been set on master consolidation", (short)20, masterConsolidation.KB_MasterBookingVersion);

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertEquals("Sub booking MasterBookingVersion should have updated to 1 less than master booking MasterBookingVersion", (short)41, subBooking.KM_MasterBookingVersion);
			AssertEquals("Sub booking should have KM_KM_MasterBooking set", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			AssertEquals("Sub consolidation MasterBookingVersion should have updated to 1 less than master consolidation MasterBookingVersion", (short)19, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Sub consolidation should have KB_KB_MasterBookingConsolidation set", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);
		}

		public void TestSubBookingUpdatesMasterBookingVersionIfNotNew_MasterBookingVersionIs1()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			AssertEquals("Precondition: sub booking MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subBooking.KM_MasterBookingVersion);
			AssertEquals("Precondition: MasterBookingVersion has been set on master booking", (short)1, masterBooking.KM_MasterBookingVersion);
			AssertEquals("Precondition: MasterBookingVersion has been set on master consolidation", (short)1, masterConsolidation.KB_MasterBookingVersion);

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertEquals("Sub booking MasterBookingVersion should have updated to maximum value", short.MaxValue, subBooking.KM_MasterBookingVersion);
			AssertEquals("Sub booking should have KM_KM_MasterBooking set", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			AssertEquals("Sub consolidation MasterBookingVersion should have updated to maximum value", short.MaxValue, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Sub consolidation should have KB_KB_MasterBookingConsolidation set", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);
		}

		public void TestSubBookingDeletesInstructionsAndDocAddresses()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);
			var subInstruction = Helper.CreateInstruction(subBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			var subConfirmation = Helper.CreateConfirmation(subInstruction, ConfirmationTypes.Codes.PickUp);

			var organisation = Helper.CreateOrganisation("QWESYD");
			subBooking.Address.OrganisationPK = organisation.PK;
			var docAddress = subBooking.DocAddresses[0];

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertEquals("Sub instruction should be deleted", true, subInstruction.IsDeleted);
			AssertEquals("Sub confirmation should be deleted", true, subConfirmation.IsDeleted);
			AssertEquals("Sub booking DocAddress should be deleted", true, docAddress.IsDeleted);
		}

		[TestDate(2017, 11, 16)]
		[TestDateIncremental(seconds: 0)]
		public void TestSubBookingUpdatesAuditColumnsOnMasterBusinessObjects()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			var masterInstruction = Helper.CreateInstruction(masterBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			var masterConfirmation = Helper.CreateConfirmation(masterInstruction, ConfirmationTypes.Codes.PickUp);

			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;
			masterInstruction.KN_MasterBookingVersion = 1;
			masterConfirmation.KK_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			var originalZDateTime = ZDateTime.UtcNow;
			CombineAssertions("Precondition: Original last edit value for all master business objects", () =>
			{
				AssertEquals(originalZDateTime, masterConsolidation.KB_SystemLastEditTimeUtc);
				AssertEquals(originalZDateTime, masterBooking.KM_SystemLastEditTimeUtc);
				AssertEquals(originalZDateTime, masterInstruction.KN_SystemLastEditTimeUtc);
				AssertEquals(originalZDateTime, masterConfirmation.KK_SystemLastEditTimeUtc);
			});

			// All times before this line of code should be the same as originalZDateTime, and all times subsequent to this line of code should be the same as updatedZDateTime
			TestDateIncrementalAttribute.Span = new TimeSpan(365, 0, 0, 0);
			var updatedZDateTime = ZDateTime.UtcNow;
			TestDateIncrementalAttribute.Span = new TimeSpan(0);

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			CombineAssertions("Should update audit columns for all master business objects", () =>
			{
				AssertEquals(updatedZDateTime, masterConsolidation.KB_SystemLastEditTimeUtc);
				AssertEquals(updatedZDateTime, masterBooking.KM_SystemLastEditTimeUtc);
				AssertEquals(updatedZDateTime, masterInstruction.KN_SystemLastEditTimeUtc);
				AssertEquals(updatedZDateTime, masterConfirmation.KK_SystemLastEditTimeUtc);
			});
		}

		public void TestSubBookingsRegisteredAsEditable()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			AssertEquals("Master Booking should not indicate it has changes", false, masterBooking.HasChanges);
			masterBooking.SubBookings.Add(subBooking);
			AssertEquals("Master Booking should indicate it has changes", true, masterBooking.HasChanges);
			AssertNoExceptionThrown("Should save without errors", () =>
			{
				Factory.Save();
			});
		}

		public void TestSubBookingUpdatesMasterBookingVersionUponDetach()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			var masterInstruction = Helper.CreateInstruction(masterBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			var masterConfirmation = Helper.CreateConfirmation(masterInstruction, ConfirmationTypes.Codes.PickUp);

			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 20;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			masterBooking.KM_MasterBookingVersion = 20;
			masterConsolidation.KB_MasterBookingVersion = 20;
			masterInstruction.KN_MasterBookingVersion = 20;
			masterConfirmation.KK_MasterBookingVersion = 20;

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);

			Factory.Save();

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBooking);

			Factory.Save();

			var subInstruction = Helper.CreateInstruction(subBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			var subConfirmation = Helper.CreateConfirmation(subInstruction, ConfirmationTypes.Codes.PickUp);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction.KN_MasterBookingVersion = 19;
			subConfirmation.KK_KK_MasterBookingConfirmation = masterConfirmation.PK;
			subConfirmation.KK_MasterBookingVersion = 19;
			Factory.Save();

			AssertEquals("Precondition: Sub consolidation MasterBookingVersion should be non-zero", (short)19, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Precondition: Sub consolidation should have KB_KB_MasterBookingConsolidation set", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("Precondition: Sub booking MasterBookingVersion should be non-zero", (short)19, subBooking.KM_MasterBookingVersion);
			AssertEquals("Precondition: Sub booking should have KM_KM_MasterBooking set", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			AssertEquals("Precondition: Sub instruction MasterBookingVersion should be non-zero", (short)19, subInstruction.KN_MasterBookingVersion);
			AssertEquals("Precondition: Sub instruction should have KN_KN_MasterBookingInstruction set", masterInstruction.PK, subInstruction.KN_KN_MasterBookingInstruction);
			AssertEquals("Precondition: Sub confirmation MasterBookingVersion should be non-zero", (short)19, subConfirmation.KK_MasterBookingVersion);
			AssertEquals("Precondition: Sub booking should have KK_KK_MasterBookingConfirmation set", masterConfirmation.PK, subConfirmation.KK_KK_MasterBookingConfirmation);

			masterBooking.SubBookings.RemoveFromRelationship(subBooking);
			List<DtbBooking> subBookingsToRemove = new()
			{
				subBooking
			};
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToRemove);
			Factory.Save();

			AssertEquals("Sub consolidation MasterBookingVersion should be zero", (short)0, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Sub consolidation should have KB_KB_MasterBookingConsolidation empty", ZGuid.Empty, subConsolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("Sub booking MasterBookingVersion should be zero", (short)0, subBooking.KM_MasterBookingVersion);
			AssertEquals("Sub booking should have KM_KM_MasterBooking empty", ZGuid.Empty, subBooking.KM_KM_MasterBooking);
			AssertEquals("Sub instruction MasterBookingVersion should be zero", (short)0, subInstruction.KN_MasterBookingVersion);
			AssertEquals("Sub instruction should have KN_KN_MasterBookingInstruction empty", ZGuid.Empty, subInstruction.KN_KN_MasterBookingInstruction);
			AssertEquals("Sub confirmation MasterBookingVersion should be zero", (short)0, subConfirmation.KK_MasterBookingVersion);
			AssertEquals("Sub confirmation should have KK_KK_MasterBookingConfirmation empty", ZGuid.Empty, subConfirmation.KK_KK_MasterBookingConfirmation);
		}

		public void TestInstructionOrgTypeSetUponDetach()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			var masterInstruction = Helper.CreateInstruction(masterBooking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);

			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 20;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			masterBooking.KM_MasterBookingVersion = 20;
			masterConsolidation.KB_MasterBookingVersion = 20;
			masterInstruction.KN_MasterBookingVersion = 20;

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			subConsolidation.KB_MasterBookingVersion = 20;
			subBooking.KM_MasterBookingVersion = 20;
			var subInstruction = Helper.CreateInstruction(subBooking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction.KN_MasterBookingVersion = 20;

			Factory.Save();
			AssertEquals("Precondition: Sub instruction should have organisation type set before detach", OrganisationTypesList.Codes.CFS, subInstruction.OrganisationType);

			// Load in new factory as masterBooking has a warning about "Update service task in progress"
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var masterBookingNewFactory = newFactory.Load<DtbBooking>(masterBooking.PK);
			var subBookingNewFactory = newFactory.Load<DtbBooking>(subBooking.PK);
			var subInstructionNewFactory = newFactory.Load<DtbBookingInstruction>(subInstruction.PK);

			AssertEquals("Precondition: Sub instruction should have organisation type set before detach", OrganisationTypesList.Codes.CFS, subInstructionNewFactory.OrganisationType);
			AssertEquals("Precondition: Sub instruction has JobDocAddress for DtbBookingInstruction property Address", 1, subInstructionNewFactory.DocAddresses.Count);
			var docAddressNewFactory = (JobDocAddress)subInstructionNewFactory.DocAddresses.Single();
			AssertEquals("Precondition: JobDocAddress for DtbBookingInstruction property Address should be empty", true, docAddressNewFactory.IsEmpty);
			masterBookingNewFactory.RunPreSaveValidation();
			AssertEquals("Precondition: Master booking does not have any errors", false, masterBookingNewFactory.HasErrors);
			AssertEquals("Precondition: Master booking does not have any warnings", false, masterBookingNewFactory.HasWarnings);

			masterBookingNewFactory.SubBookings.RemoveFromRelationship(subBookingNewFactory);
			newFactory.Save();
			AssertEquals("Sub instruction should have organisation type set after detach", OrganisationTypesList.Codes.CFS, subInstructionNewFactory.OrganisationType);
			subBookingNewFactory.RunPreSaveValidation();
			AssertEquals("Sub booking does not have any errors", false, subBookingNewFactory.HasErrors);
			AssertEquals("Sub booking does not have any warnings", false, subBookingNewFactory.HasWarnings);

			var thirdFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var subBookingThirdFactory = thirdFactory.Load<DtbBooking>(subBookingNewFactory.PK);
			var subInstructionThirdFactory = thirdFactory.Load<DtbBookingInstruction>(subInstruction.PK);
			CombineAssertions("When loaded in a new factory, sub instruction organisation type should be set after detach", () =>
			{
				AssertEquals("Sub instruction should have organisation type set after detach", OrganisationTypesList.Codes.CFS, subInstructionThirdFactory.OrganisationType);
				AssertEquals("Sub instruction should have JobDocAddress for DtbBookingInstruction property Address", 1, subInstructionThirdFactory.DocAddresses.Count);
				var docAddressThirdFactory = (JobDocAddress)subInstructionThirdFactory.DocAddresses.Single();
				AssertEquals("JobDocAddress for DtbBookingInstruction property Address should be empty", true, docAddressThirdFactory.IsEmpty);
				subBookingThirdFactory.RunPreSaveValidation();
				AssertEquals("Sub booking does not have any errors", false, subBookingThirdFactory.HasErrors);
				AssertEquals("Sub booking does not have any warnings", false, subBookingThirdFactory.HasWarnings);
			});
		}

		public void TestConsolidationWithOnlySomeBookingsDetachedDoesNotDetach()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking1 = Helper.CreateBooking(masterConsolidation);
			var masterBooking2 = Helper.CreateBooking(masterConsolidation);

			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_MasterBookingVersion = 20;
			masterBooking2.KM_IsMaster = true;
			masterBooking2.KM_MasterBookingVersion = 20;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking1 = Helper.CreateBooking(subConsolidation);
			var subBooking2 = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			masterBooking1.KM_MasterBookingVersion = 20;
			masterBooking2.KM_MasterBookingVersion = 20;
			masterConsolidation.KB_MasterBookingVersion = 20;

			Factory.Save();

			masterBooking1.SubBookings.Add(subBooking1);
			masterBooking2.SubBookings.Add(subBooking2);
			Factory.Save();

			AssertEquals("Precondition: Sub consolidation MasterBookingVersion should be non-zero", (short)19, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Precondition: Sub consolidation should have KB_KB_MasterBookingConsolidation set", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);

			masterBooking1.SubBookings.RemoveFromRelationship(subBooking1);
			Factory.Save();

			AssertEquals("Detached booking MasterBookingVersion should be zero", (short)0, subBooking1.KM_MasterBookingVersion);
			AssertEquals("Detached booking should have KM_KM_MasterBooking empty", ZGuid.Empty, subBooking1.KM_KM_MasterBooking);
			AssertEquals("Consolidation MasterBookingVersion should be non-zero", (short)19, subConsolidation.KB_MasterBookingVersion);
			AssertEquals("Consolidation should have KB_KB_MasterBookingConsolidation set", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("Non-detached booking MasterBookingVersion should be non-zero", (short)19, subBooking2.KM_MasterBookingVersion);
			AssertEquals("Non-detached booking should have KM_KM_MasterBooking set", masterBooking2.PK, subBooking2.KM_KM_MasterBooking);
		}

		public void TestSubBookingCopiesAddressesUponDetach()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			var masterInstruction = Helper.CreateInstruction(masterBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			masterInstruction.Address.Delete();

			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 2;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);

			Factory.Save();

			masterBooking.KM_MasterBookingVersion = 2;
			masterConsolidation.KB_MasterBookingVersion = 2;
			masterInstruction.KN_MasterBookingVersion = 2;

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);

			Factory.Save();

			CreateSubBookingInstructionAndLinkToMasterBookingInstruction(masterBooking, subBooking);

			Factory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking = Factory.Load<DtbBooking>(subBooking.PK);

			var subInstruction = Helper.CreateInstruction(subBooking, InstructionTypes.Codes.PickUp, "CTO", null);
			subInstruction.Address.Delete();
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction.KN_MasterBookingVersion = 1;

			AssertEquals("Precondition: Should be no address on the master consolidation", 0, masterConsolidation.DocAddresses.Count);
			AssertEquals("Precondition: Should be no address on the detached sub consolidation", 0, subConsolidation.DocAddresses.Count);

			AssertEquals("Precondition: Should be no address on the master booking", 0, masterBooking.DocAddresses.Count);
			AssertEquals("Precondition: Should be no address on the detached sub booking", 0, subBooking.DocAddresses.Count);

			AssertEquals("Precondition: Should be no address on the master instruction", 0, masterInstruction.DocAddresses.Count);
			AssertEquals("Precondition: Should be no address on the detached sub instruction", 0, subInstruction.DocAddresses.Count);

			var consolidationOrganisation = Helper.CreateOrganisation("CONORG");
			masterConsolidation.BookedByAddress.OrganisationPK = consolidationOrganisation.PK;
			var bookingOrganisation = Helper.CreateOrganisation("BOOKORG");
			masterBooking.Address.OrganisationPK = bookingOrganisation.PK;
			var instructionOrganisation = Helper.CreateOrganisation("INSTORG");
			masterInstruction.Address.E2_OA_Address = instructionOrganisation.MainAddress.PK;

			Factory.Save();

			masterBooking.SubBookings.RemoveFromRelationship(subBooking);
			List<DtbBooking> subBookingsToRemove = new()
			{
				subBooking
			};
			masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(subBookingsToRemove);
			Factory.Save();

			AssertEquals("Precondition: Should be one address on the master consolidation", 1, masterConsolidation.DocAddresses.Count);
			AssertEquals("Precondition: Should be one address on the detached sub consolidation", 1, subConsolidation.DocAddresses.Count);
			AssertNotEquals("Detached sub consolidation should have a distinct address from the master consolidation", masterConsolidation.DocAddresses[0].PK, subConsolidation.DocAddresses[0].PK);
			AssertEquals("Address on the detached sub consolidation should point to the same organisation as the address on the master consolidation does", masterConsolidation.DocAddresses[0].Organisation.PK, subConsolidation.DocAddresses[0].Organisation.PK);

			AssertEquals("Precondition: Should be one address on the master booking", 1, masterBooking.DocAddresses.Count);
			AssertEquals("Precondition: Should be one address on the detached sub booking", 1, subBooking.DocAddresses.Count);
			AssertNotEquals("Detached sub booking should have a distinct address from the master booking", masterBooking.DocAddresses[0].PK, subBooking.DocAddresses[0].PK);
			AssertEquals("Address on the detached sub booking should point to the same organisation as the address on the master booking does", masterBooking.DocAddresses[0].Organisation.PK, subBooking.DocAddresses[0].Organisation.PK);

			AssertEquals("Precondition: Should be one address on the master instruction", 1, masterInstruction.DocAddresses.Count);
			AssertEquals("Precondition: Should be one address on the detached sub instruction", 1, subInstruction.DocAddresses.Count);
			AssertNotEquals("Detached sub instruction booking should have a distinct address from the master instruction", masterInstruction.DocAddresses[0].PK, subInstruction.DocAddresses[0].PK);
			AssertEquals("Address on the detached instruction booking should point to the same organisation as the address on the master instruction does", masterInstruction.DocAddresses[0].Organisation.PK, subInstruction.DocAddresses[0].Organisation.PK);
		}

		public void TestBookingAddressCountAfterSubAttachedToMasterAndSaved()
		{
			var masterBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);

			Factory.Save();

			AssertEquals("Should be no address on the master booking", 0, masterBooking.DocAddresses.Count);
			AssertEquals("Should be no address on the sub booking", 0, subBooking.DocAddresses.Count);
		}

		public void TestAttachEvents_MasterBooking()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = (short)1;
			masterBooking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Precondition: JobDescription includes template", "Master Transport Booking - FCL Import", ((IRelatedJob)masterBooking).JobDescription);

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			AssertEquals("Logs attachment of sub to master", 1, GetLogs(subBooking.Logs, Events.Attached).Count());
			AssertLog(subBooking, Events.Attached, "Master Transport Booking", masterBooking.KM_JobID);
		}

		public void TestDetachEvents_MasterBooking()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = (short)1;
			masterBooking.KM_KT_NKBookingTemplate = template.KT_Code;
			AssertEquals("Precondition: JobDescription includes template", "Master Transport Booking - FCL Import", ((IRelatedJob)masterBooking).JobDescription);

			var subBooking = Helper.CreateBooking();

			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			masterBooking.SubBookings.RemoveFromRelationship(subBooking);
			Factory.Save();

			AssertEquals("Logs detachment of sub from master", 1, GetLogs(subBooking.Logs, Events.Detached).Count());
			AssertLog(subBooking, Events.Detached, "Master Transport Booking", masterBooking.KM_JobID);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a single-booking form is a Sub but there are no other reasons to lock it down, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a single-booking form is being managed by an authorised CBA but there are no other reasons to lock it down, it should only be partially locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full,
				generalAssertionMessage: "When a booking on a single-booking form is attached to a consolidation that is read-only, it should be locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full,
				generalAssertionMessage: "When a booking on a single-booking form is inactive or consigned, it should be locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full,
				generalAssertionMessage: "When a booking on a single-booking form has a 'Booking Confirmed' event attached, it should be locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.None
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a single-booking form is a Sub and (or) is Managed by an authorised CBA but there are no other reasons to lock it down, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: false,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a single-booking form is a Sub and has a 'Booking Confirmed' event attached, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a single-booking form is a Sub and has a 'Booking Confirmed' event attached, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsNotInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: false,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsNotReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: false,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_SingleBookingForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEventAndBookingIsInactiveOrConsignedAndConsolidationIsReadOnly()
		{
			AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: true,
				isInactiveOrConsigned: true,
				isConsolidationReadOnly: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a multi-booking form is a Sub booking but there are no other reasons to lock it down, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a multi-booking form is being managed by an authorised CBA but there are no other reasons to lock it down, it should only be partially locked down."
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: true,
				expectedLockdownLevel: BookingLockdownLevel.Full,
				generalAssertionMessage: "When a booking on a multi-booking form has a 'Booking Confirmed' event attached, it should be locked down."
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingDoesNotHaveABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: false,
				hasABookingConfirmedEvent: false,
				expectedLockdownLevel: BookingLockdownLevel.None
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingDoesNotHaveABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: false,
				expectedLockdownLevel: BookingLockdownLevel.Partial,
				generalAssertionMessage: "When a booking on a multi-booking form is a Sub and (or) is Managed by an authorised CBA but there are no other reasons to lock it down, it should be partially locked down."
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsNotBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: false,
				isSub: true,
				hasABookingConfirmedEvent: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsNotSubAndBookingHasABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: false,
				hasABookingConfirmedEvent: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		public void TestUpdateReadOnly_MultiForm_WhenBookingIsBeingManagedByAuthorisedCarrierBookingAgentAndBookingIsSubAndBookingHasABookingConfirmedEvent()
		{
			AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(
				bookingIsBeingManagedByAuthorisedCarrierBookingAgent: true,
				isSub: true,
				hasABookingConfirmedEvent: true,
				expectedLockdownLevel: BookingLockdownLevel.Full
				);
		}

		void AssertUpdateReadOnly_SingleBookingFormSetsCorrectElementsToReadOnly(bool bookingIsBeingManagedByAuthorisedCarrierBookingAgent, bool isSub, bool hasABookingConfirmedEvent, bool isInactiveOrConsigned, bool isConsolidationReadOnly, BookingLockdownLevel expectedLockdownLevel, string generalAssertionMessage = "")
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var booking = CreateBookingForUpdateReadOnlyMethod(bookingIsBeingManagedByAuthorisedCarrierBookingAgent, isSub, hasABookingConfirmedEvent, isInactiveOrConsigned, isConsolidationReadOnly);

			booking.UpdateReadOnly_SingleBookingForm();

			AssertUpdateReadOnlyMethodSetsCorrectElementsToReadOnly(booking, expectedLockdownLevel, generalAssertionMessage);
		}

		void AssertUpdateReadOnly_MultiFormSetsCorrectElementsToReadOnly(bool bookingIsBeingManagedByAuthorisedCarrierBookingAgent, bool isSub, bool hasABookingConfirmedEvent, BookingLockdownLevel expectedLockdownLevel, string generalAssertionMessage = "")
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			var booking = CreateBookingForUpdateReadOnlyMethod(bookingIsBeingManagedByAuthorisedCarrierBookingAgent, isSub, hasABookingConfirmedEvent);

			booking.UpdateReadOnly_MultiForm();

			AssertUpdateReadOnlyMethodSetsCorrectElementsToReadOnly(booking, expectedLockdownLevel, generalAssertionMessage);
		}

		DtbBooking CreateBookingForUpdateReadOnlyMethod(bool bookingIsBeingManagedByAuthorisedCarrierBookingAgent, bool isSub, bool hasABookingConfirmedEvent, bool isInactiveOrConsigned = false, bool isConsolidationReadOnly = false)
		{
			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.PackageJob.Packages.AddNew();
			booking.Instructions.AddNew();
			Helper.LoadOrCreateJobHeader(booking);
			new ProcessTaskCollectionViewFilter(booking.WorkflowItems);

			if (isInactiveOrConsigned)
			{
				booking.KM_IsActive = false;
			}
			else
			{
				var bookingIsInactiveOrConsigned = ConsolidationViewModeService.GetViewMode(Factory) == ConsolidationViewMode.MultiJob || !booking.KM_IsActive || booking.ConsignmentConsol != null;
				AssertEquals("Precondition: Booking should not be inactive or consigned.", false, bookingIsInactiveOrConsigned);
			}

			if (isConsolidationReadOnly)
			{
				booking.ConsolidationSingleJob.ReadOnly = true;
			}
			else
			{
				var consolidationIsReadOnly = booking.ConsolidationSingleJob != null && booking.ConsolidationSingleJob.ReadOnly;
				AssertEquals("Precondition: Booking's ConsolidationSingleJob should not be read only.", false, consolidationIsReadOnly);
			}

			AssertEquals("Precondition: No Packages should be ReadOnly.", false, booking.PackageJob.Packages.Any(i => i.ReadOnly));
			AssertEquals("Precondition: Package Job should not be ReadOnly.", false, booking.PackageJob.ReadOnly);

			if (isSub)
			{
				var masterBooking = Helper.CreateBooking();
				masterBooking.KM_IsMaster = true;
				booking.KM_KM_MasterBooking = masterBooking.PK;
			}
			else
			{
				AssertEquals("Precondition: Booking should not be a sub booking.", false, booking.IsSub);
			}

			if (hasABookingConfirmedEvent)
			{
				Helper.AddBookingConfirmedEventToBooking(booking, "BLUME_EAD");
				Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			}
			else
			{
				AssertNull("Precondition: Booking should not have a 'Booking Confirmed' event attached.", booking.GetLogs().MostRecentLogByEventTime(Events.BookingConfirmed));
			}

			if (bookingIsBeingManagedByAuthorisedCarrierBookingAgent)
			{
				Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
				booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			}
			else
			{
				AssertEquals("Precondition: Booking should not be being managed by an authorised Carrier Booking Agent.", false, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
			}

			return booking;
		}

		void AssertUpdateReadOnlyMethodSetsCorrectElementsToReadOnly(DtbBooking booking, BookingLockdownLevel expectedLockdownLevel, string generalAssertionMessage = "")
		{
			CombineAssertions(generalAssertionMessage, () =>
			{
				var expectedLockdownLevelIsFullOrPartial = expectedLockdownLevel == BookingLockdownLevel.Full || expectedLockdownLevel == BookingLockdownLevel.Partial;
				var expectedLockdownLevelIsPartialOrNone = expectedLockdownLevel == BookingLockdownLevel.Partial || expectedLockdownLevel == BookingLockdownLevel.None;

				var stringToInsertForControlsThatAreLockedDownInPartialLockdown = expectedLockdownLevelIsFullOrPartial ? "" : " NOT";
				var stringToInsertForControlsThatAreNotLockedDownInPartialLockdown = expectedLockdownLevelIsPartialOrNone ? " NOT" : "";

				AssertEquals("Booking should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.ReadOnly);
				AssertEquals("DocAddresses should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.DocAddresses.ReadOnly);
				AssertEquals("Transport Company Address should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.Address.ReadOnly);
				AssertEquals("Instructions should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.Instructions.ReadOnly);
				AssertEquals("Packages_PackageView should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.Packages_PackageView.ReadOnly);

				if (expectedLockdownLevelIsFullOrPartial)
				{
					AssertEquals("All Instructions should be ReadOnly.", true, booking.Instructions.All(i => i.ReadOnly));
				}
				else
				{
					AssertEquals("No Instructions should be ReadOnly.", false, booking.Instructions.Any(i => i.ReadOnly));
				}

				if (DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Transport)
				{
					AssertEquals("Consolidation should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.ConsolidationSingleJob.ReadOnly);
					AssertEquals("Package Job should" + stringToInsertForControlsThatAreLockedDownInPartialLockdown + " be ReadOnly.", expectedLockdownLevelIsFullOrPartial, booking.PackageJob.ReadOnly);

					if (expectedLockdownLevelIsFullOrPartial)
					{
						AssertEquals("All Packages should be ReadOnly.", true, booking.PackageJob.Packages.All(i => i.ReadOnly));
					}
					else
					{
						AssertEquals("No Packages should be ReadOnly.", false, booking.PackageJob.Packages.Any(i => i.ReadOnly));
					}

					AssertEquals(nameof(booking.AdditionalReferencesForBinding) + " should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.AdditionalReferencesForBinding.ReadOnly);

					foreach (var additionalReference in booking.AdditionalReferencesForBinding)
					{
						AssertEquals("Each element of the " + nameof(booking.AdditionalReferencesForBinding) + " collection should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, additionalReference.ReadOnly);
					}

					var newAdditionalReference = booking.AdditionalReferencesForBinding.AddNew();
					AssertEquals("New elements added to the " + nameof(booking.AdditionalReferencesForBinding) + " collection should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly", !expectedLockdownLevelIsPartialOrNone, newAdditionalReference.ReadOnly);
				}
				else if (DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Consolidation)
				{
					AssertEquals("Consolidation should still not be ReadOnly.", false, booking.ConsolidationSingleJob.ReadOnly);
					AssertEquals("Still no Packages should be ReadOnly.", false, booking.PackageJob.Packages.Any(i => i.ReadOnly));
					AssertEquals("Package Job should still not be ReadOnly.", false, booking.PackageJob.ReadOnly);

					AssertEquals(nameof(booking.AdditionalReferenceNumbers) + " should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.AdditionalReferenceNumbers.ReadOnly);

					foreach (var additionalReference in (BusinessObjectCollection)booking.AdditionalReferenceNumbers)
					{
						AssertEquals("Each element of the " + nameof(booking.AdditionalReferencesForBinding) + " collection should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, additionalReference.ReadOnly);
					}

					var newAdditionalReference = booking.AdditionalReferenceNumbers.AddNew() as BusinessObject;
					AssertEquals("New elements added to the " + nameof(booking.AdditionalReferencesForBinding) + " collection should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly", !expectedLockdownLevelIsPartialOrNone, newAdditionalReference.ReadOnly);
				}

				AssertEquals("WorkflowItems should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.WorkflowItems.ReadOnly);
				AssertEquals("Job should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.Job.ReadOnly);

				AssertEquals("Workflow Milestones should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.WorkflowItems.MilestonesIncludingRelatedSortable.ReadOnly);
				AssertEquals("Workflow Triggers should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.WorkflowItems.TriggersIncludingRelated.ReadOnly);
				AssertEquals("Workflow Tasks should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.WorkflowItems.Tasks.TasksViewFilter.ReadOnly);
				AssertEquals("Workflow Exceptions should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.WorkflowItems.ExceptionsIncludingRelated.ReadOnly);

				AssertEquals(nameof(booking.Notes) + " should NOT be ReadOnly.", false, booking.Notes.ReadOnly);
				foreach (var note in booking.Notes.GetAllNotes())
				{
					AssertEquals("Each element of the " + nameof(booking.Notes) + " collection should NOT be ReadOnly.", false, note.ReadOnly);
				}

				var newNote = booking.Notes.AddNew();
				AssertEquals("New elements added to the " + nameof(booking.Notes) + " collection should NOT be ReadOnly", false, newNote.ReadOnly);

				AssertEquals("CustomBusinessObject should" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly.", !expectedLockdownLevelIsPartialOrNone, booking.CustomBusinessObject_ForTest.ReadOnly);
				booking.GetCustomBusinessObject();
				AssertEquals("CustomBusinessObject should still" + stringToInsertForControlsThatAreNotLockedDownInPartialLockdown + " be ReadOnly after accessing CustomBusinessObject again.", !expectedLockdownLevelIsPartialOrNone, booking.CustomBusinessObject_ForTest.ReadOnly);
			});
		}

		enum BookingLockdownLevel
		{
			Full,
			Partial,
			None
		}

		public void TestUpdateChildReadOnlyWhenRegistering_ChildrenWhenBookingIsPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.SetReadOnlyIncludingChildren(true);

			var child = new DummyBusinessObjectCollection(Factory);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: true);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_ChildrenWhenBookingIsNotPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.SetReadOnlyIncludingChildren(true);

			var child = new DummyBusinessObjectCollection(Factory);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: true);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_StmNoteCollectionChildWhenBookingIsPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.SetReadOnlyIncludingChildren(true);

			var child = new StmNoteCollection(booking, Factory);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: false);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_StmNoteCollectionChildWhenBookingIsNotPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.SetReadOnlyIncludingChildren(true);

			var child = new StmNoteCollection(booking, Factory);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: false);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_TransportBookingAdditionalReferenceCollectionChildWhenBookingIsPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.SetReadOnlyIncludingChildren(true);

			var child = new TransportBookingAdditionalReferenceCollection(booking);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: false);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_TransportBookingAdditionalReferenceCollectionChildWhenBookingIsNotPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.SetReadOnlyIncludingChildren(true);

			var child = new TransportBookingAdditionalReferenceCollection(booking);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: true);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_ICusEntryNumAdditionalReferenceCollectionChildWhenBookingIsPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.SetReadOnlyIncludingChildren(true);

			var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
			var child = provider.GetCollection(booking);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: false);
		}

		public void TestUpdateChildReadOnlyWhenRegistering_ICusEntryNumAdditionalReferenceCollectionChildWhenBookingIsNotPartiallyLockedDown()
		{
			var booking = Helper.CreateBooking();
			booking.SetReadOnlyIncludingChildren(true);

			var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
			var child = provider.GetCollection(booking);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			child.AddNew();

			booking.RegisterEditableChildObject(child);

			AssertCollectionAndElementsHaveCorrectReadOnlyValue(child, expectedReadOnlyValue: true);
		}

		void AssertCollectionAndElementsHaveCorrectReadOnlyValue(IBusinessObjectCollection collection, bool expectedReadOnlyValue)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Collection should be ReadOnly: " + expectedReadOnlyValue, expectedReadOnlyValue, collection.ReadOnly);

				foreach (BusinessObject element in collection)
				{
					AssertEquals("Existing elements should be ReadOnly: " + expectedReadOnlyValue, expectedReadOnlyValue, element.ReadOnly);
				}

				var newElement = collection.AddNew();
				AssertEquals("New elements should be ReadOnly: " + expectedReadOnlyValue, expectedReadOnlyValue, newElement.ReadOnly);
			});
		}

		public void TestNotificationBufferForSendingXUSToCTO()
		{
			var booking = Helper.CreateBooking();
			AssertNotNull(booking.NotificationBufferForSendingXUSToCTO);
		}

		public void TestRunPreSaveValidationCore_WhenNotificationBufferForSendingXUSToCTOHasErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.Instructions.First().KN_InstructionType = ZString.Empty;
			booking.IsValidatingSendingXUSToCTO = true;

			AssertEquals("Precondition: Booking should have a KM_Status of 'Available'.", TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("Precondition: IsSendingXUSToCTO should be true.", true, booking.IsSendingXUSToCTO);

			booking.RunPreSaveValidation();

			AssertEquals("Booking should now have a KM_Status of 'Action Required'.", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
			AssertEquals("NotificationBufferForSendingXUSToCTO should have an Error as a result of the validation.", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
		}

		public void TestRunPreSaveValidationCore_WhenNotificationBufferForSendingXUSToCTOHasMessageErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			booking.IsValidatingSendingXUSToCTO = true;

			AssertEquals("Precondition: Booking should have a KM_Status of 'Available'", TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("Precondition: IsSendingXUSToCTO should be true.", true, booking.IsSendingXUSToCTO);

			booking.RunPreSaveValidation();

			AssertEquals("Booking should now have a KM_Status of 'Action Required'.", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
			AssertEquals("NotificationBufferForSendingXUSToCTO should have a Message Error as a result of the validation.", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
		}

		public void TestRunPreSaveValidationCore_WhenNotificationBufferForSendingXUSToCTOHasNoErrorsOrMessageErrorsAndStatusIsActionRequired()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.KM_Status = TransportStatuses.Codes.ActionRequired;

			AssertEquals("Precondition: IsSendingXUSToCTO should be true.", true, booking.IsSendingXUSToCTO);

			booking.RunPreSaveValidation();

			AssertEquals("Booking should now have a KM_Status of 'Available'.", TransportStatuses.Codes.Available, booking.KM_Status);
			AssertEquals("NotificationBufferForSendingXUSToCTO should have no Errors as a result of the validation.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
			AssertEquals("NotificationBufferForSendingXUSToCTO should have no Message Errors as a result of the validation.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
		}

		public void TestRunPreSaveValidationCore_WhenNotificationBufferForSendingXUSToCTOHasNoErrorsOrMessageErrorsAndStatusIsNotActionRequired()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			booking.KM_Status = TransportStatuses.Codes.Held;
			booking.IsValidatingSendingXUSToCTO = true;

			AssertEquals("Precondition: IsSendingXUSToCTO should be true.", true, booking.IsSendingXUSToCTO);

			booking.RunPreSaveValidation();

			AssertEquals("Booking should still have a KM_Status of 'Held'.", TransportStatuses.Codes.Held, booking.KM_Status);
			AssertEquals("NotificationBufferForSendingXUSToCTO should have no Errors as a result of the validation.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
			AssertEquals("NotificationBufferForSendingXUSToCTO should have no Message Errors as a result of the validation.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
		}

		public void TestBookingIsBeingManagedByAuthorisedCarrierBookingAgent_WhenTransportCompanyIsNotAnAuthorisedCarrierBookingAgentAndServiceIsNotCommenced()
		{
			AssertBookingIsBeingManagedByAuthorisedCarrierBookingAgent_ReturnsCorrectValue(
				transportCompanyIsAuthorisedCarrierBookingAgent: false,
				serviceIsCommenced: false,
				expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue: false
				);
		}

		public void TestBookingIsBeingManagedByAuthorisedCarrierBookingAgent_WhenTransportCompanyIsNotAnAuthorisedCarrierBookingAgentAndServiceIsCommenced()
		{
			AssertBookingIsBeingManagedByAuthorisedCarrierBookingAgent_ReturnsCorrectValue(
				transportCompanyIsAuthorisedCarrierBookingAgent: false,
				serviceIsCommenced: true,
				expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue: false
				);
		}

		public void TestBookingIsBeingManagedByAuthorisedCarrierBookingAgent_WhenTransportCompanyIsAnAuthorisedCarrierBookingAgentAndServiceIsNotCommenced()
		{
			AssertBookingIsBeingManagedByAuthorisedCarrierBookingAgent_ReturnsCorrectValue(
				transportCompanyIsAuthorisedCarrierBookingAgent: true,
				serviceIsCommenced: false,
				expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue: false
				);
		}

		public void TestBookingIsBeingManagedByAuthorisedCarrierBookingAgent_WhenTransportCompanyIsAnAuthorisedCarrierBookingAgentAndServiceIsCommenced()
		{
			AssertBookingIsBeingManagedByAuthorisedCarrierBookingAgent_ReturnsCorrectValue(
				transportCompanyIsAuthorisedCarrierBookingAgent: true,
				serviceIsCommenced: true,
				expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue: true
				);
		}

		void AssertBookingIsBeingManagedByAuthorisedCarrierBookingAgent_ReturnsCorrectValue(bool transportCompanyIsAuthorisedCarrierBookingAgent, bool serviceIsCommenced, bool expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue)
		{
			var booking = Helper.CreateBooking();

			if (transportCompanyIsAuthorisedCarrierBookingAgent)
			{
				Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			}

			if (serviceIsCommenced)
			{
				booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			}
			else
			{
				AssertNotEquals("Precondition: Booking should not have a status of 'Service Commenced'.", TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);
			}

			var assertionMessage = "BookingIsBeingManagedByAuthorisedCarrierBookingAgent should return " + expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue.ToString();
			AssertEquals(assertionMessage, expectedBookingIsBeingManagedByAuthorisedCarrierBookingAgentValue, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
		}

		public void TestCarrierBookingAgentIsAuthorised()
		{
			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			AssertEquals("BookingIsBeingManagedByAuthorisedCarrierBookingAgent should be true because CarrierBookingAgentIsAuthorised is true", true, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
		}

		public void TestCarrierBookingAgentIsAuthorised_ChecksEK_Module()
		{
			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			var hubMode = booking.CarrierBookingAgentDocAddress.Address.Header.EDICommunicationsModes.Single() as EDICommunicationsMode;
			hubMode.EK_Module = RelatableActivityTypeList.Codes.ArInvoice;

			AssertEquals("Precondition: IsServiceCommenced is true", true, booking.IsServiceCommenced);
			AssertEquals("BookingIsBeingManagedByAuthorisedCarrierBookingAgent should be false because CarrierBookingAgentIsAuthorised is false because EK_Module is not 'TBM'", false, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
		}

		public void TestCarrierBookingAgentIsAuthorised_ChecksEK_CommunicationsTransport()
		{
			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			var hubMode = booking.CarrierBookingAgentDocAddress.Address.Header.EDICommunicationsModes.Single() as EDICommunicationsMode;
			hubMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss;

			AssertEquals("Precondition: IsServiceCommenced is true", true, booking.IsServiceCommenced);
			AssertEquals("BookingIsBeingManagedByAuthorisedCarrierBookingAgent should be false because CarrierBookingAgentIsAuthorised is false because EK_CommunicationsTransport is not 'HUB'", false, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
		}

		public void TestCarrierBookingAgentIsAuthorised_ChecksEK_Destination()
		{
			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			var hubMode = booking.CarrierBookingAgentDocAddress.Address.Header.EDICommunicationsModes.Single() as EDICommunicationsMode;
			hubMode.EK_Destination = "Wrong";

			AssertEquals("Precondition: IsServiceCommenced is true", true, booking.IsServiceCommenced);
			AssertEquals("BookingIsBeingManagedByAuthorisedCarrierBookingAgent should be false because CarrierBookingAgentIsAuthorised is false because EK_Destination is not an authorised carrier booking agent", false, booking.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
		}

		public void TestHasABookingConfirmedEvent_WhenCarrierBookingAgentIsAuthorisedFalse()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var booking = Helper.CreateBooking();
			Helper.AddBookingConfirmedEventToBooking(booking, "BLUME_EAD");
			booking.UpdateReadOnly_SingleBookingForm();
			AssertEquals("Booking should not be read only because CarrierBookingAgentIsAuthorised and therefore HasABookingConfirmedEvent is false", false, booking.ReadOnly);
		}

		public void TestHasABookingConfirmedEvent_WhenCarrierBookingAgentIsAuthorisedTrue()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var booking = Helper.CreateBooking();
			Helper.AddBookingConfirmedEventToBooking(booking, "BLUME_EAD");
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			booking.UpdateReadOnly_SingleBookingForm();
			AssertEquals("Booking should be read only because CarrierBookingAgentIsAuthorised and therefore HasABookingConfirmedEvent is true", true, booking.ReadOnly);
		}

		public void TestHasABookingConfirmedEvent_SenderNotAuthorisedCarrierBookingAgent()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			Helper.AddBookingConfirmedEventToBooking(booking, "Not an authorised CBA");

			Factory.Save();
			booking.UpdateReadOnly_SingleBookingForm();
			AssertEquals("Booking should not be read only because HasABookingConfirmedEvent is false due to the message sender not being an authorised CBA", false, booking.ReadOnly);
		}

		public void TestHasABookingConfirmedEvent_SenderIsAuthorisedCarrierBookingAgent()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			var booking = Helper.CreateBooking();
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking);
			Helper.AddBookingConfirmedEventToBooking(booking, "BLUME_EAD");

			Factory.Save();
			booking.UpdateReadOnly_SingleBookingForm();
			AssertEquals("Booking should be read only because HasABookingConfirmedEvent is true due to the message sender being an authorised CBA", true, booking.ReadOnly);
		}

		public void TestTotalCO2eForBinding()
		{
			// Arrange
			var booking = Helper.CreateBooking();

			// Act & Assert
			AssertEquals(string.Empty, booking.TotalCO2eForBinding);

			// Act & Assert
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			booking.SetTotalCO2e(123.31331m);
			AssertEquals("123.313", booking.TotalCO2eForBinding);

			// Act & Assert
			booking.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			AssertEquals("Pending", booking.TotalCO2eForBinding);
		}

		public void TestTotalCO2eForSorting()
		{
			var booking = Helper.CreateBooking();
			AssertEquals(0m, booking.TotalCO2eForSorting);

			booking.SetTotalCO2e(123.31331m);
			AssertEquals(123.31331m, booking.TotalCO2eForSorting);
		}

		public void TestTotalCO2eForSorting_DecimalPlaces()
		{
			var co2eForSortingDp = typeof(DtbBooking)
				.GetProperty(nameof(DtbBooking.TotalCO2eForSorting))
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			AssertEquals("TotalCO2eForSorting should display using 3dp", 3, co2eForSortingDp.DecimalPlaces);
		}

		public void TestJobCO2eCollection()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			AssertEquals(0, booking.JobCO2eCollection.Count);
			Assert(!booking.HaveJobCO2e);
			ErrorReporter.Clear();

			// Act & Assert
			booking.SetCO2ePerTonneInKg(2m, "DUC");
			Assert(!booking.HaveJobCO2e);
			AssertContains("ValidateCO2eType - type not allowed", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			// Act & Assert
			booking.SetCO2ePerTonneInKg(2m);
			Assert(booking.HaveJobCO2e);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals(2m, booking.GetCO2ePerTonneInKg());
		}

		public void TestCheckCO2eCalcMandatoryField()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			// Act & Assert
			var reasons = (booking as ICO2eLocationBasedSupporter).ValidateInputs();
			AssertContainsExactElementsInAnyOrder(new List<string> { "Transport Booking > Details > Instructions is empty." }, reasons);

			// Act & Assert
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			Helper.CreatePackageDivot(pic, package, 4);
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var organisation = Helper.CreateOrganisation("QWESYD");
			dlv.Address.E2_OA_Address = organisation.MainAddress.PK;
			reasons = (booking as ICO2eLocationBasedSupporter).ValidateInputs();
			AssertContainsExactElementsInAnyOrder(new List<string> { "Transport Booking > Details > Instructions do not have a matching load/unload package." }, reasons);

			// Act & Assert
			Helper.CreatePackageDivot(dlv, package, 4);
			reasons = (booking as ICO2eLocationBasedSupporter).ValidateInputs();
			AssertContainsExactElementsInAnyOrder(new List<string> { $"Transport Booking > Details > Instruction '{pic.KN_Sequence}' > Address is empty." }, reasons);
		}

		void ActAndAssertSettingValidationStatusDoesNotTickAddressOverride(DtbBooking booking)
		{
			var addressValidation = booking as IAddressesValidation;

			// Act
			foreach (var address in addressValidation.AddressesToValidate)
			{
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			}

			// Assert
			foreach (var actions in new CO2eInstructionMatcher(booking).FindAllMatches().Actions)
			{
				Assert(!actions.Address.E2_AddressOverride);
			}
		}

		public void TestCO2eAddressValidationAddressesToValidate()
		{
			// Arrange
			var addresses = Enumerable.Range(0, 4).Select(_ => Factory.NewWithValidTestData<OrgHeader>().MainAddress).ToArray();

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", 1);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package2 = Helper.CreatePackage("PKG2", 2);
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var instruction1 = Helper.CreateInstruction(booking, "PIC", OrganisationTypesList.Codes.CNR, addresses[0]);
			Helper.CreatePackageDivot(instruction1, package1, 1);
			var instruction2 = Helper.CreateInstruction(booking, "PIC", OrganisationTypesList.Codes.WHS, addresses[1]);
			Helper.CreatePackageDivot(instruction2, package2, 1);
			var instruction3 = Helper.CreateInstruction(booking, "DLV", OrganisationTypesList.Codes.CNE, addresses[2]);
			Helper.CreatePackageDivot(instruction3, package1, 1);
			var instruction4 = Helper.CreateInstruction(booking, "PIC", OrganisationTypesList.Codes.WHS, addresses[3]);
			Helper.CreatePackageDivot(instruction4, package2, 1);
			var instruction5 = Helper.CreateInstruction(booking, "DLV", OrganisationTypesList.Codes.CNE, null);
			instruction5.Address.E2_OA_Address = ZGuid.Empty;
			instruction5.Address.E2_AddressOverride = true;
			instruction5.Address.E2_Address1 = "1 Street St";
			instruction5.Address.E2_Postcode = "XXXX";
			instruction5.Address.E2_City = "XX";
			instruction5.Address.E2_State = "NSW";
			instruction5.Address.E2_RN_NKCountryCode = "AU";
			Helper.CreatePackageDivot(instruction5, package2, 2);

			// Act & Assert
			var addressValidation = booking as IAddressesValidation;
			AssertEquals("Addresses to validate should be 5.", 5, addressValidation.AddressesToValidate.Length);
			AssertContainsExactElementsInAnyOrder(addresses.Cast<ISupportWebAddressValidation>().Concat([instruction5.Address]), addressValidation.AddressesToValidate);

			// Arrange
			instruction5.Address.E2_AddressOverride = false;

			// Act & Assert
			ActAndAssertSettingValidationStatusDoesNotTickAddressOverride(booking);
		}

		[TestDate(2024, 1, 1)]
		public void TestUpdateCO2eStatusToNotCurrent()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 1, 1000);
			var package2 = Helper.CreatePackage("PKG2", null, 1, 200);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			pic.OrganisationType = LocalCartageJobOrgTypeList.Codes.CNR;
			var divot1 = Helper.CreatePackageDivot(pic, package1, 1);
			var confirmation1 = Helper.CreateConfirmation(pic, ConfirmationTypes.Codes.PickUp);
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv, package1, 1);
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2eTestHelper.CO2Warning);

			// Act & Assert
			CO2eTestHelper.AssertHasCO2Warning(booking, () => dlv.Delete(), "Instruction package divot deleted");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => pic.KN_InstructionType = InstructionTypes.Codes.Delivery, "KN_InstructionType [PIC]->[DLV]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => pic.OrganisationType = LocalCartageJobOrgTypeList.Codes.CYD, "OrganisationType [CNR]->[CYD]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => pic.OrganisationType = LocalCartageJobOrgTypeList.Codes.CTO, "OrganisationType [CYD]->[CTO]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => pic.Address.E2_OA_Address = Helper.CreateOrganisation("ORG").MainAddress.PK, "Address");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_Weight = 800, "KP_Weight [1000]->[800]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_WeightUQ = Constants.Weight.Pounds, "KP_WeightUQ [KG]->[LB]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => package1.KP_PackageQty = 8, "KP_PackageQty [1]->[8]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => confirmation1.KK_IsEmptyContainer = true, "KK_IsEmptyContainer [N]->[Y]");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => booking.Instructions.AddNew(), "Instruction added");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => Helper.CreatePackageDivot(pic, package2, 1), $"Instruction package divot added");

			TestDateAttribute.AddMinutes(1);
			CO2eTestHelper.AssertHasCO2Warning(booking, () => divot1.Delete(), "Instruction package divot deleted");
		}

		public void TestOnCalculated_ParentIsCO2ePrePostCarriage()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyCO2ePrePostCarriageWithDtbBooking);
			var parent = Factory.New<DummyCO2ePrePostCarriageWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = (ICO2eCalculationSupporter)Helper.CreateBooking(consolidation);

			booking.OnCalculated(false);
			AssertEquals("OnTransportBookingCalculated is called on Parent once", 1, parent.OnTransportBookingCalculatedCount);

			booking.OnCalculated(true);
			AssertEquals("OnTransportBookingCalculated is called on Parent twice", 2, parent.OnTransportBookingCalculatedCount);
		}

		public void TestCO2eStatusChanged_ParentIsCO2ePrePostCarriage()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyCO2ePrePostCarriageWithDtbBooking);
			var parent = Factory.New<DummyCO2ePrePostCarriageWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = (ICO2eCalculationSupporter)Helper.CreateBooking(consolidation);

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
				AssertEquals("OnTransportBookingCO2eStatusChangedCount is called on Parent once", 1, parent.OnTransportBookingCO2eStatusChangedCount);

				booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				AssertEquals("OnTransportBookingCO2eStatusChangedCount is called on Parent twice", 2, parent.OnTransportBookingCO2eStatusChangedCount);
			}

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
				AssertEquals("OnTransportBookingCO2eStatusChangedCount is called on Parent twice", 2, parent.OnTransportBookingCO2eStatusChangedCount);

				booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				AssertEquals("OnTransportBookingCO2eStatusChangedCount is called on Parent twice", 2, parent.OnTransportBookingCO2eStatusChangedCount);
			}
		}

		public void TestActiveStatusChanged_ParentIsCO2ePrePostCarriage()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyCO2ePrePostCarriageWithDtbBooking);
			var parent = Factory.New<DummyCO2ePrePostCarriageWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidation);

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is not called when booking.IsCancelled remains false", 0, parent.OnTransportBookingActiveStatusChangedCount);

				booking.IsCancelled = true;
				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is called when booking.IsCancelled is changed from false to true", 1, parent.OnTransportBookingActiveStatusChangedCount);

				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is not called when booking.IsCancelled remains true", 1, parent.OnTransportBookingActiveStatusChangedCount);

				booking.IsCancelled = false;
				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is called when booking.IsCancelled is changed from true to false", 2, parent.OnTransportBookingActiveStatusChangedCount);

				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is not called when booking.IsCancelled remains false", 2, parent.OnTransportBookingActiveStatusChangedCount);
			}

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				booking.IsCancelled = true;
				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is not called when EnablePrePostCarriageCalculation is false", 2, parent.OnTransportBookingActiveStatusChangedCount);

				Factory.Save();
				AssertEquals("OnTransportBookingActiveStatusChangedCount is not called when booking.IsCancelled remains true", 2, parent.OnTransportBookingActiveStatusChangedCount);
			}
		}

		public void TestSaveEmissionsLogToNoteOnCalculated()
		{
			var booking = Helper.CreateBooking() as ICO2eCalculationSupporter;
			AssertEquals(false, booking.SaveEmissionsLogToNoteOnCalculated);
		}

		new TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = GetNewTestHelper()); }
		}

		TransportBookingTestHelper helper;

		new TransportBookingTestHelper GetNewTestHelper()
		{
			return new TransportBookingTestHelper(Factory);
		}
	}

	[TestedType(typeof(DtbBooking))]
	sealed class DtbBookingMasterBookingEntityTest : BaseIDtbMasterBookingEntityTest
	{
		protected override IEnumerable<SchemaColumn> ReplicatedColumns => DtbMasterBookingReplication.DtbBookingReplicatedColumns;

		protected override IEnumerable<SchemaColumn> NonReplicatedColumns => new SchemaColumn[]
		{
			DtbBookingSchema.PK,
			DtbBookingSchema.KM_SystemCreateUser,
			DtbBookingSchema.KM_SystemCreateBranch,
			DtbBookingSchema.KM_SystemCreateDepartment,
			DtbBookingSchema.KM_SystemCreateTimeUtc,
			DtbBookingSchema.KM_SystemLastEditUser,
			DtbBookingSchema.KM_SystemLastEditTimeUtc,
			DtbBookingSchema.KM_IsMaster,
			DtbBookingSchema.KM_MasterBookingVersion,
			DtbBookingSchema.KM_KM_MasterBooking,
			DtbBookingSchema.KM_KB_Booking,
			DtbBookingSchema.KM_KB_BookingConsolidationMultiJob,
			DtbBookingSchema.KM_JobID,
			DtbBookingSchema.KM_JobType,
			DtbBookingSchema.KM_IsHazardous,
			DtbBookingSchema.KM_RequiresRefrigeration,
			DtbBookingSchema.KM_Chargeable,
			DtbBookingSchema.KM_OverrideChargeable,
		};

		protected override ITableSchema tableSchema => DtbBookingSchema.Instance;

		public void TestMasterBookingUpdateToStatusUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_Status, BookingStatuses.Codes.Held);
		}

		public void TestMasterBookingUpdateToIsActiveUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_IsActive, false);
		}

		public void TestMasterBookingUpdateToTemplateUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_KT_NKBookingTemplate, "LC2C");
		}

		public void TestMasterBookingUpdateToDescriptionUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_Description, "Another description");
		}

		public void TestMasterBookingUpdateToDirectionUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_Direction, "DLV");
		}

		public void TestMasterBookingUpdateToServiceLevelUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_RS_NKServiceLevel, "D2D");
		}

		public void TestMasterBookingUpdateToCarrierServiceLevelUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_PL_NKCarrierServiceLevel, "STD");
		}

		public void TestMasterBookingUpdateToRatingFreightModeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_RatingFreightMode, "LSE");
		}

		public void TestMasterBookingUpdateToDistanceUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_Distance, 1);
		}

		public void TestMasterBookingUpdateToDistanceUnitUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_DistanceUnit, "MI");
		}

		public void TestMasterBookingUpdateToCarrierAccountUpdatesMasterBookingVersion()
		{
			var transportCo = Helper.CreateOrganisation("TRANSCO1");

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "CARRACC1";
			carrierAccount.OAN_OH_Carrier = transportCo.PK;
			Factory.Save();

			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_OAN_CarrierAccount, carrierAccount.PK);
		}

		public void TestMasterBookingUpdateToBranchUpdatesMasterBookingVersion()
		{
			var otherCompanyOrg = Helper.CreateOrganisation("ANOTHERCO1");
			var otherCompany = Helper.CreateCompany("CC1", "Another company", otherCompanyOrg);
			var otherBranch = Helper.CreateBranch("XX1", "Another branch", otherCompany, otherCompanyOrg);

			Factory.Save();

			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_GB_Branch, otherBranch.PK);
		}

		public void TestMasterBookingUpdateToBookingOfTransportRequestedDateUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_BookingOfTransportRequestedDate, ZDateTime.Now);
		}

		public void TestMasterBookingUpdateToIsAgentBookingUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_IsAgentBooking, true);
		}

		public void TestMasterBookingUpdateToTransportReferenceUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_TransportReference, "UpdatedRef");
		}

		public void TestMasterBookingUpdateToTransportModeUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingSchema.KM_TransportMode, "RAI");
		}

		public void TestMasterBookingUpdateToJobIDDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingSchema.KM_JobID, "XXX2");
		}

		public void TestMasterBookingUpdateToChargeableDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingSchema.KM_Chargeable, 1);
		}

		public void TestMasterBookingUpdateToIsHazardousDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingSchema.KM_IsHazardous, true);
		}

		public void TestMasterBookingUpdateToRequiresRefrigerationDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingSchema.KM_RequiresRefrigeration, true);
		}

		void CoreTestMasterBookingUpdateToReplicationFieldUpdatesMasterBookingVersion(SchemaColumn columnChanged, object updatedValue)
		{
			var booking = CreateBookingForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = booking.KM_MasterBookingVersion;

			booking[columnChanged] = updatedValue;
			var propertyInfo = booking.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: Booking.HasChanges is true", booking.HasChanges);
				Assert("Precondition: Booking." + columnChanged.Name + " value has changed", propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertGreaterThan("On a Master Booking, update to " + columnChanged.Name + " should update master booking version", booking.KM_MasterBookingVersion, previousMasterBookingVersion);
		}

		void CoreTestMasterBookingUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(SchemaColumn columnChanged, object updatedValue)
		{
			var booking = CreateBookingForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = booking.KM_MasterBookingVersion;

			if (columnChanged == DtbBookingSchema.KM_Chargeable)
			{
				booking.KM_OverrideChargeable = true;
			}
			booking[columnChanged] = updatedValue;
			if (columnChanged == DtbBookingSchema.KM_IsHazardous)
			{
				if ((bool)updatedValue)
				{
					booking.PackageJob.Packages.First().UNDGs.AddNew();
				}
			}
			else if (columnChanged == DtbBookingSchema.KM_RequiresRefrigeration)
			{
				if ((bool)updatedValue)
				{
					booking.PackageJob.Packages.First().KP_RequiresTemperatureControl = true;
				}
			}

			var propertyInfo = booking.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: Booking.HasChanges is true", booking.HasChanges);
				Assert("Precondition: Booking." + columnChanged.Name + " value has changed", propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertEquals("On a Master Booking, update to " + columnChanged.Name + " should not update master booking version", previousMasterBookingVersion, booking.KM_MasterBookingVersion);
		}

		public void TestNonMasterBookingUpdatesDoNotUpdateMasterBookingVersion()
		{
			var booking = CreateBookingForMasterBookingReplicationTests(false);

			AssertEquals("Precondition: Non-master booking should have KM_MasterBookingVersion of 0", (short)0, booking.KM_MasterBookingVersion);

			var otherCompanyOrg = Helper.CreateOrganisation("ANOTHERCO1");
			var otherCompany = Helper.CreateCompany("CC1", "Another company", otherCompanyOrg);
			var otherBranch = Helper.CreateBranch("XX1", "Another branch", otherCompany, otherCompanyOrg);
			var transportCo = Helper.CreateOrganisation("TRANSCO1");

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "CARRACC1";
			carrierAccount.OAN_OH_Carrier = transportCo.PK;
			Factory.Save();

			booking.KM_JobID = "XXX2";
			booking.KM_Status = BookingStatuses.Codes.Held;
			booking.KM_Direction = "DLV";
			booking.KM_Description = "New description";
			booking.KM_KT_NKBookingTemplate = "LC2C";
			booking.KM_RS_NKServiceLevel = "D2D";
			booking.KM_PL_NKCarrierServiceLevel = "STD";
			booking.KM_GB_Branch = otherBranch.PK;
			booking.KM_OAN_CarrierAccount = carrierAccount.PK;
			booking.KM_RatingFreightMode = "LSE";
			booking.KM_Distance = 1;
			booking.KM_DistanceUnit = "MI";
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;
			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 1;
			booking.KM_TransportReference = "NewRef";
			booking.KM_IsHazardous = true;
			booking.KM_RequiresRefrigeration = true;
			booking.PackageJob.Packages.First().KP_RequiresTemperatureControl = true;
			booking.PackageJob.Packages.First().UNDGs.AddNew();

			Assert("Precondition: Booking.HasChanges is true", booking.HasChanges);
			Factory.Save();

			AssertEquals("On a Non-master booking, any updates should leave master booking version at 0", (short)0, booking.KM_MasterBookingVersion);
		}

		DtbBooking CreateBookingForMasterBookingReplicationTests(bool isMaster)
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = isMaster;
			booking.KM_MasterBookingVersion = (short)(isMaster ? 1 : 0);
			var instruction = booking.Instructions.AddNew();
			instruction.KN_IsMaster = isMaster;
			instruction.KN_MasterBookingVersion = booking.KM_MasterBookingVersion;

			if (isMaster)
			{
				var subBooking = Helper.CreateBooking();
				subBooking.KM_KM_MasterBooking = booking.PK;
				subBooking.KM_MasterBookingVersion = booking.KM_MasterBookingVersion;
				var package = subBooking.PackageJob.Packages.AddNew();

				var assignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
				{
					(packagePK: package.PK, packageQty: package.KP_PackageQty),
				};

				AssignDivotsToInstructions(instruction, assignedPackages);

				Factory.Save();

				subBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = booking.ConsolidationSingleJob.PK;
				subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = booking.ConsolidationSingleJob.KB_MasterBookingVersion;
			}
			else
			{
				booking.PackageJob.Packages.AddNew();
			}

			Factory.Save();

			booking.KM_JobID = "XXX1";
			booking.KM_Status = BookingStatuses.Codes.Available;
			booking.KM_Direction = "PIC";
			booking.KM_Description = "Original description";
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_RS_NKServiceLevel = "STD";
			booking.KM_PL_NKCarrierServiceLevel = string.Empty;
			booking.KM_RatingFreightMode = string.Empty;
			booking.KM_Distance = 0;
			booking.KM_DistanceUnit = "KM";
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Empty;
			booking.KM_Chargeable = 0;
			booking.KM_OverrideChargeable = false;
			booking.KM_TransportReference = "OriginalRef";
			booking.KM_IsHazardous = false;
			booking.KM_RequiresRefrigeration = false;

			Factory.Save();

			return booking;
		}

		void AssignDivotsToInstructions(DtbBookingInstruction instruction, (ZGuid packagePK, ZInt packageQty)[] divotDetails)
		{
			instruction.PackageDivots.DeleteAll();
			foreach (var divotDetail in divotDetails)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = divotDetail.packagePK;
				packageDivot.KD_Quantity = divotDetail.packageQty;
			}
		}
	}

	[TestedType(typeof(DtbBooking))]
	sealed class DtbBookingWorkflowProviderTest : WorkflowProviderTest<DtbBooking, DtbBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingWorkflowDescriptorCode; }
		}

		public void TestGetTemplateSelectionCriteriaWithJobSet()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();

			var busObj = GetNewBusinessObject(Factory);
			var job = Helper.LoadOrCreateJobHeader(busObj);

			job.JH_OA_LocalChargesAddr = orgAddress.PK;

			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is CurrentBillingPartyOrLocalClientOrgPK (Org Header's PK)", orgHeader.PK, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		public void TestGetTemplateSelectionCriteriaWithJobSetWithoutLocalClientAndBillingAddressAndWithoutOrgHeader()
		{
			var busObj = GetNewBusinessObject(Factory);
			_ = busObj.BillingPartyAddress;
			busObj.Address.E2_OA_Address = ZGuid.Empty;
			busObj.Address.E2_AddressOverride = true;
			busObj.Address.E2_Address1 = "1 Street St";
			busObj.Address.E2_Postcode = "XXXX";
			busObj.Address.E2_City = "XX";
			busObj.Address.E2_State = "NSW";
			busObj.Address.E2_RN_NKCountryCode = "AU";
			var job = Helper.LoadOrCreateJobHeader(busObj);

			AssertEquals("Precondition - job local client address is empty", ZGuid.Empty, job.JH_OA_LocalChargesAddr);
			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is CurrentBillingPartyOrLocalClientOrgPK (Org Header's PK)", ZGuid.Empty, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		public void TestGetTemplateSelectionCriteriaWithBillingAddressWithoutOrgHeader()
		{
			var busObj = GetNewBusinessObject(Factory);
			_ = busObj.BillingPartyAddress;
			busObj.Address.E2_OA_Address = ZGuid.Empty;
			busObj.Address.E2_AddressOverride = true;
			busObj.Address.E2_Address1 = "1 Street St";
			busObj.Address.E2_Postcode = "XXXX";
			busObj.Address.E2_City = "XX";
			busObj.Address.E2_State = "NSW";
			busObj.Address.E2_RN_NKCountryCode = "AU";

			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is blank", ZGuid.Empty, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		public void TestGetTemplateSelectionCriteriaWithJobSetWithoutLocalClient()
		{
			var busObj = GetNewBusinessObject(Factory);
			var job = Helper.LoadOrCreateJobHeader(busObj);

			AssertEquals("Precondition - job local client address is blank", ZGuid.Empty, job.JH_OA_LocalChargesAddr);

			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is CurrentBillingPartyOrLocalClientOrgPK (Org Header's PK)", ZGuid.Empty, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		public void TestGetEmptyTemplateSelectionCriteria()
		{
			var busObj = GetNewBusinessObject(Factory);

			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is empty", ZGuid.Empty, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			var busObj = consolidation.Bookings.AddNew();
			busObj.BillingPartyOrLocalClientPK = orgAddress.PK;
			busObj.KM_TransportMode = Constants.TransportModes.Sea;
			busObj.KM_KT_NKBookingTemplate = template.KT_Code;

			var criteria = (ColumnValueRanker)((IWorkflowProviderCore)busObj).GetTemplateSelectionCriteria();

			AssertEquals("P0_OH_Client is CurrentBillingPartyOrLocalClientOrgPK (Org Header's PK)", orgHeader.PK, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("P0_SubType1 is Bookings Transport Mode", busObj.KM_TransportMode, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1)[0]);
			AssertEquals("P0_SubType2 is Bookings Job Direction", consolidation.KB_JobDirection, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2)[0]);
			AssertEquals("P0_SubType3 is Bookings Template Code", busObj.KM_KT_NKBookingTemplate, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType3)[0]);
		}

		public void TestGetWorkflowInformationProvider()
		{
			IWorkflowProvider booking = GetNewBusinessObject(Factory);
			AssertNull(booking.GetWorkflowInformationProvider());
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(DtbBooking))]
	class DtbBookingRelatableActivityTest : RelatableActivityTestCase<DtbBooking>
	{
		protected override DtbBooking GetNewActivity()
		{
			return Factory.NewWithValidTestData<DtbBooking>();
		}
	}

	sealed class DtbBookingFountainUniqueIndexFailureHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbBookingID; }
		}

		protected override Type BizOTypeToTest
		{
			get { return typeof(DtbBooking); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return DtbBookingSchema.KM_JobID; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var booking = (DtbBooking)testBizO;
			booking.KM_KB_Booking = BookingConsolidation.PK;
			booking.KM_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(DtbBookingSchema.Constants.KM_JobType, string.Format("'{0}'", BookingConsolidation.KB_JobType));
				result.Add(DtbBookingSchema.Constants.KM_KB_Booking, string.Format("'{0}'", BookingConsolidation.PK));
				result.Add(DtbBookingSchema.Constants.KM_GB_Branch, string.Format("'{0}'", GlbBranch.CurrentBranch.PK));
				return result;
			}
		}

		DtbBookingConsolidation BookingConsolidation
		{
			get { return bookingConsolidation ?? (bookingConsolidation = GetNewConsolidation()); }
		}

		DtbBookingConsolidation GetNewConsolidation()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		DtbBookingConsolidation bookingConsolidation;
	}

	sealed class DtbBookingNumberGenerationTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestTransportNumberGeneration()
		{
			var customisation1 = CreateCustomisations(clientCodedPrefix: "AA", includeService: true, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation1);
			AssertGeneratedJobIDFromFountain("", "AA000001");
			AssertGeneratedJobIDFromFountain("STD", "AASTD000002");
			AssertGeneratedJobIDFromFountain("D2D", "AAD2D000003");

			var customisation2 = CreateCustomisations(clientCodedPrefix: "BB", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation2);
			AssertGeneratedJobIDFromFountain("", "BB000001");
			AssertGeneratedJobIDFromFountain("STD", "BB000002");
			AssertGeneratedJobIDFromFountain("D2D", "BB000003");

			var customisation3 = CreateCustomisations(clientCodedPrefix: "", includeService: false, removeFountainPrefix: false);
			AddCustomisationToRegistry(customisation3);
			AssertGeneratedJobIDFromFountain("", "000001", usePrefix: true);
			AssertGeneratedJobIDFromFountain("STD", "000002", usePrefix: true);
			AssertGeneratedJobIDFromFountain("D2D", "000003", usePrefix: true);

			var customisation4 = CreateCustomisations(clientCodedPrefix: "TEST", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation4);

			Db.Connection.BeginTransaction();
			try
			{
				AssertEquals("00000001", Env.NumberFountains.GetDtbTransportGeneratorFountain("TEST").GetNextFormatted(Factory));
				AssertGeneratedJobIDFromFountain("", "TEST000002");
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[UseSnapshotProtection]
		public void TestTransportNumberGeneration_HandlesUnexpectedCollisions()
		{
			var customisation1 = CreateCustomisations(clientCodedPrefix: "AA", includeService: false, removeFountainPrefix: true);
			AddCustomisationToRegistry(customisation1);
			AssertGeneratedJobIDFromFountain("", "AA000001");

			var booking1 = GetSaveableTransportJob();
			booking1.KM_JobID = "AA000003"; // Hack to make job ID same as what the number fountain will hit next

			/* booking2 */
			AssertGeneratedJobIDFromFountain("", "AA000002");

			var booking3 = GetSaveableTransportJob();
			try
			{
				Factory.Save();
				Fail("Index exception should happen");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}

			Factory.Save();
			AssertEquals("Should bump up number fountain to fix collision.", booking3.KM_JobID, "AA000004");
		}

		[UseSnapshotProtection]
		public void TestTransportNumberGeneration_HandlesUnexpectedCollisions_NoCustomisation()
		{
			AssertGeneratedJobIDFromFountain("", "00000001", true);

			var booking1 = GetSaveableTransportJob();
			booking1.KM_JobID = string.Format("{0}00000003", FountainPrefixForTest); // Hack to make job ID same as what the number fountain will hit next

			/* booking2 */
			AssertGeneratedJobIDFromFountain("", "00000002", true);

			var booking3 = GetSaveableTransportJob();
			try
			{
				Factory.Save();
				Fail("Index exception should happen");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}

			Factory.Save();
			AssertEquals("Should bump up number fountain to fix collision.", booking3.KM_JobID, string.Format("{0}00000004", FountainPrefixForTest));
		}

		void AssertGeneratedJobIDFromFountain(string serviceLevel, string expect, bool usePrefix = false)
		{
			AssertGeneratedJobIDFromFountain("", serviceLevel, expect, usePrefix);
		}

		void AssertGeneratedJobIDFromFountain(string assertionMessage, string serviceLevel, string expect, bool usePrefix = false)
		{
			var booking = GetSaveableTransportJob();
			var prefix = usePrefix ? FountainPrefixForTest : ZString.Empty;
			booking.KM_RS_NKServiceLevel = serviceLevel;
			Factory.Save();
			AssertEquals(assertionMessage, prefix + expect, booking.KM_JobID);
		}

		BillOfLadingNumberCustomisationsByServiceLevel CreateCustomisations(string clientCodedPrefix, bool includeService, bool removeFountainPrefix)
		{
			var serviceCustomisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			foreach (BillOfLadingNumberCustomisation customisation in serviceCustomisations.BillOfLadingNumberCustomisations)
			{
				customisation.RemoveFountainPrefix = removeFountainPrefix;
				foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
				{
					switch (element.Key)
					{
						case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
							element.Include = true;
							element.Detail = "6";
							element.Order = 50;
							break;

						case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
							element.Include = !string.IsNullOrEmpty(clientCodedPrefix);
							element.Detail = clientCodedPrefix;
							element.Fountain = true;
							element.Order = 1;
							break;

						case BillOfLadingNumberCustomisationElement.Keys.ServiceLevel:
							element.Include = includeService;
							element.Order = 2;
							break;

						default:
							element.Include = false;
							break;
					}
				}
			}
			return serviceCustomisations;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		DtbBooking GetSaveableTransportJob()
		{
			return Helper.CreateBooking();
		}

		ZString FountainPrefixForTest
		{
			get { return "TB"; }
		}

		void AddCustomisationToRegistry(BillOfLadingNumberCustomisationsByServiceLevel customisation)
		{
			TransportRegistry.Instance.TransportBookingNumberFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}

	public class DtbBookingJobCostingPlugInTest : TestCaseWithFactory
	{
		public void TestDtbBookingJobCostingPlugInCostSupporter()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;

			AssertEquals("Booking as IJobCostingPlugIn Cost supporter should be DtbBookingJobCostSupporter", typeof(DtbBookingJobCostSupporter), bookingAsJobCostingPlugIn.CostSupporter.GetType());
		}

		public void TestDtbBookingJobCostingPlugInUniqueConsignRef()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;
			booking.KM_JobID = "TESTJOBID";

			AssertEquals("Booking as IJobCostingPlugIn Unique Consignment Ref should match booking job ID", booking.KM_JobID, bookingAsJobCostingPlugIn.JK_UniqueConsignRef);
		}

		public void TestDtbBookingJobCostingPlugInModule()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;

			AssertEquals("Booking as IJobCostingPlugIn Module should be ApportionmentMethodModules.TransportBooking", ApportionmentMethodModules.TransportBooking, bookingAsJobCostingPlugIn.Module);
		}

		public void TestDtbBookingJobCostingPlugInPrepaidCollectList()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;

			AssertEquals("Booking as IJobCostingPlugIn Prepaid Collect List should be empty", 0, bookingAsJobCostingPlugIn.PrepaidCollectList.Count);
		}

		public void TestDtbBookingJobCostingPlugInProfitLossContainer()
		{
			var booking = Helper.CreateBooking();

			AssertEquals("Booking as IJobCostingPlugIn ProfitLossContainer should be empty", 0, booking.ProfitLossContainer.Count);
		}

		public void TestDtbBookingJobCostingPlugInAddNewToLogs()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;

			bookingAsJobCostingPlugIn.AddNewToLogs(Events.CustomisableEvent00, "REF1");
			AssertEquals("Expecting 1 event logged", 1, booking.Logs.LogsNotInDB.Length);
			var log = booking.Logs.LogsNotInDB.Single();
			CombineAssertions("Checking log for correct values", () =>
			{
				AssertEquals("Log should have correct event code", Events.CustomisableEvent00, log.Event);
				AssertEquals("Log should have correct reference", "REF1", log.ReferenceFreeText);
			});
		}

		public void TestDtbBookingJobCostingPlugInEmptyOrNullProperties()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;

			CombineAssertions("All these empty/null tests should pass", () =>
			{
				AssertNull("Booking as IJobCostingPlugIn LoadPort should be null", bookingAsJobCostingPlugIn.LoadPort);
				AssertNull("Booking as IJobCostingPlugIn DischargePort should be null", bookingAsJobCostingPlugIn.DischargePort);
				AssertEquals("Booking as IJobCostingPlugIn ConsolExchangeRate should be zero", 0m, bookingAsJobCostingPlugIn.ConsolExchangeRate);
				AssertNull("Booking as IJobCostingPlugIn ConsolCurrency should be null", bookingAsJobCostingPlugIn.ConsolCurrency);
				Assert("Booking as IJobCostingPlugIn IsMasterCollect should be false", !bookingAsJobCostingPlugIn.IsMasterCollect);
				AssertNull("Booking as IJobCostingPlugIn ReceivingAgent should be null", bookingAsJobCostingPlugIn.ReceivingAgent);
				AssertNull("Booking as IJobCostingPlugIn ReceivingAgentAPInvoicingParty should be null", bookingAsJobCostingPlugIn.ReceivingAgentAPInvoicingParty);
				AssertNull("Booking as IJobCostingPlugIn ReceivingAgentARInvoicingParty should be null", bookingAsJobCostingPlugIn.ReceivingAgentARInvoicingParty);
				AssertNull("Booking as IJobCostingPlugIn SendingAgent should be null", bookingAsJobCostingPlugIn.SendingAgent);
				AssertNull("Booking as IJobCostingPlugIn SendingAgentAPInvoicingParty should be null", bookingAsJobCostingPlugIn.SendingAgentAPInvoicingParty);
				AssertNull("Booking as IJobCostingPlugIn SendingAgentARInvoicingParty should be null", bookingAsJobCostingPlugIn.SendingAgentARInvoicingParty);
				AssertEquals("Booking as IJobCostingPlugIn TransportMode should be empty", ZString.Empty, bookingAsJobCostingPlugIn.TransportMode);
				AssertEquals("Booking as IJobCostingPlugIn ContainerMode should be empty", ZString.Empty, bookingAsJobCostingPlugIn.ContainerMode);
				AssertEquals("Booking as IJobCostingPlugIn Direction should be empty", ZString.Empty, bookingAsJobCostingPlugIn.Direction);
				AssertEquals("Booking as IJobCostingPlugIn ConsolType should be empty", ZString.Empty, bookingAsJobCostingPlugIn.ConsolType);
			});
		}

		public void TestDtbBookingJobCostingPlugInExchangeRateForCurrency()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;
			var dummyRefCurrency = Factory.New<RefCurrency>();
			var dummyCurrentJobConsolCostPK = ZGuid.NewZGuid();

			var exchangeRateForCurrency = bookingAsJobCostingPlugIn.ExchangeRateForCurrency(dummyRefCurrency, dummyCurrentJobConsolCostPK);
			AssertEquals("Booking as IJobCostingPlugIn Exchange Rate for Currency should always return zero", 0m, exchangeRateForCurrency);
		}

		public void TestDtbBookingJobCostingPlugInGetPrepaidCollect()
		{
			var booking = Helper.CreateBooking();
			var bookingAsJobCostingPlugIn = (IJobCostingPlugIn)booking;
			var dummyApportionableJob = new Mock<IJobInvoicingPlugIn>().Object;

			var prepaidCollect = bookingAsJobCostingPlugIn.GetPrepaidCollect(dummyApportionableJob);
			AssertEquals("Booking as IJobCostingPlugIn GetPrepaidCollect() for Currency should always return empty string", ZString.Empty, prepaidCollect);
		}

		public void TestJobCostSupporterWhenBookingIsMaster()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = true;

			AssertEquals("CostSupporter should be DtbMasterBookingJobCostSupporter ", typeof(MasterBookingJobCostSupporter), ((IGenericJobCostPlugIn)booking).CostSupporter.GetType());
		}

		public void TestJobCostSupporterWhenBookingIsNotMaster()
		{
			var booking = Helper.CreateBooking();

			AssertEquals("Precondition: Booking is not a master booking", false, booking.KM_IsMaster);
			AssertEquals("CostSupporter should be DtbBookingConsolidationJobCostSupporter ", typeof(DtbBookingJobCostSupporter), ((IGenericJobCostPlugIn)booking).CostSupporter.GetType());
		}

		TransportBookingTestHelper Helper => helper ?? new TransportBookingTestHelper(Factory);
		readonly TransportBookingTestHelper helper;
	}
}
