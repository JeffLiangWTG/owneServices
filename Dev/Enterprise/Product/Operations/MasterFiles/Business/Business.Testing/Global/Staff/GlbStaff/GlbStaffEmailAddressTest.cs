using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffEmailAddress))]
	sealed class GlbStaffEmailAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEmailType()
		{
			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList))
			{
				EmailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;

				AssertEquals(Core.Constants.EmailFromAddressTypes.Descriptions.Main, EmailAddress.EmailType);

				EmailAddress.EmailType = "Import";

				AssertEquals("IMP", EmailAddress.GSE_Type);
			}
		}

		public void TestEmailTypeFromOtherCompany()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				AssertNull(EmailAddress.EmailTypeFromOtherCompany);

				var emailAddress1 = Factory.New<GlbStaffEmailAddress>();
				emailAddress1.GSE_GC_Company = company.PK;
				emailAddress1.GSE_Type = "IMP";

				AssertEquals("Import (WTG)", emailAddress1.EmailTypeFromOtherCompany);

				var emailAddress2 = Factory.New<GlbStaffEmailAddress>();
				emailAddress2.GSE_GC_Company = company.PK;
				emailAddress2.GSE_Type = "ERR";

				AssertEquals("ERR (WTG)", emailAddress2.EmailTypeFromOtherCompany);
			}
		}

		public void TestCanDeleteIsUsedByScheduleTaskRecipients()
		{
			ResourceString reasonForNotAbleToDelete = ResString.GetMultilingualString("B41AD2F1-71C5-4631-851F-28A6A7378902", "Main email address cannot be deleted or this email address has been used in scheduled task email from address.");
			staff.GS_EmailAddress = "main@wtg.com";
			using (EmailAddress.SuspendSettingHasChanges())
			{
				EmailAddress.GSE_EmailAddress = "tst@wtg.com";
			}

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("TST", "Test");

			var mainEmail = staff.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main);

			var scheduleTask = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
			(scheduleTask as BusinessObject)[StmScheduleTaskSchema.S5_GS_NKPrintUser] = staff.GS_Code;
			scheduleTask.S5_ScheduleType = "REP";

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			scheduleTaskRecipient.S6_GS_NKRecipient = staff.GS_Code;
			scheduleTaskRecipient.S6_EmailFromAddress = staff.GS_EmailAddress;
			scheduleTaskRecipient.S6_S5 = (scheduleTask as BusinessObject).PK;
			Factory.Save();

			AssertEquals("We cant delete main email address in any cases.", false, mainEmail.CanDelete);
			AssertEquals(reasonForNotAbleToDelete, mainEmail.ReasonForNotAbleToDelete);
			AssertEquals("Can delete customized email address if not used by scheduleTaskRecipient email from address", true, EmailAddress.CanDelete);

			scheduleTaskRecipient.S6_EmailFromAddress = EmailAddress.GSE_EmailAddress;
			Factory.Save();
			AssertEquals("We cant delete main email address in any cases.", false, mainEmail.CanDelete);
			AssertEquals(reasonForNotAbleToDelete, mainEmail.ReasonForNotAbleToDelete);
			AssertEquals("Cant delete customized email address if used by scheduleTaskRecipient email from address", false, EmailAddress.CanDelete);
			AssertEquals(reasonForNotAbleToDelete, EmailAddress.ReasonForNotAbleToDelete);
		}

		public void TestGSE_EmailAddress()
		{
			var emailAddress = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			emailAddress.GSE_GS = staff.PK;
			emailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;
			emailAddress.GSE_Type = "TEST";
			var deliveryTime = new ZDateTime(2022, 11, 3);

			AssertEquals(false, emailAddress.IsNDR);
			AssertEquals(false, emailAddress.IsNDRInfo.HasWarning((NoResString)$"A Non-Delivery Receipt has been received at {deliveryTime.ToLocalBranchTime().ToLongTimeString()}."));

			GlbEmailAddress emailAddr = Factory.New<GlbEmailAddress>();
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "test@tester.com";
			emailAddr.GI_DeliveryReportTimeUtc = deliveryTime;

			emailAddress.GSE_EmailAddress = "test@tester.com";
			AssertEquals(true, emailAddress.IsNDR);
			Assert(emailAddress.IsNDRInfo.HasWarning((NoResString)$"A Non-Delivery Receipt has been received at {deliveryTime.ToLocalBranchTime().ToLongTimeString()}."));
		}

		public void TestNonDeliveryReport()
		{
			var emailAddress = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			emailAddress.GSE_GS = staff.PK;
			emailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;
			emailAddress.GSE_Type = "TEST";
			emailAddress.GSE_EmailAddress = "test@tester.com";

			AssertEquals(false, emailAddress.IsNDR);
			AssertEquals(ZDateTime.Empty, emailAddress.DeliveryReportTimeUtc);

			GlbEmailAddress emailAddr = Factory.New<GlbEmailAddress>();
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "test@tester.com";
			emailAddr.GI_DeliveryReportTimeUtc = new ZDateTime(2022, 11, 3);

			AssertEquals(true, emailAddress.IsNDR);
			AssertEquals(new ZDateTime(2022, 11, 3), emailAddress.DeliveryReportTimeUtc);
		}

		public void TestIsNDR_ReadOnly()
		{
			var emailAddress = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			emailAddress.GSE_EmailAddress = "test@tester.com";

			AssertEquals(false, emailAddress.IsNDR);
			AssertEquals(true, emailAddress.IsNDR_ReadOnly);

			GlbEmailAddress emailAddr = Factory.New<GlbEmailAddress>();
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "test@tester.com";

			AssertEquals(true, emailAddress.IsNDR);
			AssertEquals(false, emailAddress.IsNDR_ReadOnly);
		}

		public void TestSetIsNDRShouldSetHasChanges()
		{
			EmailAddress.GSE_EmailAddress = "test@tester.com";

			var emailAddr = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "test@tester.com";
			emailAddr.GI_DeliveryReportTimeUtc = new ZDateTime(2022, 11, 3);
			Factory.Save();
			AssertEquals("Precondition", false, EmailAddress.HasChanges);
			AssertEquals("Precondition", false, EmailAddress.IsNDRInfo.HasChanges);

			var otherFactory = new BusinessObjectFactory();
			var emailAddressInNewFactory = otherFactory.Load<GlbStaffEmailAddress>(EmailAddress.PK);
			var emailAddrInNewFactory = otherFactory.Load<GlbEmailAddress>(emailAddr.PK);
			AssertEquals("Should be NDR", true, emailAddressInNewFactory.IsNDR);
			AssertEquals("Precondition", false, emailAddressInNewFactory.HasChanges);

			emailAddressInNewFactory.IsNDR = true;
			AssertEquals("HasChanges should not change since the value is the same", false, emailAddressInNewFactory.HasChanges);

			emailAddressInNewFactory.IsNDR = false;
			AssertEquals("HasChanges should be set to true", true, emailAddressInNewFactory.HasChanges);
			AssertNotEquals("Delivery status for GlbEmailAddress should be changed as well.", "NDR", emailAddrInNewFactory.GI_DeliveryStatus);
		}

		public void TestIsPubliclyVisible()
		{
			var emailAddress = Factory.NewWithValidTestData<GlbStaffEmailAddress>();

			AssertEquals(true, emailAddress.IsVisible);

			emailAddress.GSE_IsVisible = false;

			AssertEquals(false, emailAddress.IsVisible);
		}

		public void TestSetIsVisibleShouldSetHasChanges()
		{
			EmailAddress.GSE_EmailAddress = "test@tester.com";

			Factory.Save();
			AssertEquals("Precondition", false, EmailAddress.HasChanges);
			AssertEquals("Precondition", false, EmailAddress.IsVisibleInfo.HasChanges);

			var newFactory = new BusinessObjectFactory();
			var emailAddressInNewFactory = newFactory.Load<GlbStaffEmailAddress>(EmailAddress.PK);
			AssertEquals("Should be Visible", true, emailAddressInNewFactory.IsVisible);
			AssertEquals("Precondition", false, emailAddressInNewFactory.HasChanges);

			emailAddressInNewFactory.IsVisible = true;
			AssertEquals("HasChanges should not change since the value is the same", false, emailAddressInNewFactory.HasChanges);

			emailAddressInNewFactory.IsVisible = false;
			AssertEquals("HasChanges should be set to true", true, emailAddressInNewFactory.HasChanges);
		}

		public void TestMainAddressReadOnly()
		{
			staff.GS_ExternalId = "";
			EmailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;

			AssertEquals(false, EmailAddress.ReadOnly);

			staff.GS_ExternalId = "123";
			AssertEquals(true, EmailAddress.GSE_EmailAddressInfo.ReadOnly);

			staff.GS_ExternalId = "1234";
			EmailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Default;
			AssertEquals(false, EmailAddress.GSE_EmailAddressInfo.ReadOnly);
		}

		GlbStaffEmailAddress EmailAddress;
		GlbStaff staff;

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.NewWithValidTestData<GlbStaff>();
			EmailAddress = (GlbStaffEmailAddress)GetNewBusinessObject();
			EmailAddress.GSE_GS = staff.PK;
			EmailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;
			EmailAddress.GSE_Type = "TEST";

			SystemDataRegistry.Instance.EnableScimService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var emailAddress = Factory.New<GlbStaffEmailAddress>();
			emailAddress.GSE_EmailAddress = "aaa@a.com";
			return emailAddress;
		}
	}
}
