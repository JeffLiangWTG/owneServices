#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowTestHelper
	{
		#region Containment Barriers

		IProcessHeader CreateQualityIteration(IProcessTask containmentBarrierTask, IProcessTask iterateFromTask, string resourceUnderReviewStaffCode = null, string iterationReasonCode = null, bool shouldCreateWorkflowForIteration = true);
		void CreatePassedContainmentBarrierRecord(IProcessTask containmentBarrierTask, string resourceUnderReviewStaffCode = null);

		#endregion

		#region EDI Message Purpose

		IEDIMessagePurpose CreateMesagePurposeToIncludeDocuments(string xmlType, string purposeCode, params string[] eDocCodes);

		#endregion

		#region UniversalValidationRules

		BusinessObject CreateUniversalValidationRuleSet(BusinessObjectFactory factory, UniversalDataBuss.Integration.DataContextType dataContext, string code, string criteria, bool isActive = true);

		IReadOnlyUniversalValidationRule AddUniversalValidationRule(BusinessObject ruleSetBizo, string ruleMacro, string statusLevel, string message, bool isActive = true);

		#endregion
	}
}

#endif
