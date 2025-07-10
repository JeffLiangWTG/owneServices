using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.ReportTesting.MasterFiles;

[TemplateName(ExcelTemplateName)]
class WorkflowValidationFailedReport : TemplateTestCase
{
	const string ExcelTemplateName = "Workflow Validation Failed Report";
	protected override bool ReportRequiresColumnHeadings => false;

	protected override ReportForTestAllowingCacheReset GetReport(ExcelTemplate excelTemplate)
	{
		var failure = new NonPersistentRuleValidationResult(Mock.Of<IProcessTemplateValidation>(), Mock.Of<IBusiness>(), ProcessTemplateValidationSeverityList.Codes.Error, null, false, null);
		var failures = new NonPersistentRuleValidationResultCollection(Factory) { failure };
		var wrapper = new NonPersistentValidationFailureDocumentWrapper(failures);
		return new ReportForTestAllowingCacheReset(new DocumentPack(), excelTemplate, wrapper, ExcelTemplateName, null, DocumentDirection.ANY, false);
	}
}
