using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class BuiltInOnlyCountryCompaniesRegistryItemTest : TransactionedTestCase
	{
		public void TestIsVisible()
		{
			var factory = new BusinessObjectFactory();
			var companyUS = factory.New<GlbCompany>();
			companyUS.GC_Code = "US1";
			companyUS.GC_RN_NKCountryCode = "US";
			var companyAE = factory.New<GlbCompany>();
			companyAE.GC_Code = "AE1";
			companyAE.GC_RN_NKCountryCode = "AE";
			factory.Save();
			var registryItem = new NonBuiltInOnlyCountryRegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new BooleanRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, false);
			Assert("IsVisible", registryItem.IsVisible(companyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, null, null));
			Assert("IsVisible", registryItem.IsVisible(companyAE.PK.ToGuid(), Guid.Empty, Guid.Empty, null, null));
		}
	}
}
