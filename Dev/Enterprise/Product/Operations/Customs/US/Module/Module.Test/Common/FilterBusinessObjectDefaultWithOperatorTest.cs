using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class FilterBusinessObjectDefaultWithOperatorTest : TestCaseWithFactory
	{
		public void TestFilterBusinessObject()
		{
			var businessObject = new FilterBusinessObjectDefaultWithOperator("AAA", "BBB", (ZString)"CCC", ModuleTextFilter.ComparisonConstants.NotEqual);
			AssertEquals("AAA", businessObject.FilterName);
			AssertEquals("BBB", businessObject.PropertyName);
			AssertEquals("CCC", businessObject.Value);
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, businessObject.ComparisonOperator);
		}
	}
}
