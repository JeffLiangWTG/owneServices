using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEntryLine()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_CL = entryLine.PK;
			AssertEquals("Entry line", entryLine, lineFee.EntryLine);
		}

		public void TestSupportNotes()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			Assert("Support Notes = false", !lineFee.SupportsNotes);
		}

		public void TestIsConfirmed()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			AssertEquals(true, lineFee.IsConfirmed);
			AssertEquals(false, lineFee.SupportsClone());

			lineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			AssertEquals(false, lineFee.IsConfirmed);
		}

		public void TestCF_BaseValueDecimalPlaces()
		{
			var lineFee = Factory.New(GetExpectedBusinessObjectType()) as CusEntryLineFee;
			var actualValue = lineFee?.CF_BaseValueDecimalPlaces ?? -1;

			AssertEquals("Decimal places", ExpectedCF_BaseValueDecimalPlaces, actualValue);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryLineFee.Schema.CF_Source };
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var fee = (CusEntryLineFee)base.GetNewBusinessObjectForDeleteTest(factory);
			fee.EntryLine.CL_LineNumber = 123;
			fee.CF_ChargeAmount = 100m;
			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(fee.Factory);

			var header = fee.EntryLine.Header;
			header.PivotsToContainers.RemoveAndDeleteAll();
			header.PivotsToContainers.GetOrCreatePivotFor(header.Declaration.CusContainers.AddNew());

			var declaration = fee.EntryLine.Declaration ?? factory.New<BaseJobDeclaration>();

			var invoice = (declaration.Invoices.Count > 0 ? declaration.Invoices[0] : null) ?? declaration.Invoices.AddNew();

			invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;

			var invoiceLine = (declaration.InvoiceLines.Count > 0 ? declaration.InvoiceLines[0] : null) ?? declaration.InvoiceLines.AddNew();

			invoiceLine.JI_CL = fee.EntryLine.PK;

			return fee;
		}

		protected virtual int ExpectedCF_BaseValueDecimalPlaces => CusEntryLineFeeSchema.CF_BaseValue.Scale;
	}
}
