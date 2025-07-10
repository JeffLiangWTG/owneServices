using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingQuotedBookingTest : TestCaseWithFactory
	{
		#region TestGetNewQuotation

		public void TestGetNewQuotation()
		{
			ZGuid demoBranchPK = Factory.LoadFromUniqueKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, new ZString("DEM")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, demoBranchPK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 0);
				Env.Registry.Rating.WebRateValidityPeriod = 2;
				AssertEquals("PreCondition: CurrentCompany should be DEM", "DEM", GlbCompany.CurrentCompany.GC_Code);
				TrackingQuotedBooking spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);
				AssertNotNull(spotQuote);
				AssertEquals("Quote Org", Helper.TestSiteUser.CurrentOrg, spotQuote.Quote.TH_OH);
				AssertEquals("Quote FollowUpDate", ZDateTime.Empty, spotQuote.Quote.TH_FollowUpDate);
				AssertEquals("SiteUser ContactAndCompanyReference", String.Format("{0} ({1})", Helper.TestContact.OC_Email, Helper.TestOrg.OH_Code), Helper.TestSiteUser.ContactAndCompanyReference);
				AssertEquals("RateOneOffShipment AutoCreatedLogReference", Helper.TestSiteUser.ContactAndCompanyReference, spotQuote.Logs.AutoCreatedLogDefaultSL_Reference);
				AssertEquals("Quote AutoCreatedLogReference", Helper.TestSiteUser.ContactAndCompanyReference, spotQuote.Quote.Logs.AutoCreatedLogDefaultSL_Reference);
				Assert("Quote EndDate should be set", spotQuote.Quote.TH_QuoteEndDate.IsValid);

				AssertEquals("Quote EndDate should use WebRateValidityPeriod", (ZDateTime.Now.Month + Env.Registry.Rating.WebRateValidityPeriod - 1) % 12 + 1, spotQuote.Quote.TH_QuoteEndDate.Month);
				Assert("Companies list should contain at least one company", spotQuote.Quote.Lookups.Companies.Count > 0);
				Assert("QuoteCompany should be one of the companies in the list", spotQuote.Quote.Lookups.Companies.Contains(Factory.Load<GlbCompany>(spotQuote.Quote.TH_GC)));
				Assert("QuoteCompany should not be DEM", spotQuote.Quote.Company.GC_Code != "DEM");
				AssertEquals("Quote should be Active", Quote.QuoteStatusOptions.Active, spotQuote.Quote.QuoteStatus);
			}
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			var demoBranchPK = Factory.LoadFromUniqueKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, new ZString("DEM")).PK;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, demoBranchPK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);
				var container = Factory.NewWithValidTestData<TrackingQuoteContainer>();
				spotQuote.QuoteContainers.Add(container);
				AssertEquals("Containers are the same", spotQuote.Quote.CurrentOneOffQuote.Containers[0].PK, container.PK);
				spotQuote.QuoteContainers.Remove(container);
				AssertEquals("Container has been remoevd", spotQuote.Quote.CurrentOneOffQuote.Containers.Count, 0);
			}
		}

		#endregion

		#region TestIBizOChangesEmailNotification

		public void TestIBizOChangesEmailNotification()
		{
			var bizO = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);
			var bizOWithNotifier = (IBizOChangesEmailNotification)bizO;

			AssertEquals("PK", bizO.Quote.PK, bizOWithNotifier.PK);
			AssertEquals("ControllerForEnterpriseUrl", ControllerIDs.QuotedBookings, bizOWithNotifier.ControllerForEnterpriseUrl);

			bizO.Quote.TH_QuoteNumber = "0001";
			AssertEquals("Number", "0001", bizOWithNotifier.Number);
			AssertEquals("HumanReadableName", "Quote 0001", bizOWithNotifier.HumanReadableName);

			AssertEquals("IsCancelled", bizO.Quote.IsCancelled, bizOWithNotifier.IsCancelled);
			AssertEquals("IsInDatabase", bizO.Quote.IsInDatabase, bizOWithNotifier.IsInDatabase);
			AssertEquals("IsDeleted", bizO.Quote.IsDeleted, bizOWithNotifier.IsDeleted);
			AssertEquals("HasChanges", bizO.Quote.HasChanges, bizOWithNotifier.HasChanges);

			AssertEquals("LoggedInContact", Helper.TestSiteUser.LoggedInUser.PK, bizOWithNotifier.LoggedInContact.PK);
			AssertEquals("RelatedOrg", Helper.TestSiteUser.LoggedInOrganisation.PK, bizOWithNotifier.RelatedOrg.PK);
			AssertEquals("EventBranch (Quote's Company's first active branch)", bizO.Quote.Company.FirstActiveBranch.GB_Code, bizOWithNotifier.EventBranch.GB_Code);

			AssertEquals("EmailGroupRegistryItem", WebDataRegistry.Instance.QuoteNotificationEmailGroup, bizOWithNotifier.EmailGroupRegistryItem);
			AssertEquals("StaffRolesToNotify", WebDataRegistry.Instance.QuoteNotificationStaffRoles, bizOWithNotifier.StaffRolesToNotify);
			AssertEquals("NotificationSendingRule", WebDataRegistry.Instance.QuoteNotificationOptions, bizOWithNotifier.NotificationSendingRule);
		}

		public void TestNotificationEventBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BB";
			branch2.GB_Code = "CC";
			branch3.GB_Code = "AA";
			company.Branches.Add(branch1);
			company.Branches.Add(branch2);
			company.Branches.Add(branch3);
			Factory.Save();

			var bizO = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);
			var bizOWithNotifier = (IBizOChangesEmailNotification)bizO;

			bizO.Quote.TH_GC = company.PK;
			AssertEquals("Prerequisite: We have the correct company selected for the quote", company.PK, bizO.Quote.Company.PK);
			AssertNotEquals("Prerequisite: The controlling branch and the company's first active branch are different", company.FirstActiveBranch.GB_Code, GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation).GB_Code);
			AssertEquals("The first active branch is returned as the event branch when the controlling branch is not under the company selected for the quote", company.FirstActiveBranch.GB_Code, bizOWithNotifier.EventBranch?.GB_Code);

			bizO.Quote.TH_GC = ZGuid.Empty;
			AssertNull("Prerequisite: The quote has no company", bizO.Quote.Company);
			AssertEquals("The controlling branch is returned as the event branch when the quote has no company", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation).GB_Code, bizOWithNotifier.EventBranch?.GB_Code);

			Helper.TestSiteUser.LoggedInOrganisation.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Helper.TestSiteUser.LoggedInOrganisation.Factory.Save();
			AssertEquals("Prerequisite: The controlling branch is one of the company branches for the quote", branch2.GB_Code, GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation)?.GB_Code);
			AssertEquals("The controling branch is returned as the event branch when it is under the company for the quote", branch2.GB_Code, bizOWithNotifier.EventBranch?.GB_Code);

			Helper.TestSiteUser.LoggedInOrganisation.CompanyData.OB_GB_ControllingBranch = Guid.Empty;
			Helper.TestSiteUser.LoggedInOrganisation.Factory.Save();
			bizO.Quote.TH_GC = company.PK;
			AssertNull("Prerequisite: There is no controlling branch", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation));
			AssertEquals("Prerequisite: We have the correct company selected for the quote", company.PK, bizO.Quote.Company.PK);
			AssertEquals("The first active branch within the company is returned as the event branch when there is no controlling branch", company.FirstActiveBranch.GB_Code, bizOWithNotifier.EventBranch?.GB_Code);

			company.Branches.RemoveAllFromRelationship();
			AssertEquals("Prerequisite: There no branches under the company", 0, company.Branches.Count);
			AssertNull("The event branch is missing when the controlling branch is missing and the company has no branches", bizOWithNotifier.EventBranch);

			Helper.TestSiteUser.LoggedInOrganisation.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			Helper.TestSiteUser.LoggedInOrganisation.Factory.Save();
			AssertNotNull("Prerequisite: There a controlling branch", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation));
			AssertEquals("The controlling branch is returned as the event branch when the company has no branches", GlbBranch.FindControllingBranchWithFallBackToAnyCompany(Helper.TestSiteUser.LoggedInOrganisation).GB_Code, bizOWithNotifier.EventBranch?.GB_Code);
		}

		public void TestNotificationStaffRole()
		{
			var testQuote = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "HKHKG";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = Helper.TestSiteUser.LoggedInOrganisation.OH_RL_NKClosestPort;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var staffAssign1 = shipper.StaffAssignments.AddNew();
			staffAssign1.O8_Department = "FIS";
			staffAssign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			staffAssign1.O8_Role = "MAN";

			var staffAssign2 = consignee.StaffAssignments.AddNew();
			staffAssign2.O8_Department = "FES";
			staffAssign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			staffAssign2.O8_Role = "MAN";

			testQuote.Origin = "HKHKG";
			testQuote.Destination = Helper.TestSiteUser.LoggedInOrganisation.OH_RL_NKClosestPort;
			testQuote.TransportMode = Core.Constants.TransportModes.Sea;

			var staffGuid = ((IBizOChangesEmailNotification)testQuote).GetStaffGuid(shipper.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be same as staff1", staff1.PK, staffGuid);

			staffGuid = ((IBizOChangesEmailNotification)testQuote).GetStaffGuid(consignee.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be empty", ZGuid.Empty, staffGuid);

			testQuote.Origin = Helper.TestSiteUser.LoggedInOrganisation.OH_RL_NKClosestPort;
			testQuote.Destination = "HKHKG";

			staffGuid = ((IBizOChangesEmailNotification)testQuote).GetStaffGuid(consignee.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be same as staff2", staff2.PK, staffGuid);

			staffGuid = ((IBizOChangesEmailNotification)testQuote).GetStaffGuid(shipper.StaffAssignments, new CodeDescriptionBool() { Bool = true, Code = "MAN" });
			AssertEquals("Should be empty", ZGuid.Empty, staffGuid);
		}

		#endregion

		#region TestIBizOChangesEmailNotification_PropertiesForEmail

		public void TestIBizOChangesEmailNotification_PropertiesForEmail()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "C01";
			company1.GC_Name = "001";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "B01";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "AUSYD";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "C02";
			company2.GC_Name = "002";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "B02";
			branch2.GB_GC = company2.PK;
			branch2.GB_RL_NKHomePort = "ADALV";

			Helper.TestOrg.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();

			var bizO = TrackingQuotedBooking.GetNewQuotation(Factory, Helper.TestSiteUser);
			bizO.Quote.TH_GC = company2.PK;

			bizO.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			bizO.ConsigneeDocumentaryAddress.E2_CompanyName = "Company1";
			bizO.ConsigneeDocumentaryAddress.E2_Address1 = "Address1";
			bizO.ConsigneeDocumentaryAddress.E2_Contact = "Contact1";

			bizO.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			bizO.ConsignorDocumentaryAddress.E2_CompanyName = "Company2";
			bizO.ConsignorDocumentaryAddress.E2_Address1 = "Address2";
			bizO.ConsignorDocumentaryAddress.E2_Contact = "Contact3";

			bizO.Origin = "AUSYD";
			bizO.Destination = "AUMEL";

			bizO.Mode = Core.Constants.RateMode.SEA;
			bizO.PaymentTerms = "XXX";
			bizO.ServiceLevel = "D2D";

			bizO.Weight = 10;
			bizO.WeightUnit = "T";
			bizO.Volume = 5;
			bizO.VolumeUnit = "M3";
			bizO.Commodity = "ALUM";

			var container = bizO.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 5;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "40FR";
			container.TC_RC = refContainer.PK;

			var looseCargo = bizO.Quote.CurrentOneOffQuote.LooseCargo.AddNew();
			looseCargo.TPL_PackLineCount = 5;
			looseCargo.TPL_Length = 1;
			looseCargo.TPL_Width = 2;
			looseCargo.TPL_Height = 3;

			((IBizOChangesEmailNotification)bizO).AddPropertiesForEmailReporting(DataState.Original);
			PropertyChangeInfo[] properties = ((IBizOChangesEmailNotification)bizO).GetPropertiesForEmailReporting();

			Assert("Request quote from", properties.Any(x => x.OriginalValue == bizO.Quote.Company.GC_Name));
			Assert("Pickup", properties.Any(x => x.OriginalValue == bizO.ConsignorDocumentaryAddress.E2_CompanyName));

			Assert("Pickup Address", properties.Any(x => x.OriginalValue == bizO.ConsignorDocumentaryAddress.AddressAsASingleLine));
			Assert("Pickup Contact", properties.Any(x => x.OriginalValue == bizO.ConsignorDocumentaryAddress.E2_Contact));

			Assert("Delivery", properties.Any(x => x.OriginalValue == bizO.ConsigneeDocumentaryAddress.E2_CompanyName));
			Assert("Delivery Address", properties.Any(x => x.OriginalValue == bizO.ConsigneeDocumentaryAddress.AddressAsASingleLine));
			Assert("Delivery Contact", properties.Any(x => x.OriginalValue == bizO.ConsigneeDocumentaryAddress.E2_Contact));

			Assert("Origin", properties.Any(x => x.OriginalValue == bizO.Origin));
			Assert("Destination", properties.Any(x => x.OriginalValue == bizO.Destination));

			Assert("Mode", properties.Any(x => x.OriginalValue == bizO.Modes[Core.Constants.RateMode.SEA].Description));
			Assert("Payment Term", properties.Any(x => x.OriginalValue == bizO.PaymentTerms));
			Assert("Service Level", properties.Any(x => x.OriginalValue == bizO.ServiceLevel));

			Assert("Weight", properties.Any(x => x.OriginalValue == ZString.Format("{0} {1}", bizO.Weight, bizO.WeightUnit)));
			Assert("Volume", properties.Any(x => x.OriginalValue == ZString.Format("{0} {1}", bizO.Volume, bizO.VolumeUnit)));
			Assert("Commodity", properties.Any(x => x.OriginalValue == bizO.Commodity));

			Assert("Container #1", properties.Any(x => x.OriginalValue == "Count=5, Type=40FR"));
			Assert("LooseCargo #1", properties.Any(x => x.OriginalValue == "Count=5, Length=1, Width=2, Height=3"));

			AssertEquals(((IBizOChangesEmailNotification)bizO).EventBranch.GB_Code, bizO.Quote.Company.FirstActiveBranch.GB_Code);
		}

		#endregion

		#region TestNewQuotationWithGetQuotesBasedOnPostalCodesEnabled

		public void TestNewQuotationWithGetQuotesBasedOnPostalCodesEnabled()
		{
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			QuotedBooking spotQuote1 = TrackingQuotedBooking.GetNewQuotation(Factory, testHelper.TestSiteUser);
			Assert("Validation works for ConsigneeDocumentaryAddress", !spotQuote1.ConsigneeDocumentaryAddress.IsValidationSuspended);
			Assert("Validation works for ConsignorDocumentaryAddress", !spotQuote1.ConsignorDocumentaryAddress.IsValidationSuspended);

			WebDataRegistry.Instance.GetQuotesBasedOnPostalCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			QuotedBooking spotQuote2 = TrackingQuotedBooking.GetNewQuotation(Factory, testHelper.TestSiteUser);
			Assert("Validation is suspended", spotQuote2.ConsigneeDocumentaryAddress.IsValidationSuspended && spotQuote2.ConsignorDocumentaryAddress.IsValidationSuspended);
		}

		#endregion

		#region TestCompanies()

		public void TestCompanies()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();

			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			TrackingQuotedBooking spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, testHelper.TestSiteUser);

			Assert("All companies are fetched", spotQuote.Companies.Count >= 3);

			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company1.PK;
			companyData.OB_OH = testHelper.TestOrg.PK;
			companyData.OB_IsDebtor = true;

			spotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, testHelper.TestSiteUser);

			Factory.Save();

			Assert("Not all companies are fetched", spotQuote.Companies.Count == 1);
			Assert("Only those companies are fetched where current org is debtor", spotQuote.Companies[0].PK == company1.PK);
		}

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			Helper = new TestHelper(Factory);
			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
		}
		TestHelper Helper;

		#endregion

	}
}
