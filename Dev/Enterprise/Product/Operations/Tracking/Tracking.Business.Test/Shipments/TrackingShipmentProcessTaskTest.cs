using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingShipmentProcessTask))]
	sealed class TrackingShipmentProcessTaskTest : RoutingSupportProcessTaskTest<TrackingShipment>
	{
		#region Implementation

		protected override ZString ParentOrigin
		{
			get { return Job.JS_RL_NKOrigin; }
			set { Job.JS_RL_NKOrigin = value; }
		}

		protected override ZString ParentDestination
		{
			get { return Job.JS_RL_NKDestination; }
			set { Job.JS_RL_NKDestination = value; }
		}

		#endregion
	}
}
