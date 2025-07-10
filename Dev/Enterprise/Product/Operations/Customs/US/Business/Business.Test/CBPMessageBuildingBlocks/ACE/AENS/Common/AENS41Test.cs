using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS41Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens41 = new AENS41()
			{
				FTZMerchandiseStatusCode = ZoneStatusList.Codes.PrivilegedForeign,
				FTZLineItemQuantity = 100
			};

			((IACEBIRDLineRecord)aens41).Update(invoiceLine, notifications);
			AssertEquals(ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
			AssertEquals(100, invoiceLine.US_ManifestQty);
		}
	}
}
