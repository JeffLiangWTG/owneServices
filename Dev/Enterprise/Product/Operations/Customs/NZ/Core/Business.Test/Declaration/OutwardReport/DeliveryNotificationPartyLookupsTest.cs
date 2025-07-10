using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	internal class DeliveryNotificationPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNLOCOPorts()
		{
			var notificationPartyPorts = Lookups.DeliveryNotificationPartyPorts;
			AssertEquals(typeof(RefUNLOCOCollection), notificationPartyPorts.GetType());
			AssertSame(notificationPartyPorts, Lookups.DeliveryNotificationPartyPorts);
		}

		public void TestNotifyParty_List()
		{
			AssertEquals(typeof(OrgHeaderCollection), Lookups.NotifyParty_List.GetType());
		}

		DeliveryNotificationPartyLookups Lookups
		{
			get { return lookups ?? (lookups = new DeliveryNotificationPartyLookups(DeliveryNotificationParty)); }
		}
		DeliveryNotificationPartyLookups lookups;

		DeliveryNotificationParty DeliveryNotificationParty
		{
			get
			{
				return deliveryNotificationParty ?? (deliveryNotificationParty = new DeliveryNotificationParty(ORMStatus));
			}
		}
		DeliveryNotificationParty deliveryNotificationParty;

		OutwardReportManifestStatus ORMStatus
		{
			get
			{
				return fORMStatus ?? (fORMStatus = new OutwardReportManifestStatus(Consol));
			}
		}
		OutwardReportManifestStatus fORMStatus;

		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<ForwardingConsol>();
				}
				return fConsol;
			}
		}
		ForwardingConsol fConsol;
	}
}
