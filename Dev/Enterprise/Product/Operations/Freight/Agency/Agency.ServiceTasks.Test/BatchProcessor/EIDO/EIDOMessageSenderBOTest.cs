using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[EIDOMessagingConfiguration]
	internal class EIDOMessageSenderBOTest : BaseInterchangeSenderTest
	{
		#region Implementation
		protected override EDIInterchange GetNewEDIInterchangeReadyToSend()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(GetNewEDIMessageReadyToSend());
			return new EIDOInterchangeProvider(messages, new FailedMessageList()).Interchanges[0];
		}

		protected override EDIMessage GetNewEDIMessageReadyToSend()
		{
			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_OH_DeliveryAgent = principal.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			IEIDOMessageBuilder builder = EIDOMessageBuilderFactory.GetNewBuilder();
			IEIDOMessagingData data = EIDOShipmentMessagingData.NewOriginal(container);
			return EIDOMessage.New(container, data.MessageFunction, builder.GenerateMessageText(data));
		}

		protected override BaseInterchangeSender GetNewSender()
		{
			return new EIDOInterchangeSender();
		}
		#endregion
	}
}
