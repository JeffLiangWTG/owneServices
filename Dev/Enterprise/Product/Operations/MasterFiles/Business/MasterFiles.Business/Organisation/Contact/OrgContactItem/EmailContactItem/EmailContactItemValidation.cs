using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class EmailContactItemValidation : ContactItemProxyValidation
	{
		public EmailContactItemValidation(EmailContactItem contactItem)
			: base(contactItem)
		{
			EmailContactItemValidationHelper = new EmailContactItemValidationHelper(Parent);
		}

		readonly EmailContactItemValidationHelper EmailContactItemValidationHelper;

		new EmailContactItem Parent
		{
			get { return (EmailContactItem)base.Parent; }
		}

		protected override void CheckOI_Description()
		{
			base.CheckOI_Description();

			var contact = Parent.Contact;
			if (contact != null)
			{
				if (contact.EmailContactItems.Items.Any(item => item.PK != Parent.PK && item.OI_Description.EqualsIgnoringCase(Parent.OI_Description)))
				{
					Parent.OI_DescriptionInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(Parent.OI_DescriptionInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckOI_Address()
		{
			base.CheckOI_Address();

			if (Parent != null && Parent.Contact != null && string.IsNullOrEmpty(Parent.OI_Address))
			{
				if (Parent.Contact.OC_WebAccessEnabled)
				{
					Parent.OI_AddressInfo.AddError(Res.GetString("7BFA2192-C0FA-41B4-9D99-F977F7BEB33F", "Email is required when web access is enabled."));
				}

				if (Parent.Contact.OC_NotifyMode == Core.Constants.ContactNotifyModes.Email)
				{
					Parent.OI_AddressInfo.AddError(Res.GetString("87F5FECD-B5AF-413D-A37A-D4890341EDFB", "The contacts email address cannot be empty when the Default Delivery Method is set to EML"));
				}
			}

			EmailContactItemValidationHelper.ValidateEmailAddress(Parent.OI_AddressInfo);
		}
	}
}
