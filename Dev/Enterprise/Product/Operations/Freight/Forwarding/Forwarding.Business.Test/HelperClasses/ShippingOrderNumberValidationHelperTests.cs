using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShippingOrderNumberValidationHelperTests : TestCaseWithFactory
	{
		public void TestDuplicatedSld()
		{
			InsertPackLineTestData("CNSHA", "S00001001", "SLD001");
			InsertEntryNumTestData("CNSHA", "S00001002", "SLD001");
			var shipment3 = InsertPackLineTestData("CNSHA", "S00001003", "SLD001");
			Factory.Save();

			AssertEquals("S00001001,S00001002", string.Join(",", ShippingOrderNumberValidationHelper.GetDuplicatedShipmentsByShippingOrderNumber("SLD001", shipment3.PK, Factory).OrderBy(otherShipment => otherShipment.JS_UniqueConsignRef).Select(otherShipment => otherShipment.JS_UniqueConsignRef)));

			ForwardingShipment InsertPackLineTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var consol = shipment.Consols.AddNew();
				var container = consol.Containers.AddNew();

				var packline = shipment.OuterPackLines.AddNew();
				packline.SetContainer(container.PK);
				packline.JL_ContainerPackingOrder = 1;
				packline.JL_ExportRefNumber = exportRefNumber;

				return shipment;
			}

			ForwardingShipment InsertEntryNumTestData(string origin, string consignRef, string exportRefNumber)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = consignRef;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var shipmentEntryNum = shipment.Numbers.AddNew();
				shipmentEntryNum.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				shipmentEntryNum.CE_EntryNum = exportRefNumber;

				return shipment;
			}
		}
	}
}
