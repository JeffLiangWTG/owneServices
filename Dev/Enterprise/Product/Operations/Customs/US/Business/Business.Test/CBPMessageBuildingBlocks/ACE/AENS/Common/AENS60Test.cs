using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS60Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens60 = new AENS60()
			{
				AccountingClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise
			};
			((IACEBIRDLineRecord)aens60).Update(invoiceLine, notifications);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, invoiceLine.US_TaxCode);
		}
	}
}
