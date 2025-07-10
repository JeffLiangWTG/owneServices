using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OpportunityCreationTemplate))]
	sealed class OpportunityCreationTemplateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new OpportunityCreationTemplate(campaign);

		#region Tests

		public void TestParseOpportunityContentString()
		{
			var template = new OpportunityCreationTemplate(campaign);
			var testOpportunityCreationContentString = "INT" + template.Separator + "NEW" + template.Separator + "Test" + template.Separator + "WON" + template.Separator + "OPN" + template.Separator + "EVT" + template.Separator + "Details" + template.Separator + "SPI" + template.Separator + "ABC" + template.Separator + "DEF" + template.Separator + "NotesTest";

			template.ParseOpportunityContentString(testOpportunityCreationContentString);
			AssertEquals("INT", template.PackageType);
			AssertEquals("NEW", template.OpportunityType);
			AssertEquals("Test", template.OpportunityDescription);
			AssertEquals("WON", template.OpportunityStatus);
			AssertEquals("OPN", template.OpportunityStage);
			AssertEquals("EVT", template.Source);
			AssertEquals("Details", template.ActiveSourceDetails);
			AssertEquals("SPI", template.OpportunityAssignment);
			AssertEquals("ABC", template.SalesPerson);
			AssertEquals("DEF", template.StaffAssignment);
			AssertEquals(ZBlob.FromUTF8("NotesTest"), template.OpportunityNotes);
		}

		public void TestFactorySaveWithDeletedCampaign()
		{
			var template = new OpportunityCreationTemplate(campaign);
			campaign.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestNoValidationErrorsOnCampaignTypeChange()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_Code = "COR";
			coordinator.GS_EmailAddress = "unit.test@cw1.com";

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";

			var parentCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			parentCampaign.G0_CampaignName = "Test Campaign Name";
			parentCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			parentCampaign.G0_Category = "PRINT";
			parentCampaign.G0_EstimatedStartedDate = ZDateTime.Today;
			parentCampaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			parentCampaign.G0_GS_NKCampaignManager = manager.GS_Code;
			parentCampaign.G0_Type = "PREAP";

			Factory.Save();

			var campaignTouch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaignTouch.G0_CampaignName = "Test Campaign Touch A";
			campaignTouch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			campaignTouch.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(5);
			campaignTouch.G0_HorizontalId = 1;
			campaignTouch.G0_VerticalId = "A";
			campaignTouch.G0_G0_Master = parentCampaign.PK;
			campaignTouch.G0_Category = "PRINT";
			campaignTouch.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaignTouch.G0_GS_NKCampaignManager = manager.GS_Code;
			campaignTouch.G0_Type = "PREAP";
			campaignTouch.SourceCampaignPK = parentCampaign.PK;
			parentCampaign.AllTouches.Add(campaignTouch);

			campaignTouch.Validation.ValidateAll();
			Assert("Campaign should have errors for empty fields", campaignTouch.HasErrors);

			campaignTouch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			campaignTouch.HtmlDocumentBlob = ZBlob.FromAscii("Test G0_DocumentBlob");

			// Simulate save
			campaignTouch.OpportunityCreationTemplate.RunPreSaveValidation();
			Factory.Save();

			Assert("Saving non OPP campaign will clear all values and run PreSaveValidation which should clear all errors.", !campaignTouch.HasErrors);
		}

		public void TestSalesPersonClearedWhenOpportunityAssignmentChanged()
		{
			var template = new OpportunityCreationTemplate(campaign);

			template.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;
			template.SalesPerson = "TST";

			AssertEquals("Pre-condition", "TST", template.SalesPerson);

			template.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;
			Assert("Sales Person value should be cleared.", template.SalesPerson.IsEmpty);
		}

		public void TestStaffAssignmentClearedWhenOpportunityAssignmentChanged()
		{
			var template = new OpportunityCreationTemplate(campaign);

			template.OpportunityAssignment = OpportunityAssignmentList.Codes.StaffAssignment;
			template.StaffAssignment = StaffAssignmentRoles.Codes.AccountManager;

			AssertEquals("Pre-condition", StaffAssignmentRoles.Codes.AccountManager, template.StaffAssignment);

			template.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;
			Assert("Staff Assignment value should be cleared.", template.StaffAssignment.IsEmpty);
		}

		public void TestClearAllValues()
		{
			var template = OpportunityCreationTemplateTestHelper.PopulateOpportunityCreationTemplate(campaign.OpportunityCreationTemplate, "Description");

			OpportunityCreationTemplateTestHelper.AssertTemplateValues("Pre-condition", template, "STD", "UDF", "Description", "CRT", "UDF", "WEB", "NOT", OpportunityAssignmentList.Codes.MatchParentTouchSender);

			template.ClearAllValues();

			OpportunityCreationTemplateTestHelper.AssertTemplateValuesEmpty("All values should be cleared after calling ClearAllValues().", template);
		}

		public void TestSavingPRECampaignClearsAllValues()
		{
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			var template = OpportunityCreationTemplateTestHelper.PopulateOpportunityCreationTemplate(campaign.OpportunityCreationTemplate, "Description");

			Factory.Save();
			OpportunityCreationTemplateTestHelper.AssertTemplateValues("Pre-condition", template, "STD", "UDF", "Description", "CRT", "UDF", "WEB", "NOT", OpportunityAssignmentList.Codes.MatchParentTouchSender);

			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			OpportunityCreationTemplateTestHelper.AssertTemplateValues("Template values should not be cleared yet.", template, "STD", "UDF", "Description", "CRT", "UDF", "WEB", "NOT", OpportunityAssignmentList.Codes.MatchParentTouchSender);

			Factory.Save();
			OpportunityCreationTemplateTestHelper.AssertTemplateValuesEmpty("All values should be cleared after save when campaign is not OPP.", template);
		}

		public void TestRunPreSaveValidation()
		{
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			var template = campaign.OpportunityCreationTemplate;
			template.OpportunityAssignment = "XYZ";

			Assert("Pre-condition", !template.OpportunityAssignmentInfo.HasErrors());

			template.RunPreSaveValidation();
			Assert(template.OpportunityAssignmentInfo.HasErrors());
		}

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var temp = (OpportunityCreationTemplate)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, temp.OpportunityNotes);
			AssertEquals(ZBlob.Empty, temp.OpportunityNotes_HTML);

			temp.OpportunityNotes_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(temp.OpportunityNotes.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", temp.OpportunityNotes_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var temp = (OpportunityCreationTemplate)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, temp.OpportunityNotes);
			AssertEquals(ZBlob.Empty, temp.OpportunityNotes_HTML);

			temp.OpportunityNotes = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", temp.OpportunityNotes_HTML.ToUTF8());

			temp.OpportunityNotes = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", temp.OpportunityNotes_HTML.ToUTF8());
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();
		}

		GlbCompanyCampaign campaign;

		#endregion
	}
}


