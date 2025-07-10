using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class DepartureAdditionalDetailsControlBag : ControlBag
{
	DepartureAdditionalDetailsControlBag()
	{
		AmendmentTypeDropEdit = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.AmendmentTypeDropEdit));
		PresentationDateAndTimeDateTimeOffsetEdit = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.PresentationDateAndTimeDateTimeOffsetEdit));
		JustificationTextBox = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.JustificationTextBox));
		TC11DeliveryDateTimeOffsetEdit = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.TC11DeliveryDateTimeOffsetEdit));
		AdditionalTextBox = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.AdditionalTextBox));
		ActualConsigneeDocAddressControl = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.ActualConsigneeDocAddressControl));
		DepartureOfficeOfEnquiryCodeFindBox = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.DepartureOfficeOfEnquiryCodeFindBox));
		ActualOfficeOfDestinationCodeFindBox = RegisterControl(nameof(DepartureAdditionalDetailsUserControl.ActualOfficeOfDestinationCodeFindBox));
	}

	public static DepartureAdditionalDetailsControlBag Instance => instance ?? (instance = new DepartureAdditionalDetailsControlBag());

	[ThreadStatic]
	static DepartureAdditionalDetailsControlBag instance;

	protected override Control CreateTemplate() => new DepartureAdditionalDetailsUserControl();

	public ControlReference AmendmentTypeDropEdit { get; }
	public ControlReference PresentationDateAndTimeDateTimeOffsetEdit { get; }
	public ControlReference JustificationTextBox { get; }
	public ControlReference AdditionalTextBox { get; }
	public ControlReference TC11DeliveryDateTimeOffsetEdit { get; }
	public ControlReference ActualConsigneeDocAddressControl { get; }
	public ControlReference DepartureOfficeOfEnquiryCodeFindBox { get; }
	public ControlReference ActualOfficeOfDestinationCodeFindBox { get; }
}
