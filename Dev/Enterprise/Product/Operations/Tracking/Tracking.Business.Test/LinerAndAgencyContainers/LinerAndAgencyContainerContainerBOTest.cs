using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainer))]
	sealed class LinerAndAgencyContainerContainerBOTest : AgencyShipmentContainerBOTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testContainer = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertNotNull(testContainer.Milestones);
			AssertEquals(0, testContainer.Milestones.Count);

			var milestone1 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testContainer.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testContainer.Factory.Save();
			AssertEquals(0, testContainer.Milestones.Count);

			testContainer.ReloadMilestones();
			AssertEquals(2, testContainer.Milestones.Count);
		}

		#endregion

		#region Last Free Day

		public void TestLastFreeDay()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals(ZDateTime.Empty, container.JC_ArrivalCTOStorageStartDate);
			AssertEquals(ZDateTime.Empty, container.JC_LastFreeDay);

			var testDate = ZDateTime.Now;
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate.Date.AddDays(-1), container.JC_LastFreeDay);

			testDate = ZDateTime.Now.AddDays(10);
			container.JC_ArrivalCTOStorageStartDate = testDate;
			AssertEquals(testDate.Date.AddDays(-1), container.JC_LastFreeDay);
		}

		#endregion

		public void TestShipmentNumbers()
		{
			var testContainer = Factory.New<LinerAndAgencyContainer>();
			AssertEquals(testContainer.ShipmentNumbers, ZString.Empty);
		}

		public void TestWebEmailNotificationNumber()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals("Number is a ContainerNumber", container.ContainerNumber, ((IBizOChangesEmailNotification)container).Number);
		}

		public void TestWebEmailNotificationIsCancelled()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals("Is not Cancelled", false, ((IBizOChangesEmailNotification)container).IsCancelled);
		}

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.LinerAndAgencyContainerNotificationOptions, ((IBizOChangesEmailNotification)Factory.NewWithValidTestData<LinerAndAgencyContainer>()).NotificationSendingRule);
		}

		public void TestWebContainerStaffRolesToNotify()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals("ContainerStaffRolesEmailNotificationGroup", WebDataRegistry.Instance.LinerAndAgencyContainerNotificationStaffRoles, ((IBizOChangesEmailNotification)container).StaffRolesToNotify);
		}

		public void TestWebEmailNotificationEmailGroupRegistryItem()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals("ContainerEmailNotificationGroup", WebDataRegistry.Instance.LinerAndAgencyContainerNotificationEmailGroup, ((IBizOChangesEmailNotification)container).EmailGroupRegistryItem);
		}

		public void TestWebEmailNotificationControllerForEnterpriseUrl()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			AssertEquals("Containers Controller", ControllerIDs.Containers, ((IBizOChangesEmailNotification)container).ControllerForEnterpriseUrl);
		}

		public void TestRequiredDeliveryStatus()
		{
			var container = Factory.New<LinerAndAgencyContainer>();
			AssertEquals("Initially should be empty", true, container.RequiredDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.RequiredDeliveryStatus.IsEmpty);

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Now.Date.AddDays(-1); //LastFreeDay
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.RequiredDeliveryStatus);

			container.RequiredDelivery = ZDateTime.Now.Date;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.RequiredDeliveryStatus);

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Now.Date.AddDays(1).AddMinutes(11); //LastFreeDay
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.RequiredDeliveryStatus);
		}

		public void TestConfirmedDeliveryStatus()
		{
			var container = Factory.New<LinerAndAgencyContainer>();
			AssertEquals("Initially should be empty", true, container.ConfirmedDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ConfirmedDeliveryStatus.IsEmpty);

			container.RequiredDelivery = ZDateTime.Now.AddDays(-1);
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ConfirmedDeliveryStatus);

			container.ConfirmedDelivery = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.ConfirmedDeliveryStatus);

			container.RequiredDelivery = container.ConfirmedDelivery;
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.ConfirmedDeliveryStatus);
		}

		public void TestActualDeliveryStatus()
		{
			var container = Factory.New<LinerAndAgencyContainer>();
			AssertEquals("Initially should be empty", true, container.ActualDelivery.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ActualDeliveryStatus.IsEmpty);

			container.ConfirmedDelivery = ZDateTime.Now.AddDays(-1);
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ActualDeliveryStatus);

			container.ActualDelivery = ZDateTime.Now;
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Late, container.ActualDeliveryStatus);

			container.ActualDelivery = container.ConfirmedDelivery;
			AssertEquals("Status should be OnTime", Constants.DateTimeStatus.OnTime, container.ActualDeliveryStatus);
		}

		public void TestActualDehireStatus()
		{
			var container = Factory.New<LinerAndAgencyContainer>();
			AssertEquals("Initially should be empty", true, container.ActualDehire.IsEmpty);
			AssertEquals("Initially status should be empty", true, container.ActualDehireStatus.IsEmpty);

			container.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-1); // EmptyReturnRequired
			AssertEquals("Status should be Overdue", Constants.DateTimeStatus.Overdue, container.ActualDehireStatus);

			container.ActualDehire = ZDateTime.Now.AddHours(1);
			AssertEquals("Status should be Late", Constants.DateTimeStatus.Overdue, container.ActualDehireStatus);
		}

		[HttpContextEnabledTest]
		public void TestSettingLoggingReference()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull("Precondition: SiteUser has been set up", helper.TestSiteUser);

			var container = Factory.New<LinerAndAgencyContainer>();
			AssertNotNull("Precondition: SiteUser not null", container.SiteUser);
			Assert("Precondition: Site User has contact+company reference", container.SiteUser.ContactAndCompanyReference.Length > 0);
			AssertEquals("Logging Reference Set On Container", container.SiteUser.ContactAndCompanyReference, container.Logs.AutoCreatedLogDefaultSL_Reference);

			var newContact = helper.TestOrg.Contacts.AddNew();
			newContact.OC_ContactName = "newcontact";
			newContact.OC_Email = "newcontact@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("test123");
			Factory.Save();

			helper.TestSiteUser.Login("", "newcontact@cargowise.com", "test123");
			Assert("New user logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals("Email of new user", newContact.OC_Email, helper.TestSiteUser.LoggedInUser.OC_Email);

			AssertNotEquals("Logged in site user has different reference to container", container.Logs.AutoCreatedLogDefaultSL_Reference, helper.TestSiteUser.ContactAndCompanyReference);

			container.SiteUser = helper.TestSiteUser;
			AssertEquals("Logging reference set on new SiteUser", helper.TestSiteUser.ContactAndCompanyReference, container.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		#region FromPKFilteredBySiteUserTest

		[HttpContextEnabledTest]
		public void TestFromPKFilteredBySiteUser()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(false, helper.TestSiteUser.IsShipmentQuickViewUser);

			var containerWithNoConsole = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			containerWithNoConsole.JC_ContainerNum = "123458";
			Factory.Save();

			var testContainerFromNumberWithNoConsole = LinerAndAgencyContainer.FromPKFilteredBySiteUser(Factory, containerWithNoConsole.PK, helper.TestSiteUser);
			AssertEquals(null, testContainerFromNumberWithNoConsole);

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(false, helper.TestSiteUser.IsShipmentQuickViewUser);

			var containerWithConsole = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			containerWithConsole.JC_ContainerNum = "123456";
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();

			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);
			Assert("WebUser should be logged in", helper.TestSiteUser.IsLoggedIn);
			AssertEquals(true, helper.TestSiteUser.IsShipmentQuickViewUser);

			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			container.JC_ContainerNum = "123456";
			Factory.Save();

			var testContainerFromNumber = LinerAndAgencyContainer.FromPKFilteredBySiteUser(Factory, container.PK, helper.TestSiteUser);
			AssertEquals(container, testContainerFromNumber);
		}

		#endregion FromPKFilteredBySiteUserTest

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var agencyShipment = Factory.New<AgencyShipment>();
			var container = Factory.New<LinerAndAgencyContainer>();
			container.JC_JS_FCLBookingOnlyLink = agencyShipment.PK;
			return container;
		}

		#endregion
	}
}
