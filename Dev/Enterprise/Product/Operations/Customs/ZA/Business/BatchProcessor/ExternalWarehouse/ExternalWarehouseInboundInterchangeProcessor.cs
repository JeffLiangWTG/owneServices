using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ExternalWarehouseInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public ExternalWarehouseInboundInterchangeProcessor() : base(new ZString[] { GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse })
		{
		}

		protected override ZString GetStatusForProcessFailure(EDIInterchange interchange)
		{
			return interchange.EI_Status == EDIInterchange.Status.Error ? EDIInterchange.Status.Error : EDIInterchange.Status.Failed;
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return customsInformationProcessor ?? (customsInformationProcessor = new ExternalWarehouseMessageCreator());
		}
		IInboundMessageCreator customsInformationProcessor;

		class ExternalWarehouseMessageCreator : IInboundMessageCreator
		{
			public void CreateMessagesForInterchange(EDIInterchange interchange)
			{
				GenerateMessage(interchange);
			}

			protected void GenerateMessage(EDIInterchange interchange)
			{
				var status = EDIInterchangeStatusList.Codes.Error;

				try
				{
					var xmlMessage = XElement.Parse(interchange.EI_BodyText);
					var isExternalWarehouseBatch = xmlMessage.Name.LocalName.Equals("ExternalWarehouseBatch");

					if (isExternalWarehouseBatch)
					{
						EDIMessage newEDIMessage = interchange.ContainedMessages.AddNew(typeof(EWHMessage));
						newEDIMessage.EM_MessageText = xmlMessage.ToString();
						newEDIMessage.EM_IsTestMessage = false;
						newEDIMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
						newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
						status = EDIInterchangeStatusList.Codes.Received;
					}
					else
					{
						status = EDIInterchangeStatusList.Codes.Error;
						interchange.Logs.AddNew(Events.ErrorReport, "Invalid xml - Cannot find ExternalWarehouseBatch node");
					}
				}
				catch
				{
					status = EDIInterchangeStatusList.Codes.Error;
					interchange.Logs.AddNew(Events.ErrorReport, "Invalid xml - cannot process");
				}

				interchange.EI_Status = status;
			}
		}
	}
}
