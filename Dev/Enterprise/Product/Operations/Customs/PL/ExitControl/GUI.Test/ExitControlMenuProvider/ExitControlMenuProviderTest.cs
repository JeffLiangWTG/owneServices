using System;
using Enterprise.Customs.EU.ExitControl.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.GUI.Testing;

[TestedType(typeof(ExitControlMenuProvider))]
sealed class ExitControlMenuProviderTest : ExitControlMenuProviderAbstractTest<ExitControlMenuProvider>
{
	protected override Type ExpectedExitControlMainMenuProviderType => typeof(ExitControlMainMenuProvider);

	protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Poland;

	protected override Type ExpectedConsignmentsGridUserControlMenuProviderType => typeof(EU.ExitControl.GUI.ConsignmentsGridUserControlMenuProvider);

	protected override Type ExpectedReportsGridUserControlMenuProviderType => typeof(EU.ExitControl.GUI.ReportsGridUserControlMenuProvider);
}
