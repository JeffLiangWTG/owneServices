using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PackLineBulkUpdateRecord))]
	sealed class PackLineBulkUpdateRecordTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			return new PackLineBulkUpdateRecord(packLine);
		}

		public void TestShipmentId()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ShipmentId should match the packline's parent shipment's JS_UniqueConsignRef", shipment.JS_UniqueConsignRef, testObj.ShipmentId);
		}

		public void TestShipmentInspectionType()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ShipmentInspectionType should match the packline's parent shipment's JS_InspectionTypeCode", shipment.JS_InspectionTypeCode, testObj.ShipmentInspectionType);
		}

		public void TestPackLineId()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("PackLineId should match the packline's JL_PackLineId", packLine.JL_PackLineId, testObj.PackLineId);
		}

		public void TestPackLineInspectionType()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine.JL_InspectionTypeCode = "XRY";
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("PackLineInspectionType should match the packline's JL_InspectionTypeCode", packLine.JL_InspectionTypeCode, testObj.PackLineInspectionType);

			testObj.PackLineInspectionType = "APP";
			AssertEquals("Updating the PackLineInspectionType updates the underlying pack line's inspection type as well", "APP", packLine.JL_InspectionTypeCode);
		}

		public void TestPackLineInspectionTypeInfo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_InspectionTypeCode = "UNK";

			var packLine = shipment.OuterPackLines.AddNew();

			var testObj = new PackLineBulkUpdateRecord(packLine);
			AssertNoErrors("Precondition: the packLine starts off without any error messages.", packLine.JL_InspectionTypeCodeInfo);
			AssertNoErrors("Precondition: the test object starts off without any error messages.", testObj.PackLineInspectionTypeInfo);

			packLine.JL_InspectionTypeCode = "z9z";
			packLine.Validation.ValidateAll();

			AssertHasErrors("Precondition: the underlying packLine has an error message on the JL_InspectionTypeCode.", packLine.JL_InspectionTypeCodeInfo);
			AssertHasErrors("The test object has errors on the InspectionTypeCode to match the underlying packline's errors", testObj.PackLineInspectionTypeInfo);
		}

		public void TestPackLineInspectionType_ReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_InspectionTypeCode = "UNK";

			var packLine = shipment.OuterPackLines.AddNew();

			var testObj = new PackLineBulkUpdateRecord(packLine);
			AssertEquals("Precondition: the packLine's inspection type is not read-only.", false, packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			AssertEquals("Precondition: the test object's inspection type is not read-only.", false, testObj.PackLineInspectionTypeInfo.ReadOnly);
			shipment.JS_InspectionTypeCode = "APP";
			AssertEquals("Precondition: the packLine's inspection type is read-only.", true, packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			AssertEquals("The test object's inspection type is read-only when the underlying packline's Inspection Type is read-only.", true, packLine.JL_InspectionTypeCodeInfo.ReadOnly);
		}

		public void TestPackageCount()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("PackageCount should match the packline's JL_PackageCount", packLine.JL_PackageCount, testObj.PackageCount);
		}

		public void TestPackageType()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("PackageType should match the packline's JL_F3_NKPackType", packLine.JL_F3_NKPackType, testObj.PackageType);
		}

		public void TestActualVolume()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ActualVolume should match the packline's JL_ActualVolume", packLine.JL_ActualVolume, testObj.ActualVolume);
		}

		public void TestActualVolumeUQ()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ActualVolume should match the packline's JL_ActualVolumeUQ", packLine.JL_ActualVolumeUQ, testObj.ActualVolumeUQ);
		}

		public void TestActualWeight()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ActualWeight should match the packline's JL_ActualWeight", packLine.JL_ActualWeight, testObj.ActualWeight);
		}

		public void TestActualWeightUQ()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var testObj = new PackLineBulkUpdateRecord(packLine);

			AssertEquals("ActualWeight should match the packline's JL_ActualWeightUQ", packLine.JL_ActualWeightUQ, testObj.ActualWeightUQ);
		}

		public void TestPackLineInspectionTypes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_InspectionTypeCode = "UNK";

			var packLine = shipment.OuterPackLines.AddNew();
			var testObj = new PackLineBulkUpdateRecord(packLine);
			AssertNotNull("Precondition: the packline's InspectionTypes list is not null", packLine.InspectionTypes);
			AssertEquals("Precondition: there is more than one inspection type available on the packLine", true, packLine.InspectionTypes.Count > 1);
			AssertContainsExactElementsInAnyOrder(packLine.InspectionTypes, testObj.PackLineInspectionTypes);
		}
	}
}
