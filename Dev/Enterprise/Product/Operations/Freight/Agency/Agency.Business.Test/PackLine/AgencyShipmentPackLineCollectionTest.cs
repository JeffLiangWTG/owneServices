using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentPackLineCollection))]
	internal class AgencyShipmentPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPackLineDescription()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_GoodsDescription = "This is a very brief description";
			PackLine line1 = shipment.OuterPackLines.AddNew();
			AssertEquals("PackLine description should be shipment's short description", "This is a very brief description", line1.JL_DetailedDescription);
			shipment.DetailedGoodsDescriptionNoteText = "This is a very detailed description";
			PackLine line2 = shipment.OuterPackLines.AddNew();
			AssertEquals("PackLine description should be shipment's detailed description", "This is a very detailed description", line2.JL_DetailedDescription);
		}

		public void TestDefaultPackLineWouldNotBeCreatedForROROMaster()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.JS_OuterPacks = 2;
			AssertEquals("Default packline created", 1, shipment.OuterPackLines.Count);
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				shipment.JS_PackingMode = mode;
				shipment.OuterPackLines.RemoveAndDeleteAll();
				shipment.JS_OuterPacks = 2;
				AssertEquals("Default packline not created for non-FCL shipment", 0, shipment.OuterPackLines.Count);
			}
		}

		public void TestDangerousGoodsChangeTracking_AddedUpdatedRemovedElements()
		{
			var shipment = Factory.New<AgencyShipment>();
			var collection = new AgencyShipmentPackLineCollection(shipment);
			ICollectionChangeTrackable<UNDGDataItem> dangerousGoodsTracking = collection;
			var container = collection.AddNew();
			var undg = container.UNDGs.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
			undg.Delete();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.ChangedElements);
			undg = container.UNDGs.AddNew();
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.ChangedElements);
			undg.DI_TechnicalName = "ZEBRA FART";
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<UNDGDataItem>(), dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
			undg.Delete();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.AddedElements);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AgencyShipmentContainer>(), dangerousGoodsTracking.UpdatedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.RemovedElements);
			AssertContainsExactElementsInAnyOrder(new[] { undg }, dangerousGoodsTracking.ChangedElements);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			return shipment.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AgencyShipmentPackLine packLine = Factory.New<AgencyShipmentPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			return packLine;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(AgencyShipmentPackLineCollection), GetCollectionToTest().GetType());
		}
		#endregion
	}
}
