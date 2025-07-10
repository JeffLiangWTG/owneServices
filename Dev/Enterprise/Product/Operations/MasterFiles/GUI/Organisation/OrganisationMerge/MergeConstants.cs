namespace Enterprise.MasterFiles.GUI
{
	public static class MergeConstants
	{
		public static string MergeWarningTitle
		{
			get { return Res.GetString("3d42d1e1-71c8-4366-9803-83bb057f005c", "CRITICAL WARNING"); }
		}

		public static string MergeConfirmationMessage
		{
			get { return Res.GetString("b2fa3de1-7aed-4e44-9d31-0c38ed4d6e22", "THIS PROCESS IS IRREVERSIBLE"); }
		}

		public static string GetMergeWarningSingle(string oldOrgCode, string newOrgCode)
		{
			return Res.GetString("60cc00b0-eb23-49d8-b37c-78ca828e3b96", @"You are about to merge organization {0} into organization {1}. This will switch all jobs in the system to {1} that previously belonged to {0}, and will delete {0}. If merging causes Rate Entries to overlap, then only Rates belonging to {1} will be kept, the others will be deleted. This action is irreversible and cannot be undone.

If you select this option CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?", oldOrgCode, newOrgCode);
		}

		public static string GetMergeWarningManyToOne(string oldOrgCode, string newOrgCode)
		{
			return Res.GetString("5da56d31-ca68-4fd4-bdf8-7ab1b67ff0e1", @"You are about to merge organization(s) {0} into organization {1}. This will switch all jobs in the system to {1} that previously belonged to any of these organizations, and will delete these organizations. If merging causes Rate Entries to overlap, then only Rates belonging to {1} will be kept, the others will be deleted. This action is irreversible and cannot be undone.

If you select this option CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?", oldOrgCode, newOrgCode);
		}

		public static string MergeWarningManyToMany
		{
			get { return Res.GetString("28bc8889-8211-4831-8a55-a10c55fbf45e", "Every Organization you have selected will be merged with any other similar Organizations found. This operation is time consuming.This action is irreversible and cannot be undone even if you cancel merging process in the middle. If you select this option it is irreversible and CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?"); }
		}
	}
}
