using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxConfigurationCollectionForCompanyOrBranch))]
	sealed class AccTaxConfigurationCollectionForBranchTest : AccTaxConfigurationCollectionForCompanyOrBranchTest<GlbBranch>
	{
		protected override AccTaxConfigurationCollectionForCompanyOrBranch GetCollection(GlbBranch parent) => new AccTaxConfigurationCollectionForCompanyOrBranch(parent);

		protected override GlbBranch GetCollectionParent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Uruguay;
			return branch;
		}

		protected override ZString GetParentCountry(GlbBranch parent) => parent.Company.GC_RN_NKCountryCode;

		protected override AccTaxConfigurationCollection GetCollectionToTest()
		{
			return new AccTaxConfigurationCollectionForCompanyOrBranch(GlbBranch.GetCurrentBranch(Factory));
		}
	}
}
