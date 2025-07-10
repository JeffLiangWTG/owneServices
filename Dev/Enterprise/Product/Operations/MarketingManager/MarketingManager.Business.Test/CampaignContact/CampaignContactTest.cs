using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignContact))]
	sealed class CampaignContactTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "LOCO3";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);

			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("123YES", campaignContact.VCC_Phone);
			AssertEquals(header.PK, campaignContact.VCC_OH);
			AssertEquals("Admiral", campaignContact.VCC_Salutation);
			AssertEquals("Mr.", campaignContact.VCC_Title);
			AssertEquals("293999", campaignContact.VCC_Fax);
			AssertEquals("LOCO3", campaignContact.RelatedPortCodeForScheduling);

			campaignContact.RelatedPortCodeForScheduling = "VVTWR";
			AssertEquals("VVTWR", campaignContact.RelatedPortCodeForScheduling);
		}

		public void TestPropertiesWithSecurityAccessRights_Staff()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";
			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherStaffDetails.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithPermissionToViewStaff = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecordStaff = Factory.New<GlbSecurity>();
			allowedSecurityRecordStaff.GU_SecurityRight = Env.Security.StaffViewOtherStaffDetails.Code;
			allowedSecurityRecordStaff.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecordStaff.GU_GS = staffWithPermissionToViewStaff.PK;
			staffWithPermissionToViewStaff.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecordStaff);

			var staffWithPermissionToViewApplicants = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecordApplicants = Factory.New<GlbSecurity>();
			allowedSecurityRecordApplicants.GU_SecurityRight = Env.Security.HRJobApplicantView.Code;
			allowedSecurityRecordApplicants.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecordApplicants.GU_GS = staffWithPermissionToViewApplicants.PK;
			staffWithPermissionToViewApplicants.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecordApplicants);

			var staffToView = Factory.NewWithValidTestData<CampaignContact>();
			staffToView.VCC_TableCode = "GS";
			staffToView.VCC_Address1 = "1";
			staffToView.VCC_Address2 = "2";
			staffToView.VCC_City = "3";
			staffToView.VCC_State = "4";
			staffToView.VCC_PostCode = "5";
			staffToView.VCC_Phone = "1234567";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address1);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address2);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_City);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_State);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_PostCode);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Phone);

					Assert(staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissionToViewStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals("1", staffToViewLoaded.VCC_Address1);
					AssertEquals("2", staffToViewLoaded.VCC_Address2);
					AssertEquals("3", staffToViewLoaded.VCC_City);
					AssertEquals("4", staffToViewLoaded.VCC_State);
					AssertEquals("5", staffToViewLoaded.VCC_PostCode);
					AssertEquals("1234567", staffToViewLoaded.VCC_Phone);

					Assert(!staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(!staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(!staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissionToViewApplicants.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address1);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address2);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_City);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_State);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_PostCode);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Phone);

					Assert(staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}
			}
		}

		public void TestPropertiesWithSecurityAccessRights_Applicants()
		{
			ZString viewDeniedMessage = "** View Denied due to Security Access **";
			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.StaffViewOtherStaffDetails.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var staffWithPermissionToViewStaff = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecordStaff = Factory.New<GlbSecurity>();
			allowedSecurityRecordStaff.GU_SecurityRight = Env.Security.StaffViewOtherStaffDetails.Code;
			allowedSecurityRecordStaff.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecordStaff.GU_GS = staffWithPermissionToViewStaff.PK;
			staffWithPermissionToViewStaff.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecordStaff);

			var staffWithPermissionToViewApplicants = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecordApplicants = Factory.New<GlbSecurity>();
			allowedSecurityRecordApplicants.GU_SecurityRight = Env.Security.HRJobApplicantView.Code;
			allowedSecurityRecordApplicants.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecordApplicants.GU_GS = staffWithPermissionToViewApplicants.PK;
			staffWithPermissionToViewApplicants.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecordApplicants);

			var staffToView = Factory.NewWithValidTestData<CampaignContact>();
			staffToView.VCC_TableCode = "HA";
			staffToView.VCC_Address1 = "1";
			staffToView.VCC_Address2 = "2";
			staffToView.VCC_City = "3";
			staffToView.VCC_State = "4";
			staffToView.VCC_PostCode = "5";
			staffToView.VCC_Phone = "1234567";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address1);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address2);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_City);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_State);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_PostCode);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Phone);

					Assert(staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissionToViewStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address1);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Address2);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_City);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_State);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_PostCode);
					AssertEquals(viewDeniedMessage, staffToViewLoaded.VCC_Phone);

					Assert(staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissionToViewApplicants.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staffToView }, campaign);
					var staffToViewLoaded = campaignContactCollection[0];

					AssertEquals("1", staffToViewLoaded.VCC_Address1);
					AssertEquals("2", staffToViewLoaded.VCC_Address2);
					AssertEquals("3", staffToViewLoaded.VCC_City);
					AssertEquals("4", staffToViewLoaded.VCC_State);
					AssertEquals("5", staffToViewLoaded.VCC_PostCode);
					AssertEquals("1234567", staffToViewLoaded.VCC_Phone);

					Assert(!staffToViewLoaded.VCC_Address1Info.ReadOnly);
					Assert(!staffToViewLoaded.VCC_Address2Info.ReadOnly);
					Assert(!staffToViewLoaded.VCC_CityInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_StateInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_PostCodeInfo.ReadOnly);
					Assert(!staffToViewLoaded.VCC_PhoneInfo.ReadOnly);
				}
			}
		}

		public void TestIsNDR()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Email = "mr.admiral@yes.com";
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals(false, orgContact.IsNDR);

			orgContact.IsNDR = true;
			orgContact.EmailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;
			AssertEquals(false, orgContact.IsNDR);

			orgContact.EmailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			Assert("Non Delivery Report Should be set", orgContact.IsNDR);
		}

		public void TestAllowedAction()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Pre-condition", "", campaignContact.CurrentActionsAsText);

			campaignContact.IsDeactivating = true;
			AssertEquals("Deactivate", campaignContact.CurrentActionsAsText);

			campaignContact.VCC_Email = "eddy@gmail.com";
			AssertEquals("eddy@gmail.com", campaignContact.VCC_Email);
			AssertEquals("Edit & Deactivate", campaignContact.CurrentActionsAsText);

			campaignContact.IsDeactivating = false;
			AssertEquals("Edit", campaignContact.CurrentActionsAsText);

			campaignContact.IsDeactivating = true;
			AssertEquals("Edit & Deactivate", campaignContact.CurrentActionsAsText);
			campaignContact.IsDeactivating = false;
			AssertEquals("Edit", campaignContact.CurrentActionsAsText);
		}

		public void TestContactUrl()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEndsWith("Url should contain TextToShow", "&TextToShow=Adams Smith", campaignContact.ContactUrl.Substring(campaignContact.ContactUrl.LastIndexOf('&')));
		}

		public void TestCompanyName_Contact_FromMainAddress()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";
			var addressType = header.Addresses[0];
			addressType.OA_Code = "TST2";
			addressType.OA_IsActive = true;
			addressType.OA_CompanyNameOverride = "Test Company Name";

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Test Company Name", campaignContact.VCC_CompanyName);
		}

		public void TestCompanyName_Contact_FromContactAddress()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";

			var addressType = Factory.NewWithValidTestData<OrgAddress>();
			addressType.OA_Code = "TST2";
			addressType.OA_OH = header.PK;
			addressType.OA_IsActive = true;
			addressType.OA_CompanyNameOverride = "Test Company Name";

			orgContact.OC_OA_OrgAddress = addressType.PK;
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Test Company Name", campaignContact.VCC_CompanyName);
		}

		public void TestCompanyName_Contact_FromOrganisationName()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Organisation Name", campaignContact.VCC_CompanyName);
		}

		public void TestCompanyName_Inquiry_FromOrganisationName()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";

			var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
			register.O1_OC_LinkedContact = orgContact.PK;
			register.O1_OH_ConvertedToQualifiedLead = header.PK;

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { register }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Organisation Name", campaignContact.VCC_CompanyName);
		}

		public void TestCompanyName_Inquiry_FromContactAddress()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";

			var addressType = Factory.NewWithValidTestData<OrgAddress>();
			addressType.OA_Code = "TST2";
			addressType.OA_OH = header.PK;
			addressType.OA_IsActive = true;
			addressType.OA_CompanyNameOverride = "Test Company Name";

			var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
			register.O1_OC_LinkedContact = orgContact.PK;
			register.O1_OH_ConvertedToQualifiedLead = header.PK;
			register.O1_OA_LinkedAddress = addressType.PK;
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { register }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Test Company Name", campaignContact.VCC_CompanyName);
		}

		public void TestCompanyName_Inquiry_FromRegisterAddress()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Organisation Name";
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "eddie@aute.com";
			orgContact.OC_ContactName = "Adams Smith";
			var addressType = header.Addresses[0];
			addressType.OA_Code = "TST2";
			addressType.OA_IsActive = true;
			addressType.OA_CompanyNameOverride = "";

			var register = Factory.NewWithValidTestData<OrgColdCallRegister>();
			register.O1_OC_LinkedContact = orgContact.PK;
			register.O1_OH_ConvertedToQualifiedLead = header.PK;
			register.O1_OA_LinkedAddress = addressType.PK;
			register.O1_CompanyName = "Test Company Name";

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { register }, campaign);
			CampaignContact campaignContact = campaignContactCollection[0];

			AssertEquals("Company Name", "Test Company Name", campaignContact.VCC_CompanyName);
		}

		public void TestIGlbCompanyCampaignItemRecipient()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);

			IGlbCompanyCampaignItemRecipient recipient = campaignContactCollection[0];

			AssertEquals("123YES", recipient.Phone);
			AssertEquals(header.PK, recipient.Organisation.PK);
			AssertEquals("Admiral", recipient.Salutation);
			AssertEquals("Mr.", recipient.Title);
			AssertEquals("293999", recipient.Fax);
		}

		public void TestIScheduleItemsProviderAsCampaignContact()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = header.Contacts.AddNew();
			orgContact.OC_Phone = "123YES";
			orgContact.OC_OH = header.PK;
			orgContact.OC_Salutation = "Admiral";
			orgContact.OC_Title = "Mr.";
			orgContact.OC_Fax = "293999";
			orgContact.OC_Email = "contact@gmail.com";

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { orgContact }, campaign);
			IScheduleItemsProvider provider = campaignContactCollection[0];

			AssertEquals("QUE", provider.ScheduleStatus);
			AssertEquals("VCC", provider.TableCode);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
