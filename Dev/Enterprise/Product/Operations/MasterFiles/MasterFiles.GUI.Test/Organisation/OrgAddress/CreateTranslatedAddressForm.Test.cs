using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(CreateTranslatedAddressForm))]
	public class CreateTranslatedAddressFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_OA = address.PK;
			return new CreateTranslatedAddressForm(new AddressMapper(address, translatedAddress), false);
		}

		public void TestSwitchOperation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "BLAH";
			org.OH_Code = "XXXXXX";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_Language = Core.SharedConstants.Languages.French;
			org.OH_IsSalesLead = true;

			var mainAddress = org.MainAddress;
			mainAddress.OA_Language = Core.SharedConstants.Languages.French;
			mainAddress.OA_Address1 = "Nîmes";
			mainAddress.OA_City = "Nîmes";
			mainAddress.OA_PostCode = "12345";

			var translatedAddress = org.CreateEnglishEquivalentAddress(mainAddress);
			var addressMapper = new AddressMapper(mainAddress, translatedAddress);
			using (var createTranslatedAddressForm = new CreateTranslatedAddressForm(addressMapper, false))
			{
				createTranslatedAddressForm.Show();
				AssertEquals("The Address1's Language should be French.", Core.SharedConstants.Languages.French, addressMapper.Address1.Language);
				AssertEquals("The Address1's Address1 should be Nîmes.", "Nîmes", addressMapper.Address1.Address1);
				AssertEquals("The Address1's City should be Nîmes.", "Nîmes", addressMapper.Address1.City);
				AssertEquals("The Address1's Postcode should be 12345.", "12345", addressMapper.Address1.Postcode);

				AssertEquals($"The Address2's Language should be{translatedAddress.OTA_Language}.", translatedAddress.OTA_Language, addressMapper.Address2.Language);
				AssertEquals($"The Address2's Address1 should be {translatedAddress.OTA_Address1}.", translatedAddress.OTA_Address1, addressMapper.Address2.Address1);
				AssertEquals($"The Address2's City should be {translatedAddress.OTA_City}.", translatedAddress.OTA_City, addressMapper.Address2.City);
				AssertEquals($"TThe Address2's Postcode should be {translatedAddress.OTA_PostCode}.", translatedAddress.OTA_PostCode, addressMapper.Address2.Postcode);
			}
		}
	}
}
