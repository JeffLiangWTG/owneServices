using System;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingLinerAndAgencyBooking))]
	sealed class TrackingLinerAndAgencyBookingBOTest : BookingBOTest
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.AgencyShipment);
			}
		}

		#endregion

		#region Milestones

		public void TestMilestones()
		{
			var testLinerAndAgencyBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
			AssertNotNull(testLinerAndAgencyBooking.Milestones);
			AssertEquals(0, testLinerAndAgencyBooking.Milestones.Count);

			var milestone1 = testLinerAndAgencyBooking.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testLinerAndAgencyBooking.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testLinerAndAgencyBooking.Factory.Save();
			AssertEquals(0, testLinerAndAgencyBooking.Milestones.Count);

			testLinerAndAgencyBooking.ReloadMilestones();
			AssertEquals(2, testLinerAndAgencyBooking.Milestones.Count);
		}

		#endregion

		[HttpContextEnabledTest]
		public void TestAutoCreatedLogDefaultSL_Reference()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			var testLinerAndAgencyBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();

			AssertEquals(helper.TestSiteUser.ContactAndCompanyReference, testLinerAndAgencyBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestShowAgentNotes()
		{
			var testLinerAndAgencyBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
			AssertEquals("Should not show Agent Notes", false, testLinerAndAgencyBooking.ShowAgentNotes);
		}

		public void TestNotesHelper()
		{
			var testLinerAndAgencyBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
			testLinerAndAgencyBooking.UserEditableNoteHelper.EditableNoteText = "Test Detailed Goods Description";
			Factory.Save();
			AssertEquals("Test Detailed Goods Description", testLinerAndAgencyBooking.UserEditableNoteHelper.EditableNoteText);
		}
	}
}
