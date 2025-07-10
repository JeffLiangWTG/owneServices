using System;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class UseGlobalAllocationsAttribute : TestSetupAttribute
	{
		public UseGlobalAllocationsAttribute(bool value)
		{
			this.Value = value;
		}

		public bool Value { get; private set; }

		public override void SetUp(TestCase testCase)
		{
			FreightConfigurationRegistry.Instance.UseGlobalAllocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Value);
		}

		public override void TearDown(TestCase testCase)
		{
		}
	}
}
