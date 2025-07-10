using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing;

sealed class JobDeclarationProcessTaskConditionCheckerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() =>
		{
			_ = new JobDeclarationProcessTaskConditionChecker(null);
		});
	}

	public void TestIsCondition1Met() => CombineAssertions(() =>
	{
		var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
		mockDeclaration.Setup(x => x.IsImport).Returns(true);
		mockDeclaration.Setup(x => x.IsImport).Returns(true);
		var checker = new JobDeclarationProcessTaskConditionChecker(mockDeclaration.Object);

		AssertEquals("Import", true, checker.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Import));
		AssertEquals("Export", true, checker.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.Export));
		AssertEquals("NotExport", false, checker.IsCondition1Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport));
		AssertEquals("ImportOnly", false, checker.IsCondition1Met(ImportExportCodeList.Codes.ImportOnly));
	});

	public void TestIsCondition2Met() => CombineAssertions(() =>
	{
		var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
		mockDeclaration.Setup(x => x.IsImport).Returns(true);
		mockDeclaration.Setup(x => x.IsImport).Returns(true);
		var checker = new JobDeclarationProcessTaskConditionChecker(mockDeclaration.Object);

		AssertEquals("Import", true, checker.IsCondition2Met(JobDeclarationWorkflowCondition1CodeList.Codes.Import, ZString.Empty));
		AssertEquals("Export", true, checker.IsCondition2Met(JobDeclarationWorkflowCondition1CodeList.Codes.Export, ZString.Empty));
		AssertEquals("NotExport", false, checker.IsCondition2Met(JobDeclarationWorkflowCondition1CodeList.Codes.NotExport, ZString.Empty));
		AssertEquals("ImportOnly", false, checker.IsCondition2Met(ImportExportCodeList.Codes.ImportOnly, ZString.Empty));
	});
}
