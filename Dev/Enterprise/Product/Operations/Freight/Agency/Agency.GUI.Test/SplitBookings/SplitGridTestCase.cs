using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class SplitGridTestCase : TestCaseWithFactory
	{
		public void TestDoubleClick()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var child1 = booking.Vehicles.AddNew();
			var child2 = booking.Vehicles.AddNew();
			var child3 = booking.Vehicles.AddNew();

			var splitBookingHeader = SplitBookingsHeader.New(booking);
			var child4 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child5 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child6 = splitBookingHeader.NewShipment.Vehicles.AddNew();

			using (ZForm form = new ZForm(splitBookingHeader))
			{
				AgencyShipmentContainer handlerItem = null;
				SplitBookingsHeader.MoveDirection? handlerMovementDirection = null;

				void Handler(object item, SplitBookingsHeader.MoveDirection direction)
				{
					handlerItem = (AgencyShipmentContainer)item;
					handlerMovementDirection = direction;
				}

				SplitGridForTest split = new SplitGridForTest();
				split.Dock = DockStyle.Fill;
				split.Performer = Handler;
				form.Size = new Size(600, 300);
				form.Controls.Add(split);
				form.Show();
				Application.DoEvents();

				DoubleClick(split, split.Grid1, 1, 0);
				AssertEquals("Invoke.Item", child2, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToNew, handlerMovementDirection);

				handlerItem = null;
				handlerMovementDirection = null;

				DoubleClick(split, split.Grid2, 2, 0);
				AssertEquals("Invoke.Item", child6, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToOriginal, handlerMovementDirection);
			}

			Assert(true);
		}

		public void TestClickMoveButtons()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var child1 = booking.Vehicles.AddNew();
			var child2 = booking.Vehicles.AddNew();
			var child3 = booking.Vehicles.AddNew();

			var splitBookingHeader = SplitBookingsHeader.New(booking);
			var child4 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child5 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child6 = splitBookingHeader.NewShipment.Vehicles.AddNew();

			using (ZForm form = new ZForm(splitBookingHeader))
			{
				AgencyShipmentContainer handlerItem = null;
				SplitBookingsHeader.MoveDirection? handlerMovementDirection = null;

				void Handler(object item, SplitBookingsHeader.MoveDirection direction)
				{
					handlerItem = (AgencyShipmentContainer)item;
					handlerMovementDirection = direction;
				}

				SplitGridForTest split = new SplitGridForTest();
				split.Dock = DockStyle.Fill;
				split.Performer = Handler;
				form.Controls.Add(split);
				form.Show();
				Application.DoEvents();
				Button moveRightButton = (Button)split.Controls.Find("moveRightButton", true)[0];
				Button moveLeftButton = (Button)split.Controls.Find("moveLeftButton", true)[0];

				moveRightButton.PerformClick();
				AssertEquals("Invoke.Item", child1, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToNew, handlerMovementDirection);

				handlerItem = null;
				handlerMovementDirection = null;

				moveLeftButton.PerformClick();
				AssertEquals("Invoke.Item", child4, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToOriginal, handlerMovementDirection);
			}
		}

		public void TestPerformMove()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var child1 = booking.Vehicles.AddNew();
			var child2 = booking.Vehicles.AddNew();
			var child3 = booking.Vehicles.AddNew();

			var splitBookingHeader = SplitBookingsHeader.New(booking);
			var child4 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child5 = splitBookingHeader.NewShipment.Vehicles.AddNew();
			var child6 = splitBookingHeader.NewShipment.Vehicles.AddNew();

			using (ZForm form = new ZForm(splitBookingHeader))
			{
				AgencyShipmentContainer handlerItem = null;
				SplitBookingsHeader.MoveDirection? handlerMovementDirection = null;

				void Handler(object item, SplitBookingsHeader.MoveDirection direction)
				{
					handlerItem = (AgencyShipmentContainer)item;
					handlerMovementDirection = direction;
				}

				SplitGridForTest split = new SplitGridForTest();
				split.Dock = DockStyle.Fill;
				split.Performer = Handler;
				form.Controls.Add(split);
				form.Show();
				Application.DoEvents();

				split.PerformMoveRight();
				AssertEquals("Invoke.Item", child1, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToNew, handlerMovementDirection);

				handlerItem = null;
				handlerMovementDirection = null;

				split.PerformMoveLeft();
				AssertEquals("Invoke.Item", child4, handlerItem);
				AssertEquals("Invoke.MoveDirection", SplitBookingsHeader.MoveDirection.ToOriginal, handlerMovementDirection);
			}
		}

		#region SplitGridForTest

		public delegate void PerformHandler(object item, SplitBookingsHeader.MoveDirection direction);
		class SplitGridForTest : SplitGrid
		{
			protected override string CollectionName
			{
				get
				{
					return AgencyBooking.Schema.Vehicles;
				}
			}

			protected override string[] Columns
			{
				get
				{
					return new string[] { AgencyShipmentContainer.Schema.JC_ContainerCount };
				}
			}

			protected override string ItemsName
			{
				get
				{
					return "McLaren";
				}
			}

			protected override ZGridColumnInfo GetColumnControl(string columnName)
			{
				if (columnName == AgencyShipmentContainer.Schema.JC_ContainerCount)
				{
					var control = new ZTextBoxColumnStyleInfo();
					control.Caption = "McLaren";
					return control;
				}
				else
				{
					return base.GetColumnControl(columnName);
				}
			}

			public PerformHandler Performer { get; set; }

			public ZGrid Grid1
			{
				[System.Diagnostics.DebuggerStepThrough]
				get
				{
					return grid1;
				}
			}

			public ZGrid Grid2
			{
				[System.Diagnostics.DebuggerStepThrough]
				get
				{
					return grid2;
				}
			}

			protected override void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
			{
				Performer(item, direction);
			}

			public int? RowToClick { get; set; }
			public int? ColumnToClick { get; set; }

			protected override void MoveAtPoint(ZGrid fromGrid, ZGrid toGrid, int x, int y, SplitBookingsHeader.MoveDirection direction)
			{
				if (RowToClick != null
					&& ColumnToClick != null)
				{
					var clickLocation = fromGrid.GetCellBounds(RowToClick.Value, ColumnToClick.Value).Location;
					base.MoveAtPoint(fromGrid, toGrid, clickLocation.X, clickLocation.Y, direction);
				}
				else
				{
					base.MoveAtPoint(fromGrid, toGrid, x, y, direction);
				}
			}
		}

		#endregion

		#region Implementation

		static void DoubleClick(SplitGridForTest split, ZGrid grid, int row, int column)
		{
			var clickLocation = grid.GetCellBounds(row, column).Location;

			// click location will be adjusted before it reaches MoveAtPoint hence need to adjust coordinates
			split.RowToClick = row;
			split.ColumnToClick = column;

			var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 2, clickLocation.X, clickLocation.Y, 0);
			grid.PerformMouseDownForTest(mouseEventArgs, row, column);

			var hitTest = grid.HitTest(clickLocation.X, clickLocation.Y);
			AssertEquals("prerequisite; calculated row location is correct", row, hitTest.Row);
			AssertEquals("prerequisite; calculated column location is correct", column, hitTest.Column);
			AssertEquals("prerequisite; clicked inside the cell", DataGrid.HitTestType.Cell, hitTest.Type);
		}

		#endregion
	}
}
