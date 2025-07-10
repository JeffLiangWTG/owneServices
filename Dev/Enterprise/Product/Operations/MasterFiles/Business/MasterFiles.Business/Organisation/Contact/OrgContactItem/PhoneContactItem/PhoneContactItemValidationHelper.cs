using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class PhoneContactItemValidationHelper
	{
		public PhoneContactItemValidationHelper(OrgContact contact)
		{
			this.contact = contact;
		}

		public PhoneContactItemValidationHelper(PhoneContactItem phoneItem)
		{
			this.phoneItem = phoneItem;
		}

		readonly OrgContact contact;
		readonly PhoneContactItem phoneItem;

		OrgContact Contact
		{
			get { return contact ?? phoneItem.Contact; }
		}

		#region Phone Numbers

		public void ValidateFaxAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Fax);
		}

		public void ValidateHomeAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Home);
		}

		public void ValidateMobileAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Mobile);
		}

		public void ValidateOtherAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Other);
		}

		public void ValidatePagerAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Pager);
		}

		public void ValidateWorkAddress(ZPropertyInfo propertyInfo)
		{
			var contact = Contact;
			ValidatePhoneNumber(propertyInfo, contact, PhoneContactItemDescriptionList.Codes.Work);
		}

		void ValidatePhoneNumber(ZPropertyInfo propertyInfo, OrgContact contact, string phoneDescription)
		{
			var closestPort = contact != null ? contact.OrgClosestPort : null;
			PhoneNumberValidation.PerformNumberValidation(propertyInfo, closestPort, true);
			if (contact != null && contact.IsEnglish)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(propertyInfo);
			}

			CheckAddressIsUniqueInCollection(propertyInfo, contact, phoneDescription);
		}

		#endregion

		#region Skype

		public void ValidateSkypeAddress(ZPropertyInfo propertyInfo)
		{
			SkypeValidation.ValidateSkypeId(propertyInfo);
			CheckAddressIsUniqueInCollection(propertyInfo, Contact, PhoneContactItemDescriptionList.Codes.Skype);
		}

		#endregion

		#region CheckAddressIsUniqueInCollection

		public void CheckAddressIsUniqueInCollection(ZPropertyInfo propertyInfo, OrgContact contact, string phoneDescription)
		{
			if (contact != null)
			{
				var propertyValue = (ZString)propertyInfo.Value;
				var propertyValueCount = 0;
				foreach (var phoneItem in contact.PhoneContactItems.GetItemsWithDescription(phoneDescription))
				{
					if (phoneItem.OI_Address_Formatted.EqualsIgnoringCase(propertyValue))
					{
						propertyValueCount++;
						if (propertyValueCount > 1)
						{
							propertyInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(propertyInfo.HumanReadableName));
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Implementations

		#region Phone Number Validator

		internal PhoneNumberFormatAndValidation PhoneNumberValidation
		{
			get { return phoneNumberValidation ?? (phoneNumberValidation = new PhoneNumberFormatAndValidation()); }
		}
		PhoneNumberFormatAndValidation phoneNumberValidation;

		#endregion

		#region SkypeIdValidation

		SkypeIdValidation SkypeValidation
		{
			get { return skypeValidation ?? (skypeValidation = new SkypeIdValidation()); }
		}
		SkypeIdValidation skypeValidation;

		#endregion

		#endregion
	}
}
