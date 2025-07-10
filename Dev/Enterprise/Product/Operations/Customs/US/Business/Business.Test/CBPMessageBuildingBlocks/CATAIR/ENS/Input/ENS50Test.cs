using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS50Test : BIRDLineUpdateTest
	{
		public void TestDifferentPartyIndicatorsOnLineLevel()
		{
			var messageText =
"AA7501                                  201305021139570100                      " +
"10A1502995201-0067156-219873100                 8042913   OHL 0115724106036  NC " +
"20     FTZ0219               1502042913                          042913L844     " +
"30                                                  6                           " +
"40001HN00000002280000000029000000000000000000000000000003     N         INV001  " +
"50P6101200010          000000000400DOZ000000002900KG                HN      Y   " +
"51                                             308276124                        " +
"60                                        HNGILACT6VIL                          " +
"62          49900000000                                                         " +
"40002HN00000000840000000013000000000000000000000000000001     N         INV001  " +
"50P6103431520          000000000300DOZ000000001300KG                HN      Y   " +
"51                                             308276124                        " +
"60                                        HNGILACT6VIL                          " +
"62          49900000000                                                         " +
"40003BD00000010010000000064000000000000000000000000000013     N         INV001  " +
"50 6109100012          000000003000DOZ000000006400KG                BD      N   " +
"60                                        BDRIPKNIGAZ                           " +
"62          49900000347                                                         " +
"40004VN00000007260000000069000000000000000000000000000009     N         INV001  " +
"50 6307909882          000000043200NO 000000006900KG                VN      N   " +
"60                                        VNPHOPHU09HOC                         " +
"62          49900000251                                                         " +
"89499                                                                           " +
"9000000626830           0 00000000000000000000000000000733200000872923          " +
"ZAGildan Activewear SRL              Newton Industrial Park                     " +
"ZBChrist Church                 NCBB17047                                       " +
"ZI001USD00000626830100000000000000000 00000000000                               " +
"ZZ7501000000432                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(4, declaration.InvoiceLines.Count);
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6101200010");
			AssertEquals(YesNoDefaultList.Codes.Yes, line.US_TransactionsRelated);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6103431520");
			AssertEquals(YesNoDefaultList.Codes.Yes, line.US_TransactionsRelated);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6109100012");
			AssertEquals(YesNoDefaultList.Codes.No, line.US_TransactionsRelated);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6307909882");
			AssertEquals(YesNoDefaultList.Codes.No, line.US_TransactionsRelated);
		}

		public void TestWhenDatesOfExportExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens50 = new ENS50();
			ens50.DateOfExportation = new ZDate(2009, 1, 3);

			var notifications = new NotificationCollection();

			((IBIRDLineRecord)ens50).Update(invoiceLine, notifications);

			AssertEquals("Date Of Export is updated", new ZDate(2009, 1, 3), declaration.JE_ExportDate);

			ens50.DateOfExportation = new ZDate(2009, 1, 4);
			((IBIRDLineRecord)ens50).Update(invoiceLine, notifications);
			AssertEquals("Date Of Export of declaration untouched", new ZDate(2009, 1, 3), declaration.JE_ExportDate);
			AssertEquals("date of export", new ZDate(2009, 1, 4), declaration.US_DateOfExport);

			ens50.DateOfExportation = new ZDate(2009, 1, 5);
			((IBIRDLineRecord)ens50).Update(invoiceLine, notifications);
			AssertEquals("Date Of Export of declaration untouched", new ZDate(2009, 1, 3), declaration.JE_ExportDate);
			AssertEquals("date of export", new ZDate(2009, 1, 4), declaration.US_DateOfExport);
			AssertEquals("date of export", new ZDate(2009, 1, 5), invoice.US_DateOfExport);
		}

		public void TestWarnWhenBothSPICountryAndPrimaryExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens50 = new ENS50();
			ens50.SpecialProgramsIndicatorPrimary = "A";
			ens50.SpecialProgramsIndicatorCountry = "AU";

			var notifications = new NotificationCollection();

			((IBIRDLineRecord)ens50).Update(invoiceLine, notifications);
			AssertContains(ENS50.BothSPIPrimaryAndCountryExist, notifications.ToUniqueMessageListString());
			AssertEquals("SPI is updated with country", "AU", invoiceLine.US_SPI);
		}

		public void TestSetSPIToNAIfApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens50 = new ENS50();
			ens50.CountryOfExport = "AU";
			ens50.TariffNumber1 = "6104220010";
			ens50.SpecialProgramsIndicatorCountry = "";

			var notifications = new NotificationCollection();

			((IBIRDLineRecord)ens50).Update(invoiceLine, notifications);
			AssertEquals("SPI in the message is empty, but should be set to N/A", SPICompleteList.MoreCodes.NotApplicable, invoiceLine.US_SPI);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			var ens50 = new ENS50();

			ens50.TariffNumber1 = "0000000000";
			ens50.Quantity1 = 123456m;
			ens50.Duty = 3490.99m;
			ens50.UnitOfMeasure1 = "KG";

			ens50.Quantity2 = 23456m;
			ens50.UnitOfMeasure2 = "L";

			ens50.UnitOfMeasure3 = "BO";

			ens50.CountryOfExport = "AU";

			ens50.DateOfExportation = new ZDate(2009, 1, 3);

			ens50.RelatedPartyIndicator = YesNoDefaultList.Codes.Yes;

			ens50.SpecialProgramsIndicatorCountry = "AU";

			ens50.SpecialProgramsIndicatorSecondary = SecondarySpecProgIndicatorList.Codes.F;

			var ens50WithPrimarySPI = new ENS50();

			ens50WithPrimarySPI.TariffNumber1 = "0000000000";
			ens50WithPrimarySPI.Quantity1 = 123456m;
			ens50WithPrimarySPI.UnitOfMeasure1 = "KG";

			ens50WithPrimarySPI.Quantity2 = 23456m;
			ens50WithPrimarySPI.UnitOfMeasure2 = "L";

			ens50WithPrimarySPI.Quantity3 = 56m;
			ens50WithPrimarySPI.UnitOfMeasure3 = "BO";

			ens50WithPrimarySPI.CountryOfExport = "AU";

			ens50WithPrimarySPI.DateOfExportation = new ZDate(2009, 1, 3);

			ens50WithPrimarySPI.RelatedPartyIndicator = YesNoDefaultList.Codes.No;

			ens50WithPrimarySPI.SpecialProgramsIndicatorPrimary = "A";

			return new IBIRDLineRecord[] { ens50, ens50WithPrimarySPI };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "L";
			tariff.UE_Unit3 = "BO";
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS50);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
