using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconDeclarationTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusReconDeclaration = bizO as CusReconDeclaration;
			if (cusReconDeclaration != null)
			{
				cusReconDeclaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var cusReconDeclaration = Factory.New<CusReconDeclaration>();
			cusReconDeclaration.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;
			cusReconDeclaration.CRD_JobReferenceNumber = "MC00001";
			return cusReconDeclaration;
		}

		protected override Type BaseTypeDecidedType => typeof(CusReconDeclaration);

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusReconDeclaration>() },
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusReconDeclaration>() },
			};
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusReconDeclaration>() },
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusReconDeclaration>() },
			};
		}
	}
}
