using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PhoneContactItemValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckOI_Description

		public void TestCheckOI_Description_UniqueInCollection()
		{
			var contact = Factory.New<OrgContact>();
			var itemCollection = contact.PhoneContactItems;
			var item1 = itemCollection.AddNew();
			item1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			var item2 = itemCollection.AddNew();
			item2.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			var item3 = itemCollection.AddNew();
			item3.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;

			AssertPropertyIsUniqueInCollectionValidationError(item2.OI_DescriptionInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(item3.OI_DescriptionInfo, false);
		}

		#endregion

		#region CheckOI_Address

		public void TestCheckOI_Address_Formatted_PhoneNumber()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			org.OH_RL_NKClosestPort = "AUSYD";

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = true;

			var phoneItem = contact.PhoneContactItems.AddNew();
			var phoneNumberDescriptions = new[]
			{
				PhoneContactItemDescriptionList.Codes.Mobile,
				PhoneContactItemDescriptionList.Codes.Mobile2,
				PhoneContactItemDescriptionList.Codes.Work,
				PhoneContactItemDescriptionList.Codes.Work2,
				PhoneContactItemDescriptionList.Codes.Home,
				PhoneContactItemDescriptionList.Codes.Fax,
				PhoneContactItemDescriptionList.Codes.Pager,
				PhoneContactItemDescriptionList.Codes.Other
			};

			foreach (var description in phoneNumberDescriptions)
			{
				phoneItem.OI_Description = description;
				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Test CheckOI_Address_Formatted when OI_Description is [{0}]", description), () =>
				{
					phoneItem.OI_Address_Formatted = "0296";
					AssertHasErrors("Incorrect format, error expected", phoneItem.OI_Address_FormattedInfo);
					phoneItem.OI_Address_Formatted = new string('A', 100);
					AssertHasErrors("Too long, error expected.", phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address_Formatted = "0426 829 924";
					AssertNoErrors(phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address_Formatted = "0296654455 SYD";
					AssertHasErrors("Invalid number, error expected", phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address_IsManuallyVerified = true;
					AssertNoErrors("Invalid number but manually verified, no errors expected", phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address = "";
					AssertMandatoryValidationError(phoneItem.OI_Address_FormattedInfo, false);
				});
			}
		}

		public void TestCheckOI_Address_Formatted_PhoneNumber_When_Org_Has_Addresses_From_Different_Countries()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var address1 = org.Addresses.AddNew();
			address1.OA_RN_NKCountryCode = "AU";
			address1.Address1 = "Address 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_RN_NKCountryCode = "CN";
			address2.Address1 = "Address 2";
			var contact = org.Contacts.AddNew();
			contact.OC_OA_OrgAddress = ZGuid.Empty;
			contact.OC_OH_AddressOverride = ZGuid.Empty;
			var phoneItem = contact.PhoneContactItems.AddNew();
			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			Factory.Save();
			Assert(!contact.HasChanges);

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = true;

			phoneItem.OI_Address_Formatted = "0426829924";
			AssertHasErrors("Incorrect format, error expected", phoneItem.OI_Address_FormattedInfo);
			Assert(contact.HasChanges);
			Factory.Save();
			Assert(!contact.HasChanges);
		}

		public void TestCheckOI_Address_Formatted_Skype()
		{
			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = true;
			var skypeDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Skype, PhoneContactItemDescriptionList.Codes.Skype2 };
			foreach (var skypeDescription in skypeDescriptions)
			{
				var contact = Factory.New<OrgContact>();
				var phoneItem = contact.PhoneContactItems.AddNew();
				phoneItem.OI_Description = skypeDescription;

				CombineAssertions(skypeDescription, () =>
				{
					phoneItem.OI_Address_Formatted = "a.n,d-r_ew";
					AssertNoErrors("Should be no errors when valid skype name", phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address_Formatted = "andrew.luong@wisetechglobal.com";
					AssertNoErrors("Should be no errors when valid email address", phoneItem.OI_Address_FormattedInfo);

					phoneItem.OI_Address_Formatted = "andrew,luong@wisetechglobal,com";
					AssertHasError("Should have error when not valid skype name nor email address", phoneItem.OI_Address_FormattedInfo, SkypeIdValidation.InvalidSkypeIdErrorMessage);

					phoneItem.OI_Address_Formatted = "";
					AssertMandatoryValidationError(phoneItem.OI_Address_FormattedInfo, false);
				});
			}
		}

		public void TestCheckOI_Address_Formatted_UniqueInCollection()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			var contact = header.Contacts.AddNew();
			var phoneItemCollection = contact.PhoneContactItems;
			var phoneItem1 = phoneItemCollection.AddNew();
			var phoneItem2 = phoneItemCollection.AddNew();

			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phoneItem1.OI_Address_Formatted = "02 12345678";
			phoneItem2.OI_Address_Formatted = "02 12345678";
			AssertPropertyIsUniqueInCollectionValidationError(phoneItem2.OI_Address_FormattedInfo, true);

			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phoneItem2.OI_Address_Formatted = "02 98765432";
			AssertPropertyIsUniqueInCollectionValidationError(phoneItem2.OI_Address_FormattedInfo, false);

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = true;
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			phoneItem2.OI_Address_Formatted = "02 12345678";
			AssertPropertyIsUniqueInCollectionValidationError(phoneItem2.OI_Address_FormattedInfo, false);
		}

		public void TestCheckOI_Address_Formatted_When_HomePhoneViewDenied()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			org.OH_RL_NKClosestPort = "AUSYD";
			var oldSecuritySetting = Env.Security.OrgContactViewHomePhoneNumber.IsAllowed;
			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = false;
			var phoneItem = contact.PhoneContactItems.AddNew();

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Test CheckOI_Address_Formatted when OI_Description is [{0}] and HomePhoneViewDenied", PhoneContactItemDescriptionList.Codes.Home), () =>
			{
				phoneItem.OI_Address_Formatted = "0296";
				AssertNoErrors("Should not run validation", phoneItem.OI_Address_FormattedInfo);
				phoneItem.OI_Address_Formatted = new string('A', 100);
				AssertNoErrors("Should not run validation", phoneItem.OI_Address_FormattedInfo);
				phoneItem.OI_Address_Formatted = "0426 829 924";
				AssertNoErrors("Should not run validation", phoneItem.OI_Address_FormattedInfo);
				phoneItem.OI_Address_Formatted = "0296654455 SYD";
			});

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Test CheckOI_Address_Formatted when OI_Description is [{0}] and HomePhoneViewDenied", PhoneContactItemDescriptionList.Codes.Home), () =>
			{
				phoneItem.OI_Address_Formatted = "0296";
				AssertHasErrors("Incorrect format, error expected", phoneItem.OI_Address_FormattedInfo);
				phoneItem.OI_Address_Formatted = new string('A', 100);
				AssertHasErrors("Too long, error expected.", phoneItem.OI_Address_FormattedInfo);
				phoneItem.OI_Address_Formatted = "0426 829 924";
				AssertNoErrors(phoneItem.OI_Address_FormattedInfo);
			});

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = oldSecuritySetting;
		}
		#endregion
	}
}
