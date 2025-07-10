using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaign))]
	sealed class HRGlbCompanyCampaignTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			AssertEquals("Default Sales And Marketing should be false", false, campaign.G0_IsSalesAndMarketing);
		}

		public void TestLookups()
		{
			var lookups = CachedBusinessObject.Lookups;
			AssertEquals(CachedBusinessObject, lookups.Parent);
		}

		public void TestHumanReadableName()
		{
			CachedBusinessObject.G0_CampaignID = "EXM001";
			AssertEquals("HR Campaign EXM001", CachedBusinessObject.HumanReadableName);
		}

		public void TestContactDataSource()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.ContactDataSource = "AAA";
			AssertEquals("Invalid value will not be accepted", HRContactDataSourceList.Codes.Staff, campaign.ContactDataSource);
			campaign.ContactDataSource = "";
			AssertEquals("Invalid value will not be accepted", HRContactDataSourceList.Codes.Staff, campaign.ContactDataSource);
			campaign.ContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;
			AssertEquals("Valid value will be accepted", HRContactDataSourceList.Codes.CampaignTracking, campaign.ContactDataSource);
			campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			AssertEquals("Valid value will be accepted", HRContactDataSourceList.Codes.JobApplicant, campaign.ContactDataSource);
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			AssertEquals("Valid value will be accepted", HRContactDataSourceList.Codes.Staff, campaign.ContactDataSource);
		}

		public void TestContactDataSourceForTouch()
		{
			var factory = new BusinessObjectFactory();
			var master = factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.AllTouches.Add(touch);
			AssertEquals(false, touch.IsMasterCampaign);
			AssertEquals(true, touch.IsTouchCampaign);

			touch.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch.ContactDataSource);

			touch.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch.ContactDataSource);

			touch.ContactDataSource = "";
			AssertEquals(ContactDataSourceList.Codes.CampaignTracking, touch.ContactDataSource);
		}

		public void TestDripMarketingModuleID()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			AssertEquals(ModuleIDs.DripMarketingFilterRuleHR, campaign.DripMarketingFilterRuleModule);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var campaign = (HRGlbCompanyCampaign)base.GetNewBusinessObjectForDeleteTest(factory);
			campaign.Questions.DeleteAll();
			return campaign;
		}

		new HRGlbCompanyCampaign CachedBusinessObject
		{
			get { return (HRGlbCompanyCampaign)base.CachedBusinessObject; }
		}

		public class HRGlbCompanyCampaignForTest : HRGlbCompanyCampaign
		{
			public const string PhoneFilterForLoadingLessObjects = "000000000000000";
			public HRGlbCompanyCampaignForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				FilterToStopLoadingAllContacts = new ZQuery(ViewCampaignContactSchema.VCC_Phone, PhoneFilterForLoadingLessObjects);
			}

			readonly ZQuery FilterToStopLoadingAllContacts;

			protected override ZQuery ContactFilter
			{
				get
				{
					ZQuery filter = base.ContactFilter;
					filter.AddToFilter(FilterToStopLoadingAllContacts);
					return filter;
				}
			}
		}

		public static HRGlbCompanyCampaignForTest GetCampaignForTestWithoutErrors(BusinessObjectFactory factory)
		{
			var campaign = factory.NewWithValidTestData<HRGlbCompanyCampaignForTest>();
			PopulateCampaign(campaign, factory);
			return campaign;
		}

		public static void PopulateCampaign(GlbCompanyCampaign campaign, BusinessObjectFactory factory, bool addName = false)
		{
			var categoryList = new CodeDescriptionBoolCollection
			{
				{ "***", (NoResString)"Category" }
			};
			var stageList = new CodeDescriptionPairList();
			stageList.AddPair("***", "Category");

			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryList);
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stageList);

			var collection = new CodeDescriptionBoolCollection
			{
				{ "***", (NoResString)"Category" }
			};
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			staff.StaffPlainTextPassword = "1";
			staff.StaffConfirmPassword = "1";
			staff.GS_City = "AUSYD";
			staff.GS_FullName = "full name";
			staff.GS_UserAddress1 = "address1";

			campaign.G0_BatchCountDefault = 1;
			campaign.G0_Stage = "***";
			campaign.G0_Category = "***";
			campaign.G0_Type = "***";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;

			if (addName)
			{
				campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			}
		}
		#endregion
	}
}
