using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE60Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notification = new NotificationBuffer();
			var asese60 = new ASESE60()
			{
				HTSNumber = "123456",
				LineItemValue = 100m
			};

			((IACEBIRDSecondaryLineRecord)asese60).Update(invoiceLine, false, notification);
			AssertEquals(100m, invoiceLine.JI_LinePrice);
		}
	}
}
