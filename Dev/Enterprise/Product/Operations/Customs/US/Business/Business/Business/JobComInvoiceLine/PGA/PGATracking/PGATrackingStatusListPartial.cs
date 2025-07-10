using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class PGATrackingStatusList
	{
		public static bool ShouldTrack(ZString status)
		{
			return status == Codes.Added;
		}

		public static bool IsDeletedOrBeingDeleted(ZString status)
		{
			return status == Codes.Deleted || status == Codes.Deleting || status == Codes.ToBeDeleted;
		}

		public static bool CanBeChangedToBeUpdated(ZString status)
		{
			return status == Codes.Added || status == Codes.ToBeDeleted;
		}
	}
}
