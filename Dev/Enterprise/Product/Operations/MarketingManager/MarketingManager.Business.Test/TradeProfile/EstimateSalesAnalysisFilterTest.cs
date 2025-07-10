using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EstimateSalesAnalysisFilter))]
	sealed class EstimateSalesAnalysisFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var analysisFilter = new EstimateSalesAnalysisFilter();
			AssertEquals(EstimateSalesAnalysisStatusFilterList.Codes.All, analysisFilter.Status);
			AssertEquals(true, analysisFilter.ShouldMatchOnBuyerSupplier);
		}

		#endregion
	}
}
