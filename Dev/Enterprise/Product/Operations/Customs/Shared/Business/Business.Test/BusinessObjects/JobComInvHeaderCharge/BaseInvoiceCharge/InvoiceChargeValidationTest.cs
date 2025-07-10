using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	public class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestValidateAllInvoiceLinesHaveDistributeByFields()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvHeaderCharge invCharge = invoice.Charges.AddNew();
			invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			invCharge.J7_Amount = 100m;
			invCharge.J7_RX_NKCurrency = "AUD";

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Weight = 0.30m;
			line1.JI_WeightUQ = "KG";

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 0m;
			line2.JI_WeightUQ = "";

			invCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			AssertHasMessageError(invCharge.J7_DistributeByInfo, "There are invoice lines that don't have a weight. The apportionment of this charge won't be correct for the invoice lines.");

			line2.JI_Weight = 0.3m;
			line2.JI_WeightUQ = "KG";
			invCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			AssertNoMessageError(invCharge.J7_DistributeByInfo, "There are invoice lines that don't have a weight. The apportionment of this charge won't be correct for the invoice lines.");
		}

		public void TestValidateChargeTypeWhichEnteredInvalid()
		{
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "XX";
			AssertEquals("Invalid Charge Type", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateChargeTypeForEXWInvoice()
		{
			invoice.JZ_IncoTerm = "EXW";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT cannot be on EXW invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			AssertEquals("Packing can be on EXW invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateChargeTypeForFOBInvoice()
		{
			invoice.JZ_IncoTerm = "FOB";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT cannot be on FOB invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			AssertEquals("FIFT can be on FOB  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public void TestValidateChargeTypeForCFRInvoice()
		{
			invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("ONS cannot be on CFR invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on CFR  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateChargeTypeForCIPInvoice()
		{
			invoice.JZ_IncoTerm = "CIP";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on CIP invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("ONS can be on CIP  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateChargeTypeForCIFInvoice()
		{
			invoice.JZ_IncoTerm = "CIF";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("LCH cannot be on CIF invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on CIF  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateChargeTypeForDDPInvoice()
		{
			invoice.JZ_IncoTerm = "DDP";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("LCH can be on DDP invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			AssertEquals("ExWorks can be on DDP invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public virtual void TestValidateNonDutiablePreFOBChargeWithFCLContainer()
		{
			invoice.JZ_IncoTerm = "FOB";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			invoiceCharge.J7_IsDutiable = false;
			AssertHasWarning(invoiceCharge.J7_IsDutiableInfo, Customs.Business.JobComInvHeaderChargeValidation.NonDutiablePreFOBChargeForNoFCLContainer);

			BaseCusContainer fCLContainer = testDec.CusContainers.AddNew();
			fCLContainer.CO_FCL_LCL_AIR = "FCL";

			invoiceCharge.RunPreSaveValidation();
			AssertNoWarning(invoiceCharge.J7_IsDutiableInfo, Customs.Business.JobComInvHeaderChargeValidation.NonDutiablePreFOBChargeForNoFCLContainer);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}

		#endregion
	}
}
