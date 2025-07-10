using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS34Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
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
			var aens34 = new AENS34()
			{
				AccountingClassCode1 = Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge,
				HeaderFeeAmount1 = 100m,
				AccountingClassCode2 = Core.Constants.USCustoms.FeeCodes.Beef,
				HeaderFeeAmount2 = 200m,
			};

			((IBIRDHeaderRecord)aens34).Update(declaration, notifications);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(100m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge));
			AssertEquals(200m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef));
		}
	}
}
