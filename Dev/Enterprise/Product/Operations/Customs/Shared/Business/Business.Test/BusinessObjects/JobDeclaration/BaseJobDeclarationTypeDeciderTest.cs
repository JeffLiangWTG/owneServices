using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobDeclarationTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>();

		public void TestGetEMCSDeclarationTypeForLoad()
		{
			var declaration = (BusinessObject)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			declaration.FillWithValidTestData();

			AssertEquals("Precondition.", "EMC", declaration[BaseJobDeclaration.Schema.JE_ApplicationCode].ToString());

			Factory.Save();

			var expectedType = ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobDeclaration>();
			var newDeclaration = NewFactory().Load<BaseJobDeclaration>(declaration.PK);
			AssertType("Should load EMCSJobDeclaration.", expectedType, newDeclaration);
		}

		public override void TestGetTypeForLoad()
		{
			const string ASY = "ASYCO";
			const string ZZ = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ASY, "Asycuda country");
			helper.CreateNewOrGetExistingCusCodeList(ZZ, ASY, Core.Constants.CountryCodes.Namibia, "Namibia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ZZ, ASY, Core.Constants.CountryCodes.Lesotho, "Lesotho", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ZZ, ASY, Core.Constants.CountryCodes.Botswana, "Botswana", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ZZ, ASY, Core.Constants.CountryCodes.Swaziland, "Swaziland", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			base.TestGetTypeForLoad();
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var baseJobDeclaration = bizO as BaseJobDeclaration;
			if (baseJobDeclaration != null)
			{
				baseJobDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);

		protected override Type BaseTypeDecidedType => typeof(BaseJobDeclaration);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.FrenchGuiana, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Guadeloupe, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Martinique, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Mayotte, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Reunion, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.SaintBarthelemy, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.SaintMartin, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobDeclaration>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>() },
				{ Core.Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobDeclaration>() },
				{ Core.Constants.CountryGuids._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobDeclaration>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Denmark, ObjectFactory.GetType<Integration.Customs.DK.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.FrenchGuyana, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Guadeloupe, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Martinique, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Mayotte, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Reunion, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.SaintBarthelemy, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.SaintMartin, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobDeclaration>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>() },
				{ Core.Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobDeclaration>() },
				{ Core.Constants.CountryCodes._EUTemplateCountryName_, ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobDeclaration>() },
			};
		}
	}
}
