using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingContainerEventMessagingHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[TestDate(2021, 1, 1)]
		public void TestUniversalEvent()
		{
			var today = ZDateTime.Today;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_ATA = today.AddDays(10);
			transport.JW_VoyageFlight = "ZZ1234";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "NZAKL";

			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				consol.Containers.RemoveAll();
				var container = Factory.New<ForwardingContainer>();
				container.JC_ContainerNum = "CONT1234565";
				container.JC_FCLAvailable = new ZDateTime(2021, 01, 22);
				container.JC_OverrideFCLAvailableStorage = false;
				strategy.Reset();
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (new ZDateTime(2021, 01, 22), new ZDateTime(2021, 01, 22), "CTO Gate Out"));
				consol.Containers.Add(container);

				var eventXML = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<EventTime>2021-01-05T00:00:00</EventTime>
		<EventType>CAV</EventType>
		<EventParameters>
			<Facility>CTO</Facility>
			<Location>NZTRG</Location>
			<TransportMode>SEA</TransportMode>
        </EventParameters>
		<EventReference />
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
            <Context>
                <Type>ContainerNumber</Type>
                <Value>CONT1234565</Value>
            </Context>
        </ContextCollection>
	</Event>
</UniversalEvent>
";

				var message = GetQueuedUniversalEventMessage(eventXML);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals(new ZDateTime(2021, 01, 22), container.JC_ArrivalCTOStorageStartDate);
			}
		}
	}
}
