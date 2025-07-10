using System;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	internal sealed class ShipmentDateUpdateConfigurationAttribute : TestSetupAttribute
	{
		public bool Value { get; set; }

		public override void SetUp(TestCase testCase)
		{
			AgencyRegistry.Instance.UpdateShipmentDatesFromSailing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Value);
		}

		public override void TearDown(TestCase testCase)
		{
		}
	}
}
