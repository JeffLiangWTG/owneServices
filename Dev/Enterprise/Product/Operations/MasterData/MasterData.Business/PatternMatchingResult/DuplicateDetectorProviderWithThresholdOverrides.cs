using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class DuplicateDetectorProviderWithThresholdOverrides : DuplicateDetectorProvider, IDuplicateDetectorProvider
	{
		protected override GlbPersonDuplicationFinder GetGlbPersonFinder(GlbPerson person)
		{
			return new GlbPersonDuplicationFinderWithThresholdOverride(person, true, true);
		}

		protected override OrgHeaderDuplicationFinder GetOrgHeaderFinder(OrgHeader header)
		{
			return new OrgHeaderDuplicationFinderWithThresholdOverride(header, true, true);
		}
	}
}
