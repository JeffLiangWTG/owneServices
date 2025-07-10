using System;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentNumberFountainUniqueIndexFailureHandlingTest : ShipmentNumberFountainUniqueIndexFailureHandlingTest
	{
		#region Implementation

		public override void TestNumberFountainFix()
		{
			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				Env.Registry.AllowManualShipmentEntry = false;
			}

			base.TestNumberFountainFix();
		}

		protected override Type BizOTypeToTest
		{
			get { return typeof(ForwardingShipment); }
		}

		#endregion
	}
}
