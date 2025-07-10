using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	class ReadOnlyUniversalValidationRule : IReadOnlyUniversalValidationRule
	{
		public ReadOnlyUniversalValidationRule(UniversalValidationRule bizRule)
		{
			BusinessRule = bizRule.VR_BusinessRule;
			IsActive = bizRule.VR_IsActive;
			MessageLog = bizRule.VR_MessageLog;
			Sequence = bizRule.VR_Sequence;
			Status = bizRule.VR_Status;
			IsError = bizRule.IsError;
		}

		public ZString BusinessRule { get; }
		public ZBool IsActive { get; }
		public ZString MessageLog { get; }
		public ZShort Sequence { get; }
		public ZString Status { get; }
		public ZBool IsError { get; }
	}
}
