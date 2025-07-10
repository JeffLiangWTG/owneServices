using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OrgTranslatedAddressEffectiveAddressTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgTranslatedAddressEffectiveAddress(null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrgTranslatedAddressEffectiveAddress(Factory.New<OrgTranslatedAddress>()));
			AssertNoExceptionThrown(() => new OrgTranslatedAddressEffectiveAddress(translatedAddress));
		}

		[ExpectNoExceptions]
		public void TestOrgTranslatedAddressEffectiveAddress()
		{
			orgAddress.OA_Phone = "13925568211";
			orgAddress.OA_Email = "test@wistechglobal.com";
			orgAddress.OA_RN_NKCountryCode = "TW";
			translatedAddress.CompanyName = "X1";
			translatedAddress.Address1 = "X2";
			translatedAddress.Address2 = "X3";
			translatedAddress.OTA_AdditionalAddressInformation = "AdditionalAddressInformation";
			translatedAddress.City = "X4";
			translatedAddress.State = "NSW";
			IEffectiveAddress orgTranslatedAddressEffectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			CombineAssertions("Test all the properties", () =>
			{
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.CompanyName, NUnit.Framework.Is.EqualTo("X1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.Address1, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.Address2, NUnit.Framework.Is.EqualTo("X3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.AdditionalAddressInformation, NUnit.Framework.Is.EqualTo("AdditionalAddressInformation").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.City, NUnit.Framework.Is.EqualTo("X4").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.StateCode, NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.Phone, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgTranslatedAddressEffectiveAddress.EMail, NUnit.Framework.Is.EqualTo("test@wistechglobal.com").Using(CustomComparers.TypeComparison));
			}

			);
		}

		[ExpectNoExceptions]
		public void TestGetStateDescription()
		{
			var refCountryStates = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates.RW_Code = "TPE";
			refCountryStates.RW_RN_NKCountryCode = "TW";
			refCountryStates.RW_Description = "Taipei";
			var translateEnglish = Factory.New<RefLanguageText>();
			translateEnglish.RLT_ColumnName = "RW_Description";
			translateEnglish.RLT_Language = Core.SharedConstants.Languages.English;
			translateEnglish.RLT_ParentId = refCountryStates.PK;
			translateEnglish.RLT_ParentTableCode = "RW";
			translateEnglish.RLT_Text = "Taipei";
			var translateChinese = Factory.New<RefLanguageText>();
			translateChinese.RLT_ColumnName = "RW_Description";
			translateChinese.RLT_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translateChinese.RLT_ParentId = refCountryStates.PK;
			translateChinese.RLT_ParentTableCode = "RW";
			translateChinese.RLT_Text = "台北";
			orgAddress.OA_RN_NKCountryCode = "TW";
			translatedAddress.CompanyName = "X1";
			translatedAddress.Address1 = "X2";
			translatedAddress.Address2 = "X3";
			translatedAddress.City = "TT";
			translatedAddress.State = "TPE";
			IEffectiveAddress orgAddressEffectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taipei").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
			}

			GlbStaff.CurrentUser[ZArchitecture.Schema.GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taipei").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("台北").Using(CustomComparers.TypeComparison));
			}

			translatedAddress.State = "";
			orgAddressEffectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestGetCountryName()
		{
			orgAddress.OA_RN_NKCountryCode = "TW";
			translatedAddress.CompanyName = "X1";
			translatedAddress.Address1 = "X2";
			translatedAddress.Address2 = "X3";
			translatedAddress.City = "TT";
			translatedAddress.State = "TPE";
			IEffectiveAddress orgAddressEffectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("台灣").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName("A"), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
			}

			GlbStaff.CurrentUser[ZArchitecture.Schema.GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("台灣").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName("A"), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
			}

			orgAddress.OA_RN_NKCountryCode = "";
			orgAddressEffectiveAddress = new OrgTranslatedAddressEffectiveAddress(translatedAddress);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName("A"), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress = Factory.New<OrgAddress>();
			translatedAddress = orgAddress.TranslatedAddresses.AddNew();
		}

		OrgAddress orgAddress;
		OrgTranslatedAddress translatedAddress;
	}
}
