using CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business
{
	public static class TriggerConditionTypeConverter
	{
		public static TriggerConditionType Convert(string code)
		{
			switch (code)
			{
				case EventReferenceConditionList.Codes.ConditionWithMacros:
					return TriggerConditionType.Macro;
				case EventReferenceConditionList.Codes.EventReference:
					return TriggerConditionType.Reference;
				case EventReferenceConditionList.Codes.EventReferenceParameters:
					return TriggerConditionType.ReferenceWithParameters;
				case EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions:
					return TriggerConditionType.ReferenceWithRegex;
				case EventReferenceConditionList.Codes.EventReferenceWithWildcards:
					return TriggerConditionType.ReferenceWithWildcards;
				case EventReferenceConditionList.Codes.UserDefined:
					return TriggerConditionType.UserDefinedMacro;
				default:
					return TriggerConditionType.None;
			}
		}
	}
}
