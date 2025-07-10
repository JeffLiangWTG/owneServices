using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	public class RefCusConditionApplicabilityValueFixture
	{
		[Test]
		public void Constructor()
		{
			var val = new RefCusConditionValue
			{
				ZX3_Value = "AAA",
				ZX3_ZX4_NKValueType = "BBB",
				ZX3_ZX4_ZZZ_NKDataGrouping = "CCC",
				ZX3_LogicalORWithinGroup = new byte()
			};

			var condAppVal = new RefCusConditionApplicabilityValue(val);

			Assert.That(condAppVal.S08_Value == val.ZX3_Value);
			Assert.That(condAppVal.S08_ZX4_NKValueType == val.ZX3_ZX4_NKValueType);
			Assert.That(condAppVal.S08_ZX4_ZZZ_NKDataGrouping == val.ZX3_ZX4_ZZZ_NKDataGrouping);
			Assert.That(condAppVal.S08_LogicalORWithinGroup == val.ZX3_LogicalORWithinGroup);
		}
	}
}
