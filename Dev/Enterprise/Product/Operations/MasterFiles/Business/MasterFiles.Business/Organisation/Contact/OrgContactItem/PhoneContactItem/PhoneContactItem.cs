using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneContactItem : ContactItemProxy
	{
		public PhoneContactItem(ZString description, ZPropertyInfo addressInfoOnOrgContact)
			: base(OrgContactItemTypes.Codes.Phone, description, addressInfoOnOrgContact)
		{
		}

		public PhoneContactItem(OrgContactItem orgContactItem)
			: base(orgContactItem)
		{
			if (orgContactItem.OI_ContactItemType != OrgContactItemTypes.Codes.Phone)
			{
				throw new ArgumentException("orgContactItem.OI_ContactItemType must be Phone, but was " + orgContactItem.OI_ContactItemType);
			}
		}

		public PhoneContactItem(OrgContact contact)
			: base(contact, OrgContactItemTypes.Codes.Phone)
		{
		}

		#region DefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
		}

		#endregion

		#region AddressInfoOnOrgContact

		protected override ZPropertyInfo GetAddressInfoOnOrgContact(OrgContact contact, string description)
		{
			if (contact != null)
			{
				var addressColumnName = GetAddressColumnNameOnOrgContact(description);
				if (!string.IsNullOrEmpty(addressColumnName))
				{
					return contact.ZPropertyInfoHash[addressColumnName];
				}
			}

			return null;
		}

		string GetAddressColumnNameOnOrgContact(string description)
		{
			SchemaStringColumn addressColumn;
			if (Lookups.OrgContactColumnsByDescription.TryGetValue(description, out addressColumn))
			{
				return addressColumn.Name;
			}

			return null;
		}

		#endregion

		#region AddressIsManuallyVerifiedInfoOnOrgContact

		protected override ZPropertyInfo GetAddressIsManuallyVerifiedInfoOnOrgContact(OrgContact contact, string description)
		{
			if (contact != null)
			{
				var addressColumnName = GetAddressIsManuallyVerifiedColumnNameOnOrgContact(description);
				if (!string.IsNullOrEmpty(addressColumnName))
				{
					return contact.ZPropertyInfoHash[addressColumnName];
				}
			}

			return null;
		}

		string GetAddressIsManuallyVerifiedColumnNameOnOrgContact(string description)
		{
			string addressColumnName;
			if (Lookups.OrgContactPhoneIsManuallyVerifiedColumnsByDescription.TryGetValue(description, out addressColumnName))
			{
				return addressColumnName;
			}

			return null;
		}

		protected override void SetAddressIsManuallyVerifiedOnOrgContact(OrgContact contact, ZString oI_Description, ZBool value)
		{
			var propertyInfo = GetAddressIsManuallyVerifiedInfoOnOrgContact(contact, oI_Description);
			if (propertyInfo != null)
			{
				propertyInfo.Value = value;
			}
		}

		#endregion

		#region Properties

		#region PhoneNumber

		public PhoneNumber Number
		{
			get
			{
				if (number == null)
				{
					number = new PhoneNumber(OI_Address_FormattedInfo, null, OI_Address_FormattedLocalNumberIfLoggedInSameCountryInfo, OI_Address_IsManuallyVerifiedInfo);
				}
				return number;
			}
		}
		PhoneNumber number;

		#endregion

		#region OI_Address

		public override ZString OI_Address
		{
			get
			{
				return base.OI_Address;
			}

			set
			{
				if (base.OI_Address != value)
				{
					base.OI_Address = value;

					Contact?.Header?.FindDuplicates();
				}
			}
		}

		public void FormatPhoneNumber()
		{
			base.OI_Address = GetFormattedAddress(OI_Description, base.OI_Address);
		}

		ZString GetFormattedAddress(ZString phoneItemDescription, ZString value)
		{
			switch (phoneItemDescription)
			{
				case PhoneContactItemDescriptionList.Codes.Extension:
					return PhoneContactItemPropertyHelper.GetFormattedExtensionAddress(value);

				case PhoneContactItemDescriptionList.Codes.Fax:
					return PhoneContactItemPropertyHelper.GetFormattedFaxAddress(value);

				case PhoneContactItemDescriptionList.Codes.Home:
					return PhoneContactItemPropertyHelper.GetFormattedHomeAddress(value);

				case PhoneContactItemDescriptionList.Codes.Mobile:
				case PhoneContactItemDescriptionList.Codes.Mobile2:
					return PhoneContactItemPropertyHelper.GetFormattedMobileAddress(value);

				case PhoneContactItemDescriptionList.Codes.Other:
					return PhoneContactItemPropertyHelper.GetFormattedOtherAddress(value);

				case PhoneContactItemDescriptionList.Codes.Pager:
					return PhoneContactItemPropertyHelper.GetFormattedPagerAddress(value);

				case PhoneContactItemDescriptionList.Codes.Skype:
				case PhoneContactItemDescriptionList.Codes.Skype2:
					return PhoneContactItemPropertyHelper.GetFormattedSkypeAddress(value);

				case PhoneContactItemDescriptionList.Codes.Work:
				case PhoneContactItemDescriptionList.Codes.Work2:
					return PhoneContactItemPropertyHelper.GetFormattedWorkAddress(value);
			}

			return value;
		}

		public int OI_Address_MaxLength
		{
			get { return CalculateAddressMaxLength(OI_Description); }
		}

		static int CalculateAddressMaxLength(string description)
		{
			switch (description)
			{
				case PhoneContactItemDescriptionList.Codes.Extension:
					return OrgContact.Schema.OC_PhoneExtensionMaxLength;

				case PhoneContactItemDescriptionList.Codes.Fax:
					return OrgContact.Schema.OC_FaxMaxLength;

				case PhoneContactItemDescriptionList.Codes.Home:
					return OrgContact.Schema.OC_HomePhoneMaxLength;

				case PhoneContactItemDescriptionList.Codes.Mobile:
				case PhoneContactItemDescriptionList.Codes.Mobile2:
					return OrgContact.Schema.OC_MobileMaxLength;

				case PhoneContactItemDescriptionList.Codes.Other:
					return OrgContact.Schema.OC_OtherPhoneMaxLength;

				case PhoneContactItemDescriptionList.Codes.Pager:
					return OrgContact.Schema.OC_PagerMaxLength;

				case PhoneContactItemDescriptionList.Codes.Skype:
				case PhoneContactItemDescriptionList.Codes.Skype2:
					return OrgContactItem.Schema.OI_AddressMaxLength;

				case PhoneContactItemDescriptionList.Codes.Work:
				case PhoneContactItemDescriptionList.Codes.Work2:
					return OrgContact.Schema.OC_PhoneMaxLength;
			}

			return OrgContactItem.Schema.OI_AddressMaxLength;
		}

		#endregion

		#region OI_Address_Formatted

		String ViewDeniedMessage => Res.GetString("cd292400-8e29-45b1-9fd5-135a6f21ed83", "** View Denied **");

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OI_Address_Formatted
		{
			get
			{
				if (PhoneContactItemPropertyHelper.IsViewDenied)
				{
					return ViewDeniedMessage;
				}
				else
				{
					return IsFormattablePhoneNumber ? PhoneNumberPropertyHelper.GetPhoneNumber(OI_AddressInfo) : OI_Address;
				}
			}
			set
			{
				if (!PhoneContactItemPropertyHelper.IsViewDenied)
				{
					var rawFormattedValue = OI_Address_Formatted;

					if (!addressIsSetting)
					{
						addressIsSetting = true;
						try
						{
							if (IsFormattablePhoneNumber)
							{
								PhoneNumberPropertyHelper.SetPhoneNumber(OI_AddressInfo, OI_Address_FormattedInfo, value,
									((PhoneContactItemValidation)Validation).ValidateOI_Address_Formatted, OI_Address_IsManuallyVerifiedInfo);
							}
							else
							{
								OI_Address = GetFormattedAddress(OI_Description, value);
								((PhoneContactItemValidation)Validation).ValidateOI_Address_Formatted();
							}
							OI_Address_FormattedInfo.RefreshBinding();
						}
						finally
						{
							addressIsSetting = false;
						}
					}

					if (rawFormattedValue != OI_Address_Formatted)
					{
						Contact?.Person?.FindDuplicates(Contact);
					}
				}
			}
		}

		bool addressIsSetting;

		public bool OI_Address_Formatted_ReadOnly => PhoneContactItemPropertyHelper.IsViewDenied || PhoneContactItemPropertyHelper.IsModifyDenied;

		public ZPropertyInfo OI_Address_FormattedInfo
		{
			get { return GetZPropertyInfo(nameof(OI_Address_Formatted)); }
		}

		/// <summary>
		/// Gets whether the current phone contact item is a formattable phone number.
		/// </summary>
		public bool IsFormattablePhoneNumber
		{
			get
			{
				var description = OI_Description;
				var result = false;
				switch (description)
				{
					case PhoneContactItemDescriptionList.Codes.Fax:
					case PhoneContactItemDescriptionList.Codes.Home:
					case PhoneContactItemDescriptionList.Codes.Mobile:
					case PhoneContactItemDescriptionList.Codes.Mobile2:
					case PhoneContactItemDescriptionList.Codes.Other:
					case PhoneContactItemDescriptionList.Codes.Pager:
					case PhoneContactItemDescriptionList.Codes.Work:
					case PhoneContactItemDescriptionList.Codes.Work2:
						result = true;
						break;
				}
				return result;
			}
		}

		#endregion

		#region OI_Address_IsManuallyVerified

		public override ZBool OI_Address_IsManuallyVerified
		{
			get
			{
				var isManuallyVerified = false;

				if (IsFormattablePhoneNumber)
				{
					var columnName = GetAddressColumnNameOnOrgContact(OI_Description);
					if (AddressInfoOnOrgContact != null)
					{
						isManuallyVerified = PhoneNumberFormatterAndValidator.IsManuallyVerified(Contact, columnName, Contact.AddOnRuleAcks);
					}
					else if (OrgContactItem != null)
					{
						isManuallyVerified = PhoneNumberFormatterAndValidator.IsManuallyVerified(OrgContactItem, OrgContactItemSchema.Constants.OI_Address, OrgContactItem.AddOnRuleAcks);
					}
				}
				return isManuallyVerified;
			}
			set
			{
				if (IsFormattablePhoneNumber && OI_Address_IsManuallyVerified != value)
				{
					base.OI_Address_IsManuallyVerified = value;

					var columnName = GetAddressColumnNameOnOrgContact(OI_Description);

					if (AddressInfoOnOrgContact != null)
					{
						PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OI_Address_IsManuallyVerifiedInfo, value, Contact.AddOnRuleAcks, Contact, columnName, ((PhoneContactItemValidation)Validation).ValidateOI_Address_Formatted, Number.FormattedForBindingInfo);
					}
					else if (OrgContactItem != null)
					{
						PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OI_Address_IsManuallyVerifiedInfo, value, OrgContactItem.AddOnRuleAcks, OrgContactItem, OrgContactItemSchema.Constants.OI_Address, ((PhoneContactItemValidation)Validation).ValidateOI_Address_Formatted, Number.FormattedForBindingInfo);
					}
				}
			}
		}

		#endregion

		#region OI_Address_FormattedLocalNumberIfLoggedInSameCountry

		public ZString OI_Address_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(OI_AddressInfo); }
		}

		public ZPropertyInfo OI_Address_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(nameof(OI_Address_FormattedLocalNumberIfLoggedInSameCountry)); }
		}

		#endregion

		#region OI_Description

		public override ZString OI_Description
		{
			get { return base.OI_Description; }
			set
			{
				if (OI_Description != value)
				{
					var newMaxLength = CalculateAddressMaxLength(value);
					if (base.OI_Address.Length > newMaxLength)
					{
						base.OI_Address = base.OI_Address.Left(newMaxLength);
					}
					base.OI_Description = value;
					OI_Address_Formatted = OI_Address_Formatted; // Revalidate the phone number.
				}
			}
		}

		#endregion

		#region Implementations

		PhoneContactItemPropertyHelper PhoneContactItemPropertyHelper
		{
			get { return phoneContactItemPropertyHelper ?? (phoneContactItemPropertyHelper = new PhoneContactItemPropertyHelper(this)); }
		}
		PhoneContactItemPropertyHelper phoneContactItemPropertyHelper;

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelper ?? (phoneNumberPropertyHelper = new PhoneNumberPropertyHelper(() => Contact.DefaultCountryCodeForPhoneNumbers)); }
		}
		PhoneNumberPropertyHelper phoneNumberPropertyHelper;

		#endregion

		#endregion

		#region IsCallable

		public ZBool IsCallable
		{
			get { return CallableDescriptions.Contains(OI_Description); }
		}

		HashSet<ZString> CallableDescriptions
		{
			get
			{
				if (callableDescriptions == null)
				{
					callableDescriptions = new HashSet<ZString>
						{
							PhoneContactItemDescriptionList.Codes.Home,
							PhoneContactItemDescriptionList.Codes.Mobile,
							PhoneContactItemDescriptionList.Codes.Mobile2,
							PhoneContactItemDescriptionList.Codes.Other,
							PhoneContactItemDescriptionList.Codes.Pager,
							PhoneContactItemDescriptionList.Codes.Skype,
							PhoneContactItemDescriptionList.Codes.Skype2,
							PhoneContactItemDescriptionList.Codes.Work,
							PhoneContactItemDescriptionList.Codes.Work2,
						};
				}

				return callableDescriptions;
			}
		}
		HashSet<ZString> callableDescriptions;

		#endregion

		#region Lookups

		public new PhoneContactItemLookups Lookups
		{
			get { return (PhoneContactItemLookups)base.Lookups; }
		}

		protected override ContactItemProxyLookups GetNewLookups()
		{
			return new PhoneContactItemLookups(this);
		}

		#endregion

		#region Validation

		protected override ContactItemProxyValidation GetNewValidation()
		{
			return new PhoneContactItemValidation(this);
		}

		#endregion
	}
}
