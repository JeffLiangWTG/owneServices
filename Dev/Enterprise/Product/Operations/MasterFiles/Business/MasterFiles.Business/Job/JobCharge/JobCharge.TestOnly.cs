#if DEBUG

using System;
using NUnit.Framework;

//[Reminder] This file wasn't moved to test proejct, because it's used by non test class.
namespace Enterprise.MasterFiles.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class SuspendToTestReportJobChargeIsChangedByDifferentCompanyAttribute : TestSetupAttribute
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

#endif
