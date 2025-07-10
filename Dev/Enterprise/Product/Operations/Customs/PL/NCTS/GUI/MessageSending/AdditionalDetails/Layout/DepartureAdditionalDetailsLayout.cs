using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public class DepartureAdditionalDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	static PanelLayout CreateLayout()
	{
		var builder = new DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.AmendmentTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.JustificationTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TC11DeliveryDateTimeOffsetEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.AdditionalTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DepartureOfficeOfEnquiryCodeFindBox, ControlWidthClass.Auto);
		builder.AddControlBehaviour<ZDateTimeOffsetEdit>(commonBag.TC11DeliveryDateTimeOffsetEdit, UpdateTC11DeliveryDateTimeOffsetEditControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.AdditionalTextBox, UpdateAdditionalTextBoxControlBehaviourAction);
		builder.AddControlBehaviour<ZCodeFindBox>(commonBag.DepartureOfficeOfEnquiryCodeFindBox, UpdateDepartureOfficeOfEnquiryCodeFindBoxControlBehaviourAction);

		builder.AddColumn();
		builder.Add(commonBag.ActualConsigneeDocAddressControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ActualOfficeOfDestinationCodeFindBox, ControlWidthClass.Auto, commonBag.DepartureOfficeOfEnquiryCodeFindBox);
		builder.AddControlBehaviour<ZCodeFindBox>(commonBag.ActualOfficeOfDestinationCodeFindBox, UpdateActualOfficeOfDestinationCodeFindBoxControlBehaviourAction);

		void UpdateTC11DeliveryDateTimeOffsetEditControlBehaviourAction(Control control, MessageSendingObject messageSendingObject)
		{
			control.Left = ControlDpiScalingHelper.MarkAsScaled(213);
		}

		void UpdateAdditionalTextBoxControlBehaviourAction(Control control, MessageSendingObject messageSendingObject)
		{
			control.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			control.Left = ControlDpiScalingHelper.MarkAsScaled(111);
		}

		void UpdateDepartureOfficeOfEnquiryCodeFindBoxControlBehaviourAction(Control control, MessageSendingObject messageSendingObject)
		{
			control.Top = ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			control.Width = ControlDpiScalingHelper.MarkAsScaled(387);
			control.Left = ControlDpiScalingHelper.MarkAsScaled(111);
		}

		void UpdateActualOfficeOfDestinationCodeFindBoxControlBehaviourAction(Control control, MessageSendingObject messageSendingObject)
		{
			control.Width = ControlDpiScalingHelper.MarkAsScaled(334);
		}

		return builder.Build();
	}
}
