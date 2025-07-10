using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class SuretyToBrokerNoticeMessageProcessorHelperTest : TestCaseWithFactory
	{
		public void TestIsSuretyToBrokerNoticeMessage()
		{
			const string incomingEvent = @"<UniversalEvent>
	  <Event>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Key>B00158390</Key>
			  <Type>CustomsDeclaration</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<EventTime>2014-09-09T09:30:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
		  <Reason>You are not registered with eHub. Contact WTG to register.</Reason>
		  <MessageType>eBond Message to Surety Agent</MessageType>
		</EventParameters>
	  </Event>
</UniversalEvent>";
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			Assert(SuretyToBrokerNoticeMessageProcessorHelper.IsSuretyToBrokerNoticeMessage(xmlEvent as Event));
		}
	}
}
