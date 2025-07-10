using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS43Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var aens43 = new AENS43()
			{
				RulingTypeCode = PIRPRulingTypeList.Codes.CommercialDescription,
				RulingNumber = "GTV34"
			};

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			((IACEBIRDLineRecord)aens43).Update(invoiceLine, notifications);
			AssertEquals(PIRPRulingTypeList.Codes.CommercialDescription, invoiceLine.US_PIRPRulingType);
			AssertEquals("GTV34", invoiceLine.US_PIRPRulingNo);
		}
	}
}
