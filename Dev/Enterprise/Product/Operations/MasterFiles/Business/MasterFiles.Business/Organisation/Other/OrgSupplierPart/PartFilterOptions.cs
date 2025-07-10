using System;

namespace Enterprise.MasterFiles.Business
{
	[Flags]
	public enum PartFilterOptions
	{
		None = 0,
		ExcludeNotForResale = 1,
		IncludeInActive = 2,
		OwnerMandatory = 4
	}
}
