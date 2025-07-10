using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccWithholdingFilterBusinessObject))]
	sealed class AccWithholdingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccWithholdingFilterBusinessObject();
		}

		#region Filters

		public void TestRateFilter()
		{
			AccWithholding withholding1 = Factory.NewWithValidTestData<AccWithholding>();
			AccWithholding withholding2 = Factory.NewWithValidTestData<AccWithholding>();
			AccWithholding withholding3 = Factory.NewWithValidTestData<AccWithholding>();

			withholding1.AW_Rate = 0;
			withholding2.AW_Rate = 1.2;
			withholding3.AW_Rate = 2.2;

			AccWithholdingFilterBusinessObject withholdingFilter = new AccWithholdingFilterBusinessObject();
			((ModuleTextFilter)withholdingFilter["Rate"]).Property = "1.2";
			((ModuleTextFilter)withholdingFilter["Rate"]).IsActive = true;

			Factory.Save();

			AccWithholdingCollection withholdingsCollection = new AccWithholdingCollection(Factory);
			withholdingsCollection.Load(withholdingFilter.Filter);

			AssertCollectionNotContains(withholding1, withholdingsCollection);
			AssertCollectionContains(withholding2, withholdingsCollection);
			AssertCollectionNotContains(withholding3, withholdingsCollection);
		}

		#endregion
	}
}
