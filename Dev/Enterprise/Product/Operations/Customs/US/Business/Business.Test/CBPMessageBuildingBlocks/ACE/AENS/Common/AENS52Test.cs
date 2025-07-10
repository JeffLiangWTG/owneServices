using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS52Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens52 = new AENS52()
			{
				LicenseCertificatePermitTypeCode = "A1",
				LicenseNumberCertificateNumberPermitNumber = "1234"
			};
			((IACEBIRDLineRecord)aens52).Update(invoiceLine, notifications);
			AssertEquals(1, invoiceLine.LicenceAndPermits.Count);
			AssertEquals("A1", invoiceLine.LicenceAndPermits[0].CY_Code);
			AssertEquals("1234", invoiceLine.LicenceAndPermits[0].CY_Data);
		}
	}
}
