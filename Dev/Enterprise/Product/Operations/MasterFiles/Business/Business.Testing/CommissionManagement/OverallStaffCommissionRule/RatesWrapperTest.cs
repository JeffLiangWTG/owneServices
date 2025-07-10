using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OverallStaffCommissionRule.RatesWrapper))]
	sealed class RatesWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new AccCommissionRuleRateCollection(Factory);
			return new OverallStaffCommissionRule.RatesWrapper(collection);
		}
	}
}
