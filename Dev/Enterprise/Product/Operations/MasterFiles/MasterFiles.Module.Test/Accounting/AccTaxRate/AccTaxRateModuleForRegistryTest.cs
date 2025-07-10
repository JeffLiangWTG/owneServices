using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxRateModuleForRegistry))]
	sealed class AccTaxRateModuleForRegistryTest : ZModuleBasherTest
	{
		public void TestCollectionFilter()
		{
			using (var module = new AccTaxRateModuleForRegistryForTest())
			{
				var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				AssertNotNull("Pre-condition: Should be a company with different Country", company);
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();

				taxRate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
				Factory.Save();

				var collection = (BusinessObjectCollection)module.GetNewGridCollectionForTesting();

				var filter = new ZQuery(AccTaxRateSchema.AT_Code, taxRate.AT_Code);
				collection.Load(filter);
				AssertEquals("Contains(taxRate)", true, collection.Contains(taxRate));

				collection = (BusinessObjectCollection)module.GetNewGridCollectionForTesting();

				collection.Load();
				AssertEquals("Contains(taxRate)", true, collection.Contains(taxRate));
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccTaxRateForRegistry;
		}

		#endregion
	}
}
