using CargoWise.EventReference;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseInstanceLookups : BaseAgencyTest
	{
		public void TestReleaseType_List()
		{
			var booking = Factory.New<AgencyBooking>();
			var header = new ReleaseHeader(booking, true);
			var list = header.Instances.AddNew().Lookups.ReleaseType_List;
			AssertNotEquals(null, list);
			AssertEquals(4, list.Count);
			AssertEquals(Constants.EventReferenceReleaseTypes.Desc.Reprint, list[Constants.EventReferenceReleaseTypes.Codes.Reprint].Description);
		}
	}
}
