using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbAgentBooking))]
	class DtbAgentBookingTest : DtbBookingBusinessObjectTestCase
	{
		public void TestBookingProperty_Null()
		{
			var agentBooking = Factory.New<DtbAgentBooking>();
			AssertNull(agentBooking.Booking);
		}

		public void TestBookingProperty()
		{
			var agentBooking = Factory.New<DtbAgentBooking>();
			var transportBooking = Factory.New<DtbBooking>();

			agentBooking.LTB_KM_TransportBooking = transportBooking.PK;
			AssertEquals(transportBooking, agentBooking.Booking);
		}

		public void TestOnFactorySaving_GenerateNextBookingAgentIdIfAlreadyUsed()
		{
			var consolidation1 = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation1);
			var agentBooking1 = Factory.New<DtbAgentBooking>();
			agentBooking1.LTB_KM_TransportBooking = booking1.PK;

			Factory.Save();

			AssertEquals("Precondition", true, agentBooking1.IsInDatabase);
			AssertEquals("Precondition", false, agentBooking1.LTB_JobID.IsEmpty);
			AssertEquals("After save it should populate with first number.", "AB00000001", agentBooking1.LTB_JobID);

			agentBooking1.LTB_JobID = "AB00000002"; // create a conflict with the next fountain value
			Factory.Save();
			AssertEquals("It should not populate the Job ID with the next value as it is already in the DB.", "AB00000002", agentBooking1.LTB_JobID);

			var consolidation2 = Helper.CreateConsolidation();
			var booking2 = Helper.CreateBooking(consolidation2);
			Factory.Save();

			var agentBooking2 = Factory.New<DtbAgentBooking>();
			agentBooking2.LTB_KM_TransportBooking = booking2.PK;

			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				saveFailed = true;
				AssertEquals("Agent Booking ID should remain populated if it failed to save, the Job ID will populate in the next save with the next fountain value.", "AB00000002", agentBooking2.LTB_JobID);
				ZExceptionReporting.HandleSaveException(ex); // simulate the form handling the exception
				ErrorReporter.Clear();
			}

			AssertEquals("User should be notified about the unique index conflict.", @"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.
Number Fountain: DtbAgentBookingID
Index: NR_UC__LTB_JobID
Value: DtbAgentBooking", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Save should have failed", saveFailed);
			UnitTestUserNotification.Instance.ClearMessages();

			Factory.Save();
			AssertEquals("Next Agent Booking ID should be given out.", "AB00000003", agentBooking2.LTB_JobID);
			AssertEquals("There should be no error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestNewFromBooking()
		{
			var transportBooking = Helper.CreateBooking(Helper.CreateConsolidation());
			AssertNull(transportBooking.AgentBooking);

			var newAgentBooking = DtbAgentBooking.NewFromBooking(transportBooking);
			AssertEquals(transportBooking.PK, newAgentBooking.LTB_KM_TransportBooking);
			AssertEquals(newAgentBooking.PK, transportBooking.ConsolidationSingleJob.KB_ParentID);
			AssertEquals(DtbAgentBookingSchema.Constants.Prefix, transportBooking.ConsolidationSingleJob.KB_ParentTableCode);
		}

		public void TestIsAuthorisedCarrierBookingAgent()
		{
			var cbas = new ZString[]
			{
				"3GTMS_EAD",
				"SMARTFREIGHT_EAD",
				"TEKNOWLOGI_EAD",
				"PIERBRIDGE_EAD",
				"TRINIUM_EAD",
				"SAASTRANS_EAD",
				"BLUME_EAD",
				DtbAgentBooking.ContainerTransportOptimizationCBA
			};

			foreach (var cba in cbas)
			{
				Assert(DtbAgentBooking.IsAuthorisedCarrierBookingAgent(cba));
			}
			AssertEquals(false, DtbAgentBooking.IsAuthorisedCarrierBookingAgent("Donald Duck"));
		}

		// interfaces

		public void TestIDtbBookingParent()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			Factory.Save();

			var bookingParent = (IDtbBookingParent)agentBooking;
			AssertEquals(false, bookingParent.IsSupportsDirectSchedule);
			AssertEquals(ZString.Empty, bookingParent.JobType);
			AssertEquals(ZString.Empty, bookingParent.JobTypeDescription);
			AssertEquals(0, bookingParent.GetSupportedDirections().Length);
			AssertEquals(agentBooking, bookingParent.InvoicingJob);
			AssertEquals(ZString.Empty, bookingParent.JobDescription);
			AssertEquals(ZString.Empty, bookingParent.JobStatus);
			AssertNull(bookingParent.ControllerID);
			AssertEquals(bookingParent.PK.ToGuid(), bookingParent.BusinessObjectPK);
			AssertEquals(agentBooking.LTB_JobID, bookingParent.JobNumber);
		}

		public void TestCanCreateTransportBooking()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			var bookingParent = (IDtbBookingParent)agentBooking;
			AssertEquals("CanCreateTransportBooking should always return true", true, bookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			var bookingParent = (IDtbBookingParent)agentBooking;
			AssertEquals("BookingParentPK should be the Agent Booking PK.", agentBooking.PK, bookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			var bookingParent = (IDtbBookingParent)agentBooking;
			AssertEquals("BookingParentTablePrefix should be the Agent Booking table prefix.", agentBooking.TablePrefix, bookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfrimMessageBeforCreateTransportBooking()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			var bookingParent = (IDtbBookingParent)agentBooking;

			var (isShouldShow, caption, message, confirmation) = bookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("GetExtendingConfrimMessageBeforCreateTransportBooking should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		public void TestIJobInvoicingPlugIn()
		{
			var agentBooking = Factory.New<DtbAgentBooking>();
			IJobInvoicingPlugIn plugIn = agentBooking;
			AssertType<DtbAgentBookingInvoicingSupporter>(plugIn.InvoicingSupporter);
			AssertEquals(true, plugIn.AllowInvoiceDeletion);
			AssertEquals("", agentBooking.LTB_JobID);

			plugIn.SetJobNumberFieldOnSaving();
			AssertEquals("AB00000001", agentBooking.LTB_JobID);
		}

		public void TestIJobNumber()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			Factory.Save();

			var jobNumber = (IJobNumber)agentBooking;
			AssertEquals(booking.KM_JobID, jobNumber.JobNumber);
		}
	}

	[TestedType(typeof(DtbAgentBooking))]
	class AgentBookingParentTest : IDtbBookingParentTestCase<DtbAgentBooking>
	{
		protected override DtbAgentBooking GetNewParent()
		{
			var booking = (DtbBooking)Helper.CreateBooking();
			return DtbAgentBooking.NewFromBooking(booking);
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking() => false;

		protected override bool CanHaveDirectCartageChild => false;
	}

	class AgentBookingIDFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override SchemaColumn ColumnThatUsesNumberFountain => DtbAgentBookingSchema.LTB_JobID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.DtbAgentBookingID;

		protected override Type BizOTypeToTest => typeof(DtbAgentBooking);

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var helper = new TransportBookingTestHelper(Factory);
			var agentBooking = (DtbAgentBooking)testBizO;
			agentBooking.LTB_KM_TransportBooking = helper.CreateBooking(helper.CreateConsolidation()).PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var factory = new BusinessObjectFactory();
				var helper = new TransportBookingTestHelper(factory);
				var bookingPK = helper.CreateBooking(helper.CreateConsolidation()).PK;
				factory.Save();

				var valueCollection = base.AdditionalInsertValues;
				valueCollection.Add(DtbAgentBookingSchema.Constants.LTB_KM_TransportBooking, string.Format(Culture.Invariant, "'{0}'", bookingPK));
				return valueCollection;
			}
		}
	}
}
