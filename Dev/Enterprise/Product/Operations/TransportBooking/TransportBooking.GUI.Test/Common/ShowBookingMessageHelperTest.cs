using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using MessageOperation = Enterprise.TransportBookings.GUI.ShowBookingMessageHelper.MessageOperation;

namespace Enterprise.TransportBookings.GUI.Testing
{
	class ShowBookingMessageHelperTest : DtbBookingTestCaseWithFactory
	{
		public void TestDoesBookingHaveConsolApportionedChargesForCurrentJob()
		{
			var shipment1 = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var shipment2 = Helper.CreateForwardingShipment("S02", "HSB", "SEA", "FCL");
			var consolidation1 = Helper.CreateConsolidation((IDtbBookingParent)shipment1); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var consolidation2 = Helper.CreateConsolidation((IDtbBookingParent)shipment2); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking1 = Helper.CreateBooking(consolidation1);
			var booking2 = Helper.CreateBooking(consolidation2);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);
			// Don't create a charge against jobHeader2, which is associated with booking2

			var result1 = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(Factory, multiJobConsolidation, jobHeader1);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be true for this job header", true, result1);

			var result2 = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(Factory, multiJobConsolidation, jobHeader2);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be false for this job header, because no charge was created against it", false, result2);
		}

		public void TestDoesBookingHaveConsolApportionedChargesForCurrentJob_FetchOnlyFromLocalCache()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var multiJobConsolidationOtherFactory = otherFactory.Load<DtbBookingConsolidation>(multiJobConsolidation.PK);
			var jobHeaderOtherFactory = otherFactory.Load<JobHeader>(jobHeader.PK);

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(otherFactory, multiJobConsolidationOtherFactory, jobHeaderOtherFactory);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be false because of FetchOnlyFromLocalCache", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesForCurrentJob_ConsolCostForDifferentConsol_ReturnsFalse()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);
			var otherMultiJobConsolidation = Helper.CreateConsolidationMultiJob();

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			// Create a consol cost against otherMultiJobConsolidation, rather than multiJobConsolidation
			var consolCost = (BusinessObject)Helper.CreateConsolCost(otherMultiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(Factory, multiJobConsolidation, jobHeader);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be false because the charge is for a different consol", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesForCurrentJob_ChargeIsNotAgainstConsol_ReturnsFalse()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			// Don't create a cost (JobConsolCost)
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, ZGuid.Empty, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(Factory, multiJobConsolidation, jobHeader);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be false because the charge is not for the multiJobConsolidation", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesForCurrentJob_SomeChargesAgainstConsol_ReturnsTrue()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var accChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost1 = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode1, 100, 100);
			// Don't create consolCost2 with accChargeCode2
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader, accChargeCode1, consolCost1.PK, 100, 100);
			var charge2 = Helper.CreateCharge(jobHeader, accChargeCode2, ZGuid.Empty, 200, 200);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesForCurrentJob(Factory, multiJobConsolidation, jobHeader);
			AssertEquals("DoesBookingHaveConsolApportionedChargesForCurrentJob should be true because charge1 is for the multiJobConsolidation", true, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesInDB_JobHeaderRefersToShipment_ReturnsTrue()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be true even though the job header refers to a shipment rather than a booking", true, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesInDB_JobHeaderRefersToBooking_NoJobConsolCost_ReturnsFalse()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, booking2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);
			// Don't create a charge against jobHeader2, which is associated with booking2

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking2);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be false as no charge has been created against jobHeader2", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesInDB_ConsolCostForDifferentConsol_ReturnsFalse()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);
			var otherMultiJobConsolidation = Helper.CreateConsolidationMultiJob();

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			// Create a consol cost against otherMultiJobConsolidation, rather than multiJobConsolidation
			var consolCost = (BusinessObject)Helper.CreateConsolCost(otherMultiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be false as the charge is against another consol", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesInDB_ChargeIsNotAgainstConsol_ReturnsFalse()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			// Don't create a cost (JobConsolCost)
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, ZGuid.Empty, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be false because the charge is not for the multiJobConsolidation", false, result);
		}

