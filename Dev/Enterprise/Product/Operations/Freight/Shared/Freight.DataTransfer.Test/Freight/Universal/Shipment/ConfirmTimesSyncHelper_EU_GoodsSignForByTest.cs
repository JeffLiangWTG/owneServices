using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ConfirmTimesSyncHelper_EU_GoodsSignForByTest : TestCaseWithFactory
	{
		public void TestEU_GoodsSignForBy_DoNotExceedMaxLength_ForContainerisedConfirms()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			var consol = Factory.New<CommonConsol>();
			consol.Shipments.Add(shipment);
			consol.Containers.AddNew();
			AssertEquals(1, shipment.Containers.Count());

			var confirm = shipment.Containers.First().OriginConfirm;
			AssertNotNull(confirm);

			var logger = new TestErrorLogger();
			ConfirmTimesSyncHelper.SetConfirmSignedBy(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, "RALPH LAUREN ASIA PACIFIC LTD", logger);
			AssertEquals("RALPH LAUREN ASIA PACIFIC", confirm.EU_GoodsSignForBy);
		}

		public void TestEU_GoodsSignForBy_DoNotExceedMaxLength_ForNonContainerisedConfirms()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			var today = ZDateTime.Today;
			shipment.DocsAndCartage.JP_PickupRequiredBy = today;
			AssertEquals(1, shipment.PickupConfirms.Count);

			var logger = new TestErrorLogger();
			ConfirmTimesSyncHelper.SetConfirmSignedBy(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, "RALPH LAUREN ASIA PACIFIC LTD", logger);
			AssertEquals("RALPH LAUREN ASIA PACIFIC", shipment.PickupConfirms[0].EU_GoodsSignForBy);
		}
	}
}
