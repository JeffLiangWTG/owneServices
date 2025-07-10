using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(TranCircumstance))]
class TranCircumstanceTest : Customs.Business.Testing.CusCodeDataTest<TranCircumstance>
{
	public void TestLoad()
	{
		var code = TranCircumstance.LoadOrCreate(header, 1);
		AssertEquals(code, TranCircumstance.Load(header, 1));
	}

	public void TestTypeOfValidation() => AssertType<TranCircumstanceValidation>(Factory.New<TranCircumstance>().Validation);

	public void TestTypeOfLookups() => AssertType<TranCircumstanceLookups>(Factory.New<TranCircumstance>().Lookups);

	public void TestLoadOrCreate()
	{
		var code = TranCircumstance.LoadOrCreate(header, 1);
		AssertNotNull("Code must be created", code);
		Factory.Save();

		var headerViaNewFactory = Factory.Load<JobComInvoiceHeader>(header.PK);
		var codeViaNewFactory = TranCircumstance.LoadOrCreate(headerViaNewFactory, 1);
		AssertEquals("Code must be loaded", code.PK, codeViaNewFactory.PK);
	}

	public void TestCY_CodeMaxLength()
	{
		var code = TranCircumstance.LoadOrCreate(header, 1);
		AssertEquals(5, code.CY_CodeInfo.MaxLength);
	}

	public void TestSetDefaultValues()
	{
		var code = TranCircumstance.LoadOrCreate(header, 1);
		AssertEquals(PL.Business.CusCodeDataTypeList.Codes.DV1, code.CY_Type);
		AssertEquals(header.PK, code.CY_ParentID);
	}

	public void TestValidation()
	{
		var code = TranCircumstance.LoadOrCreate(header, 1);
		AssertNotNull("Validation should never be null", code.Validation);

		var tranCircumstanceWithNoParents = Factory.New<TranCircumstance>();
		AssertNotNull("Validation should never be null", tranCircumstanceWithNoParents.Validation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<TranCircumstance>();
	}

	protected override IEnumerable<TranCircumstance> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var result = factory.NewWithValidTestData<TranCircumstance>();

		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var header = declaration.Invoices.AddNew();
		header.AdditionalTranCircumstanceCodes.Add(result);

		yield return result;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, TranCircumstance bizObj)
	{
		factory.Load<JobComInvoiceHeader>(bizObj.CY_ParentID);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		header = declaration.Invoices.AddNew();
	}

	JobComInvoiceHeader header;
}
