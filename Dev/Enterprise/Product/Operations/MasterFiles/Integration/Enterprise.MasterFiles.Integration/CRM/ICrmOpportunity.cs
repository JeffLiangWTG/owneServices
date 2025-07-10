using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICrmOpportunity : IBusiness
	{
		bool IsRestrictedForCurrentUser();
	}
}
