using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentExportAWBRateLineCollection))]
	sealed class ShipmentExportAWBRateLineCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ShipmentExportAWBHeader master = Factory.New<ShipmentExportAWBHeader>();
			master.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			return new ShipmentExportAWBRateLineCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ShipmentExportAWBRateLine>();
		}

		#endregion
	}
}
