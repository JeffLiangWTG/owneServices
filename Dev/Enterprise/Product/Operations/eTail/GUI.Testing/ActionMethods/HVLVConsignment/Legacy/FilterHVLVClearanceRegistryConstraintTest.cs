using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Business.Testing;

namespace Enterprise.eTail.GUI.Testing
{
	public sealed class FilterHVLVClearanceRegistryConstraintTest : ConstraintTest<FilterHVLVClearanceRegistryConstraint>
	{
		public override void TestGetValue()
		{
			HVLVDataRegistry.HasHVLVClearance = true;

			AssertEquals("Y", Constraint.GetValue());

			HVLVDataRegistry.HasHVLVClearance = false;

			AssertEquals("N", Constraint.GetValue());
		}

		#region Implementation

		protected override string ExpectedName => "HasHVLVClearance";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;

		#endregion
	}
}
