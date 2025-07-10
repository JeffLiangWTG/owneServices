using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC170CHouseConsignmentProvider : ConsignmentProviderBase, ICC170CHouseConsignment
{
	public CC170CHouseConsignmentProvider(int sequenceNumber, NctsBill bill, ICC170C cc170cProvider)
		: base(bill)
	{
		this.cc170cProvider = Argument.NotNull(cc170cProvider, nameof(cc170cProvider));
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly ICC170C cc170cProvider;

	public string SequenceNumber { get; }

	protected override string GetDepartureTransportMeansTransportMode() =>
		cc170cProvider.Consignment is ICC170CConsignment consignment
		&& consignment.DepartureTransportMeans.IsNullOrEmpty()
		&& !transportMeansProvider.TransportTypeAtDeparture.IsEmpty
			? consignment.InlandModeOfTransport
			: null;
}
