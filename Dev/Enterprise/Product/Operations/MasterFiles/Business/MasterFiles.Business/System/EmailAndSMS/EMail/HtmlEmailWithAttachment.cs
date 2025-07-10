using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	//We're just passing through the custom business object of our parent.
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class HtmlEmailWithAttachment : EmailWithAttachment, ICustomFieldProvider
	{
		public HtmlEmailWithAttachment(ISendEmailSource source)
			: base(source.OverridingDefaultFromEmailAddress, source.DefaultFromDisplayName)
		{
			Source = source;
			var addressBookSelection = source.GetAddressBookSelection();
			this.recipientSelection = new RecipientSelection(addressBookSelection);
			SetupRecipients(addressBookSelection);
			FilledBodyIsMandatory = true;
		}

		void SetupRecipients(AddressBookSelection selection)
		{
			ToEmailAddress = string.Join(";", selection.ToAddresses);
		}

		#region Properties

		public readonly ISendEmailSource Source;

		#region Recipient Selection

		public RecipientSelection RecipientSelection
		{
			get
			{
				if (recipientSelection != null)
				{
					recipientSelection.ToEmailAddress = ToEmailAddress;
					recipientSelection.Cc = Cc;
					recipientSelection.Bcc = Bcc;
				}
				return recipientSelection;
			}
		}
		readonly RecipientSelection recipientSelection;

		public void Update(RecipientSelection selection)
		{
			ToEmailAddress = selection.ToEmailAddress;
			Cc = selection.Cc;
			Bcc = selection.Bcc;
		}

		#endregion

		#region Bcc

		public string[] BccRecipients
		{
			get { return GetEmailAddressArray(Bcc); }
		}

		[MaxLength(512)]
		public ZString Bcc
		{
			get { return bcc; }
			set
			{
				CheckMaximumLength(BccInfo, value);
				SetNonPersistentPropertyValue(BccInfo, ref bcc, value.Replace(" ", ""));
				if (!IsValidationSuspended)
				{
					Validation.ValidateBcc();
				}
			}
		}

		public ZPropertyInfo BccInfo
		{
			get { return GetZPropertyInfo(nameof(Bcc)); }
		}

		ZString bcc;

		#endregion

		public override string AllRecipientsCommaDelimited
		{
			get
			{
				string result = base.AllRecipientsCommaDelimited;
				if (!Bcc.IsEmpty)
				{
					if (result.Length > 0)
					{
						result += ";";
					}
					result += Bcc.Trim(';');
				}
				if (result.Length > 0)
				{
					result = result.Replace(";", ", ");
				}
				return result;
			}
		}

		public override string[] AllRecipients
		{
			get
			{
				var list = new List<string>(base.AllRecipients);
				list.AddRange(BccRecipients);
				return list.ToArray();
			}
		}

		public override ZString Body
		{
			get { return base.Body; }
			set
			{
				base.Body = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateBody();
				}
				BodyInfo.RefreshBinding();
			}
		}

		public bool FilledBodyIsMandatory
		{
			get { return filledBodyIsMandatory; }
			set { filledBodyIsMandatory = value; }
		}
		bool filledBodyIsMandatory;

		#region Templates

		[BusinessObjectTestExclude] // don't want a max length
		[List("Templates")]
		public ZGuid Template
		{
			get { return template; }
			set
			{
				if (template != value)
				{
					template = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTemplate();
					}
					if (!TemplateInfo.HasErrors() && !value.IsEmpty)
					{
						Body = Templates.GetTemplateBody(value);
					}
					TemplateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TemplateInfo
		{
			get { return GetZPropertyInfo(nameof(Template)); }
		}

		[DocumentFieldExcludeFromMap]
		public IMailItemTemplateCollection Templates
		{
			get
			{
				if (templates == null)
				{
					templates = ObjectFactory.Get<IMailItemTemplateCollection>();
					templates.CategoryCodeToFilter = Source.TemplateCategory;
					templates.FilterByCurrentLoginDetails();
				}
				return templates;
			}
		}

		ZGuid template;
		IMailItemTemplateCollection templates;

		#endregion

		#endregion

		public new HtmlEmailWithAttachmentValidation Validation
		{
			get { return (HtmlEmailWithAttachmentValidation)GetNewValidation(); }
		}

		protected override EmailWithAttachmentValidation GetNewValidation()
		{
			return new HtmlEmailWithAttachmentValidation(this);
		}

		protected override EmailDef GetNewEmailDef()
		{
			return new HtmlEmailDef();
		}

		protected override EmailDef GetEmailCore()
		{
			var email = (HtmlEmailDef)base.GetEmailCore();
			email.LoadHtmlUsingTemplate(ParseMessage(email.Body));
			email.AddRecipientForUserCommunication(BccRecipients, RecipientDef.RecipientTypes.BCC);
			return email;
		}

		protected string ParseMessage(ZString message)
		{
			if (Source.DocWrapperType != null)
			{
				BusinessObject bizO;
				if (Source is NonPersistentBusinessObject && Source.DocManagerInfo != null) // example of Source being a NonPersistentBO is ProjectContactForEmail
				{
					bizO = Source.DocManagerInfo.BusinessEntity;
				}
				else
				{
					bizO = Source as BusinessObject;
				}

				if (bizO != null)
				{
					var parser = DocumentParser.New(Source.DocWrapperType, bizO.Factory);
					message = parser.Parse(bizO, message);
				}
			}

			return NormaliseWhitespaceCharactersForHtml(message);
		}

		protected string NormaliseWhitespaceCharactersForHtml(ZString text)
		{
			string result = NormaliseNewLine(text);
			result = result.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
			result = result.Replace("\r\n", "<br />");
			return result;
		}

		protected string NormaliseNewLine(ZString originalString)
		{
			return Regex.Replace(originalString, @"\r\n|\n|\r", "\r\n");
		}

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			return Source is ICustomFieldProvider sourceProvider ?
				sourceProvider.GetCustomBusinessObject(shouldRefresh) : null;
		}

		public bool ShouldCheckFormatOfEmailBody { get; set; }
	}
}
