using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public interface IPGADataChangeTrackerSupporter : IBusinessObjectInternals
	{
		PGADataChangeTracker Tracker { get; }
	}
}
