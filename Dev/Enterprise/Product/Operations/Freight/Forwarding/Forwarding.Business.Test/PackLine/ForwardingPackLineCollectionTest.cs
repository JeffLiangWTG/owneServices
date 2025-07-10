using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackLineCollection))]
	public class ForwardingPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Tests

		public void TestRemovePackLineTriggerSyncDirty()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			TestSynchroniser sync = new TestSynchroniser();
			shipment.PackLineSynchroniser = sync;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			sync.MarkedDirty = false;
			shipment.OuterPackLines.RemoveAndDelete(packLine);
			AssertEquals(true, sync.MarkedDirty);

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			Factory.Save();

			AssertEquals(false, packLine.HasChanges);
			AssertEquals(true, packLine.IsInDatabase);
			sync.MarkedDirty = false;
			shipment.OuterPackLines.Remove(packLine);
			AssertEquals(true, sync.MarkedDirty);
		}

		public void TestSuspendReadOnly()
		{
			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			AssertEquals(0, ((IBusinessObjectInternals)packLine1).ParentCollections.Length);

			var packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			AssertEquals(0, ((IBusinessObjectInternals)packLine2).ParentCollections.Length);

			var collection1 = new ForwardingPackLineCollection(CreateShipmentForReadOnlyCollection());
			collection1.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, collection1.ReadOnly);

			collection1.Add(packLine1);
			AssertContainsExactElementsInAnyOrder(new ForwardingPackLineCollection[] { collection1 }, ((IBusinessObjectInternals)packLine1).ParentCollections);
			AssertEquals(true, packLine1.ReadOnly);
			AssertEquals(false, packLine2.ReadOnly);

			var collection2 = new ForwardingPackLineCollection(CreateShipmentForReadOnlyCollection());
			collection2.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, collection2.ReadOnly);

			collection2.Add(packLine1);
			collection2.Add(packLine2);
			AssertContainsExactElementsInAnyOrder(new ForwardingPackLineCollection[] { collection1, collection2 }, ((IBusinessObjectInternals)packLine1).ParentCollections);
			AssertContainsExactElementsInAnyOrder(new ForwardingPackLineCollection[] { collection2 }, ((IBusinessObjectInternals)packLine2).ParentCollections);
			AssertEquals(true, packLine1.ReadOnly);
			AssertEquals(true, packLine2.ReadOnly);

			collection1.SuspendReadOnly = true;
			AssertEquals(false, packLine1.ReadOnly);
			AssertEquals(true, packLine2.ReadOnly);

			collection1.SuspendReadOnly = false;
			collection2.SuspendReadOnly = true;
			AssertEquals(false, packLine1.ReadOnly);
			AssertEquals(false, packLine2.ReadOnly);

			collection2.Remove(packLine1);

			AssertEquals(true, packLine1.ReadOnly);
			AssertEquals(false, packLine2.ReadOnly);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(ForwardingPackLineCollection), GetCollectionToTest().GetType());
		}

		public void TestJL_InspectionTypeIsNotValidatedOnLoad()
		{
			var expectedWarning = "This Shipment is destined for the United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_InspectionTypeCode = "UNK";

			var packLine = shipment.OuterPackLines.AddNew();

			AssertEquals("UNK", packLine.JL_InspectionTypeCode);
			AssertNoWarning("Validation should not run on AddNew - it causes performance issues", packLine.JL_InspectionTypeCodeInfo, expectedWarning);

			packLine.Validation.ValidateJL_InspectionTypeCode();
			AssertHasWarning("Validation warning is now added", packLine.JL_InspectionTypeCodeInfo, expectedWarning);

			Factory.Save();

			var reloadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			var reloadedPackLine = reloadedShipment.OuterPackLines[0];
			AssertNoWarning("Validation should not run on Load - it causes performance issues", reloadedPackLine.JL_InspectionTypeCodeInfo, expectedWarning);

			reloadedPackLine.Validation.ValidateJL_InspectionTypeCode();
			AssertHasWarning("Validation warning is now added", reloadedPackLine.JL_InspectionTypeCodeInfo, expectedWarning);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return shipment.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingPackLine packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;

			return packLine;
		}

		ForwardingShipment CreateShipmentForReadOnlyCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			return shipment;
		}

		#endregion
	}
}
