using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgSupplierPartTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode) => GlbCompany.CurrentCompany.SetCountry(countryCode);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.WallisAndFutunaIslands);
			var bizObj = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IGlobalOrgSupplierPart>();
			var orgSupplierPart = bizObj as OrgSupplierPart;
			orgSupplierPart.OP_PartNum = orgSupplierPart.PK.ToString().Replace("-", "");
			return bizObj;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Denmark, ObjectFactory.GetType<Enterprise.Integration.Customs.DK.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Finland, ObjectFactory.GetType<Enterprise.Integration.Customs.FI.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Enterprise.Integration.Customs.IE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Enterprise.Integration.Customs.IL.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Enterprise.Integration.Customs.MY.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Enterprise.Integration.Customs.PL.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Enterprise.Integration.Customs.SG.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Sweden, ObjectFactory.GetType<Enterprise.Integration.Customs.SE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Enterprise.Integration.Customs.AE.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Enterprise.Integration.Customs._CustomsTemplate_.IOrgSupplierPart>() },
				{ Core.Constants.CountryGuids._EUTemplateCountryName_, ObjectFactory.GetType<Enterprise.Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Type BaseTypeDecidedType => typeof(OrgSupplierPart);

		protected override Type OverriddenDefaultDecidedType => ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IGlobalOrgSupplierPart>();

		protected override void SetUp()
		{
			base.SetUp();
			SetAsycudaCustomsCountryCodes();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Denmark, ObjectFactory.GetType<Enterprise.Integration.Customs.DK.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Finland, ObjectFactory.GetType<Enterprise.Integration.Customs.FI.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Enterprise.Integration.Customs.IE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Enterprise.Integration.Customs.MY.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Enterprise.Integration.Customs.PL.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Enterprise.Integration.Customs.SG.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Enterprise.Integration.Customs.SE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Enterprise.Integration.Customs.AE.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Enterprise.Integration.Customs._CustomsTemplate_.IOrgSupplierPart>() },
				{ Core.Constants.CountryCodes._EUTemplateCountryName_, ObjectFactory.GetType<Enterprise.Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart>() },
			};
		}
	}
}
