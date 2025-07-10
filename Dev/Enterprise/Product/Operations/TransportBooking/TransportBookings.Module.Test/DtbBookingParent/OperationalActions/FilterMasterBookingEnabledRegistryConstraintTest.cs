using System;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.TransportCommon.Registry;

namespace Enterprise.TransportBookings.Module.Testing
{
	sealed class FilterMasterBookingEnabledRegistryConstraintTest : ConstraintTest<FilterMasterBookingEnabledRegistryConstraint>
	{
		public override void TestGetValue()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Should indicate when registry setting is false", "N", Constraint.GetValue());
			}

			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Should indicate when registry setting is true", "Y", Constraint.GetValue());
			}
		}

		protected override string ExpectedName => "MasterBookingsEnabled";

		protected override string ExpectedSingularValueName => "value";

		protected override string ExpectedPluralValueName => "values";

		protected override bool ExpectGetValueToReturnGetDefaultStringValue => true;
	}
}
