using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public partial class ASESE11 : Abstract.ASESE11, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_EntryDateElectionCode = EntryDateElectionCode;

			if (!ElectedEntryDate.IsEmpty && ElectedEntryDate.IsValid)
			{
				if (EntryDateElectionCode == EntryDateElectionCodeList.Codes.PresentationDate || declaration.IsWeeklyEstimateFilingDate)
				{
					declaration.US_PresentationDate = ElectedEntryDate;
				}
				else if (EntryDateElectionCode == EntryDateElectionCodeList.Codes.ArrivalDate)
				{
					declaration.US_EntryDate = ElectedEntryDate;
				}
				else
				{
					declaration.US_EstimatedEntryDate = ElectedEntryDate;
				}
			}

			declaration.US_US_NKLocationOfGoods = LocationOfGoodsFIRMS;
			declaration.US_US_NKCentralizedExamSite = ElectedExamSiteFIRMS;
			declaration.JE_VoyageFlightNo = VoyageFlightTripManifestNumber;
			declaration.US_GeneralOrderNo = GeneralOrderGONumber;
			declaration.US_WHSEntryFilerCode = OriginatingWarehouseEntryFilerCode;
			declaration.US_WHSEntryNumber = OriginatingWarehouseEntryNumber;

			if (declaration.US_EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				declaration.US_FTZNo = ConveyanceNameOrFTZZoneID.SubstringSafe(3, declaration.US_FTZNoInfo.MaxLength);
			}
			else
			{
				declaration.JE_VesselName = ConveyanceNameOrFTZZoneID;
			}
		}

		#endregion
	}
}
