using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DocDeliveryContactValidation : ZValidation
	{
		public DocDeliveryContactValidation(DocDeliveryContact parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		readonly DocDeliveryContact Parent;

		#region Delivery Method

		public void ValidateDeliveryMethod()
		{
			ValidateCalculatedProperty(Parent.DeliveryMethodInfo);
		}

		protected void CheckDeliveryMethod()
		{
			MandatoryValidation.CheckEntered(Parent.DeliveryMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DeliveryMethodInfo);
			AddDeliveryMethodWarningIfHasNonSupportedDocuments(Parent.DeliveryMethodInfo);
			AddDeliveryMethodErrorIfEPrintAndStaffHasNoEmail(Parent.DeliveryMethodInfo);
			AddDeliveryMethodErrorIfEmailAttachmentIsOutOfSizeLimit(Parent.DeliveryMethodInfo);
		}

		public void ValidateDeliveryMethodDescription()
		{
			ValidateCalculatedProperty(Parent.DeliveryMethodDescriptionInfo);
		}

		protected void CheckDeliveryMethodDescription()
		{
			MandatoryValidation.CheckEntered(Parent.DeliveryMethodDescriptionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DeliveryMethodDescriptionInfo);
			AddDeliveryMethodWarningIfHasNonSupportedDocuments(Parent.DeliveryMethodDescriptionInfo);
			AddDeliveryMethodErrorIfEPrintAndStaffHasNoEmail(Parent.DeliveryMethodDescriptionInfo);
			AddDeliveryMethodErrorIfEmailAttachmentIsOutOfSizeLimit(Parent.DeliveryMethodDescriptionInfo);
		}

		void AddDeliveryMethodWarningIfHasNonSupportedDocuments(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.ParentCollections.Count > 0 && Parent.DeliveryMethod != Core.Constants.ContactNotifyModes.EDoc)
			{
				var contactCollection = Parent.ParentCollections.First() as DocDeliveryContactCollection;
				if (contactCollection?.Deliverables != null)
				{
					var documentsToPrint = contactCollection.Deliverables
						.OfType<IDocument>()
						.Where(d => d.IncludeInPrint);

					var warning = NonSupportedDeliveryMethodMessageBuilder.GetMessage(documentsToPrint, Parent.DeliveryMethod);

					if (warning.Length > 0)
					{
						info.AddWarning(warning);
					}
				}
			}
		}

		void AddDeliveryMethodErrorIfEPrintAndStaffHasNoEmail(ZPropertyInfo info)
		{
			if (Parent.DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint
				&& Globals.IsUserInteractive
				&& GlbStaff.CurrentUser != null
				&& (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty || GlbStaff.CurrentUser.GS_EmailAddressInfo.HasErrors()))
			{
				info.AddError(Res.GetString("72df3699-2d23-46bc-97b1-abb1476b7247", "ePrint can only be used when the currently logged in user has a valid email address. Please set one on your staff profile."));
			}
		}

		void AddDeliveryMethodErrorIfEmailAttachmentIsOutOfSizeLimit(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.ParentCollections.Count > 0 &&
					Parent.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				var contactCollection = Parent.ParentCollections.First() as DocDeliveryContactCollection;
				if (contactCollection?.Deliverables != null)
				{
					var filesToBeDelivered = contactCollection.Deliverables.OfType<IDeliveryEmailAttachment>();
					var error = DeliveryMethodHelper.GetEmailAttachmentOutOfSizeLimitError(filesToBeDelivered);
					if (!string.IsNullOrEmpty(error))
					{
						info.AddError(error);
					}
				}
			}
		}

		#endregion

		#region Attachment Type

		public void ValidateAttachmentType()
		{
			ValidateCalculatedProperty(Parent.AttachmentTypeInfo);
		}

		protected void CheckAttachmentType()
		{
			if (!Parent.AttachmentType_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.AttachmentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AttachmentTypeInfo);
				if (Parent.DeliveryMethod != Core.Constants.ContactNotifyModes.Email)
				{
					if (Parent.AttachmentType == OrgConstants.AttachmentType.HTML || Parent.AttachmentType == OrgConstants.AttachmentType.HTMF || Parent.AttachmentType == OrgConstants.AttachmentType.PDFC)
					{
						Parent.AttachmentTypeInfo.AddError(Res.GetString("30dcaf84-911b-4c13-b97e-c725105334c7", "Attachment type {0} is only supported for Email.", Parent.AttachmentType));
					}
				}
			}
		}

		#endregion

		#region Delivery Address

		public void ValidateDeliveryAddress()
		{
			ValidateCalculatedProperty(Parent.DeliveryAddressInfo);
		}

		protected virtual void CheckDeliveryAddress()
		{
			if (Parent.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				MandatoryValidation.CheckEntered(Parent.DeliveryAddressInfo, Res.GetString("736a4732-6253-4456-b2f5-0275118ad883", "Email Address"));
				if (!Parent.DeliveryAddressInfo.HasErrors())
				{
					EmailValidation.ValidateCommaSeparatedEmailAddresses(Parent.DeliveryAddressInfo);
				}
			}
			else if (Parent.DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint)
			{
				if (Parent.DeliveryAddressInfo.Value.IsEmpty)
				{
					Parent.DeliveryAddressInfo.AddError(Res.GetString("b4202771-7a8b-4e61-863a-232190ae3614", "ePrint email address is not defined. This can be set in the registry setting Documents > ePrint Email Address."));
				}
			}
			else if (Parent.DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
			{
				MandatoryValidation.CheckEntered(Parent.DeliveryAddressInfo, Res.GetString("569f0dc0-5561-4857-9eef-6aeed26c8e22", "Fax Number"));
				if (!Parent.DeliveryAddressInfo.HasErrors())
				{
					Parent.PhoneNumberFormatterAndValidator.Validate(Parent.DeliveryAddressInfo, null, null, Parent.ClosestPort);
				}
			}
		}

		public void ValidateEmailFromAddress()
		{
			ValidateCalculatedProperty(Parent.EmailFromAddressInfo);
		}

		protected void CheckEmailFromAddress()
		{
			if (!Parent.EmailFromAddress.IsEmpty && Parent.EmailFromAddressWithTypeList.GetCodeFromDescription(Parent.EmailFromAddress) == null)
			{
				Parent.EmailFromAddressInfo.AddError(Res.GetString("B5D334C1-07D8-4A38-B1B1-DEB883CD798A", "Please select a valid Email Address."));
			}
		}

		#endregion

		#region Organisation

		public void ValidateOrgHeaderPK()
		{
			ValidateCalculatedProperty(Parent.OrgHeaderPKInfo);
		}

		protected virtual void CheckOrgHeaderPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.OrgHeaderPKInfo);
		}

		public void ValidateOrgAddressPK()
		{
			ValidateCalculatedProperty(Parent.OrgAddressPKInfo);
		}

		protected void CheckOrgAddressPK()
		{
			TypeValidation.CheckValidGuid(Parent.OrgAddressPKInfo);
		}

		#endregion

		#region Copy Recipients

		#region EmailCarbonCopyRecipientsAsString

		public void ValidateEmailCarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.EmailCarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckEmailCarbonCopyRecipientsAsString()
		{
			EmailValidation.ValidateCommaSeparatedEmailAddresses(Parent.EmailCarbonCopyRecipientsAsStringInfo);
		}

		#endregion

		#region EmailBlindCarbonCopyRecipientsAsString

		public void ValidateEmailBlindCarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.EmailBlindCarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckEmailBlindCarbonCopyRecipientsAsString()
		{
			EmailValidation.ValidateCommaSeparatedEmailAddresses(Parent.EmailBlindCarbonCopyRecipientsAsStringInfo);
		}

		#endregion

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(DocDeliveryContactValidation); }
		}

		public override void ValidateAll()
		{
			ValidateDeliveryMethod();
			ValidateDeliveryMethodDescription();
			ValidateAttachmentType();
			ValidateDeliveryAddress();
			ValidateOrgHeaderPK();
			ValidateOrgAddressPK();
			ValidateEmailFromAddress();
			ValidateStaffCode();
			ValidateDeliveryRecipientType();
		}

		EmailAddressForSendValidation EmailValidation
		{
			get { return emailValidation ?? (emailValidation = new EmailAddressForSendValidation(Parent.Factory)); }
		}

		EmailAddressForSendValidation emailValidation;

		#endregion

		#region staff code

		public void ValidateStaffCode()
		{
			ValidateCalculatedProperty(Parent.StaffCodeInfo);
		}

		protected virtual void CheckStaffCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.StaffCodeInfo);
		}

		#endregion

		#region delivery to type

		public void ValidateDeliveryRecipientType()
		{
			ValidateCalculatedProperty(Parent.DeliveryRecipientTypeInfo);
		}

		protected void CheckDeliveryRecipientType()
		{
			if (!string.IsNullOrEmpty(Parent.DeliveryRecipientTypeInfo.Value.ToString()) && string.IsNullOrEmpty(Parent.DeliveryRecipientTypes.GetCodeFromDescription(Parent.DeliveryRecipientTypeInfo.Value.ToString())))
			{
				Parent.DeliveryRecipientTypeInfo.AddError(Res.GetString("5EAE9041-A7F5-4BC0-B1EE-B98600FA80E1", "Enter a valid selection.", Parent.DeliveryRecipientTypeInfo.HumanReadableName));
			}
		}

		#endregion
	}
}
