using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseHeaderLookups : BusinessObjectLookupsTestCase
	{
		public void TestReleaseNumbers()
		{
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-3";
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-2";
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-1";
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-1";
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-2";
			Shipment.BookedContainers.AddNew().JC_ReleaseNum = "Rel-3";
			const string expectedList = "Rel-1, Rel-2, Rel-3";
			AssertEquals(expectedList, Header.Lookups.ReleaseNumbers.CodesAsString);
		}

		#region Implementation
		ReleaseHeader Header
		{
			get
			{
				return header ?? (header = new ReleaseHeader(Shipment, false));
			}
		}

		ReleaseHeader header;
		AgencyBooking Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking shipment;
		#endregion
	}
}
