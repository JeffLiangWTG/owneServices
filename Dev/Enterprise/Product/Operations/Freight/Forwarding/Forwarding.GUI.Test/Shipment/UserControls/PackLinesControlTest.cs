using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class PackLinesControlTest : TestCaseWithFactory
	{
		public void TestPackLinesBoundToGrid()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			using (var form = new TestForm(shipment))
			{
				form.Show();

				AssertEquals(shipment, form.PackLinesContol.CurrentDataItem);
				AssertEquals(shipment, form.PackLinesContol.Grid.DataSource);
				AssertContainsExactElementsInAnyOrder(new ForwardingPackLine[] { packLine1, packLine2 }, form.PackLinesContol.Grid.ListManager.List);
			}
		}

		public void TestExportReferenceNumber_ChangesWithShipmentOrigin_China()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				CheckExportReferenceNumberHeader();
			}
		}

		public void TestExportReferenceNumber_ChangesWithShipmentOrigin_HongKong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				CheckExportReferenceNumberHeader();
			}
		}

		public void TestExportReferenceNumber_ChangesWithShipmentOrigin_Taiwan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				CheckExportReferenceNumberHeader();
			}
		}

		void CheckExportReferenceNumberHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "CN123";

			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			using (var form = new TestForm(shipment))
			{
				form.Show();

				var exportRefNumberCol = form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber];
				AssertEquals("Shipping Order/Shi Lian Dan", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "AU123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "HK123";
				AssertEquals("Shipping Order/Shi Lian Dan", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "NZ123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "TW123";
				AssertEquals("Shipping Order/Shi Lian Dan", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "CN123";
			using (var form = new TestForm(shipment))
			{
				form.Show();

				var exportRefNumberCol = form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber];
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "AU123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "HK123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "NZ123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_RL_NKOrigin = "TW123";
				AssertEquals("Export Reference Number", exportRefNumberCol.ColumnStyle.HeaderText);

				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Shipping Order/Shi Lian Dan", exportRefNumberCol.ColumnStyle.HeaderText);
			}
		}

		public void TestExportReferenceNumber_DoesNotChangeWithWrongCompanyCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CN123";

				shipment.OuterPackLines.AddNew();
				shipment.OuterPackLines.AddNew();

				using (var form = new TestForm(shipment))
				{
					form.Show();

					Assert(form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText.StartsWith("Export Ref"));

					shipment.JS_RL_NKOrigin = "AU123";
					Assert(form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText.StartsWith("Export Ref"));

					shipment.JS_RL_NKOrigin = "HK123";
					Assert(form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText.StartsWith("Export Ref"));

					shipment.JS_RL_NKOrigin = "NZ123";
					Assert(form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText.StartsWith("Export Ref"));

					shipment.JS_RL_NKOrigin = "TW123";
					Assert(form.PackLinesContol.Grid.Columns[PackLine.Schema.JL_ExportRefNumber].ColumnStyle.HeaderText.StartsWith("Export Ref"));
				}
			}
		}

		public void TestColumnsBoundToGrid()
		{
			using (var packLinesControl = new PackLinesContol())
			{
				Assert("Grid should include JL_ExportRefNumber column", ColumnExists(packLinesControl.Grid, "JL_ExportRefNumber"));
				Assert("Grid should include JL_ImportRefNumber column", ColumnExists(packLinesControl.Grid, "JL_ImportRefNumber"));
				Assert("Grid should include JL_VehicleMake column", ColumnExists(packLinesControl.Grid, "JL_VehicleMake"));
				Assert("Grid should include JL_VehicleModel column", ColumnExists(packLinesControl.Grid, "JL_VehicleModel"));
				Assert("Grid should include JL_VehicleYear column", ColumnExists(packLinesControl.Grid, "JL_VehicleYear"));
				Assert("Grid should include JL_VehicleColor column", ColumnExists(packLinesControl.Grid, "JL_VehicleColor"));
				Assert("Grid should include JL_VehicleNumberOfDoors column", ColumnExists(packLinesControl.Grid, "JL_VehicleNumberOfDoors"));
				Assert("Grid should include JL_VehicleTransmission column", ColumnExists(packLinesControl.Grid, "JL_VehicleTransmission"));
				Assert("Grid should include JL_InspectionTypeCode column", ColumnExists(packLinesControl.Grid, "JL_InspectionTypeCode"));
				Assert("Grid should include Shipment+JS_UniqueConsignRef column", ColumnExists(packLinesControl.Grid, "Shipment+JS_UniqueConsignRef"));
				Assert("Grid should include JL_OriginTransitWarehouseStatus column", ColumnExists(packLinesControl.Grid, "JL_OriginTransitWarehouseStatus"));
				Assert("Grid should include JL_IsHighRisk column", ColumnExists(packLinesControl.Grid, "JL_IsHighRisk"));
				Assert("Grid should include JL_AdditionalInspectionTypeCode column", ColumnExists(packLinesControl.Grid, "JL_AdditionalInspectionTypeCode"));
			}
		}

		public void TestJL_MarksAndNumbersColumn()
		{
			using (var packLinesControl = new PackLinesContol())
			{
				var marksAndNumbersColumnStyle = packLinesControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().First(columnStyle => columnStyle.ColumnName == "JL_MarksAndNumbers");
				AssertNotNull("Grid should include JL_MarksAndNumbers column", marksAndNumbersColumnStyle);
				Assert("JL_MarksAndNumbers column should be invisible as default", !marksAndNumbersColumnStyle.IsVisible);
			}
		}

		public void TestShowBookingLineColumnWhenEnableAdvOrmFeatureIsTrue()
		{
			TestBookingLineColumn(true);
		}

		public void TestRemoveBookingLineColumnWhenEnableAdvOrmFeatureIsFalse()
		{
			TestBookingLineColumn(false);
		}

		void TestBookingLineColumn(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				using (var packLinesControl = new PackLinesContol())
				{
					AssertEquals(enableAdvOrmFeature, packLinesControl.Grid.ColumnStyles.OfType<ZGridColumnInfo>().Any(column => column.ColumnName == nameof(JobPackLinesSchema.JL_JSL_BookingLine)));
				}
			});
		}

		#region Implmentation

		bool ColumnExists(ZGrid grid, string columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnName);
		}

		#endregion

		class TestForm : ZForm
		{
			public TestForm(CommonShipment shipment)
				: base(shipment)
			{
				Init();
			}

			public PackLinesContol PackLinesContol
			{
				get;
				set;
			}

			void Init()
			{
				PackLinesContol = new PackLinesContol();
				Controls.Add(PackLinesContol);
			}
		}
	}
}
