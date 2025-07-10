using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	#region enum MessageDeliveryMethod

	public enum MessageDeliveryMethod
	{
		EMail,
		EMailViaMailClient
	}

	#endregion

	public class EmailSenderConfiguration : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Construction

		public EmailSenderConfiguration(ISendEmailSource contextItemSource)
		{
			this.contextItemSource = contextItemSource;

			if (CanSaveToEDocs)
			{
				ContextItemSource.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			}
		}

		#endregion

		#region DeliveryMethod

		public MessageDeliveryMethod DeliveryMethod
		{
			get { return deliveryMethod; }
			set
			{
				deliveryMethod = value;
				if (MessageDeliveryMethod.EMailViaMailClient == value)
				{
					HtmlEmail.Template = Guid.Empty;
					HtmlEmail.FilledBodyIsMandatory = false;
				}
				else
				{
					HtmlEmail.FilledBodyIsMandatory = true;
				}
			}
		}

		MessageDeliveryMethod deliveryMethod;

		#endregion

		public HtmlEmailWithAttachment HtmlEmail
		{
			get
			{
				if (htmlEmail == null)
				{
					htmlEmail = new HtmlEmailWithAttachment(contextItemSource);
					RegisterEditableChildObject(htmlEmail);
				}
				return htmlEmail;
			}
		}
		HtmlEmailWithAttachment htmlEmail;

		public EmailDef GetEmail()
		{
			var email = HtmlEmail.GetEmail();
			var sourceAsBizObj = ContextItemSource as BusinessObject;
			if (sourceAsBizObj != null)
			{
				email.SetupBusinessEntityInfo(sourceAsBizObj);
			}

			return email;
		}

		public virtual bool CheckIsReadyToSendEmail()
		{
			return HtmlEmail.CheckIsReadyToSendEmail();
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateSelectedRecipientsCount();
			ValidateSaveToEDocs();
		}

		#endregion

		#region New Properties

		public ISendEmailSource ContextItemSource
		{
			get { return contextItemSource; }
		}

		readonly ISendEmailSource contextItemSource;

		#endregion

		#region SelectedRecipientsCount

		public ZInt SelectedRecipientsCount
		{
			get { return HtmlEmail.AllRecipients.Length; }
		}

		public ZPropertyInfo SelectedRecipientsCountInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedRecipientsCount), "Recipients"); }
		}

		public void ValidateSelectedRecipientsCount()
		{
			SelectedRecipientsCountInfo.ClearAllNotifications();
			if (SelectedRecipientsCount < 1)
			{
				SelectedRecipientsCountInfo.AddError(Res.GetString("ed010726-6414-4613-8638-85d13ac1a9db", "No recipients have been selected to receive the message."));
			}
		}

		#endregion

		#region eDocs

		public ZBool CanSaveToEDocs
		{
			get { return contextItemSource != null && contextItemSource.DocManagerInfo != null; }
		}

		[MaxLength(3)]
		[List("EDocsDocumentTypes")]
		public ZString SaveToEDocsDocumentType
		{
			get { return saveToEDocsDocumentType; }
			set
			{
				if (saveToEDocsDocumentType != value)
				{
					CheckMaximumLength(SaveToEDocsDocumentTypeInfo, value);
					saveToEDocsDocumentType = value;
					ValidateSaveToEDocs();
					SaveToEDocsDocumentTypeInfo.RefreshBinding();
				}
			}
		}

		ZString saveToEDocsDocumentType;

		public ZPropertyInfo SaveToEDocsDocumentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SaveToEDocsDocumentType)); }
		}

		[DocumentFieldExcludeFromMap]
		public IRefDocTypeCollection EDocsDocumentTypes
		{
			get
			{
				if (eDocsDocumentTypes == null)
				{
					if (CanSaveToEDocs)
					{
						eDocsDocumentTypes = contextItemSource.DocManagerInfo.GetAvailableDocumentTypes();
					}
					else
					{
						eDocsDocumentTypes = new RefDocTypeCollection(new BusinessObjectFactory());
					}
				}
				return eDocsDocumentTypes;
			}
		}
		IRefDocTypeCollection eDocsDocumentTypes;

		void ValidateSaveToEDocs()
		{
			SaveToEDocsDocumentTypeInfo.ClearAllNotifications();
			if (CanSaveToEDocs && !SaveToEDocsDocumentType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(SaveToEDocsDocumentTypeInfo, EDocsDocumentTypes, GetSaveToEDocsErrorMessage());
			}
		}

		IMultilingualString GetSaveToEDocsErrorMessage()
		{
			var specificCategory = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(contextItemSource.DocManagerInfo.DocManagerCode);
			return ResString.GetMultilingualString("72CCBFAB-C5FB-4BAB-A25B-F9901D1D51B7", "Select a Document Type where the Category is ALL or {0}.", specificCategory);
		}

		#endregion
	}
}
