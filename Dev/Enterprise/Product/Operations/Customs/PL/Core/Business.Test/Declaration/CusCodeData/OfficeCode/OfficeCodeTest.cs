using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(OfficeCode))]
sealed class OfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<OfficeCode>
{
	public void TestGetNewValidation()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var office = declaration.CustomsOffices.AddNew();
			AssertType<ExportJobDeclarationOfficeCodeValidation>("Export", office.Validation);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ImportJobDeclarationOfficeCodeValidation>("Import", office.Validation);
		});
	}

	public void TestLookup()
	{
		var declaration = Factory.New<JobDeclaration>();
		var office = declaration.CustomsOffices.AddNew();
		AssertType<OfficeCodeLookups>(office.Lookups);
	}

	protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return factory.New<JobDeclaration>().CustomsOffices.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsOffices.AddNew();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();
}
