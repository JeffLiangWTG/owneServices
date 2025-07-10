using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgReceivablesModuleFilter))]
	internal class OrgReceivablesModuleFilterTest : ModuleTextFilterTest
	{
		#region InvoiceTypeFilter

		OrganisationFilterBusinessObject FilterStripBizO
		{
			get
			{
				if (filterStripBizO == null)
				{
					filterStripBizO = new OrganisationFilterBusinessObject();
				}
				return filterStripBizO;
			}
		}
		OrganisationFilterBusinessObject filterStripBizO;

		OrgReceivablesModuleFilter OrgReceivablesModuleFilterForTest
		{
			get
			{
				if (orgReceivablesModuleFilterForTest == null)
				{
					orgReceivablesModuleFilterForTest = (OrgReceivablesModuleFilter)FilterStripBizO["Receivables - Invoice Number"];
				}
				return orgReceivablesModuleFilterForTest;
			}
		}
		OrgReceivablesModuleFilter orgReceivablesModuleFilterForTest;

		public void TestInvoiceNumberFilter()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;

			// Valid Numbers that should be found
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionNum, "00001000", ((ZByte)0));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00002000", ((ZByte)1));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "00003000", ((ZByte)2));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ReceiptBatchNo, "00004000", ((ZByte)3));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionNum, "00005000", ((ZByte)4));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionReference, "00006000", ((ZByte)5));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "00007000", ((ZByte)6));
			CreateNewInvoice(org, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_ReceiptBatchNo, "00008000", ((ZByte)7));

			// Invalid numbers that should not be found
			CreateNewInvoice(org, LedgerTypes.CashBook, TransactionTypes.DDRBatch, AccTransactionHeaderSchema.AH_TransactionNum, "00009000", ((ZByte)8));
			CreateNewInvoice(org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00010000", ((ZByte)9));
			Factory.Save();

			// Testing Starts With
			var orgReceivablesModuleFilterForTest = (OrgReceivablesModuleFilter)FilterStripBizO["Receivables - Invoice Number"];
			orgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.All;
			orgReceivablesModuleFilterForTest.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			orgReceivablesModuleFilterForTest.IsActive = true;

			AssertInvoiceSearch("00001000", true);
			AssertInvoiceSearch("00002000", true);
			AssertInvoiceSearch("00003000", true);
			AssertInvoiceSearch("00004000", true);
			AssertInvoiceSearch("00005000", true);
			AssertInvoiceSearch("00006000", true);
			AssertInvoiceSearch("00007000", true);
			AssertInvoiceSearch("00008000", true);
			AssertInvoiceSearch("00009000", false);
			AssertInvoiceSearch("00010000", false);

			// Testing Contains
			orgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.All;
			orgReceivablesModuleFilterForTest.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertInvoiceSearch("1000", true);
			AssertInvoiceSearch("2000", true);
			AssertInvoiceSearch("3000", true);
			AssertInvoiceSearch("4000", true);
			AssertInvoiceSearch("5000", true);
			AssertInvoiceSearch("6000", true);
			AssertInvoiceSearch("7000", true);
			AssertInvoiceSearch("8000", true);
			AssertInvoiceSearch("9000", false);
			AssertInvoiceSearch("10000", false);

			// Testing Invoice Type Filter and Equal comparison operator
			orgReceivablesModuleFilterForTest.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertInvoiceSearch("00001000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.InvoiceNumber, true);
			AssertInvoiceSearch("00002000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.GovtTaxNumber, true);
			AssertInvoiceSearch("00003000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.JobInvoiceNumber, true);
			AssertInvoiceSearch("00004000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.BatchInvoiceNumber, true);
			AssertInvoiceSearch("00005000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.InvoiceNumber, true);
			AssertInvoiceSearch("00006000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.GovtTaxNumber, true);
			AssertInvoiceSearch("00007000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.JobInvoiceNumber, true);
			AssertInvoiceSearch("00008000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.BatchInvoiceNumber, true);

			AssertInvoiceSearch("1000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.InvoiceNumber, false);
			AssertInvoiceSearch("2000", OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.GovtTaxNumber, false);
		}

		public void TestDefaultProperty_MaxLength()
		{
			var filter = new OrgReceivablesModuleFilter("For test");
			var type = typeof(OrgReceivablesModuleFilter);
			var property = type.GetProperty("Property_MaxLength", BindingFlags.NonPublic | BindingFlags.Instance);
			var propertyMaxLength = new int[] { AccTransactionHeaderSchema.AH_TransactionNum.MaxLength, AccTransactionHeaderSchema.AH_TransactionReference.MaxLength, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength }.Max();
			AssertEquals($"The Property_MaxLength of property for OrgReceivablesModuleFilter should be {propertyMaxLength}", propertyMaxLength, (int)property.GetValue(filter));
		}

		public void TestModuleFilterWithAllInvoiceType()
		{
			var orgs = CreateOrganization(4, true);

			// Valid Numbers that should be found
			CreateNewInvoice(orgs[0], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionNum, "00001000", ((ZByte)0));
			CreateNewInvoice(orgs[1], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00001001", ((ZByte)1));
			CreateNewInvoice(orgs[2], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "00002000", ((ZByte)2));
			CreateNewInvoice(orgs[3], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ReceiptBatchNo, "00002001", ((ZByte)3));

			// Invalid numbers that should not be found
			CreateNewInvoice(orgs[0], LedgerTypes.CashBook, TransactionTypes.DDRBatch, AccTransactionHeaderSchema.AH_TransactionNum, "00003000", ((ZByte)4));
			CreateNewInvoice(orgs[1], LedgerTypes.AccountsPayable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00004000", ((ZByte)5));
			Factory.Save();

			OrgHeaderCollection result = new OrgHeaderCollection(Factory);
			OrgReceivablesModuleFilterForTest.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			OrgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.All;
			OrgReceivablesModuleFilterForTest.IsActive = true;

			OrgReceivablesModuleFilterForTest.Property = "0000";
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 4);

			OrgReceivablesModuleFilterForTest.Property = "00001";
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 2);

			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234";
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 0);

			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234567890123456789";
			AssertEquals(true, OrgReceivablesModuleFilterForTest.Query.IsEmpty);
		}

		public void TestModuleFilterWithSingleInvoiceType()
		{
			var orgs = CreateOrganization(4, true);

			// Valid Numbers that should be found
			CreateNewInvoice(orgs[0], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionNum, "00001000", ((ZByte)0));
			CreateNewInvoice(orgs[1], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00001000", ((ZByte)1));
			CreateNewInvoice(orgs[2], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, "00001000", ((ZByte)2));
			CreateNewInvoice(orgs[3], LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_ReceiptBatchNo, "00001000", ((ZByte)3));

			// Invalid numbers that should not be found
			CreateNewInvoice(orgs[0], LedgerTypes.CashBook, TransactionTypes.DDRBatch, AccTransactionHeaderSchema.AH_TransactionNum, "00001001", ((ZByte)8));
			CreateNewInvoice(orgs[1], LedgerTypes.AccountsPayable, TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionReference, "00001002", ((ZByte)9));
			Factory.Save();

			OrgHeaderCollection result = new OrgHeaderCollection(Factory);
			OrgReceivablesModuleFilterForTest.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			OrgReceivablesModuleFilterForTest.IsActive = true;

			OrgReceivablesModuleFilterForTest.Property = "0000100";
			OrgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.InvoiceNumber;
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 1);
			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234567890123456789";
			AssertEquals(true, OrgReceivablesModuleFilterForTest.Query.IsEmpty);

			OrgReceivablesModuleFilterForTest.Property = "0000100";
			OrgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.GovtTaxNumber;
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 1);
			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234";
			AssertEquals(true, OrgReceivablesModuleFilterForTest.Query.IsEmpty);

			OrgReceivablesModuleFilterForTest.Property = "0000100";
			OrgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.JobInvoiceNumber;
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 1);
			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234567890123456789";
			AssertEquals(true, OrgReceivablesModuleFilterForTest.Query.IsEmpty);

			OrgReceivablesModuleFilterForTest.Property = "0000100";
			OrgReceivablesModuleFilterForTest.InvoiceType = OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.BatchInvoiceNumber;
			result.Load(FilterStripBizO.Filter);
			AssertEquals(result.Count, 1);
			OrgReceivablesModuleFilterForTest.Property = "0123456789012345678901234";
			AssertEquals(true, OrgReceivablesModuleFilterForTest.Query.IsEmpty);
		}
		void AssertInvoiceSearch(ZString invoiceNumber, bool shouldBeFound)
		{
			AssertInvoiceSearch(invoiceNumber, Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.All, shouldBeFound);
		}

		void AssertInvoiceSearch(ZString invoiceNumber, ZString invoiceType, bool shouldBeFound)
		{
			OrgHeaderCollection result = new OrgHeaderCollection(Factory);
			OrgReceivablesModuleFilterForTest.Property = invoiceNumber;
			OrgReceivablesModuleFilterForTest.InvoiceType = invoiceType;
			result.Load(FilterStripBizO.Filter);
			Assert("Invoice number " + invoiceNumber + " should " + (!shouldBeFound ? "NOT " : "") + " have been found", result.Count == (shouldBeFound ? 1 : 0));
		}

		void CreateNewInvoice(OrgHeader org, ZString ledgerType, ZString transactionType, SchemaColumn transactionNumberField, ZString transactionNum, ZByte transCount)
		{
			AccTransactionHeader result = Factory.New<AccTransactionHeader>();
			result.AH_OH = org.PK;
			result.AH_Ledger = ledgerType;
			result.AH_TransactionType = transactionType;
			result[transactionNumberField] = transactionNum;
			result.AH_TransactionCount = transCount;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			if (result.AH_TransactionNum.IsEmpty)
			{ 
				result.AH_TransactionNum = "VALUEFORTEST";
			}
		}

		List<OrgHeader> CreateOrganization(int count, bool isDebtor)
		{
			var result = new List<OrgHeader>();
			for (int i = 0; i < count; i++)
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_IsDebtor = isDebtor;
				result.Add(org);
			}
			return result;
		}

		#endregion

		#region Override

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new OrgReceivablesModuleFilter("moo");
		}

		#endregion
	}
}
