using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IDpsEntityProvider : IScreeningStatusProvider
	{
		ZBool NeedsScreening { get; }
		ZBool ShouldUpdateRelatedJobs { get; set; }
		ZString OriginalScreeningStatus { get; }
		void InvalidateByLocalDataChanges();
	}
}
