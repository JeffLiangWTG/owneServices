using System;

namespace Enterprise.MasterFiles.Business
{
	[Flags]
	public enum TemplateEntityType
	{
		None = 0,
		All = 1 << 1,
		Workflows = 1 << 2,
		Tasks = 1 << 3,
		CompletionStatements = 1 << 4,
		Milestones = 1 << 5,
		Triggers = 1 << 6,
		ScreenLayout = 1 << 7,
		CustomFields = 1 << 8,
		ReleaseGroupRules = 1 << 9,
		Exceptions = 1 << 10,
		ValidationTool = 1 << 11,
	}
}
