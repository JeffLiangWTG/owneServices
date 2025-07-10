using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestConsolNumberAndContainerNumberOnEventMatchToRightContainer()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00001535";
			var consol1container1 = consol1.Containers.AddNew();
			consol1container1.JC_ContainerNum = "OOCL0000006";
			var consol1container2 = consol1.Containers.AddNew();
			consol1container2.JC_ContainerNum = "OOCL0000011";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C00001536";
			var consol2container1 = consol2.Containers.AddNew();
			consol2container1.JC_ContainerNum = "OOCL0000011";

			Factory.SaveForTesting();

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var message = GetQueuedUniversalEventMessage(resourceRetriever.GetString(GetResourcePathFor("UniversalEventWithDataTargetAndContainerNumber.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Container 'OOCL0000011'.
".Trim(), message.GetLogNoteText());

				consol1container2.Reload();
				var logs = consol1container2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookedCode));
				AssertEquals("[BKD] - Booked event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
Container Number - OOCL0000011
MBOL Origin UNLOCO - AUSYD
MBOL Destination UNLOCO - ZAJNB
".Trim(), actualContextItems);
			});
		}

		string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.TestFiles.{fileName}";
		}
	}
}
