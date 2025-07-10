using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>))]
	class BaseInvoiceLineApportionedChargeCollectionBusinessLevelTest : JobComInvApportionedChargeCollectionTest<IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>, BaseInvoiceLineApportionedCharge>
	{
		public void TestTypeOfElements()
		{
			var localDec = Factory.New<BaseJobDeclaration>();
			var localInvoice = localDec.Invoices.AddNew();
			var localInvoiceLine = localInvoice.JobComInvoiceLines.AddNew();
			var collection = new JobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>(localInvoiceLine);
			AssertEquals("TypeOfElements", typeof(BaseInvoiceLineApportionedCharge), collection.TypeOfElements);
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
			var localInvoiceLine = localInvoice.JobComInvoiceLines.AddNew();
			var localInvoiceLineCharge1 = localInvoiceLine.ApportionedCharges.AddNew();

			var usDec = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			var usInvoice = usDec.Invoices.AddNew();
			var usInvoiceLine = usInvoice.JobComInvoiceLines.AddNew();
			var usInvoiceLineCharge1 = usInvoiceLine.ApportionedCharges.AddNew();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var localDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(localDec.PK);
			var localInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(localInvoice.PK);
			var localInvoiceLineInDiffFactory = newFactory.Load<BaseJobComInvoiceLine>(localInvoiceLine.PK);
			var localInvoiceLineCharge1InDiffFactory = newFactory.Load<BaseInvoiceLineApportionedCharge>(localInvoiceLineCharge1.PK);

			var usDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(usDec.PK);
			var usInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(usInvoice.PK);
			var usInvoiceLineInDiffFactory = newFactory.Load<BaseJobComInvoiceLine>(usInvoiceLine.PK);
			var usInvoiceLineCharge1InDiffFactory = newFactory.Load<BaseInvoiceLineApportionedCharge>(usInvoiceLineCharge1.PK);

			AssertEquals("localInvoiceLineCharge1InDiffFactory Type", typeof(BaseInvoiceLineApportionedCharge), localInvoiceLineCharge1InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceLineInDiffFactory.GetType(), usInvoiceLineCharge1InDiffFactory.GetType());

			var localInvoiceLineCharge2InDiffFactory = localInvoiceLineInDiffFactory.ApportionedCharges.AddNew();
			var usInvoiceLineCharge2InDiffFactory = usInvoiceLineInDiffFactory.ApportionedCharges.AddNew();
			AssertEquals("localInvoiceLineCharge2InDiffFactory Type", typeof(BaseInvoiceLineApportionedCharge), localInvoiceLineCharge2InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceLineCharge2InDiffFactory.GetType(), usInvoiceLineCharge2InDiffFactory.GetType());
		}

		public void TestHasValidCharges()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Has no valid charges", false, invoiceLine.ApportionedCharges.HasAnElementWithValidCharges());

			BaseInvoiceLineApportionedCharge appLineCharge = invoiceLine.ApportionedCharges.AddNew();
			AssertEquals("Has no valid charges", false, invoiceLine.ApportionedCharges.HasAnElementWithValidCharges());

			appLineCharge.J7_Amount = 100m;
			AssertEquals("Has no valid charges", false, invoiceLine.ApportionedCharges.HasAnElementWithValidCharges());

			appLineCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertEquals("Has valid charges", true, invoiceLine.ApportionedCharges.HasAnElementWithValidCharges());
		}

		public void TestDefaultValues()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			BaseInvoiceLineApportionedCharge apportioned = invoiceLine.ApportionedCharges.AddNew();

			AssertEquals("TableName", "JI", apportioned.J7_ParentTableCode);
			AssertEquals("IsApportioned", true, apportioned.J7_IsApportionedCharge);
			AssertEquals("ForeignKey", invoiceLine.PK, apportioned.J7_ParentID);
		}

		public void TestLoadOnlyApportionedCharges()
		{
			BaseInvoiceLineApportionedCharge apportioned = Factory.New<BaseInvoiceLineApportionedCharge>();
			apportioned.J7_IsApportionedCharge = true;
			apportioned.Parent = InvoiceLine;

			BaseInvoiceLineCharge nonApportioned = Factory.New<BaseInvoiceLineCharge>();
			nonApportioned.J7_IsApportionedCharge = false;
			nonApportioned.Parent = InvoiceLine;
			TestCollection.Rebuild();

			AssertEquals("Test Collection has one item", 1, TestCollection.Count);
			AssertEquals("Test Collection loads apportionedCharges", true, TestCollection[0].J7_IsApportionedCharge);
		}

		protected override JobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> GetCollectionToTest()
		{
			return new JobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return InvoiceLine.ApportionedCharges.AddNew();
		}

		#region Implementation

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = BaseJobDeclaration.New(Factory);
				}

				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = TestDec.Invoices.AddNew();
				}

				return fInvoice;
			}
		}
		BaseJobComInvoiceHeader fInvoice;

		BaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				}

				return fInvoiceLine;
			}
		}
		BaseJobComInvoiceLine fInvoiceLine;

		protected IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> TestCollection
		{
			get { return InvoiceLine.ApportionedCharges; }
		}

		#endregion
	}

	class BaseOnlyForReApportionForNonPersistedChargesTest : TestCaseWithFactory
	{
		public void TestIsIncludedInITOTForLinesSetDuringApportioning()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			BaseInvoiceLineCharge invLineCharge = invoiceLine.Charges.AddNew();
			invLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			invLineCharge.J7_Percentage = 10m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice Apportioned Charge from line", 1000m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("InvLineCharge IsIncludedITOT", invoice.GroupCharges[0].J7_IsIncludedInITOT, invLineCharge.J7_IsIncludedInITOT);
		}
	}
}
