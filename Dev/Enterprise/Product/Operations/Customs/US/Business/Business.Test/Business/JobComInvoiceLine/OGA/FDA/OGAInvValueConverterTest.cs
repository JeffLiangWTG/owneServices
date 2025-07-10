using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class OGAInvValueConverterTest : TestCaseWithFactory
	{
		public void TestConvertForCIF()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				audCurrency.ExchangeRates.DeleteAll();

				var rate = audCurrency.ExchangeRates.AddNew();
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = ZDateTime.BrettsBirthday;
				rate.RE_ExpiryDate = ZDateTime.BrettsBirthday.AddDays(1);
				rate.RE_SellRate = 1.0754m;

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
				declaration.US_EnableCRL = true;
				declaration.US_EntryFilerCode = "XJ5";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 5800.50m;
				invoice.JZ_RX_NKInvoice_Currency = "AUD";
				invoice.JZ_IncoTerm = "CIF";

				var oft = invoice.Charges.AddNew("OFT", 230.50m, "USD");
				oft.J7_IsIncludedInITOT = true;

				var ons = invoice.Charges.AddNew("ONS", 12m, "AUD");
				ons.J7_IsIncludedInITOT = true;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 5800.50m;

				var fda1 = invoiceLine.FDAs.AddNew();
				fda1.US_InvCurrFDAValue = 2500.20m;

				var fda2 = invoiceLine.FDAs.AddNew();
				fda2.US_InvCurrFDAValue = 3300.30m;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertNotEquals("FDA Inv. Value converted", 0m, fda1.US_FDAValue);
				AssertNotEquals("FDA Inv. Value converted", 0m, fda2.US_FDAValue);

				AssertEquals("Customs value match", invoiceLine.CusEntryLine.CL_CustomsValue, fda1.US_FDAValue + fda2.US_FDAValue);
			}
		}

		public void TestCS00188242WithNegativeAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5800.50m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2m;

			var fda1 = invoiceLine.FDAs.AddNew();
			fda1.US_InvCurrFDAValue = 0.5m;

			var fda2 = invoiceLine.FDAs.AddNew();
			fda2.US_InvCurrFDAValue = 0.5m;

			var fda3 = invoiceLine.FDAs.AddNew();
			fda3.US_InvCurrFDAValue = 0.5m;

			var fda4 = invoiceLine.FDAs.AddNew();
			fda4.US_InvCurrFDAValue = 0.5m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Assert(fda1.US_FDAValue >= 0m);
			Assert(fda2.US_FDAValue >= 0m);
			Assert(fda3.US_FDAValue >= 0m);
			Assert(fda4.US_FDAValue >= 0m);
			AssertEquals("Total", 2m, invoiceLine.FDAValueUSDRunningTotal);

			fda1.US_InvCurrFDAValue = 0m;
			fda2.US_InvCurrFDAValue = 0m;
			fda3.US_InvCurrFDAValue = 0.5m;
			fda4.US_InvCurrFDAValue = 0.5m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Total", 1m, invoiceLine.FDAValueUSDRunningTotal);
		}

		public void TestConvertForCIFFromEnteredValueToInvCurr()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				audCurrency.ExchangeRates.DeleteAll();

				var rate = audCurrency.ExchangeRates.AddNew();
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = ZDateTime.BrettsBirthday;
				rate.RE_ExpiryDate = ZDateTime.BrettsBirthday.AddDays(1);
				rate.RE_SellRate = 1.0754m;

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
				declaration.US_EnableCRL = true;
				declaration.US_EntryFilerCode = "XJ5";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 5800.50m;
				invoice.JZ_RX_NKInvoice_Currency = "AUD";
				invoice.JZ_IncoTerm = "CIF";

				var oft = invoice.Charges.AddNew("OFT", 230.50m, "USD");
				oft.J7_IsIncludedInITOT = true;

				var ons = invoice.Charges.AddNew("ONS", 12m, "AUD");
				ons.J7_IsIncludedInITOT = true;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 5800.50m;

				var fda1 = invoiceLine.FDAs.AddNew();
				var fda2 = invoiceLine.FDAs.AddNew();

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				fda1.B7_AddInfoData = "FDAValue=2500*InvCurrFDAValue=2324.72";
				fda2.B7_AddInfoData = "FDAValue=3494*InvCurrFDAValue=3249.02";
				Factory.Save();

				var factory2 = new BusinessObjectFactory();

				invoiceLine = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);
				AssertEquals(2324.72m, invoiceLine.FDAs[0].US_InvCurrFDAValue);
				AssertEquals(3249.02m, invoiceLine.FDAs[1].US_InvCurrFDAValue);

				declaration = factory2.Load<JobDeclaration>(declaration.PK);
				new OGAInvValueConverter().ConvertFromEnteredValueToInvValue(declaration);
				AssertEquals(5800.50m, invoiceLine.FDAValueInvCurrRunningTotal);
			}
		}

		public void TestOneFDALineAndFuzzyRoundingForEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.32m;

			var fda1 = invoiceLine.FDAs.AddNew();
			fda1.US_InvCurrFDAValue = 500.32m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.18m;

			var fda2 = invoiceLine2.FDAs.AddNew();
			fda2.US_InvCurrFDAValue = 200.18m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("FDA value should match", 501m, fda1.US_FDAValue);

			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("FDA value should match", 200m, fda2.US_FDAValue);
		}

		public void TestForStandAlonePriorNoticeWithoutEntry()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				audCurrency.ExchangeRates.DeleteAll();

				var rate = audCurrency.ExchangeRates.AddNew();
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = ZDateTime.BrettsBirthday;
				rate.RE_ExpiryDate = ZDateTime.BrettsBirthday.AddDays(1);
				rate.RE_SellRate = 1.0754m;

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
				declaration.US_EnableSPN = true;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 5800.50m;
				invoice.JZ_RX_NKInvoice_Currency = "AUD";
				invoice.JZ_IncoTerm = "CIF";

				var oft = invoice.Charges.AddNew("OFT", 230.50m, "USD");
				oft.J7_IsIncludedInITOT = true;

				var ons = invoice.Charges.AddNew("ONS", 12m, "AUD");
				ons.J7_IsIncludedInITOT = true;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 5800.50m;

				var fda1 = invoiceLine.FDAs.AddNew();
				fda1.US_InvCurrFDAValue = 2500.20m;

				var fda2 = invoiceLine.FDAs.AddNew();
				fda2.US_InvCurrFDAValue = 3300.30m;

				declaration.ResumeApportionment();

				AssertNotEquals("FDA Inv. Value converted", 0m, fda1.US_FDAValue);
				AssertNotEquals("FDA Inv. Value converted", 0m, fda2.US_FDAValue);

				AssertEquals("Customs value match", 5994m, fda1.US_FDAValue + fda2.US_FDAValue);
			}
		}

		public void TestForACE_FDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.32m;
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			fda1.US_InvCurrValue = 500.32m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.18m;
			var fda2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2.US_InvCurrValue = 200.18m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("FDA value should match", 501m, fda1.US_TotalValue);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("FDA value should match", 200m, fda2.US_TotalValue);
		}

		public void TestForFWS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.32m;
			var header1 = invoiceLine.FWSHeaders.AddNew();
			header1.US_InvCurrPGAValue = 500.32m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200.18m;
			var header2 = invoiceLine2.FWSHeaders.AddNew();
			header2.US_InvCurrPGAValue = 200.18m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("US_CustomsValue rounded", 501m, invoiceLine.US_CustomsValue);
			AssertEquals("FDA value should match", 501m, header1.US_Value);
			AssertEquals("US_CustomsValue rounded", 200m, invoiceLine2.US_CustomsValue);
			AssertEquals("FDA value should match", 200m, header2.US_Value);
		}

		public void TestCS00736291ForACEFDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 109155.8m;
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			fda1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda1.US_InvCurrValue = 7275.95m;
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_InvCurrValue = 38216.81m;
			var fda3 = invoiceLine.ACE_FDALines.AddNew();
			fda3.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda3.US_InvCurrValue = 19910.46m;
			var fda4 = invoiceLine.ACE_FDALines.AddNew();
			fda4.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda4.US_InvCurrValue = 43752.58m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(7276m, fda1.US_TotalValue);
			AssertEquals(38217m, fda2.US_TotalValue);
			AssertEquals(19910m, fda3.US_TotalValue);
			AssertEquals(43753m, fda4.US_TotalValue);
		}
	}
}
