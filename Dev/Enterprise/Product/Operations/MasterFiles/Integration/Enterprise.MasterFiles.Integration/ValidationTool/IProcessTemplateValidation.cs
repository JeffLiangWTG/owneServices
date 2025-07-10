using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration;

public interface IProcessTemplateValidation : IWorkflowTypeProvider
{
	ZGuid PK { get; }
	ZString P0V_Condition1 { get; }
	ZString P0V_Condition2 { get; }
	ZString P0V_Condition2Value { get; }
	ZGuid P0V_GC_Company { get; }
	ZString P0V_ContextType { get; }
	ZString P0V_ValidationRule { get; }
	ZString P0V_Severity { get; }
	ZString P0V_Message { get; }
	ZString P0V_FieldToDisplayValidation { get; }
	ZString P0V_Description { get; }
	ZGuid P0_GC { get; }
	ZGuid P0V_RQT_RequestTypeOnFailure { get; }
	ZBool P0V_LogValidationFailEvent { get; }
}
