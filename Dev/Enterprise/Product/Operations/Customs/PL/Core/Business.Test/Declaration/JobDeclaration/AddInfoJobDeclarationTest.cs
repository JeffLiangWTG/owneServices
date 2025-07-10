using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class AddInfoJobDeclarationBOTest : TestCaseWithFactory
{
	public void TestGetNewValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>("Export Declaration", declaration.AddInfoValidation);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>("Import Declaration", declaration.AddInfoValidation);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExitSummaryJobDeclarationValidation>("Exit Summary Declaration", declaration.AddInfoValidation);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>("Miscellaneous Declaration", declaration.AddInfoValidation);
		});
	}

	public void TestGetNewLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>("Export Declaration", declaration.AddInfoLookups);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<JobDeclarationLookups>("Import Declaration", declaration.AddInfoLookups);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<ExportJobDeclarationLookups>("Exit Summary Declaration", declaration.AddInfoLookups);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<ExportJobDeclarationLookups>("Miscellaneous Declaration", declaration.AddInfoLookups);
		});
	}

	public void TestZG_ExciseCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(1, declaration.ZG_ExciseCodeInfo.MaxLength);
	}

	public void TestZG_VATDeferType()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(1, declaration.ZG_VATDeferTypeInfo.MaxLength);
	}
}
