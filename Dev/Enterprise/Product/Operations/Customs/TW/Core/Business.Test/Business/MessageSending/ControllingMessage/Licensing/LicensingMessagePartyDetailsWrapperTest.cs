using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessagePartyDetailsWrapper))]
	sealed class LicensingMessagePartyDetailsWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.ContactsActive.AddNew();
			contact.OC_ContactName = "Anthony";
			var address = orgHeader.Addresses.AddNew();
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
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

			var supplierDocAddress = controllingMessageHeader.SupplierDocumentaryAddress;
			supplierDocAddress.OrganisationPK = orgHeader.PK;
			supplierDocAddress.E2_OA_Address = address.PK;
			supplierDocAddress.ContactPK = contact.PK;

			CombineAssertions(() =>
			{
				var partyDetails = new LicensingMessagePartyDetailsWrapper(supplierDocAddress, false) as IPartyDetails;
				NUnit.Framework.Assert.That(partyDetails.ID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(partyDetails.Name, NUnit.Framework.Is.EqualTo("Company Name").Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(partyDetails.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(partyDetails.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._58).Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(partyDetails.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CustomsControlID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.PaymentOnAccountBusinessID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PaymentOnAccountBusinessID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "RoleCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.SubBoxID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "SubBoxID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)), "LPCOAuthorizedParty - should be [null]");
				NUnit.Framework.Assert.That(partyDetails.ContactName, NUnit.Framework.Is.EqualTo("Anthony").Using(CustomComparers.TypeComparison), "ContactName");
				NUnit.Framework.Assert.That(partyDetails.OwnerName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "OwnerName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MainManufacturer - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UndertakeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(partyDetails.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalInformation>)), "AdditionalInformations - should be [null]");

				var address = partyDetails.Address;
				NUnit.Framework.Assert.That(address.Line, NUnit.Framework.Is.EqualTo("ADDRESS1 ADDRESS2 TAIPEI STATE 1234 TAIWAN").Using(CustomComparers.TypeComparison), "Address Line");
				NUnit.Framework.Assert.That(address.ChineseLine, NUnit.Framework.Is.EqualTo("5678台灣中壢市遠東路88號地址2").Using(CustomComparers.TypeComparison), "Address ChineseLine");

				supplierDocAddress.E2_AddressOverride = true;
				supplierDocAddress.LocalAddress.Postcode = "9012";
				supplierDocAddress.LocalAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				supplierDocAddress.LocalAddress.City = "中壢市";
				supplierDocAddress.LocalAddress.Address1 = "遠東路88號";
				supplierDocAddress.LocalAddress.Address2 = "本地地址2";
				supplierDocAddress.LocalAddress.AdditionalAddressInformation = "本地地址3";

				partyDetails = new LicensingMessagePartyDetailsWrapper(supplierDocAddress, true);
				address = partyDetails.Address;
				NUnit.Framework.Assert.That(address.Line, NUnit.Framework.Is.EqualTo("ADDRESS1 ADDRESS2 ADDRESS3 TAIPEI STATE 1234 TAIWAN").Using(CustomComparers.TypeComparison), "(Overrided and TW1_CertificateType is 15) Address Line");
				NUnit.Framework.Assert.That(address.ChineseLine, NUnit.Framework.Is.EqualTo("中壢市遠東路88號本地地址2本地地址3").Using(CustomComparers.TypeComparison), "(Overrided and TW1_CertificateType is 15) Address ChineseLine");
			});
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			IPartyDetails exporterWrapper = new LicensingMessagePartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(exporterWrapper.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._53).Using(CustomComparers.TypeComparison));

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
			exporterWrapper = new LicensingMessagePartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(exporterWrapper.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._174).Using(CustomComparers.TypeComparison));

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
			exporterWrapper = new LicensingMessagePartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(exporterWrapper.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._58).Using(CustomComparers.TypeComparison));

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.IDCodeType = "XXX";
			exporterWrapper = new LicensingMessagePartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(exporterWrapper.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			var controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			var jobDocAddress = controllingMessageHeader.ImporterDocumentaryAddress;
			jobDocAddress.OrganisationPK = org.PK;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_Fax = "021111";
			jobDocAddress.E2_Phone = "035855";
			jobDocAddress.E2_Email = "xxx@ppp.com";
			IPartyDetails importerWrapper = new LicensingMessagePartyDetailsWrapper(jobDocAddress, false);
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "FX" && x.ID == "021111"), NUnit.Framework.Is.True, "Fax");
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "MA" && x.ID == "xxx@ppp.com"), NUnit.Framework.Is.True, "Email");
			NUnit.Framework.Assert.That(importerWrapper.Communications.Any(x => x.TypeID == "TE" && x.ID == "035855"), NUnit.Framework.Is.True, "Phone");
		}

		protected override void SetUp()
		{
			base.SetUp();
			controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
		}

		CusTWControllingMessageHeader controllingMessageHeader;
	}
}
