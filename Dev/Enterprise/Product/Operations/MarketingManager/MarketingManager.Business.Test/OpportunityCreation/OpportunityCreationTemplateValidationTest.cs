using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OpportunityCreationTemplateValidationTest : BusinessObjectValidationTestCase
	{
		#region Tests

		public void TestCheckOpportunityDescription()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
			Assert(opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
			Assert(!opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityDescription = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
			Assert(opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());

			opportunityCreationTemplate.OpportunityDescription = "Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
			Assert(!opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());

			var registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			var registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_OpportunityDescription);

			registryItem.Bool = true;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.OpportunityDescription = string.Empty;
				opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
				Assert(opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());
			}

			registryItem.Bool = false;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.OpportunityDescription = string.Empty;
				opportunityCreationTemplate.Validation.ValidateOpportunityDescription();
				Assert(!opportunityCreationTemplate.OpportunityDescriptionInfo.HasErrors());
			}
		}

		public void TestCheckPackageType()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidatePackageType();
			Assert(!opportunityCreationTemplate.PackageTypeInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.PackageType = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidatePackageType();
			Assert(opportunityCreationTemplate.PackageTypeInfo.HasErrors());

			opportunityCreationTemplate.PackageType = "Invalid";
			opportunityCreationTemplate.Validation.ValidatePackageType();
			Assert(opportunityCreationTemplate.PackageTypeInfo.HasErrors());

			opportunityCreationTemplate.PackageType = "STD";
			opportunityCreationTemplate.Validation.ValidatePackageType();
			Assert(!opportunityCreationTemplate.PackageTypeInfo.HasErrors());

			var registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			var registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_PackageType);

			registryItem.Bool = true;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.PackageType = string.Empty;
				opportunityCreationTemplate.Validation.ValidatePackageType();
				Assert(opportunityCreationTemplate.PackageTypeInfo.HasErrors());
			}

			registryItem.Bool = false;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.PackageType = string.Empty;
				opportunityCreationTemplate.Validation.ValidatePackageType();
				Assert(!opportunityCreationTemplate.PackageTypeInfo.HasErrors());
			}
		}

		public void TestCheckOpportunityType()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Validation.ValidateOpportunityType();
			Assert(opportunityCreationTemplate.OpportunityTypeInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityType();
			Assert(!opportunityCreationTemplate.OpportunityTypeInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityType = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityType();
			Assert(opportunityCreationTemplate.OpportunityTypeInfo.HasErrors());

			opportunityCreationTemplate.OpportunityType = "Invalid";
			opportunityCreationTemplate.Validation.ValidateOpportunityType();
			Assert(opportunityCreationTemplate.OpportunityTypeInfo.HasErrors());

			opportunityCreationTemplate.OpportunityType = "UDF";
			opportunityCreationTemplate.Validation.ValidateOpportunityType();
			Assert(!opportunityCreationTemplate.OpportunityTypeInfo.HasErrors());
		}

		public void TestCheckOpportunityStage()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Validation.ValidateOpportunityStage();
			Assert(opportunityCreationTemplate.OpportunityStageInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityStage();
			Assert(!opportunityCreationTemplate.OpportunityStageInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityStage = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityStage();
			Assert(opportunityCreationTemplate.OpportunityStageInfo.HasErrors());

			opportunityCreationTemplate.OpportunityStage = "Invalid";
			opportunityCreationTemplate.Validation.ValidateOpportunityStage();
			Assert(opportunityCreationTemplate.OpportunityStageInfo.HasErrors());

			opportunityCreationTemplate.OpportunityStage = "UDF";
			opportunityCreationTemplate.Validation.ValidateOpportunityStage();
			Assert(!opportunityCreationTemplate.OpportunityStageInfo.HasErrors());
		}

		public void TestCheckOpportunityStatus()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Validation.ValidateOpportunityStatus();
			Assert(opportunityCreationTemplate.OpportunityStatusInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityStatus();
			Assert(!opportunityCreationTemplate.OpportunityStatusInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityStatus = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityStatus();
			Assert(opportunityCreationTemplate.OpportunityStatusInfo.HasErrors());

			opportunityCreationTemplate.OpportunityStatus = "Invalid";
			opportunityCreationTemplate.Validation.ValidateOpportunityStatus();
			Assert(opportunityCreationTemplate.OpportunityStatusInfo.HasErrors());

			opportunityCreationTemplate.OpportunityStatus = "CRT";
			opportunityCreationTemplate.Validation.ValidateOpportunityStatus();
			Assert(!opportunityCreationTemplate.OpportunityStatusInfo.HasErrors());
		}

		public void TestCheckSource()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Source = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateSource();
			Assert(opportunityCreationTemplate.SourceInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateSource();
			Assert(!opportunityCreationTemplate.SourceInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.Source = "Invalid";
			opportunityCreationTemplate.Validation.ValidateSource();
			Assert(opportunityCreationTemplate.SourceInfo.HasErrors());

			opportunityCreationTemplate.Source = "WEB";
			opportunityCreationTemplate.Validation.ValidateSource();
			Assert(!opportunityCreationTemplate.SourceInfo.HasErrors());

			var registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			var registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_Source);

			registryItem.Bool = true;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.Source = string.Empty;
				opportunityCreationTemplate.Validation.ValidateSource();
				Assert(opportunityCreationTemplate.SourceInfo.HasErrors());
			}

			registryItem.Bool = false;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.Source = string.Empty;
				opportunityCreationTemplate.Validation.ValidateSource();
				Assert(!opportunityCreationTemplate.SourceInfo.HasErrors());
			}
		}

		public void TestCheckSourceDetails()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection
			{
				{ "WEB", (NoResString)"Web Campaign", true, "CL2" },
				{ "MKT", (NoResString)"Marketing Campaign", false, "CL2" }
			};
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.SourceDetails = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateSourceDetails();
			Assert(opportunityCreationTemplate.SourceDetailsInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateSourceDetails();
			Assert(!opportunityCreationTemplate.SourceDetailsInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.SourceDetails = "Invalid";
			opportunityCreationTemplate.Validation.ValidateSourceDetails();
			Assert(opportunityCreationTemplate.SourceDetailsInfo.HasErrors());

			opportunityCreationTemplate.Source = "MKT";
			opportunityCreationTemplate.SourceDetails = "PREAP";
			opportunityCreationTemplate.Validation.ValidateSourceDetails();
			Assert(!opportunityCreationTemplate.SourceDetailsInfo.HasErrors());
		}

		public void TestCheckActiveSourceDetails()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.ActiveSourceDetails = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateActiveSourceDetails();
			Assert(opportunityCreationTemplate.ActiveSourceDetailsInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateActiveSourceDetails();
			Assert(!opportunityCreationTemplate.ActiveSourceDetailsInfo.HasErrors());

			opportunityCreationTemplate.ActiveSourceDetails = "Test";
			opportunityCreationTemplate.Validation.ValidateActiveSourceDetails();
			Assert(!opportunityCreationTemplate.ActiveSourceDetailsInfo.HasErrors());
		}

		public void TestCheckOpportunityAssignment()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.Validation.ValidateOpportunityAssignment();
			Assert(opportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplate.OpportunityAssignment = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateOpportunityAssignment();
			Assert(opportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityAssignment();
			Assert(!opportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityAssignment = "Invalid";
			opportunityCreationTemplate.Validation.ValidateOpportunityAssignment();
			Assert(opportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplate.OpportunityAssignment = "IND";
			opportunityCreationTemplate.Validation.ValidateOpportunityAssignment();
			Assert(!opportunityCreationTemplate.OpportunityAssignmentInfo.HasErrors());
		}

		public void TestCheckOpportunityAssignment_MatchParentTouchSender()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = masterCampaign.PK;
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch1A = CreateOpportunityCreationTemplate(touch1A, "Test 1A");
			masterCampaign.AllTouches.Add(touch1A);

			var touch1B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";
			touch1B.G0_G0_Master = masterCampaign.PK;
			touch1B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch1B = CreateOpportunityCreationTemplate(touch1B, "Test 1B");
			masterCampaign.AllTouches.Add(touch1B);

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_G0_Master = masterCampaign.PK;
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch2A = CreateOpportunityCreationTemplate(touch2A, "Test 2A");
			masterCampaign.AllTouches.Add(touch2A);

			var touch2B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2B.G0_HorizontalId = 2;
			touch2B.G0_VerticalId = "B";
			touch2B.G0_G0_Master = masterCampaign.PK;
			touch2B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch2B = CreateOpportunityCreationTemplate(touch2B, "Test 2B");
			masterCampaign.AllTouches.Add(touch2B);

			var touch2C = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2C.G0_HorizontalId = 2;
			touch2C.G0_VerticalId = "C";
			touch2C.G0_G0_Master = masterCampaign.PK;
			touch2C.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch2C = CreateOpportunityCreationTemplate(touch2C, "Test 2C");
			masterCampaign.AllTouches.Add(touch2C);

			var touch3A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch3A.G0_HorizontalId = 3;
			touch3A.G0_VerticalId = "A";
			touch3A.G0_G0_Master = masterCampaign.PK;
			touch3A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch3A = CreateOpportunityCreationTemplate(touch3A, "Test 3A");
			masterCampaign.AllTouches.Add(touch3A);
			Factory.Save();

			opportunityCreationTemplateTouch1A.Validation.ValidateOpportunityAssignment();
			Assert("Should have an error, because Touch 1A it is on the first touch series", opportunityCreationTemplateTouch1A.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplateTouch1B.Validation.ValidateOpportunityAssignment();
			Assert("Should have an error, because Touch 1B it is on the first touch series", opportunityCreationTemplateTouch1B.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplateTouch2A.Validation.ValidateOpportunityAssignment();
			Assert("Should not have an error, because Touch 2A it is on the second touch series", !opportunityCreationTemplateTouch2A.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplateTouch2B.Validation.ValidateOpportunityAssignment();
			Assert("Should not have an error, because Touch 2B it is on the second touch series", !opportunityCreationTemplateTouch2B.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplateTouch2C.Validation.ValidateOpportunityAssignment();
			Assert("Should not have an error, because Touch 2C it is on the second touch series", !opportunityCreationTemplateTouch2C.OpportunityAssignmentInfo.HasErrors());

			opportunityCreationTemplateTouch3A.Validation.ValidateOpportunityAssignment();
			Assert("Should not have an error, because Touch 3A it is on the third touch series", !opportunityCreationTemplateTouch3A.OpportunityAssignmentInfo.HasErrors());
		}

		OpportunityCreationTemplate CreateOpportunityCreationTemplate(GlbCompanyCampaign touch, string opportunityDescription)
		{
			return new OpportunityCreationTemplate(touch)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = opportunityDescription,
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender
			};
		}

		public void TestCheckSalesPerson()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.StaffAssignment;
			opportunityCreationTemplate.Validation.ValidateSalesPerson();
			Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());

			opportunityCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;
			opportunityCreationTemplate.SalesPerson = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateSalesPerson();
			Assert(opportunityCreationTemplate.SalesPersonInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateSalesPerson();
			Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			opportunityCreationTemplate.SalesPerson = "VBI";
			opportunityCreationTemplate.Validation.ValidateSalesPerson();
			Assert(opportunityCreationTemplate.SalesPersonInfo.HasErrors());

			opportunityCreationTemplate.SalesPerson = staff.GS_Code;
			opportunityCreationTemplate.Validation.ValidateSalesPerson();
			Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());

			var registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			var registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson);

			registryItem.Bool = true;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.SalesPerson = GlbStaff.CurrentUser.GS_Code;
				opportunityCreationTemplate.Validation.ValidateSalesPerson();
				Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());

				opportunityCreationTemplate.SalesPerson = "";
				opportunityCreationTemplate.Validation.ValidateSalesPerson();
				Assert(opportunityCreationTemplate.SalesPersonInfo.HasErrors());

				opportunityCreationTemplate.SalesPerson = staff.GS_Code;
				opportunityCreationTemplate.Validation.ValidateSalesPerson();
				Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());

				Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
				opportunityCreationTemplate.SalesPerson = string.Empty;
				opportunityCreationTemplate.Validation.ValidateSalesPerson();
				Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());
			}

			registryItem.Bool = false;
			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunityCreationTemplate.SalesPerson = string.Empty;
				opportunityCreationTemplate.Validation.ValidateSalesPerson();
				Assert(!opportunityCreationTemplate.SalesPersonInfo.HasErrors());
			}
		}

		public void TestCheckStaffAssignment()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.StaffAssignment = $"Test {opportunityCreationTemplate.Separator} Test";
			opportunityCreationTemplate.Validation.ValidateStaffAssignment();
			Assert(opportunityCreationTemplate.StaffAssignmentInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateStaffAssignment();
			Assert(!opportunityCreationTemplate.StaffAssignmentInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.StaffAssignment = "BLAH";
			opportunityCreationTemplate.Validation.ValidateStaffAssignment();
			Assert(opportunityCreationTemplate.StaffAssignmentInfo.HasErrors());

			opportunityCreationTemplate.StaffAssignment = "ACT";
			opportunityCreationTemplate.Validation.ValidateStaffAssignment();
			Assert(!opportunityCreationTemplate.StaffAssignmentInfo.HasErrors());
		}

		public void TestCheckOpportunityNotes()
		{
			var opportunityCreationTemplate = new OpportunityCreationTemplate(Campaign);

			opportunityCreationTemplate.OpportunityNotes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf($"Test {opportunityCreationTemplate.Separator} Test"));
			opportunityCreationTemplate.Validation.ValidateOpportunityNotes();
			Assert(opportunityCreationTemplate.OpportunityNotesInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			opportunityCreationTemplate.Validation.ValidateOpportunityNotes();
			Assert(!opportunityCreationTemplate.OpportunityNotesInfo.HasErrors());

			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			opportunityCreationTemplate.OpportunityNotes = ZBlob.FromUTF8("Valid Value");
			opportunityCreationTemplate.Validation.ValidateOpportunityNotes();
			Assert(!opportunityCreationTemplate.OpportunityNotesInfo.HasErrors());
		}

		public void TestCheckOpportunityAssignment_StaffPoolAssignments()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "teststaffpool1@test.com";
			staff1.GS_Code = "TS1";
			staff1.GS_FullName = "Test Staff Pool 1";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = masterCampaign.PK;
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var opportunityCreationTemplateTouch1A = new OpportunityCreationTemplate(touch1A)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test 1A",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityAssignment = OpportunityAssignmentList.Codes.StaffPoolAssignments
			};
			masterCampaign.AllTouches.Add(touch1A);

			var touch1B = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";
			touch1B.G0_G0_Master = masterCampaign.PK;
			touch1B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			const string invalidStaffCode = "ISC";
			var poolItem1B = touch1B.SenderPool.AddNew();
			poolItem1B.GCP_GS_NKSender = invalidStaffCode;
			poolItem1B.GCP_SendRatio = 1;

			var opportunityCreationTemplateTouch1B = new OpportunityCreationTemplate(touch1B)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test 1B",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityAssignment = OpportunityAssignmentList.Codes.StaffPoolAssignments
			};
			masterCampaign.AllTouches.Add(touch1B);

			var touch1C = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1C.G0_HorizontalId = 1;
			touch1C.G0_VerticalId = "C";
			touch1C.G0_G0_Master = masterCampaign.PK;
			touch1C.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			var poolItem1C = touch1C.SenderPool.AddNew();
			poolItem1C.GCP_GS_NKSender = staff1.GS_Code;
			poolItem1C.GCP_SendRatio = 1;

			var opportunityCreationTemplateTouch1C = new OpportunityCreationTemplate(touch1C)
			{
				PackageType = "STD",
				OpportunityType = "UDF",
				OpportunityDescription = "Test 1C",
				OpportunityStatus = "CRT",
				OpportunityStage = "UDF",
				Source = "WEB",
				ActiveSourceDetails = "NOT",
				OpportunityAssignment = OpportunityAssignmentList.Codes.StaffPoolAssignments
			};
			masterCampaign.AllTouches.Add(touch1C);
			Factory.Save();

			opportunityCreationTemplateTouch1A.Validation.ValidateOpportunityAssignment();
			Assert(opportunityCreationTemplateTouch1A.OpportunityAssignmentInfo.HasErrors());
			AssertContains("Staff Pool Assignments cannot be empty.", opportunityCreationTemplateTouch1A.OpportunityAssignmentInfo.Notifications.GetFirstMessage());

			opportunityCreationTemplateTouch1B.Validation.ValidateOpportunityAssignment();
			Assert(opportunityCreationTemplateTouch1B.OpportunityAssignmentInfo.HasErrors());
			AssertContains("Errors in Staff Pool Assignments.", opportunityCreationTemplateTouch1B.OpportunityAssignmentInfo.Notifications.GetFirstMessage());

			opportunityCreationTemplateTouch1C.Validation.ValidateOpportunityAssignment();
			Assert(!opportunityCreationTemplateTouch1C.OpportunityAssignmentInfo.HasErrors());
		}

		public void TestCheckForInvalidChar_ThrowsExceptionIfPropertyNotZString()
		{
			var template = new OpportunityCreationTemplate(Campaign);
			var templateValidation = new OpportunityCreationTemplateValidation_CheckOpportunityNotesThrowsException(template);

			templateValidation.ValidateOpportunityNotes();
			Assert("Error should be added when CheckForInvalidChar() used on non-ZString property.", template.OpportunityNotesInfo.HasError(templateValidation.InvalidCharacterMessageExposed));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
		}

		GlbCompanyCampaign Campaign => campaign ?? (campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>());
		GlbCompanyCampaign campaign;

		class OpportunityCreationTemplateValidation_CheckOpportunityNotesThrowsException : OpportunityCreationTemplateValidation
		{
			public OpportunityCreationTemplateValidation_CheckOpportunityNotesThrowsException(OpportunityCreationTemplate parent)
				: base(parent) { }

			protected override void CheckOpportunityNotes()
			{
				CheckForInvalidChar(Parent.OpportunityNotesInfo);
			}

			public string InvalidCharacterMessageExposed => InvalidCharacterMessage;
		}

		#endregion
	}
}
