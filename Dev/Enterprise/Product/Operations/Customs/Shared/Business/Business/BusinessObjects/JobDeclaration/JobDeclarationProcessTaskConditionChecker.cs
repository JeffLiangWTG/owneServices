using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business;

public sealed class JobDeclarationProcessTaskConditionChecker
{
	BaseJobDeclaration Parent { get; }

	public JobDeclarationProcessTaskConditionChecker(BaseJobDeclaration parent)
	{
		Parent = Argument.NotNull(parent, nameof(parent));
	}

	public bool IsCondition1Met(ZString conditionCode) => IsConditionMet(conditionCode);

	public bool IsCondition2Met(ZString conditionCode, ZString value) => IsConditionMet(conditionCode);

	bool IsConditionMet(ZString conditionCode) => (string)conditionCode switch
	{
		JobDeclarationWorkflowCondition1CodeList.Codes.Import => Parent.IsImport,
		JobDeclarationWorkflowCondition1CodeList.Codes.Export => Parent.IsExport,
		JobDeclarationWorkflowCondition1CodeList.Codes.NotExport => !Parent.IsExport,
		ImportExportCodeList.Codes.ImportOnly => Parent.JE_MessageType == JobMessageTypeList.Codes.Import,
		_ => false
	};
}
