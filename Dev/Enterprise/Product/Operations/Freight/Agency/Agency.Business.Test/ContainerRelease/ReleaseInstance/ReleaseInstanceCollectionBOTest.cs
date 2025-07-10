using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseInstanceCollection))]
	internal class ReleaseInstanceCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<ReleaseInstanceCollection>
	{
		#region Implementation
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReleaseInstance(Header);
		}

		protected override ReleaseInstanceCollection GetCollectionToTest()
		{
			return new ReleaseInstanceCollection(Header);
		}

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
