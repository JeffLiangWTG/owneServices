using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class SendACASAmendmentMenuItem : SendACASMessageMenuItem
	{
		public SendACASAmendmentMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("01c53ba2-eed6-4d97-a661-230ab001524a", "Send ACAS Amendment"), shipment)
		{
		}

		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = Visible && ConsignmentsForACAS.Any();
		}

		public override ACASReportAction ACASAction => ACASReportAction.SendAmendment;

		protected override string SaveFormBeforeSendACASMessage => Res.GetString("0be14ba0-f669-4a6e-871b-4422c59d94fe", "Please save the form before sending ACAS Amendment.");

		protected override string ACASMessageStatus => HVLVACASMessageStatusList.Codes.AmendmentRequired;
	}
}
