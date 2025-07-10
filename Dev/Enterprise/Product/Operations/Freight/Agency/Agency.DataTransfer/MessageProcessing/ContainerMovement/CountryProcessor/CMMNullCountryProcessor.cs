using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	internal sealed class CMMNullCountryProcessor : ICMMCountryProcessor
	{
		void ICMMCountryProcessor.UpdateContainer(CMMMessageContainer containerData, AgencyShipmentContainer container)
		{
		}

		void ICMMCountryProcessor.ValidateContainer(CMMMessageContainer cmmMessageContainer, AgencyShipmentContainer container)
		{
		}
	}
}
