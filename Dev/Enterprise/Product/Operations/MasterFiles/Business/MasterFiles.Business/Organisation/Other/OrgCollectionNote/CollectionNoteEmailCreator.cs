using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CollectionNoteEmailCreator : EmailToContactBusinessObject
	{
		public CollectionNoteEmailCreator(OrgCollectionNote note)
			: base(note)
		{
			if (note == null)
			{
				throw new ArgumentException("Collection Note cannot be null");
			}

			SetDefaultsForEmail(note);
			CollectionNote = note;
		}

		// We have to do this here because we don't have a reference
		// to the Collection Note when SetDefaultValues() is run.
		void SetDefaultsForEmail(OrgCollectionNote note)
		{
			Subject = Res.GetString("98d899f4-bdba-4668-a121-a6f1dcb013d7", "Collection Call for {0}", note.Header.OH_FullNameTruncated);

			if (!note.EmailAddress.IsEmpty)
			{
				ToEmailAddress = note.EmailAddress;
			}

			FromDisplayName = GlbStaff.CurrentUser.GS_FullName;
			FromEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			Body = ZString.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SaveAsNote = true; // We always want this to save as a note - see modified AddNote() method below
		}

		// Instead of Adding the Note as an StmNote, we want to add it to the ZString Note on the business object.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded note string")]
		protected override void AddNote(string emailRecipients, string noteText)
		{
			// Don't call base!

			ZStringBuilder noteData = new ZStringBuilder();
			noteData.Append("------------------------------");
			noteData.Append((NoResString)"From: " + FromDisplayName + (NoResString)" <" + FromEmailAddress + (NoResString)">");
			noteData.Append((NoResString)"Sent: " + ZDateTime.Now.ToString());
			noteData.Append("To: " + ToEmailAddress);
			if (!Cc.IsEmpty)
			{
				noteData.Append("Cc: " + Cc);
			}

			noteData.Append("Subject: " + Subject);
			noteData.Append(noteText);

			CollectionNote.PN_CallDetailNote += System.Environment.NewLine + noteData.ToStringWithNewLineBetweenAppends();
			CollectionNote.PN_CallDetailNoteInfo.RefreshBinding();
		}

		protected override void AddEvent(string emailRecipients)
		{
			//Do not actually needed as well as OrgCollectionNote has not its own form
			//base.AddEvent(EmailRecipients);

			CollectionNote.Header.Logs.AddNew(Events.EmailSent,
				(emailRecipients.Length > StmALogSchema.SL_Reference.MaxLength) ? emailRecipients.Substring(0, StmALogSchema.SL_Reference.MaxLength) : emailRecipients);

			AddEDoc(emailRecipients);
		}

		void AddEDoc(string emailRecipients)
		{
			if (CollectionNote.Header is IDocManagerSupport)
			{
				DocManagerInfo docSupportInfo = ((IDocManagerSupport)CollectionNote.Header).DocManagerInfo;

				string emailAsString = Res.GetString("35f52618-d56d-4a0a-b6e8-b6b27e82b921", "To: {0}", emailRecipients) + "\r\n\r\n";
				emailAsString += Res.GetString("69099085-e06d-4cbf-baf3-2416b28bc9bc", "Subject: {0}", Subject) + "\r\n\r\n";
				emailAsString += Res.GetString("7514bb06-500d-407c-bfe9-105bd9e8a996", "Body: \r\n\r\n{0}", Body);

				byte[] bytes = System.Text.Encoding.ASCII.GetBytes(emailAsString);
				string fileName = (NoResString)"Collection Call Email.txt";

				docSupportInfo.AddFileOrDocument(bytes, fileName, Core.Constants.RefDocTypes.MiscellaneousDocument);

				foreach (ICodeDescription attachment in AttachmentList)
				{
					string fullPathAndFileName = GetFileNameFromAttachmentListDescription(attachment.Description);
					docSupportInfo.AddFileOrDocument(fullPathAndFileName, Core.Constants.RefDocTypes.MiscellaneousDocument);
				}

				docSupportInfo.Save();
			}
		}

		readonly OrgCollectionNote CollectionNote;
	}
}
