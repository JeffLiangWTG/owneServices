using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IOUTR01
	{
		ZString CarrierCode { get; }
		ZString CBPDistrictPort { get; }
		ZString VesselName { get; }
		ZString VoyageNumber { get; }
		ZString ManifestSequenceNumber { get; }
		ZDate EstimatedDate { get; }
	}

	public interface IOUTR02Combine
	{
		ZString Data { get; }
	}

	public interface IOUTR03
	{
		ZString Remarks { get; }
	}

	public interface IOUTR04
	{
		ZString DDPP { get; }
		ZString FilerEntryNumber { get; }
		ZString CarrierCode { get; }
		ZString VesselName { get; }
		ZString VoyageFlightNumber { get; }
		ZString BillOfLadingNumber { get; }
		ZDecimal EnteredQuantity { get; }
	}

	public interface IOUTR06
	{
		ZString EventCode { get; }
		ZDate ActionDate { get; }
		ZString ActionTime { get; }
	}
}
