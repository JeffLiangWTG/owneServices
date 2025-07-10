using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem))]
	sealed class LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItemTest : StronglyTypedRegistryItemTestCase<bool>
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
			var companyNL = factory.New<GlbCompany>();
			companyNL.GC_Code = "NL1";
			companyNL.GC_RN_NKCountryCode = "NL";
			var companyIE = factory.New<GlbCompany>();
			companyIE.GC_Code = "IE1";
			companyIE.GC_RN_NKCountryCode = "IE";
			var companyBE = factory.New<GlbCompany>();
			companyBE.GC_Code = "BE1";
			companyBE.GC_RN_NKCountryCode = "BE";
			var companyCN = factory.New<GlbCompany>();
			companyCN.GC_Code = "CN1";
			companyCN.GC_RN_NKCountryCode = "CN";
			factory.Save();
			var registryItem = new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
			Assert("US - Invisible", !registryItem.IsVisible(companyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
			Assert("AE - Invisible", !registryItem.IsVisible(companyAE.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
			Assert("NL - Invisible", !registryItem.IsVisible(companyNL.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
			Assert("IE - Visible", registryItem.IsVisible(companyIE.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
			Assert("BE - Visible", registryItem.IsVisible(companyBE.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
			Assert("CN - Invisible", !registryItem.IsVisible(companyCN.PK.ToGuid(), Guid.Empty, Guid.Empty, null));
		}

		public void TestNewBooleanRegistryItem()
		{
			var registryItem = new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);
			AssertEquals("Name", registryItem.Name);
			AssertEquals("Category", registryItem.Category);
			AssertEquals("Caption", registryItem.Caption);
			AssertEquals("Hint", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
		}

		protected override StronglyTypedRegistryItem<bool, bool> GetNewRegistryItem()
		{
			return new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
