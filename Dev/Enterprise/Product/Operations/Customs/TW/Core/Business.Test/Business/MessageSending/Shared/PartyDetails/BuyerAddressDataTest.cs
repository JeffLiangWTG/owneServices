using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BuyerAddressDataTest : AddressDataAbstractTest<BuyerAddressData>
	{
		[ExpectNoExceptions]
		public override void TestEnglishAddressFormat()
		{
			NUnit.Framework.Assert.That(base.englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo(@"1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var englishAddress1 = Factory.New<TWJobDocAddress>();
			englishAddress1.E2_AddressOverride = false;
			englishAddress1.E2_CompanyName = "HAPPY CO., LTD.";
			englishAddress1.E2_Address1 = "1500 HAPPY RD";
			englishAddress1.E2_Address2 = "ORANGE DISTRICT";
			englishAddress1.AdditionalAddressInformation = "5F";
			englishAddress1.E2_RN_NKCountryCode = "TW";
			englishAddress1.E2_City = "APPLE CITY";
			englishAddress1.E2_Postcode = "12345";
			englishAddress1.E2_State = "TPE";
			englishAddress = new BuyerAddressData(header, englishAddress1, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison));
			englishAddress1.E2_AddressOverride = true;
			englishAddress = new BuyerAddressData(header, englishAddress1, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT 5F APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestChineseTraditionalAddressFormat()
		{
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var localAddress = Factory.New<TWJobDocAddress>();
			localAddress.E2_AddressOverride = false;
			localAddress.E2_CompanyName = "綠晃科技股份有限公司";
			localAddress.E2_Address1 = "臺北加工出口區園東街7號";
			localAddress.E2_Address2 = string.Empty;
			localAddress.AdditionalAddressInformation = "5樓";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_City = "臺北巿";
			localAddress.E2_Postcode = "90093";
			localAddress.E2_State = "TPE";
			chineseTraditionalAddress = new BuyerAddressData(header, localAddress, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison));
			localAddress.E2_AddressOverride = true;
			chineseTraditionalAddress = new BuyerAddressData(header, localAddress, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街7號5樓").Using(CustomComparers.TypeComparison));
		}

		protected override BuyerAddressData CreateChineseTraditionalAddressData() => new BuyerAddressData(header, jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);

		protected override BuyerAddressData CreateEnglishAddressData() => new BuyerAddressData(header, jobDocAddress, Core.SharedConstants.Languages.English);

		protected override BuyerAddressData CreateFrenchAddressData() => new BuyerAddressData(header, jobDocAddress, Core.SharedConstants.Languages.French);
	}
}
