using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackageTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadWhenCountryIsEmpty()
		{
			var package = (CusPackage)GetNewBusinessObjectForLoadTest();
			package.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = string.Empty;
			Factory.Save();
			var newBusinessObjectFactory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var loadedPackage = newBusinessObjectFactory.Load(BaseTypeDecidedType, package.PK);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TW.ICusPackage>(), loadedPackage.GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loadedPackage = newBusinessObjectFactory.Load(BaseTypeDecidedType, package.PK);
			AssertType<CusPackage>(loadedPackage);
		}

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			if (bizO is CusPackage package)
			{
				package.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			return packingList.PackageJob.Packages.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusPackage);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackage>() },
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
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackage>() }
			};
		}

		#endregion
	}
}
