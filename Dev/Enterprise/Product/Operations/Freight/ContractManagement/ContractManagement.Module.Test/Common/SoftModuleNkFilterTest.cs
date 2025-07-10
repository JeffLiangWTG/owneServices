using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(SoftModuleNkFilter))]
	public class SoftModuleNkFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyList = new DummyBusinessObjectCollection(Factory);
			var bizo1 = dummyList.AddNew();

			bizo1.Z0_Code = "XXX";

			var bizo2 = dummyList.AddNew();

			bizo2.Z0_Code = "YYY";

			return new SoftModuleNkFilter("boo", (sqlComparisonOperator, code) => new ZQuery(DummyBizoSchema.Z0_Code, code), DummyModuleIDs.Dummy, dummyList);
		}

		public void TestWarnOnCodeNotPresent()
		{
			var filter = (SoftModuleNkFilter)GetNewBusinessObject();

			filter.Property = "ZZZ";
			AssertHasWarning(filter.PropertyInfo, ListValidation.InvalidCodeMessage);
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = "XXX";
			AssertNoWarning(filter.PropertyInfo, ListValidation.InvalidCodeMessage);
		}
	}
}
