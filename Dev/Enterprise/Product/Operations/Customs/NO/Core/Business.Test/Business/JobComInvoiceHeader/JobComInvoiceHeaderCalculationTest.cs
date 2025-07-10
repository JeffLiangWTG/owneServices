using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		JobDeclaration Declaration { get; set; }
		JobComInvoiceHeader Invoice => Declaration.Invoices[0];
		JobComInvChargeCollection<GroupInvoiceCharge> GroupCharges => Declaration.TopGroupInvoice.Charges;

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Declaration = Factory.New<JobDeclaration>();

		#region Overseas carges (OFT/ONS) are dutiable in NO

		public override void TestCalculateCIFWithFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;

				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1));
				var preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));

				header.JZ_IncoTerm = "FOB";
				var expected = 10605m;
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);

				PrepareCharge(groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));

				expected = 10605m + 100;
				testDec.ResumeApportionment();
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public override void TestCalculateRealInvoiceTotalWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			var preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);

			header.JZ_IncoTerm = "FOB";
			var expected = 10600m - 100 - 200 + 1;
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
		}

		public override void TestCalculateFOBValueWithCFR()
		{
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", Invoice.JZ_Calc_FOBAmount == 0);
			Invoice.JZ_IncoTerm = Invoice.IncotermEquivalentToCFRForTesting;

			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			Invoice.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600);
			AssertEquals("FOB Value / CFR ", expectedFOB, Invoice.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithCIF()
		{
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", Invoice.JZ_Calc_FOBAmount == 0);
			Invoice.JZ_IncoTerm = "CIF";
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			Invoice.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600);
			AssertEquals("FOB Value / CIF ", expectedFOB, Invoice.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithDDP()
		{
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", Invoice.JZ_Calc_FOBAmount == 0);
			Invoice.JZ_IncoTerm = "DDP";
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);

			Invoice.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600 - 200);
			AssertEquals("FOB Value / DTD ", expectedFOB, Invoice.JZ_Calc_FOBAmount);
		}

		public override void TestCalculateFOBValueWithEXW()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				Assert("No FOB", Invoice.JZ_Calc_FOBAmount == 0);
				Invoice.JZ_IncoTerm = "EXW";
				Invoice.JZ_InvoiceAmount = 10600;
				Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

				GroupCharges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 200, Invoice.JobDeclaration.LocalCurrencyCode);
				GroupCharges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300, Invoice.JobDeclaration.LocalCurrencyCode);
				GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, Invoice.JobDeclaration.LocalCurrencyCode);

				Declaration.ResumeApportionment();
				var expectedFOB = new ZDecimal(10600 + 300 + 200 + 100);
				AssertEquals("FOB Value / EXW ", expectedFOB, Invoice.JZ_Calc_FOBAmount);
			}
		}

		public override void TestCalculateFOBValueWithFOB()
		{
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", Invoice.JZ_Calc_FOBAmount == 0);
			Invoice.JZ_IncoTerm = "FOB";
			Invoice.JZ_InvoiceAmount = 10600;

			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			var expectedFOB = new ZDecimal(10600 + 100);
			AssertEquals("FOB Value / FOB ", expectedFOB, Invoice.JZ_Calc_FOBAmount);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			header = testDec.Invoices.AddNew();
		}
		JobDeclaration testDec;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader header;
	}
}
