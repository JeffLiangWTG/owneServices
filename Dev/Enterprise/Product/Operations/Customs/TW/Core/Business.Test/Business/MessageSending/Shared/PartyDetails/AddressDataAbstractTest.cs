using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class AddressDataAbstractTest<TAddressData> : TestCaseWithFactory
		where TAddressData : AddressData
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestCompanyName()
		{
			NUnit.Framework.Assert.That(englishAddress.CompanyName, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.CompanyName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.CompanyName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddress1()
		{
			NUnit.Framework.Assert.That(englishAddress.Address1, NUnit.Framework.Is.EqualTo("1500 HAPPY RD").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.Address1, NUnit.Framework.Is.EqualTo("臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.Address1, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddress2()
		{
			NUnit.Framework.Assert.That(englishAddress.Address2, NUnit.Framework.Is.EqualTo("ORANGE DISTRICT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.Address2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(frenchAddress.Address2, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAdditionalAddressInformation()
		{
			var tWJobDocAddress = Factory.New<TWJobDocAddress>();
			tWJobDocAddress.E2_AddressOverride = true;
			tWJobDocAddress.AdditionalAddressInformation = "新北市樹林區";
			englishAddress = new AddressData(tWJobDocAddress, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.AdditionalAddressInformation, NUnit.Framework.Is.EqualTo("新北市樹林區").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.AdditionalAddressInformation.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(frenchAddress.AdditionalAddressInformation.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			englishAddress = new AddressData(tWJobDocAddress, true, false, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.AdditionalAddressInformation, NUnit.Framework.Is.EqualTo("新北市樹林區").Using(CustomComparers.TypeComparison));
			chineseTraditionalAddress = new AddressData(tWJobDocAddress, false, true, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.AdditionalAddressInformation, NUnit.Framework.Is.EqualTo("新北市樹林區").Using(CustomComparers.TypeComparison));
		}

		public void TestChineseTraditionalAddressWhenAddressIsNull()
		{
			var chineseAddress = new AddressData((JobDocAddress)null, Core.SharedConstants.Languages.ChineseTraditional);
			AssertNoExceptionThrown("Should not have the ArgumentNullException, message: Value cannot be null, Parameter name: jobDocAddress", () => { _ = chineseAddress.ChineseTraditionalAddressFormat; });
		}

		[ExpectNoExceptions]
		public void TestCity()
		{
			NUnit.Framework.Assert.That(englishAddress.City, NUnit.Framework.Is.EqualTo("APPLE CITY").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.City, NUnit.Framework.Is.EqualTo("臺北巿").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.City, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestStateCode()
		{
			NUnit.Framework.Assert.That(englishAddress.StateCode, NUnit.Framework.Is.EqualTo("TPE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.StateCode, NUnit.Framework.Is.EqualTo("TPE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.StateCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestStateDescription()
		{
			var stateQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, "TW");
			stateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, "TPE");
			var states = Factory.LoadTop1<RefCountryStates>(stateQuery);
			var refLang = Factory.New<RefLanguageText>();
			refLang.RLT_ParentId = states.PK;
			refLang.RLT_ParentTableCode = "RW";
			refLang.RLT_Language = "ZH-TW";
			refLang.RLT_ColumnName = "RW_Description";
			refLang.RLT_Text = "台北";
			NUnit.Framework.Assert.That(englishAddress.StateDescription, NUnit.Framework.Is.EqualTo("TAIPEI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.StateDescription, NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.StateDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(englishAddress.StateDescription, NUnit.Framework.Is.EqualTo("TAIPEI").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(chineseTraditionalAddress.StateDescription, NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(frenchAddress.StateDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			}
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(englishAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.CountryCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCountryName()
		{
			NUnit.Framework.Assert.That(englishAddress.CountryName, NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.CountryName, NUnit.Framework.Is.EqualTo("台灣").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.CountryName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(englishAddress.CountryName, NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(chineseTraditionalAddress.CountryName, NUnit.Framework.Is.EqualTo("台灣").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(frenchAddress.CountryName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			}
		}

		[ExpectNoExceptions]
		public void TestPostcode()
		{
			NUnit.Framework.Assert.That(englishAddress.Postcode, NUnit.Framework.Is.EqualTo("12345").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.Postcode, NUnit.Framework.Is.EqualTo("90093").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.Postcode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPhone()
		{
			NUnit.Framework.Assert.That(englishAddress.Phone, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(chineseTraditionalAddress.Phone, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.Phone, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestEnglishAddressFormat()
		{
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
			englishAddress = new AddressData(header.MainAddress, true, true, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			header.MainAddress.OA_RN_NKCountryCode = "AU";
			header.MainAddress.OA_State = "ACT";
			englishAddress = new AddressData(header, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY AUSTRALIAN CAPITAL TERRITORY 12345 AUSTRALIA").Using(CustomComparers.TypeComparison));
			englishAddress = new AddressData((OrgAddress)null, false, false, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			jobDocAddress.E2_AddressOverride = true;
			englishAddress = new AddressData(jobDocAddress, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY AUSTRALIAN CAPITAL TERRITORY 12345 AUSTRALIA").Using(CustomComparers.TypeComparison));
			jobDocAddress.E2_Address1 = ZString.Empty;
			englishAddress = new AddressData(jobDocAddress, Core.SharedConstants.Languages.English);
			NUnit.Framework.Assert.That(englishAddress.EnglishAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestChineseTraditionalAddressFormat()
		{
			var stateQuery = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, "AU");
			stateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, "ACT");
			var states = Factory.LoadTop1<RefCountryStates>(stateQuery);
			var refLang = Factory.New<RefLanguageText>();
			refLang.RLT_ParentId = states.PK;
			refLang.RLT_ParentTableCode = "RW";
			refLang.RLT_Language = "ZH-TW";
			refLang.RLT_ColumnName = "RW_Description";
			refLang.RLT_Text = "澳洲首都領地";
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(frenchAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
			chineseTraditionalAddress = new AddressData(header.MainAddress, true, true, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			header.MainAddress.OA_RN_NKCountryCode = "AU";
			header.MainAddress.TranslatedAddresses.First().OTA_State = "ACT";
			chineseTraditionalAddress = new AddressData(header, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("90093澳洲澳洲首都領地臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison));
			jobDocAddress.E2_AddressOverride = true;
			chineseTraditionalAddress = new AddressData(jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo("12345澳洲TPEAPPLE CITY1500 HAPPY RDORANGE DISTRICT").Using(CustomComparers.TypeComparison));
			jobDocAddress.E2_Address1 = ZString.Empty;
			chineseTraditionalAddress = new AddressData(jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);
			NUnit.Framework.Assert.That(chineseTraditionalAddress.ChineseTraditionalAddressFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion

		protected abstract TAddressData CreateChineseTraditionalAddressData();
		protected abstract TAddressData CreateEnglishAddressData();
		protected abstract TAddressData CreateFrenchAddressData();

		protected override void SetUp()
		{
			base.SetUp();
			header = new TestTWCreator(Factory).CreateOrganization();
			englishAddress = CreateEnglishAddressData();
			chineseTraditionalAddress = CreateChineseTraditionalAddressData();
			frenchAddress = CreateFrenchAddressData();
			jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.E2_OA_Address = header.MainAddress.PK;
		}

		protected OrgHeader header;
		protected AddressData englishAddress;
		protected AddressData chineseTraditionalAddress;
		protected AddressData frenchAddress;
		protected TWJobDocAddress jobDocAddress;
	}
}
