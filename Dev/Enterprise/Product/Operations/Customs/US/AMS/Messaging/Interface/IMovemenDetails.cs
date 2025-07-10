using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IMovemenDetails
	{
		ZString PreviousInBondNumber { get; }
		ZInt InBondQuantity { get; }
		ZString InbondEntryType { get; }
		ZBool IsBTAFDA { get; }
		ZString ConventionalInbondNumber { get; }
		ZString InbondCarrierCode { get; }
		ZString USPortOfDestination { get; }
		ZString ForeignDestination { get; }
		ZInt Value { get; }
		ZString BondedCarrierID { get; }
		ZString PaperlessInbondNumber { get; }
		ZString ExportVesselName { get; }

		ZDateTime ArrivalDateTime { get; }
		ZDateTime ExportDateTime { get; }

		ZString TOLInBondCarrierCode { get; }
		ZString TOLBondedCarrierID { get; }
		ZDateTime TOLDateTime { get; }
		ZString TOLCityName { get; }
		ZString TOLStateCode { get; }
	}
}
