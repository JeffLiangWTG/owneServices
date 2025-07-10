using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}

		public override void TestValidateNonDutiablePreFOBChargeWithFCLContainer()
		{
			Assert(true);
		}

		public new void TestValidateChargeTypeForCIFInvoice()
		{
			invoiceHeader.JZ_IncoTerm = "CIF";
			var invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("LCH cannot be on CIF invoice as it is not available in SG", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on CIF  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public new void TestValidateChargeTypeForDDPInvoice()
		{
			invoiceHeader.JZ_IncoTerm = "DDP";
			BaseInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("LCH cannot be on DDP invoice as it is not available in SG", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			AssertEquals("ExWorks cannot be on DDP invoice as it is not available in SG", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public new void TestValidateChargeTypeForEXWInvoice()
		{
			invoiceHeader.JZ_IncoTerm = "EXW";
			BaseInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on EXW invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			AssertEquals("Packing cannot be on EXW invoice as it is not available in SG", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public new void TestValidateChargeTypeForFOBInvoice()
		{
			invoiceHeader.JZ_IncoTerm = "FOB";
			BaseInvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("OFT can be on FOB invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			AssertEquals("FIF cannot be on FOB  invoice as it is not available in SG", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		public void TestValidateONSAndOFTForCIFInvoice()
		{
			invoiceHeader.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight;
			AssertEquals("OFT cannot be on CIF invoice as it will be rejected by SG Customs", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			AssertHasMessageError(invoiceCharge.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
			invoiceCharge.J7_ChargeType = Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance;
			AssertEquals("ONS cannot be on CIF invoice as it will be rejected by SG Customs", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			AssertHasMessageError(invoiceCharge.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoiceCharge2 = invoiceHeader.Charges.AddNew();
			invoiceCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertNoMessageError(invoiceCharge2.J7_ChargeTypeInfo, "Freight and Insurance is not permitted when the INCO Term is CIF.");
		}

		BaseJobComInvoiceHeader invoiceHeader;
		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<BaseJobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
	}
}
