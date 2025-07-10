using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public interface ICMMCountryProcessor
	{
		void UpdateContainer(CMMMessageContainer containerData, AgencyShipmentContainer container);
		void ValidateContainer(CMMMessageContainer cmmMessageContainer, AgencyShipmentContainer container);
	}
}

