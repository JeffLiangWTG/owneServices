using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackingListTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadWhenCountryIsEmpty()
		{
			var packingList = (CusPackingList)GetNewBusinessObjectForLoadTest();
			packingList.Declaration.Branch.Company.GC_RN_NKCountryCode = string.Empty;
			Factory.Save();
			var newBusinessObjectFactory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var loadedPackingList = newBusinessObjectFactory.Load(BaseTypeDecidedType, packingList.PK);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TW.ICusPackingList>(), loadedPackingList.GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loadedPackingList = newBusinessObjectFactory.Load(BaseTypeDecidedType, packingList.PK);
			AssertType<CusPackingList>(loadedPackingList);
		}

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			if (bizO is CusPackingList packingList)
			{
				packingList.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			return packingList;
		}

		protected override Type BaseTypeDecidedType => typeof(CusPackingList);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackingList>() },
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
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackingList>() }
			};
		}

		#endregion
	}
}
