using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class BaseInvoiceLineChargeCollectionBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestRemovingInvoiceLineChargeReapportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, testDec.LocalCurrencyCode);

				BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 3000m;
				BaseJobComInvHeaderCharge lineCharge = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 500m, testDec.LocalCurrencyCode);
				lineCharge.J7_IsNotIncludedInInvoice = true;
				BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 7000m;
				testDec.ResumeApportionment();
				AssertEquals("Line1 not apportioned", 0, line1.ApportionedCharges.Count);
				AssertEquals("Line2 apportioned", 1, line2.ApportionedCharges.Count);
				AssertEquals("Line2 apportioned amount", 500m, line2.ApportionedCharges[0].J7_Amount);

				line1.Charges.RemoveAndDelete(lineCharge);
				testDec.ResumeApportionment();
				AssertEquals("Line1 is apportioned", 300m, line1.ApportionedCharges[0].J7_Amount);
				AssertEquals("Line2 apportioned", 700m, line2.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestTypeOfElements()
		{
			var localDec = Factory.New<BaseJobDeclaration>();
			var localInvoice = localDec.Invoices.AddNew();
			var localInvoiceLine = localInvoice.JobComInvoiceLines.AddNew();
			var collection = new JobComInvChargeCollection<BaseInvoiceLineCharge>(localInvoiceLine);
			AssertEquals("typeofelements", typeof(BaseInvoiceLineCharge), collection.TypeOfElements);
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
			var localInvoiceLineCharge1 = localInvoiceLine.Charges.AddNew();

			var usDec = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			var usInvoice = usDec.Invoices.AddNew();
			var usInvoiceLine = usInvoice.JobComInvoiceLines.AddNew();
			var usInvoiceLineCharge1 = usInvoiceLine.Charges.AddNew();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var localDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(localDec.PK);
			var localInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(localInvoice.PK);
			var localInvoiceLineInDiffFactory = newFactory.Load<BaseJobComInvoiceLine>(localInvoiceLine.PK);
			var localInvoiceLineCharge1InDiffFactory = newFactory.Load<BaseInvoiceLineCharge>(localInvoiceLineCharge1.PK);

			var usDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(usDec.PK);
			var usInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(usInvoice.PK);
			var usInvoiceLineInDiffFactory = newFactory.Load<BaseJobComInvoiceLine>(usInvoiceLine.PK);
			var usInvoiceLineCharge1InDiffFactory = newFactory.Load<BaseInvoiceLineCharge>(usInvoiceLineCharge1.PK);

			AssertEquals("localInvoiceLineCharge1InDiffFactory Type", typeof(BaseInvoiceLineCharge), localInvoiceLineCharge1InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceLineInDiffFactory.GetType(), usInvoiceLineCharge1InDiffFactory.GetType());

			var localInvoiceLineCharge2InDiffFactory = localInvoiceLineInDiffFactory.Charges.AddNew();
			var usInvoiceLineCharge2InDiffFactory = usInvoiceLineInDiffFactory.Charges.AddNew();
			AssertEquals("localInvoiceLineCharge2InDiffFactory Type", typeof(BaseInvoiceLineCharge), localInvoiceLineCharge2InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceLineCharge2InDiffFactory.GetType(), usInvoiceLineCharge2InDiffFactory.GetType());
		}
	}

	public abstract class BaseInvoiceLineChargeCollectionTest<T> : Common.Testing.ChargeCollectionTest<JobComInvChargeCollection<T>, T> where T : BaseInvoiceLineCharge
	{
		public virtual void TestHasValidCharges()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			BaseInvoiceLineCharge appLineCharge = invoiceLine.Charges.AddNew();
			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			appLineCharge.J7_Amount = 100m;
			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			appLineCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			AssertEquals("Has valid charges", true, invoiceLine.Charges.HasAnElementWithValidCharges());
		}

		public void TestMarkApportionmentDirtyWhenInvoiceLineChargeGetsRemoved()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			BaseInvoiceLineCharge nonApportioned = invoiceLine.Charges.AddNew();
			nonApportioned.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			nonApportioned.J7_Amount = 100m;
			nonApportioned.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			testDec.ApportionmentDirty = false;
			invoiceLine.Charges.RemoveAndDelete(nonApportioned);
			AssertEquals("Removing an invoice line charge will make Apportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestDefaultValues()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			BaseInvoiceLineCharge nonApportioned = invoiceLine.Charges.AddNew();

			AssertEquals("TableName", "JI", nonApportioned.J7_ParentTableCode);
			AssertEquals("IsApportioned", false, nonApportioned.J7_IsApportionedCharge);
			AssertEquals("ForeignKey", invoiceLine.PK, nonApportioned.J7_ParentID);
		}

		public void TestRebuildOnConstruction()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = TestDec;
				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.Charges.AddNew("OFT", 100m, testDec.LocalCurrencyCode);
				BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 100m;
				BaseInvoiceLineCharge nonApportioned = invoiceLine.Charges.AddNew();
				Factory.Save();

				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				BaseJobComInvoiceLine lineLoaded = anotherFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
				AssertEquals("1 charge row", 1, lineLoaded.Charges.Count);
				AssertEquals("NonApportioned", false, lineLoaded.Charges[0].J7_IsApportionedCharge);
				AssertEquals("1 apportioned row", 1, lineLoaded.ApportionedCharges.Count);
				AssertEquals("Apportioned", true, lineLoaded.ApportionedCharges[0].J7_IsApportionedCharge);
			}
		}

		public void TestLoadOnlyNonApportionedCharges()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			BaseInvoiceLineCharge nonApportioned = Factory.New<BaseInvoiceLineCharge>();
			nonApportioned.Parent = invoiceLine;
			nonApportioned.J7_IsApportionedCharge = false;

			BaseInvoiceLineApportionedCharge apportioned = Factory.New<BaseInvoiceLineApportionedCharge>();
			apportioned.Parent = invoiceLine;
			apportioned.J7_IsApportionedCharge = true;

			AssertEquals("One item", 1, invoiceLine.ApportionedCharges.Count);

			AssertEquals("One item", 1, invoiceLine.Charges.Count);
		}

		public void TestRemovingInvoiceLineChargeAggregateTheOtherLines()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3581.21m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 360m;
			line1.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 2.70m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 2880m;
			line2.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 21.60m, testDec.LocalCurrencyCode);

			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 312m;
			line3.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 2.34m, testDec.LocalCurrencyCode);

			BaseInvoiceCharge invFIF = invoice.Charges.AddNew();
			invFIF.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			invFIF.J7_Amount = 55.85m;
			invFIF.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Invoice discount amount in apportioned charges", 26.64m, invoice.GroupCharges[0].J7_Amount);

			line3.Charges.RemoveAndDeleteAll();
			testDec.ResumeApportionment();
			AssertEquals("Invoice discount amount in apportioned charges", 24.30m, invoice.GroupCharges[0].J7_Amount);
		}

		#region Implementation

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = GetNewDeclaration();
				}
				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		protected virtual BaseJobDeclaration GetNewDeclaration() => BaseJobDeclaration.New(Factory);

		BaseJobComInvoiceHeader fInvoice;
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

		BaseJobComInvoiceLine fInvoiceLine;
		protected BaseJobComInvoiceLine InvoiceLine
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

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseInvoiceLineCharge result = Factory.New<BaseInvoiceLineCharge>();
			result.Parent = InvoiceLine;
			return result;
		}

		#endregion
	}

	[TestedType(typeof(BaseInvoiceLineCharge))]
	public class BaseInvoiceLineChargeCollectionTest : BaseInvoiceLineChargeCollectionTest<BaseInvoiceLineCharge>
	{
		protected override JobComInvChargeCollection<BaseInvoiceLineCharge> GetCollectionToTest()
		{
			return new JobComInvChargeCollection<BaseInvoiceLineCharge>(InvoiceLine);
		}
	}
}
