using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS53Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var notifications = new NotificationBuffer();
			var aens53 = new AENS53()
			{
				CaseNumber = "A462105011",
				BondCashClaimCode = "B",
				CaseRateTypeQualifierCode = "1",
				ADCVDValueOfGoodsAmount = 100m,
				ADCVDQuantity = 10m
			};
			((IACEBIRDLineRecord)aens53).Update(invoiceLine, notifications);
			AssertEquals("A462105011", invoiceLine.US_ADDCaseNo);
			AssertEquals(true, invoiceLine.US_IsBondedADD);
			AssertEquals("1", invoiceLine.US_ADDDepositRateIndicator);
			AssertEquals(100m, invoiceLine.US_ADDDepositValue);
			AssertEquals(10m, invoiceLine.US_ADDQty);
		}
	}
}
