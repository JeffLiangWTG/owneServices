using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
sealed class CompanyCredentialsDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertThisControl(x => x.WithBindingSource<Business.GlbCompanyWrapper>());
	}

	public void TestCompanyCredentialsGroupBox()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<ZGroupBox>(nameof(CompanyCredentialsDetailsUserControl.CompanyCredentialsGroupBox), x => x.WithCaption("Digital Customs Exchange Details"));
	}

	public void TestCertificateLoaderUserControl()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<DigitalCertificateControl_p12>(nameof(CompanyCredentialsDetailsUserControl.CertificateLoaderUserControl), x => x
			.WithBindTo("Credential.GP_Certificate")
			.WithStrategy("FileFilter", "p12 certificates (*.p12)|*.p12", (self, control, controlName) => AssertEquals($"Control {controlName} {self.name}", self.expected, control.FileFilter)));
	}

	public void TestStatusTextBox()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(CompanyCredentialsDetailsUserControl.StatusTextBox), x => x
			.WithBindTo("Credential.GP_PasswordStatus")
			.WithCharacterCasing(CharacterCasing.Normal));
	}

	public void TestCurrentPasswordTextBox()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(CompanyCredentialsDetailsUserControl.CurrentPasswordTextBox), x => x
			.WithBindTo("Credential.CurrentDecryptedCertificatePassphrase")
			.WithCharacterCasing(CharacterCasing.Normal)
			.WithPasswordChar('*'));
	}

	public void TestUserIDTextBox()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(CompanyCredentialsDetailsUserControl.UserIDTextBox), x => x
			.WithBindTo("Credential.GP_UserID")
			.WithCharacterCasing(CharacterCasing.Normal));
	}

	public void TestExpiryDateTextBox()
	{
		using var control = new CompanyCredentialsDetailsUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(CompanyCredentialsDetailsUserControl.ExpiryDateTextBox), x => x
			.WithBindTo("Credential.GP_ExpiryDate")
			.WithCharacterCasing(CharacterCasing.Normal));
	}
}
