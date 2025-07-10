using System;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(ExitControlConfiguration))]
sealed class ExitControlConfigurationTest : EU.ExitControl.Business.Testing.ExitControlConfigurationTest<ExitControlConfiguration>
{
	protected override Type GetCusExitReportConfigurationTypeForTest() => typeof(CusExitReportConfiguration);
}
