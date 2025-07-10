using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BasePackageTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var package = bizO as BasePackage;
			if (package != null)
			{
				package.PackingGroup.Bill.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123456";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345678";

			var packGroup = (houseBill.PackingGroups.Count > 0) ? houseBill.PackingGroups[0] : houseBill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;
			packGroup.CR_CU_HouseBill = houseBill.PK;

			var package = (packGroup.Packages.Count > 0) ? packGroup.Packages[0] : packGroup.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "PT";
			return package;
		}

		protected override Type BaseTypeDecidedType => typeof(BasePackage);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IPackage>() },
				{ Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.IPackage>() },
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IPackage>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IPackage>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.IPackage>() },
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IPackage>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IPackage>() },
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IPackage>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IPackage>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IPackage>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IPackage>() },
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

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IPackage>() },
				{ Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IPackage>() },
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IPackage>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IPackage>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IPackage>() },
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IPackage>() },
				{ Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IPackage>() },
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.IPackage>() },
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IPackage>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IPackage>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IPackage>() },
			};
		}
	}
}
