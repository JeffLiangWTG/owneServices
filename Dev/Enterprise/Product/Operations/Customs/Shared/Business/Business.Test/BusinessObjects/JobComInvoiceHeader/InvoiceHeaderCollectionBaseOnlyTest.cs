using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionBaseOnlyTest : BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, BaseJobComInvoiceHeader>
	{
		public void TestUpdateExchangeRateOnExportDateChanged()
		{
			var today = ZDateTime.Today;
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(today.AddDays(-10), today.AddDays(-8), 0.70m, uSDCurrency, Factory);
			SetExchangeRate(today.AddDays(-7), today.AddDays(-5), 0.69m, uSDCurrency, Factory);

			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = today.AddDays(-9);

			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;

			AssertEquals("PreCondition:Exchange rate", 0.70m, invoice.JZ_InvoiceCurrLandedCostExRate);
			testDec.JE_ExportDate = today.AddDays(-6);
			AssertEquals("Exchange rate updated", 0.69m, invoice.JZ_InvoiceCurrLandedCostExRate);
		}

		public void TestFindBySequenceNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			AssertEquals("PreCondition", (short)1, invoice.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)2, invoice2.JZ_InvoiceDisplaySequence);

			AssertEquals(invoice, declaration.Invoices.Find(1));
			AssertEquals(invoice2, declaration.Invoices.Find(2));
		}

		public void TestApportionChargeKeyWhenChargeDescriptionIsEmpty()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrencyCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 224.86m;
			var lineCharge = invoiceLine.Charges.AddNew();
			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			lineCharge.J7_Percentage = 35m;
			lineCharge.J7_ChargeDescription = ZString.Empty;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 585.14m;
			var lineCharge2 = invoiceLine2.Charges.AddNew();
			lineCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			lineCharge2.J7_Percentage = 35m;
			lineCharge2.J7_ChargeDescription = CustomsChargeTypeList.Descriptions.Discount;

			declaration.ResumeApportionment();

			Assert(declaration.Invoices.AreChargesBalancedForInvoices(out var _));
		}

		public void TestApportionmentBalanceWhenPercentageIsCalculated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = invoice.LocalCurrencyCode;

			var chargeWithPercentage = declaration.TopGroupInvoice.Charges.AddNew();
			chargeWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			chargeWithPercentage.J7_Percentage = 0.25m;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 224.86m;//0.56215

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 585.14m;//1.46285

			declaration.ResumeApportionment();

			Assert(declaration.Invoices.AreChargesBalancedForInvoices(out var _));

			Factory.Save();

			var declarationLoaded = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);
			Assert(declarationLoaded.Invoices.AreChargesBalancedForInvoices(out var _));
		}

		public void TestAddingDetachInvoiceAffectWeightApportion()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_AutoWeightApportion = true;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_InvoiceAmount = 1000m;
			AssertEquals(1000m, invoice1.JZ_Weight);
			var invoice2 = (BaseJobComInvoiceHeader)((IBindingList)declaration.Invoices).AddNew(); // add uncommitted invoice
			invoice2.JZ_InvoiceAmount = 1000m;
			AssertEquals(500m, invoice1.JZ_Weight);
			AssertEquals(500m, invoice2.JZ_Weight);
			((ICancelAddNew)declaration.Invoices).CancelNew(1);
			AssertEquals(1000m, invoice1.JZ_Weight);
			AssertEquals(true, invoice2.IsDeleted);
		}

		public void TestCommittingDetachInvoiceRefreshInvoiceStructure()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			int eventCalled = 0;
			InvoiceStructureChangeEvent.AddInvoiceStructureChangedEventHandler(Factory, new EventHandler(delegate
			{ eventCalled++; }));
			((IBindingList)declaration.Invoices).AddNew(); // add uncommitted invoice
			eventCalled = 0;
			((ICancelAddNew)declaration.Invoices).EndNew(0); // commit invoice
			AssertEquals("1 AddNew + 1 Setting JZ_JZ_GroupInvoiceFK + 1 EndNew", 3, eventCalled);
		}

		public void TestEffectiveValuationDateForParentGroup()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);

			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			AssertEquals("Effective Valuation Date for group", new ZDateTime(2005, 1, 1), groupHeader.AllJobComInvoiceHeaders.EffectiveValuationDateForParentGroup);

			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Effective Valuation Date for group", new ZDateTime(2005, 1, 1), groupHeader.AllJobComInvoiceHeaders.EffectiveValuationDateForParentGroup);

			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Effective Valuation Date for group", new ZDateTime(2005, 1, 2), groupHeader.AllJobComInvoiceHeaders.EffectiveValuationDateForParentGroup);

			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 3);
			AssertEquals("Effective Valuation Date for group", new ZDateTime(2005, 1, 1), groupHeader.AllJobComInvoiceHeaders.EffectiveValuationDateForParentGroup);
		}

		public void TestBalanceCheckWithGroupCharge()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "123";
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var line = declaration.InvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			invoice.GroupCharges.RemoveAndDeleteAll();
			line.ApportionedCharges.RemoveAndDeleteAll();

			Assert(!declaration.Invoices.AreChargesBalancedForInvoices(out var message));
			AssertEquals("(OFT at Group Invoice All Invoices) Group Inv. Amount:100.00 ERN, Total Inv. Amount:0.00 ERN", message);

			declaration.ResumeApportionment();
			Assert(declaration.Invoices.AreChargesBalancedForInvoices(out var _));
		}

		public void TestApportionmentErrorWhenTwoOFTsExist()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "123";
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var line = declaration.InvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20m, declaration.LocalCurrencyCode);
			declaration.ResumeApportionment();
			Assert(declaration.Invoices.AreChargesBalancedForInvoices(out var _));
		}

		public void TestBalanceCheckWithGroupCharge2()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 101m, declaration.LocalCurrencyCode);
			var line = declaration.InvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			declaration.ResumeApportionment();
			Assert("group OFT will be ignored", !declaration.Invoices.AreChargesBalancedForInvoices(out var message));
			AssertEquals("(OFT at Group Invoice All Invoices) Group Inv. Amount:100.00 ERN, Total Inv. Amount:101.00 ERN", message);
		}

		public void TestBalanceCheckWithGroupCharge3()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line = declaration.InvoiceLines.AddNew();
			line.JI_LinePrice = 1000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 40000m;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line2 = invoice2.InvoiceLines.AddNew();
			line2.JI_LinePrice = 4000m;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 70m, declaration.LocalCurrencyCode);
			declaration.TopGroupInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
			declaration.ResumeApportionment();

			Assert("invoice level OFT override will be compared against group oft along with apportioned OFT to the first invoice", declaration.Invoices.AreChargesBalancedForInvoices(out var _));
		}

		public void TestValuationDate()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 8, 16);
			AssertEquals(new ZDateTime(2005, 8, 16), testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.ValuationDate);
			var invoice1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertEquals(new ZDateTime(2005, 8, 17), testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.ValuationDate);
		}

		public void TestDoAllInvoicesHaveTheSameValuationDate()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var invoice1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoice2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals(true, testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate);
			testDec.JE_ExportDate = new ZDateTime(2005, 8, 16);
			AssertEquals(true, testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate);
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 16);
			AssertEquals(true, testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate);
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 17);
			AssertEquals(false, testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate);
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 16);
			AssertEquals(true, testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.DoAllInvoicesHaveTheSameValuationDate);
		}

		public void TestOnAddedForNullInvoice()
		{
			// In .net 2.0 RelatedCurrencyManager.ParentManager_CurrentItemChanged annoyingly calls AddNew().CancelEdit()
			// which causes re-entrency problems, so IBindingList.AddNew will return null to overcome this performance issue.
			// The exception is caught but it causes issues downstream with HasChanges on Brokerage Plugin on Shipments
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeaderActiveCollection = new InvoiceHeaderActiveCollectionForTest(declaration);
			AssertNoExceptionThrown(() => invoiceHeaderActiveCollection.OnAddedExposed(null));
		}

		protected override InvoiceHeaderActiveCollection GetCollectionToTest() => GetNewJobDeclaration().Invoices;

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

		sealed class InvoiceHeaderActiveCollectionForTest : InvoiceHeaderActiveCollection
		{
			public InvoiceHeaderActiveCollectionForTest(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public void OnAddedExposed(BaseJobComInvoiceHeader businessObject) => OnAdded(businessObject);
		}
	}
}
