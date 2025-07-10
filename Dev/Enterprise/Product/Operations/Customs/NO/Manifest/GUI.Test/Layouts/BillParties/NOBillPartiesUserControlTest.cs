using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOBillPartiesUserControl))]
sealed class NOBillPartiesUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new NOBillPartiesUserControl();
		_ = userControl.AssertContainsControl<SeparatorUserControl>(nameof(userControl.RepresentativeSeparatorUserControl),
			x => x
			.WithCaption("Representative Details"));
		_ = userControl.AssertContainsControl<ZAddressControl>(nameof(userControl.RepresentativeAddressControl),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_OA_Forwarder))
			.WithCaption("Party"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeNameTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderCompanyName))
			.WithCaption("Name"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeStreet1TextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderStreet1))
			.WithCaption("Street 1"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeStreet2TextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderStreet2))
			.WithCaption("Street 2"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeCityTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderCity))
			.WithCaption("City"));
		_ = userControl.AssertContainsControl<ZCodeFindBox>(nameof(userControl.RepresentativeCountryCodeFindBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_Forwarder_RN_NKCountryCode))
			.WithShortCaption("Ctry/Rgn.")
			.WithCaption("Country/Region")
			.WithFullDescription("The Country/Region code for the Representative address"));
		_ = userControl.AssertContainsControl<ZDropEdit>(nameof(userControl.RepresentativeStateDropEdit),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderState))
			.WithCaption("State"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativePostCodeTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderPostCode))
			.WithCaption("Postcode"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativePhoneTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderPhone))
			.WithCaption("Phone"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeRegNoTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderRegNumber))
			.WithCaption("Reg.No"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.RepresentativeEmailTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ForwarderEmail)));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.ShipperEmailTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ShipperEmail)));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.ConsigneeEmailTextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.ABL_ConsigneeEmail)));
	});
}
