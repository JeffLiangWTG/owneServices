using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	[TestedType(typeof(Shipment))]
	sealed class ShipmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllPackingLinesIncludeCoLoad_BCNAndSCN()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_UniqueConsignRef = "SHP000001";

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 3;

			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_UniqueConsignRef = "SHP000002";

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 4;

			var shipment3 = shipment.CoLoadShipments.AddNew();
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse;
			shipment3.JS_UniqueConsignRef = "SHP000003";

			var packingLine3 = shipment3.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 5;

			var shipment4 = shipment.CoLoadShipments.AddNew();
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.ShippersConsolLead;
			shipment4.JS_UniqueConsignRef = "SHP000004";

			var packingLine4 = shipment4.OuterPackLines.AddNew();
			packingLine4.JL_PackageCount = 7;

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertContainsExactElementsInAnyOrder(new[] { packingLine1.PK, packingLine2.PK, packingLine3.PK, packingLine4.PK }, shipmentDO.AllPackingLinesIncludeCoLoad.Select(p => (ZGuid)p.Identifier));
		}

		public void TestAllPackingLinesIncludeCoLoad_ASM()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_UniqueConsignRef = "SHP000002";

			var packingLine1 = shipment2.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 4;

			var shipment3 = shipment.CoLoadShipments.AddNew();
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment3.JS_UniqueConsignRef = "SHP000003";

			var shipment4 = shipment3.CoLoadShipments.AddNew();
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment4.JS_UniqueConsignRef = "SHP000004";

			var packingLine2 = shipment4.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 5;

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertContainsExactElementsInAnyOrder(new[] { packingLine1.PK, packingLine2.PK }, shipmentDO.AllPackingLinesIncludeCoLoad.Select(p => (ZGuid)p.Identifier));
		}

		public void TestFormatITNNumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(string.Empty, shipmentDO.ITNNumber);

			var itnCusEntryNumber = shipment.CusEntryNumbers.AddNew();
			itnCusEntryNumber.CE_EntryType = "ITN";
			itnCusEntryNumber.CE_EntryNum = "C001,C002,C001";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001, C002, C001", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "X20230316311995 X20230316306971 X20230316309620 X20230316302085 X20230316303144 X20230316301396 X20230316301837 X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = @"X20230316311995
X20230316306971
X20230316309620
X20230316302085
X20230316303144
X20230316301396
X20230316301837
X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "X20230316311995 ,X20230316306971 ,X20230316309620 ,X20230316302085 ,X20230316303144 ,X20230316301396 ,X20230316301837 ,X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "X20230316311995\tX20230316306971\tX20230316309620\tX20230316302085\tX20230316303144\tX20230316301396\tX20230316301837\t\t\tX20230316303825\t\t\t\r\n";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "X20230316311995,X20230316306971,,X20230316309620;X20230316302085;;/X20230316303144/X20230316301396//X20230316301837  \t\t\tX20230316303825\t\t\t\r\n";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "x20230316311995";
			AssertEquals("X20230316311995", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = "                                            X20230316306971                                            ";
			AssertEquals("X20230316306971", shipmentDO.ITNNumber);

			shipmentDO.ITNNumber = @"X20230316311995	\tX20230316306971";
			AssertEquals(@"X20230316311995, \TX20230316306971", shipmentDO.ITNNumber);
		}

		public void TestFormatUCRNumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(string.Empty, shipmentDO.UCRNumber);

			var ucrCusEntryNumber = shipment.CusEntryNumbers.AddNew();
			ucrCusEntryNumber.CE_EntryType = "UCR";
			ucrCusEntryNumber.CE_EntryNum = "C001,C002,C001";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001, C002, C001", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "X20230316311995 X20230316306971 X20230316309620 X20230316302085 X20230316303144 X20230316301396 X20230316301837 X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = @"X20230316311995
X20230316306971
X20230316309620
X20230316302085
X20230316303144
X20230316301396
X20230316301837
X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "X20230316311995 ,X20230316306971 ,X20230316309620 ,X20230316302085 ,X20230316303144 ,X20230316301396 ,X20230316301837 ,X20230316303825";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "X20230316311995\tX20230316306971\tX20230316309620\tX20230316302085\tX20230316303144\tX20230316301396\tX20230316301837\t\t\tX20230316303825\t\t\t\r\n";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "X20230316311995,X20230316306971,,X20230316309620;X20230316302085;;/X20230316303144/X20230316301396//X20230316301837  \t\t\tX20230316303825\t\t\t\r\n";
			AssertEquals("X20230316311995, X20230316306971, X20230316309620, X20230316302085, X20230316303144, X20230316301396, X20230316301837, X20230316303825", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "x20230316311995";
			AssertEquals("X20230316311995", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = "                                            X20230316306971                                            ";
			AssertEquals("X20230316306971", shipmentDO.UCRNumber);

			shipmentDO.UCRNumber = @"X20230316311995	\tX20230316306971";
			AssertEquals(@"X20230316311995, \TX20230316306971", shipmentDO.UCRNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Shipment(ZGuid.NewZGuid())
			{
				PackingLines = new[] { new PackingLine(ZGuid.NewZGuid(), Factory) },
				Shipments = new[] { new Shipment(ZGuid.NewZGuid()) }
			};
		}
	}
}
