#if DEBUG

using System;
using NUnit.Framework;

//[Reminder] This file wasn't moved to test proejct, because it's shared by many non-Accounting test classes.
namespace Enterprise.MasterFiles.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SuspendToTestReportJobIsChangedByDifferentCompanyAttribute : TestSetupAttribute
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
