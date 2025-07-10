using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class OrganisationPatternMatchingDomainRegenerator : PatternMatchingDomainRegenerator<OrgHeader>
	{
		public OrganisationPatternMatchingDomainRegenerator(PatternMatchingRecalculator<OrgHeader> recalculator)
			: base(recalculator)
		{
		}
	}
}
