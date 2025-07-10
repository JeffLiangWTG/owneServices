using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EmailWithAttachment : NonPersistentBusinessObject
	{
		public EmailWithAttachment()
			: this(string.Empty, string.Empty)
		{
		}

		public EmailWithAttachment(string overridingDefaultFromEmailAddress, string defaultFromDisplayName)
			: this(null, overridingDefaultFromEmailAddress, defaultFromDisplayName)
		{
		}

		public EmailWithAttachment(BusinessObjectFactory factory, string overridingDefaultFromEmailAddress, string defaultFromDisplayName)
			: base(factory)
		{
			DefaultFromDisplayName = defaultFromDisplayName;
			DefaultFromEmailAddress = !string.IsNullOrEmpty(overridingDefaultFromEmailAddress) ? overridingDefaultFromEmailAddress : Env.Registry.SMTPDefaultReturnEmailAddress;
		}

		#region Schema

		public static class Schema
		{
			public const string FromDisplayName = "FromDisplayName";
			public const string FromEmailAddress = "FromEmailAddress";
			public const string ToDisplayName = "ToDisplayName";
			public const string Attachment = "Attachment";
			public const string NumberOfAttachmentsMessage = "NumberOfAttachmentsMessage";
			public const string Priority = "Priority";
		}

		public const int ToDisplayNameMaxLength = 8000;
		public const int ToEmailAddressMaxLength = 8000;

		#endregion

		#region Set Default Values

		#region Name, Title and Email Address

		protected static ZString FullNameOfCurrentUser
		{
			get
			{
				return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_FullName : ZString.Empty;
			}
		}

		protected static ZString EmailOfCurrentUser
		{
			get
			{
				return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_EmailAddress : ZString.Empty;
			}
		}

		public ZBool UseCurrentUsersNameAndTitle
		{
			get { return useCurrentUsersNameAndTitle; }
			set
			{
				SetNonPersistentPropertyValue(UseCurrentUsersNameAndTitleInfo, ref useCurrentUsersNameAndTitle, value);
				if (value)
				{
					FromDisplayName = FullNameOfCurrentUser;
				}
				else
				{
					FromDisplayName = DefaultFromDisplayName;
				}
			}
		}

		public ZPropertyInfo UseCurrentUsersNameAndTitleInfo
		{
			get { return GetZPropertyInfo(nameof(UseCurrentUsersNameAndTitle)); }
		}

		public ZBool UseCurrentUsersEmailAddress
		{
			get
			{
				return useCurrentUsersEmailAddress;
			}
			set
			{
				SetNonPersistentPropertyValue(UseCurrentUsersEmailAddressInfo, ref useCurrentUsersEmailAddress, value);
				FromEmailAddress = value ? EmailOfCurrentUser : (ZString)DefaultFromEmailAddress;
			}
		}

		public ZPropertyInfo UseCurrentUsersEmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(UseCurrentUsersEmailAddress)); }
		}

		ZBool useCurrentUsersNameAndTitle;
		ZBool useCurrentUsersEmailAddress;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Priority = "MED";
			using (SuspendSettingHasChanges())
			{
				SetupDefaultFromAddressCore();
			}
		}

		protected virtual void SetupDefaultFromAddressCore()
		{
			UseCurrentUsersNameAndTitle = true;
			UseCurrentUsersEmailAddress = Env.Registry.AllowEmailsToBeSentFromUsersAddress;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore(); // call RunPreSaveValidationCore() on all children then fire OnNotificationsChanged()
		}

		public EmailWithAttachmentValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EmailWithAttachmentValidation GetNewValidation()
		{
			return new EmailWithAttachmentValidation(this);
		}

		#endregion

		#region Add/Remove Attachments

		public void AddAttachment(params string[] attachments)
		{
			foreach (string currentAttachment in attachments)
			{
				string fileName = Path.GetFileName(currentAttachment);

				if (!AttachmentList.ContainsCode(fileName) && File.Exists(currentAttachment))
				{
					FileInfo fileInfo = new FileInfo(currentAttachment);
					if (fileName.Length > AttachmentInfo.MaxLength)
					{
						fileName = fileName.Substring(0, AttachmentInfo.MaxLength);
					}
					AttachmentList.AddPair(fileName, currentAttachment + (NoResString)" [Size: " + (fileInfo.Length / 1024) + (NoResString)"KB]");
				}
			}

			SelectLatestAttachment();
		}

		public void RemoveAttachment()
		{
			AttachmentList.RemoveCode(Attachment);
			SelectLatestAttachment();
		}

		void SelectLatestAttachment()
		{
			Attachment = (AttachmentList.Count > 0) ? AttachmentList[AttachmentList.Count - 1].Code : "";
			AttachmentInfo.RefreshBinding();
			NumberOfAttachmentsMessageInfo.RefreshBinding();
		}

		public CodeDescriptionPairList AttachmentList
		{
			get
			{
				if (fAttachmentList == null)
				{
					fAttachmentList = new CodeDescriptionPairList();
				}

				return fAttachmentList;
			}
		}

		public string GetFileNameFromAttachmentListDescription(string description)
		{
			int index = description.IndexOf(" [Size: ");
			return (index != -1) ? description.Substring(0, index) : description;
		}

		#endregion

		#region Bound Properties

		public string DefaultFromEmailAddress { get; private set; }
		public string DefaultFromDisplayName { get; private set; }

		#region From Display Name

		[List("DisplayNameList")]
		[MaxLength(ToDisplayNameMaxLength)]
		[BusinessObjectTestExclude]
		public ZString FromDisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(fromDisplayName))
				{
					fromDisplayName = UseCurrentUsersNameAndTitle ? FullNameOfCurrentUser : (ZString)DefaultFromDisplayName;
				}
				return fromDisplayName;
			}
			set
			{
				CheckMaximumLength(FromDisplayNameInfo, value);
				SetNonPersistentPropertyValue(FromDisplayNameInfo, ref fromDisplayName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFromDisplayName();
				}
			}
		}

		public ZPropertyInfo FromDisplayNameInfo
		{
			get { return GetZPropertyInfo(Schema.FromDisplayName); }
		}

		ZString fromDisplayName;

		public bool FromDisplayName_ReadOnly
		{
			get { return UseCurrentUsersNameAndTitle; }
		}

		#endregion

		#region From Email Address

		[List("FromEmailAddressList")]
		[MaxLength(254)]
		[BusinessObjectTestExclude]
		[EmailAddress]
		public ZString FromEmailAddress
		{
			get
			{
				if (string.IsNullOrEmpty(fromEmailAddress))
				{
					fromEmailAddress = UseCurrentUsersEmailAddress ? EmailOfCurrentUser : (ZString)DefaultFromEmailAddress;
				}
				return fromEmailAddress;
			}
			set
			{
				CheckMaximumLength(FromEmailAddressInfo, value);
				SetNonPersistentPropertyValue(FromEmailAddressInfo, ref fromEmailAddress, value.Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					Validation.ValidateFromEmailAddress();
				}
			}
		}

		public ZPropertyInfo FromEmailAddressInfo
		{
			get { return GetZPropertyInfo(Schema.FromEmailAddress); }
		}

		ZString fromEmailAddress;

		public bool FromEmailAddress_ReadOnly
		{
			get { return UseCurrentUsersEmailAddress; }
		}

		#endregion

		public bool ShouldValidateFrom { get; set; }

		#region To Display Name

		[MaxLength(ToDisplayNameMaxLength)]
		public ZString ToDisplayName
		{
			get { return toDisplayName; }
			set
			{
				CheckMaximumLength(ToDisplayNameInfo, value);
				SetNonPersistentPropertyValue(ToDisplayNameInfo, ref toDisplayName, value);
			}
		}

		public ZPropertyInfo ToDisplayNameInfo
		{
			get { return GetZPropertyInfo(Schema.ToDisplayName); }
		}

		ZString toDisplayName;

		#endregion

		#region To Email Address

		[MaxLength(ToEmailAddressMaxLength)]
		[EmailAddress]
		public ZString ToEmailAddress
		{
			get { return toEmailAddress; }
			set
			{
				CheckMaximumLength(ToEmailAddressInfo, value);
				SetNonPersistentPropertyValue(ToEmailAddressInfo, ref toEmailAddress, value.Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					Validation.ValidateToEmailAddress();
				}
			}
		}

		public ZPropertyInfo ToEmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(ToEmailAddress)); }
		}

		ZString toEmailAddress;

		#endregion

		#region Cc

		[MaxLength(512)]
		[EmailAddress]
		public ZString Cc
		{
			get { return cc; }
			set
			{
				CheckMaximumLength(CcInfo, value);
				SetNonPersistentPropertyValue(CcInfo, ref cc, value.Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					Validation.ValidateCc();
				}
			}
		}

		public ZPropertyInfo CcInfo
		{
			get { return GetZPropertyInfo(nameof(Cc)); }
		}

		ZString cc;

		#endregion

		#region Subject

		[MaxLength(256)]
		public ZString Subject
		{
			get { return subject; }
			set
			{
				CheckMaximumLength(SubjectInfo, value);
				SetNonPersistentPropertyValue(SubjectInfo, ref subject, value);
			}
		}

		public ZPropertyInfo SubjectInfo
		{
			get { return GetZPropertyInfo(nameof(Subject)); }
		}

		ZString subject;

		#endregion

		#region Body

		public virtual ZString Body
		{
			get { return body; }
			set
			{
				CheckMaximumLength(BodyInfo, value);
				SetNonPersistentPropertyValue(BodyInfo, ref body, value);
			}
		}

		public ZPropertyInfo BodyInfo
		{
			get { return GetZPropertyInfo(nameof(Body)); }
		}

		ZString body;

		#endregion

		#region Attachment
		[List("AttachmentList")]
		[MaxLength(60)]
		public ZString Attachment
		{
			get { return attachment; }
			set
			{
				CheckMaximumLength(AttachmentInfo, value);
				SetNonPersistentPropertyValue(AttachmentInfo, ref attachment, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAttachment();
				}
			}
		}

		public ZPropertyInfo AttachmentInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Attachment);
			}
		}

		protected bool Attachment_ReadOnly
		{
			get { return (AttachmentList.Count == 0); }
		}

		ZString attachment;

		#endregion

		#region Number of Attachments Message

		public ZString NumberOfAttachmentsMessage
		{
			get
			{
				int count = AttachmentList.Count;
				return
					(count == 1)
						? Res.GetString("EmailToContactBusinessObject|1AttachedMessage", "There is 1 attachment.")
						: Res.GetString("EmailToContactBusinessObject|NAttachedMessages", "There are {0} attachments.", count);
			}
		}

		public ZPropertyInfo NumberOfAttachmentsMessageInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfAttachmentsMessage); }
		}

		CodeDescriptionPairList fAttachmentList;

		#endregion

		#region Priority
		[List("PriorityList")]
		[MaxLength(3)]
		public ZString Priority
		{
			get { return priority; }
			set
			{
				CheckMaximumLength(PriorityInfo, value);
				SetNonPersistentPropertyValue(PriorityInfo, ref priority, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePriority();
				}
			}
		}

		public ZPropertyInfo PriorityInfo
		{
			get { return GetZPropertyInfo(Schema.Priority); }
		}

		ZString priority;

		#endregion

		#endregion

		protected virtual EmailDef GetNewEmailDef()
		{
			return new EmailDef();
		}

		public EmailDef GetEmail()
		{
			return GetEmailCore();
		}

		protected virtual EmailDef GetEmailCore()
		{
			EmailDef email = GetNewEmailDef();

			if (Priority == "HI")
			{
				email.Priority = EmailDef.PriorityFlag.High;
			}
			else if (Priority == "LOW")
			{
				email.Priority = EmailDef.PriorityFlag.Low;
			}
			else
			{
				email.Priority = EmailDef.PriorityFlag.Medium;
			}

			if (!string.IsNullOrEmpty(FromDisplayName))
			{
				email.FromDisplayName = FromDisplayName;
			}

			if (!string.IsNullOrEmpty(FromEmailAddress))
			{
				email.FromAddress = FromEmailAddress;
			}

			email.AddRecipientForUserCommunication(Recipients);
			email.AddRecipientForUserCommunication(CcRecipients, RecipientDef.RecipientTypes.CC);

			email.Subject = Subject;
			email.Body = Body;

			for (int i = 0; i < AttachmentList.Count; i++)
			{
				string currentAttachment = GetFileNameFromAttachmentListDescription(AttachmentList[i].Description);
				email.Attachments.Add(new AttachmentDef(Path.GetFileName(currentAttachment), currentAttachment));
			}

			return email;
		}

		public bool CheckIsReadyToSendEmail()
		{
			Validation.ValidateAll();
			return !HasErrors;
		}

		#region Priority List

		public CodeDescriptionPairList PriorityList
		{
			get
			{
				if (fPriorityList == null)
				{
					fPriorityList = new CodeDescriptionPairList();
					fPriorityList.AddPair("HI", ResString.GetMultilingualString("8ec4be16-125c-4479-acc3-2f56023dd4e8", "High"));
					fPriorityList.AddPair("MED", ResString.GetMultilingualString("caf365c9-9b5a-4b70-8f81-c320fdc2412c", "Medium"));
					fPriorityList.AddPair("LOW", ResString.GetMultilingualString("4d6948a0-1cdf-4256-81f0-6652d9db9b83", "Low"));
				}

				return fPriorityList;
			}
		}

		CodeDescriptionPairList fPriorityList;

		#endregion

		#region DisplayNameList

		public UntranslatableCodeDescriptionPairList DisplayNameList
		{
			get
			{
				if (displayNameList == null)
				{
					displayNameList = new UntranslatableCodeDescriptionPairList((NoResString)"DisplayName from current user and system default");
					if (!string.IsNullOrEmpty(DefaultFromDisplayName))
					{
						displayNameList.AddPair(DefaultFromDisplayName);
					}
					if (!string.IsNullOrEmpty(FullNameOfCurrentUser) && !displayNameList.ContainsCode(FullNameOfCurrentUser))
					{
						displayNameList.AddPair(FullNameOfCurrentUser);
					}

					var mailboxDisplayName = Env.Registry.MailboxDisplayName;
					if (!string.IsNullOrEmpty(mailboxDisplayName) && !displayNameList.ContainsCode(mailboxDisplayName))
					{
						displayNameList.AddPair(mailboxDisplayName);
					}
				}
				return displayNameList;
			}
		}

		UntranslatableCodeDescriptionPairList displayNameList;

		#endregion

		#region FromEmailAddressList

		public UntranslatableCodeDescriptionPairList FromEmailAddressList
		{
			get
			{
				if (fromEmailAddressList == null)
				{
					fromEmailAddressList = new UntranslatableCodeDescriptionPairList((NoResString)"Email address from current user and system default");
					if (!string.IsNullOrEmpty(DefaultFromEmailAddress))
					{
						fromEmailAddressList.AddPair(DefaultFromEmailAddress);
					}
					if (!string.IsNullOrEmpty(EmailOfCurrentUser) && !fromEmailAddressList.ContainsCode(EmailOfCurrentUser))
					{
						fromEmailAddressList.AddPair(EmailOfCurrentUser);
					}
					if (GlbStaff.CurrentUser != null)
					{
						foreach (var item in GlbStaff.CurrentUser.EmailAddresses)
						{
							var emailAddress = item.GSE_EmailAddress;
							if (!string.IsNullOrEmpty(emailAddress) && !fromEmailAddressList.ContainsCode(emailAddress))
							{
								fromEmailAddressList.AddPair(emailAddress);
							}
						}
					}
				}
				return fromEmailAddressList;
			}
		}

		UntranslatableCodeDescriptionPairList fromEmailAddressList;

		#endregion

		#region Recipients

		public string[] Recipients
		{
			get { return GetEmailAddressArray(ToEmailAddress); }
		}

		public string[] CcRecipients
		{
			get { return GetEmailAddressArray(Cc); }
		}

		public virtual string[] AllRecipients
		{
			get
			{
				var result = new List<string>();
				result.AddRange(Recipients);
				result.AddRange(CcRecipients);
				return result.ToArray();
			}
		}

		public virtual string AllRecipientsCommaDelimited
		{
			get
			{
				string result = ToEmailAddress.TrimEnd(';');
				if (!Cc.IsEmpty)
				{
					if (result.Length > 0)
					{
						result += ";";
					}
					result += Cc.Trim(';');
				}
				if (result.Length > 0)
				{
					result = result.Replace(";", ", ");
				}
				return result;
			}
		}

		protected string[] GetEmailAddressArray(string emailAddresses)
		{
			string[] result = emailAddresses.TrimEnd(';').Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			return (result.Length == 1 && string.IsNullOrEmpty(result[0])) ? Array.Empty<string>() : result;
		}

		#endregion

		#region Preview

		public string PreviewFilePath
		{
			get { return previewFilePath; }
		}

		string previewFilePath;

		public IDisposable SetupPreviewFile()
		{
			EmailDef emailDef = GetEmail();
			string dirName = Temp.GetNewTempSubdirectory();
			string extension = (emailDef.ContentType == EmailContentTypes.HTML) ? (NoResString)"html" : (NoResString)"txt";
			string fileName = Temp.GetTempFileName(dirName, extension);

			using (StreamWriter writer = new StreamWriter(File.Create(fileName), new UTF8Encoding(true)))
			{
				writer.Write(emailDef.Body);
			}

			foreach (AttachmentDef attachment in emailDef.Attachments)
			{
				string targetPath = Path.Combine(dirName, Path.GetFileName(attachment.DisplayName));
				File.WriteAllBytes(targetPath, attachment.Data);
			}

			previewFilePath = fileName;
			return new TempDirCleanUpHelper(dirName);
		}

		class TempDirCleanUpHelper : IDisposable
		{
			public TempDirCleanUpHelper(string dirName)
			{
				this.dirName = dirName;
			}

			public void Dispose()
			{
				try
				{
					foreach (string fileName in Directory.GetFiles(dirName))
					{
						File.Delete(fileName);
					}
					Directory.Delete(dirName);
				}
				catch (IOException)
				{
					// We should not care if the temporary files / directory cannot be deleted
				}
			}

			readonly string dirName;
		}

		#endregion
	}
}
