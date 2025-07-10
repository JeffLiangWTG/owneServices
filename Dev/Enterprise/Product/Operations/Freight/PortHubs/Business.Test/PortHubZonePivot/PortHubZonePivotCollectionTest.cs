using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.PortHubs.Business;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortHubZonePivotCollection))]
	public class PortHubZonePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PortHubZonePivotCollection(GetNewPortHubSelection(), Factory);
		}

		protected virtual PortHubSelection GetNewPortHubSelection()
		{
			return Factory.New<PortHubSelection>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PortHubZonePivot>();
		}
	}
}
