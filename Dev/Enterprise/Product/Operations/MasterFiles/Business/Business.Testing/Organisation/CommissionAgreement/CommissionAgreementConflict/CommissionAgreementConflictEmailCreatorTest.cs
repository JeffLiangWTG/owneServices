using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CommissionAgreementConflictEmailCreator))]
	sealed class CommissionAgreementConflictEmailCreatorTest : NonPersistentBusinessObjectTestCase
	{
		#region CreateAndSave

		public void TestCreateAndSave()
		{
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
			var risDuplicateStaff = Factory.NewWithValidTestData<GlbStaff>();
			risDuplicateStaff.GS_Code = "SIR";
			risDuplicateStaff.GS_FullName = "Drahcir";
			risDuplicateStaff.GS_EmailAddress = "richard.smith@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(adlStaff);

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O0001001";
			var genericAgreement = opportunity.CommissionAgreements.AddNew();
			genericAgreement.CA0_Name = "#1";
			genericAgreement.CA0_OH_Customer = customer.PK;
			genericAgreement.FillWithValidTestData();
			genericAgreement.Recipients.AddNew(adlStaff);
			genericAgreement.Recipients.AddNew(scwStaff);
			genericAgreement.Recipients.AddNew(risStaff);
			genericAgreement.Recipients.AddNew(risDuplicateStaff);
			var genericAgreementItemAll = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(genericAgreement, "ALL", "ALL", "ALL");

			var specificAgreement = opportunity.CommissionAgreements.AddNew();
			specificAgreement.CA0_Name = "#2";
			specificAgreement.CA0_OH_Customer = customer.PK;
			specificAgreement.FillWithValidTestData();

			var specificAgreementItemEntOdm = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(specificAgreement, "ENT", "ODM", "ALL");
			var specificAgreementItemEntHos = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(specificAgreement, "ENT", "HOS", "ALL");

			var notificationGroupTemplate = OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroupEmailTemplate.Value;
			notificationGroupTemplate.EmailSubject = "Email for Notification Group";
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroupEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroupTemplate);

			var agreementRecipientsTemplate = OrganisationRegistry.Instance.CommissionAgreementConflictRecipientsEmailTemplate.Value;
			agreementRecipientsTemplate.EmailSubject = "Email for Agreement Recipients";
			OrganisationRegistry.Instance.CommissionAgreementConflictRecipientsEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, agreementRecipientsTemplate);

			var creator = new CommissionAgreementConflictEmailCreator(Factory, specificAgreement, genericAgreement, Enumerable.Empty<ICommissionAgreementConflict>());

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			{
				creator.CreateAndSave();

				AssertContainsExactElementsInAnyOrder("Should not send email to sam since he has no email address; and should only send to richard once",
					new[]
					{
						"Subject: Email for Agreement Recipients  To: andrew.luong@wisetechglobal.com",
						"Subject: Email for Agreement Recipients  To: richard.smith@wisetechglobal.com",
					},
					Env.OutgoingMailManager.EmailsCreated.Select(x => "Subject: " + x.Subject + "  To: " + x.Recipients.RecipientsAsDelimitedString()));

				var adlEmail = Env.OutgoingMailManager.EmailsCreated.Single(x => x.Recipients.Contains("andrew.luong@wisetechglobal.com"));
				AssertEquals(EmailContentTypes.HTML, adlEmail.ContentType);
				AssertContains("Body", "Dear Andrew,", adlEmail.Body);

				var risEmail = Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains("richard.smith@wisetechglobal.com"));
				AssertEquals(EmailContentTypes.HTML, risEmail.ContentType);
				AssertContains("Body", "Dear Richard,", risEmail.Body);

				AssertEquals(1, opportunity.DocManagerInfo().Files.Count);
				var document = opportunity.DocManagerInfo().Files[0];
				AssertEquals("Email for Agreement Recipients.txt", document.FileName);
				AssertEquals(Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument, document.DocType);
			}

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				RemoveAllEDocs(opportunity.DocManagerInfo());

				creator.CreateAndSave();

				AssertContainsExactElementsInAnyOrder("Should only send to notification group",
					new[]
					{
						"Subject: Email for Notification Group  To: andrew.luong@wisetechglobal.com"
					},
					Env.OutgoingMailManager.EmailsCreated.Select(x => "Subject: " + x.Subject + "  To: " + x.Recipients.RecipientsAsDelimitedString()));

				var adlEmail = Env.OutgoingMailManager.EmailsCreated.Single(x => x.Recipients.Contains("andrew.luong@wisetechglobal.com"));
				AssertEquals(EmailContentTypes.HTML, adlEmail.ContentType);
				AssertContains("Body", "Dear Andrew,", adlEmail.Body);

				AssertEquals(1, opportunity.DocManagerInfo().Files.Count);
				var document = opportunity.DocManagerInfo().Files[0];
				AssertEquals("Email for Notification Group.txt", document.FileName);
				AssertEquals(Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument, document.DocType);
			}

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				RemoveAllEDocs(opportunity.DocManagerInfo());

				creator.CreateAndSave();

				AssertContainsExactElementsInAnyOrder("Should notification groups AND agreement recipients",
					new[]
					{
						"Subject: Email for Notification Group  To: andrew.luong@wisetechglobal.com",
						"Subject: Email for Agreement Recipients  To: andrew.luong@wisetechglobal.com",
						"Subject: Email for Agreement Recipients  To: richard.smith@wisetechglobal.com",
					},
					Env.OutgoingMailManager.EmailsCreated.Select(x => "Subject: " + x.Subject + "  To: " + x.Recipients.RecipientsAsDelimitedString()));

				AssertContainsExactElementsInAnyOrder(
					new ZString[]
					{
						"Email for Notification Group.txt",
						"Email for Agreement Recipients.txt",
					},
					opportunity.DocManagerInfo().Files.Cast<IeDoc>().Select(x => x.FileName));
			}

			OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				RemoveAllEDocs(opportunity.DocManagerInfo());

				creator.CreateAndSave();

				AssertContainsExactElementsInAnyOrder("Should not send any notification emails",
					Enumerable.Empty<ZString>(),
					Env.OutgoingMailManager.EmailsCreated.Select(x => "Subject: " + x.Subject + "  To: " + x.Recipients.RecipientsAsDelimitedString()));

				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<ZString>(),
					opportunity.DocManagerInfo().Files.Cast<IeDoc>().Select(x => x.FileName));
			}
		}

		#endregion

		#region Implementation

		void RemoveAllEDocs(DocManagerInfo docManager)
		{
			foreach (IBusiness eDoc in docManager.AllEDocs.Cast<IeDoc>().ToArray())
			{
				eDoc.Delete();
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var loserAgreement = Factory.New<OrgCommissionAgreement>();
			var winnerAgreement = Factory.New<OrgCommissionAgreement>();
			return new CommissionAgreementConflictEmailCreator(Factory, winnerAgreement, loserAgreement, Enumerable.Empty<ICommissionAgreementConflict>());
		}

		#endregion
	}
}
