using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer.Testing
{
	class PortMessagingShipmentDataObjectForConsolWriterTest : TestCaseWithFactory
	{
		public void TestPopulateSZBNumbersOfSubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2";

			var bcnShipment = consol.Shipments.AddNew();
			bcnShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var bcnShipmentSZBNumber = bcnShipment.Numbers.AddNew();
			bcnShipmentSZBNumber.CE_EntryNum = "Z0000";
			bcnShipmentSZBNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			bcnShipmentSZBNumber.CE_EntryIsSystemGenerated = true;

			var subFCLShipment = bcnShipment.CoLoadShipments.AddNew();
			subFCLShipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var subFCLShipmentSZBNumber = subFCLShipment.Numbers.AddNew();
			subFCLShipmentSZBNumber.CE_EntryNum = "Z0001";
			subFCLShipmentSZBNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			subFCLShipmentSZBNumber.CE_EntryIsSystemGenerated = true;

			var subLCLShipment = bcnShipment.CoLoadShipments.AddNew();
			subLCLShipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			var subLCLShipmentSZBNumber = subLCLShipment.Numbers.AddNew();
			subLCLShipmentSZBNumber.CE_EntryNum = "Z0002";
			subLCLShipmentSZBNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			subLCLShipmentSZBNumber.CE_EntryIsSystemGenerated = true;

			var subBLKShipment = bcnShipment.CoLoadShipments.AddNew();
			subBLKShipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;

			var subBLKShipmentSZBNumber = subBLKShipment.Numbers.AddNew();
			subBLKShipmentSZBNumber.CE_EntryNum = "Z0003";
			subBLKShipmentSZBNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			subBLKShipmentSZBNumber.CE_EntryIsSystemGenerated = true;

			var subBLKShipment2 = bcnShipment.CoLoadShipments.AddNew();
			subBLKShipment2.JS_PackingMode = Core.Constants.ContainerModes.Bulk;

			var subBLKShipment2SZBNumber = subBLKShipment.Numbers.AddNew();
			subBLKShipment2SZBNumber.CE_EntryNum = "Z0004";
			subBLKShipment2SZBNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			subBLKShipment2SZBNumber.CE_EntryIsSystemGenerated = false;

			Factory.Save();

			AssertEquals("Z0000", bcnShipment.JS_SZB);
			AssertEquals("Z0001", subFCLShipment.JS_SZB);
			AssertEquals("Z0002", subLCLShipment.JS_SZB);
			AssertEquals("Z0003", subBLKShipment.JS_SZB);
			AssertNullOrEmpty(subBLKShipment2.JS_SZB);

			var writer = new PortMessagingShipmentDataObjectForConsolWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, bcnShipment)), false, false, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var bcnShipmentDataObject = writer.GetDataObject(bcnShipment);

			AssertNotNull(bcnShipmentDataObject);

			var szbNumberAdditionalReferences = bcnShipmentDataObject.AdditionalReferenceCollection
				.Where(additionalReference => additionalReference.Type != null && additionalReference.Type.Code.Value == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber);

			AssertEquals(3, szbNumberAdditionalReferences.Count());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Z0000", "Z0002", "Z0003" }, szbNumberAdditionalReferences.Select(additionalReference => additionalReference.ReferenceNumber.Value));
		}

		public void TestGetAllPackingLinesIncludeCoLoad()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2";

			var bcnShipment = consol.Shipments.AddNew();
			bcnShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var subShipment = bcnShipment.CoLoadShipments.AddNew();

			var bcnShipmentPackLine = bcnShipment.OuterPackLines.AddNew();
			bcnShipmentPackLine.SetContainer(container1.PK);
			bcnShipmentPackLine.JL_PackageCount = 3;

			var subShipmentPackLine = subShipment.OuterPackLines.AddNew();
			subShipmentPackLine.SetContainer(container2.PK);
			subShipmentPackLine.JL_PackageCount = 6;

			var writer = new PortMessagingShipmentDataObjectForConsolWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, bcnShipment)), false, false, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var bcnShipmentDataObject = writer.GetDataObject(bcnShipment);

			AssertNotNull(bcnShipmentDataObject);
			AssertEquals("Included both packingline from bcn and sub", 2, bcnShipmentDataObject.PackingLineCollection.Count);

			var packLine1Data = bcnShipmentDataObject.PackingLineCollection.First(line => line.ContainerNumber.Value == "CON1");
			var packLine2Data = bcnShipmentDataObject.PackingLineCollection.First(line => line.ContainerNumber.Value == "CON2");
			AssertEquals(3, packLine1Data.PackQty.Value);
			AssertEquals(6, packLine2Data.PackQty.Value);
		}
	}
}
