using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		Common.JobComInvChargeCollection<InvoiceCharge> Charges => invoice.Charges;

		public void TestCheckJ7_IsDutiable()
		{
			InvoiceCharge charge;
			CombineAssertions(() =>
			{
				charge = Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				charge.J7_IsDutiable = true;
				AssertNoMessageErrors("When ChargeCode == OFT", charge.J7_IsDutiableInfo);

				charge = Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
				charge.J7_IsDutiable = true;
				AssertNoMessageErrors("When ChargeCode == ONS", charge.J7_IsDutiableInfo);
			});
		}

		public override void TestValidateChargeTypeForCIFInvoice()
		{
			invoice.JZ_IncoTerm = "CIF";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				AssertEquals("LCH cannot be on CIF invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("OFT can be on CIF  invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			});
		}

		public override void TestValidateChargeTypeForDDPInvoice()
		{
			invoice.JZ_IncoTerm = "DDP";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				AssertEquals("LCH can be on DDP invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
				AssertEquals("ExWorks can be on DDP invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			});
		}

		public override void TestValidateChargeTypeForEXWInvoice()
		{
			invoice.JZ_IncoTerm = "EXW";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("OFT cannot be on EXW invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
				AssertEquals("Packing can be on EXW invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			});
		}

		public override void TestValidateChargeTypeForFOBInvoice()
		{
			invoice.JZ_IncoTerm = "FOB";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			CombineAssertions(() =>
			{
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("OFT cannot be on FOB invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				AssertEquals("FIFT can be on FOB  invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec = base.Factory.New<JobDeclaration>();
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
	}
}
