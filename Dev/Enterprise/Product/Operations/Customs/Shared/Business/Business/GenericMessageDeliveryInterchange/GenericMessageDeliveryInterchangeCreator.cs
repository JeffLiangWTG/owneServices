using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business
{
	public class GenericMessageDeliveryInterchangeCreator : IGenericMessageDeliveryInterchangeCreator
	{
		public string[] SupportedInterchangeTypes => new[]
		{
			GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
		};

		public IEDIInterchange Create(ZGuid branchPK, ZString senderId, ZString recipientId, ZString interchangeType, ZString body)
		{
			var factory = new BusinessObjectFactory();
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_GB = branchPK;
			interchange.EI_From = senderId;
			interchange.EI_To = recipientId;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_BodyText = body;
			try
			{
				factory.Save();
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			return interchange;
		}
	}
}
