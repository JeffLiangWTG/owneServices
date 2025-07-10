using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class MiscOptionsControlBag : ControlBag
{
	public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	public MiscOptionsControlBag() : base()
	{
		PaymentSeparatorUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentSeparatorUserControl));
		SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.SupportingInformationUserControl));
		PaymentPartyEORINumberTextBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.PaymentPartyEORINumberTextBox));
		VATPartyTaxNumberTextBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.VATPartyTaxNumberTextBox));
		CustomsAccountTextBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.CustomsAccountTextBox));
	}

	public ControlReference PaymentSeparatorUserControl { get; }
	public ControlReference SupportingInformationUserControl { get; }
	public ControlReference PaymentPartyEORINumberTextBox { get; }
	public ControlReference VATPartyTaxNumberTextBox { get; }
	public ControlReference CustomsAccountTextBox { get; }

	protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
}
