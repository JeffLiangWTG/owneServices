using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneContactItemPropertyHelper
	{
		public PhoneContactItemPropertyHelper(OrgContact contact)
		{
			this.contact = contact;
		}

		public PhoneContactItemPropertyHelper(PhoneContactItem phoneItem)
		{
			this.phoneItem = phoneItem;
		}

		#region Properties

		readonly OrgContact contact;
		readonly PhoneContactItem phoneItem;

		OrgContact Contact
		{
			get { return contact ?? phoneItem.Contact; }
		}

		#endregion

		#region Phone Numbers

		public ZString GetFormattedFaxAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_FaxMaxLength, value);
		}

		public ZString GetFormattedHomeAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_HomePhoneMaxLength, value);
		}

		public ZString GetFormattedMobileAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_MobileMaxLength, value);
		}

		public ZString GetFormattedOtherAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_OtherPhoneMaxLength, value);
		}

		public ZString GetFormattedPagerAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_PagerMaxLength, value);
		}

		public ZString GetFormattedWorkAddress(ZString value)
		{
			return GetFormattedPhoneNumber(OrgContact.Schema.OC_PhoneMaxLength, value);
		}

		ZString GetFormattedPhoneNumber(int maxLength, ZString value)
		{
			if (Env.Registry.OrgUsePhoneNumberFormatting)
			{
				var contact = Contact;
				var orgClosestPort = contact != null ? contact.OrgClosestPort : null;
				var formattedNum = PhoneNumberFormatter.ConvertToInternationalFormattedPhoneNumber(value, orgClosestPort);
				if (formattedNum.Length > maxLength)
				{
					formattedNum = formattedNum.SubstringSafe(0, maxLength);
				}
				return formattedNum;
			}
			else
			{
				return value;
			}
		}

		#region Phone Number Formatter

		public PhoneNumberFormatAndValidation PhoneNumberFormatter
		{
			get { return phoneNumberValidator ?? (phoneNumberValidator = new PhoneNumberFormatAndValidation()); }
		}
		PhoneNumberFormatAndValidation phoneNumberValidator;

		#endregion

		#endregion

		#region Extension

		public ZString GetFormattedExtensionAddress(ZString value)
		{
			return value;
		}

		#endregion

		#region Skype

		public ZString GetFormattedSkypeAddress(ZString value)
		{
			return value;
		}

		#endregion

		#region Security

		bool IsHomePhone => phoneItem.OI_Description == PhoneContactItemDescriptionList.Codes.Home;
		bool IsMobile => phoneItem.OI_Description == PhoneContactItemDescriptionList.Codes.Mobile || phoneItem.OI_Description == PhoneContactItemDescriptionList.Codes.Mobile2;

		bool IsHomePhoneViewDenied => IsHomePhone && !Env.Security.OrgContactViewHomePhoneNumber.IsAllowed;
		bool IsMobileViewDenied => IsMobile && !Env.Security.OrgContactViewMobileNumber.IsAllowed;

		bool IsHomePhoneModifyDenied => IsHomePhone && !Env.Security.OrgContactModifyHomePhoneNumber.IsAllowed;
		bool IsMobileModifyDenied => IsMobile && !Env.Security.OrgContactModifyMobileNumber.IsAllowed;

		public bool IsViewDenied => IsHomePhoneViewDenied || IsMobileViewDenied;
		public bool IsModifyDenied => IsHomePhoneModifyDenied || IsMobileModifyDenied;

		#endregion
	}
}
