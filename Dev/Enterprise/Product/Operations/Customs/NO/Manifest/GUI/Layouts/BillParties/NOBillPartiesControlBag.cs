using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class NOBillPartiesControlBag : ControlBag
{
	NOBillPartiesControlBag()
	{
		RepresentativeSeparatorUserControl = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeSeparatorUserControl));
		RepresentativeAddressControl = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeAddressControl));
		RepresentativeNameTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeNameTextBox));
		RepresentativeStreet1TextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeStreet1TextBox));
		RepresentativeStreet2TextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeStreet2TextBox));
		RepresentativeCityTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeCityTextBox));
		RepresentativeCountryCodeFindBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeCountryCodeFindBox));
		RepresentativeStateDropEdit = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeStateDropEdit));
		RepresentativePhoneTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativePhoneTextBox));
		RepresentativePostCodeTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativePostCodeTextBox));
		RepresentativeRegNoTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeRegNoTextBox));
		RepresentativeEmailTextBox = RegisterControl(nameof(NOBillPartiesUserControl.RepresentativeEmailTextBox));
		ShipperEmailTextBox = RegisterControl(nameof(NOBillPartiesUserControl.ShipperEmailTextBox));
		ConsigneeEmailTextBox = RegisterControl(nameof(NOBillPartiesUserControl.ConsigneeEmailTextBox));
	}

	public static NOBillPartiesControlBag Instance => instance ??= new NOBillPartiesControlBag();

	[ThreadStatic]
	static NOBillPartiesControlBag instance;

	protected override Control CreateTemplate() => new NOBillPartiesUserControl();

	public ControlReference RepresentativeSeparatorUserControl { get; }
	public ControlReference RepresentativeAddressControl { get; }
	public ControlReference RepresentativeNameTextBox { get; }
	public ControlReference RepresentativeStreet1TextBox { get; }
	public ControlReference RepresentativeStreet2TextBox { get; }
	public ControlReference RepresentativeCityTextBox { get; }
	public ControlReference RepresentativeCountryCodeFindBox { get; }
	public ControlReference RepresentativeStateDropEdit { get; }
	public ControlReference RepresentativePostCodeTextBox { get; }
	public ControlReference RepresentativePhoneTextBox { get; }
	public ControlReference RepresentativeRegNoTextBox { get; }
	public ControlReference ShipperEmailTextBox { get; }
	public ControlReference RepresentativeEmailTextBox { get; }
	public ControlReference ConsigneeEmailTextBox { get; }
}
