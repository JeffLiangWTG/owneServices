using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BulkCommissionAgreementConflictEmailCreatorTest : TestCaseWithFactory
	{
		#region Notification Message

		public void TestGetNotificationPromptMessage()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var group = Factory.New<GlbGroup>();
				var opportunity = Factory.New<OrgOpportunity>();
				opportunity.P8_OpportunityID = "O0001001";
				var agreement1 = opportunity.ApprovedCommissionAgreements.AddNew();
				agreement1.CA0_Name = "#1";
				var agreement2 = opportunity.ApprovedCommissionAgreements.AddNew();
				agreement2.CA0_Name = "#2";

				agreement1.ProductItems.DeleteAll();
				var agreementItem_XXX_XXX_XXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "XXX", "XXX", "XXX");
				var agreementItem_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "XXX", "XXX", "ALL");
				var agreementItem_YYY_YYY_YYY = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "YYY", "YYY", "YYY");
				var agreementItem_YYY_YYY_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "YYY", "YYY", "ALL");

				var conflictXXX = new CommissionAgreementItemConflict(agreementItem_XXX_XXX_XXX, agreementItem_XXX_XXX_ALL);
				var conflictYYY = new CommissionAgreementItemConflict(agreementItem_YYY_YYY_YYY, agreementItem_YYY_YYY_ALL);

				var creator = new BulkCommissionAgreementConflictEmailCreator(Factory, new[] { conflictXXX, conflictYYY });

				OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				AssertMultilineASCIIEquals("NotificationPromptMessage",
	@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX > XXX > XXX
   YYY > YYY > YYY

An email will be sent to all members of the existing commission agreement wolf packs. Are you sure you want to continue?",
					creator.GetNotificationPromptMessage());

				OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
				AssertMultilineASCIIEquals("NotificationPromptMessage",
	@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX > XXX > XXX
   YYY > YYY > YYY

An email will be sent to a notification group and all members of the existing commission agreement wolf packs. Are you sure you want to continue?",
					creator.GetNotificationPromptMessage());

				OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
				AssertMultilineASCIIEquals("NotificationPromptMessage",
	@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX > XXX > XXX
   YYY > YYY > YYY

An email will be sent to a notification group. Are you sure you want to continue?",
					creator.GetNotificationPromptMessage());

				OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				AssertMultilineASCIIEquals("NotificationPromptMessage",
					ZString.Empty,
					creator.GetNotificationPromptMessage());
			}
		}

		public void TestGetNotificationPromptMessage_ModeOrgiginDestination()
		{
			var group = Factory.New<GlbGroup>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O0001001";
			var agreement1 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			var agreement2 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";

			agreement1.ProductItems.DeleteAll();
			var agreementItem_XXX_XXX_AUSYD_UAIEV = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement1, "XXX", "XXX", "AUSYD", "UAIEV");
			var agreementItem_XXX_XXX_AUSYD_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement2, "XXX", "XXX", "AUSYD", "");
			var agreementItem_YYY_YYY_USLAX_GBLON = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement1, "YYY", "YYY", "USLAX", "GBLON");
			var agreementItem_YYY_YYY_USLAX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement2, "YYY", "YYY", "AUSYD", "");

			var conflictXXX = new CommissionAgreementItemConflict(agreementItem_XXX_XXX_AUSYD_UAIEV, agreementItem_XXX_XXX_AUSYD_ALL);
			var conflictYYY = new CommissionAgreementItemConflict(agreementItem_YYY_YYY_USLAX_GBLON, agreementItem_YYY_YYY_USLAX_ALL);

			var creator = new BulkCommissionAgreementConflictEmailCreator(Factory, new[] { conflictXXX, conflictYYY });

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertMultilineASCIIEquals("NotificationPromptMessage",
@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX | AUSYD > UAIEV | XXX
   YYY | USLAX > GBLON | YYY

An email will be sent to all members of the existing commission agreement wolf packs. Are you sure you want to continue?",
				creator.GetNotificationPromptMessage());

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AssertMultilineASCIIEquals("NotificationPromptMessage",
@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX | AUSYD > UAIEV | XXX
   YYY | USLAX > GBLON | YYY

An email will be sent to a notification group and all members of the existing commission agreement wolf packs. Are you sure you want to continue?",
				creator.GetNotificationPromptMessage());

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AssertMultilineASCIIEquals("NotificationPromptMessage",
@"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

O0001001#1 will take commission from O0001001#2
   XXX | AUSYD > UAIEV | XXX
   YYY | USLAX > GBLON | YYY

An email will be sent to a notification group. Are you sure you want to continue?",
				creator.GetNotificationPromptMessage());

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertMultilineASCIIEquals("NotificationPromptMessage",
				ZString.Empty,
				creator.GetNotificationPromptMessage());
		}

		#endregion

		#region CreateAndSave

		public void TestCreateAndSave()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_FullName = "WiseTech & Global";
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";
			adlStaff.GS_FullName = "Andrew";
			adlStaff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";
			scwStaff.GS_FullName = "Samuel";
			scwStaff.GS_EmailAddress = "";
			var risStaff = Factory.NewWithValidTestData<GlbStaff>();
			risStaff.GS_Code = "RIS";
			risStaff.GS_FullName = "Richard";
			risStaff.GS_EmailAddress = "richard.smith@wisetechglobal.com";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O0001001";
			var genericAgreement = opportunity.CommissionAgreements.AddNew();
			genericAgreement.CA0_Name = "#1";
			genericAgreement.CA0_OH_Customer = customer.PK;
			genericAgreement.FillWithValidTestData();
			genericAgreement.Recipients.AddNew(adlStaff);
			genericAgreement.Recipients.AddNew(scwStaff);
			genericAgreement.Recipients.AddNew(risStaff);
			var genericAgreementItemAll = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(genericAgreement, "ALL", "ALL", "ALL");

			var specificAgreement = opportunity.CommissionAgreements.AddNew();
			specificAgreement.CA0_Name = "#2";
			specificAgreement.CA0_OH_Customer = customer.PK;
			specificAgreement.FillWithValidTestData();

			var specificAgreementItemEntOdm = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(specificAgreement, "ENT", "ODM", "ALL");
			var specificAgreementItemEntHos = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(specificAgreement, "ENT", "HOS", "ALL");

			var conflict1 = new CommissionAgreementItemConflict(specificAgreementItemEntOdm, genericAgreementItemAll);
			var conflict2 = new CommissionAgreementItemConflict(specificAgreementItemEntHos, genericAgreementItemAll);
			var emailCreator = new BulkCommissionAgreementConflictEmailCreator(Factory, new[] { conflict1, conflict2 });
			emailCreator.CreateAndSave();

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var adlEmail = Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains("andrew.luong@wisetechglobal.com"));
			AssertContainsExactElementsInAnyOrder(
				new[] { "andrew.luong@wisetechglobal.com" },
				adlEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email));

			AssertEquals(EmailContentTypes.HTML, adlEmail.ContentType);
			AssertMultilineASCIIEquals("Subject", "Commission Agreement O0001001#1 (WiseTech & Global) Conflict", adlEmail.Subject);
			AssertContains("Body", "Dear Andrew,", adlEmail.Body);

			var risEmail = Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains("richard.smith@wisetechglobal.com"));
			AssertContainsExactElementsInAnyOrder(
				new[] { "richard.smith@wisetechglobal.com" },
				risEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email));

			AssertEquals(EmailContentTypes.HTML, risEmail.ContentType);
			AssertMultilineASCIIEquals("Subject", "Commission Agreement O0001001#1 (WiseTech & Global) Conflict", risEmail.Subject);
			AssertContains("Body", "Dear Richard,", risEmail.Body);

			AssertEquals(1, opportunity.DocManagerInfo().Files.Count);
			var document = opportunity.DocManagerInfo().Files[0];
			AssertEquals("Commission Agreement O0001001#1 (WiseTech & Global) Conflict.txt", document.FileName);
			AssertEquals(Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument, document.DocType);
		}

		#endregion
	}
}
