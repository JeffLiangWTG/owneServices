using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryLineHelperTest : TestCaseWithFactory
	{
		public void TestRoundedCustomsValueWhen98TariffAndDDP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_Tariff = "4418.60.0000";
			line1.US_SupTariff = "9802.00.5060";
			line1.US_98GoodsValue = 11000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "4418600000");
			var helper = new EntryLineHelper(entryLine1);
			AssertEquals(9690m, helper.GetRoundedCustomsValue());
		}

		public void TestGetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_CustomsValue = 50m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_CustomsValue = 60m;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.US_CustomsValue = 100m;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100m;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 90m;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			entryLine1.US_CL_ParentLine = entryLine2.PK;
			AssertEquals("Sum of US_CustomsValue of Invoice Line", 210m, ((IDrawbackEntryLine)entryLine1).TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines);
			var helper1 = new EntryLineHelper(entryLine1);
			AssertEquals("Sum of US_CustomsValue of Invoice Line", 210m, helper1.GetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines());
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var usImportEntryLine = factory.Load<USImportEntryLine>(entryLine1.PK);
			AssertEquals("Sum of US_CustomsValue of Invoice Line", 210m, ((IDrawbackEntryLine)usImportEntryLine).TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines);
			var helper2 = new EntryLineHelper(usImportEntryLine);
			AssertEquals("Sum of US_CustomsValue of Invoice Line", 210m, helper2.GetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines());
		}

		public void TestGetCustomsQuantityAndUnitQty()
		{
			#region Setup Tariff

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8215993500", "7", 0.068m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8215.99.3500";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "NO";
			invoiceLine.JI_CustomsSecondQuantity = 15m;
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLines = declaration.FormalEntry.MergedLines;
			var entryLine8215993500 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "8215993500");
			AssertEquals("Customs Quantity for tariff 8215.99.3500", 100m, entryLine8215993500.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 8215.99.3500", "KG", entryLine8215993500.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity for tariff 8215.99.3500", 15m, entryLine8215993500.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 8215.99.3500", "NO", entryLine8215993500.Helper.GetSecondCustomsUnitQty());
			var entryLine99030120 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99030120");
			AssertEquals("Customs Quantity for tariff 9903.01.20", 0m, entryLine99030120.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.01.20", ZString.Empty, entryLine99030120.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.01.20", 0m, entryLine99030120.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.01.20", ZString.Empty, entryLine99030120.Helper.GetSecondCustomsUnitQty());
			var entryLine99038815 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99038815");
			AssertEquals("Customs Quantity for tariff 9903.88.15", 0m, entryLine99038815.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.88.15", ZString.Empty, entryLine99038815.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.88.15", 0m, entryLine99038815.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.88.15", ZString.Empty, entryLine99038815.Helper.GetSecondCustomsUnitQty());

			invoiceLine.US_SupUQ1 = "NO";
			invoiceLine.US_SupQty1 = 200m;
			invoiceLine.US_SupUQ2 = "KG";
			invoiceLine.US_SupQty2 = 300m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			entryLine8215993500 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "8215993500");
			AssertEquals("Customs Quantity for tariff 8215.99.3500", 100m, entryLine8215993500.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 8215.99.3500", "KG", entryLine8215993500.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity for tariff 8215.99.3500", 15m, entryLine8215993500.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 8215.99.3500", "NO", entryLine8215993500.Helper.GetSecondCustomsUnitQty());
			entryLine99030120 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99030120");
			AssertEquals("Customs Quantity for tariff 9903.01.20", 200m, entryLine99030120.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.01.20", "NO", entryLine99030120.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.01.20", 300m, entryLine99030120.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.01.20", "KG", entryLine99030120.Helper.GetSecondCustomsUnitQty());
			entryLine99038815 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99038815");
			AssertEquals("Customs Quantity for tariff 9903.88.15", 0m, entryLine99038815.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.88.15", ZString.Empty, entryLine99038815.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.88.15", 0m, entryLine99038815.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.88.15", ZString.Empty, entryLine99038815.Helper.GetSecondCustomsUnitQty());

			invoiceLine.US_SupAdditionalTariff1UQ = "M2";
			invoiceLine.US_SupAdditionalTariff1Qty = 500m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			entryLine8215993500 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "8215993500");
			AssertEquals("Customs Quantity for tariff 8215.99.3500", 100m, entryLine8215993500.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 8215.99.3500", "KG", entryLine8215993500.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity for tariff 8215.99.3500", 15m, entryLine8215993500.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 8215.99.3500", "NO", entryLine8215993500.Helper.GetSecondCustomsUnitQty());
			entryLine99030120 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99030120");
			AssertEquals("Customs Quantity for tariff 9903.01.20", 200m, entryLine99030120.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.01.20", "NO", entryLine99030120.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.01.20", 300m, entryLine99030120.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.01.20", "KG", entryLine99030120.Helper.GetSecondCustomsUnitQty());
			entryLine99038815 = entryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "99038815");
			AssertEquals("Customs Quantity for tariff 9903.88.15", 500m, entryLine99038815.Helper.GetCustomsQuantity());
			AssertEquals("Customs UQ for tariff 9903.88.15", "M2", entryLine99038815.Helper.GetCustomsUnitQty());
			AssertEquals("Customs Second Quantity Quantity for tariff 9903.88.15", 0m, entryLine99038815.Helper.GetSecondCustomsQuantity());
			AssertEquals("Customs Second UQ for tariff 9903.88.15", ZString.Empty, entryLine99038815.Helper.GetSecondCustomsUnitQty());
		}
	}
}
