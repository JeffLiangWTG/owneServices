using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS51Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens51 = new AENS51()
			{
				StandardVisaNumber = "2LS456789"
			};
			((IACEBIRDLineRecord)aens51).Update(invoiceLine, notifications);
			AssertEquals("2LS456789", invoiceLine.US_VisaNo);
		}
	}
}
