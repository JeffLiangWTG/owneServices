using System;

namespace Enterprise.Packing.DataTransfer.Universal
{
	[Flags]
	public enum ImportOption
	{
		Default = 0,
		MatchOnPackageIDs = 1,
		KeepUnmatchedPackages = 2,
		PartialMatch = 4,
		MatchAndRelabelOnPreviousPackageID = 8,
		ImportInnersAsNonTrackableItem = 16,
		MatchOnPackingLineID = 32
	}
}
