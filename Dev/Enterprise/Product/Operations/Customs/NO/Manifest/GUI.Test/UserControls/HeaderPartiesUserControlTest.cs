using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(HeaderPartiesUserControl))]
sealed class HeaderPartiesUserControlTest : TestCaseWithFactory
{
	public void TestIAdditionalTabPage() => CombineAssertions(() =>
	{
		using var control = new HeaderPartiesUserControl();
		var additionalTabPage = (IAdditionalTabPage)control;

		AssertEquals("TabPageCaption", "Header Parties", additionalTabPage.AdditionalTabPageCaption.Caption);
		AssertEquals("Tab Page Sequence", 1, additionalTabPage.TabPageSequence);
		AssertEquals("Visibility", true, additionalTabPage.AdditionalControlVisibility.isVisible(null));
		AssertEquals("Control Reference", control, additionalTabPage.AdditionalTabPageUserControl);
	});

	public void TestControls()
	{
		using var userControl = new HeaderPartiesUserControl();
		_ = userControl.AssertContainsControl<ZArchitecture.GUI.DynamicLayoutPanel>(nameof(userControl.DynamicHeaderPartiesPanel),
				x => x.WithBindTo(nameof(AsycudaManifestHeader.MasterBill)));
	}
}
