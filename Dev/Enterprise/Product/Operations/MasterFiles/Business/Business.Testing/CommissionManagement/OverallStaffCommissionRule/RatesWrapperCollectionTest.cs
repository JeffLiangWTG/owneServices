using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OverallStaffCommissionRule.RatesWrapperCollection))]
	sealed class RatesWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OverallStaffCommissionRule.RatesWrapperCollection>
	{
		protected override OverallStaffCommissionRule.RatesWrapperCollection GetCollectionToTest()
		{
			return new OverallStaffCommissionRule.RatesWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = new AccCommissionRuleRateCollection(Factory);
			return new OverallStaffCommissionRule.RatesWrapper(collection);
		}
	}
}
