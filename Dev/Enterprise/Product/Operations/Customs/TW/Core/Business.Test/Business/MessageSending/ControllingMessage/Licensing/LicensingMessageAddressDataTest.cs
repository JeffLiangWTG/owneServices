using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageAddressData))]
	sealed class LicensingMessageAddressDataTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			address.CompanyName = "Company Name";
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			address.OA_AdditionalAddressInformation = "Address3";
			address.OA_City = "TAIPEI";
			address.State = "State";
			address.Postcode = "1234";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var localAddress = address.TranslatedAddresses.AddNew();
			localAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			localAddress.OTA_CompanyName = "綠晃科技股份有限公司";
			localAddress.Postcode = "5678";
			localAddress.CountryCodeISO2 = Core.Constants.CountryCodes.Taiwan;
			localAddress.City = "中壢市";
			localAddress.Address1 = "遠東路88號";
			localAddress.Address2 = "地址2";
			localAddress.OTA_AdditionalAddressInformation = "地址3";

			var supplierDocAddress = declaration.SupplierDocumentaryAddress;
			supplierDocAddress.OrganisationPK = orgHeader.PK;
			supplierDocAddress.E2_OA_Address = address.PK;

			CombineAssertions(() =>
			{
				var addressData = new LicensingMessageAddressData(supplierDocAddress, false, SharedHelper.GetEnglishLanguageCodes());
				NUnit.Framework.Assert.That(addressData.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("ADDRESS1 ADDRESS2 TAIPEI STATE 1234 TAIWAN").Using(CustomComparers.TypeComparison), "Address Line");

				addressData = new LicensingMessageAddressData(supplierDocAddress, false, Core.SharedConstants.Languages.ChineseTraditional);
				NUnit.Framework.Assert.That(addressData.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("5678台灣中壢市遠東路88號地址2").Using(CustomComparers.TypeComparison), "Address ChineseLine");

				supplierDocAddress.E2_AddressOverride = true;
				supplierDocAddress.LocalAddress.Postcode = "9012";
				supplierDocAddress.LocalAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				supplierDocAddress.LocalAddress.City = "中壢市";
				supplierDocAddress.LocalAddress.Address1 = "遠東路88號";
				supplierDocAddress.LocalAddress.Address2 = "本地地址2";
				supplierDocAddress.LocalAddress.AdditionalAddressInformation = "本地地址3";

				addressData = new LicensingMessageAddressData(supplierDocAddress, true, SharedHelper.GetEnglishLanguageCodes());
				NUnit.Framework.Assert.That(addressData.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("ADDRESS1 ADDRESS2 ADDRESS3 TAIPEI STATE 1234 TAIWAN").Using(CustomComparers.TypeComparison), "(Overrided and TW1_CertificateType is 15) Address Line");

				addressData = new LicensingMessageAddressData(supplierDocAddress.LocalAddress, true, Core.SharedConstants.Languages.ChineseTraditional);
				NUnit.Framework.Assert.That(addressData.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("中壢市遠東路88號本地地址2本地地址3").Using(CustomComparers.TypeComparison), "(Overrided and TW1_CertificateType is 15) Address ChineseLine");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
