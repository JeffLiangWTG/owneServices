using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseInvoiceCharge))]
	public class BaseInvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDeciderForInvoiceCharge()
		{
			AssertEquals("Type decider type", typeof(BaseInvoiceChargeTypeDecider), BaseInvoiceCharge.TypeDecider.GetType());
		}

		public void TestSetDefaultValues()
		{
			BaseInvoiceCharge charge = Factory.New<BaseInvoiceCharge>();
			AssertEquals("IsApportioned is set", false, charge.J7_IsApportionedCharge);

			charge.Parent = invoice;
			AssertEquals("ParentTableCode is set", "JZ", charge.J7_ParentTableCode);
		}

		public virtual void TestJ7_Calc_IsIncludedInInvoice()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();

			BaseInvoiceCharge aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			aDD.J7_Amount = 10m;
			aDD.J7_RX_NKCurrency = "AUD";
			AssertEquals("J7_Calc_IsIncludedInInvoice is not readonly", false, aDD.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			aDD.J7_Calc_IsIncludedInInvoiceAmount = false;
			AssertEquals("Setting this to false should set J7_IsNotIncludedInInvoice as true", true, aDD.J7_IsNotIncludedInInvoice);

			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
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

		protected virtual ZString GetIncotermTermFreightCanBeIncluded()
		{
			return Core.Constants.IncoTerms.CostInsuranceAndFreight;
		}

		protected virtual string GetIncotermToTestIsIncludedInInvoice() => "FOB";

		public virtual void TestReadOnlyOfIsIncludedInInvoice()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();

			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 10m;
			AssertEquals("IsIncludedInInvoiceAmount", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice should be readonly", true, oFT.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			BaseInvoiceCharge cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Amount = 10m;
			AssertEquals("IsIncludedInInvoiceAmount for COM by default", true, cOM.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("IsIncludedInInvoice should not be readonly", false, cOM.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobComInvoiceHeader invoiceLoaded = factory2.Load<BaseJobComInvoiceHeader>(invoice.PK);
			BaseInvoiceCharge oFTLoaded = invoiceLoaded.Charges.GetChargeByChargeName(CustomsChargeTypeList.Codes.OverseasFreight);
			BaseInvoiceCharge cOMLoaded = invoiceLoaded.Charges.GetChargeByChargeName(CustomsChargeTypeList.Codes.Commission);

			AssertEquals("IsIncludedInInvoice for OFT should be readonly", true, oFTLoaded.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
			AssertEquals("IsIncludedInInvoice for COM should not be readonly", false, cOMLoaded.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestSettingIncludedInITOTTrueToIncludedInInvoiceTrue()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseInvoiceCharge oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			oTH.J7_Amount = 10m;

			oTH.J7_IsIncludedInITOT = false;
			oTH.J7_Calc_IsIncludedInInvoiceAmount = false;

			AssertEquals("OTH is not included in ITOT", false, oTH.J7_IsIncludedInITOT);
			AssertEquals("OTH is not included in invoice", false, oTH.J7_Calc_IsIncludedInInvoiceAmount);

			oTH.J7_IsIncludedInITOT = true;
			AssertEquals("Included in Line Total -> Included in Invoice", true, oTH.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestSettingincludedInInvoiceFalseSetIncludedInITOTToFalse()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseInvoiceCharge oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			oTH.J7_Amount = 10m;

			oTH.J7_IsIncludedInITOT = true;
			oTH.J7_Calc_IsIncludedInInvoiceAmount = true;

			AssertEquals("IsIncludedInITOT", true, oTH.J7_IsIncludedInITOT);
			AssertEquals("IsIncludedInInvoice", true, oTH.J7_Calc_IsIncludedInInvoiceAmount);

			oTH.J7_Calc_IsIncludedInInvoiceAmount = false;
			AssertEquals("Not included in invoice -> Cannot be part of Line Total", false, oTH.J7_IsIncludedInITOT);
		}

		public virtual void TestJ7_IsNotIncludedInInvoiceSetOnFactorySaving()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncoTermForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving();

			BaseInvoiceCharge cOM = invoice.Charges.AddNew();
			cOM.J7_ChargeType = GetChargeTypeForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving();
			cOM.J7_Amount = 0m;
			AssertEquals("J7_IsNotIncludedInInvoice", false, cOM.J7_IsNotIncludedInInvoice);

			cOM.J7_Calc_IsIncludedInInvoiceAmount = false;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseInvoiceCharge cOMLoaded = factory2.Load<BaseInvoiceCharge>(cOM.PK);
			AssertEquals("J7_IsNotIncludedInInvoice was set to true when saving", true, cOMLoaded.J7_IsNotIncludedInInvoice);
		}

		protected virtual string GetChargeTypeForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving() => CustomsChargeTypeList.Codes.Commission;

		protected virtual string GetIncoTermForTestJ7_IsNotIncludedInInvoiceSetOnFactorySaving() => "FOB";

		public virtual void TestIsIncludedInLinesReadOnly()
		{
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			BaseInvoiceCharge aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;

			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			BaseInvoiceCharge oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;

			AssertEquals("ADD Included in ITOT should be readonly", false, aDD.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OFT Included in ITOT should be readonly", true, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OTH  Included in ITOT should not be readonly", false, oTH.J7_IsIncludedInITOTInfo.ReadOnly);
		}

		public virtual void TestResetDefaultIsIncludedInAmount()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = GetIncotermToTestIsIncludedInInvoice();
			var overseasFreightCharge = invoice.Charges.AddNew();

			overseasFreightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			overseasFreightCharge.J7_Amount = 100m;
			overseasFreightCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			AssertEquals("Default IsIncludedInAmount for OFT", false, overseasFreightCharge.J7_Calc_IsIncludedInInvoiceAmount);

			invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
			AssertEquals("Default IsIncludedInAmount for OFT", true, overseasFreightCharge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestApportionmentDirtyWhenAmountChanges()
		{
			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testCharge.J7_Amount = 100m;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenTypeChanges()
		{
			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testCharge.J7_ChargeType = "OTH";
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenCurrencyChanges()
		{
			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testCharge.J7_RX_NKCurrency = "AUD";
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenIsDutiableChanges()
		{
			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testCharge.J7_IsDutiable = !testCharge.J7_IsDutiable;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestApportionmentDirtyWhenIsGSTApplicableChanges()
		{
			testDec.ApportionmentDirty = false;
			AssertEquals("JobDeclaration Apportionment is not dirty", false, testDec.ApportionmentDirty);

			testCharge.J7_IsGSTApplicable = !testCharge.J7_IsGSTApplicable;
			AssertEquals("JobDeclaration Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestReadOnlyOfAmountCurrencyIfPercentage()
		{
			BaseInvoiceCharge invCharge = invoice.Charges.AddNew();
			invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;

			invCharge.J7_Percentage = 10m;

			AssertEquals("Amount is calculated and should be readonly", true, invCharge.J7_AmountInfo.ReadOnly);
			AssertEquals("Currency is calculated and should be readonly", true, invCharge.J7_RX_NKCurrencyInfo.ReadOnly);

			invCharge.J7_Percentage = 0m;
			AssertEquals("% is zero and should not be readonly", false, invCharge.J7_AmountInfo.ReadOnly);
			AssertEquals("% is zero and should not be readonly", false, invCharge.J7_RX_NKCurrencyInfo.ReadOnly);
		}

		public virtual void TestReapportionAllChargesWhenChargeCodeChargeKeyChanged()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_IncoTerm = Constants.IncoTerms.DeliveredAtPlace;
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;

				BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
				BaseJobComInvHeaderCharge oNS = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100);

				BaseJobComInvoiceGroupHeader groupHeader = invoice.Master;
				BaseJobComInvHeaderCharge lCH = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100, invoice.JobDeclaration.LocalCurrencyCode);
				PrepareCharge(lCH);
				lCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);

				BaseJobComInvHeaderCharge invoiceLCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100);
				PrepareCharge(invoiceLCH);
				invoiceLCH.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("No apportioned Charge for invoice", 0, invoice.GroupCharges.Count);

				invoice.Charges.RemoveAndDelete(invoiceLCH);
				testDec.ResumeApportionment();
				AssertEquals("One apportioned Charge for Invoice", 1, invoice.GroupCharges.Count);
			}
		}

		public virtual void TestSettingIsIncludedInLinesInInvoiceChargeChangesLineApportionedCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
				BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);
				PrepareCharge(oFT);
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;
				invoice.JobDeclaration.ResumeApportionment();

				BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", false, line.ApportionedCharges[0].J7_IsIncludedInITOT);

				oFT.J7_IsIncludedInITOT = true;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned charge for line", true, line.ApportionedCharges[0].J7_IsIncludedInITOT);
			}
		}

		protected virtual void PrepareCharge(Common.JobComInvCharge charge)
		{
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			SetupAllTestObjects();
			return testCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupAllTestObjects();
		}

		protected BaseJobDeclaration testDec;
		protected BaseJobComInvoiceHeader invoice;
		protected BaseJobComInvHeaderCharge testCharge;

		protected virtual void SetupAllTestObjects()
		{
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testCharge = invoice.Charges.AddNew();
		}

		#endregion
	}
}
