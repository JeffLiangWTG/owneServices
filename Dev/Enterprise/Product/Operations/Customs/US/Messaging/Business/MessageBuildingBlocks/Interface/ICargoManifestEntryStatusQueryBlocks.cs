using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface ICargoManifestEntryReleaseStatusQuery
	{
		ZString EntryFilerCode { get; set; }
		ZString EntryNumber { get; set; }
		ZString InbondNumber { get; set; }
		ZString IssuerCodeOfBillOfLadingNumber { get; set; }
		ZString BillOfLadingNumber { get; set; }
		ZString AirWaybillNumber { get; set; }
		ZString HouseAirWaybillNumber { get; set; }
		ZString RequestForRelatedBOLIndicator { get; set; }
		ZString RequestForBillOfLadingAndEntryDataIndicator { get; set; }
		ZString LimitOutputOption { get; set; }
	}

	public interface ICountryOfOriginTariffDetailsBlock
	{
		ZInt RecordControlNumber { get; }
		ZString CountryOfOrigin { get; }
		ZString TariffNumber { get; }
	}

	public interface ICargoManifestQueryInputR1Block
	{
		ZString BillOfLadingNumber { get; }
		ZString AirWaybillNumber { get; }
		ZString HouseAirWaybillNumber { get; }
	}
}
