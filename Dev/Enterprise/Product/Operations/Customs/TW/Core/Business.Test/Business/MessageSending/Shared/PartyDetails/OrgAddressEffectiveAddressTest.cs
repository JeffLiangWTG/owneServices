using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class OrgAddressEffectiveAddressTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgAddressEffectiveAddress(null));
			AssertNoExceptionThrown(() => new OrgAddressEffectiveAddress(address));
		}

		[ExpectNoExceptions]
		public void TestOrgAddressEffectiveAddress()
		{
			address.CompanyName = "X1";
			address.Address1 = "X2";
			address.Address2 = "X3";
			address.OA_AdditionalAddressInformation = "AdditionalAddressInformation";
			address.City = "X4";
			address.State = "NSW";
			address.OA_RN_NKCountryCode = "TW";
			address.OA_Phone = "13925568211";
			address.OA_Email = "test@wistechglobal.com";
			IEffectiveAddress orgAddressEffectiveAddress = new OrgAddressEffectiveAddress(address);
			CombineAssertions("Test all the properties", () =>
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.CompanyName, NUnit.Framework.Is.EqualTo("X1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.Address1, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.Address2, NUnit.Framework.Is.EqualTo("X3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.AdditionalAddressInformation, NUnit.Framework.Is.EqualTo("AdditionalAddressInformation").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.City, NUnit.Framework.Is.EqualTo("X4").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.StateCode, NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("NSW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetCountryName(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("Taiwan").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.Phone, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.EMail, NUnit.Framework.Is.EqualTo("test@wistechglobal.com").Using(CustomComparers.TypeComparison));
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
			address.CompanyName = "X1";
			address.Address1 = "X2";
			address.Address2 = "X3";
			address.City = "X4";
			address.State = "TPE";
			address.OA_RN_NKCountryCode = "TW";
			address.OA_Phone = "13925568211";
			IEffectiveAddress orgAddressEffectiveAddress = new OrgAddressEffectiveAddress(address);
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

			address.State = "";
			orgAddressEffectiveAddress = new OrgAddressEffectiveAddress(address);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseTraditional))
			{
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.English), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(orgAddressEffectiveAddress.GetStateDescription(Core.SharedConstants.Languages.ChineseTraditional), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestGetCountryName()
		{
			address.CompanyName = "X1";
			address.Address1 = "X2";
			address.Address2 = "X3";
			address.City = "X4";
			address.State = "TPE";
			address.OA_RN_NKCountryCode = "TW";
			address.OA_Phone = "13925568211";
			IEffectiveAddress orgAddressEffectiveAddress = new OrgAddressEffectiveAddress(address);
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

			address.OA_RN_NKCountryCode = "";
			orgAddressEffectiveAddress = new OrgAddressEffectiveAddress(address);
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
			address = Factory.New<OrgAddress>();
		}

		OrgAddress address;
	}
}
