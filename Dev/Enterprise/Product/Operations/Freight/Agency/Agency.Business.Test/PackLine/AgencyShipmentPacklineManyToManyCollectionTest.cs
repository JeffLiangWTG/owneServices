using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(AgencyShipmentPackLineManyToManyCollection))]
	internal class AgencyShipmentPacklineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<AgencyShipment>().BookedContainers.AddNew().PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<AgencyShipment>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
