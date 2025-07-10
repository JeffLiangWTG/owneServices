using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS62Test : BIRDLineUpdateTest
	{
		public void TestHMFApplicableSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens62 = new ENS62();
			ens62.ClassCode = Core.Constants.USCustoms.FeeCodes.HMF;
			ens62.UserFeeAmount = 30.99m;

			((IBIRDLineRecord)ens62).Update(declaration.InvoiceLines[0], new NotificationCollection());

			AssertEquals("HMF applicable has been set", YesNoDefaultList.Codes.Yes, declaration.US_IsHMFApplicable);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS62 ens62 = new ENS62();

			ens62.TariffNumber = USCTariff.CottonFeeApplicable;
			ens62.ClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			ens62.UserFeeAmount = 30.99m;

			return new IBIRDLineRecord[] { ens62 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS62);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
