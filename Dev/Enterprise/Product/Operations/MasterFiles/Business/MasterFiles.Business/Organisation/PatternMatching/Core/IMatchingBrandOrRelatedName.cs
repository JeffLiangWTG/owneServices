using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public interface IMatchingBrandOrRelatedName
	{
		ZGuid PK { get; }
		ZString P1_RelatedName { get; }
		ZString P1_RelatedNameOriginal { get; }
	}
}
