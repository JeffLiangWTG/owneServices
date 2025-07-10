using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirmDivotCollection))]
	sealed class CommonPickupDeliveryConfirmDivotCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonPickupDeliveryConfirmDivotCollection>
	{
		public void TestDebugLogMessage()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			var pickupConfirm = shipment.PickupConfirms.AddNew();
			var divot = pickupConfirm.Divots.AddNew();
			AssertContains(FormattableString.Invariant($@"OnAdded - Divot created: {divot.PK}
Confirm: IsInDatabase: 'False', EU_PickupDeliveryType: 'PCU', EU_JS: '{shipment.PK}', EU_D1: '{ZGuid.Empty}', EU_JC: '{ZGuid.Empty}'
EU_SystemCreateTimeUtc: '', EU_SystemCreateUser: '', EU_SystemLastEditTimeUtc: '', EU_SystemLastEditUser: ''"),
CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage);
		}

		protected override CommonPickupDeliveryConfirmDivotCollection GetCollectionToTest()
		{
			PackLine packLine = Factory.New<PackLine>();
			return new CommonPickupDeliveryConfirmDivotCollection(packLine);
		}
	}
}