		public void TestDoesBookingHaveConsolApportionedChargesInDB_SomeChargesAgainstConsol_ReturnsTrue()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			var accChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost1 = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode1, 100, 100);
			// Don't create consolCost2 with accChargeCode2
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader, accChargeCode1, consolCost1.PK, 100, 100);
			var charge2 = Helper.CreateCharge(jobHeader, accChargeCode2, ZGuid.Empty, 200, 200);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be true because charge1 is for the multiJobConsolidation", true, result);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDoesBookingHaveConsolApportionedChargesInDB_ApportionedChargeInAnotherCompany_ReturnsTrue()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			// Create an apportioned charge for booking2 in another company
			TransportBookingsModuleButtonGridTest.CreateConsolCostAndApportionToBookingInAnotherCompany(booking2.PK);
			AssertNull("Precondition: booking2 does not have a job in the current environment, because it's from another company", booking2.Job);
			var result = ShowBookingMessageHelper.DoesBookingHaveConsolApportionedChargesInDB(Factory, multiJobConsolidation, booking2);
			AssertEquals("DoesBookingHaveConsolApportionedChargesInDB should be true because it detects the apportioned charge even though it's in another company", true, result);
		}

		public void TestGetCannotDetachMessage_BookingsLackMultiJobConsolidation_ReturnsEmpty()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);
			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			var result = ShowBookingMessageHelper.GetCannotDetachMessage(masterBooking.SubBookings);
			AssertEquals("GetCannotDetachMessage should not throw an exception and instead return an empty string when there is no MultiJobConsolidation", ZString.Empty, result);
		}

		public void TestGetCannotDetachFromMasterBookingMessage_AllBookingsInSync()
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

			subBooking1.KM_MasterBookingVersion = subBooking2.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = subBooking2.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			Factory.Save();

			var result = ShowBookingMessageHelper.GetCannotDetachFromMasterBookingMessage(new[] { subBooking1, subBooking2 }, masterBooking);
			AssertEquals("If all sub bookings are in sync with master booking GetCannotDetachFromMasterBookingMessage() should return empty string", string.Empty, result);
		}

		public void TestGetCannotDetachFromMasterBookingMessage_SomeBookingsNotInSync()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();
			Factory.Save();
			subBooking1.KM_JobID = "Sub1";
			subBooking2.KM_JobID = "Sub2";
			subBooking3.KM_JobID = "Sub3";

			masterBooking.SubBookings.Add(subBooking1);
			masterBooking.SubBookings.Add(subBooking2);
			masterBooking.SubBookings.Add(subBooking3);
			Factory.Save();

			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			CombineAssertions("Preconditions - checking subBooking2 and subBooking3 are not in sync with masterBooking", () =>
			{
				AssertNotEquals("Precondition: subBooking2 not in sync with master", subBooking2.KM_MasterBookingVersion, masterBooking.KM_MasterBookingVersion);
				AssertNotEquals("Precondition: subBooking3 not in sync with master", subBooking3.KM_MasterBookingVersion, masterBooking.KM_MasterBookingVersion);
			});

			var result = ShowBookingMessageHelper.GetCannotDetachFromMasterBookingMessage(new[] { subBooking1, subBooking2, subBooking3 }, masterBooking);
			var expectedMessage = FormattableString.Invariant($@"Cannot detach the transport booking(s) {subBooking2.KM_JobID}, {subBooking3.KM_JobID} from the {masterBooking.HumanReadableName} as they are not yet in sync.
Please reload the form and try again.");
			AssertEquals("If some sub bookings are in sync with master booking GetCannotDetachFromMasterBookingMessage() should return message with not in sync sub bookings mentioned", expectedMessage, result);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesForCurrentJob()
		{
			var shipment1 = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var shipment2 = Helper.CreateForwardingShipment("S02", "HSB", "SEA", "FCL");
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			var booking1 = Helper.CreateBooking();
			booking1.KM_KM_MasterBooking = masterBooking.PK;
			var booking2 = Helper.CreateBooking();
			booking2.KM_KM_MasterBooking = masterBooking.PK;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(masterBooking, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);

			var result1 = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(Factory, masterBooking, jobHeader1);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesForCurrentJob should be true for this job header", true, result1);

			var result2 = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(Factory, masterBooking, jobHeader2);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesForCurrentJob should be false for this job header, because no charge was created against it", false, result2);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesForCurrentJob_FetchOnlyFromLocalCache()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking.PK;
			booking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(masterBooking, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreate();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var masterBookingOtherFactory = otherFactory.Load<DtbBooking>(masterBooking.PK);
			var jobHeaderOtherFactory = otherFactory.Load<JobHeader>(jobHeader.PK);

			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(otherFactory, masterBookingOtherFactory, jobHeaderOtherFactory);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesForCurrentJob should be return false because of FetchOnlyFromLocalCache", false, result);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesForCurrentJob_ConsolCostForDifferentMasterBooking_Characterisation()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking.PK;
			booking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			var otherMasterBooking = Helper.CreateBooking();
			otherMasterBooking.KM_IsMaster = true;
			otherMasterBooking.KM_MasterBookingVersion = 1;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(otherMasterBooking, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreate();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(Factory, masterBooking, jobHeader);
			AssertEquals("Characterisation test: DoesBookingHaveMasterBookingApportionedChargesForCurrentJob checks whether the charge is associated with the Master Booking", false, result);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesInDB_JobHeaderRefersToShipment_ReturnsTrue()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking.PK;
			booking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(masterBooking, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreate();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesInDB(Factory, masterBooking, booking);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesInDB should be true even though the job header refers to a shipment rather than a booking", true, result);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesInDB_JobHeaderRefersToBooking_NoJobConsolCost_ReturnsFalse()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking1.KM_KM_MasterBooking = masterBooking.PK;
			booking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			booking2.KM_KM_MasterBooking = masterBooking.PK;
			booking2.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(masterBooking, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreate();
			var jobHeader2 = new JobHeader.Loader(Factory, booking2).TryCreate();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesInDB(Factory, masterBooking, booking2);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesInDB should be false as no charge has been created against jobHeader2", false, result);
		}

		public void TestDoesBookingHaveMasterBookingApportionedChargesInDB_ConsolCostForDifferentMasterBooking_ReturnsFalse()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking.KM_KM_MasterBooking = masterBooking.PK;
			booking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			var otherMasterBooking = Helper.CreateBooking();
			otherMasterBooking.KM_IsMaster = true;
			otherMasterBooking.KM_MasterBookingVersion = 1;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(otherMasterBooking, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreate();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesInDB(Factory, masterBooking, booking);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesInDB should be false as the charge is against another Master Booking", false, result);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDoesBookingHaveMasterBookingApportionedChargesInDB_ApportionedChargeInAnotherCompany_ReturnsTrue()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			booking1.KM_KM_MasterBooking = masterBooking.PK;
			booking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			booking2.KM_KM_MasterBooking = masterBooking.PK;
			booking2.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(masterBooking, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreate();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			TransportBookingsModuleButtonGridTest.CreateConsolCostAndApportionToBookingAttachedToMasterBookingInAnotherCompany(booking2.PK);
			AssertNull("Precondition: booking2 does not have a job in the current environment, because it's from another company", booking2.Job);
			var result = ShowBookingMessageHelper.DoesBookingHaveMasterBookingApportionedChargesInDB(Factory, masterBooking, booking2);
			AssertEquals("DoesBookingHaveMasterBookingApportionedChargesInDB should be true because it detects the apportioned charge even though it's in another company", true, result);
		}

		public void TestGetCannotDetachFromMasterBookingMessage_SkipsCheckIfNotOriginallyAttached()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var otherBooking = Helper.CreateBooking();
			Factory.Save();

			otherBooking.KM_KM_MasterBooking = masterBooking.PK;
			otherBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;

			var result = ShowBookingMessageHelper.GetCannotDetachFromMasterBookingMessage(new[] { otherBooking }, masterBooking);
			AssertEquals("If sub booking last saved state was not originally attached to master then skip check for in sync and return no message", string.Empty, result);
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationCreateWhenBookingFormOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndNoLoggerProvided(
				MessageOperation.Create,
				@"Cannot create a new Transport Booking as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationDeliverWhenBookingFormOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndNoLoggerProvided(
				MessageOperation.Deliver,
				@"Cannot prepare new Cartage Advice as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationOpenWhenBookingFormOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndNoLoggerProvided(
				MessageOperation.Open,
						@"A Transport Booking screen belonging to the same consolidation is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationCreateWhenMultipleBookingFormsOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndNoLoggerProvided(
				MessageOperation.Create,
				@"Cannot create a new Transport Booking as the existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationDeliverWhenMultipleBookingFormsOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndNoLoggerProvided(
				MessageOperation.Deliver,
				@"Cannot prepare new Cartage Advices as existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationOpenWhenMultipleBookingFormsOpenAndNoLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndNoLoggerProvided(
				MessageOperation.Open,
						@"More than one Transport Booking screen belonging to the same Consolidation is open.

Close the Transport Booking screens and try again.");
		}

		void CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndNoLoggerProvided(MessageOperation operation, string expectedMessage)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var form = new TransportBookingFormForTest(booking))
			{
				OpenedFormCache.GetInstance().Add(booking.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				form.Show();
				var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, operation);
				CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation open in a form with MessageOperation." + operation.ToString(), () =>
				{
					Assert("Should return true if booking under consolidation is open in a form", result);
					Assert("Should display correct error message if booking under consolidation is open in a form",
						UnitTestUserNotification.Instance.LastMessage.Contains(expectedMessage));
				});
			}
		}

		void CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndNoLoggerProvided(MessageOperation operation, string expectedMessage)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var form1 = new TransportBookingFormForTest(booking1))
			using (var form2 = new TransportBookingFormForTest(booking2))
			{
				OpenedFormCache.GetInstance().Add(booking1.PK.ToGuid(), form1, nameof(ControllerIDs.DtbBooking));
				OpenedFormCache.GetInstance().Add(booking2.PK.ToGuid(), form2, nameof(ControllerIDs.DtbBooking));
				form1.Show();
				form2.Show();
				var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, operation);
				CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation open in multiple forms with MessageOperation." + operation.ToString(), () =>
				{
					Assert("Should return true if booking under consolidation is open in a form", result);
					Assert("Should display correct error message if booking under consolidation is open in a form",
						UnitTestUserNotification.Instance.LastMessage.Contains(expectedMessage));
				});
			}
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationCreateWhenBookingFormOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndLoggerProvided(
				MessageOperation.Create,
				@"Cannot create a new Transport Booking as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationDeliverWhenBookingFormOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndLoggerProvided(
				MessageOperation.Deliver,
				@"Cannot prepare new Cartage Advice as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationOpenWhenBookingFormOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndLoggerProvided(
				MessageOperation.Open,
				@"A Transport Booking screen belonging to the same consolidation is open.

Close the Transport Booking screen and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationCreateWhenMultipleBookingFormsOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndLoggerProvided(
				MessageOperation.Create,
				@"Cannot create a new Transport Booking as the existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationDeliverWhenMultipleBookingFormsOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndLoggerProvided(
				MessageOperation.Deliver,
				@"Cannot prepare new Cartage Advices as existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
		}

		public void TestShowErrorIfBookingFormOpenWithMessageOperationOpenWhenMultipleBookingFormsOpenAndLoggerProvided()
		{
			CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndLoggerProvided(
				MessageOperation.Open,
						@"More than one Transport Booking screen belonging to the same Consolidation is open.

Close the Transport Booking screens and try again.");
		}

		void CoreTestShowErrorIfBookingFormOpenWhenBookingFormOpenAndLoggerProvided(MessageOperation operation, string expectedMessage)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var logger = new TestServiceLogger();

			using (var form = new TransportBookingForm(booking))
			{
				OpenedFormCache.GetInstance().Add(booking.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				form.Show();
				var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, logger, operation);
				CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation open in a form with MessageOperation." + operation.ToString(), () =>
				{
					Assert("Should return true if booking under consolidation is open in multiple forms", result);
					Assert("Should have no global messages if booking under consolidation is open in a form", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Close the Transport Booking screen"));
					AssertEquals(
						"Should log correct error message if booking under consolidation is open in a form",
						"Error|" + expectedMessage + System.Environment.NewLine,
						logger.ToString());
				});
			}
		}

		void CoreTestShowErrorIfBookingFormOpenWhenMultipleBookingFormsOpenAndLoggerProvided(MessageOperation operation, string expectedMessage)
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var logger = new TestServiceLogger();

			using (var form1 = new TransportBookingForm(booking1))
			using (var form2 = new TransportBookingForm(booking2))
			{
				OpenedFormCache.GetInstance().Add(booking1.PK.ToGuid(), form1, nameof(ControllerIDs.DtbBooking));
				OpenedFormCache.GetInstance().Add(booking2.PK.ToGuid(), form2, nameof(ControllerIDs.DtbBooking));
				form1.Show();
				form2.Show();
				var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, logger, operation);
				CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation open in multiple forms with MessageOperation." + operation.ToString(), () =>
				{
					Assert("Should return true if booking under consolidation is open in a form", result);
					Assert("Should have no global messages if booking under consolidation is open in multiple forms", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Close the Transport Booking screen"));
					AssertEquals(
						"Should log correct error message if booking under consolidation is open in multiple forms",
						"Error|" + expectedMessage + System.Environment.NewLine,
						logger.ToString());
				});
			}
		}

		public void TestShowErrorIfBookingFormOpenWhenNoBookingFormOpenAndNoLoggerProvided()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, MessageOperation.Create);
			CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation not open in a form", () =>
			{
				Assert("Should return false if booking under consolidation is not open in a form", !result);
				Assert("Should have no global messages if booking under consolidation is not open in a form", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Close the Transport Booking screen"));
			});
		}

		public void TestShowErrorIfBookingFormOpenWhenNoBookingFormOpenAndLoggerProvided()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var logger = new TestServiceLogger();

			var result = ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, logger, MessageOperation.Create);
			CombineAssertions("Correct results for ShowErrorIfBookingFormOpen() when consolidation not open in a form", () =>
			{
				Assert("Should return false if booking under consolidation is not open in a form", !result);
				Assert("Should have no global messages if booking under consolidation is not open in a form", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Close the Transport Booking screen"));
				AssertEquals("Should have no logged messages if booking under consolidation is not open in a form", string.Empty, logger.ToString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
		}
	}
}
