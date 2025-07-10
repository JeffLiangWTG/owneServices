namespace Enterprise.MasterFiles.Business
{
	public static class OrgMergeBillingConstants
	{
		public const string BillingAction = "ACT";
		public const string ActionSource = "SRC";
		public const string Confidence = "CON";
		public const string Category = "MDM";
		public const string PriceCode = "OMG";
		public const string ReportingSource = "CW1";
		public const string ALLTargets = "ALL";

		public static class Source
		{
			public const string DeduplicationResultsViewerForm = "ORG";
			public const string AdministrationPanelForm = "MDA";
			public const string MergeIntoOrgHeaderForm = "GRD";
			public const string MergeOrgHeaderForm = "GR1";
		}

		public static class Action
		{
			public const string Link = "LNK";
			public const string Exclude = "EXC";
			public const string MergeSuccess = "OMS";
			public const string IgnoreForEveryone = "IGN";
			public const string Include = "INC";
			public const string MergeFailure = "OMF";
			public const string RemoveIgnores = "IGR";
			public const string Deactivate = "DAC";
			public const string MergeCancel = "OMC";
		}
	}
}
