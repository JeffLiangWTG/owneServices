using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class SendACASAcknowledgementMenuItem : SendACASMessageMenuItem
	{
		public SendACASAcknowledgementMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("1689eea9-7227-4f08-a9ba-6050e2261440", "Send ACAS Acknowledgement"), shipment)
		{
		}

		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = Visible && ConsignmentsForACAS.Any();
		}

		public override ACASReportAction ACASAction => ACASReportAction.SendAcknowledgement;

		protected override string SaveFormBeforeSendACASMessage => Res.GetString("7885ea87-b664-48d3-9d2f-c43d2fe3fab3", "Please save the form before sending ACAS Acknowledgement.");

		protected override string ACASMessageStatus => HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
	}
}
