using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalProcedureCodeCollection))]
class AdditionalProcedureCodeCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAsString()
	{
		var collection = (AdditionalProcedureCodeCollection)GetCollectionToTest();
		CombineAssertions(() =>
		{
			AssertEquals("Empty", 0, collection.Count);
			collection.AsString = "ASDF,QWER,TYUI";
			AssertEquals("multiple", 3, collection.Count);
			collection.AddNew("GHJK");
			AssertEquals("Add new", "ASDF,GHJK,QWER,TYUI", collection.AsString);
		});
	}

	public void TestAllowNew()
	{
		var line = GetInvoiceLine();
		var collection = line.AdditionalProcedureCodes;
		AssertEquals(line.MaxNumberOfAdditionalProcedureCode, collection.MaxCount);
	}

	public void TestAdditionalProcedureCodeType()
	{
		var collection = (AdditionalProcedureCodeCollection)GetCollectionToTest();
		var additionalProcedureCode = collection.AddNew();
		AssertType<AdditionalProcedureCode>(additionalProcedureCode);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => GetInvoiceLine().AdditionalProcedureCodes;

	JobComInvoiceLine GetInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		return line;
	}
}
