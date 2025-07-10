using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AdditionalProcedureCode))]
class AdditionalProcedureCodeTest : Customs.Business.Testing.CusCodeDataTest<AdditionalProcedureCode>
{
	public void TestSetDefaultValues()
	{
		AssertEquals(EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode, GetAdditionalProcedureCode(Factory).CY_Type);
	}

	public void TestValidation()
	{
		CombineAssertions(() =>
		{
			var procedureCode = GetAdditionalProcedureCode(Factory);
			AssertType<ImportAdditionalProcedureCodeValidation>("IMP", procedureCode.Validation);

			procedureCode = GetAdditionalProcedureCode(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Export);
			AssertType<AdditionalProcedureCodeValidation>("EXP", procedureCode.Validation);

			procedureCode = GetAdditionalProcedureCode(Factory, Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms);
			AssertType<AdditionalProcedureCodeValidation>("MSC", procedureCode.Validation);
		});
	}

	public void TestParent() => AssertType<JobComInvoiceLine>(GetAdditionalProcedureCode(Factory).Parent);

	static AdditionalProcedureCode GetAdditionalProcedureCode(BusinessObjectFactory factory, string messageType = Common.Shared.SharedJobMessageTypeList.Codes.Import)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		return invoiceLine.AdditionalProcedureCodes.AddNew();
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<AdditionalProcedureCode>();
	}

	protected override IEnumerable<AdditionalProcedureCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var result = factory.NewWithValidTestData<AdditionalProcedureCode>();

		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		line.AdditionalProcedureCodes.Add(result);

		yield return result;
	}
}
