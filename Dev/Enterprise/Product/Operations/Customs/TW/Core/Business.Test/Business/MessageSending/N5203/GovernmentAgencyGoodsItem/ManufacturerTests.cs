using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ManufacturerTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo("99966665").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo("X21XXXX3344").Using(CustomComparers.TypeComparison));
			var manufacturerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerOrgOrgAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerOrgOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.OA_IsActive = true;
			manufacturerOrgOrgAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			manufacturerOrgOrgAddress.OA_CompanyNameOverride = "公司名稱";
			var englishAddress = manufacturerOrgOrgAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			englishAddress.Address1 = "test1";
			englishAddress.Address2 = "test2";
			englishAddress.OTA_CompanyName = "ota company name";
			var twJobDocAddress = Factory.NewWithValidTestData<TWJobDocAddress>();
			twJobDocAddress.E2_OA_Address = manufacturerOrgOrgAddress.PK;
			var testManufacturer = new GovernmentAgencyGoodsItemManufacturer(twJobDocAddress);
			NUnit.Framework.Assert.That(testManufacturer.Name, NUnit.Framework.Is.EqualTo("ota company name").Using(CustomComparers.TypeComparison));

			twJobDocAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(testManufacturer.Name, NUnit.Framework.Is.EqualTo("公司名稱").Using(CustomComparers.TypeComparison));

			twJobDocAddress.CompanyName = "Override company name";
			NUnit.Framework.Assert.That(testManufacturer.Name, NUnit.Framework.Is.EqualTo("Override company name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(manufacturer.ChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(manufacturer.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(manufacturer.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(manufacturer.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(manufacturer.RoleCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(manufacturer.SubBoxID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(manufacturer.Address, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAddress)));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(manufacturer.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(manufacturer.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(manufacturer.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestMainManufacturer()
		{
			NUnit.Framework.Assert.That(manufacturer.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestUndertakeCode()
		{
			NUnit.Framework.Assert.That(manufacturer.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var address = Factory.NewWithValidTestData<TWJobDocAddress>();
			address.E2_OA_Address = new TestTWCreator(Factory).CreateOrganizationForManufacturer().MainAddress.PK;
			manufacturer = new GovernmentAgencyGoodsItemManufacturer(address);
		}

		IPartyDetails manufacturer;
	}
}
