using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceLineChargeApportionForInvoiceChargeTest : TestCaseWithFactory
	{
		public void TestChangingAmountLeadsToReapportion()
		{
			var invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			var line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_Amount = 400m;
			testDec.ResumeApportionment();
			AssertEquals("Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("Line2's apportioned charge amount", 600m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);
		}

		public void TestChangingCurrencyLeadsToRapportion()
		{
			var invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			var line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_RX_NKCurrency = "USD";
			testDec.ResumeApportionment();
			AssertEquals("Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			Assert("Line2's apportioned charge amount", line2.ApportionedCharges[0].J7_Amount != 600m);
			AssertEquals("Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);
		}

		public void TestZeroAmountWithCurrencyClearsApportionedCharge()
		{
			var invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line1 has an apportioned charge", 1, line1.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);

			var invLineOTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 0m, invoice.JobDeclaration.LocalCurrencyCode);
			invLineOTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("Zero amount with a valid currency clears the existing apportioned charge", 0, line1.ApportionedCharges.Count);
			AssertEquals("Apportioned amount for Line 2 should be 1000m", 1000m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestClearingCurrencyReapportion()
		{
			var invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			var line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_RX_NKCurrency = ZString.Empty;
			testDec.ResumeApportionment();
			AssertEquals("Line 1 has an apportioned amount", 1, line1.ApportionedCharges.Count);
			AssertEquals("Line 2 has an apportioned amount", 1, line2.ApportionedCharges.Count);
			AssertEquals("Line 1 has an apportioned amount", 500m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("Line 2 has an apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestChangingChargeTypeReapportion()
		{
			BaseJobComInvHeaderCharge invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			testDec.ResumeApportionment();
			AssertEquals("Line1 now has an apportioned OTH", 1, line1.ApportionedCharges.Count);
			AssertEquals("Line1 's apportioned charge", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
			AssertEquals("Line 1 apportionment amount", 500m, line1.ApportionedCharges[0].J7_Amount);
		}

		public void TestChangingIsDutiableLeadsToReapportion()
		{
			BaseJobComInvHeaderCharge invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_IsDutiable = !line1OTH.J7_IsDutiable;
			testDec.ResumeApportionment();
			AssertEquals("Line1 now has an apportioned OTH", 1, line1.ApportionedCharges.Count);
			AssertEquals("Line1 's apportioned charge", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
			AssertEquals("Line 1 apportionment amount", 500m, line1.ApportionedCharges[0].J7_Amount);
		}

		public void TestChangingIsGSTLeadsToReapportion()
		{
			BaseJobComInvHeaderCharge invOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invOTH.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge line1OTH = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600m, invoice.JobDeclaration.LocalCurrencyCode);
			line1OTH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Line2 has an apportioned charge", 1, line2.ApportionedCharges.Count);
			AssertEquals("PreCondition:Line2's apportioned charge amount", 400m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("PreCondition:Line 1 doesnt have any apportioned amount", 0, line1.ApportionedCharges.Count);

			line1OTH.J7_IsGSTApplicable = !line1OTH.J7_IsGSTApplicable;
			testDec.ResumeApportionment();
			AssertEquals("Line1 now has an apportioned OTH", 1, line1.ApportionedCharges.Count);
			AssertEquals("Line1 's apportioned charge", CustomsChargeTypeList.Codes.OtherCharges, line1.ApportionedCharges[0].J7_ChargeType);
			AssertEquals("Line 1 apportionment amount", 500m, line1.ApportionedCharges[0].J7_Amount);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine line1;
		BaseJobComInvoiceLine line2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;
			line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;
			distributeByForExport = Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		#endregion
	}
}
