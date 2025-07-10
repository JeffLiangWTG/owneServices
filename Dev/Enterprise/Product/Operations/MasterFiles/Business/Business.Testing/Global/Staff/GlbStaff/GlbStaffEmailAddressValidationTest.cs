using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffEmailAddressValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckGSE_EmailAddress

		public void TestCheckGSE_Type()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "main@test.com";

			var emailAddresses = staff.EmailAddresses;
			var emailAddress = emailAddresses.AddNew();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList))
			{
				CombineAssertions(() =>
				{
					emailAddress.GSE_Type = "";
					AssertMandatoryValidationError(emailAddress.GSE_TypeInfo, true);

					emailAddress.GSE_Type = "TEST";
					AssertListValidationInvalidCodeError(emailAddress.GSE_TypeInfo, true);

					emailAddress.GSE_Type = "IMP";
					AssertNoErrors(emailAddress.GSE_TypeInfo);
				});
			}

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";
			emailAddress.GSE_GC_Company = company.PK;

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				emailAddress.GSE_Type = "ERR";
				AssertHasError(emailAddress.GSE_TypeInfo, "Company 'WiseTech' does not contain Email Type 'ERR'");
			}
		}

		public void TestCheckGSE_Type_UniqueInCollection()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "main@test.com";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList))
			{
				var emailAddresses = staff.EmailAddresses;
				var emailAddress1 = emailAddresses.AddNew();
				emailAddress1.GSE_Type = "IMP";

				var emailAddress2 = emailAddresses.AddNew();
				emailAddress2.GSE_Type = "IMP";

				var emailAddress3 = emailAddresses.AddNew();
				emailAddress3.GSE_Type = "EXP";

				var emailAddress4 = emailAddresses.AddNew();
				emailAddress4.GSE_GC_Company = company.PK;
				emailAddress4.GSE_Type = "EXP";

				AssertPropertyIsUniqueInCollectionValidationError(emailAddress1.EmailTypeInfo, false);
				AssertPropertyIsUniqueInCollectionValidationError(emailAddress2.EmailTypeInfo, true);
				AssertPropertyIsUniqueInCollectionValidationError(emailAddress3.EmailTypeInfo, false);
				AssertPropertyIsUniqueInCollectionValidationError(emailAddress4.EmailTypeInfo, false);
			}
		}

		public void TestCheckGSE_EmailAddress()
		{
			var staff = Factory.New<GlbStaff>();
			var emailAddress = staff.EmailAddresses.AddNew();

			CombineAssertions(() =>
			{
				emailAddress.GSE_EmailAddress = "test invalid email";
				AssertHasErrorContaining(emailAddress.GSE_EmailAddressInfo, "test invalid email is not a valid email address.");

				emailAddress.GSE_EmailAddress = "test@test.com";
				AssertNoErrors(emailAddress.GSE_EmailAddressInfo);

				emailAddress.GSE_EmailAddress = "";
				AssertMandatoryValidationError(emailAddress.GSE_EmailAddressInfo, true);

				staff.EmailAddresses[0].GSE_EmailAddress = ZString.Empty;
				AssertNoErrors(staff.EmailAddresses[0].GSE_EmailAddressInfo);
			});
		}

		public void TestCheckGSE_EmailAddress_UniqueInCollection()
		{
			var staff = Factory.New<GlbStaff>();
			var emailAddress1 = staff.EmailAddresses.AddNew();
			emailAddress1.GSE_EmailAddress = "my address";
			var emailAddress2 = staff.EmailAddresses.AddNew();
			emailAddress2.GSE_EmailAddress = "my address";
			var emailAddress3 = staff.EmailAddresses.AddNew();
			emailAddress3.GSE_EmailAddress = "my address 2";

			AssertPropertyIsUniqueInCollectionValidationError(emailAddress2.GSE_EmailAddressInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(emailAddress3.GSE_EmailAddressInfo, false);
		}

		public void TestShouldValidateFKToCancelledRecord_NoErrorsForMainEmailAddress()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "main@test.com";
			staff.GS_IsActive = false;

			var mainEmailAddress = staff.EmailAddresses[0];

			Assert(!mainEmailAddress.GSE_GSInfo.HasErrors());
		}

		public void TestValidateStaffScheduleTaskRecipient()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "JBA";
			staff.GS_LoginName = "test";

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Staff;
			scheduleTaskRecipient.S6_GS_NKRecipient = staff.GS_Code;

			Factory.Save();

			var mainEmailAddress = staff.EmailAddresses[0];

			mainEmailAddress.GSE_EmailAddress = ZString.Empty;
			mainEmailAddress.RunPreSaveValidation();
			AssertHasError(mainEmailAddress.GSE_EmailAddressInfo, "Email cannot set to empty because staff is assigned to recipient of scheduled reports.");

			mainEmailAddress.GSE_EmailAddress = "test@test.com";
			mainEmailAddress.RunPreSaveValidation();
			AssertNoErrors(mainEmailAddress.GSE_EmailAddressInfo);

			// 2nd email shouldn't have schedule task recipient error even though it is empty
			var secondEmail = staff.EmailAddresses.AddNew();
			secondEmail.GSE_EmailAddress = ZString.Empty;
			secondEmail.RunPreSaveValidation();
			AssertNoErrorContaining(secondEmail.GSE_EmailAddressInfo, "Email cannot set to empty because staff is assigned to recipient of scheduled reports.");
		}

		public void TestValidateGroupScheduleTaskRecipient()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "ST1";

			group.Staff.Add(staff1);

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
			scheduleTaskRecipient.S6_GG = group.PK;

			Factory.Save();

			var mainEmailAddress = staff1.EmailAddresses[0];
			mainEmailAddress.GSE_EmailAddress = ZString.Empty;
			mainEmailAddress.RunPreSaveValidation();
			AssertHasError(mainEmailAddress.GSE_EmailAddressInfo, "Email cannot set to empty because staff is assigned to group recipient of scheduled reports.");

			mainEmailAddress.GSE_EmailAddress = "test@test.com";
			mainEmailAddress.RunPreSaveValidation();
			AssertNoErrors(mainEmailAddress.GSE_EmailAddressInfo);

			// 2nd email shouldn't have schedule task recipient error even though it is empty
			var secondEmail = staff1.EmailAddresses.AddNew();
			secondEmail.GSE_EmailAddress = ZString.Empty;
			secondEmail.RunPreSaveValidation();
			AssertNoErrorContaining(secondEmail.GSE_EmailAddressInfo, "Email cannot set to empty because staff is assigned to group recipient of scheduled reports.");
			staff1.EmailAddresses.Delete(secondEmail);

			// After adding another staff with a main email address to the recipient group, the first staff should have no error when its main email is empty
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "ST2";
			staff2.GS_EmailAddress = "test@test.com";
			group.Staff.Add(staff2);

			Factory.Save();

			mainEmailAddress.GSE_EmailAddress = ZString.Empty;
			mainEmailAddress.RunPreSaveValidation();
			AssertNoErrors(mainEmailAddress.GSE_EmailAddressInfo);
		}

		public void TestAddWarningIfUsedByScheduleTaskSendFromAddress()
		{
			var warningString = "The original email address is used in scheduled task which will be upgraded too.";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "main@wtg.com";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("TST", "Test");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList))
			{
				var customizedEmail = staff.EmailAddresses.AddNew();
				using (customizedEmail.SuspendSettingHasChanges())
				{
					customizedEmail.GSE_EmailAddress = "tst@wtg.com";
					customizedEmail.EmailType = typeList[0].Code;
					customizedEmail.GSE_GC_Company = company.PK;
				}
				Factory.Save();
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

				var mainEmail = staff.EmailAddresses[0];
				mainEmail.GSE_EmailAddress = "tstMain@wtg.com";
				mainEmail.RunPreSaveValidation();
				customizedEmail.RunPreSaveValidation();
				AssertHasWarning(mainEmail.GSE_EmailAddressInfo, warningString);
				AssertNoWarning(customizedEmail.GSE_EmailAddressInfo, warningString);

				staff.GS_EmailAddress = "main@wtg.com";
				mainEmail.RunPreSaveValidation();
				customizedEmail.RunPreSaveValidation();
				AssertNoWarning(mainEmail.GSE_EmailAddressInfo, warningString);
				AssertNoWarning(customizedEmail.GSE_EmailAddressInfo, warningString);
				scheduleTaskRecipient.S6_EmailFromAddress = customizedEmail.GSE_EmailAddress;
				Factory.Save();

				staff.GS_EmailAddress = "tstMain@wtg.com";
				mainEmail.RunPreSaveValidation();
				customizedEmail.RunPreSaveValidation();
				AssertNoWarning(mainEmail.GSE_EmailAddressInfo, warningString);
				AssertNoWarning(customizedEmail.GSE_EmailAddressInfo, warningString);

				customizedEmail.GSE_EmailAddress = "tst2@wtg.com";
				mainEmail.RunPreSaveValidation();
				customizedEmail.RunPreSaveValidation();
				AssertNoWarning(mainEmail.GSE_EmailAddressInfo, warningString);
				AssertHasWarning(customizedEmail.GSE_EmailAddressInfo, warningString);
			}
		}

		public void TestValidateTwoFactorAuthentication()
		{
			using (SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email"))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "JBA";
				staff.GS_LoginName = "test";
				Factory.Save();

				var mainEmailAddress = staff.EmailAddresses[0];
				staff.GS_IsTwoFactorAuthenticationEnabled = true;
				mainEmailAddress.GSE_EmailAddress = ZString.Empty;
				mainEmailAddress.RunPreSaveValidation();
				AssertHasError(mainEmailAddress.GSE_EmailAddressInfo, "An email address is required to enable two factor authentication.");

				mainEmailAddress.GSE_EmailAddress = "valid@test.com";
				mainEmailAddress.RunPreSaveValidation();
				AssertNoErrors(mainEmailAddress.GSE_EmailAddressInfo);

				// 2nd email shouldn't have 2FA error even though it is empty
				var secondEmail = staff.EmailAddresses.AddNew();
				secondEmail.GSE_EmailAddress = ZString.Empty;
				secondEmail.RunPreSaveValidation();
				AssertNoErrorContaining(secondEmail.GSE_EmailAddressInfo, "An email address is required to enable two factor authentication.");
			}
		}

		public void TestValidateEmailAddressChanges()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var dbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dbStaff.GS_LoginName = "dbuser";
			dbStaff.GS_EmailAddress = "dbuser@email.com";
			dbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var nonDbStaff = Factory.NewWithValidTestData<GlbStaff>();
			nonDbStaff.GS_LoginName = "nondbuser";
			dbStaff.GS_EmailAddress = "nondbuser@email.com";
			nonDbStaff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();
			dbStaff.IsDatabaseDeveloper = true; // don't do this before save as it will cause creation of sql user

			dbStaff.GS_EmailAddress = "dbuser.2@mail.com";
			dbStaff.RunPreSaveValidation();
			AssertNoErrors("No error for email change in non-EDI - dbuser", dbStaff.GS_EmailAddressInfo);

			nonDbStaff.GS_EmailAddress = "nondbuser.2@email.com";
			nonDbStaff.RunPreSaveValidation();
			AssertNoErrors("No error for email change in non-EDI - nondbuser", nonDbStaff.GS_EmailAddressInfo);

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				dbStaff.RunPreSaveValidation();
				AssertHasError("Error for email change in EDI for dbuser", dbStaff.GS_EmailAddressInfo,
					@$"The Email Address's local-part (username part) cannot be changed and it must match the Login Name if the staff has database access.
If you need to update it, please disable database access and try again.");

				nonDbStaff.RunPreSaveValidation();
				AssertNoErrors("No error for email change in EDI for nondbuser", nonDbStaff.GS_EmailAddressInfo);

				dbStaff.GS_EmailAddress = "dbuser@hotmail.com";
				dbStaff.RunPreSaveValidation();
				AssertNoErrors("No error for email change in domain part in EDI for dbuser", dbStaff.GS_EmailAddressInfo);
			}
		}

		#endregion

		#region validateIsNDR
		public void TestValidateIsNDR()
		{
			var deliveryTime = new ZDateTime(2022, 11, 3);
			var warningString = $"A Non-Delivery Receipt has been received at {deliveryTime.ToLocalBranchTime().ToLongTimeString()}.";
			GlbEmailAddress emailAddr = Factory.New<GlbEmailAddress>();
			emailAddr.GI_DeliveryStatus = "NDR";
			emailAddr.GI_EmailAddress = "valid@test.com";
			emailAddr.GI_DeliveryReportTimeUtc = deliveryTime;
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			var emailAddress = staff.EmailAddresses.AddNew();
			emailAddress.GSE_EmailAddress = "valid@test.com";
			emailAddress.RunPreSaveValidation();
			AssertHasWarning(emailAddress.IsNDRInfo, warningString);
		}

		#endregion
	}
}
