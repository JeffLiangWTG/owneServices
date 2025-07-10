using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceChargeApportionAgainstGroupChargeTest : TestCaseWithFactory
	{
		public void TestChangingChargeTypeClearsApportionedChargeAndReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice2.GroupCharges[0].J7_Amount);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 30m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
		}

		public void TestChangingIsDutiableClearsApportionedChargeAndReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice2.GroupCharges[0].J7_Amount);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 30m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_IsDutiable = !invoiceCharge.J7_IsDutiable;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
		}

		public void TestChangingIsGSTClearsApportionedChargAndReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("PreCondition:Apportioned charge", 50m, invoice2.GroupCharges[0].J7_Amount);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 30m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_IsGSTApplicable = !invoiceCharge.J7_IsGSTApplicable;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
		}

		public void TestChangingOneOfApportionKeyForMultiLevelGroup()
		{
			BaseJobComInvHeaderCharge topOTH = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			topOTH.J7_IsDutiable = false;

			BaseJobComInvoiceGroupHeader subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvHeaderCharge subOTH = subGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 60m, topGroupHeader.JobDeclaration.LocalCurrencyCode);

			BaseJobComInvoiceHeader invoiceFromTop = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromTop, 10000m);

			BaseJobComInvoiceHeader invoiceFromSub1 = subGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromSub1, 10000m);
			BaseJobComInvoiceHeader invoiceFromSub2 = subGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoiceFromSub2, 10000m);
			BaseJobComInvHeaderCharge invoiceCharge = invoiceFromSub2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 40m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();

			AssertEquals("InvoiceFromTop has one apportioned charge", 1, invoiceFromTop.GroupCharges.Count);
			AssertEquals("InvoiceFromSub1 has two apportioned charge", 2, invoiceFromSub1.GroupCharges.Count);
			AssertEquals("InvoiceFromSub2 has one apportioned charge", 1, invoiceFromSub2.GroupCharges.Count);

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			testDec.ResumeApportionment();
			AssertEquals("InvoiceFromSub2 has two apportioned charges", 2, invoiceFromSub2.GroupCharges.Count);
		}

		public void TestChangingAmountLeadToReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 30m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_Amount = 60m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 reapportioned charge", 40m, invoice1.GroupCharges[0].J7_Amount);
		}

		public void TestChangingCurrencyLeadToReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 30m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_RX_NKCurrency = "USD";
			testDec.ResumeApportionment();
			Assert("Invoice1 reapportioned charge", invoice1.GroupCharges[0].J7_Amount != 30m);
		}

		public void TestZeroInvoiceChargeWithValidCurrencyClearsApportionedCharge()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 0m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 100m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);
		}

		public void TestClearingCurrencyReapportion()
		{
			BaseJobComInvHeaderCharge charge = topGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvoiceHeader invoice1 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice1, 10000m);
			BaseJobComInvoiceHeader invoice2 = topGroupHeader.JobComInvoiceHeaders.AddNew();
			SetUpInvoice(invoice2, 10000m);

			BaseJobComInvHeaderCharge invoiceCharge = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 60m, topGroupHeader.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 40m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge cleared", 0, invoice2.GroupCharges.Count);

			invoiceCharge.J7_RX_NKCurrency = ZString.Empty;
			testDec.ResumeApportionment();
			AssertEquals("Invoice1 apportioned charge", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 apportioned charge", 50m, invoice2.GroupCharges[0].J7_Amount);
		}

		#region Implementation
		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroupHeader;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			distributeByForExport = Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		void SetUpInvoice(BaseJobComInvoiceHeader invoice, ZDecimal amount)
		{
			invoice.JZ_InvoiceAmount = amount;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
		}
		#endregion
	}
}
