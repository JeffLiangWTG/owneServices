using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public class SupportingDocumentFieldsControlBag : ControlBag
{
	public SupportingDocumentFieldsControlBag()
	{
		ValueConvertToLocalCurrencyControl = RegisterControl(nameof(SupportingDocumentFieldsControl.ValueConvertToLocalCurrencyControl));
		SupDocReference2TextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.SupDocReference2TextBox));
		SupDocDescriptionTextBox = RegisterControl(nameof(SupportingDocumentFieldsControl.SupDocDescriptionTextBox));
	}

	[ThreadStatic]
	static SupportingDocumentFieldsControlBag instance;

	public static SupportingDocumentFieldsControlBag Instance => instance ?? (instance = new SupportingDocumentFieldsControlBag());

	protected override Control CreateTemplate() => new SupportingDocumentFieldsControl();

	public ControlReference ValueConvertToLocalCurrencyControl { get; }
	public ControlReference SupDocReference2TextBox { get; }
	public ControlReference SupDocDescriptionTextBox { get; }
}
