using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS20 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS20, IBIRDHeaderRecord
	{
		#region IBIRDDecRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			BIRDTransportMode.SetTransportMode(declaration, ModeOfTransportationMOTCode, notifications);

			if (ImportingVesselName.StartsWith("FTZ", System.StringComparison.OrdinalIgnoreCase))
			{
				declaration.US_FTZNo = ImportingVesselName;
			}
			else
			{
				declaration.JE_VesselName = ImportingVesselName;
			}

			declaration.US_SchDArrival = DistrictPortOfUnlading;

			declaration.JE_DateOfArrival = DateOfImportation;

			declaration.US_ClientBranchDesignation = ClientBranchDesignation;

			declaration.JE_VoyageFlightNo = VoyageFlightTripManifestNumber;

			declaration.US_EntryDate = EstimatedDateOfArrival;

			declaration.US_US_NKLocationOfGoods = LocationOfGoods;

			declaration.US_FixRecon = true;

			declaration.US_NAFTAReconIndicator = TradeAgreementReconciliationIndicator == "1";

			declaration.US_OtherReconIndicator = OtherReconciliationIndicator.IsEmpty ? ReconIssueCodeList.Codes.NotApplicable : ReconIssueCodeList.ConvertFromENSIssueCode(OtherReconciliationIndicator);
		}

		#endregion
	}
}
