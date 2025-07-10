using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;
using static Enterprise.Customs.US.Business.IDutyDataExtensionMethod;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DDPDisbursementSupAdditionalDutyDataTest : DDPDisbursementDutyDataBaseTest
	{
		public void TestParentTariffLine()
		{
			var supAdditionalDutyData1 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
			AssertNull(supAdditionalDutyData1.ParentTariffLine);
			var supAdditionalDutyData2 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff2, invoiceLine.ImportSupAdditionalTariff2, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff2, true);
			AssertNotNull(supAdditionalDutyData2.ParentTariffLine);
			AssertEquals("99038801", supAdditionalDutyData2.ParentTariffLine.Tariff);
			var supDutyData = new DDPDisbursementSupDutyData(invoiceLine, chargesAndFees, customsValues, true);
			AssertNotNull(supDutyData.ParentTariffLine);
			AssertEquals("99038801", supDutyData.ParentTariffLine.Tariff);
			var lineDutyData = new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, true);
			AssertNotNull(lineDutyData.ParentTariffLine);
			AssertEquals("99038801", lineDutyData.ParentTariffLine.Tariff);
		}

		public void TestSecondaryLines()
		{
			var supAdditionalDutyData1 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
			var secondaryLinesForSupAdditionalDutyData1 = ((IEntryLineOrInvoiceLineDutyData)supAdditionalDutyData1).SecondaryLines.ToArray();
			AssertEquals(3, secondaryLinesForSupAdditionalDutyData1.Length);
			AssertType<DDPDisbursementSupAdditionalDutyData>(secondaryLinesForSupAdditionalDutyData1[0]);
			AssertEquals("99038802", secondaryLinesForSupAdditionalDutyData1[0].Tariff);
			AssertType<DDPDisbursementSupDutyData>(secondaryLinesForSupAdditionalDutyData1[1]);
			AssertEquals("99030123", secondaryLinesForSupAdditionalDutyData1[1].Tariff);
			AssertType<DDPDisbursementLineDutyData>(secondaryLinesForSupAdditionalDutyData1[2]);
			AssertEquals("8424201000", secondaryLinesForSupAdditionalDutyData1[2].Tariff);

			var supAdditionalDutyData2 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff2, invoiceLine.ImportSupAdditionalTariff2, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff2, true);
			var secondaryLinesForSupAdditionalDutyData2 = ((IEntryLineOrInvoiceLineDutyData)supAdditionalDutyData2).SecondaryLines.ToArray();
			AssertEquals(0, secondaryLinesForSupAdditionalDutyData2.Length);
		}

		public void TestGetSupCustomsValue()
		{
			var supAdditionalDutyData1 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
			AssertEquals("Sup customs value for additional tariff 1 is zero", 0m, ((IDutyData)supAdditionalDutyData1).SupCustomsValue);
			var supAdditionalDutyData2 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff2, true);
			AssertEquals("Sup customs value for additional tariff 2 is zero", 0m, ((IDutyData)supAdditionalDutyData2).SupCustomsValue);

			invoiceLine.US_SupAdditionalTariff1GoodsValue = 100m;
			invoiceLine.US_SupAdditionalTariff2GoodsValue = 200m;

			supAdditionalDutyData1 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
			AssertEquals("Sup customs value for additional tariff 1 is 100", 100m, ((IDutyData)supAdditionalDutyData1).SupCustomsValue);
			supAdditionalDutyData2 = new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff2, true);
			AssertEquals("Sup customs value for additional tariff 2 is 200", 200m, ((IDutyData)supAdditionalDutyData2).SupCustomsValue);
		}

		public void TestSecondaryLinesForDerivedLines()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1, "PCS");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12, "DOZ");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030124", "7", 0.2m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038823", "7", 0.25m, "");

			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice.JZ_InvoiceNumber = "INV25031501";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;

			var parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			parentInvoiceLine.JI_FormattedTariff = "8206.00.0000";
			parentInvoiceLine.SupTariffFormatted = "9903.01.24";
			parentInvoiceLine.SupFormattedAdditionalTariff1 = "9903.88.23";
			parentInvoiceLine.JI_LinePrice = 0m;
			var childInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentInvoiceLine.PK;
			childInvoiceLine.JI_FormattedTariff = "8203.20.4000";
			childInvoiceLine.JI_LinePrice = 5000m;

			var supAdditionalDutyData1 = new DDPDisbursementSupAdditionalDutyData(parentInvoiceLine, parentInvoiceLine.US_SupAdditionalTariff1, parentInvoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
			var secondaryLinesForSupAdditionalDutyData1 = ((IEntryLineOrInvoiceLineDutyData)supAdditionalDutyData1).SecondaryLines.ToArray();
			AssertEquals(3, secondaryLinesForSupAdditionalDutyData1.Length);
			AssertType<DDPDisbursementSupDutyData>(secondaryLinesForSupAdditionalDutyData1[0]);
			AssertEquals("99030124", secondaryLinesForSupAdditionalDutyData1[0].Tariff);
			AssertType<DDPDisbursementLineDutyData>(secondaryLinesForSupAdditionalDutyData1[1]);
			AssertEquals("8206000000", secondaryLinesForSupAdditionalDutyData1[1].Tariff);
			AssertType<DDPDisbursementLineDutyData>(secondaryLinesForSupAdditionalDutyData1[2]);
			AssertEquals("8203204000", secondaryLinesForSupAdditionalDutyData1[2].Tariff);
		}

		#region Overrides

		internal override DDPDisbursementDutyDataBase GetDDPDisbursementDutyDataForTest(JobComInvoiceLine invoiceLine)
		{
			return new DDPDisbursementSupAdditionalDutyData(invoiceLine, "99038803", null, chargesAndFees, customsValues, TariffTypeForDDP.AdditionalTariff1, true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			chargesAndFees = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038801", "7", 0.25m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "0", 0m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8424201000";
			invoiceLine.US_SupTariff = "99030123";
			invoiceLine.SupFormattedAdditionalTariff1 = "99038801";
			invoiceLine.SupFormattedAdditionalTariff2 = "99038802";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees;
		Dictionary<JobComInvoiceLine, CustomsValues> customsValues;

		#endregion
	}
}
