using System;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	sealed class UseDummyDetentionStrategyAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value = true;
		}

		public override void TearDown(TestCase testCase)
		{
			DetentionStrategy.useDummyDetentionStrategy_ForTesting.Value = false;
			DummyDetentionStrategy.ResetInstance();
		}
	}
}
