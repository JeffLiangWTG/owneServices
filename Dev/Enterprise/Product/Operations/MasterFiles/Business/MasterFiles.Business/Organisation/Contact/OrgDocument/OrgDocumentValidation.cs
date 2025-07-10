using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentValidation : AutoOrgDocumentValidation
	{
		public OrgDocumentValidation(AutoOrgDocument parent)
			: base(parent)
		{
		}

		#region OD_DefaultContact

		protected override void CheckOD_DefaultContact()
		{
			base.CheckOD_DefaultContact();
			if (Parent.OD_DefaultContact && HasMoreThanOneOfficialContact)
			{
				Parent.OD_DefaultContactInfo.AddError(Res.GetString("5eaec373-c253-4caf-9bc1-a84442da7834", "There can only be one official contact for each document / document group."));
			}
		}

		bool HasMoreThanOneOfficialContact
		{
			get
			{
				bool result = false;

				if (Parent.Contact != null && Parent.Contact.Header != null)
				{
					if (!Parent.OD_DocumentGroup.IsEmpty)
					{
						result = Parent.Contact.Header.HasMoreThanOneDefaultForDocumentGroup(Parent.OD_DocumentGroup);
					}
					else if (!Parent.OD_SU_MenuItem.IsEmpty)
					{
						result = Parent.Contact.Header.HasMoreThanOneDefaultForDocumentMenuItem(Parent.OD_SU_MenuItem);
					}
				}

				return result;
			}
		}

		#endregion

		#region OD_DeliveryBy

		protected override void CheckOD_DeliverBy()
		{
			base.CheckOD_DeliverBy();
			ListValidation.ErrorIfInvalidCode(Parent.OD_DeliverByInfo);
		}

		#endregion

		#region OD_AttachmentType

		protected override void CheckOD_AttachmentType()
		{
			base.CheckOD_AttachmentType();
			if (DeliveryMethodHelper.IsEmailOrEPrint(Parent.OD_DeliverBy))
			{
				MandatoryValidation.CheckEntered(Parent.OD_AttachmentTypeInfo);
				if (Parent.OD_DeliverBy != Core.Constants.ContactNotifyModes.Email && (Parent.OD_AttachmentType == OrgConstants.AttachmentType.HTML || Parent.OD_AttachmentType == OrgConstants.AttachmentType.HTMF || Parent.OD_AttachmentType == OrgConstants.AttachmentType.PDFC))
				{
					Parent.OD_AttachmentTypeInfo.AddError(Res.GetString("bf5bafe8-dbb5-419a-9ce4-b78064d1c09f", "Attachment type {0} is only supported for Email.", Parent.OD_AttachmentType));
				}
			}
			ListValidation.ErrorIfInvalidCode(Parent.OD_AttachmentTypeInfo);
		}

		#endregion

		#region OD_DocumentGroup

		protected override void CheckOD_DocumentGroup()
		{
			base.CheckOD_DocumentGroup();
			ListValidation.ErrorIfInvalidCode(Parent.OD_DocumentGroupInfo);
			CheckMenuItemAndDocGroupAreMutuallyExclusive(Parent.OD_DocumentGroupInfo);
			if (Parent.Contact != null
				&& Parent.Contact.Header != null
				&& Parent.Contact.Header.HasMoreThanOneNotifyParty())
			{
				Parent.OD_DocumentGroupInfo.AddError(Res.GetString("7861e6cf-f806-4086-96db-0d23c32ae43d", "There is already a Notify Party entered. There is only allowed to be one notify party per organization."));
			}
		}

		#endregion

		#region OD_FilterShipmentMode

		protected override void CheckOD_FilterShipmentMode()
		{
			base.CheckOD_FilterShipmentMode();
			MandatoryValidation.CheckEntered(Parent.OD_FilterShipmentModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OD_FilterShipmentModeInfo);
		}

		#endregion

		#region OD_SU_MenuItem

		protected override void CheckOD_SU_MenuItem()
		{
			base.CheckOD_SU_MenuItem();
			CheckMenuItemAndDocGroupAreMutuallyExclusive(Parent.OD_SU_MenuItemInfo);
		}

		void CheckMenuItemAndDocGroupAreMutuallyExclusive(ZPropertyInfo info)
		{
			if (Parent.OD_DocumentGroup.IsEmpty && !Parent.OD_SU_MenuItem.IsValid)
			{
				info.AddError(Res.GetString("09e1a0db-6580-4436-8b54-b8055da72f7b", "Either a document group or a specific document must be entered."));
			}
			if (!Parent.OD_DocumentGroup.IsEmpty && Parent.OD_SU_MenuItem.IsValid)
			{
				info.AddError(Res.GetString("d01a39f1-84ac-4453-9e45-f0284b9e3f56", "A document group and a specific document cannot be entered at the same time."));
			}
		}

		#endregion

		#region OD_FilterForeignPort

		protected override void CheckOD_FilterForeignPort()
		{
			base.CheckOD_FilterForeignPort();
			ListValidation.ErrorIfInvalidCode(Parent.OD_FilterForeignPortInfo);
		}

		#endregion

		#region OD_FilterLocalPort

		protected override void CheckOD_FilterLocalPort()
		{
			base.CheckOD_FilterLocalPort();
			ListValidation.ErrorIfInvalidCode(Parent.OD_FilterLocalPortInfo);
		}

		#endregion

		#region OD_FilterDirection

		protected override void CheckOD_FilterDirection()
		{
			base.CheckOD_FilterDirection();
			MandatoryValidation.CheckEntered(Parent.OD_FilterDirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OD_FilterDirectionInfo);
		}

		#endregion

		#region OD_OH_RelatedFilterByParty

		protected override void CheckOD_OH_RelatedFilterByParty()
		{
			base.CheckOD_OH_RelatedFilterByParty();
			if (Parent.OD_OH_RelatedFilterByParty != ZGuid.Empty)
			{
				var info = Parent.OD_OH_RelatedFilterByPartyInfo;
				bool isCneOrCnr = Parent.OD_DocumentGroup == ContactType.Consignee ||
								  Parent.OD_DocumentGroup == ContactType.Consignor;
				var document = Parent.MenuItem;
				if (document?.SU_ContactType == ContactType.Consignee ||
					document?.SU_ContactType == ContactType.Consignor)
				{
					isCneOrCnr = true;
				}

				if (!isCneOrCnr)
				{
					info.AddWarning(Res.GetString("84b9bbea-a618-4930-9add-0de26e2732d5",
						"Document group should be Consignee (CNE) or Consignor (CNR). Otherwise, system cannot find the related party."));
				}
			}
		}

		#endregion

		#region CopyRecipients

		#region OD_CarbonCopyRecipientsAsString

		public void ValidateOD_CarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.OD_CarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckOD_CarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.OD_CarbonCopyRecipientsAsStringInfo, OrgDocumentCopyRecipientSchema.ODR_EmailAddress);
		}

		#endregion

		#region OD_BlindCarbonCopyRecipientsAsString

		public void ValidateOD_BlindCarbonCopyRecipientsAsString()
		{
			ValidateCalculatedProperty(Parent.OD_BlindCarbonCopyRecipientsAsStringInfo);
		}

		protected virtual void CheckOD_BlindCarbonCopyRecipientsAsString()
		{
			EmailAddressValidation.ValidateEmailAddressesAsString(Parent.OD_BlindCarbonCopyRecipientsAsStringInfo, OrgDocumentCopyRecipientSchema.ODR_EmailAddress);
		}

		#endregion

		#endregion

		new OrgDocument Parent
		{
			get { return (OrgDocument)base.Parent; }
		}
	}
}
