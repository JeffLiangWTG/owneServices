using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS43Test : BIRDLineUpdateTest
	{
		public void TestMultipleTypesOfENS43WithINVREQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var ens43s = new List<ENS43>();

			var ens43 = new ENS43();
			ens43.TypeIndicator = "R";
			ens43.PreImportationReviewProgramPIRPRulingsNumber = "123456";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "Commercial Description";
			ens43s.Add(ens43);

			foreach (IBIRDLineRecord lineRecord in ens43s)
			{
				lineRecord.Update(invoiceLine, new NotificationCollection());
			}

			AssertEquals("Commercial Description", invoiceLine.JI_Description);
			AssertEquals("Ruling number", "123456", invoiceLine.US_PIRPRulingNo);
			AssertEquals("Ruling type", "R", invoiceLine.US_PIRPRulingType);
		}

		public void TestMultipleTypesOfENS43WithBindingRules()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var ens43s = new List<ENS43>();

			var ens43 = new ENS43();
			ens43.TypeIndicator = PIRPRulingTypeList.Codes.Preclassification;
			ens43.PreImportationReviewProgramPIRPRulingsNumber = "C78324";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "One or more entry summary transactions may be transmitted to the U.S.";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = " Customs and Border Protection (CBP) Data Center.";
			ens43s.Add(ens43);

			foreach (IBIRDLineRecord lineRecord in ens43s)
			{
				lineRecord.Update(invoiceLine, new NotificationCollection());
			}

			AssertEquals("One or more entry summary transactions may be transmitted to the U.S. Customs and Border Protection (CBP) Data Center.", invoiceLine.JI_Description);
			AssertEquals(PIRPRulingTypeList.Codes.Preclassification, invoiceLine.US_PIRPRulingType);
			AssertEquals("C78324", invoiceLine.US_PIRPRulingNo);
		}

		public void TestMultipleENS43ContainingExtensiveDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var ens43s = new List<ENS43>();

			var ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "One or more entry summary transactions may be transmitted to the U.S.";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "Customs and Border Protection (CBP) Data Center. An entry summary tran";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "saction consists of at least five mandatory records that include heade";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "r, line item and line item control information. There are 17 additiona";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "l entry summary records that may be included in the transaction if cer";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "tain conditions exist. The entry summary records that make up the tran";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "saction must be transmitted to the CBP Data Center in ascending record";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "identifier order with certain record segments repeated as necessary (f";
			ens43s.Add(ens43);

			ens43 = new ENS43();
			ens43.TypeIndicator = "D";
			ens43.CommercialDescription = "or example, Record Identifiers 40 through 81).";
			ens43s.Add(ens43);

			foreach (IBIRDLineRecord lineRecord in ens43s)
			{
				lineRecord.Update(invoiceLine, new NotificationCollection());
			}

			var description = invoiceLine.JI_Description + invoiceLine.JI_ExtraInfoForClassification;

			foreach (ENS43 lineRecord in ens43s)
			{
				AssertContains(lineRecord.CommercialDescription, description);
			}
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS43 pirl43 = new ENS43();
			pirl43.PreImportationReviewProgramPIRPRulingsNumber = "123456";
			pirl43.TypeIndicator = "R";

			ENS43 description43 = new ENS43();
			description43.TypeIndicator = "D";
			description43.CommercialDescription = "Commercial Description";

			return new IBIRDLineRecord[] { pirl43, description43 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			declaration.US_EnableAII = true;
		}

		protected override System.Type GetTypeOfMessageBlock() => typeof(ENS43);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
