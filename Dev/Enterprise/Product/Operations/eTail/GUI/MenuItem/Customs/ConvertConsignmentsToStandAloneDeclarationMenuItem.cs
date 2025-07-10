using System;
using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class ConvertConsignmentsToStandAloneDeclarationMenuItem : BaseHVLVMenuItem
	{
		public ConvertConsignmentsToStandAloneDeclarationMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("22664d8e-d364-4d13-adbe-031ec40331ca", "Convert Consignments to Stand Alone Declarations"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
			if (shipment.ArrivalConsol == null)
			{
				Globals.Message.ShowError(ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentMissingTransportDetails);
			}
			else if (shipment.HasChanges || shipment.ArrivalConsol.HasChanges)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("8d255c72-bd94-47fc-bb29-f1d0006bbc15", "Please save any changes made before creating Stand Alone Declarations"));
			}
			else if (shipment.Destination == null)
			{
				Globals.Message.ShowError(Res.GetString("9c41dac6-daa4-42df-8f13-10225a3d0466", "No destination set for the current shipment."));
			}
			else if (!ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclarationSupportedForDestinationCountry(shipment.Destination?.Country?.Code))
			{
				Globals.Message.ShowError(ConvertToStandAloneDeclarationResponse.ErrorMessages.ShipmentDestinationNotSupported);
			}
			else if (shipment.HVLVConsignments.OfType<HVLVConsignment>().All(c => c.ReleaseStatusForCurrentDirection == HVLVReleaseStatus.Cleared))
			{
				Globals.Message.ShowError(Res.GetString("e5970cef-b641-4aa8-b07a-a9e5c8bf7826", "All consignments are cleared. There are no uncleared consignments to convert to Stand Alone Declarations."));
			}
			else if (!shipment.HVLVConsignments.OfType<HVLVConsignment>().Any(c => ((c.ReleaseStatusForCurrentDirection != HVLVReleaseStatus.Cleared && !c.ReleaseStatusForCurrentDirection.IsEmpty) || c.DirectionOfTrade == MasterFiles.Business.Directions.Unknown) && !c.HasDeclarationForCurrentDirection))
			{
				Globals.Message.ShowError(Res.GetString("eed44970-2a6d-4c44-af11-50e2d9469671", "There are no eligible consignments to convert to Stand Alone Declarations."));
			}
			else if (Header is HVLVConsignmentHeader && PreValidateData())
			{
				using (var form = new HVLVConsignmentsToStandAloneDeclarationsForm(Header))
				{
					ZFormModaliser.ShowDialogAndDispose(form);
				}
			}
		};

		bool CheckShipmentRequiresWaybillValidation()
		{
			return !shipment.IsShipmentDestinationUS()
				|| UserPromptCheckingHelper.US.CheckWayBill(Header.Consignments.OfType<HVLVConsignment>(), Res.GetString("79415dcd-b591-4e07-88a7-2a37da258986", "Stand Alone Declarations"));
		}

		protected override bool PreValidateData()
		{
			return CheckShipmentRequiresWaybillValidation();
		}
	}
}
