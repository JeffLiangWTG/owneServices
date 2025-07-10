using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryValidation : AutoJobDocumentDeliveryValidation
	{
		public JobDocumentDeliveryValidation(AutoJobDocumentDelivery parent)
			: base(parent)
		{
		}

		#region OD_DocumentGroup

		protected override void CheckJDC_DocumentGroup()
		{
			base.CheckJDC_DocumentGroup();
			ListValidation.ErrorIfInvalidCode(Parent.JDC_DocumentGroupInfo);
			CheckMenuItemAndDocGroupAreMutuallyExclusive(Parent.JDC_DocumentGroupInfo);
		}

		#endregion

		#region JDC_SU_MenuItem

		protected override void CheckJDC_SU_MenuItem()
		{
			base.CheckJDC_SU_MenuItem();
			CheckMenuItemAndDocGroupAreMutuallyExclusive(Parent.JDC_SU_MenuItemInfo);
		}

		#endregion

		#region JDC_DeliveryMethod

		protected override void CheckJDC_DeliveryMethod()
		{
			base.CheckJDC_DeliveryMethod();
			MandatoryValidation.CheckEntered(Parent.JDC_DeliveryMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JDC_DeliveryMethodInfo);

			if (!Parent.JDC_OC_Contact.IsValid && Parent.JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.DoNotDeliver)
			{
				Parent.JDC_DeliveryMethodInfo.AddError(Res.GetString("46ffa1a5-57ac-4549-bc57-6faa62709995", "The delivery method {0} can only be used when a valid organization contact is selected.", Core.Constants.ContactNotifyModes.DoNotDeliver));
			}
		}

		#endregion

		#region JDC_AttachmentType

		protected override void CheckJDC_AttachmentType()
		{
			base.CheckJDC_AttachmentType();
			if (DeliveryMethodHelper.IsEmailOrEPrint(Parent.JDC_DeliveryMethod))
			{
				MandatoryValidation.CheckEntered(Parent.JDC_AttachmentTypeInfo);
				if (Parent.JDC_DeliveryMethod != Core.Constants.ContactNotifyModes.Email && (Parent.JDC_AttachmentType == OrgConstants.AttachmentType.HTML || Parent.JDC_AttachmentType == OrgConstants.AttachmentType.HTMF || Parent.JDC_AttachmentType == OrgConstants.AttachmentType.PDFC))
				{
					Parent.JDC_AttachmentTypeInfo.AddError(Res.GetString("bf5bafe8-dbb5-419a-9ce4-b78064d1c09f", "Attachment type {0} is only supported for Email.", Parent.JDC_AttachmentType));
				}
			}
			ListValidation.ErrorIfInvalidCode(Parent.JDC_AttachmentTypeInfo);
		}

		#endregion

		#region FaxNumber

		protected override void CheckJDC_FaxNumber()
		{
			base.CheckJDC_AttachmentType();
			if (Parent.JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
			{
				MandatoryValidation.CheckEntered(Parent.JDC_FaxNumberInfo);
				if (!Parent.JDC_FaxNumberInfo.HasErrors())
				{
					PhoneValidator.Validate(Parent.JDC_FaxNumberInfo, null, null, Parent.Header?.ClosestPort);
				}
			}
		}

		PhoneNumberFormatterAndValidator PhoneValidator => phoneValidator ?? (phoneValidator = new PhoneNumberFormatterAndValidator());
		PhoneNumberFormatterAndValidator phoneValidator;

		#endregion

		#region CopyRecipients

		#region JDC_EmailToRecipientsAsString

		public void ValidateJDC_EmailToRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.JDC_EmailToRecipientsAsStringInfo);
		}

		protected void CheckJDC_EmailToRecipientsAsString()
		{
			if (DeliveryMethodHelper.IsEmail(Parent.JDC_DeliveryMethod))
			{
				MandatoryValidation.CheckEntered(Parent.JDC_EmailToRecipientsAsStringInfo);
			}
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.JDC_EmailToRecipientsAsStringInfo, JobDocumentDeliveryCopyRecipientSchema.JDR_EmailAddress);
		}

		#endregion

		#region JDC_CarbonCopyRecipientsAsString

		public void ValidateJDC_CarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.JDC_CarbonCopyRecipientsAsStringInfo);
		}

		protected void CheckJDC_CarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.JDC_CarbonCopyRecipientsAsStringInfo, JobDocumentDeliveryCopyRecipientSchema.JDR_EmailAddress);
		}

		#endregion

		#region JDC_BlindCarbonCopyRecipientsAsString

		public void ValidateJDC_BlindCarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.JDC_BlindCarbonCopyRecipientsAsStringInfo);
		}

		protected void CheckJDC_BlindCarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.JDC_BlindCarbonCopyRecipientsAsStringInfo, JobDocumentDeliveryCopyRecipientSchema.JDR_EmailAddress);
		}

		#endregion

		#endregion

		void CheckMenuItemAndDocGroupAreMutuallyExclusive(ZPropertyInfo info)
		{
			if (Parent.JDC_DocumentGroup.IsEmpty && !Parent.JDC_SU_MenuItem.IsValid)
			{
				info.AddError(Res.GetString("09e1a0db-6580-4436-8b54-b8055da72f7b", "Either a document group or a specific document must be entered."));
			}
			if (!Parent.JDC_DocumentGroup.IsEmpty && Parent.JDC_SU_MenuItem.IsValid)
			{
				info.AddError(Res.GetString("d01a39f1-84ac-4453-9e45-f0284b9e3f56", "A document group and a specific document cannot be entered at the same time."));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJDC_EmailToRecipientsAsString();
			ValidateJDC_CarbonCopyRecipientsAsString();
			ValidateJDC_BlindCarbonCopyRecipientsAsString();
		}

		new JobDocumentDelivery Parent => (JobDocumentDelivery)base.Parent;
	}
}
