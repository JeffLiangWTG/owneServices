using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.Business.Testing;

sealed class ProcessTemplateValidationConditionCheckerTest : TestCaseWithFactory
{
	public void TestIsCondition1Met() => CombineAssertions(() =>
	{
		AssertEquals("Import", true, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.Import).AreConditionsMet());
		AssertEquals("Export", true, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.Export).AreConditionsMet());
		AssertEquals("NotExport", false, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport).AreConditionsMet());
		AssertEquals("ImportOnly", false, GetChecker(ImportExportCodeList.Codes.ImportOnly).AreConditionsMet());

		return;

		ProcessTemplateValidationConditionChecker GetChecker(string condition1)
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(x => x.IsImport).Returns(true);
			mockDeclaration.Setup(x => x.IsImport).Returns(true);
			var rule = Mock.Of<IProcessTemplateValidation>(
				x => x.P0V_ContextType == (ZString)ProcessTemplateValidationContextType.Codes.CargoWise && x.P0V_Condition1 == (ZString)condition1);
			var result = new ProcessTemplateValidationConditionChecker(rule, mockDeclaration.Object);
			return result;
		}
	});

	public void TestIsCondition2Met() => CombineAssertions(() =>
	{
		AssertEquals("Import", true, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.Import).AreConditionsMet());
		AssertEquals("Export", true, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.Export).AreConditionsMet());
		AssertEquals("NotExport", false, GetChecker(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport).AreConditionsMet());
		AssertEquals("ImportOnly", false, GetChecker(ImportExportCodeList.Codes.ImportOnly).AreConditionsMet());

		return;

		ProcessTemplateValidationConditionChecker GetChecker(string condition2)
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(x => x.IsImport).Returns(true);
			mockDeclaration.Setup(x => x.IsImport).Returns(true);
			var rule = Mock.Of<IProcessTemplateValidation>(
				x => x.P0V_ContextType == (ZString)ProcessTemplateValidationContextType.Codes.CargoWise && x.P0V_Condition2 == (ZString)condition2);
			var result = new ProcessTemplateValidationConditionChecker(rule, mockDeclaration.Object);
			return result;
		}
	});

	public void TestIsCompanyMet()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		var rule = Mock.Of<IProcessTemplateValidation>(
			x => x.P0V_ContextType == (ZString)ProcessTemplateValidationContextType.Codes.CargoWise && x.P0V_GC_Company == declaration.JE_GC);
		var checker = new ProcessTemplateValidationConditionChecker(rule, declaration);
		AssertEquals(true, checker.AreConditionsMet());
	}
}
