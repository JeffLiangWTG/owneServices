using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public override void TestIsIncludedInLinesReadOnly()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			var aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			var dED = invoice.Charges.AddNew();
			dED.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			var oNS = invoice.Charges.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			var fIF = invoice.Charges.AddNew();

			fIF.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			var pAC = invoice.Charges.AddNew();
			pAC.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			var lCH = invoice.Charges.AddNew();
			lCH.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("ADD Included in ITOT should be readonly", true, aDD.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("DED Included in ITOT should be readonly", true, dED.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OFT Included in ITOT should be readonly", true, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("ONS  Included in ITOT should be readonly", true, oNS.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("FIF Included in ITOT should be readonly", false, fIF.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("PAC  Included in ITOT should not be readonly", false, pAC.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("LCH Included in ITOT should be readonly", true, lCH.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public override void TestJ7_Calc_IsIncludedInInvoice()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			var aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			aDD.J7_Amount = 10m;
			aDD.J7_RX_NKCurrency = "AUD";
			AssertEquals("J7_Calc_IsIncludedInInvoice is readonly", true, aDD.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			AssertEquals("J7_IsNotIncludedInInvoice is true", true, aDD.J7_IsNotIncludedInInvoice);
			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 10m;
			oFT.J7_RX_NKCurrency = "AUD";
			AssertEquals("J7_Calc_IsIncludedInInvoice should be readonly", true, oFT.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			AssertEquals("OFT's J7_Calc_IsIncludedInInvoice should be false for FOB Invoice", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("OFT's J7_IsNotIncludedInInvoice should be true for FOB Invoice", true, oFT.J7_IsNotIncludedInInvoice);
			invoice.JZ_IncoTerm = GetIncotermTermFreightCanBeIncluded();
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for ADD should stay", false, aDD.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("J7_Calc_IsIncludedInInvoiceAmount for OFT should be changed", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("J7_IsNotIncludedInInvoice for OFT should be changed", false, oFT.J7_IsNotIncludedInInvoice);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestReapportionAllChargesWhenChargeCodeChargeKeyChanged()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
				var oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100);
				var groupHeader = invoice.Master;
				var lCH = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100, invoice.JobDeclaration.LocalCurrencyCode);
				PrepareCharge(lCH);
				lCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);
				var invoiceLCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100);
				PrepareCharge(invoiceLCH);
				invoiceLCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("No apportioned Charge for invoice", 0, invoice.GroupCharges.Count);
				invoice.Charges.RemoveAndDelete(invoiceLCH);
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);
			}
		}

		public void TestDefaultPercentageForOverseasInsurance()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OH_Buyer = consignee.PK;

			var buyerLink = supplier.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = consignee.PK;
			buyerLink.OL_InsuranceUplift = 0.0m;

			using (CustomsDataRegistry.Instance.DefaultInsuranceRate.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 2.0m))
			{
				var oNSBranch = invoice.Charges.AddNew();
				oNSBranch.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				AssertEquals("J7_Percentage should be equal to current branch's DefaultInsuranceRate", 2.0m, oNSBranch.J7_Percentage);

				buyerLink.OL_InsuranceUplift = 3.0m;
				var oNSBuyerLink = invoice.Charges.AddNew();
				oNSBuyerLink.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
				AssertEquals("J7_Percentage should be equal to supplier's InsuranceUplift", 3.0m, oNSBuyerLink.J7_Percentage);

				testDec.SupplierAddressOrgPK = supplier.PK;
				testDec.ImporterDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
				Factory.Save();
				testDec.JE_ShipmentIncoTerm = "CIF";
				var invoice1 = testDec.Invoices.AddNew();
				AssertEquals("J7_Percentage should be equal to supplier's InsuranceUplift when suppler and buyer from job", 3.0m, invoice1.Charges[0].J7_Percentage);
			}
		}

		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var charge = Factory.New<InvoiceCharge>();
			Assert("AllowNonWesternEuropeanCharacterForChargeDescription should be true", charge.AllowNonWesternEuropeanCharacterForChargeDescription);
		}

		public override void TestSettingIsIncludedInLinesInInvoiceChargeChangesLineApportionedCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);
				PrepareCharge(oFT);
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
				invoice.JobDeclaration.ResumeApportionment();
				var line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", true, line.ApportionedCharges[0].J7_IsIncludedInITOT);
				oFT.J7_IsIncludedInITOT = false;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", false, line.ApportionedCharges[0].J7_IsIncludedInITOT);
				oFT.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", true, line.ApportionedCharges[0].J7_IsIncludedInITOT);
			}
		}

		public void TestShouldResetDefaultIsIncludedInITOT()
		{
			void AssertDefaultIsIncludeInITOT(BaseJobComInvoiceHeader invoiceHeader, ZString incoTerm, List<ZString> chargeCodesExpectedToBeTrue)
			{
				invoiceHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader.JZ_IncoTerm = incoTerm;
				var chargeCodesToTest = invoiceHeader.IncoTermAndChargeFactory.GetAllCharges().Where(x => !x.IsIncludedInITOTDeemedForThisCharge).Select(x => x.Code);
				foreach (var code in chargeCodesToTest)
				{
					var charge = invoiceHeader.Charges.AddNew(code, 1000);
					PrepareCharge(charge);
					var expected = chargeCodesExpectedToBeTrue.Contains(code);
					var messageType = invoiceHeader.IsImport ? "Import" : "Export";
					AssertEquals($"MessageType: {messageType}, IncoTerm: {incoTerm}, charge code {code} should have J7_IsIncludedInITOT defaulted to {expected}", expected, charge.J7_IsIncludedInITOT);
				}
			}

			var chargesIncludeInITOTDefaultToTrueForCIF = new List<ZString>()
			{ CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Codes.PackingCost, };
			var chargesIncludeInITOTDefaultToTrueForCFR = new List<ZString>()
			{ CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Codes.PackingCost, };
			var chargesIncludeInITOTDefaultToTrueForCI = new List<ZString>()
			{ CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Codes.PackingCost, };
			var chargesIncludeInITOTDefaultToTrueForFOBOrFAS = new List<ZString>()
			{ CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Codes.PackingCost, };
			var chargesIncludeInITOTDefaultToTrueForEXW = new List<ZString>()
			{ CustomsChargeTypeList.Codes.DeductionCharge };
			var emptyList = new List<ZString>();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			CombineAssertions(() =>
			{
				invoice.JobDeclaration.JE_MessageType = "EXP";
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostInsuranceAndFreight, chargesIncludeInITOTDefaultToTrueForCIF);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostAndFreight, chargesIncludeInITOTDefaultToTrueForCFR);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.FreeOnBoard, chargesIncludeInITOTDefaultToTrueForFOBOrFAS);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostAndInsurance, chargesIncludeInITOTDefaultToTrueForCI);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, chargesIncludeInITOTDefaultToTrueForFOBOrFAS);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.ExWorks, chargesIncludeInITOTDefaultToTrueForEXW);
				invoice.JobDeclaration.JE_MessageType = "IMP";
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostInsuranceAndFreight, chargesIncludeInITOTDefaultToTrueForCIF);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostAndFreight, chargesIncludeInITOTDefaultToTrueForCFR);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.FreeOnBoard, chargesIncludeInITOTDefaultToTrueForFOBOrFAS);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.CostAndInsurance, chargesIncludeInITOTDefaultToTrueForCI);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, chargesIncludeInITOTDefaultToTrueForFOBOrFAS);
				AssertDefaultIsIncludeInITOT(invoice, Core.Constants.IncoTerms.ExWorks, chargesIncludeInITOTDefaultToTrueForEXW);
			}

			);
		}

		public void TestShouldResetDefaultIsIncludedInAmount()
		{
			void AssertDefaultIsIncludedInAmount(BaseJobComInvoiceHeader invoiceHeader, ZString incoTerm, List<ZString> chargeCodesExpectedToBeFalse)
			{
				invoiceHeader.Charges.RemoveAndDeleteAll();
				invoiceHeader.JZ_IncoTerm = incoTerm;
				var configurations = invoiceHeader.IncoTermAndChargeFactory.GetIncotermChargeRelationshipConfigurations() as SortedDictionary<ZString, SortedDictionary<ZString, ChargeConfiguration>>;
				foreach (var configuration in configurations.Values)
				{
					var chargeCodesToTest = configuration.Where(x => !x.Value.IsIncludedInInvoiceAmountFixed);
					foreach (var chargeCode in chargeCodesToTest)
					{
						var code = chargeCode.Key;
						var charge = invoiceHeader.Charges.AddNew(code, 1000);
						PrepareCharge(charge);
						var expected = !chargeCodesExpectedToBeFalse.Contains(code);
						var messageType = invoiceHeader.IsImport ? "Import" : "Export";
						AssertEquals($"MessageType: {messageType}, IncoTerm: {incoTerm}, charge code {code} should have J7_Calc_IsIncludedInInvoiceAmount defaulted to {expected}", expected, charge.J7_Calc_IsIncludedInInvoiceAmount);
					}
				}
			}

			CombineAssertions(() =>
			{
				invoice.JobDeclaration.JE_MessageType = "EXP";
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostInsuranceAndFreight, new List<ZString> { "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostAndFreight, new List<ZString> { "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.FreeOnBoard, new List<ZString> { "OFT", "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostAndInsurance, new List<ZString> { "OFT", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, new List<ZString> { "OFT", "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.ExWorks, new List<ZString> { "OFT", "ONS", "ADD", "FIF", "PAC", "LCH", "EXW" });
				invoice.JobDeclaration.JE_MessageType = "IMP";
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostInsuranceAndFreight, new List<ZString> { "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostAndFreight, new List<ZString> { "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.FreeOnBoard, new List<ZString> { "OFT", "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.CostAndInsurance, new List<ZString> { "OFT", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, new List<ZString> { "OFT", "ONS", "LCH", "ADD" });
				AssertDefaultIsIncludedInAmount(invoice, Core.Constants.IncoTerms.ExWorks, new List<ZString> { "OFT", "ONS", "ADD", "FIF", "PAC", "LCH", "EXW" });
			});
		}

		[ExpectNoExceptions]
		public void TestCaptions()
		{
			var invoiceCharge = Factory.NewWithValidTestData<InvoiceCharge>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo, "Included in Inv. Amt", "It indicates whether the charge is included in the invoice amount. The Incoterm and charge code determine whether the charge is included by default.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceCharge.J7_IsStatisticalValueApplicableInfo, "Incl. in FOB", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceCharge.J7_IsGSTApplicableInfo, "Included in Total Disbursed Amount", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.");
		}
	}
}
