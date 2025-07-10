using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	public abstract class ITrackingEventsProviderTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestTrackingEvents()
		{
			AssertNotNull(TestEventsProvider);
			AssertNotNull(TestEventsProvider.TrackingEvents);
			AssertNotNull(SiteUser);
			AssertEquals(true, SiteUser.IsLoggedIn);

			WebDataRegistry.Instance.EventVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new EventVisibilityCollection());
			WebDataRegistry.Instance.EventIncludeEstimates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.EventIncludeRelated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.Chronological);

			ChangeSecurityRight(SiteUser, WebSecurityRightsList.WebEventsView, false);
			AssertEquals(false, SiteUser.CanViewEvents);
			AssertEquals(0, TestEventsProvider.TrackingEvents.Count);

			ChangeSecurityRight(SiteUser, WebSecurityRightsList.WebEventsView, true);
			AssertEquals(true, SiteUser.CanViewEvents);
			AssertEquals(0, TestEventsProvider.TrackingEvents.Count);

			var related = TestEventsProvider.BusinessObjectsWithRelatedEvents;
			StmALog relatedEvent = null;
			if (related.Length > 0)
			{
				relatedEvent = ((IStmALogParent)related[0]).Logs.AddNew();
				using (relatedEvent.LockForUpdatingKeyFieldsForTesting())
				{
					relatedEvent.SL_SE_NKEvent = AutoEvents.DeliveredCode;
					relatedEvent.SL_EventTime = ZDateTime.Now.AddDays(-1);
				}
			}

			StmALog estimatedEvent = TestEventsProvider.Logs.AddNew();
			using (estimatedEvent.LockForUpdatingKeyFieldsForTesting())
			{
				estimatedEvent.SL_SE_NKEvent = AutoEvents.CustomsClearedCode;
				estimatedEvent.SL_EventTime = ZDateTime.Now.AddDays(-2);
				estimatedEvent.SL_IsEstimate = true;
			}

			StmALog hiddenEvent = TestEventsProvider.Logs.AddNew();
			using (hiddenEvent.LockForUpdatingKeyFieldsForTesting())
			{
				hiddenEvent.SL_SE_NKEvent = AutoEvents.DepartureCode;
				hiddenEvent.SL_EventTime = ZDateTime.Now.AddDays(-3);
			}

			StmALog event1 = TestEventsProvider.Logs.AddNew();
			using (event1.LockForUpdatingKeyFieldsForTesting())
			{
				event1.SL_SE_NKEvent = AutoEvents.SecurityModifiedCode;
				event1.SL_EventTime = ZDateTime.Now.AddDays(-10);
			}

			StmALog event2 = TestEventsProvider.Logs.AddNew();
			using (event2.LockForUpdatingKeyFieldsForTesting())
			{
				event2.SL_SE_NKEvent = AutoEvents.EmailSentCode;
				event2.SL_EventTime = ZDateTime.Now.AddDays(-9);
			}

			StmALog event3 = TestEventsProvider.Logs.AddNew();
			using (event3.LockForUpdatingKeyFieldsForTesting())
			{
				event3.SL_SE_NKEvent = AutoEvents.ArrivalCode;
				event3.SL_EventTime = ZDateTime.Now;
			}

			Factory.Save();
			AssertEquals(0, TestEventsProvider.TrackingEvents.Count);

			var eventVisibility = new EventVisibilityCollection();
			eventVisibility.AddNew(AutoEvents.CustomsClearedCode);
			eventVisibility.AddNew(AutoEvents.SecurityModifiedCode);
			eventVisibility.AddNew(AutoEvents.EmailSentCode);
			eventVisibility.AddNew(AutoEvents.DeliveredCode);
			eventVisibility.AddNew(AutoEvents.ArrivalCode);
			WebDataRegistry.Instance.EventVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eventVisibility);
			AssertEquals(3, TestEventsProvider.TrackingEvents.Count);
			AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
			AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
			AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);

			event3.Cancel();
			AssertEquals(2, TestEventsProvider.TrackingEvents.Count);

			event3.Reactivate();
			AssertEquals(3, TestEventsProvider.TrackingEvents.Count);

			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.ReversedChronological);
			AssertEquals(3, TestEventsProvider.TrackingEvents.Count);
			AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
			AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
			AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);

			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.Chronological);
			WebDataRegistry.Instance.EventIncludeEstimates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(4, TestEventsProvider.TrackingEvents.Count);
			AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
			AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
			AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
			AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);

			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.ReversedChronological);
			AssertEquals(4, TestEventsProvider.TrackingEvents.Count);
			AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
			AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
			AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
			AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);

			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.Chronological);
			WebDataRegistry.Instance.EventIncludeRelated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			if (relatedEvent == null)
			{
				AssertEquals(4, TestEventsProvider.TrackingEvents.Count);
				AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
				AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
				AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
				AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);

				WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.ReversedChronological);
				AssertEquals(4, TestEventsProvider.TrackingEvents.Count);
				AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
				AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
				AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
				AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);
			}
			else
			{
				AssertEquals(5, TestEventsProvider.TrackingEvents.Count);
				AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
				AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
				AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
				AssertEquals(relatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);
				AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[4].SL_SE_NKEvent);

				WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EventSortOrderList.Codes.ReversedChronological);
				AssertEquals(5, TestEventsProvider.TrackingEvents.Count);
				AssertEquals(event3.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[0].SL_SE_NKEvent);
				AssertEquals(relatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[1].SL_SE_NKEvent);
				AssertEquals(estimatedEvent.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[2].SL_SE_NKEvent);
				AssertEquals(event2.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[3].SL_SE_NKEvent);
				AssertEquals(event1.SL_SE_NKEvent, TestEventsProvider.TrackingEvents[4].SL_SE_NKEvent);
			}

			ChangeSecurityRight(SiteUser, WebSecurityRightsList.WebEventsView, false);
			AssertEquals(false, SiteUser.CanViewEvents);
			AssertEquals(0, TestEventsProvider.TrackingEvents.Count);
		}

		public void TestCanViewTrackingEvents()
		{
			AssertNotNull(TestEventsProvider);
			AssertNotNull(SiteUser);
			AssertEquals(true, SiteUser.IsLoggedIn);

			ChangeSecurityRight(SiteUser, WebSecurityRightsList.WebEventsView, false);
			AssertEquals(false, TestEventsProvider.CanViewTrackingEvents);

			ChangeSecurityRight(SiteUser, WebSecurityRightsList.WebEventsView, true);
			AssertEquals(true, TestEventsProvider.CanViewTrackingEvents);
		}

		#endregion

		#region Implementation

		protected void ChangeSecurityRight(TrackingSiteUser siteUser, WebSecurityRight right, bool isGranted)
		{
			foreach (OrgSecurityContacts userRight in siteUser.LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (userRight.Security.OX_SecurityItemName == right.Code)
				{
					userRight.OZ_Granted = isGranted;
					break;
				}
			}
			siteUser.OnSecurityRightsChangedForTest();
			AssertEquals(isGranted, siteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(right));
		}

		protected TrackingSiteUser SiteUser
		{
			get
			{
				if (siteUser == null)
				{
					var client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_IsActive = true;
					client.MainAddress.OA_Address1 = "123 Main Street";
					client.MainAddress.OA_City = "Chicago";
					client.MainAddress.OA_PostCode = "60077";
					client.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
					var countryData = Factory.New<OrgCountryData>();
					countryData.OV_OA_ApprovedLocation = client.MainAddress.PK;
					countryData.OV_EXApprovedOrMajorExporter = "YES";
					countryData.OV_OH_OrgHeader = client.PK;
					var clientContact = client.Contacts.AddNew();
					clientContact.OC_Email = "test@test.com";
					clientContact.SetHashedPassword("12345");
					clientContact.OC_WebAccessEnabled = true;
					Factory.Save();
					siteUser = new TrackingSiteUser();
					siteUser.Login(clientContact.OrganisationCode, clientContact.OC_Email, clientContact.PasswordForTesting);
					AssertEquals("Precondition: User is logged in", true, siteUser.IsLoggedIn);
				}
				return siteUser;
			}
		}

		TrackingSiteUser siteUser;

		protected ITrackingEventsProvider TestEventsProvider;
		EventVisibilityCollection cachedEventVisibility;
		bool cachedEventIncludeEstimates;
		bool cachedEventIncludeRelated;
		string cachedEventSortOrder;

		protected override void SetUp()
		{
			base.SetUp();
			cachedEventVisibility = WebDataRegistry.Instance.EventVisibility.Value;
			cachedEventIncludeEstimates = WebDataRegistry.Instance.EventIncludeEstimates.Value;
			cachedEventIncludeRelated = WebDataRegistry.Instance.EventIncludeRelated.Value;
			cachedEventSortOrder = WebDataRegistry.Instance.EventSortOrder.Value;

			TestEventsProvider = GetNewTestEventsProvider();
		}

		protected override void TearDown()
		{
			base.TearDown();
			WebDataRegistry.Instance.EventVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedEventVisibility);
			WebDataRegistry.Instance.EventIncludeEstimates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedEventIncludeEstimates);
			WebDataRegistry.Instance.EventIncludeRelated.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedEventIncludeRelated);
			WebDataRegistry.Instance.EventSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedEventSortOrder);
		}

		protected abstract ITrackingEventsProvider GetNewTestEventsProvider();

		#endregion
	}
}
