using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(NonMatchingAgentsSecurity))]
	sealed class NonMatchingAgentsSecurityTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor()
		{
			var agentSecurity = new NonMatchingAgentsSecurity();
			AssertEquals(ZString.Empty, agentSecurity.Message);

			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();

			var helper = new Mock<IShipmentVsConsolMessageHelper>() { CallBase = true };

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper.Setup(m => m.CheckRelatedReceivingAgents(new[] { shipment }, new[] { consol })).Returns("receivingmessage");

				agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
				AssertEquals("receivingmessage", agentSecurity.Message);
				helper.VerifyAll();

				helper.Reset();
				helper.Setup(m => m.CheckRelatedSendingAgents(new[] { shipment }, new[] { consol })).Returns("sendingmessage");

				agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
				AssertEquals("sendingmessage", agentSecurity.Message);
				helper.VerifyAll();

				helper.Reset();
				helper.Setup(m => m.CheckRelatedReceivingAgents(new[] { shipment }, new[] { consol })).Returns("receivingmessage");
				helper.Setup(m => m.CheckRelatedSendingAgents(new[] { shipment }, new[] { consol })).Returns("sendingmessage");

				agentSecurity = new NonMatchingAgentsSecurity(new[] { consol }, new[] { shipment });
				AssertEquals("receivingmessage" + "\r\n" + "sendingmessage", agentSecurity.Message);
				helper.VerifyAll();
			}
		}
	}
}
