using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceHeaderCollectionApportionmentTest : TestCaseWithFactory
	{
		public void TestBalanceCheckWithPercentage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			BaseJobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LinePrice = 1000m;

			BaseJobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LinePrice = 4000m;

			BaseJobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			line3.JI_LinePrice = 5000m;
			line3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 40m, declaration.LocalCurrencyCode);

			BaseJobComInvHeaderCharge oft = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge ons = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			ons.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			ons.J7_Percentage = 10m;

			declaration.ResumeApportionment();

			AssertEquals(12m, line1.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, invoice.LocalCurrency));
			AssertEquals(48m, line2.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, invoice.LocalCurrency));
			AssertEquals(100m, invoice.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, invoice.LocalCurrency));

			AssertEquals(100m, line1.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, invoice.LocalCurrency));
			AssertEquals(400m, line2.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, invoice.LocalCurrency));
			AssertEquals(500m, line3.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, invoice.LocalCurrency));
			AssertEquals(1000m, invoice.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, invoice.LocalCurrency));

			string message;
			AssertEquals("Apportionment is balanced", true, declaration.Invoices.AreChargesBalancedForInvoices(out message));
		}

		public void TestDeletingInvoiceReapportionGroupCharge()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice1.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "FOB";

			BaseJobComInvHeaderCharge oTH = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300m, testDec.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Invoice1 has an apportioned OTH", 150m, invoice1.GroupCharges.GetCharge(oTH.ChargeKey).Amount);
			AssertEquals("PreCondition:Invoice2 has an apportioned OTH", 150m, invoice2.GroupCharges.GetCharge(oTH.ChargeKey).Amount);

			testDec.Invoices.Delete(invoice1);
			testDec.ResumeApportionment();
			AssertEquals("Invoice2 has an apportioned OTH", 300m, invoice2.GroupCharges.GetCharge(oTH.ChargeKey).Amount);
		}

		public void TestDefaultIncoTermAndCurrencyDoesntCauseReapportion()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice1.JZ_IncoTerm = "FOB";

			testDec.ResumeApportionment();
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();

			AssertEquals("PreCondition:Incoterm is set", "FOB", invoice2.JZ_IncoTerm);
			AssertEquals("PreCondition:Currency is set", true, invoice2.JZ_RX_NKInvoice_Currency.IsValid);

			AssertEquals("Invoice1 gets apportioned", 1, invoice1.GroupCharges.Count);
			AssertEquals("Invoice2 doesnt get apportioned until users enter invoice amount", 0, invoice2.GroupCharges.Count);
		}

		public void TestAreChargesBalancedForInvoicesEndToEnd()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("Line and Invoice should have a charge apportioned", 100m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("Line and invoice should have a charge apportioned", 100m, line.ApportionedCharges[0].J7_Amount);

			string message;
			AssertEquals("Apportionment is balanced", true, testDec.Invoices.AreChargesBalancedForInvoices(out message));
		}

		public void TestAreChargesBalancedForOneInvoiceWhenThereIsNoCharge()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.Charges.AddNew("OFT");

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			string message;
			AssertEquals("It is balanced as there is no charge", true, testDec.Invoices.AreChargesBalancedForInvoices(out message));
		}

		public void TestAreChargesBalancedForInvoices()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvHeaderCharge oFT1 = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);
			oFT1.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			BaseJobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			var oFT2 = line2.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 250m, testDec.LocalCurrencyCode);
			oFT2.J7_IsNotIncludedInInvoice = true;
			oFT2.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			var oNS1 = line2.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, testDec.LocalCurrencyCode);
			oNS1.J7_IsNotIncludedInInvoice = true;
			oNS1.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			BaseJobComInvHeaderCharge inv1OFT = invoice1.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 350m, testDec.LocalCurrencyCode);
			inv1OFT.J7_IsNotIncludedInInvoice = true;
			inv1OFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			BaseJobComInvHeaderCharge inv1ONS = invoice1.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, testDec.LocalCurrencyCode);
			inv1ONS.J7_IsNotIncludedInInvoice = true;
			inv1ONS.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line3 = invoice2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoice2.JobComInvoiceLines.AddNew();
			var oNS3 = line3.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 40m, testDec.LocalCurrencyCode);
			oNS3.J7_IsNotIncludedInInvoice = true;
			oNS3.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			BaseJobComInvHeaderCharge inv2ONS = invoice2.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, testDec.LocalCurrencyCode);
			inv2ONS.J7_IsNotIncludedInInvoice = true;
			inv2ONS.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			string message;
			bool result = testDec.Invoices.AreChargesBalancedForInvoices(out message);
			AssertEquals("It is not balanced due to Invoice2", false, result);

			var oNS4 = line4.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, testDec.LocalCurrencyCode);
			oNS4.J7_IsNotIncludedInInvoice = true;
			oNS4.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			result = testDec.Invoices.AreChargesBalancedForInvoices(out message);
			AssertEquals("It is balanced now", true, result);
		}

		public void TestTotalNoOfPacks()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 12.5m;
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_NoOfPacks = 87.5m;

			AssertEquals(100m, testDec.Invoices.TotalNoOfPacks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			distributeByForExport = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;
	}
}
