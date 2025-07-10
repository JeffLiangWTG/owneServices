using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BasePackingGroupTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		#region Overrides

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.IPackingGroup>();

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var packGroup = bizO as BasePackingGroup;
			if (packGroup != null)
			{
				packGroup.Bill.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
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

			return packGroup;
		}

		protected override Type BaseTypeDecidedType => typeof(BasePackingGroup);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IPackingGroup>() },
				{ Core.Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.IPackingGroup>() },
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IPackingGroup>() },
				{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.IPackingGroup>() },
				{ Core.Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.IPackingGroup>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IPackingGroup>() }
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
				{ Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IPackingGroup>() }
			};
		}

		#endregion
	}
}
