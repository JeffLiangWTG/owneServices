using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE41Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notification = new NotificationBuffer();
			var asese41 = new ASESE41()
			{
				ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign,
				FTZLineItemQuantity = 100
			};

			((IACEBIRDLineRecord)asese41).Update(invoiceLine, notification);
			AssertEquals(ZoneStatusList.Codes.PrivilegedForeign, invoiceLine.US_ZoneStatus);
			AssertEquals(100, invoiceLine.US_ManifestQty);
		}
	}
}
