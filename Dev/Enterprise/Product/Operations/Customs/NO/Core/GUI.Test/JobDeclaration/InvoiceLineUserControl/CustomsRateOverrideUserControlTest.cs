using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(CustomsRateOverrideUserControl))]
	sealed class CustomsRateOverrideUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new CustomsRateOverrideUserControl())
			{
				AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using var control = new CustomsRateOverrideUserControl();
			CombineAssertions(() =>
			{
				control.AssertContainsControl<ZCalcEdit>("CustomsRateCalcEdit", x => x
					.WithBindTo(nameof(JobComInvoiceLine.CustomsRate))
				);
				control.AssertContainsControl<ZDropEdit>("CustomsTypeDropEdit", x => x
					.WithBindTo(nameof(JobComInvoiceLine.CustomsRateType))
				);
				control.AssertContainsControl<ZCheckBox>("CustomsRateOverrideCheckBox", x => x
					.WithBindTo(nameof(JobComInvoiceLine.CustomsRateIsOverridden))
				);
			});
		}
	}
}
