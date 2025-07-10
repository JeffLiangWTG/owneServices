using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(AvailableRuleCollection))]
	sealed class AvailableRuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AvailableRuleCollection>
	{
		#region Implementation

		protected override AvailableRuleCollection GetCollectionToTest()
		{
			return new AvailableRuleCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AvailableRule(Factory);
		}

		#endregion
	}
}
