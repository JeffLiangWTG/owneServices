using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Tracking.Business.Testing
{
	abstract class LinerAndAgencyBaseWebEmailNotificationTest<T> : BizOChangesEmailNotificationTest<T> where T : LinerAndAgencyBaseWebInterfacesHelper
	{
		public void TestPkAndControllerForEnterpriseURL()
		{
			ZGuid bookingPK = Shipment.PK;

			AssertEquals("PK", bookingPK, (Helper).PK);
			AssertEquals("ControllerForEnterpriseUrl", ExpectedControllerID, Helper.ControllerForEnterpriseUrl);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", Shipment.HumanReadableName, Helper.HumanReadableName);
		}

		public void TestHumanReadableNameAndNumber()
		{
			Shipment.JS_UniqueConsignRef = "01234543210";
			AssertEquals("HumanReadableName", Shipment.HumanReadableName, Helper.HumanReadableName);
			AssertEquals("Number", Shipment.JS_UniqueConsignRef, Helper.Number);
		}

		public void TestLoggedInContactIsNull()
		{
			AssertNull("LoggedInContact", Helper.LoggedInContact);
		}

		public void TestRelatedOrg()
		{
			AssertNull("Booking Party", Shipment.BookingParty);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.BookingPartyDocumentaryAddress.OrganisationNameOrPK = org.PK.ToString();
			AssertEquals("RelatedOrg", org.PK, Helper.RelatedOrg.PK);
		}

		public void TestNotificationSendingRule()
		{
			AssertEquals(ExpectedNotificationSendingRule, Helper.NotificationSendingRule);
		}

		public void TestStaffRolesToNotify()
		{
			AssertEquals(ExpectedStaffRolesToNotify, Helper.StaffRolesToNotify);
		}

		public void TestEmailGroupRegistryItem()
		{
			AssertEquals(ExpectedEmailGroupRegistryItem, Helper.EmailGroupRegistryItem);
		}

		public void TestEventBranch()
		{
			OrgHeader bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_RL_NKClosestPort = "NZAKL";
			Shipment.BookingPartyDocumentaryAddress.OrganisationNameOrPK = bookingParty.PK.ToString();
			Shipment.JS_NKLoadPort = "AUMEL";

			AssertNull("No controlling branch, load port and no Booking Party's closest port -- should return null", Helper.EventBranch);

			GlbBranch branch1 = CreateBranch(bookingParty, false, "NZAKL");
			Factory.Save();
			AssertEquals("No controlling branch and load so it searches by BookingParty's closest port", branch1, Helper.EventBranch);

			GlbBranch branch2 = CreateBranch(bookingParty, true, "USPTQ");
			Factory.Save();
			AssertEquals("It searches by controlling branch", branch2, Helper.EventBranch);

			GlbBranch branch3 = CreateBranch(bookingParty, false, "AUMEL");
			Factory.Save();
			AssertEquals("Even with controlling branch it searches by load port", branch3, Helper.EventBranch);
		}

		GlbBranch CreateBranch(OrgHeader org, bool setControllingBranch, string homePort)
		{
			OrgCompanyData orgCompanyData = org.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;
			glbBranch.GB_RL_NKHomePort = homePort;

			orgCompanyData.OB_GC = glbCompany.PK;

			if (setControllingBranch)
			{
				orgCompanyData.OB_GB_ControllingBranch = glbBranch.PK;
			}

			return glbBranch;
		}

		public void TestIsCancelled()
		{
			Shipment.JS_IsCancelled = false;
			AssertEquals("IsCancelled", false, Helper.IsCancelled);

			Shipment.JS_IsCancelled = true;
			AssertEquals("IsCancelled", true, Helper.IsCancelled);
		}

		public void TestFactoryRelatedProperties()
		{
			AssertEquals("Factory", Factory, Helper.Factory);
			AssertEquals("IsInDatabase", false, Helper.IsInDatabase);
			AssertEquals("IsDeleted", false, Helper.IsDeleted);
			AssertEquals("HasChanges", true, Helper.HasChanges);

			Factory.Save();
			AssertEquals("IsInDatabase", true, Helper.IsInDatabase);
			AssertEquals("IsDeleted", false, Helper.IsDeleted);
			AssertEquals("HasChanges", false, Helper.HasChanges);

			Shipment.Delete();
			AssertEquals("IsInDatabase", true, Helper.IsInDatabase);
			AssertEquals("IsDeleted", true, Helper.IsDeleted);
			AssertEquals("HasChanges", true, Helper.HasChanges);
		}

		#region TestGeneratePackLineDetailsForEmailReporting_UNDGsNullIssue

		public void TestGeneratePackLineDetailsForEmailReporting_UNDGsNullIssue()
		{
			var packline = Shipment.OuterPackLines.AddNew();
			packline.UNDGs.AddNew();
			AssertNoExceptionThrown(() => Helper.GeneratePackLineDetailsForEmailReportingForTest(packline));
		}

		#endregion

		protected override T GetNewBizOForNotification()
		{
			return Helper;
		}

		#region Implementation

		protected T Helper;
		protected AgencyShipment Shipment;

		protected abstract ControllerID ExpectedControllerID { get; }
		protected abstract CodePairRegistryItem ExpectedNotificationSendingRule { get; }
		protected abstract CodeDescriptionBoolRegistryItem ExpectedStaffRolesToNotify { get; }
		protected abstract GuidRegistryItem ExpectedEmailGroupRegistryItem { get; }

		protected abstract AgencyShipment GetBusinessObjectForHelper();
		protected abstract T GetHelper(AgencyShipment shipment);

		protected override void SetUp()
		{
			base.SetUp();

			Shipment = GetBusinessObjectForHelper();
			Helper = GetHelper(Shipment);
		}

		#endregion

	}
}
