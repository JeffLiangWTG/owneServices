namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class InterestedPartyTypeConverter : ListConverter<InterestedPartyTypes, PartnersInterestedPartiesInterestedPartyTypeList>
	{
		public override PartnersInterestedPartiesInterestedPartyTypeList Convert(InterestedPartyTypes data, FormattingResult formattingResult)
		{
			PartnersInterestedPartiesInterestedPartyTypeList result = null;
			switch (data)
			{
				case InterestedPartyTypes.Agent:
					result = PartnersInterestedPartiesInterestedPartyTypeList.AgentAgency;
					break;
				case InterestedPartyTypes.Airline:
					result = PartnersInterestedPartiesInterestedPartyTypeList.Airline;
					break;
				case InterestedPartyTypes.NotifyParty:
					result = PartnersInterestedPartiesInterestedPartyTypeList.AlsoNotifyParty;
					break;
				case InterestedPartyTypes.Broker:
					result = PartnersInterestedPartiesInterestedPartyTypeList.Broker;
					break;
				case InterestedPartyTypes.Consignee:
					result = PartnersInterestedPartiesInterestedPartyTypeList.Consignee;
					break;
				case InterestedPartyTypes.Consignor:
					result = PartnersInterestedPartiesInterestedPartyTypeList.Shipper;
					break;
				case InterestedPartyTypes.Trucker:
					result = PartnersInterestedPartiesInterestedPartyTypeList.Trucker;
					break;
			}

			return result;
		}
	}
}
