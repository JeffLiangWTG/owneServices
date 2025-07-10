using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvoiceLineCalculationTest : TestCaseWithFactory
	{
		public void TestLinePriceIsCalculated()
		{
			line.JI_InvoiceQuantity = 100m;
			line.JI_LinePrice = ZDecimal.Zero;
			AssertEquals("Line.UnitPrice", ZDecimal.Zero, line.UnitPrice);

			line.JI_LinePrice = 1500m;
			AssertEquals("Line.UnitPrice", 15m, line.UnitPrice);

			line.JI_InvoiceQuantity = 200m;
			AssertEquals("Line.UnitPrice", 7.5m, line.UnitPrice);
			AssertEquals("Line.JI_LinePrice", 1500m, line.JI_LinePrice);

			line.UnitPrice = 20m;
			AssertEquals("Line.JI_InvoiceQuantity", 200m, line.JI_InvoiceQuantity);
			AssertEquals("Line.JI_LinePrice", 4000m, line.JI_LinePrice);
		}

		public void TestProportionOfEntryLine()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = Factory.New<BaseJobDeclaration>();

				var entryHeader = testDec.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();

				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var invoice2 = testDec.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 2000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				var oFT = invoice2.Charges.AddNew("OFT", 900m);
				oFT.J7_IsIncludedInITOT = true;
				var oNS = invoice2.Charges.AddNew("ONS", 100m);
				oNS.J7_IsIncludedInITOT = true;
				invoice2.JZ_IncoTerm = "CIF";

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_LinePrice = 1000m;

				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;
				invoiceLine2.JI_LinePrice = 2000m;

				entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100m;
				testDec.ResumeApportionment();
				AssertEquals("Duty apportioned Amount for invoice line", 50m, invoiceLine.JI_Calc_DutyAmount);
				AssertEquals("Duty apportioned Amount for invoice line2", 50m, invoiceLine2.JI_Calc_DutyAmount);
			}
		}

		public void TestApportionmentDirtyOnUnitPriceChange()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_InvoiceQuantity = 1m;
			testDec.ApportionmentDirty = false;

			line.UnitPrice = 100m;
			AssertEquals("Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyOnLinePriceChange()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			var line = invoice.JobComInvoiceLines.AddNew();
			testDec.ApportionmentDirty = false;

			line.JI_LinePrice = 100m;
			AssertEquals("Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyOnParentInvoiceChange()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_InvoiceNumber = "INV1";

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice2.JZ_InvoiceNumber = "INV2";

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100m;
			testDec.ApportionmentDirty = false;

			line.JI_Calc_Invoice = invoice2.JZ_InvoiceNumber;
			AssertEquals("Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyOnLineChargesChange()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			var line = invoice.JobComInvoiceLines.AddNew();
			testDec.ApportionmentDirty = false;

			line.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			AssertEquals("Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestFOBValueWithDTDLinesPreExw()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();

				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_IncoTerm = "DDP";
				header.JZ_InvoiceAmount = 10600;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Line total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithDTDLinesExw()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "DDP";
				header.JZ_InvoiceAmount = 10600;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 240);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Line total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithDTDLinesFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "DDP";
				header.JZ_InvoiceAmount = 10360;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Line total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithDTDLinesCIF()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "DDP";
				header.JZ_InvoiceAmount = 9800;
				header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Line total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 9600m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithDTDLinesDDP()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "DDP";
				header.JZ_InvoiceAmount = 9600;
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Line total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithCIFLinePreEXW()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIF";
			header.JZ_InvoiceAmount = 10400;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineEXW()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIF";
			header.JZ_InvoiceAmount = 10200;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9640m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10200m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 964m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3856m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4820m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineFOB()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIF";
			header.JZ_InvoiceAmount = 10160;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 60);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Header CIF", 10160m, header.JZ_Calc_CIFAmount);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIFLineCIF()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIF";
			header.JZ_InvoiceAmount = 9600;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LinePreEXW()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
			header.JZ_InvoiceAmount = 10340;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9840m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10340m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 984m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3936m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4920m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIF_LinePreEXWWithOverseasInsurance()
		{
			var groupHeader = jobDec.JobComInvoiceGroupHeaders[0];

			var oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 35.25m, jobDec.LocalCurrencyCode);
			oNS.J7_IsIncludedInITOT = true;
			var oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 7095m, jobDec.LocalCurrencyCode);
			oFT.J7_IsIncludedInITOT = true;

			var line1 = header.JobComInvoiceLines.AddNew();

			header.JZ_IncoTerm = "CIF";
			header.JZ_InvoiceAmount = 13600;
			header.JZ_RX_NKInvoice_Currency = jobDec.LocalCurrencyCode;

			line1.JI_LinePrice = 13600;
			jobDec.ResumeApportionment();
			AssertEquals("Line 1 FOB", 6469.75m, line1.JI_Calc_FOB, 0.01m);
		}

		public void TestFOBValueWithCFR_LineEXW()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
			header.JZ_InvoiceAmount = 10140;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9640m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10140m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 964m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3856m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4820m, line3.JI_Calc_FOB);
		}

		public void TestCIFValueWithCIF()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var groupHeader = header.GroupHeader;

				var oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 38.62m, header.JobDeclaration.LocalCurrencyCode);
				oNS.J7_IsIncludedInITOT = true;
				var oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120.73m, header.JobDeclaration.LocalCurrencyCode);
				oFT.J7_IsIncludedInITOT = true;

				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;
				header.JZ_IncoTerm = "CIF";
				header.JZ_InvoiceAmount = 15570.81m;
				line1.JI_LinePrice = 15570.81m;
				jobDec.ResumeApportionment();
				AssertEquals("Line FOB", 15411.46m, line1.JI_Calc_FOB, 0.01m);
				AssertEquals("Line CIF", 15570.81m, line1.JI_Calc_CIF, 0.01m);
			}
		}

		public void TestFOBValueWithCFR_LineFOB()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
			header.JZ_InvoiceAmount = 10100;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10100m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LineFOBWithGroupCharge()
		{
			var groupHeader = header.Master;
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
			header.JZ_InvoiceAmount = 9600;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

			var oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, header.JobDeclaration.LocalCurrencyCode);
			oFT.J7_IsIncludedInITOT = true;

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9100m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 910m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3640m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4550m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCFR_LineCFR()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
			header.JZ_InvoiceAmount = 9600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9600m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9600m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 960m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3840m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4800m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LinePreEXW()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "CIP";
				header.JZ_InvoiceAmount = 10640;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 10140m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 10640m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 1014m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 4056m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 5070m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueWithCIP_LineEXW()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIP";
			header.JZ_InvoiceAmount = 10440;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9940m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10440m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 994m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3976m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4970m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LineFOB()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "CIP";
			header.JZ_InvoiceAmount = 10400;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9900m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10400m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 990m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3960m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4950m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueWithCIP_LineCIPWithGroupChargeInsurance()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvoiceGroupHeader groupHeader = header.Master;
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "CIP";
				header.JZ_InvoiceAmount = 9900;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

				var oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, header.JobDeclaration.LocalCurrencyCode);
				oNS.J7_IsIncludedInITOT = true;

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 9400m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 9900m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 940m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3760m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4700m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueForFOB_LinePreEXW()
		{
			var groupHeader = header.Master;
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "FOB";
			header.JZ_InvoiceAmount = 10140;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50, header.JobDeclaration.LocalCurrencyCode);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 10140m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 10190m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 1014m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 4056m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 5070m, line3.JI_Calc_FOB);
		}

		public void TestFOBValueForFOB_LineEXW()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var groupHeader = header.Master;
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "FOB";
				header.JZ_InvoiceAmount = 9940;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 40);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50, header.JobDeclaration.LocalCurrencyCode);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 9940m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 9990m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 994m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3976m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4970m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueForFOB_LineFOB()
		{
			var groupHeader = header.Master;
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

			header.JZ_IncoTerm = "FOB";
			header.JZ_InvoiceAmount = 9900;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300);

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50, header.JobDeclaration.LocalCurrencyCode);

			line1.JI_LinePrice = 1000;
			line2.JI_LinePrice = 4000;
			line3.JI_LinePrice = 5000;
			jobDec.ResumeApportionment();
			AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
			AssertEquals("Header FOB", 9900m, header.JZ_Calc_FOBAmount);
			AssertEquals("Header CIF", 9950m, header.JZ_Calc_CIFAmount);
			AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
			AssertEquals("Line 1 FOB", 990m, line1.JI_Calc_FOB);
			AssertEquals("Line 2 FOB", 3960m, line2.JI_Calc_FOB);
			AssertEquals("Line 3 FOB", 4950m, line3.JI_Calc_FOB);
		}

		public void TestFOB_CIFValueForCFRWithNotIncludedONS()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = header.IncotermEquivalentToCFRForTesting;
				header.JZ_InvoiceAmount = 9600;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

				var oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 300, header.JobDeclaration.LocalCurrencyCode);
				oFT.J7_IsIncludedInITOT = true;

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 30, header.JobDeclaration.LocalCurrencyCode);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 9300m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 9630m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 930m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 CIF", 963m, line1.JI_Calc_CIF);
				AssertEquals("Line 2 FOB", 3720m, line2.JI_Calc_FOB);
				AssertEquals("Line 2 CIF", 3852m, line2.JI_Calc_CIF);
				AssertEquals("Line 3 FOB", 4650m, line3.JI_Calc_FOB);
				AssertEquals("Line 3 CIF", 4815m, line3.JI_Calc_CIF);
			}
		}

		public void TestFOBValueForEXW_LinePreEXW()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var groupHeader = header.Master;
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "EXW";
				header.JZ_InvoiceAmount = 9600;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 200, header.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 300, header.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200, header.JobDeclaration.LocalCurrencyCode);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 10000m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 10300m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 1000m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 4000m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 5000m, line3.JI_Calc_FOB);
			}
		}

		public void TestFOBValueForEXW_LineEXW()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var groupHeader = header.Master;
				var line1 = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				var line3 = header.JobComInvoiceLines.AddNew();
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.JZ_IncoTerm = "EXW";
				header.JZ_InvoiceAmount = 9600;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 400);

				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 300, header.JobDeclaration.LocalCurrencyCode);
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 200, header.JobDeclaration.LocalCurrencyCode);

				line1.JI_LinePrice = 1000;
				line2.JI_LinePrice = 4000;
				line3.JI_LinePrice = 5000;
				jobDec.ResumeApportionment();
				AssertEquals("Header Invoice Line Total", 10000m, header.InvoiceLineTotal);
				AssertEquals("Header FOB", 9800m, header.JZ_Calc_FOBAmount);
				AssertEquals("Header CIF", 10100m, header.JZ_Calc_CIFAmount);
				AssertEquals("Header Balance", 0m, header.JZ_Calc_Balance);
				AssertEquals("Line 1 FOB", 980m, line1.JI_Calc_FOB);
				AssertEquals("Line 2 FOB", 3920m, line2.JI_Calc_FOB);
				AssertEquals("Line 3 FOB", 4900m, line3.JI_Calc_FOB);
			}
		}

		public void TestForeignInlandFreightAffectLineFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				header.JZ_IncoTerm = "FOB";
				header.JZ_InvoiceAmount = 10000m;
				header.JZ_RX_NKInvoice_Currency = header.JobDeclaration.LocalCurrencyCode;

				header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);

				line.JI_LinePrice = 9000m;
				jobDec.ResumeApportionment();
				AssertEquals("Line FOB", 10000m, line.JI_Calc_FOB);
			}
		}

		[ExpectNoExceptions]
		public void TestDeletingLineWhenLoadedInAnotherFactoryDoesNotGiveDeletedRowException()
		{
			var organisation = OrgHeader.New(Factory);
			organisation.OH_Code = "Code";
			MasterFiles.Business.OrgSupplierPart part = MasterFiles.Business.OrgSupplierPart.New(Factory);
			string partNumber = "PART";
			part.OP_PartNum = partNumber;
			part.RelatedOrganisations.AddOrganisationIfNotExist(organisation.PK, OrgPartRelation.RelationshipTypes.Supplier);
			line.JI_PartNo = part.OP_PartNum;
			AssertNotNull("Precondition : Part number on line", line.JI_OP);
			Factory.Save();
			part.OP_Desc = "Description"; // Force change on part - should attempt to update part in other factory
			var factory2 = new BusinessObjectFactory();
			factory2.Load(typeof(BaseJobComInvoiceLine), line.PK);
			line.Delete();
			factory2.Save();
		}

		public void TestPartIsNotNullOnLoadOfExistingInvoiceLine()
		{
			var part = SetupGoodToGoPart();
			line.JI_PartNo = part.OP_PartNum;
			AssertNotNull("Precondition : Part number on line", line.Part);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var lineInFactory2 = factory2.Load<BaseJobComInvoiceLine>(line.PK);
			AssertNotNull(lineInFactory2.Part);
			AssertEquals(line.Part.PK, lineInFactory2.Part.PK);
		}

		public void TestDeletingPartInAnotherFactoryRemovesFromInvoiceLine()
		{
			var part = SetupGoodToGoPart();
			Factory.Save();
			var partNumber = part.OP_PartNum;
			line.JI_PartNo = partNumber;
			AssertNotNull("Part", line.Part);

			var factory2 = new BusinessObjectFactory();
			var partInAnotherFactory = factory2.Load(typeof(OrgSupplierPart), part.PK);
			partInAnotherFactory.Delete();
			factory2.Save();

			AssertEquals("Part number should still exist", partNumber, line.JI_PartNo);
			AssertEquals("Part foreign key should be empty", ZGuid.Empty, line.JI_OP);
			AssertNull("Part", line.Part);

			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var lineReloaded = factory3.Load<BaseJobComInvoiceLine>(line.PK);
			AssertEquals("Part number should still exist", partNumber, lineReloaded.JI_PartNo);
			AssertEquals("Part foreign key should be empty", ZGuid.Empty, lineReloaded.JI_OP);
			AssertNull("Part", lineReloaded.Part);
		}

		public void TestDeletingPartReturnsNullFromCachedPart()
		{
			var organisation = OrgHeader.New(Factory);
			organisation.OH_Code = "Code";
			MasterFiles.Business.OrgSupplierPart part = MasterFiles.Business.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "UNIQUEPARTNUMBER";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = organisation.PK;
			Factory.Save();
			header.JZ_OH_Supplier = organisation.PK;
			line.JI_PartNo = part.OP_PartNum;
			AssertNotNull("Precondition : Part number on line", line.JI_OP);
			AssertNotNull("Precondition", line.Part);
			part.Delete();
			AssertNull("Part should now be null", line.Part);
		}

		public void TestLockCustomsQty()
		{
			line.JI_InvoiceUQ = "";
			line.JI_CustomsUnitQty = "KG";
			AssertEquals("Customs Qty open", false, line.JI_CustomsQuantityInfo.ReadOnly);

			line.JI_CustomsUnitQty = "";
			AssertEquals("Customs Qty is not locked", false, line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestInvoiceAndLineReference()
		{
			AssertEquals("invoice INV1 line 1", line.InvoiceAndLineReference);
		}

		public void TestJI_Calc_FOBInForeignCurrency()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				SetExchangeRate(ZDateTime.Now, ZDateTime.Now.AddYears(10), 0.5m, uSDCurrency, "CUS");

				header.JZ_InvoiceAmount = 10000m;
				header.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
				header.JZ_IncoTerm = "CIF";

				header.Charges.RemoveAll();
				var oFT = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m);
				oFT.J7_IsIncludedInITOT = true;
				header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m);

				line.JI_LinePrice = 9950m;
				jobDec.ResumeApportionment();
				AssertEquals("FOB Value of line", line.JI_Calc_FOB, 9450m);
			}
		}

		public void TestChangingJI_Calc_InvoiceSetsLineNo()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";

			var invoiceLine = testDec.FilteredInvoiceLines.AddNew();
			AssertEquals("PreCondition:Belongs to invoice1", invoice1.JZ_InvoiceNumber, invoiceLine.JI_Calc_Invoice);
			AssertEquals("Line No", (ZShort)1, invoiceLine.JI_LineNo);

			var invoiceLine2 = testDec.FilteredInvoiceLines.AddNew();
			AssertEquals("PreCondition:Belongs to invoice1", invoice1.JZ_InvoiceNumber, invoiceLine2.JI_Calc_Invoice);
			AssertEquals("Line No", (ZShort)2, invoiceLine2.JI_LineNo);

			invoiceLine2.JI_Calc_Invoice = invoice2.JZ_InvoiceNumber;
			AssertEquals("PreCondition:Belongs to invoice2 now", invoice2.JZ_InvoiceNumber, invoiceLine2.JI_Calc_Invoice);
			AssertEquals("Line No", (ZShort)1, invoiceLine2.JI_LineNo);

			invoiceLine.JI_Calc_Invoice = invoice2.JZ_InvoiceNumber;
			AssertEquals("Line no for invoice line 1", (ZShort)2, invoiceLine.JI_LineNo);
		}

		public void TestJI_Calc_GSTVATDeferred()
		{
			jobDec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			jobDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			line.JI_Tariff = "0101.1000/25";
			line.JI_LinePrice = 1m;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0101.1000/25";
			line2.JI_LinePrice = 2m;
			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0101.1000/25";
			line3.JI_LinePrice = 3m;

			AssertEquals("PreCondition : Invoice Line Count", 3, jobDec.FilteredInvoiceLines.Count);

			header.JZ_InvoiceAmount = 30m;
			header.JZ_RX_NKInvoice_Currency = jobDec.LocalCurrencyCode;
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDec.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			jobDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			jobDec.DoMerge();
			AssertEquals("PreCondition : Entries", 1, jobDec.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition : Merged Lines", 1, jobDec.CustomsEntryHeaders[0].MergedLines.Count);
			line.CusEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 30m);

			AssertEquals(BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred, 5m, line[BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred]);
			AssertEquals(BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred, 10m, line2[BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred]);
			AssertEquals(BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred, 15m, line3[BaseJobComInvoiceLine.Schema.JI_Calc_GSTVATDeferred]);
		}

		// WI00004142
		[ExpectNoExceptions()]
		public void TestNotCachingCurrencyConverterFromADeletedInvoice()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoice1Oft = invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			invoice1Oft.J7_Amount = 100m;
			invoice1Oft.J7_RX_NKCurrency = nzd.RX_Code;
			var invoiceline1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceline1.JI_LinePrice = 600m;
			var invoiceline1Oft = invoiceline1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			invoiceline1Oft.J7_Amount = 60m;
			var invoiceline2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceline2.JI_LinePrice = 400m;
			var invoiceline2Oft = invoiceline2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			invoiceline2Oft.J7_Amount = 40m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice2.JZ_InvoiceAmount = 1000m;

			var cif = invoiceline1.JI_Calc_CIF;
			var invoice2Oft = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			invoice2Oft.J7_Amount = 100m;
			invoice2Oft.J7_RX_NKCurrency = nzd.RX_Code;
			Factory.Save();
			invoiceline1.JI_JZ = invoice2.PK;
			invoiceline2.JI_JZ = invoice2.PK;
			invoice1.Delete();
			Factory.Save();
			var invoiceline1Oth = invoiceline1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges);
			invoiceline1Oth.J7_Amount = 50m;
			invoiceline1Oth.J7_RX_NKCurrency = nzd.RX_Code;
			cif = invoiceline1.JI_Calc_CIF;
		}

		public void TestSettingDescripionWhenClassificationChanges()
		{
			var classificationGranny = Factory.New<BaseCusClassification>();
			classificationGranny.FillWithValidTestData();
			classificationGranny.CC_TariffNum = "0001.01.01 1";
			classificationGranny.CC_LookupCode = "GRANNY";
			classificationGranny.CC_Description = "GRANNY APPLE";
			classificationGranny.CC_ClassificationType = "BTH";
			var classificationJonno = Factory.New<BaseCusClassification>();
			classificationJonno.FillWithValidTestData();
			classificationJonno.CC_TariffNum = "0002.02.02 1";
			classificationJonno.CC_LookupCode = "JONNO";
			classificationJonno.CC_Description = "JONNO APPLE";
			classificationJonno.CC_ClassificationType = "BTH";

			line.JI_CC = classificationGranny.PK;
			AssertEquals("Line description should be set", "GRANNY APPLE", line.JI_Description);
			line.JI_CC = classificationJonno.PK;
			AssertEquals("Line description should be changed", "JONNO APPLE", line.JI_Description);
			line.JI_Description = "NOT DEFAULT DESCRIPTION";
			AssertEquals("Line description should be overriden", "NOT DEFAULT DESCRIPTION", line.JI_Description);
			line.JI_CC = classificationGranny.PK;
			AssertEquals("Line description should not change", "NOT DEFAULT DESCRIPTION", line.JI_Description);
			line.JI_CC = classificationJonno.PK;
			AssertEquals("Line description should not change", "NOT DEFAULT DESCRIPTION", line.JI_Description);
		}

		public void TestManufacturerAddressBindingList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var descriptor = invoiceLine.JI_OA_ManufacturerAddressInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertNotNull(listAttribute);
			AssertEquals("JI_OA_ManufacturerAddress_ZAddress.OrgAddress_List", listAttribute.ListDataSourceMember);
		}

		public void TestConsigneeAddressBindingList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var descriptor = invoiceLine.JI_OA_ConsigneeAddressInfo.PropertyDescriptor;
			var listAttribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			AssertNotNull(listAttribute);
			AssertEquals("JI_OA_ConsigneeAddress_ZAddress.OrgAddress_List", listAttribute.ListDataSourceMember);
		}

		BaseJobDeclaration jobDec;
		BaseJobComInvoiceGroupHeader groupHeader;
		BaseJobComInvoiceHeader header;
		BaseJobComInvoiceLine line;

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				jobDec = BaseJobDeclaration.New(Factory);
				groupHeader = jobDec.JobComInvoiceGroupHeaders[0];
				header = jobDec.Invoices.AddNew();
				header.JZ_InvoiceNumber = "Inv1";
				line = jobDec.FilteredInvoiceLines.AddNew();
				line.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			}
		}

		void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, ZString exchangeRateType)
		{
			var sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exchangeRateType);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty != null)
			{
				exchangeRateDuty.Delete();
			}

			var newOne = Factory.New<RefExchangeRate>();
			newOne.RE_ExpiryDate = endDate;
			newOne.RE_ExRateType = exchangeRateType;
			newOne.RE_GC = GlbCompany.CurrentCompany.PK;
			newOne.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			newOne.RE_StartDate = startDate;
			newOne.RE_SellRate = exchangeRate;
		}

		OrgSupplierPart SetupGoodToGoPart()
		{
			var organisation = OrgHeader.New(Factory);
			organisation.OH_Code = "Code";
			header.JZ_OH_Supplier = organisation.PK;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "SUP";
			relation.OU_OH = organisation.PK;

			return part;
		}
	}
}
