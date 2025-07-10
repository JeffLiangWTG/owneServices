using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocationMethodDefaultRuleCollection))]
	sealed class AllocationMethodDefaultRuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AllocationMethodDefaultRuleCollection>
	{
		#region Implementation

		protected override AllocationMethodDefaultRuleCollection GetCollectionToTest()
		{
			return new AllocationMethodDefaultRuleCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AllocationMethodDefaultRule(Factory);
		}

		#endregion
	}
}
