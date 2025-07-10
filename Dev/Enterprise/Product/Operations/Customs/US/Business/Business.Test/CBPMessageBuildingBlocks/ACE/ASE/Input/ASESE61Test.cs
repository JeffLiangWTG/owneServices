using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE61Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var asese61 = new ASESE61()
			{
				CurrentHTSNumberForPFStatusMerchandise = "9001000010"
			};
			((IACEBIRDLineRecord)asese61).Update(invoiceLine, notifications);
			AssertEquals("9001000010", invoiceLine.US_FTZCurrentTariff);
		}
	}
}
