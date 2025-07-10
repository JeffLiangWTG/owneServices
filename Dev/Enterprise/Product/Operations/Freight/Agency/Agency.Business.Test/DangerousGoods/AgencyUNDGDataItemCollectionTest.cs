using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyUNDGDataItemCollection))]
	internal class AgencyUNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<AgencyUNDGDataItemCollection>
	{
		protected override AgencyUNDGDataItemCollection GetCollectionToTest()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			collection.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First().PK;
			Factory.Save();
			return collection;
		}

		public void TestUNDGSubstanceCollection()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var collection = new AgencyUNDGDataItemCollection(packLine);
			AssertNotEquals(null, collection.AllUNDGSubstances);
			AssertType<AgencyUNDGSubstanceCollection>(collection.AllUNDGSubstances);
		}
	}
}
