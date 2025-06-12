using System.Linq;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using eServices.eHubRoutingRuleEngine;
using NUnit.Framework;
using Rule = eServices.eHubRoutingRuleEngine.Rule;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.IntegrationTests.NGBEDI
{
    [TestFixture]
	public class NGBEDI_RoutingRuleTests : RoutingRuleIntegrationTestBase
	{
		[Test]
		public void TestNGBEDI_EP1()
		{
			var message = new TestingMessage("HYEDAUUAT", "FORWARDING_PORT_MESSAGE");
			message.ShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerLoadPlan/1";
			message.Containers = "";
			message.OperationalPort = "CNNGB";
			var context = CreateeHubTransactionsContext();
			if (context.eHubClients != null)
			{
				var clientService = context.eHubClients.First(x => x.CC_ID == "FORWARDING_PORT_MESSAGE");
				var factResolver = new RoutingRuleMessageFactResolver(message);

				var rule = Rule.GetForReading(clientService);

				var result = rule.Evaluate(context, new IFactResolver[] { factResolver }, mockLogger);
				AssertClient("NGBEDI_EP1", result);
			}
		}

		[Test]
		public void TestNGBEDI_Reject()
		{
			var message = new TestingMessage("HYEDAUUAT", "FORWARDING_PORT_MESSAGE");
			message.ShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerLoadPlan/1";
			message.Containers = "";
			message.OperationalPort = "AAAAA";
			var context = CreateeHubTransactionsContext();
			if (context.eHubClients != null)
			{
				var clientService = context.eHubClients.First(x => x.CC_ID == "FORWARDING_PORT_MESSAGE");
				var factResolver = new RoutingRuleMessageFactResolver(message);

				var rule = Rule.GetForReading(clientService);

				var result = rule.Evaluate(context, new IFactResolver[] { factResolver }, mockLogger);
				AssertError("IRJ", "Department=WiseTechGlobal|Reason=You are not registered with this Carrier for this message type. Contact WTG to register.", result);
			}
		}
	}
}
