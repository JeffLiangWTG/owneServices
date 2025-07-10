using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(DeliveryNotificationParty))]
	public class DeliveryNotificationPartyTest : NonPersistentBusinessObjectTestCase
	{
		protected class TestManifestStatusClass : OutwardReportManifestStatus
		{
			public TestManifestStatusClass(ForwardingConsol consol)
				: base(consol)
			{
			}
		}

		public void TestDeliveryNotificationPartyAddress()
		{
			var deliveryParty = Factory.New<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var testManifestStatus = new TestManifestStatusClass(consol);
			testManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = deliveryParty.PK;
			AssertEquals("DeliveryNotificationParty", deliveryParty.PK, testManifestStatus.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty);
		}

		public void TestDeliveryNotificationPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			AssertEquals(ZString.Empty, manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort);
			manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort = "NZAKL";
			AssertEquals("NZAKL", manifestStatus.DeliveryNotificationParty.DeliveryNotificationPartyPort);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			var manifestStatus = new OutwardReportManifestStatus(consol);
			return new DeliveryNotificationParty(manifestStatus);
		}
	}
}
