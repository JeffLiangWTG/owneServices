using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class SendACASReportMenuItem : SendACASMessageMenuItem
	{
		public SendACASReportMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("7e2f94ad-12d1-44d4-b273-75799820d17e", "Send ACAS Report"), shipment)
		{
		}

		protected override bool CheckCanSendACASMessage()
		{
			return UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(shipment, SaveFormBeforeSendACASMessage)
				&& UserPromptCheckingHelper.CheckHasAnyActiveConsignmentOrNotify(Header.Consignments.OfType<HVLVConsignment>())
				&& UserPromptCheckingHelper.CheckHasAnyConsignmentAvailableToSendOriginalACASReport(ConsignmentsForACAS);
		}

		public override ACASReportAction ACASAction => ACASReportAction.SendOriginal;

		protected override string SaveFormBeforeSendACASMessage => Res.GetString("a7ea0d1b-c9c8-4644-adbc-d412e2ce2dd9", "Please save the form before sending ACAS Report.");

		protected override string ACASMessageStatus => ZString.Empty;
	}
}
