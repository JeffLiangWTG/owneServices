using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PackLineInspectionTypeBulkUpdateForm))]
	public class PackLineInspectionTypeBulkUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PackLineInspectionTypeBulkUpdateForm(new PackLineBulkUpdateDataSource(Factory));
		}

		public void TestPacklineInspectionStatusDropdown()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USCHI";
				var packline1 = shipment1.OuterPackLines.AddNew();
				packline1.JL_InspectionTypeCode = "XRY";
				var packline2 = shipment1.OuterPackLines.AddNew();
				packline2.JL_InspectionTypeCode = "CMD";
				var packline5 = shipment1.OuterPackLines.AddNew();

				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_RL_NKOrigin = "JMKIN";
				shipment2.JS_RL_NKDestination = "JMAPP";
				shipment2.JS_InspectionTypeCode = "APP";
				var packline3 = shipment2.OuterPackLines.AddNew();

				var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_RL_NKOrigin = "DEFRA";
				shipment3.JS_RL_NKDestination = "USCHI";
				var packline4 = shipment3.OuterPackLines.AddNew();
				packline4.JL_InspectionTypeCode = "XRY";

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("Precondition: shipment1's inspection type is not read only", false, shipment1.JS_InspectionTypeCodeInfo.ReadOnly);
					AssertEquals("Precondition: packline1's inspection type is not read only", false, packline1.JL_InspectionTypeCodeInfo.ReadOnly);
					AssertEquals("Precondition: shipment2's inspection type is 'APP'", "APP", shipment2.JS_InspectionTypeCode);
					AssertEquals("Precondition: packline3's inspection type is read only since the shipment's inspection type is APP", true, packline3.JL_InspectionTypeCodeInfo.ReadOnly);
					AssertEquals("Precondition: packline3's inspection type is an empty string", ZString.Empty, packline3.JL_InspectionTypeCode);
					AssertEquals("Precondition: packline4's inspection type is not read only", false, packline4.JL_InspectionTypeCodeInfo.ReadOnly);
				});

				var shipments = new ForwardingShipment[] { shipment1, shipment2 };
				using (var form = CreateTestForm(shipments))
				{
					form.Show();
					var bulkSetterDropDown = form.Controls.Find("InspectionBulkSetDropEdit", true)[0] as ZDropEdit;
					AssertNotNull("A bulk setter for inspection types is available on the form", bulkSetterDropDown);
					AssertEquals("The bulk setter is visible", true, bulkSetterDropDown.Visible);
					AssertEquals("Show the inspection code in the drop down", true, bulkSetterDropDown.ShowCodeInDropDown);

					bulkSetterDropDown.SelectItem("CMD");
					bulkSetterDropDown.CommitBoundValue();
					CombineAssertions(() =>
					{
						AssertEquals("packline1's InspectionType was updated.", "CMD", packline1.JL_InspectionTypeCode);
						AssertEquals("packline2's InspectionType was updated.", "CMD", packline2.JL_InspectionTypeCode);
						AssertEquals("packline5's InspectionType was updated.", "CMD", packline5.JL_InspectionTypeCode);

						AssertEquals("packline3's InspectionType was not updated since the it should be read only based on the APP status of the shipment.", ZString.Empty, packline3.JL_InspectionTypeCode);

						AssertEquals("packline4's InspectionType was not updated since it was not included on the form.", "XRY", packline4.JL_InspectionTypeCode);
					});
				}
			}
		}

		public void TestGridColumns()
		{
			Action<ZGrid, string, bool, string> assertColumnSetup = (grid, columnBinding, isReadOnly, headerText) =>
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, grid.Columns.Contains(columnBinding));
					var col = grid.Columns[columnBinding];
					AssertEquals(isReadOnly, col.ColumnStyle.ReadOnly);
					AssertEquals(headerText, col.ColumnStyle.HeaderText);
					AssertEquals(true, col.IsVisible);
				});
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USCHI";
				var packline1 = shipment1.OuterPackLines.AddNew();

				Factory.Save();

				var shipments = new ForwardingShipment[] { shipment1 };
				using (var form = CreateTestForm(shipments))
				{
					form.Show();
					var grid = form.Controls.Find("PackLineGrid", true)[0] as ZGrid;
					AssertNotNull("A grid exists on the form for displaying the pack lines.", grid);
					AssertEquals("The grid is visible", true, grid.Visible);

					assertColumnSetup(grid, "ShipmentId", true, "Shipment ID");
					assertColumnSetup(grid, "PackLineId", true, "Packline ID");
					assertColumnSetup(grid, "ShipmentInspectionType", true, "Shipment Inspection");
					assertColumnSetup(grid, "PackLineInspectionType", false, "Packline Inspection");
					assertColumnSetup(grid, "PackageCount", true, "Packs");
					assertColumnSetup(grid, "ActualVolume", true, "Volume");
					assertColumnSetup(grid, "ActualVolumeUQ", true, "UV");
					assertColumnSetup(grid, "ActualWeight", true, "Weight");
					assertColumnSetup(grid, "ActualWeightUQ", true, "UW");
					assertColumnSetup(grid, "PackageType", true, "Package Type");
				}
			}
		}

		public void TestBulkSetDescription()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "DEFRA";
			shipment1.JS_RL_NKDestination = "USCHI";
			shipment1.OuterPackLines.AddNew();

			Factory.Save();

			var shipments = new ForwardingShipment[] { shipment1 };
			using (var form = CreateTestForm(shipments))
			{
				form.Show();
				var description = form.Controls.Find("statusBulkSetDescription", true)[0] as ZLabel;
				AssertEquals("Setting the status will update all of the following selected packlines, which you can manually override as required:", description.Text);
			}
		}

		PackLineInspectionTypeBulkUpdateForm CreateTestForm(ForwardingShipment[] shipments)
		{
			var dataSource = new PackLineBulkUpdateDataSource(Factory);
			dataSource.AddAllOuterPackLines(shipments);
			return new PackLineInspectionTypeBulkUpdateForm(dataSource);
		}
	}
}
