using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailContactItemValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckOI_Description

		public void TestCheckOI_Description_UniqueInCollection()
		{
			var contact = Factory.New<OrgContact>();
			var emailItemCollection = contact.EmailContactItems;
			var emailItem1 = emailItemCollection.AddNew();
			emailItem1.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			var emailItem2 = emailItemCollection.AddNew();
			emailItem2.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			var emailItem3 = emailItemCollection.AddNew();
			emailItem3.OI_Description = EmailContactItemDescriptionList.Codes.Other;

			AssertPropertyIsUniqueInCollectionValidationError(emailItem2.OI_DescriptionInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(emailItem3.OI_DescriptionInfo, false);
		}

		#endregion

		#region CheckOI_Address

		public void TestCheckOI_Address()
		{
			var contact = Factory.New<OrgContact>();
			var emailItem = contact.EmailContactItems.AddNew();
			CombineAssertions(() =>
			{
				emailItem.OI_Address = "test invalid email";
				AssertHasErrorContaining(emailItem.OI_AddressInfo, "Email Address is not valid");

				emailItem.OI_Address = "test@test.com";
				AssertNoErrors(emailItem.OI_AddressInfo);

				emailItem.OI_Address = "";
				AssertMandatoryValidationError(emailItem.OI_AddressInfo, false);

				contact.OC_WebAccessEnabled = true;
				AssertHasError(emailItem.OI_AddressInfo, "Email is required when web access is enabled.");

				emailItem.OI_Address = "unit.test@cargowise.com";
				contact.OC_WebAccessEnabled = false;
				AssertNoErrors(emailItem.OI_AddressInfo);
			});
		}

		public void TestCheckOI_Address_WhenNotifyModeIsEML()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.New<OrgContact>();
			var emailItem = contact.EmailContactItems.AddNew();

			CombineAssertions(() =>
			{
				contact.OC_OH = header.PK;
				contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
				emailItem.OI_Address = "unit.test@cargowise.com";

				AssertEquals("There should be no errors", false, emailItem.OI_AddressInfo.HasError("The contacts email address cannot be empty when the Default Delivery Method is set to EML"));

				contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Fax;
				AssertEquals("There should be no errors", false, emailItem.OI_AddressInfo.HasError("The contacts email address cannot be empty when the Default Delivery Method is set to EML"));

				contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
				emailItem.OI_Address = "";

				AssertHasErrorContaining(emailItem.OI_AddressInfo, "The contacts email address cannot be empty");
			});
		}

		public void TestCheckOI_Address_UniqueInCollection()
		{
			var contact = Factory.New<OrgContact>();
			var emailItemCollection = contact.EmailContactItems;
			var emailItem1 = emailItemCollection.AddNew();
			emailItem1.OI_Address = "my address";
			var emailItem2 = emailItemCollection.AddNew();
			emailItem2.OI_Address = "my address";
			var emailItem3 = emailItemCollection.AddNew();
			emailItem3.OI_Address = "my address 2";

			AssertPropertyIsUniqueInCollectionValidationError(emailItem2.OI_AddressInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(emailItem3.OI_AddressInfo, false);
		}

		#endregion
	}
}
