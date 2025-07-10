using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery)]
	public partial class ACEQWR1 : MessageBlock, ICargoManifestEntryReleaseStatusQuery, ICargoManifestQueryInputR1Block
	{
		#region ICargoManifestEntryReleaseStatusQuery

		ZString ICargoManifestEntryReleaseStatusQuery.EntryFilerCode
		{
			get { return EntryFilerCode; }
			set { EntryFilerCode = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.EntryNumber
		{
			get { return EntryNumber; }
			set { EntryNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.InbondNumber
		{
			get { return InbondNumber; }
			set { InbondNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.IssuerCodeOfBillOfLadingNumber
		{
			get { return IssuerCodeOfBillOfLadingNumber; }
			set { IssuerCodeOfBillOfLadingNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.BillOfLadingNumber
		{
			get { return BillOfLadingNumber; }
			set { BillOfLadingNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.AirWaybillNumber
		{
			get { return AirWaybillNumber; }
			set { AirWaybillNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.HouseAirWaybillNumber
		{
			get { return HouseAirWaybillNumber; }
			set { HouseAirWaybillNumber = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.RequestForRelatedBOLIndicator
		{
			get { return RequestForRelatedBOLIndicator; }
			set { RequestForRelatedBOLIndicator = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.RequestForBillOfLadingAndEntryDataIndicator
		{
			get { return RequestForBillOfLadingAndEntryDataIndicator; }
			set { RequestForBillOfLadingAndEntryDataIndicator = value; }
		}

		ZString ICargoManifestEntryReleaseStatusQuery.LimitOutputOption
		{
			get { return LimitOutputOption; }
			set { LimitOutputOption = value; }
		}

		#endregion

		#region ICargoManifestQueryInputR1Block

		ZString ICargoManifestQueryInputR1Block.BillOfLadingNumber { get { return BillOfLadingNumber; } }
		ZString ICargoManifestQueryInputR1Block.AirWaybillNumber { get { return AirWaybillNumber; } }
		ZString ICargoManifestQueryInputR1Block.HouseAirWaybillNumber { get { return HouseAirWaybillNumber; } }

		#endregion
	}
}