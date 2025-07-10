using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	internal class NZAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMsgTransportList()
		{
			AssertNotNull(Lookups.MsgTransportList);
			AssertEquals(typeof(MsgTransportList), Lookups.MsgTransportList.GetType());
		}

		OutwardReportLookups Lookups
		{
			get { return lookups ?? (lookups = new OutwardReportLookups(ORMStatus)); }
		}
		OutwardReportLookups lookups;

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
