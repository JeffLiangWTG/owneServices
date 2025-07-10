using System;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class DisableZeroExchangeRateOverridingAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadStatic]
		public static bool IsActive;
	}
}
