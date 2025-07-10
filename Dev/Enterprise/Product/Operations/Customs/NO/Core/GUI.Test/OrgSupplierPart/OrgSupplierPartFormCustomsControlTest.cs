using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(OrgSupplierPartFormCustomsControl))]
sealed class OrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new OrgSupplierPartFormCustomsControl();
		_ = userControl.AssertThisControl(x =>
			x.WithCaptionRenderingEnabled()
		);
		var detailsPanel = userControl.AssertContainsControl<ZPanel>("detailsPanel");
		var detailsTabControl = detailsPanel.AssertContainsControl<ZTabControl>("DetailTabControl");
		var detailsTabPage = detailsTabControl.FindSingleOrDefault<ZTabPage>("DetailsTabPage");
		AssertNotNull("DetailsTabPage", detailsTabPage);
		detailsTabPage?.Show();

		_ = detailsTabPage.AssertContainsControl<SupplementaryCodesUserControl>("SupplementaryCodesUserControl");
		_ = detailsTabPage.AssertContainsControl<ZCodeFindBox>("TariffFindBox",
			x => x
			.WithCaption("Tariff")
			.WithBindTo("PivotsForBinding.CI_FormattedTariffNum"));
		_ = detailsTabPage.AssertContainsControl<ZArchitecture.ZCalcEdit>("AddQty1CalcEdit",
			x => x
			.WithCaption("Additional Qty 1"));
		_ = detailsTabPage.AssertContainsControl<ZArchitecture.ZCalcEdit>("AddQty2CalcEdit",
			x => x
			.WithShortCaption("Add. Qty 2")
			.WithCaption("Add. Qty 2")
			.WithFullDescription("Additional Qty 2"));
		_ = detailsTabPage.AssertContainsControl<ZArchitecture.ZCalcEdit>("AddQty3CalcEdit",
			x => x
			.WithShortCaption("Add. Qty 3")
			.WithCaption("Add. Qty 3")
			.WithFullDescription("Additional Qty 3"));
		_ = detailsTabPage.AssertContainsControl<ZArchitecture.ZCalcEdit>("AddQty4CalcEdit",
			x => x
			.WithShortCaption("Add. Qty 4")
			.WithCaption("Add. Qty 4")
			.WithFullDescription("Additional Qty 4"));
		_ = detailsTabPage.AssertContainsControl<ZDropEdit>("VATCodeDropEdit",
			x => x
			.WithBindTo("PivotsForBinding.CI_ZZF_NKTaxType"));
		_ = detailsTabPage.AssertContainsControl<ZDropEdit>("ReducedCustomsFlagDropEdit",
			x => x
			.WithBindTo("PivotsForBinding.CI_ReducedCustomsFlag"));
		_ = detailsTabPage.AssertContainsControl<ZDropEdit>("ProcedureCodeDropEdit",
			x => x
			.WithCaption("Procedure Code"));
		_ = detailsTabPage.AssertContainsControl<ZArchitecture.ZTextBox>("CountryOfOriginTextBox",
			x => x
			.WithBindTo("PivotsForBinding.CI_RN_NKCountryOfOrigin")
			.WithShortCaption("Ctry./Rgn. of Orig.")
			.WithCaption("Ctry./Rgn. of Origin")
			.WithFullDescription("Country/Region of Origin"));
		_ = detailsTabPage.AssertContainsControl<ZDropEdit>("CountyOfOriginDropEdit",
			x => x
			.WithBindTo("PivotsForBinding.CI_RW_NKOriginState"));
		_ = detailsTabPage.AssertContainsControl<ZDropEdit>("PreferenceCodeDropEdit",
			x => x
			.WithCaption("Pref. Code"));
	});
}
