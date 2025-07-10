using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class PGATrackingStatusListTest : TestCase
	{
		public void TestShouldTrack()
		{
			var list = new PGATrackingStatusList();
			list.RemoveCode(PGATrackingStatusList.Codes.Added);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, PGATrackingStatusList.ShouldTrack(pair.Code));
			}
			AssertEquals(true, PGATrackingStatusList.ShouldTrack(PGATrackingStatusList.Codes.Added));
		}

		public void TestIsDeletedOrBeingDeleted()
		{
			var list = new PGATrackingStatusList();
			list.RemoveCode(PGATrackingStatusList.Codes.Deleted);
			list.RemoveCode(PGATrackingStatusList.Codes.Deleting);
			list.RemoveCode(PGATrackingStatusList.Codes.ToBeDeleted);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, PGATrackingStatusList.IsDeletedOrBeingDeleted(pair.Code));
			}
			AssertEquals(true, PGATrackingStatusList.IsDeletedOrBeingDeleted(PGATrackingStatusList.Codes.Deleted));
			AssertEquals(true, PGATrackingStatusList.IsDeletedOrBeingDeleted(PGATrackingStatusList.Codes.Deleting));
			AssertEquals(true, PGATrackingStatusList.IsDeletedOrBeingDeleted(PGATrackingStatusList.Codes.ToBeDeleted));
		}

		public void TestCanBeChangedToBeUpdated()
		{
			var list = new PGATrackingStatusList();
			list.RemoveCode(PGATrackingStatusList.Codes.Added);
			list.RemoveCode(PGATrackingStatusList.Codes.ToBeDeleted);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, PGATrackingStatusList.CanBeChangedToBeUpdated(pair.Code));
			}
			AssertEquals(true, PGATrackingStatusList.CanBeChangedToBeUpdated(PGATrackingStatusList.Codes.Added));
			AssertEquals(true, PGATrackingStatusList.CanBeChangedToBeUpdated(PGATrackingStatusList.Codes.ToBeDeleted));
		}
	}
}
