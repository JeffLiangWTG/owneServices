namespace Enterprise.MasterData.Common
{
	public static class UsagePointConstants
	{
		public const string UsageCategoryMdm = "MDM";
		public const string ReportingSourceCw1 = "CW1";

		public static class PriceItemCodes
		{
			public const string OrgMerge = "OMG";
		}

		public static class OrgeMergeActions
		{
			public const string Link = "LNK";
			public const string IgnoreEveryone = "IGN";
			public const string RemoveIgnores = "IGR";
			public const string ExcludeOrg = "EXC";
			public const string IncludeOrg = "INC";
			public const string DeactivateOrg = "DAC";
			public const string OrgMergeSuccess = "OMS";
			public const string OrgMergeFailure = "OMF";
			public const string OrgMergeCancel = "OMC";
		}

		public static class OrgeMergeSourceWindows
		{
			public const string OrgPotentialDupesWindow = "ORG";
			public const string MdmAdminPanelWindow = "MDA";
		}
	}
}
