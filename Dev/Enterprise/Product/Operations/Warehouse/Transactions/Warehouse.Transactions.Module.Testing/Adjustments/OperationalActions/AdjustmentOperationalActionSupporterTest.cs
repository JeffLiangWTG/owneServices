using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdjustmentOperationalActionSupporter))]
	public class AdjustmentOperationalActionSupporterTest : WhsOperationalActionSupporterTest<AdjustmentOperationalActionSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsAdjustment;

		#endregion

		#region TestSingularElementNoun

		public void TestSingularElementNoun()
		{
			AssertEquals("Adjustment", Supporter.SingularElementNoun);
		}

		#endregion

		#region TestPluralElementNoun

		public void TestPluralElementNoun()
		{
			AssertEquals("Adjustments", Supporter.PluralElementNoun);
		}

		#endregion
	}
}
