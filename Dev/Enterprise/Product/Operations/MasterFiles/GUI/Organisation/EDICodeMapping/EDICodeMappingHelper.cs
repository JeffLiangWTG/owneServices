using Enterprise.Core;

namespace Enterprise.MasterFiles.GUI
{
	public static class EDICodeMappingHelper
	{
		public static bool IsGuidFindBox(string relationship)
		{
			return relationship == Constants.OrgPatternMatchOverrideRelationships.Organisation ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Port ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Currency ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Country ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Commodities ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Equipment ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.ContainerType ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.Warehouse ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.ServiceLevel ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.IntZone ||
			relationship == Constants.OrgPatternMatchOverrideRelationships.DocumentType;
		}

		public static bool IsCodeFindBox(string relationship)
		{
			return relationship == Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
		}

		public static bool IsDropDownFindBox(string relationship)
		{
			return !IsGuidFindBox(relationship) && !IsCodeFindBox(relationship);
		}
	}
}
