using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusGoodsLocationValidationTest : TestCaseWithFactory
{
	public void TestCGL_Qualifier_Required()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = cusEntryInstruction.PK;

		var goodsLocation = cusEntryInstruction.GoodsLocation;

		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = ZString.Empty;
			invoiceLine.JI_Procedure = "1000";
			AssertNoMessageErrorContaining("TestQualifier : JI_Procedure is 1000 Qualifier is empty", goodsLocation.CGL_QualifierInfo, QualifierErrorMessage);

			invoiceLine.JI_Procedure = "1007";
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining("TestQualifier : JI_Procedure is 1007 Qualifier is empty", goodsLocation.CGL_QualifierInfo, QualifierErrorMessage);

			goodsLocation.CGL_Qualifier = "Q";
			AssertNoMessageErrorContaining("TestQualifier : JI_Procedure is 1007 Qualifier is not empty", goodsLocation.CGL_QualifierInfo, QualifierErrorMessage);
		});
	}

	public void TestCGL_Type_Required()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = cusEntryInstruction.PK;

		var goodsLocation = cusEntryInstruction.GoodsLocation;

		CombineAssertions(() =>
		{
			invoiceLine.JI_Procedure = "1000";
			goodsLocation.CGL_Type = ZString.Empty;
			AssertNoMessageErrorContaining("TestType : JI_Procedure is 1000 Type is empty", goodsLocation.CGL_TypeInfo, TypeErrorMessage);

			invoiceLine.JI_Procedure = "1007";
			goodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining("TestType : JI_Procedure is 1007 Type is empty", goodsLocation.CGL_TypeInfo, TypeErrorMessage);

			goodsLocation.CGL_Type = "R";
			AssertNoMessageErrorContaining("TestType : JI_Procedure is 1007 Type is not empty", goodsLocation.CGL_TypeInfo, TypeErrorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	const string QualifierErrorMessage = "[R0086E] You have not entered a Qualifier for Location of Goods.";
	const string TypeErrorMessage = "[R0086E] You have not entered a Type for Location of Goods.";
}
