using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ContactPhoneDialInfoBuilderTest : TestCaseWithFactory
	{
		public void TestGetDefaultDialInfo()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";
			var contact = org.Contacts.AddNew();
			var workPhone = contact.PhoneContactItems.AddNew();
			workPhone.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone.OI_Address = "02 12345678";

			var builder = new ContactPhoneDialInfoBuilder();
			var defaultDialInfo = builder.GetDefaultDialInfo(contact, org);
			AssertEquals(workPhone, defaultDialInfo.ContactItem);

			contact.PhoneContactItems.RemoveAndDelete(workPhone);
			defaultDialInfo = builder.GetDefaultDialInfo(contact, org);
			AssertEquals("Office", defaultDialInfo.Description);
			AssertEquals("02 11119999", defaultDialInfo.Number);

			org.MainAddress.OA_Phone = "";
			defaultDialInfo = builder.GetDefaultDialInfo(contact, org);
			AssertNull(defaultDialInfo);
		}

		public void TestGetDefaultDialInfo_WithExtension()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";
			var contact = org.Contacts.AddNew();
			var workPhone = contact.PhoneContactItems.AddNew();
			workPhone.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone.OI_Address = "02 12345678";

			var extension = contact.PhoneContactItems.AddNew();
			extension.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			extension.OI_Address = "112";

			var builder = new ContactPhoneDialInfoBuilder();
			var defaultDialInfo = builder.GetDefaultDialInfo(contact, org);
			AssertEquals(workPhone, defaultDialInfo.ContactItem);
			AssertEquals("Ext. 112", defaultDialInfo.Extension);
		}

		public void TestGetAlternativePhoneDialInfos()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			var workPhone1 = contact.PhoneContactItems.AddNew();
			var workPhone2 = contact.PhoneContactItems.AddNew();
			var homePhone = contact.PhoneContactItems.AddNew();
			workPhone1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone1.OI_Address = "02 12345678";
			workPhone2.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			workPhone2.OI_Address = "02 98765432";
			homePhone.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhone.OI_Address = "02 33336666";

			Factory.Save();

			var builder = new ContactPhoneDialInfoBuilder();
			var actualAlternativePhoneDialInfos = builder.GetAlternativePhoneDialInfos(contact, org);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Home (02 33336666)",
					"Office (02 11119999)",
					"Work (02 12345678)",
					"Work 2 (02 98765432)"
				},
				actualAlternativePhoneDialInfos.Select(info => string.Format("{0} ({1})", info.Description, info.Number)).ToArray());
		}

		public void TestGetAlternativePhoneDialInfos_WithExtension()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 11119999";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			var workPhone1 = contact.PhoneContactItems.AddNew();
			var workPhone2 = contact.PhoneContactItems.AddNew();
			var homePhone = contact.PhoneContactItems.AddNew();
			var extension = contact.PhoneContactItems.AddNew();
			workPhone1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			workPhone1.OI_Address = "02 12345678";

			extension.OI_Description = PhoneContactItemDescriptionList.Codes.Extension;
			extension.OI_Address = "112";

			workPhone2.OI_Description = PhoneContactItemDescriptionList.Codes.Work2;
			workPhone2.OI_Address = "02 98765432";
			homePhone.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			homePhone.OI_Address = "02 33336666";

			Factory.Save();

			var builder = new ContactPhoneDialInfoBuilder();
			var actualAlternativePhoneDialInfos = builder.GetAlternativePhoneDialInfos(contact, org).ToArray();

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Home (02 33336666)",
					"Office (02 11119999)",
					"Work 2 (02 98765432)"
				},
				actualAlternativePhoneDialInfos.Where(info => string.IsNullOrEmpty(info.Extension)).Select(info => $"{info.Description} ({info.Number})").ToArray());

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"Work (02 12345678) Ext. 112",
				},
				actualAlternativePhoneDialInfos.Where(info => !string.IsNullOrEmpty(info.Extension)).Select(info => $"{info.Description} ({info.Number}) {info.Extension}").ToArray());
		}
	}
}
