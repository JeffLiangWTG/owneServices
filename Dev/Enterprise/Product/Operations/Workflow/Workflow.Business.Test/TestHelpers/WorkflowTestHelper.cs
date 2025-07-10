using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowTestHelper : IWorkflowTestHelper
	{
		IProcessHeader IWorkflowTestHelper.CreateQualityIteration(IProcessTask containmentBarrierTask, IProcessTask iterateFromTask, string resourceUnderReviewStaffCode, string iterationReasonCode, bool shouldCreateWorkflowForIteration)
		{
			return PublishedWorkflowTestHelper.CreateQualityIteration(containmentBarrierTask, iterateFromTask, resourceUnderReviewStaffCode, iterationReasonCode, shouldCreateWorkflowForIteration);
		}

		void IWorkflowTestHelper.CreatePassedContainmentBarrierRecord(IProcessTask containmentBarrierTask, string resourceUnderReviewStaffCode)
		{
			PublishedWorkflowTestHelper.CreatePassedContainmentBarrierRecord(containmentBarrierTask, resourceUnderReviewStaffCode);
		}

		#region EDIMessagePurpose

		IEDIMessagePurpose IWorkflowTestHelper.CreateMesagePurposeToIncludeDocuments(string xmlType, string purposeCode, params string[] eDocCodes)
		{
			return WorkflowEdiMessageTestHelper.CreateMesagePurposeToIncludeDocuments(xmlType, purposeCode, eDocCodes);
		}

		#endregion

		#region UniversalValidationRules

		public BusinessObject CreateUniversalValidationRuleSet(BusinessObjectFactory factory, UniversalDataBuss.Integration.DataContextType dataContext, string code, string criteria, bool isActive = true)
		{
			var ruleHelper = new RuleHelper(factory);
			var ruleSet = ruleHelper.CreateRuleSet(dataContext);
			ruleSet.VRS_Code = code;
			ruleSet.VRS_Criteria = criteria;
			ruleSet.VRS_IsActive = isActive;
			return ruleSet;
		}

		public IReadOnlyUniversalValidationRule AddUniversalValidationRule(BusinessObject ruleSetBizo, string ruleMacro, string statusLevel, string message, bool isActive = true)
		{
			var ruleSet = (UniversalValidationRuleSet)ruleSetBizo;
			var ruleHelper = new RuleHelper(ruleSet.Factory);
			var rule = ruleHelper.AddRule(ruleSet, ruleMacro);
			rule.VR_IsActive = isActive;
			rule.VR_Status = statusLevel;
			rule.VR_MessageLog = message;

			return new ReadOnlyUniversalValidationRule(rule);
		}

		#endregion
	}
}
