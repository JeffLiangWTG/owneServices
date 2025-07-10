using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackageJobTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadWhenCountryIsEmpty()
		{
			var packageJob = (CusPackageJob)GetNewBusinessObjectForLoadTest();
			packageJob.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = string.Empty;
			Factory.Save();
			var newBusinessObjectFactory = new BusinessObjectFactory();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var loadedPackageJob = newBusinessObjectFactory.Load(BaseTypeDecidedType, packageJob.PK);
			AssertEquals(ObjectFactory.GetType<Integration.Customs.TW.ICusPackageJob>(), loadedPackageJob.GetType());

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			loadedPackageJob = newBusinessObjectFactory.Load(BaseTypeDecidedType, packageJob.PK);
			AssertType<CusPackageJob>(loadedPackageJob);
		}

		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			if (bizO is CusPackageJob packageJob)
			{
				packageJob.PackingList.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = packingList.PackageJob;
			packageJob.Packages.AddNew();
			return packageJob;
		}

		protected override Type BaseTypeDecidedType => typeof(CusPackageJob);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackageJob>() },
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
				{ Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusPackageJob>() }
			};
		}

		#endregion
	}
}
