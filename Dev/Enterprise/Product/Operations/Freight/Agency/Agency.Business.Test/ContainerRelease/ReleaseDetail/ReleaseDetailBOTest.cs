using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseDetail))]
	internal class ReleaseDetailBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Impletentation
		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipment>().BookedContainers.AddNew();
			return new ReleaseDetail(container);
		}
		#endregion
	}
}
