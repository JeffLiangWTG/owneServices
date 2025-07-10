using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PackLineBulkUpdateDataSource))]
	sealed class PackLineBulkUpdateDataSourceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddAllOuterPackLines_FromForwardingShipment()
		{
			var shipment1 = GetNewShipment();
			var packline1 = shipment1.OuterPackLines.AddNew();
			var packline2 = shipment1.OuterPackLines.AddNew();
			var packline3 = shipment1.OuterPackLines.AddNew();
			var innerPackline1 = shipment1.InnerPackLines.AddNew();

			var testObj = new PackLineBulkUpdateDataSource(Factory);
			testObj.AddAllOuterPackLines(shipment1);

			Assert("Precondition: shipment1 has multiple OuterPackLines", shipment1.OuterPackLines.Count > 1);
			Assert("Precondition: shipment1 has at least one InnerPackLine", shipment1.InnerPackLines.Count >= 1);
			var expected = new List<ZString>();
			expected.Add(packline1.JL_PackLineId);
			expected.Add(packline2.JL_PackLineId);
			expected.Add(packline3.JL_PackLineId);

			var actual = new List<ZString>();
			foreach (PackLineBulkUpdateRecord packline in testObj.OuterPackLines)
			{
				actual.Add(packline.PackLineId);
			}

			AssertContainsExactElementsInAnyOrder("All OuterPackLines from shipment1 were added to the object under test",
				expected, actual);
		}

		public void TestAddAllOuterPackLines_FromForwardingShipmentsEnumerable()
		{
			var shipment1 = GetNewShipment();
			var packline1 = shipment1.OuterPackLines.AddNew();
			var packline2 = shipment1.OuterPackLines.AddNew();
			var packline3 = shipment1.OuterPackLines.AddNew();
			var innerPackline1 = shipment1.InnerPackLines.AddNew();
			var shipment2 = GetNewShipment();
			var packline4 = shipment2.OuterPackLines.AddNew();
			var packline5 = shipment2.OuterPackLines.AddNew();
			var innerPackline2 = shipment1.InnerPackLines.AddNew();

			var shipment3 = GetNewShipment();
			shipment3.JS_InspectionTypeCode = "APP";
			var packline6 = shipment3.OuterPackLines.AddNew();
			var innerPackline3 = shipment1.InnerPackLines.AddNew();

			var shipmentList = new List<ForwardingShipment>();
			shipmentList.Add(shipment1);
			shipmentList.Add(shipment2);
			shipmentList.Add(shipment3);

			var testObj = new PackLineBulkUpdateDataSource(Factory);
			testObj.AddAllOuterPackLines(shipmentList);

			var expected = new List<ZString>();
			expected.Add(packline1.JL_PackLineId);
			expected.Add(packline2.JL_PackLineId);
			expected.Add(packline3.JL_PackLineId);
			expected.Add(packline4.JL_PackLineId);
			expected.Add(packline5.JL_PackLineId);
			expected.Add(packline6.JL_PackLineId);

			var actual = new List<ZString>();
			foreach (PackLineBulkUpdateRecord packline in testObj.OuterPackLines)
			{
				actual.Add(packline.PackLineId);
			}

			AssertContainsExactElementsInAnyOrder("All OuterPackLines from shipments 1, 2, and 3 were added to the object under test",
				expected, actual);
		}

		public void TestOuterPackLinesBulkSetter_ReadOnly()
		{
			var shipment = GetNewShipment();
			var packline = shipment.OuterPackLines.AddNew();
			var testObj = new PackLineBulkUpdateDataSource(Factory);
			testObj.AddAllOuterPackLines(shipment);
			Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
			Assert(!packline.JL_InspectionTypeCodeInfo.ReadOnly);

			Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;
			Assert(packline.JL_InspectionTypeCodeInfo.ReadOnly);
		}

		public void TestInspectionTypesForOuterPackLinesBulkSetter()
		{
			var shipment1 = GetNewShipment();
			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_InspectionTypeCode = "UNK";
			var packline2 = shipment1.OuterPackLines.AddNew();
			packline2.JL_InspectionTypeCode = "UNK";
			var packline3 = shipment1.OuterPackLines.AddNew();
			packline3.JL_InspectionTypeCode = "UNK";

			var shipment2 = GetNewShipment();
			var packline4 = shipment2.OuterPackLines.AddNew();
			packline4.JL_InspectionTypeCode = "CMD";
			var packline5 = shipment2.OuterPackLines.AddNew();
			packline5.JL_InspectionTypeCode = "CMD";

			var shipment3 = GetNewShipment();
			shipment3.JS_InspectionTypeCode = "APP";
			var packline6 = shipment3.OuterPackLines.AddNew();
			AssertEquals("Precondition: packline6 has read-only inspection type", true, packline6.JL_InspectionTypeCodeInfo.ReadOnly);

			var testObj = new PackLineBulkUpdateDataSource(Factory);
			testObj.AddAllOuterPackLines(shipment1);
			testObj.AddAllOuterPackLines(shipment2);
			testObj.AddAllOuterPackLines(shipment3);

			testObj.OuterPackLinesInspectionTypeBulkSetter = "XRY";
			AssertEquals("XRY", packline1.JL_InspectionTypeCode);
			AssertEquals("XRY", packline2.JL_InspectionTypeCode);
			AssertEquals("XRY", packline3.JL_InspectionTypeCode);
			AssertEquals("XRY", packline4.JL_InspectionTypeCode);
			AssertEquals("XRY", packline5.JL_InspectionTypeCode);
			AssertEquals(ZString.Empty, packline6.JL_InspectionTypeCode);
		}

		public void TestOuterPackLinesInspectionTypeBulkSetter()
		{
			var testObj = new PackLineBulkUpdateDataSource(Factory);
			AssertEquals("The list of available inspection types is empty when there are no OuterPackLines available.", 0, testObj.InspectionTypesForOuterPackLinesBulkSetterList.Count);
			var shipment = GetNewShipment();
			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			testObj.AddAllOuterPackLines(shipment);
			AssertContainsExactElementsInAnyOrder("The list of available inspection types matches the first OuterPackLine's inspection type list",
				packline1.InspectionTypes, testObj.InspectionTypesForOuterPackLinesBulkSetterList);
		}

		public void TestPackLinesInspectionTypeBulkDropEdit_ShouldValidateEntryMatchToList()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "DEFRA";
			shipment1.JS_RL_NKDestination = "USCHI";
			shipment1.OuterPackLines.AddNew();

			Factory.Save();

			var shipments = new ForwardingShipment[] { shipment1 };
			var dataSource = new PackLineBulkUpdateDataSource(Factory);
			dataSource.AddAllOuterPackLines(shipments);

			dataSource.OuterPackLinesInspectionTypeBulkSetter = "AAA";
			AssertHasError(dataSource.OuterPackLinesInspectionTypeBulkSetterInfo, "Enter a valid Inspection Status.");

			dataSource.OuterPackLinesInspectionTypeBulkSetter = "UNK";
			AssertNoErrors(dataSource.OuterPackLinesInspectionTypeBulkSetterInfo);
		}

		ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_InspectionTypeCode = "UNK";
			return shipment;
		}
	}
}
