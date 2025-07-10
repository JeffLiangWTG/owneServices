using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusGuaranteeHeaderTypeDeciderTest : TestCaseWithFactory
	{
		[AsycudaCustomsCountries(Core.Constants.CountryCodes.Congo)]
		public void TestGetTypeForLoadForAsycudaCustoms()
		{
			AssertGetTypeForLoad<Integration.Customs.AsycudaCustoms.ICusGuaranteeHeader>(Core.Constants.CountryCodes.Congo);
		}

		public void TestGetTypeForLoadForZA()
		{
			AssertGetTypeForLoad<Integration.Customs.IBaseCusGuaranteeHeader>(Constants.CountryCodes.SouthAfrica);
		}

		public void TestGetTypeForLoadForLV()
		{
			AssertGetTypeForLoad<Integration.Customs.EU.ICusGuaranteeHeader>(Constants.CountryCodes.Latvia);
		}

		public void TestGetTypeForLoadForGB()
		{
			AssertGetTypeForLoad<Integration.Customs.EU.ICusGuaranteeHeader>(Constants.CountryCodes.UnitedKingdom);
		}

		public void TestGetTypeForLoadForCH()
		{
			AssertGetTypeForLoad<Integration.Customs.EU.ICusGuaranteeHeader>(Constants.CountryCodes.Switzerland);
		}

		public void TestGetTypeForLoadForFR()
		{
			AssertGetTypeForLoad<Integration.Customs.FR.ICusGuaranteeHeader>(Constants.CountryCodes.France);
		}

		public void TestGetTypeForLoadForNL()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<BaseCusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.NL.ICusGuaranteeHeader>();

			AssertType("NL guarantee should be of Customs.NL.CusGuaranteeHeader type", expectedType, loadedGuaranteeHeader);
		}

		public void TestGetTypeForNew()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("New ZA guarantee should be of Customs.BaseCusGuaranteeHeader type", typeof(BaseCusGuaranteeHeader), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("New AU guarantee should be of Customs.BaseCusGuaranteeHeader type", typeof(BaseCusGuaranteeHeader), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals("New GB guarantee should be of EU GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				AssertEquals("New LV guarantee should be of EU GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				AssertEquals("New FR guarantee should be of FR GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				AssertEquals("New NL guarantee should be of NL GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForNew());
			}
		}

		public void TestGetTypeForBinding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("New ZA guarantee should be of Customs.BaseCusGuaranteeHeader type", typeof(BaseCusGuaranteeHeader), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("New AU guarantee should be of Customs.BaseCusGuaranteeHeader type", typeof(BaseCusGuaranteeHeader), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals("New GB guarantee should be of EU GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				AssertEquals("New LV guarantee should be of EU GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				AssertEquals("New FR guarantee should be of FR GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				AssertEquals("New NL guarantee should be of NL GuaranteeHeader type", ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>(), new BaseCusGuaranteeHeaderTypeDecider().GetTypeForBinding());
			}
		}

		void AssertGetTypeForLoad<T>(string countryCode) where T : class, Integration.Customs.IBaseCusGuaranteeHeader
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = countryCode;
			Factory.Save();

			var expectedType = ObjectFactory.GetType<T>();

			CombineAssertions(() =>
			{
				var loadedGuaranteeHeader = NewFactory().Load<T>(guaranteeHeader.PK) as BusinessObject;
				AssertType("Load by Interface", expectedType, loadedGuaranteeHeader);

				loadedGuaranteeHeader = NewFactory().Load<BaseCusGuaranteeHeader>(guaranteeHeader.PK);
				AssertType("Load by BaseCusGuaranteeHeader", expectedType, loadedGuaranteeHeader);

				loadedGuaranteeHeader = NewFactory().Load<CommonCusPermitHeader>(guaranteeHeader.PK);
				AssertType("Load by CommonCusPermitHeader", expectedType, loadedGuaranteeHeader);
			});
		}
	}
}
