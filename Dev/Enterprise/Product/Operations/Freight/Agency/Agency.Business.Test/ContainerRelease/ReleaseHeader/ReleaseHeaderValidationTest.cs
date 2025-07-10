using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ReleaseHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestReleaseNumber()
		{
			AgencyBookingContainer container = Shipment.BookedContainers.AddNew();
			container.JC_ReleaseNum = "Ref-1";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 5;
			Header.Init();
			Header.ReleaseNumber = "";
			AssertHasError(Header.ReleaseNumberInfo, "Please enter a Release Number.");
			Header.ReleaseNumber = "Ref-1";
			AssertNoNotifications(Header.ReleaseNumberInfo);
			Header.ReleaseNumber = "XXX";
			AssertHasError(Header.ReleaseNumberInfo, "Enter a valid Release Number.");
			header = new ReleaseHeader(Shipment, false);
			Header.Init();
			Header.Validation.ValidateReleaseNumber();
			AssertNoNotifications(Header.ReleaseNumberInfo);
		}

		#region Implementation
		ReleaseHeader Header
		{
			get
			{
				return header ?? (header = new ReleaseHeader(Shipment, true));
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
