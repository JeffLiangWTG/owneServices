using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS62Test : TestCaseWithFactory
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
			var aens62 = new AENS62()
			{
				AccountingClassCode = Core.Constants.USCustoms.FeeCodes.HMF,
				UserFeeAmount = 100m
			};
			((IACEBIRDLineRecord)aens62).Update(invoiceLine, notifications);
			AssertEquals("Y", declaration.US_IsHMFApplicable);
			AssertEquals(1, invoiceLine.FeeCusCodes.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.HMF, invoiceLine.FeeCusCodes[0].CY_Code);
		}
	}
}
