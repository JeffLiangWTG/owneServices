using System;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitReportConfiguration))]
sealed class CusExitReportConfigurationTest : EU.ExitControl.Business.Testing.CusExitReportConfigurationTest<CusExitReportConfiguration>
{
	protected override EU.ExitControl.Business.CusExitReport GetUcc6ExitReport() => Factory.New<CusExitReport>();

	protected override Type ExpectedBaseValidationDeciderType => typeof(CusExitReportUcc6ValidationDecider);

	protected override Type ExpectedUcc6ValidationDeciderType => typeof(CusExitReportUcc6ValidationDecider);
}
