using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	public class BookingContainersAndPackLinesControlTest : TestCaseWithFactory
	{
		public void TestZTextBoxColumnWithInvalidEmptyValue()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var forwardingPackLine = booking.OuterPackLines.AddNew();
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var errorMessageBuilder = new StringBuilder();

			using (var form = new ZForm(quotedBooking))
			using (var forwarderAdditionalContactsControl = new BookingContainersAndPackLinesControl())
			{
				form.Controls.Add(forwarderAdditionalContactsControl);
				var grid = form.FindSingleOrDefault<ZGrid>(o => o.Name == "LooseCargoGrid");
				grid.SetAllColumnsVisible(true);
				form.Load += (sender, e) =>
				{
					for (var i = 0; i < grid.Columns.Count; i++)
					{
						if (grid.Columns[i].ColumnStyle is ZTextBoxColumnStyle style && style.TextBox is DataGridTextBox dataGridTextBox)
						{
							try
							{
								var cell1 = new DataGridCell(0, i);
								var columnBeside = i == 0 ? i + 1 : i - 1;
								var cell2 = new DataGridCell(0, columnBeside);

								grid.CurrentCell = cell1;

								dataGridTextBox.Text = string.Empty;
								dataGridTextBox.IsInEditOrNavigateMode = false;

								grid.CurrentCell = cell2;
							}
							catch (Exception ex)
							{
								errorMessageBuilder.AppendLine($"{grid.Columns[i].ColumnName} Column with ZTextBoxColumnStyle may cause the following exception. Please consider using a proper type of ColumnStyle instead : {ex}");
							}
						}
					}
				};
				form.Show();
				AssertNullOrEmpty(errorMessageBuilder.ToString());
			}
		}

		public void TestLooseCargoGrids_WhenUnderDifferentContainerModes_ShouldDisplayColumnsCorrectly()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var forwardingPackLine = booking.OuterPackLines.AddNew();
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
			quotedBooking.ContainerMode = Core.Constants.ContainerModes.FCL;

			using (var form = new ZForm(quotedBooking))
			using (var forwarderAdditionalContactsControl = new BookingContainersAndPackLinesControl())
			{
				form.Controls.Add(forwarderAdditionalContactsControl);
				var grid = form.FindSingleOrDefault<ZGrid>(o => o.Name == "LooseCargoGrid");

				form.Load += (sender, e) =>
				{
					var vehicleColumns = new[]
					{
						AutoJobPackLines.Schema.JL_VehicleMake,
						AutoJobPackLines.Schema.JL_VehicleModel,
						AutoJobPackLines.Schema.JL_VehicleYear,
						AutoJobPackLines.Schema.JL_VehicleColor,
						AutoJobPackLines.Schema.JL_VehicleNumberOfDoors,
						AutoJobPackLines.Schema.JL_VehicleTransmission,
					};
					foreach (var col in vehicleColumns)
					{
						AssertEquals($"Vehicle column '{col}' should NOT be visible when ContainerMode is NOT RollOnRollOff.", false, grid.Columns.Contains(col));
					}
					Assert("Reference number column should always be visible.", grid.Columns.Contains(AutoJobPackLines.Schema.JL_RefNumber));

					quotedBooking.ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;

					foreach (var col in vehicleColumns)
					{
						Assert($"Vehicle column '{col}' should be visible when ContainerMode is RollOnRollOff.", grid.Columns.Contains(col));
					}
					Assert("Reference number column should always be visible.", grid.Columns.Contains(AutoJobPackLines.Schema.JL_RefNumber));
				};

				form.Show();
			}
		}
	}
}
