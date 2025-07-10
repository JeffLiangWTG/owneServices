#if DEBUG

using System;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Test
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class MatchAgainstOnlineFlightsInUnitTestAttribute : TestSetupAttribute
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Test only field used to control behavior in specific test cases.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211", Justification = "Test only field. Turning this into a property won't improve readability.")]
		public static bool Enabled;

		public override void SetUp(TestCase testCase)
		{
			Enabled = true;
		}

		public override void TearDown(TestCase testCase)
		{
			Enabled = false;
		}

		public static IDisposable Enable()
			=> new DisposableAction(
				createAction: () => Enabled = true,
				disposeAction: () => Enabled = false);
	}
}

#endif
