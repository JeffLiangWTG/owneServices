using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class GlbCompanyCampaignTestHelper
	{
		public GlbCompanyCampaignTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public BusinessObjectFactory Factory { get; }

		public GlbCompanyCampaign GetCampaignWithoutErrors()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			PopulateCampaign(campaign, Factory);
			return campaign;
		}

		public GlbCompanyCampaignTest.GlbCompanyCampaignForTest GetCampaignForTestWithoutErrors()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignTest.GlbCompanyCampaignForTest>();
			PopulateCampaign(campaign, Factory);
			return campaign;
		}

		public GlbCompanyCampaign Master { get; private set; }
		public GlbCompanyCampaign Touch1A { get; private set; }
		public GlbCompanyCampaign Touch1B { get; private set; }
		public GlbCompanyCampaign Touch2A { get; private set; }
		public GlbCompanyCampaign Touch2B { get; private set; }
		public GlbCompanyCampaign Touch3A { get; private set; }
		public OrgHeader Org { get; private set; }
		public OrgContact Contact1 { get; private set; }
		public OrgContact Contact2 { get; private set; }
		public OrgContact Contact3 { get; private set; }
		public GlbCompanyCampaignItem Item1_1a { get; private set; }
		public GlbCompanyCampaignItem Item2_1a { get; private set; }
		public GlbCompanyCampaignItem Item1_1b { get; private set; }
		public GlbCompanyCampaignItem Item1_2a { get; private set; }
		public GlbCompanyCampaignItem Item1_2b { get; private set; }
		public GlbCompanyCampaignItem ItemMaster1 { get; private set; }
		public GlbCompanyCampaignItem ItemMaster2 { get; private set; }
		public GlbCompanyCampaignItem ItemMaster3 { get; private set; }

		public void SetupDripCampaign(bool setAllHorizontalIdsToOne = false)
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Contact1 = Org.Contacts.AddNew();
			Contact1.OC_Email = "thomas@test.com";
			Contact1.OC_ContactName = "Thomas";
			Contact2 = Org.Contacts.AddNew();
			Contact2.OC_Email = "gordon@test.com";
			Contact2.OC_ContactName = "Gordon";
			Contact3 = Org.Contacts.AddNew();
			Contact3.OC_Email = "viktor@test.com";
			Contact3.OC_ContactName = "Viktor";

			Master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			ItemMaster1 = Master.CampaignsItemsSent.AddNew();
			ItemMaster1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ItemMaster1.G8_RecipientID = Contact1.PK;
			ItemMaster1.G8_TrackingStatus = "UNV";
			ItemMaster2 = Master.CampaignsItemsSent.AddNew();
			ItemMaster2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ItemMaster2.G8_RecipientID = Contact2.PK;
			ItemMaster2.G8_TrackingStatus = "UNV";
			ItemMaster3 = Master.CampaignsItemsSent.AddNew();
			ItemMaster3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			ItemMaster3.G8_RecipientID = Contact3.PK;
			ItemMaster3.G8_TrackingStatus = "UNV";

			Touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Touch1A.G0_CampaignName = "Touch 1A";
			Touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			Touch1A.G0_HorizontalId = 1;
			Touch1A.G0_VerticalId = "A";
			Touch1A.G0_G0_Master = Master.PK;
			Item1_1a = Touch1A.CampaignsItemsSent.AddNew();
			Item1_1a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Item1_1a.G8_RecipientID = Contact1.PK;
			Item1_1a.G8_TrackingStatus = "QUE";
			Item2_1a = Touch1A.CampaignsItemsSent.AddNew();
			Item2_1a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Item2_1a.G8_RecipientID = Contact2.PK;
			Item2_1a.G8_TrackingStatus = "VER";
			Master.AllTouches.Add(Touch1A);

			Touch1B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Touch1B.G0_CampaignName = "Touch 1B";
			Touch1B.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(2);
			Touch1B.G0_HorizontalId = 1;
			Touch1B.G0_VerticalId = "B";
			Touch1B.G0_G0_Master = Master.PK;
			Item1_1b = Touch1B.CampaignsItemsSent.AddNew();
			Item1_1b.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Item1_1b.G8_RecipientID = Contact3.PK;
			Item1_1b.G8_TrackingStatus = "UNV";
			Master.AllTouches.Add(Touch1B);

			Touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Touch2A.G0_CampaignName = "Touch 2A";
			Touch2A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(3);
			Touch2A.G0_HorizontalId = (byte)(setAllHorizontalIdsToOne ? 1 : 2);
			Touch2A.G0_VerticalId = "A";
			Touch2A.G0_G0_Master = Master.PK;
			Item1_2a = Touch2A.CampaignsItemsSent.AddNew();
			Item1_2a.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Item1_2a.G8_RecipientID = Contact2.PK;
			Item1_2a.G8_TrackingStatus = "QUE";
			Master.AllTouches.Add(Touch2A);

			Touch2B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Touch2B.G0_CampaignName = "Touch 2B";
			Touch2B.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(4);
			Touch2B.G0_HorizontalId = (byte)(setAllHorizontalIdsToOne ? 1 : 2);
			Touch2B.G0_VerticalId = "B";
			Touch2B.G0_G0_Master = Master.PK;
			Item1_2b = Touch2B.CampaignsItemsSent.AddNew();
			Item1_2b.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Item1_2b.G8_RecipientID = Contact3.PK;
			Item1_2b.G8_TrackingStatus = "VER";
			Master.AllTouches.Add(Touch2B);

			Touch3A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Touch3A.G0_CampaignName = "Touch 3A";
			Touch3A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(5);
			Touch3A.G0_HorizontalId = (byte)(setAllHorizontalIdsToOne ? 1 : 3);
			Touch3A.G0_VerticalId = "A";
			Touch3A.G0_G0_Master = Master.PK;
			Master.AllTouches.Add(Touch3A);

			PopulateCampaign(Master, Factory);
			PopulateCampaign(Touch1A, Factory);
			PopulateCampaign(Touch1B, Factory);
			PopulateCampaign(Touch2A, Factory);
			PopulateCampaign(Touch2B, Factory);
			PopulateCampaign(Touch3A, Factory);

			Factory.Save();
		}

		public static void PopulateCampaign(GlbCompanyCampaign campaign, BusinessObjectFactory factory, bool addName = false)
		{
			var categoryList = new CodeDescriptionBoolCollection();
			categoryList.Add("***", (NoResString)"Category");
			CodeDescriptionPairList stageList = new CodeDescriptionPairList();
			stageList.AddPair("***", "Category");

			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryList);
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stageList);

			var collection = new CodeDescriptionBoolCollection();
			collection.Add("***", (NoResString)"Category");
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
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
	}
}
