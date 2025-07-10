using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ReleaseEntryInvoiceRetrieverTest : TestCaseWithFactory
	{
		public void TestRetrieveInvoiceValuesCorrectly()
		{
			var releaseDec = Factory.New<JobDeclaration>();
			releaseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDec.US_EnableENS = true;
			releaseDec.US_EntryFilerCode = "XJ5";
			releaseDec.ImportEntryNumber = "9342838";
			releaseDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = releaseDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_InvoiceAmount = 540m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			invoice.JZ_IncoTermPlace = "Seoul";
			invoice.JZ_Weight = 500m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilotonnes;
			invoice.JZ_NetWeight = 400m;
			invoice.JZ_NetWeightUQ = Core.Constants.Weight.Kilotonnes;
			invoice.JZ_NoOfPacks = 3m;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoice.US_DateOfExport = new ZDateTime(2019, 1, 1);
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice.US_FirstSale = YesNoDefaultList.Codes.No;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "1234.56.7890";
			invoiceLine.JI_NetWeight = 250m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Weight = 300m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilotonnes;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = Core.Constants.Weight.Kilotonnes;
			invoiceLine.JI_CustomsSecondQuantity = 200m;
			invoiceLine.SupTariffFormatted = "9876.54.3210";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 1.234m;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mali;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Taiwan;
			invoiceLine.US_SPI = SpecialProgramList.Codes.MX;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_FirstSale = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_DestinationState = "IL";
			invoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoiceLine.JI_Description = "TOYS";
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			invoiceLine.JI_Volume = 50m;
			invoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicFeet;
			invoiceLine.JI_LinePrice = 1200m;
			invoiceLine.JI_RH_NKCommodity_Code = "MTD";
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.SupFormattedAdditionalTariff1 = "9903.88.01";
			invoiceLine2.SupFormattedAdditionalTariff2 = "9903.88.02";
			invoiceLine2.SupFormattedAdditionalTariff3 = "9903.88.03";
			invoiceLine2.SupFormattedAdditionalTariff4 = "9903.88.04";
			invoiceLine2.SupFormattedAdditionalTariff5 = "9903.88.05";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_ParentID = ZGuid.Empty;

			releaseDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoice.JZ_InvoiceCurrExRate = 2.13m;
			Factory.Save();

			var consolidatedDeclaration = Factory.New<JobDeclaration>();
			new ReleaseEntryInvoiceRetriever(consolidatedDeclaration).ImportInvoice(releaseDec, null);

			var invoiceImported = consolidatedDeclaration.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == "INV1");
			AssertEquals("XJ59342838", invoiceImported.US_ReleaseEntryNumber);
			AssertEquals("INV1", invoiceImported.JZ_InvoiceNumber);
			AssertEquals(540m, invoiceImported.JZ_InvoiceAmount);
			AssertEquals("EUR", invoiceImported.JZ_RX_NKInvoice_Currency);
			AssertEquals(2.13m, invoiceImported.JZ_InvoiceCurrExRate);
			AssertEquals("FOB", invoiceImported.JZ_IncoTerm);
			AssertEquals("Seoul", invoiceImported.JZ_IncoTermPlace);
			AssertEquals(500m, invoiceImported.JZ_Weight);
			AssertEquals("KT", invoiceImported.JZ_WeightUQ);
			AssertEquals(400m, invoiceImported.JZ_NetWeight);
			AssertEquals("KT", invoiceImported.JZ_NetWeightUQ);
			AssertEquals(3m, invoiceImported.JZ_NoOfPacks);
			AssertEquals("CA", invoiceImported.US_UC_NKCountryOfExport);
			AssertEquals("CN", invoiceImported.US_UC_NKCountryOfOrigin);
			AssertEquals(new ZDateTime(2019, 1, 1), invoiceImported.US_DateOfExport);
			AssertEquals("N", invoiceImported.US_TransactionsRelated);
			AssertEquals("N", invoiceImported.US_FirstSale);

			var invoiceLineImported = consolidatedDeclaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Calc_Invoice == "INV1" && x.JI_LineNo == 1);
			AssertEquals("1234.56.7890", invoiceLineImported.JI_FormattedTariff);
			AssertEquals(250m, invoiceLineImported.JI_NetWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, invoiceLineImported.JI_NetWeightUQ);
			AssertEquals(300m, invoiceLineImported.JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, invoiceLineImported.JI_WeightUQ);
			AssertEquals(100m, invoiceLineImported.JI_CustomsQuantity);
			AssertEquals(Core.Constants.Weight.Kilotonnes, invoiceLineImported.JI_CustomsUnitQty);
			AssertEquals("1234.56.7890", invoiceLineImported.JI_FormattedTariff);
			AssertEquals(200m, invoiceLineImported.JI_CustomsSecondQuantity);
			AssertEquals(Core.Constants.Weight.Kilotonnes, invoiceLineImported.JI_CustomsSecondUnitQty);
			AssertEquals("9876.54.3210", invoiceLineImported.SupTariffFormatted);
			AssertEquals(TaxApplyList.Codes.Override, invoiceLineImported.US_TaxApply);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.OtherExcise, invoiceLineImported.US_TaxCode);
			AssertEquals(RateTypeList.Codes.Primary, invoiceLineImported.US_TaxRateT);
			AssertEquals(AppendixBTaxRateList.Codes.Specify, invoiceLineImported.US_TaxRateS);
			AssertEquals(1.234m, invoiceLineImported.US_TaxRate);
			AssertEquals(Core.Constants.CountryCodes.Mali, invoiceLineImported.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.Taiwan, invoiceLineImported.US_UC_NKCountryOfOrigin);
			AssertEquals(SpecialProgramList.Codes.MX, invoiceLineImported.US_SPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, invoiceLineImported.US_SecondarySPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.X, invoiceLineImported.US_SetInd);
			AssertEquals(YesNoDefaultList.Codes.Yes, invoiceLineImported.US_FirstSale);
			AssertEquals("IL", invoiceLineImported.US_DestinationState);
			AssertEquals(YesNoDefaultList.Codes.No, invoiceLineImported.US_TransactionsRelated);
			AssertEquals("TOYS", invoiceLineImported.JI_Description);
			AssertEquals(10m, invoiceLineImported.JI_InvoiceQuantity);
			AssertEquals("PCS", invoiceLineImported.JI_InvoiceUQ);
			AssertEquals(50m, invoiceLineImported.JI_Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, invoiceLineImported.JI_VolumeUQ);
			AssertEquals(1200m, invoiceLineImported.JI_LinePrice);
			AssertEquals("MTD", invoiceLineImported.JI_RH_NKCommodity_Code);

			var invoiceLineImported2 = consolidatedDeclaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Calc_Invoice == "INV1" && x.JI_LineNo == 2);
			AssertEquals(invoiceLineImported.PK, invoiceLineImported2.JI_ParentID);
			AssertEquals("9903.88.01", invoiceLineImported2.SupFormattedAdditionalTariff1);
			AssertEquals("9903.88.02", invoiceLineImported2.SupFormattedAdditionalTariff2);
			AssertEquals("9903.88.03", invoiceLineImported2.SupFormattedAdditionalTariff3);
			AssertEquals("9903.88.04", invoiceLineImported2.SupFormattedAdditionalTariff4);
			AssertEquals("9903.88.05", invoiceLineImported2.SupFormattedAdditionalTariff5);

			var invoiceLineImported3 = consolidatedDeclaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Calc_Invoice == "INV1" && x.JI_LineNo == 3);
			AssertEquals(ZGuid.Empty, invoiceLineImported3.JI_ParentID);
		}

		public void TestRetrieveInvoiceCounts()
		{
			var releaseDeclarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var releaseDec1 = releaseDeclarations.AddNew();
			releaseDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDec1.US_EnableENS = true;
			releaseDec1.US_EntryFilerCode = "XJ5";
			releaseDec1.ImportEntryNumber = "9342838";
			releaseDec1.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = releaseDec1.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();

			releaseDec1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:InvoiceLine1 and InvoiceLine2 are merged together", invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);

			var releaseDec2 = releaseDeclarations.AddNew();
			releaseDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec2.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			releaseDec2.US_EnableENS = true;
			releaseDec2.US_EntryFilerCode = "SV9";
			releaseDec2.ImportEntryNumber = "8382439";
			releaseDec2.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice2 = releaseDec2.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();

			releaseDec2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:InvoiceLine3 and InvoiceLine4 are merged together", invoiceLine3.CusEntryLine, invoiceLine4.CusEntryLine);

			Factory.Save();

			var consolidatedDeclaration = Factory.New<JobDeclaration>();
			new ReleaseEntryInvoiceRetriever(consolidatedDeclaration).ImportDeclarations(releaseDeclarations);

			AssertEquals("2 invoices have been retrieved", 2, consolidatedDeclaration.Invoices.Count);
			AssertEquals("4 invoice lines have been retrieved", 4, consolidatedDeclaration.InvoiceLines.Count);

			consolidatedDeclaration.Invoices.FirstOrDefault(x => x.JZ_InvoiceNumber == "INV1")?.Delete();
			AssertEquals("Pre-condition: 1 invoice exists", 1, consolidatedDeclaration.Invoices.Count);
			AssertEquals("Pre-condition: 2 invoice lines exist", 2, consolidatedDeclaration.InvoiceLines.Count);

			new ReleaseEntryInvoiceRetriever(consolidatedDeclaration).ImportDeclarations(releaseDeclarations);
			AssertEquals("1 invoice has been retrieved", 2, consolidatedDeclaration.Invoices.Count);
			AssertEquals("2 invoice lines have been retrieved", 4, consolidatedDeclaration.InvoiceLines.Count);

			var invoiceImported1 = consolidatedDeclaration.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == "INV1");
			AssertEquals("XJ59342838", invoiceImported1.US_ReleaseEntryNumber);
			var invoiceImported2 = consolidatedDeclaration.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == "INV2");
			AssertEquals("SV98382439", invoiceImported2.US_ReleaseEntryNumber);

			new ReleaseEntryInvoiceRetriever(consolidatedDeclaration).ImportDeclarations(releaseDeclarations);
			AssertEquals("No invoice has been retrieved", 2, consolidatedDeclaration.Invoices.Count);
			AssertEquals("No invoice line has been retrieved", 4, consolidatedDeclaration.InvoiceLines.Count);
		}

		[ExpectNoExceptions]
		public void TestImportInvoice_TwoDeclarationAreSame()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			Factory.Save();

			new ReleaseEntryInvoiceRetriever(declaration).ImportInvoice(declaration, null);
		}
	}
}
