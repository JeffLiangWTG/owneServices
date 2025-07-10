using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PhoneContactItem))]
	sealed class PhoneContactItemTest : NonPersistentBusinessObjectTestCase
	{
		#region DefaultValues

		public void TestDefaultValues()
		{
			var phoneItem = GetNewPhoneContactItem();
			AssertEquals(OrgContactItemTypes.Codes.Phone, phoneItem.OI_ContactItemType);
		}

		#endregion

		#region OI_Description

		public void TestOI_Description_TruncatesAddressIfTooLong()
		{
			Assert("Precondition", OrgContactItem.Schema.OI_AddressMaxLength > OrgContact.Schema.OC_PhoneMaxLength);
			Assert("Precondition", OrgContact.Schema.OC_PhoneMaxLength > OrgContact.Schema.OC_PhoneExtensionMaxLength);

			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Description = "";
			phoneItem.OI_Address = new ZString('a', OrgContactItem.Schema.OI_AddressMaxLength);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			AssertEquals("Should have been truncated", new ZString('a', OrgContact.Schema.OC_PhoneMaxLength), phoneItem.OI_Address);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			AssertEquals("Should have been truncated", new ZString('a', OrgContact.Schema.OC_PhoneExtensionMaxLength), phoneItem.OI_Address);
		}

		#endregion

		#region OI_Address

		public void TestSettingAddressForOrgInvokesDeduplication()
		{
			//Arrange
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = true;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//Act
				var contact = testOrg.Contacts.AddNew();
				var item = contact.PhoneContactItems.AddNew();
				item.OI_Address = "0414 123 987";

				//Assert
				AssertEquals(true, dedupeStarted);

				contact.PhoneContactItems.RemoveAndDeleteAll();
			}
		}

		public void TestSettingAddressForOrgDoesNotInvokeDeduplication()
		{
			//Arrange
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var dedupeStarted = false;
			testOrg.DeduplicationStarted += (o, e) => { dedupeStarted = true; };
			((IDeduplicatable)testOrg).ShouldRunDeduplication = false;

			//Act
			var contact = testOrg.Contacts.AddNew();
			var item = contact.PhoneContactItems.AddNew();
			item.OI_Address = "0414 123 987";

			//Assert
			AssertEquals(false, dedupeStarted);

			contact.PhoneContactItems.RemoveAndDeleteAll();
		}

		public void TestOI_Address_Mobile()
		{
			var mobileDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Mobile, PhoneContactItemDescriptionList.Codes.Mobile2 };

			foreach (var mobileDescription in mobileDescriptions)
			{
				CombineAssertions(mobileDescription, () =>
				{
					var mobileItem = GetNewPhoneContactItem();
					mobileItem.OI_Description = mobileDescription;

					Env.Registry.SetOrgUsePhoneNumberFormatting(true);
					mobileItem.OI_Address = "0414 123 987";
					AssertEquals("Number formatted correctly", "0414 123 987", mobileItem.OI_Address);

					Env.Registry.SetOrgUsePhoneNumberFormatting(false);
					mobileItem.OI_Address = "0414 123 986";
					AssertEquals("Number formatted correctly", "0414 123 986", mobileItem.OI_Address);

					AssertEquals("MaxLength", OrgContact.Schema.OC_MobileMaxLength, mobileItem.OI_AddressInfo.MaxLength);
					Assert("Max length must be less than or equal OI_AddressMaxLength", mobileItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
				});
			}
		}

		public void TestOI_Address_WorkPhone()
		{
			var workPhoneDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Work, PhoneContactItemDescriptionList.Codes.Work2 };

			foreach (var workPhoneDescription in workPhoneDescriptions)
			{
				CombineAssertions(workPhoneDescription, () =>
				{
					var workPhoneItem = GetNewPhoneContactItem();
					workPhoneItem.OI_Description = workPhoneDescription;

					Env.Registry.SetOrgUsePhoneNumberFormatting(true);
					workPhoneItem.OI_Address = "61 2 9025 1100";
					AssertEquals("Number formatted correctly", "61 2 9025 1100", workPhoneItem.OI_Address);

					Env.Registry.SetOrgUsePhoneNumberFormatting(false);
					workPhoneItem.OI_Address = "61 2 9025 1101";
					AssertEquals("Number formatted correctly", "61 2 9025 1101", workPhoneItem.OI_Address);

					AssertEquals("MaxLength", OrgContact.Schema.OC_PhoneMaxLength, workPhoneItem.OI_AddressInfo.MaxLength);
					Assert("Max length must be less than or equal OI_AddressMaxLength", workPhoneItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
				});
			}
		}

		public void TestOI_Address_HomePhone()
		{
			var homePhoneItem = GetNewPhoneContactItem();
			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			homePhoneItem.OI_Address = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "61 2 9025 1100", homePhoneItem.OI_Address);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			homePhoneItem.OI_Address = "61 2 9025 1101";
			AssertEquals("Number formatted correctly", "61 2 9025 1101", homePhoneItem.OI_Address);

			AssertEquals("MaxLength", OrgContact.Schema.OC_HomePhoneMaxLength, homePhoneItem.OI_AddressInfo.MaxLength);
			Assert("Max length must be less than or equal OI_AddressMaxLength", homePhoneItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
		}

		public void TestOI_Address_Fax()
		{
			var faxItem = GetNewPhoneContactItem();
			faxItem.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			faxItem.OI_Address = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "61 2 9025 1100", faxItem.OI_Address);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			faxItem.OI_Address = "61 2 9025 1101";
			AssertEquals("Number formatted correctly", "61 2 9025 1101", faxItem.OI_Address);

			AssertEquals("MaxLength", OrgContact.Schema.OC_FaxMaxLength, faxItem.OI_AddressInfo.MaxLength);
			Assert("Max length must be less than or equal OI_AddressMaxLength", faxItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
		}

		public void TestOI_Address_Pager()
		{
			var pagerItem = GetNewPhoneContactItem();
			pagerItem.OI_Description = PhoneContactItemDescriptionList.Codes.Pager;

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			pagerItem.OI_Address = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "61 2 9025 1100", pagerItem.OI_Address);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			pagerItem.OI_Address = "61 2 9025 1101";
			AssertEquals("Number formatted correctly", "61 2 9025 1101", pagerItem.OI_Address);

			AssertEquals("MaxLength", OrgContact.Schema.OC_PagerMaxLength, pagerItem.OI_AddressInfo.MaxLength);
			Assert("Max length must be less than or equal OI_AddressMaxLength", pagerItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
		}

		public void TestOI_Address_Other()
		{
			var otherPhoneItem = GetNewPhoneContactItem();
			otherPhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Other;

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			otherPhoneItem.OI_Address = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "61 2 9025 1100", otherPhoneItem.OI_Address);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			otherPhoneItem.OI_Address = "61 2 9025 1101";
			AssertEquals("Number formatted correctly", "61 2 9025 1101", otherPhoneItem.OI_Address);

			AssertEquals("MaxLength", OrgContact.Schema.OC_OtherPhoneMaxLength, otherPhoneItem.OI_AddressInfo.MaxLength);
			Assert("Max length must be less than or equal OI_AddressMaxLength", otherPhoneItem.OI_AddressInfo.MaxLength <= OrgContactItem.Schema.OI_AddressMaxLength);
		}

		public void TestOI_Address_ValueOnContactShouldBeTheSameAsContactItemAfterFormatting()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			var phoneItem = contact.PhoneContactItems.AddNew();

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			foreach (ICodeDescription description in new PhoneContactItemDescriptionList())
			{
				phoneItem.OI_Description = description.Code;
				phoneItem.OI_Address = "55555";

				if (phoneItem.AddressInfoOnOrgContact != null)
				{
					AssertEquals(string.Format("Address on contact for item with description:[{0}]", description.Code), phoneItem.OI_Address, phoneItem.AddressInfoOnOrgContact.Value);
				}
			}
		}

		#endregion

		#region OI_Address_Formatted

		public void TestNotInvokeDeduplicationWhenPhoneContactItemsContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testPerson = Factory.NewWithValidTestData<GlbPerson>();
			((IDeduplicatable)testPerson).ShouldRunDeduplication = true;
			var contact = testPerson.ContactCollection.AddNew();
			contact.OC_PER = testPerson.PK;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				contact.OC_OH = orgHeader.PK;
				var item = contact.PhoneContactItems.AddNew();
				item.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
				var dedupeStarted = false;
				testPerson.DeduplicationStarted += (o, e) => { dedupeStarted = true; };

				item.OI_Address_Formatted = "INVALID";
				AssertEquals(false, dedupeStarted);

				item.OI_Address_Formatted = "+1 201-555-5554";
				AssertEquals(true, dedupeStarted);

				dedupeStarted = false;
				item.OI_Address_Formatted = "+12015555554";
				AssertEquals(false, dedupeStarted);

				contact.PhoneContactItems.RemoveAndDeleteAll();
			}
		}

		public void TestOI_Address_Formatted_Mobile_WithOverridingAddressWithCountryCode()
		{
			var overridingAddressHeader = Factory.NewWithValidTestData<OrgHeader>();
			var overridingAddress = overridingAddressHeader.Addresses.AddNew();
			overridingAddress.FillWithValidTestData();
			overridingAddress.OA_RN_NKCountryCode = "CN";
			var mobileDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Mobile, PhoneContactItemDescriptionList.Codes.Mobile2 };

			foreach (var mobileDescription in mobileDescriptions)
			{
				CombineAssertions(mobileDescription, () =>
				{
					var mobileItem = GetNewPhoneContactItem();
					mobileItem.Contact.OC_OA_OrgAddress = overridingAddress.PK;
					mobileItem.OI_Description = mobileDescription;
					mobileItem.OI_Address_Formatted = "156 01 1 3 19 8 1";
					AssertEquals("Number saved correctly", "+8615601131981", mobileItem.OI_Address);
					AssertEquals("Number formatted correctly", "+86 156 0113 1981", mobileItem.OI_Address_Formatted);
					AssertEquals("The LocalNumberIfLoggedInSameCountry should be blank", string.Empty, mobileItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
				});
			}
		}

		public void TestOI_Address_Formatted_WorkPhone_WithOverridingAddressWithUnloco()
		{
			var overridingAddressHeader = Factory.NewWithValidTestData<OrgHeader>();
			var overridingAddress = overridingAddressHeader.Addresses.AddNew();
			overridingAddress.FillWithValidTestData();
			overridingAddress.OA_RL_NKRelatedPortCode = "AU";
			var workPhoneDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Work, PhoneContactItemDescriptionList.Codes.Work2 };

			foreach (var workPhoneDescription in workPhoneDescriptions)
			{
				CombineAssertions(workPhoneDescription, () =>
				{
					var workPhoneItem = GetNewPhoneContactItem();
					workPhoneItem.Contact.OC_OA_OrgAddress = overridingAddress.PK;
					workPhoneItem.OI_Description = workPhoneDescription;
					workPhoneItem.OI_Address_Formatted = "61 2 9025 1100";
					AssertEquals("Number formatted correctly", "+61290251100", workPhoneItem.OI_Address);
					AssertEquals("Number formatted correctly", "+61 2 9025 1100", workPhoneItem.OI_Address_Formatted);
					AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", "(02) 9025 1100", workPhoneItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
				});
			}
		}

		public void TestOI_Address_Formatted_HomePhone_WithOverridingHeaderWithAddressesFromTheSameCountry()
		{
			var homePhoneItem = GetNewPhoneContactItem();
			var overridingHeader = Factory.NewWithValidTestData<OrgHeader>();
			overridingHeader.MainAddress.OA_RN_NKCountryCode = "CN";
			homePhoneItem.Contact.OC_OH_AddressOverride = overridingHeader.PK;

			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhoneItem.OI_Address_Formatted = "010 6528 1649";
			AssertEquals("Number formatted correctly", "+861065281649", homePhoneItem.OI_Address);
			AssertEquals("Number formatted correctly", "+86 10 6528 1649", homePhoneItem.OI_Address_Formatted);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be blank", string.Empty, homePhoneItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOI_Address_Formatted_Fax_WithHeaderWithAddressesFromTheSameCountry()
		{
			var faxItem = GetNewPhoneContactItem();
			faxItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";

			faxItem.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			faxItem.OI_Address_Formatted = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "+61290251100", faxItem.OI_Address);
			AssertEquals("Number formatted correctly", "+61 2 9025 1100", faxItem.OI_Address_Formatted);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", "(02) 9025 1100", faxItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOI_Address_Formatted_Pager()
		{
			var pagerItem = GetNewPhoneContactItem();

			pagerItem.OI_Description = PhoneContactItemDescriptionList.Codes.Pager;
			pagerItem.OI_Address_Formatted = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "+61290251100", pagerItem.OI_Address);
			AssertEquals("Number formatted correctly", "+61 2 9025 1100", pagerItem.OI_Address_Formatted);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", "(02) 9025 1100", pagerItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public void TestOI_Address_Formatted_Other()
		{
			var otherPhoneItem = GetNewPhoneContactItem();

			otherPhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Other;
			otherPhoneItem.OI_Address_Formatted = "61 2 9025 1100";
			AssertEquals("Number formatted correctly", "+61290251100", otherPhoneItem.OI_Address);
			AssertEquals("Number formatted correctly", "+61 2 9025 1100", otherPhoneItem.OI_Address_Formatted);
			AssertEquals("The LocalNumberIfLoggedInSameCountry should be shown correctly", "(02) 9025 1100", otherPhoneItem.OI_Address_FormattedLocalNumberIfLoggedInSameCountry);
		}

		#endregion

		#region Format Phone Number

		public void TestFormatPhoneNumber()
		{
			var mobileDescriptions = new[] { PhoneContactItemDescriptionList.Codes.Mobile, PhoneContactItemDescriptionList.Codes.Mobile2 };

			foreach (var mobileDescription in mobileDescriptions)
			{
				CombineAssertions(mobileDescription, () =>
				{
					var mobileItem = GetNewPhoneContactItem();
					mobileItem.OI_Description = mobileDescription;

					Env.Registry.SetOrgUsePhoneNumberFormatting(true);
					mobileItem.OI_Address = "0414 123 987";
					mobileItem.FormatPhoneNumber();
					AssertEquals("Number formatted correctly", "+61 (414) 123-987", mobileItem.OI_Address);

					Env.Registry.SetOrgUsePhoneNumberFormatting(false);
					mobileItem.OI_Address = "0414 123 986";
					mobileItem.FormatPhoneNumber();
					AssertEquals("Number formatted correctly", "0414 123 986", mobileItem.OI_Address);
				});
			}
		}

		#endregion

		#region Phone IsManuallyVerified

		public void TestOI_Address_IsManuallyVerified()
		{
			Func<PhoneContactItem> getPhoneWithNumber = () =>
			{
				var phoneItem = GetNewPhoneContactItem();
				phoneItem.OI_Address = "+61 425 465 800";
				return phoneItem;
			};

			PhoneContactItem phoneItem1;
			PhoneContactItem phoneItem2;

			foreach (var keyValuePair in PhoneContactItemLookups.GetOrgContactPhoneIsManuallyVerifiedColumnsByDescription(Factory))
			{
				var phoneColumn = PhoneContactItemLookups.GetOrgContactColumnsByDescription(Factory)[keyValuePair.Key].Name;
				phoneItem1 = getPhoneWithNumber();
				phoneItem2 = getPhoneWithNumber();
				phoneItem1.OI_Description = keyValuePair.Key;
				phoneItem2.OI_Description = keyValuePair.Key;
				PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, keyValuePair.Value, OrgContactSchema.Constants.Prefix, phoneColumn, phoneItem1.Contact, phoneItem2.Contact);
			}

			phoneItem1 = getPhoneWithNumber();
			phoneItem2 = getPhoneWithNumber();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContactItem.Schema.OI_Address_IsManuallyVerified, OrgContactItemSchema.Constants.Prefix, OrgContactItemSchema.Constants.OI_Address, phoneItem1.OrgContactItem, phoneItem2.OrgContactItem);

			phoneItem1 = getPhoneWithNumber();
			phoneItem2 = getPhoneWithNumber();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			phoneItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContactItem.Schema.OI_Address_IsManuallyVerified, OrgContactItemSchema.Constants.Prefix, OrgContactItemSchema.Constants.OI_Address, phoneItem1.OrgContactItem, phoneItem2.OrgContactItem);

			phoneItem1 = getPhoneWithNumber();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			phoneItem1.OI_Address_IsManuallyVerified = true;
			Assert("Non-formattable fields can't be manually verified", !phoneItem1.OI_Address_IsManuallyVerified);

			phoneItem1 = getPhoneWithNumber();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Skype;
			phoneItem1.OI_Address_IsManuallyVerified = true;
			Assert("Non-formattable fields can't be manually verified", !phoneItem1.OI_Address_IsManuallyVerified);

			phoneItem1 = getPhoneWithNumber();
			phoneItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Skype2;
			phoneItem1.OI_Address_IsManuallyVerified = true;
			Assert("Non-formattable fields can't be manually verified", !phoneItem1.OI_Address_IsManuallyVerified);
		}

		public void TestSettingPhoneNumbersResetsIsManuallyVerifiedFlag()
		{
			Func<PhoneContactItem> getPhoneWithNumber = () =>
			{
				var phoneContactItem = GetNewPhoneContactItem();
				phoneContactItem.OI_Address = "+61 425 465 800";
				return phoneContactItem;
			};

			PhoneContactItem phoneItem;
			GenCustomAddOnRuleAckCollection acks;

			foreach (var keyValuePair in PhoneContactItemLookups.GetOrgContactPhoneIsManuallyVerifiedColumnsByDescription(Factory))
			{
				var phoneColumn = PhoneContactItemLookups.GetOrgContactColumnsByDescription(Factory)[keyValuePair.Key].Name;
				phoneItem = getPhoneWithNumber();
				phoneItem.OI_Description = keyValuePair.Key;
				phoneItem.OI_Address_IsManuallyVerified = true;
				Assert("Precondition", phoneItem.OI_Address_IsManuallyVerified);
				acks = new GenCustomAddOnRuleAckCollection(phoneItem.Contact);
				AssertEquals("Precondition", 1, acks.Count);
				phoneItem.OI_Address_Formatted = "+61 425 465 801";
				Assert(!phoneItem.OI_Address_IsManuallyVerified);
				AssertEquals(0, acks.Count);

				phoneItem.OI_Address_Formatted = "+61 425 465 80";
				phoneItem.OI_Address_IsManuallyVerified = true;
				AssertEquals(1, acks.Count);
				phoneItem.OI_Address_Formatted = string.Empty;
				Assert(!phoneItem.OI_Address_IsManuallyVerified);
				AssertEquals(0, acks.Count);
			}

			phoneItem = getPhoneWithNumber();
			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			phoneItem.OI_Address_IsManuallyVerified = true;
			acks = new GenCustomAddOnRuleAckCollection(phoneItem.OrgContactItem);
			Assert("Precondition", phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals("Precondition", 1, acks.Count);
			phoneItem.OI_Address_Formatted = "+61 425 465 801";
			Assert(!phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals(0, acks.Count);
			phoneItem.OI_Address_IsManuallyVerified = true;
			AssertEquals(1, acks.Count);
			phoneItem.OI_Address_Formatted = string.Empty;
			Assert(!phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals(0, acks.Count);

			phoneItem = getPhoneWithNumber();
			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			phoneItem.OI_Address_IsManuallyVerified = true;
			acks = new GenCustomAddOnRuleAckCollection(phoneItem.OrgContactItem);
			Assert("Precondition", phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals("Precondition", 1, acks.Count);
			phoneItem.OI_Address_Formatted = "+61 425 465 801";
			Assert(!phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals(0, acks.Count);
			phoneItem.OI_Address_IsManuallyVerified = true;
			AssertEquals(1, acks.Count);
			phoneItem.OI_Address_Formatted = string.Empty;
			Assert(!phoneItem.OI_Address_IsManuallyVerified);
			AssertEquals(0, acks.Count);
		}

		public void TestIsManuallyVerifiedFlagIsPreservedWhenChangingDescription()
		{
			Func<GenCustomAddOnRuleAck> getSingleAckWithAssert = () =>
			{
				var acks = Factory.Load<GenCustomAddOnRuleAck>(new ZQuery(GenCustomAddOnRuleAckSchema.XK_RuleID, Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation));
				AssertEquals(1, acks.Length);
				return acks[0];
			};

			var phoneContactItem = GetNewPhoneContactItem();
			phoneContactItem.OI_Address = "+61 425 465 800";

			phoneContactItem.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			phoneContactItem.OI_Address_IsManuallyVerified = true;
			Assert("Precondition", phoneContactItem.OI_Address_IsManuallyVerified);
			var ack = getSingleAckWithAssert();
			AssertEquals(OrgContactSchema.Constants.Prefix, ack.XK_ParentTableCode);
			AssertContains(OrgContactSchema.Constants.OC_Fax, ack.XK_Warning);

			phoneContactItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			ack = getSingleAckWithAssert();
			AssertEquals(OrgContactItemSchema.Constants.Prefix, ack.XK_ParentTableCode);
			AssertContains(OrgContactItemSchema.Constants.OI_Address, ack.XK_Warning);

			phoneContactItem.OI_Description = PhoneContactItemDescriptionList.Codes.Pager;
			ack = getSingleAckWithAssert();
			AssertEquals(OrgContactSchema.Constants.Prefix, ack.XK_ParentTableCode);
			AssertContains(OrgContactSchema.Constants.OC_Pager, ack.XK_Warning);
		}
		#endregion

		#region IsCallable

		public void TestIsCallable()
		{
			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Description = "";
			AssertEquals(false, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			AssertEquals(false, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			AssertEquals(false, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Other;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Pager;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Skype;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Skype2;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			AssertEquals(true, phoneItem.IsCallable);

			phoneItem.OI_Description = "XXX";
			AssertEquals(false, phoneItem.IsCallable);
		}

		#endregion

		#region Phone Number

		public void TestPhoneNumber()
		{
			var homePhoneItem = GetNewPhoneContactItem();
			homePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhoneItem.OI_Address_Formatted = "61 2 9025 1100";
			AssertEquals("+61 2 9025 1100", homePhoneItem.Number.FormattedForBinding);
			AssertEquals("AU shows local number", "(02) 9025 1100", homePhoneItem.Number.FormattedLocalNumberIfLoggedInSameCountryForBinding);

			var workPhoneItem = GetNewPhoneContactItem();
			workPhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "CN";
			workPhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhoneItem.OI_Address_Formatted = "61 2 9025 1101";
			AssertEquals("CN shows blank number to avoid redundancy", string.Empty, workPhoneItem.Number.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		public void TestMobileViewDenied()
		{
			ZString viewDeniedMessage = "** View Denied **";

			var homePhoneItem = GetNewPhoneContactItem();
			homePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhoneItem.OI_Address_Formatted = "61 2 9025 1100";

			var mobilePhoneItem = GetNewPhoneContactItem();
			mobilePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			mobilePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			mobilePhoneItem.OI_Address_Formatted = "+61 2 9025 2211";

			var oldViewPermission = Env.Security.OrgContactViewMobileNumber.IsAllowed;

			Env.Security.OrgContactViewMobileNumber.IsAllowed = false;

			AssertEquals(mobilePhoneItem.OI_Address_Formatted, viewDeniedMessage);
			AssertEquals(homePhoneItem.OI_Address_Formatted, "+61 2 9025 1100");

			Env.Security.OrgContactViewMobileNumber.IsAllowed = oldViewPermission;
		}

		public void TestMobileViewDenied_ForMobile2()
		{
			ZString viewDeniedMessage = "** View Denied **";

			var homePhoneItem = GetNewPhoneContactItem();
			homePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhoneItem.OI_Address_Formatted = "61 2 9025 1100";

			var mobilePhoneItem = GetNewPhoneContactItem();
			mobilePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			mobilePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile2;
			mobilePhoneItem.OI_Address_Formatted = "+61 2 9025 2211";

			var oldViewPermission = Env.Security.OrgContactViewMobileNumber.IsAllowed;

			Env.Security.OrgContactViewMobileNumber.IsAllowed = false;

			AssertEquals(mobilePhoneItem.OI_Address_Formatted, viewDeniedMessage);
			AssertEquals(homePhoneItem.OI_Address_Formatted, "+61 2 9025 1100");

			Env.Security.OrgContactViewMobileNumber.IsAllowed = oldViewPermission;
		}

		public void TestHomePhoneViewDenied()
		{
			ZString viewDeniedMessage = "** View Denied **";

			var homePhoneItem = GetNewPhoneContactItem();
			homePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			homePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhoneItem.OI_Address_Formatted = "61 2 9025 1100";

			var mobilePhoneItem = GetNewPhoneContactItem();
			mobilePhoneItem.Contact.Header.MainAddress.OA_RN_NKCountryCode = "AU";
			mobilePhoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			var mobileNumber = "+61 2 9025 2211";
			mobilePhoneItem.OI_Address_Formatted = mobileNumber;

			var oldViewPermission = Env.Security.OrgContactViewHomePhoneNumber.IsAllowed;

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = false;

			AssertEquals(homePhoneItem.OI_Address_Formatted, viewDeniedMessage);
			AssertEquals(mobilePhoneItem.OI_Address_Formatted, mobileNumber);

			Env.Security.OrgContactViewHomePhoneNumber.IsAllowed = oldViewPermission;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var phoneItem = GetNewPhoneContactItem();
			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			phoneItem.OI_Address_Formatted = "61 2 9025 1101";
			return phoneItem;
		}

		PhoneContactItem GetNewPhoneContactItem()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			var phoneContactItem = new PhoneContactItem(contact);
			return phoneContactItem;
		}

		#endregion
	}
}
