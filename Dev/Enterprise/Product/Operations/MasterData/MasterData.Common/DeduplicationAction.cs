namespace Enterprise.MasterData.Common
{
	public enum DeduplicationAction
	{
		None = 0,
		Ignore = 1,
		Link = 2,
		Merge = 3,
		NotMatched = 4,
		OpenMaster = 5,
		OpenTarget = 6,
		ExcludeCountriesFilterChanged = 7,
		ExcludeInactiveFilterChanged = 8,
		MasterExclusionToggled = 9,
		ReloadRequired = 10,
		ShowIgnoredFilterChanged = 11
	}
}
