using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	enum InterestedPartyTypes
	{
		None,
		Agent,
		Airline,
		NotifyParty,
		Broker,
		Consignee,
		Consignor,
		Trucker,
	}

	struct InterestedParty
	{
		public InformationResult<ZString> Id;
		public InformationResult<InterestedPartyTypes> Type;
		public InformationResult<ZString> ServiceIndicator;
		public InformationResult<ZString> Reference;
		public InformationResult<ZString> Remarks;
	}
}
