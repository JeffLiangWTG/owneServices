using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementConflictEmailCreator : NonPersistentBusinessObject
	{
		public CommissionAgreementConflictEmailCreator(BusinessObjectFactory factory, OrgCommissionAgreement winnerCommissionAgreement, OrgCommissionAgreement loserCommissionAgreement, IEnumerable<ICommissionAgreementConflict> conflicts)
			: base(factory)
		{
			Argument.NotNull(winnerCommissionAgreement, "winnerCommissionAgreement");
			Argument.NotNull(loserCommissionAgreement, "loserCommissionAgreement");
			Argument.NotNull(conflicts, "conflicts");

			this.factory = factory;
			this.winnerCommissionAgreement = winnerCommissionAgreement;
			this.loserCommissionAgreement = loserCommissionAgreement;
			this.conflicts = conflicts;
		}

		readonly BusinessObjectFactory factory;

		#region Properties

		#region WinnerCommissionAgreement

		public OrgCommissionAgreement WinnerCommissionAgreement
		{
			get { return winnerCommissionAgreement; }
		}
		readonly OrgCommissionAgreement winnerCommissionAgreement;

		#endregion

		#region LoserCommissionAgreement

		public OrgCommissionAgreement LoserCommissionAgreement
		{
			get { return loserCommissionAgreement; }
		}
		readonly OrgCommissionAgreement loserCommissionAgreement;

		#endregion

		#region ConflictsDescription

		public ZString GetConflictsDescription(int indentLevel, bool html)
		{
			return ConflictsTextProvider.ToDisplayList(conflicts, indentLevel, html);
		}

		readonly IEnumerable<ICommissionAgreementConflict> conflicts;

		CommissionAgreementConflictsTextProvider ConflictsTextProvider
		{
			get { return conflictsTextProvider ?? (conflictsTextProvider = CommissionAgreementConflictsTextProvider.New()); }
		}
		CommissionAgreementConflictsTextProvider conflictsTextProvider;

		#endregion

		#endregion

		#region Email Customization Doc Fields

		public ZString RecipientName
		{
			get;
			internal set;
		}

		#endregion

		#region CreateAndSave

		public void CreateAndSave()
		{
			HashSet<string> sentNotificationGroupEmailAddresses = null;
			HashSet<string> sentAgreementRecipientEmailAddresses = null;

			var notificationGroup = Factory.Load<GlbGroup>(OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.Value);
			if (notificationGroup != null)
			{
				SendEmails(
					OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroupEmailTemplate.Value,
					notificationGroup.Staff.Cast<GlbStaff>().Select(x => Tuple.Create(x.GS_FullName, GetStaffEmails(x))),
					out sentNotificationGroupEmailAddresses);
			}

			if (OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.Value)
			{
				SendEmails(
					OrganisationRegistry.Instance.CommissionAgreementConflictRecipientsEmailTemplate.Value,
					LoserCommissionAgreement.Recipients.Select(x => Tuple.Create(x.Name, x.NotificationEmailAddresses)),
					out sentAgreementRecipientEmailAddresses);
			}

			if ((sentNotificationGroupEmailAddresses != null && sentNotificationGroupEmailAddresses.Any()) || (sentAgreementRecipientEmailAddresses != null && sentAgreementRecipientEmailAddresses.Any()))
			{
				Factory.Save();

				CreateEDocForSentEmails(OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroupEmailTemplate.Value, sentNotificationGroupEmailAddresses);
				CreateEDocForSentEmails(OrganisationRegistry.Instance.CommissionAgreementConflictRecipientsEmailTemplate.Value, sentAgreementRecipientEmailAddresses);
			}
		}

		static IEnumerable<ZString> GetStaffEmails(GlbStaff staff)
		{
			if (!staff.GS_EmailAddress.IsEmpty)
			{
				yield return staff.GS_EmailAddress;
			}
		}

		void SendEmails(NotificationEmailTemplate template, IEnumerable<Tuple<ZString, IEnumerable<ZString>>> recipientNameAndEmailAddressPairs, out HashSet<string> sentEmailAddresses)
		{
			var sentEmailAddressesLocal = new HashSet<string>();
			var parser = new CommissionAgreementConflictEmailDocumentParser(factory);
			var htmlParser = new HtmlCommissionAgreementConflictEmailDocumentParser(factory);

			foreach (var nameAndEmailAddressPair in recipientNameAndEmailAddressPairs)
			{
				RecipientName = nameAndEmailAddressPair.Item1;

				var emailAddresses = nameAndEmailAddressPair.Item2.Where(x => !sentEmailAddressesLocal.Contains(x)).ToArray();
				if (emailAddresses.Any())
				{
					var email = new HtmlFormatEmailToContactBusinessObject(WinnerCommissionAgreement);
					email.FromEmailAddress = string.IsNullOrEmpty(GlbStaff.CurrentUser.GS_EmailAddress) ? (ZString)Env.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress : GlbStaff.CurrentUser.GS_EmailAddress;
					email.FromDisplayName = GlbStaff.CurrentUser.GS_FullName;
					email.Subject = parser.Parse(this, template.EmailSubject);
					email.Body = htmlParser.Parse(this, template.EmailBody).Replace("\r\n", "<br />");
					email.ToEmailAddress = string.Join("; ", emailAddresses);

					email.ShouldSaveEmailInNewFactory = false;
					email.ShouldSaveBizOFactoryOnSent = false;
					email.SaveAsNote = false;

					email.SendEmail();

					foreach (var emailAddress in emailAddresses)
					{
						sentEmailAddressesLocal.Add(emailAddress);
					}
				}
			}

			sentEmailAddresses = sentEmailAddressesLocal;
		}

		void CreateEDocForSentEmails(NotificationEmailTemplate template, HashSet<string> sentEmailAddresses)
		{
			if (sentEmailAddresses != null && sentEmailAddresses.Count > 0)
			{
				RecipientName = ZString.Empty;

				var parser = new CommissionAgreementConflictEmailDocumentParser(WinnerCommissionAgreement.Factory);
				var htmlParser = new HtmlCommissionAgreementConflictEmailDocumentParser(WinnerCommissionAgreement.Factory);

				var eDocString = new ZStringBuilder();
				eDocString.AppendLine(Res.GetString("27ddb0ff-2085-412d-bf12-ddc424aeb89c", "To: {0}", string.Join(";", sentEmailAddresses)));
				var subject = parser.Parse(this, template.EmailSubject);
				eDocString.AppendLine(Res.GetString("cea983e3-6c6f-4f9d-acd7-491a9fb63e80", "Subject: {0}", subject));
				eDocString.AppendLine(Res.GetString("962a0ed2-0978-446b-942f-4a5f5c8e92b0", "Body: {0}", htmlParser.Parse(this, template.EmailBody)));

				var bytes = System.Text.Encoding.ASCII.GetBytes(eDocString.ToString());
				var fileName = subject + ".txt";

				var docManager = WinnerCommissionAgreement.Opportunity.DocManagerInfo();
				docManager.AddFileOrDocument(bytes, fileName, Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument);
				docManager.Save();
			}
		}

		#endregion
	}
}
