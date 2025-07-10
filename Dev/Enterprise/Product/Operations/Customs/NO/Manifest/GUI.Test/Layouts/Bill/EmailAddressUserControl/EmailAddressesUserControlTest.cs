using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(EmailAddressesUserControl))]
sealed class EmailAddressesUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new EmailAddressesUserControl();
		_ = userControl.AssertThisControl(x =>
			x.WithCaptionRenderingEnabled()
		);
		_ = userControl.AssertContainsControl<ZArchitecture.GUI.ZGroupBox>(nameof(userControl.EmailAddressesGroupBox),
			x => x
			.WithCaption("Email addresses for border passing confirmation:"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.Email1TextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.EmailAddress1))
			.WithCaption("Address 1"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.Email2TextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.EmailAddress2))
			.WithCaption("Address 2"));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.Email3TextBox),
			x => x
			.WithBindTo(nameof(AsycudaBill.EmailAddress3))
			.WithCaption("Address 3"));
	});
}
