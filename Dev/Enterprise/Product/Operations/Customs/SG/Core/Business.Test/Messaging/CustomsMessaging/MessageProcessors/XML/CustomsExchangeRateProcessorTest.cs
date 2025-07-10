using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class CustomsExchangeRateProcessorTest : BaseResponseSectionProcessorTest
	{
		[TestDate(2020, 6, 4)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12345";
			var messageText = LoadXmlFile("CustomsExchangeRate.xml");
			var incomingMessage = newFactory.New<SGXmlEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			newFactory.Save();
			using (var stream = incomingMessage.GetEM_MessageTextReader())
			{
				var serializer = ZXmlSerializer.New(typeof(TradenetResponse));
				var tradenetResponse = serializer.Deserialize(stream) as TradenetResponse;
				MessageProcessor = GetMessageProcessor(tradenetResponse);
				MessageProcessor.ProcessMessage(incomingMessage);
			}

			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);
			var interchanges = newFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.RefDataRepoMessage));
			AssertEquals(1, interchanges.Length);
			AssertContains("IFTRIN", interchanges[0].EI_BodyText);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new CustomsExchangeRateProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.CustomsExchangeRate);
	}
}
