using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS54Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens54 = new AENS54()
			{
				ImportersAdditionalDeclarationInformation = "Y000000120010"
			};
			((IACEBIRDLineRecord)aens54).Update(invoiceLine, notifications);
			AssertEquals("Y", invoiceLine.US_LumberImporterDeclaration);
			AssertEquals(1200m, invoiceLine.US_LumberExportCharges);
			AssertEquals(10m, invoiceLine.US_LumberExportPrice);
		}
	}
}
