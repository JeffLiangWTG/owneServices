using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class TallyPackLineTest : BaseFreightTest
	{
		public void TestJY_LCLUnpackDateisDefaultedWhen1stShipmentisOutturned()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection collectionForAdding = new PackUnpackShipmentDependentCollection(Factory, container);
			PackUnpackShipment shipmentAdded1 = Factory.New<PackUnpackShipment>();
			PackUnpackShipment shipmentAdded2 = Factory.New<PackUnpackShipment>();
			PackLine line1 = shipmentAdded1.OuterPackLines.AddNew();
			PackLine line2 = shipmentAdded2.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line2.JL_PackageCount = 12;
			line1.JL_Outturn = 0;
			line2.JL_Outturn = 0;
			collectionForAdding.Add(shipmentAdded1);
			collectionForAdding.Add(shipmentAdded2);
			container.JC_LCLUnpack = ZDateTime.Empty;
			AssertEquals("Shipment1 is not outturn yet", shipmentAdded1.OuterPackLines.TotalOutturned, 0);
			AssertEquals("Shipment2 is not outturn yet", shipmentAdded2.OuterPackLines.TotalOutturned, 0);
			line1.JL_Outturn = 10;
			AssertEquals("Container JY_LCLUnpackDate should be updated", false, container.JC_LCLUnpack.IsEmpty);
			ZDateTime currentLCLUnpack = container.JC_LCLUnpack;
			line2.JL_Outturn = 12;
			AssertEquals("The JY_LCLUnpack Date is not updated", currentLCLUnpack, container.JC_LCLUnpack);
		}

		public void TestOutturnEditableInPacklines()
		{
			PackUnpackLoadListConsol loadList = GetLoadlistToTally();
			AssertEquals("Container Pack Lines Read Only", false, loadList.Containers[0].PackLines.ReadOnly);
			AssertEquals("Outturn Read Only", false, loadList.Containers[0].PackLines.ReadOnly);
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var loadList2 = Factory.Load<PackUnpackLoadListConsol>(loadList.PK);

			AssertEquals("Container Pack Lines Read Only", false, loadList.Containers[0].PackLines.ReadOnly);
			AssertEquals("Outturn Read Only", false, loadList.Containers[0].PackLines.ReadOnly);
		}

		public void TestActualUnitsReadonly()
		{
			var line = Factory.NewWithValidTestData<TallyPackLine>();
			AssertEquals("Pack Line Actual Volume Unit is Readonly", true, line.JL_ActualVolumeUQInfo.ReadOnly);
			AssertEquals("Pack Line Dimensions Unit is Readonly", true, line.JL_UnitOfDimensionInfo.ReadOnly);
			AssertEquals("Pack Line Actual weight Unit is Readonly", true, line.JL_ActualWeightUQInfo.ReadOnly);
		}

		#region Implmentation

		protected PackUnpackLoadListConsol GetLoadlistToTally()
		{
			PackUnpackLoadListConsol result = Factory.New<PackUnpackLoadListConsol>();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport = result.Transports[0];
			transport.JW_JX = ImportSailing1.PK;
			transport.JW_ATA = ZDateTime.Today;
			TallyContainer container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFCU3365489";
			PackUnpackShipment shipment = result.Shipments.AddNew();
			PackLine line = shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Shipment Packs == Line Packs", 10, line.JL_PackageCount);
			return result;
		}

		#endregion
	}
}
