using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRatingZone
	{
		ZString ZoneCode { get; }
		ZGuid RelatedOrgPK { get; }
	}
}
