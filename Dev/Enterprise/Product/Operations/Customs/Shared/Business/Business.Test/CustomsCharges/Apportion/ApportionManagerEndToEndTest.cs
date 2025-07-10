using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ApportionManagerEndToEndTest : TestCaseWithFactory
	{
		public void TestClearOrDeleteNonSystemChargesOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "CIF";
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			BaseApportionedCharge apportionedSystemCharge = invoice.GroupCharges.AddNew();
			apportionedSystemCharge.J7_IsSystem = true;
			apportionedSystemCharge.J7_Amount = 100m;
			apportionedSystemCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			var apportionedSystemCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedSystemCharge2.J7_IsSystem = true;
			apportionedSystemCharge2.J7_Amount = 100m;
			apportionedSystemCharge2.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			declaration.TopGroupInvoice.Charges.AddNew("OFT", 20m, declaration.LocalCurrencyCode);

			AssertEquals("Amount still there", 100m, apportionedSystemCharge.J7_Amount);
			AssertEquals("Amount still there", 100m, apportionedSystemCharge2.J7_Amount);
		}

		public void TestWriteChargeDescriptionToApportionedCharges()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				BaseInvoiceCharge charge = invoice.Charges.AddNew();
				charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				charge.J7_Amount = 50m;
				charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
				charge.J7_ChargeDescription = "ASSEMBLY";

				BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;
				declaration.ResumeApportionment();
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				AssertEquals("ASSEMBLY", invoiceLine.ApportionedCharges[0].J7_ChargeDescription);
			}
		}

		public void TestApportionForPercentage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_IncoTerm = "CIF";
			invoice1.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 40000m;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "CFR";
			invoice2.JobComInvoiceLines.AddNew().JI_LinePrice = 40000m;

			declaration.TopGroupInvoice.Charges.AddNew("OFT", 100m, declaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge groupONS = declaration.TopGroupInvoice.Charges.AddNew("ONS");
			groupONS.J7_Percentage = 5m;
			declaration.ResumeApportionment();
			AssertEquals("ONS included in lines for CIF invoice", true, invoice1.GroupCharges.GetCharge("ONS")[0].J7_IsIncludedInITOT);
			AssertEquals("CIF Amount for CIF invoice", 10000m, invoice1.JZ_Calc_CIFAmount);

			AssertEquals("ONS included in lines for CFR invoice", false, invoice2.GroupCharges.GetCharge("ONS")[0].J7_IsIncludedInITOT);
			AssertEquals("CIF Amount for CFR invoice", 42000m, invoice2.JZ_Calc_CIFAmount);
		}

		public void TestApportionGroupOFTWhenInvoiceHasOFT()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
				declaration.TopGroupInvoice.Charges.AddNew("OFT", 500m, "AUD");

				BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 10000m;
				invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice1.JZ_IncoTerm = "CIF";

				BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 40000m;
				invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice2.JZ_IncoTerm = "CIF";
				declaration.ResumeApportionment();
				AssertEquals("Group OFT is apportioned for invoice1", 100m, invoice1.GroupCharges[0].J7_Amount);
				AssertEquals("Group OFT is apportioned for invoice1", true, invoice1.GroupCharges[0].J7_IsIncludedInITOT);

				AssertEquals("Group OFT is apportioned for invoice2", 400m, invoice2.GroupCharges[0].J7_Amount);
				AssertEquals("Group OFT is apportioned for invoice2", true, invoice2.GroupCharges[0].J7_IsIncludedInITOT);

				BaseInvoiceCharge invoiceOFT = invoice1.Charges.AddNew("OFT", 200m, "AUD");
				invoiceOFT.J7_IsIncludedInITOT = false;
				declaration.ResumeApportionment();
				AssertEquals("Group OFT is not apportioned", 0, invoice1.GroupCharges.Count);

				AssertEquals("Group OFT is apportioned for invoice2", 300m, invoice2.GroupCharges[0].J7_Amount);
				AssertEquals("Group OFT is apportioned for invoice2", true, invoice2.GroupCharges[0].J7_IsIncludedInITOT);
			}
		}

		public void TestNoDeveloperExceptionWhenLineChargesAreAggregatedToInvoice()
		{
			ErrorReporter.Clear();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);

			BaseJobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 50m, declaration.LocalCurrencyCode);
			declaration.ResumeApportionment();

			AssertEquals("Invoice Apportioned charges should have 150m", 150m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestValidateJZ_Calc_CIFAmount()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.RunPreSaveValidation();
				AssertHasWarning(invoice.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);

				declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertNoWarning(invoice.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);
			}
		}

		public void TestApportionByWeightOrVolume()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 1000m;
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 2000m;
			line1.JI_Weight = 15m;
			line1.JI_WeightUQ = "KG";
			line1.JI_Volume = 90m;
			line1.JI_VolumeUQ = "M3";

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 8000m;
			line2.JI_Weight = 5000m;
			line2.JI_WeightUQ = "g";
			line2.JI_Volume = 10m;
			line2.JI_VolumeUQ = "M3";

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;
			testDec.ResumeApportionment();
			AssertEquals("OFT is apportioned by volume", 900m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("OFT is apportioned by Volume", 100m, line2.ApportionedCharges[0].J7_Amount);

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			testDec.ResumeApportionment();
			AssertEquals("OFT is apportioned by weight", 750m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("OFT is apportioned by weight", 250m, line2.ApportionedCharges[0].J7_Amount);

			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
			testDec.ResumeApportionment();
			AssertEquals("OFT is apportioned by value", 200m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("OFT is apportioned by value", 800m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestFullApportionment()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
				BaseJobComInvHeaderCharge groupOFT = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3000m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceHeader invoice1_TopGroup = topGroup.JobComInvoiceHeaders.AddNew();
				invoice1_TopGroup.JZ_InvoiceAmount = 10000m;
				invoice1_TopGroup.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvHeaderCharge subGroupOFT = subGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);
				subGroupOFT.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

				BaseJobComInvoiceHeader invoice1_SubGroup = subGroup.JobComInvoiceHeaders.AddNew();
				invoice1_SubGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400m, testDec.LocalCurrencyCode);
				invoice1_SubGroup.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice1_SubGroup.JZ_InvoiceAmount = 10000m;

				BaseJobComInvoiceHeader invoice2_SubGroup = subGroup.JobComInvoiceHeaders.AddNew();
				invoice2_SubGroup.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2_SubGroup.JZ_InvoiceAmount = 10000m;
				testDec.ResumeApportionment();

				AssertEquals("Invoice1 should have only one apportioned charge", 1, invoice1_TopGroup.GroupCharges.Count);
				AssertEquals("Invoice 1 should have (3000 - 400)/2", 1300m, invoice1_TopGroup.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice1_SubGroup should have only one apportioned charge", 1, invoice1_SubGroup.GroupCharges.Count);
				AssertEquals("Invoice1_SubGroup should have 1000/2 apportioned from SubGroupOFT", 500m, invoice1_SubGroup.GroupCharges.GetCharge(subGroupOFT.ApportionChargeKey).Amount);
				AssertEquals("Invoice2_SubGroup should have two apportioned charges", 2, invoice2_SubGroup.GroupCharges.Count);
				AssertEquals("Invoice2_SubGroup should have 1000/2 apportioned from SubGroupOFT", 500m, invoice2_SubGroup.GroupCharges.GetCharge(subGroupOFT.ApportionChargeKey).Amount);
				AssertEquals("Invoice2_SubGroup should have (3000 - 400)/2", 1300m, invoice2_SubGroup.GroupCharges.GetCharge(groupOFT.ApportionChargeKey).Amount);
			}
		}

		/// <summary>
		///													Own OFT
		/// TopGroup											$1,000
		///		|---------- Invoice1_TopGroup						$70
		///		|---------- SubGroup1								$100
		///						|--- Invoice1_SubGroup1					$50
		///						|--- Invoice2_SubGroup1
		///		|---------- SubGroup2								
		///						|--- Invoice1_SubGroup2					
		///						|--- Invoice2_SubGroup2					$9	
		///									|---InvoiceLine					$5
		///									|---InvoiceLine2
		/// </summary>
		public void TestMultiGroupsApportionment()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
				topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceHeader invoice1_TopGroup = topGroup.JobComInvoiceHeaders.AddNew();
				invoice1_TopGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 70m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
				subGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceHeader invoice1_SubGroup1 = subGroup1.JobComInvoiceHeaders.AddNew();
				invoice1_SubGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 50m, testDec.LocalCurrencyCode);
				invoice1_SubGroup1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice1_SubGroup1.JZ_InvoiceAmount = 10000m;

				BaseJobComInvoiceHeader invoice2_SubGroup1 = subGroup1.JobComInvoiceHeaders.AddNew();
				invoice2_SubGroup1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2_SubGroup1.JZ_InvoiceAmount = 10000m;

				BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
				BaseJobComInvoiceHeader invoice1_SubGroup2 = subGroup2.JobComInvoiceHeaders.AddNew();
				invoice1_SubGroup2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice1_SubGroup2.JZ_InvoiceAmount = 10000m;

				BaseJobComInvoiceHeader invoice2_SubGroup2 = subGroup2.JobComInvoiceHeaders.AddNew();
				invoice2_SubGroup2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 9m, testDec.LocalCurrencyCode);
				invoice2_SubGroup2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2_SubGroup2.JZ_InvoiceAmount = 10000m;

				BaseJobComInvoiceLine invoiceLine = invoice2_SubGroup2.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 5000m;
				invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 5m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceLine invoiceLine2 = invoice2_SubGroup2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 5000m;

				testDec.ResumeApportionment();
				AssertEquals("Invoice2_SubGroup1 should have 100 - 50", 50m, invoice2_SubGroup1.GroupCharges[0].J7_Amount);
				AssertEquals("Invoice1_SubGroup2 should have 1000 - 70 - 100 - 9", 821m, invoice1_SubGroup2.GroupCharges[0].J7_Amount);
				AssertEquals("InvoiceLine2 should have 9-5", 4m, invoiceLine2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestWithPercentage()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseInvoiceCharge cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Percentage = 10m;

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 2000m;
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 8000m;
			testDec.ResumeApportionment();

			AssertEquals("Line1 should have 10% of 2000m", 10m, line1.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line1 should have 10% of 2000m", 200m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("Line2 should have 10% of 8000m", 10m, line2.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line2 should have 10% of 8000m", 800m, line2.ApportionedCharges[0].J7_Amount);

			AssertEquals("Commission should have an amount calculated and no apportioned amounts should be there", 0, invoice.GroupCharges.Count);
			AssertEquals("Commission has an amount calculated", 1000m, cOM.J7_Amount);

			line1.JI_LinePrice = 4000m;
			invoice.JZ_InvoiceAmount = 12000m;
			testDec.ResumeApportionment();
			AssertEquals("Line1 should have 10% of 4000m", 10m, line1.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line1 should have 10% of 4000m", 400m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals("Line2 should have 10% of 8000m", 10m, line2.ApportionedCharges[0].J7_Percentage);
			AssertEquals("Line2 should have 10% of 8000m", 800m, line2.ApportionedCharges[0].J7_Amount);

			AssertEquals("Invoice has no apportioned charge", 0, invoice.GroupCharges.Count);
			AssertEquals("Invoice commission is calculated", 1200m, cOM.J7_Amount);

			line1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 150m, testDec.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("Commission with percentage copied down to line1", 1, line1.ApportionedCharges.Count);
			AssertEquals("ApportionedCharge of Commission", 10m, line1.ApportionedCharges[0].J7_Percentage);
			AssertEquals("ApportionedCharge of Commission", 400m, line1.ApportionedCharges[0].J7_Amount);

			AssertEquals("Line2 should still have 10% of 8000m", 800m, line2.ApportionedCharges[0].J7_Amount);
			AssertEquals("Invoice has a Commission with 400m + 800m", 1200m, cOM.J7_Amount);

			AssertEquals("Invoice has a apportioned Commission from invoice line 1", 150m, invoice.GroupCharges[0].J7_Amount);
		}
	}
}
