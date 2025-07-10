using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupplementaryCode))]
sealed class SupplementaryCodeTest : CusCodeDataWithOrderAbstractTest<SupplementaryCode>
{
	public void TestValidationType()
	{
		var supplementaryCode = Factory.NewWithValidTestData<SupplementaryCode>();
		AssertType<SupplementaryCodeValidation>(supplementaryCode.Validation);
	}

	public void TestLookupsType()
	{
		var supplementaryCode = Factory.NewWithValidTestData<SupplementaryCode>();
		AssertType<SupplementaryCodeLookups>(supplementaryCode.Lookups);
	}

	protected override string ExpectedCusCodeDataType => BaseCusCodeDataTypeList.Codes.SupplementaryCode;

	protected override IEnumerable<SupplementaryCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_SupplementaryCode1 = "AAAA";
		var codeLoader = new BaseSupplementaryCode.Loader(factory);
		yield return codeLoader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var code = factory.NewWithValidTestData<SupplementaryCode>();
		code.CY_Code = SupplementaryCode.CodeDataType;
		return code;
	}
}
