using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class BaseInvoiceChargeCollectionBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestTypeofElements()
		{
			var localDec = Factory.New<BaseJobDeclaration>();
			var localInvoice = localDec.Invoices.AddNew();
			var collection = new JobComInvChargeCollection<BaseInvoiceCharge>(localInvoice);
			AssertEquals("typeofelements", typeof(BaseInvoiceCharge), collection.TypeOfElements);
		}

		public void TestCollectionElementsAreCreatedWithTheRightType()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "ZUS";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "ZUS";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var localDec = Factory.New<BaseJobDeclaration>();
			localDec.JE_GB = GlbBranch.CurrentBranch.PK;
			var localInvoice = localDec.Invoices.AddNew();
			var localInvoiceCharge1 = localInvoice.Charges.AddNew();

			var usDec = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			var usInvoice = usDec.Invoices.AddNew();
			var usInvoiceCharge1 = usInvoice.Charges.AddNew();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var localDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(localDec.PK);
			var localInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(localInvoice.PK);
			var localInvoiceCharge1InDiffFactory = newFactory.Load<BaseInvoiceCharge>(localInvoiceCharge1.PK);

			var usDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(usDec.PK);
			var usInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(usInvoice.PK);
			var usInvoiceCharge1InDiffFactory = newFactory.Load<BaseInvoiceCharge>(usInvoiceCharge1.PK);

			AssertEquals("localInvoiceCharge1InDiffFactory Type", typeof(BaseInvoiceCharge), localInvoiceCharge1InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge1InDiffFactory.GetType(), usInvoiceCharge1InDiffFactory.GetType());

			var localInvoiceCharge2InDiffFactory = localInvoiceInDiffFactory.Charges.AddNew();
			var usInvoiceCharge2InDiffFactory = usInvoiceInDiffFactory.Charges.AddNew();
			AssertEquals("localInvoiceCharge2InDiffFactory Type", typeof(BaseInvoiceCharge), localInvoiceCharge2InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge2InDiffFactory.GetType(), usInvoiceCharge2InDiffFactory.GetType());
		}
	}

	public abstract class BaseInvoiceChargeCollectionTest<T> : Common.Testing.ChargeCollectionTest<JobComInvChargeCollection<T>, T> where T : BaseInvoiceCharge
	{
		public void TestCFRCalculationWithFreightAdjustedFlag()
		{
			Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Invoice.JZ_InvoiceAmount = 10500m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvHeaderCharge oFT = Invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;
			oFT.J7_IsDutiable = true;
			oFT.J7_IsIncludedInITOT = false;

			BaseJobComInvHeaderCharge adjustedOFT = Invoice.Charges.AddNew();
			adjustedOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			adjustedOFT.J7_Amount = 300m;
			adjustedOFT.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;
			adjustedOFT.J7_AdjustedCharge = true;
			adjustedOFT.J7_IsIncludedInITOT = false;

			AssertEquals("AmountToAddForITOT", 500m, Invoice.Charges.AmountToAddForITOT(Invoice.Invoice_Currency));
		}

		public void TestSetCurrency()
		{
			var collection = new JobComInvChargeCollection<T>(Invoice);
			BaseInvoiceCharge overseasFreight = collection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 10.0, Core.Constants.CurrencyCodes.UnitedStates);
			BaseInvoiceCharge otherCharges = collection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 10.0, "");
			BaseInvoiceCharge overseasInsurance = collection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, ZDecimal.Zero, Core.Constants.CurrencyCodes.NewZealand);
			collection.SetCurrency(Core.Constants.CurrencyCodes.Australia);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, overseasFreight.J7_RX_NKCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, otherCharges.J7_RX_NKCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.NewZealand, overseasInsurance.J7_RX_NKCurrency);
		}

		public void TestITOTCalculationWithIncludedInInvoiceFlag()
		{
			Invoice.JZ_IncoTerm = "FOB";
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			BaseInvoiceCharge oFT = Invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;

			AssertEquals("PreCondition:OFT is not included in invoice", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("Therefore not affecting line total", 10000m, Invoice.InvoiceLineTotal);

			BaseInvoiceCharge cOM = Invoice.Charges.AddNew();
			cOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			cOM.J7_Amount = 200m;
			cOM.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;
			cOM.J7_Calc_IsIncludedInInvoiceAmount = true;
			cOM.J7_IsIncludedInITOT = false;
			AssertEquals("Is included in invoice, but not included in lines-> decrease InvoiceLineTotal by 200m", 9800m, Invoice.InvoiceLineTotal);

			cOM.J7_Calc_IsIncludedInInvoiceAmount = false;
			AssertEquals("Is not included in invoice, thereofre not affecting line total calculation", 10000m, Invoice.InvoiceLineTotal);
		}

		public void TestSetIncludedInInvoiceFlagsForNonIncotermNeutralCharges()
		{
			Invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			BaseInvoiceCharge oFT = Invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 10m;
			oFT.J7_RX_NKCurrency = "AUD";
			AssertEquals("OFT for FOB invoice should not be included in invoice amount", false, oFT.J7_Calc_IsIncludedInInvoiceAmount);

			Invoice.JZ_IncoTerm = Invoice.IncotermEquivalentToCFRForTesting;
			AssertEquals("SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges() is trigerred", true, oFT.J7_Calc_IsIncludedInInvoiceAmount);
		}

		public void TestApportionmentDirtyWhenAnInvoiceChargeGetsRemoved()
		{
			Invoice.JZ_IncoTerm = "FOB";
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			BaseInvoiceCharge invCharge = Invoice.Charges.AddNew();
			invCharge.J7_ChargeType = "OFT";
			invCharge.J7_Amount = 100m;
			invCharge.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;

			Invoice.JobDeclaration.ApportionmentDirty = false;
			Invoice.Charges.RemoveAndDelete(invCharge);
			AssertEquals("Apportionment is dirty", true, Invoice.JobDeclaration.ApportionmentDirty);
		}

		public void TestClearApportionedChargesOfLinesWhenAnEmptyChargeTypeIsRemoved()
		{
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;
			Invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = Invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			BaseJobComInvHeaderCharge invCharge = Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, Invoice.JobDeclaration.LocalCurrencyCode);
			Declaration.ResumeApportionment();
			AssertEquals("Line has an apportioned charge", 1, line.ApportionedCharges.Count);

			invCharge.J7_ChargeType = ZString.Empty;

			Invoice.Charges.RemoveAndDelete(invCharge);
			Declaration.ResumeApportionment();
			AssertEquals("Line has no apportioned charge now", 0, line.ApportionedCharges.Count);
		}

		public void TestClearApportionedChargesOfLinesWhenAChargeIsRemoved()
		{
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;
			Invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = Invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			BaseJobComInvHeaderCharge invCharge = Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, Invoice.JobDeclaration.LocalCurrencyCode);
			Declaration.ResumeApportionment();
			AssertEquals("Line has an apportioned charge", 1, line.ApportionedCharges.Count);

			Invoice.Charges.RemoveAndDelete(invCharge);
			Declaration.ResumeApportionment();
			AssertEquals("Line has no apportioned charge now", 0, line.ApportionedCharges.Count);
		}

		public void TestReapportionchargeWhenAInvoiceChargeIsRemoved()
		{
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;
			Invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = Invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			BaseJobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "FOB";
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			BaseJobComInvHeaderCharge groupCharge = Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			groupCharge.J7_Amount = 1000m;
			groupCharge.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;
			groupCharge.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge invCharge = Invoice.Charges.AddNew();
			invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			invCharge.J7_Amount = 600m;
			invCharge.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;
			invCharge.J7_IsIncludedInITOT = true;
			Declaration.ResumeApportionment();
			AssertEquals("PreCondition:Line is apportioned from InvCharge", 600m, line.ApportionedCharges.GetCharge(invCharge.ChargeKey).Amount);
			AssertEquals("PreCondition:Line2 is apportioned from GroupCharge", 400m, line2.ApportionedCharges.GetCharge(invCharge.ChargeKey).Amount);

			Invoice.Charges.RemoveFromRelationship(invCharge);
			Declaration.ResumeApportionment();
			AssertEquals("Line is now apportioned from Group Charge", 500m, line.ApportionedCharges.GetCharge(invCharge.ChargeKey).Amount);
			AssertEquals("Line2 is apportioned from GroupCharge", 500m, line2.ApportionedCharges.GetCharge(invCharge.ChargeKey).Amount);
		}

		public void TestRemovingAnInvoiceChargeReapportionFromGroup()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_IncoTerm = "FOB";
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 9000m;

			BaseJobComInvHeaderCharge invCOM = Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 1000m, Declaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge groupCOM = groupHeader.Charges.AddNew();
			groupCOM.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			groupCOM.J7_Percentage = 10m;
			Declaration.ResumeApportionment();
			AssertEquals("Invoice COM is apportioned into Line", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);

			Invoice.Charges.RemoveAndDelete(invCOM);
			Declaration.ResumeApportionment();
			AssertEquals("Group COM is apportioned into line", 900m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("Invoice has the aggregated amount", 900m, Invoice.GroupCharges[0].J7_Amount);
		}

		public void TestIncludedInLinesDiscountDontAffectBalance()
		{
			Invoice.JZ_IncoTerm = "FOB";
			Invoice.JZ_InvoiceAmount = 10000m;
			Invoice.JZ_RX_NKInvoice_Currency = Invoice.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvHeaderCharge discount = Invoice.Charges.AddNew();
			discount.J7_ChargeType = "DIS";
			discount.J7_Amount = 200m;
			discount.J7_RX_NKCurrency = Invoice.JobDeclaration.LocalCurrencyCode;

			AssertEquals("Balance with excluded-from-lines discount", 10200m, Invoice.InvoiceLineTotal);

			discount.J7_IsIncludedInITOT = true;
			AssertEquals("Balance with included-in-lines discount", 10000m, Invoice.InvoiceLineTotal);
		}

		public void TestInvoiceChargeCollectionDoesNotLoadApportionedCharges()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 10m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			testDec.ResumeApportionment();
			AssertEquals("There is one charge in charge collection", 1, invoice.Charges.Count);
			AssertEquals("There is one apportioned charge in charge collection", 1, invoice.GroupCharges.Count);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobComInvoiceHeader invoiceLoaded = factory2.Load<BaseJobComInvoiceHeader>(invoice.PK);
			testDec.ResumeApportionment();
			AssertEquals("Charges collection", 1, invoiceLoaded.Charges.Count);
			AssertEquals("Charges has Commission", CustomsChargeTypeList.Codes.Commission, invoiceLoaded.Charges[0].J7_ChargeType);
			AssertEquals("Apportioned charge collection", 1, invoiceLoaded.GroupCharges.Count);
			AssertEquals("Apportioned Charge has OFT", CustomsChargeTypeList.Codes.OverseasFreight, invoiceLoaded.GroupCharges[0].J7_ChargeType);
		}

		#region Implementation

		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = BaseJobDeclaration.New(Factory);
				}

				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Declaration.Invoices.AddNew();
				}

				return fInvoice;
			}
		}
		BaseJobComInvoiceHeader fInvoice;

		protected IJobComInvChargeCollection<BaseInvoiceCharge> TestCollection => Invoice.Charges;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseInvoiceCharge result = Factory.New<BaseInvoiceCharge>();
			result.Parent = Invoice;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			distributeByForExport = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		#endregion
	}

	[TestedType(typeof(BaseInvoiceCharge))]
	public class BaseInvoiceChargeCollectionTest : BaseInvoiceChargeCollectionTest<BaseInvoiceCharge>
	{
		protected override JobComInvChargeCollection<BaseInvoiceCharge> GetCollectionToTest()
		{
			return (JobComInvChargeCollection<BaseInvoiceCharge>)TestCollection;
		}
	}
}
