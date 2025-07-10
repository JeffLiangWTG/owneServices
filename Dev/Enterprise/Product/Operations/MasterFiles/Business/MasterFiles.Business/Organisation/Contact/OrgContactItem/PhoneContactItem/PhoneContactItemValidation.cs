using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class PhoneContactItemValidation : ContactItemProxyValidation
	{
		public PhoneContactItemValidation(PhoneContactItem contactItem)
			: base(contactItem)
		{
			phoneContactItemValidationHelper = new PhoneContactItemValidationHelper(Parent);
			phoneContactItemPropertyHelper = new PhoneContactItemPropertyHelper(Parent);
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		#region OI_Description

		protected override void CheckOI_Description()
		{
			base.CheckOI_Description();

			var contact = Parent.Contact;
			if (contact != null)
			{
				if (contact.PhoneContactItems.Items.Any(item => item.PK != Parent.PK && item.OI_Description.EqualsIgnoringCase(Parent.OI_Description)))
				{
					Parent.OI_DescriptionInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(Parent.OI_DescriptionInfo.HumanReadableName));
				}
			}
		}

		#endregion

		#region OI_Address_Formatted

		public void ValidateOI_Address_Formatted()
		{
			if (!phoneContactItemPropertyHelper.IsViewDenied)
			{
				ValidateCalculatedProperty(Parent.OI_Address_FormattedInfo);
			}
		}

		protected virtual void CheckOI_Address_Formatted()
		{
			if (!Parent.OI_Address_Formatted.IsEmpty && !phoneContactItemPropertyHelper.IsViewDenied)
			{
				ValidatePhoneNumber(Parent.OI_Address_FormattedInfo, Parent.OI_AddressInfo, Parent.OI_Address_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region Implementations

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateOI_Address_Formatted();
		}

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			if (Parent.IsFormattablePhoneNumber)
			{
				var contact = Parent.Contact;
				if (contact != null)
				{
					PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, contact.DefaultCountryCodeForPhoneNumbers);

					if (!phoneNumberProperty.HasErrors())
					{
						phoneContactItemValidationHelper.CheckAddressIsUniqueInCollection(phoneNumberProperty, contact, Parent.OI_Description);
					}
				}
			}
			else if (Parent.OI_Description == PhoneContactItemDescriptionList.Codes.Skype || Parent.OI_Description == PhoneContactItemDescriptionList.Codes.Skype2)
			{
				phoneContactItemValidationHelper.ValidateSkypeAddress(phoneNumberProperty);
			}
		}

		new PhoneContactItem Parent
		{
			get { return (PhoneContactItem)base.Parent; }
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		readonly PhoneContactItemValidationHelper phoneContactItemValidationHelper;
		readonly PhoneContactItemPropertyHelper phoneContactItemPropertyHelper;

		#endregion
	}
}
