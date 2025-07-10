using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PersonIdentityInformationData))]
	class PersonIdentityInformationDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLineNumber()
		{
			var personIdentify1 = MessageData.PIIs.AddNew();
			AssertEquals(1, personIdentify1.US_LineNo);
			var personIdentify2 = MessageData.PIIs.AddNew();
			AssertEquals(2, personIdentify2.US_LineNo);
			var personIdentify3 = MessageData.PIIs.AddNew();
			AssertEquals(3, personIdentify3.US_LineNo);
			MessageData.PIIs.RemoveAndDelete(personIdentify2);
			AssertEquals(1, personIdentify1.US_LineNo);
			AssertEquals(2, personIdentify3.US_LineNo);
			var personIdentify4 = MessageData.PIIs.AddNew();
			AssertEquals(3, personIdentify4.US_LineNo);
		}

		public void TestDefaultsFromContact()
		{
			var contact = MessageData.wrapper.organisation.Contacts.AddNew();
			contact.OC_ContactName = "IANaaaaaaabbbbbbbbbbccccccccccddddddddddeeeee THEaaaaaaabbbbbbbbbb BUILDERaaabbbbbbbbbbccccccccccddddddddddeeeee";
			contact.OC_Email = "INC@ABC.COM";
			contact.OC_Title = "PRESIDENT";
			contact.OC_Phone = "+61 (2) 1234 5678";
			contact.OC_Fax = "+61 (2) 8765 4321";
			contact.OC_PhoneExtension = "123";

			var passport = contact.Certificates.AddNew();
			passport.XZ_Type = CertificateTypePairList.Codes.PA1;
			passport.XZ_RefNumber = "E019274821";
			passport.XZ_ExpiryOrDueDate = ZDateTime.BrettsBirthday;
			passport.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.UnitedStates;

			var personIdentify = MessageData.PIIs.AddNew();
			personIdentify.US_OC_Contact = contact.PK;
			AssertEquals("BUILDERaaabbbbbbbbbbccccccccccdddddddddd, IANaaaaaaabbbbbbbbbbccccccccccdddddddddd, T", personIdentify.US_Name);
			AssertEquals("INC@ABC.COM", personIdentify.US_Email);
			AssertEquals("PRESIDENT", personIdentify.US_Title);
			AssertEquals("123", personIdentify.US_Extension);
			AssertEquals("E019274821", personIdentify.US_PassportNo);
			AssertEquals(ZDateTime.BrettsBirthday, personIdentify.US_ExpirationDate);
			AssertEquals("US", personIdentify.US_CountryOfIssuance);
			AssertEquals(ZString.Empty, personIdentify.US_PassportType);
			AssertEquals(ZString.Empty, personIdentify.US_SSN);

			contact = MessageData.wrapper.organisation.Contacts.AddNew();
			contact.OC_ContactName = "BOBaaaaaaabbbbbbbbbbccccccccccddddddddddeeeee THEaaaaaaabbbbbbbbbb BUILDERaaabbbbbbbbbbccccccccccddddddddddeeeee";
			contact.OC_Email = "BOB@TEST.COM";
			contact.OC_Title = "MANAGER";

			personIdentify.US_PassportType = PassportTypesList.Codes._01;
			personIdentify.US_SSN = "1234";
			personIdentify.US_OC_Contact = contact.PK;
			AssertEquals("BUILDERaaabbbbbbbbbbccccccccccdddddddddd, BOBaaaaaaabbbbbbbbbbccccccccccdddddddddd, T", personIdentify.US_Name);
			AssertEquals("BOB@TEST.COM", personIdentify.US_Email);
			AssertEquals("MANAGER", personIdentify.US_Title);
			AssertEquals(ZString.Empty, personIdentify.US_Extension);
			AssertEquals(ZString.Empty, personIdentify.US_PassportNo);
			AssertEquals(ZDateTime.Empty, personIdentify.US_ExpirationDate);
			AssertEquals(ZString.Empty, personIdentify.US_CountryOfIssuance);
			AssertEquals(ZString.Empty, personIdentify.US_PassportType);
			AssertEquals(ZString.Empty, personIdentify.US_SSN);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PersonIdentityInformationData(Factory, MessageData);
		}

		OrgAddressMessageData MessageData
		{
			get
			{
				if (messageData == null)
				{
					var organization = Factory.New<OrgHeader>();
					var wrapper = OrgHeaderWrapper.New(organization);
					messageData = new OrgAddressMessageData(wrapper);
				}

				return messageData;
			}
		}
		OrgAddressMessageData messageData;

		#endregion
	}
}
