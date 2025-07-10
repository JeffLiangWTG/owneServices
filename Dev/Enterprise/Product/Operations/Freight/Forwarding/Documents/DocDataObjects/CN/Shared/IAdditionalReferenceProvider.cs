using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	interface IAdditionalReferenceProvider
	{
		ZString ShipperReference { get; }
		ZString FreightForwarderReference { get; }
		ZString CarrierContractNumber { get; }
		ZString QuotationNumber { get; }
	}
}
