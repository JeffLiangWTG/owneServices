using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseDetailCollection))]
	internal class ReleaseDetailCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<ReleaseDetailCollection>
	{
		#region Implementation
		protected override ReleaseDetailCollection GetCollectionToTest()
		{
			return new ReleaseDetailCollection(Factory.New<AgencyShipment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReleaseDetail(Factory.New<AgencyShipmentContainer>());
		}
		#endregion
	}
}
