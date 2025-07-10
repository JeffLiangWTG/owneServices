using CargoWise.Types;

namespace Enterprise.MasterData.Common
{
	public interface IPatternMatchingBusinessObjects
	{
		ZGuid OrganisationPK { get; set; }
		ZGuid PersonPK { get; set; }
		ZInt HashedValue { get; set; }
		ZString ParentTableCode { get; set; }
		ZGuid ParentId { get; set; }
		ZString PatternMatchingCountryCode { get; set; }
		ZBool IsActive { get; set; }
	}
}
