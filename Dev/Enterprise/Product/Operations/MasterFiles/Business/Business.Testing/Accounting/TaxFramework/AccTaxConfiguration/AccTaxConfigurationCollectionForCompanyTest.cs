using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxConfigurationCollectionForCompanyOrBranch))]
	sealed class AccTaxConfigurationCollectionForCompanyTest : AccTaxConfigurationCollectionForCompanyOrBranchTest<GlbCompany>
	{
		protected override AccTaxConfigurationCollectionForCompanyOrBranch GetCollection(GlbCompany parent) => new AccTaxConfigurationCollectionForCompanyOrBranch(parent);

		protected override GlbCompany GetCollectionParent() => Factory.NewWithValidTestData<GlbCompany>();

		protected override ZString GetParentCountry(GlbCompany parent) => parent.GC_RN_NKCountryCode;

		protected override AccTaxConfigurationCollection GetCollectionToTest()
		{
			return new AccTaxConfigurationCollectionForCompanyOrBranch(GlbCompany.GetCurrentCompany(Factory));
		}
	}
}
