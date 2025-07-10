using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderCalculationTest : TestCaseWithFactory
	{
		public void TestChangeIncotermResetIncludedInInvoiceFlagsForInvoiceCharges()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseInvoiceCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);
			BaseInvoiceCharge oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, testDec.LocalCurrencyCode);
			BaseInvoiceCharge cOM = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvHeaderCharge line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("IsIncludedInInvoice is set for OFT", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", false, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for COM", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for Line1 Charge", false, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);

			cOM.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoice.JZ_IncoTerm = "CIF";

			AssertEquals("IsIncludedInInvoice is set for OFT", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is set for ONS", true, oNS.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice for COM stayed as users entered", false, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice is refreshed for Line1 Charge for the incoterm change", true, line1OFT.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestPaymentCurrencyConverterDataProvider()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 2);

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			AssertEquals("PaymentCurrencyConverter.DateOfValuation", new ZDateTime(2005, 1, 2), invoice.PaymentCurrencyConverter.DateForRate);
			AssertEquals("PaymentCurrencyConverter.RateType", ZArchitecture.Core.ExchangeRateType.Buy, invoice.PaymentCurrencyConverter.RateType);
			AssertEquals("PaymentCurrencyConverter.MaximumDaysToFallBack", 7, invoice.PaymentCurrencyConverter.MaximumDaysToFallback);
		}

		public virtual void TestCalculateFOBValueWithDDP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "DDP";
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);

			header.JZ_InvoiceAmount = 10600;
			ZDecimal expectedFOB = new ZDecimal(10600 - 10 - 100 - 200);
			AssertEquals("FOB Value / DTD ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public virtual void TestCalculateFOBValueWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "CIF";
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.JZ_InvoiceAmount = 10600;
			ZDecimal expectedFOB = new ZDecimal(10600 - 10 - 100);
			AssertEquals("FOB Value / CIF ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public virtual void TestCalculateFOBValueWithCFR()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			header.JZ_InvoiceAmount = 10600;
			ZDecimal expectedFOB = new ZDecimal(10600 - 100);
			AssertEquals("FOB Value / CFR ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateBalanceWithInvalidCurrencyOnInlandFreight()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 10000;
			testDec.ResumeApportionment();
			AssertEquals("Initial Balance", new ZDecimal(10000), header.JZ_Calc_Balance);

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);
			testDec.ResumeApportionment();
			AssertEquals("Balance after adding Inland Freight (defaulted currency)", new ZDecimal(9000), header.JZ_Calc_Balance);
		}

		public virtual void TestCalculateFOBValueWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = "FOB";
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			ZDecimal expectedFOB = new ZDecimal(10600);
			AssertEquals("FOB Value / FOB ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public virtual void TestCalculateFOBValueWithEXW()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
				header.JZ_IncoTerm = "EXW";
				header.JZ_InvoiceAmount = 10600;
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 200, header.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300, header.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode);

				ZDecimal expectedFOB = 10600 + 300 + 200;
				testDec.ResumeApportionment();
				AssertEquals("FOB Value / EXW ", expectedFOB, header.JZ_Calc_FOBAmount);
			}
		}

		public void TestEXWInvoiceAmountDoesNotGetCalculatedIfSet()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(0m, header.JZ_InvoiceAmount);
			header.JZ_InvoiceAmount = 500;
			AssertEquals(500m, header.JZ_InvoiceAmount);
			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100;
			AssertEquals(500m, header.JZ_InvoiceAmount);
		}

		public void TestCalculateRealInvoiceTotal()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 5);

			//FOB Incoterm
			header.JZ_IncoTerm = "FOB";
			ZDecimal expected = new ZDecimal(10600 - 200 - 300 + 1 - 5);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);

			//DDP Incoterm
			header.JZ_IncoTerm = "DDP";
			expected = new ZDecimal(10600 - 200 - 300 - 100 - 200 + 1 - 5);
			AssertEquals("Real Invoice / DDP ", expected, header.InvoiceLineTotal);

			//CIF Incoterm
			header.JZ_IncoTerm = "CIF";
			expected = 10600 - 200 - 300 - 100 + 1 - 5 - 10;
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateRealInvoiceTotalWithUFB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = "FOB";

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 250);//not relevant
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 120);//not relevant
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 5);

			ZDecimal expected = 10600m - 300m + 5m;
			AssertEquals("Real Invoice / UFB ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateCIFValueWithEXW()
		{
			header.JZ_InvoiceAmount = 10600;
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "EXW";

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			ZDecimal expectedCIF = 10600m + 300 + 100;
			Assert("CIF Value / EXW ", expectedCIF == header.JZ_Calc_CIFAmount);

			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);

			expectedCIF = 10600m + 300 + 100 + 10;
			Assert("CIF Value / EXW ", expectedCIF == header.JZ_Calc_CIFAmount);
		}

		public virtual void TestCalculateRealInvoiceTotalWithEXW()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;

			header.JZ_IncoTerm = "EXW";
			ZDecimal expected = 10600 - 5 + 1m;
			AssertEquals("Real Invoice / EXW ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateRealInvoiceTotalWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000); // not relevant

			header.JZ_IncoTerm = "FOB";
			ZDecimal expected = 10600m - 100 - 200 - 5 + 1;
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateCIFWithFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;

				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));

				header.JZ_IncoTerm = "FOB";
				ZDecimal expected = 10600m;
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);

				PrepareCharge(groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));

				expected = 10600m + 100;
				testDec.ResumeApportionment();
				AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public virtual void TestCalculateRealInvoiceTotalWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			BaseJobComInvHeaderCharge oFT = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);//irrelevant
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = "CIP";
			ZDecimal expected = 10600m - 100 - 200 - 300 - 5 + 1 - 1000;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);

			header.Charges.RemoveAndDelete(oFT);
			expected = 10600m - 100 - 200 - 300 - 5 + 1;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateCIFWithCIP()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;

				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

				header.JZ_IncoTerm = "CIP";
				ZDecimal expected = 10600m;
				testDec.ResumeApportionment();
				AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);

				BaseJobComInvoiceGroupHeader groupHeader = header.Master;

				PrepareCharge(groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, header.JobDeclaration.LocalCurrencyCode));
				testDec.ResumeApportionment();
				header.GroupCharges[0].J7_IsIncludedInITOT = false;

				expected = 10600m + 100;
				AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public virtual void TestCalculateRealInvoiceTotalWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400));
			PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300));

			header.JZ_IncoTerm = "CIF";
			ZDecimal expected = 10600m - 400 - 300;
			AssertEquals("ITOT / CIF ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateCIFWithCFR()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_InvoiceAmount = 10600;

				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100));
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1));
				BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
				PrepareCharge(preOTH);
				preOTH.J7_IsDutiable = true;
				preOTH.J7_IsGSTApplicable = true;
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200));
				PrepareCharge(header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400));

				header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
				ZDecimal expected = 10600m;
				testDec.ResumeApportionment();
				AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);

				PrepareCharge(groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, header.JobDeclaration.LocalCurrencyCode));
				testDec.ResumeApportionment();
				expected = 10600m + 100;
				AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);
			}
		}

		public virtual void TestCalculateRealInvoiceTotalWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			BaseJobComInvHeaderCharge postOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600);
			postOTH.J7_IsDutiable = false;
			postOTH.J7_IsGSTApplicable = false;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 1000);

			header.JZ_IncoTerm = "DDU";

			ZDecimal expected = 10600m - 40 - 100 + 1 - 5 - 600 - 200 - 400 - 300 - 1000;
			AssertEquals("Real Invoice / DDU ", expected, header.InvoiceLineTotal);
		}

		public virtual void TestCalculateCIFWithDDU()
		{
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			BaseJobComInvHeaderCharge preOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			preOTH.J7_IsDutiable = true;
			preOTH.J7_IsGSTApplicable = true;
			BaseJobComInvHeaderCharge postOTH = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600);
			postOTH.J7_IsDutiable = false;
			postOTH.J7_IsGSTApplicable = false;
			postOTH.J7_IsIncludedInITOT = true;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 500);

			header.JZ_IncoTerm = "DDU";

			ZDecimal expected = 10600m - 500 - 600;
			AssertEquals("CIF value / DDU ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestLinesEntered()
		{
			BaseJobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 0);

			line1.JI_LinePrice = 1000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 1000);

			line2.JI_LinePrice = 4000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 5000);

			line3.JI_LinePrice = 5000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 10000);
		}

		public void TestBalance()
		{
			BaseJobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_InvoiceAmount = 10000;
			testDec.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 10000);

			line1.JI_LinePrice = 1000;
			testDec.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 9000);

			line2.JI_LinePrice = 4000;
			testDec.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 5000);

			line3.JI_LinePrice = 5000;
			testDec.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 0);
		}

		public void TestBalanceGroupHeaderChargeAndHeaderCharge()
		{
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, groupHeader.JobDeclaration.LocalCurrencyCode);

			header.JZ_InvoiceAmount = 1000;
			header.JZ_RX_NKInvoice_Currency = groupHeader.JobDeclaration.LocalCurrencyCode;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);

			ZDecimal expected = header.JZ_InvoiceAmount - 200;
			testDec.ResumeApportionment();
			AssertEquals("The balance should be Invoice Total less cost of its own", expected, header.JZ_Calc_Balance);
		}

		protected virtual void PrepareCharge(BaseJobComInvHeaderCharge charge)
		{
		}

		#region Implementation
		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader header;

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec = GetNewDeclaration();
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			header = testDec.Invoices.AddNew();
		}
		#endregion
	}
}
