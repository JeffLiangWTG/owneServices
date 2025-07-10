using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	class DedupePanelTranslationHelperTest : TestCase
	{
		public void TestTranslationDictionaryKeysAndCount_AreCorrect()
		{
			var dictionary = DedupePanelTranslationHelper.translationDictionary;
			var expectedKeys = new List<string>
			{
				"Name",
				"Number",
				"Website",
				"Domain",
				"Email",
				"Similarity",
				"Source",
				"Address",
				"Brand",
				"ContactPhone",
				"AddressPhone",
				"ContactMobile",
				"AddressMobile",
				"Emails",
				"Birthday",
				"Birthdays",
				"OtherPhone",
				"HomePhone",
				"ContactFax",
				"AddressFax",
				"Type",
				"Country",
				"CusCodeCustomsRegNo",
				"Coordinates",
				"Code",
				"Organisations",
				"Addresses",
				"Contacts",
				"RegistrationCodes",
				"Websites",
				"Domains",
				"PhoneNumbers",
				"OrganisationNames",
				"PersonNames",
				"Person",
				"Staff",
				"Applicant",
				"PersonPhone",
				"PersonWorkPhone",
				"PersonMobile",
				"PersonFax",
				"ActiveAssociations",
				"PrimaryWorkplace",
				"UNLOCO",
				"City",
				"State",
				"Active",
				"None",
				"Low",
				"Medium",
				"High",
				"Exact",
				"Undefined",
				"Merge",
				"Add"
			};

			AssertContainsExactElementsInAnyOrder(expectedKeys, dictionary.Keys);
		}
	}
}
