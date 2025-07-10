using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EmailToContactBusinessObject : EmailWithAttachment
	{
		public EmailToContactBusinessObject(BusinessObject businessObjectSendingEmail)
			: this(businessObjectSendingEmail, string.Empty, string.Empty)
		{
		}

		public EmailToContactBusinessObject(BusinessObject businessObjectSendingEmail, string overridingDefaultFromEmailAddress, string defaultFromDisplayName)
			: base(businessObjectSendingEmail.Factory, overridingDefaultFromEmailAddress, defaultFromDisplayName)
		{
			this.businessObjectSendingEmail = businessObjectSendingEmail;
		}

		public bool SaveAsNote
		{
			get { return saveAsNote; }
			set { saveAsNote = value; }
		}

		public BusinessObject BusinessObjectSendingEmail
		{
			get { return businessObjectSendingEmail; }
		}

		bool saveAsNote;
		readonly BusinessObject businessObjectSendingEmail;

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Priority = "MED";
			SaveAsNote = true;
			ShouldSaveEmailInNewFactory = true;
			ShouldSaveBizOFactoryOnSent = true;
			ShouldAddNoteAndEvent = true;
		}

		#endregion

		#region Send Email

		/// <summary>
		/// Sends the email and adds an applicable event and note to the BusinessObject sending the email.
		/// </summary>
		public void SendEmail(bool systemCommunication = false)
		{
			PreSendEmail();
			SendEmailCore(systemCommunication);
			PostSendEmail();
		}

		protected virtual void PreSendEmail()
		{
		}

		protected virtual void SendEmailCore(bool systemCommunication = false)
		{
			var email = GetEmail();

			SetEmailWithRecipients(email, systemCommunication);

			if (ShouldSaveEmailInNewFactory)
			{
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			else
			{
				Env.OutgoingMailManager.Create(Factory, email);
			}

			if (ShouldAddNoteAndEvent)
			{
				AddNoteAndEvent(GetEmailBodyForNote(email));
			}
		}

		protected virtual void SetEmailWithRecipients(EmailDef email, bool systemCommunication = false)
		{
			if (systemCommunication)
			{
				for (var i = 0; i < email.Recipients.Count; ++i)
				{
					var recipient = email.Recipients[i];
					recipient.IsForSystemCommunication = true;
				}
			}
		}

		protected virtual void PostSendEmail()
		{
			if (ShouldSaveBizOFactoryOnSent)
			{
				Factory.Save();
			}
		}

		protected virtual string GetEmailBodyForNote(EmailDef email)
		{
			return Body;
		}

		public bool ShouldSaveEmailInNewFactory { get; set; }
		public bool ShouldSaveBizOFactoryOnSent { get; set; }
		public bool ShouldAddNoteAndEvent { get; set; }

		protected void AddNoteAndEvent(string emailBodyForNote)
		{
			string noteText = emailBodyForNote + System.Environment.NewLine;
			string emailRecipients = AllRecipientsCommaDelimited;

			for (int i = 0; i < AttachmentList.Count; i++)
			{
				string crrentAttachment = GetFileNameFromAttachmentListDescription(AttachmentList[i].Description);
				noteText += System.Environment.NewLine + (NoResString)"Attachment" + (i + 1) + (NoResString)": " + Path.GetFileName(crrentAttachment);
			}
			if (SaveAsNote)
			{
				AddNote(emailRecipients, noteText);
			}

			AddEvent(emailRecipients);
		}

		protected virtual void AddNote(string emailRecipients, string noteText)
		{
			StmNote note = BusinessObjectSendingEmail.GetNotes().AddNew();
			note.ST_NoteType = StmNoteDescription.Pub;
			note.ST_Description = Res.GetString("1bf198de-d6c8-4592-8a89-286d0ebf6e84", "Email Sent");
			StringBuilder noteData = new StringBuilder();
			noteData.Append((NoResString)"Sent To: ");
			noteData.Append(emailRecipients);
			noteData.Append(System.Environment.NewLine);
			noteData.Append(System.Environment.NewLine);
			noteData.Append(noteText);

			note.ST_NoteDataAsText = noteData.ToString();
		}

		protected virtual void AddEvent(string emailRecipients)
		{
			BusinessObjectSendingEmail.GetLogs().AddNew(Events.EmailSent,
				emailRecipients.Length > StmALogSchema.SL_Reference.MaxLength ? emailRecipients.Substring(0, StmALogSchema.SL_Reference.MaxLength) : emailRecipients);
		}

		#endregion
	}
}
