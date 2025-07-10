using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
{
	public void TestPaymentSeparatorUserControl()
	{
		AssertType<SeparatorUserControl>(control.PaymentSeparatorUserControl);
	}

	public void TestPaymentSeparatorUserControl_Caption()
	{
		AssertEquals("In NL, we override the caption.", "Payment", control.PaymentSeparatorUserControl.CaptionResourceString.Caption);
	}

	public void TestSupportingInformationUserControl()
	{
		AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
	}

	public void TestPaymentPartyEORINumberTextBox()
	{
		AssertType<ZTextBox>(control.PaymentPartyEORINumberTextBox);
	}

	public void TestPaymentPartyEORINumberTextBox_ReadOnly()
	{
		AssertReadOnly(control.PaymentPartyEORINumberTextBox);
	}

	public void TestVATPartyTaxNumberTextBox()
	{
		AssertType<ZTextBox>(control.VATPartyTaxNumberTextBox);
	}

	public void TestVATPartyTaxNumberTextBox_ReadOnly()
	{
		AssertReadOnly(control.VATPartyTaxNumberTextBox);
	}

	public void TestCustomsAccountTextBox()
	{
		AssertType<ZTextBox>(control.CustomsAccountTextBox);
	}

	public void TestCustomsAccountTextBox_ReadOnly()
	{
		AssertReadOnly(control.CustomsAccountTextBox);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new MiscOptionsLayoutUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	MiscOptionsLayoutUserControl control;

	void AssertReadOnly(ZTextBox textBox)
	{
		using (var form = new ZForm(Factory.New<JobDeclaration>()))
		{
			form.Controls.Add(control);
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals("visible", true, textBox.Visible);
				AssertEquals("read-only", true, textBox.ReadOnly);
			});
		}
	}
}
