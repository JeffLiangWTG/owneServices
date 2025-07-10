using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.GUI
{
	public class AcceptAllPreScreeningErrorsAsAcceptedMenuItem : BaseHVLVMenuItem
	{
		public AcceptAllPreScreeningErrorsAsAcceptedMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("36d63ba5-0272-4e52-b57e-3b627c35a996", "Accept All Pre-Screening Errors as Entered"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
			var consignments = Header.ConsignmentsForBinding;

			if (consignments.All(c => c.HasUnknownPrescreeningStatus))
			{
				Globals.Message.ShowInformation(PreScreeningNotRunMessage, PreScreeningNotRunCaption);
				return;
			}

			if (!consignments.Any(c => c.HasFailedPreScreeningStatus))
			{
				Globals.Message.ShowInformation(NoFALConsignmentMessage, NoFALConsignmentCaption);
				return;
			}

			if (Globals.Message.Show(FailedAcceptedAsEnteredMessage, ConfirmingPreScreeningStatusCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
			{
				consignments.Where(c => c.HVC_PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Failed).ForEach(c => c.AcceptFailedPreScreeningAsEntered());
			}
		};

		string PreScreeningNotRunCaption => Res.GetString("3b364abe-39ee-4c67-97ad-09dc147c08b1", "Run HVLV Pre-Screening");

		string PreScreeningNotRunMessage => Res.GetString("7c7c0fd9-bd75-4cfa-8561-da8872ab5a4b", "HVLV Pre-Screening hasn't been run on this shipment yet. Please run Pre-Screen HVLV Details action first.");

		string NoFALConsignmentCaption => Res.GetString("a087405e-9bb6-4a92-8945-d734a78cdfc7", "No FAL Consignment");

		string NoFALConsignmentMessage => Res.GetString("72773798-8857-480a-8695-c6592354beb2", "There's no Consignment within this shipment containing FAL Pre-Screening Status. This action will not proceed.");

		string FailedAcceptedAsEnteredMessage => Res.GetString("5ebdeb74-cc1a-434c-90a3-6233905a8ebe", "Click OK to proceed with updating all Consignments with FAL Pre-Screening Status to FAE: Failed Accepted as Entered");

		string ConfirmingPreScreeningStatusCaption => Res.GetString("33ef42a0-de8d-4199-8787-3d257091c785", "Confirming pre-screening status");
	}
}
