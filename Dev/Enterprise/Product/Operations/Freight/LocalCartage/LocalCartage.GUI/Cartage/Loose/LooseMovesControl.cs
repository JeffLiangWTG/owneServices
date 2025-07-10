using System;
using System.Windows.Forms;
using Enterprise.Freight.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class LooseMovesControl : ZUserControl
	{
		public LooseMovesControl()
		{
			InitializeComponent();

			LooseCartageLegsGrid.ColorContextKey = LegGridColourScheme.LegColourKey;
			LooseCartageLegsGrid.ShareActiveColorScheme = false;

			SetUpContextMenus();
			new UNDGDataItemFormManager(LooseCartageLegsGrid, "BookedCtgMove").Initialize();
			new UNDGDataItemFormManager(LooseDeliveryGrid).Initialize();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember parameter not supported", nameof(dataMember));
			}

			Unhook();

			CommonCartage cartage = dataSource != null ? (CommonCartage)dataSource : null;
			base.SetDataBinding(cartage, "");

			Hook();
		}

		void Hook()
		{
			if (Cartage != null)
			{
				workflowCustomFields = WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(LooseCartageLegsGrid, Cartage.CartageLegs, true);
			}
		}

		void Unhook()
		{
			if (workflowCustomFields != null)
			{
				workflowCustomFields.Dispose();
				workflowCustomFields = null;
			}
		}

		IDisposable workflowCustomFields;

		void SetUpContextMenus()
		{
			var splitMenu = new ZMenuItem(ResString.GetMultilingualString("LocalTransport.Context.SplitLooseBooking", "Split Loose Booking"));
			splitMenu.Click += new EventHandler(Split_Click);
			LooseDeliveryGrid.ContextMenu.MenuItems.Add(0, splitMenu);
			var groupMenu = new ZMenuItem(ResString.GetMultilingualString("LocalTransport.Context.GroupLooseBookings", "Group Loose Bookings"));
			groupMenu.Click += new EventHandler(Group_Click);
			LooseDeliveryGrid.ContextMenu.MenuItems.Add(1, groupMenu);
			LooseDeliveryGrid.ContextMenu.MenuItems.Add(2, new ZMenuItem("-"));

			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(LooseDeliveryGrid);
			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(LooseCartageLegsGrid);
		}

		void Split_Click(object sender, EventArgs e)
		{
			if (LooseDeliveryGrid.SelectedElements.Length > 0)
			{
				CommonBookedCtgMove selectedMove = (CommonBookedCtgMove)LooseDeliveryGrid.SelectedElements[0];
				if (selectedMove.EW_BookedPackCount > 1)
				{
					LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(selectedMove);
					using (var splitForm = new LooseBookedMoveSplitForm(master))
					{
						splitForm.ShowDialog(this);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("2eb17629-16f8-48b1-88e6-8824f37f5ede", "The selected Loose Booked Move requires more than 1 package to use the split function."), Res.GetString("457c5dc8-d894-4e2f-95f6-5a7323f188ae", "Split Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		void Group_Click(object sender, EventArgs e)
		{
			if (LooseDeliveryGrid.SelectedElements.Length <= 1)
			{
				Globals.Message.Show(Res.GetString("d60a8727-bce3-466c-8b8f-5458c81e4111", "More than 1 row is required to be selected to use the group function."), Res.GetString("f3a4ef25-afa2-4413-a492-ee5bde153569", "Group Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				LooseBookedMoveSplitMaster.Group(LooseDeliveryGrid.GetSelectedElements<CommonBookedCtgMove>());
			}
		}

		void LooseCartageLegsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				ZGrid legsGrid = sender as ZGrid;
				if (legsGrid.HitTest(e.X, e.Y).Row > -1)
				{
					HandleDoubleClick(legsGrid);
				}
			}
		}

		void HandleDoubleClick(ZGrid legsGrid)
		{
			bool hasCurrent = (legsGrid != null && legsGrid.ListManager != null && legsGrid.ListManager.Position >= 0);
			CommonCartageLeg currentSelection = hasCurrent ? legsGrid.ListManager.GetCurrent() as CommonCartageLeg : null;

			if (currentSelection != null)
			{
				ShowCartageLeg(currentSelection);
			}
		}

		internal IZForm ShowCartageLeg(CommonCartageLeg leg)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.CartageLeg);

			return controller.ShowEditForm(leg);
		}

		public void SelectCartageLeg(CommonCartageLeg leg)
		{
			if (LooseDeliveryGrid.List != null)
			{
				LooseDeliveryGrid.SelectSingleElement(leg.BookedCtgMove);
			}

			if (LooseCartageLegsGrid.List != null)
			{
				LooseCartageLegsGrid.SelectSingleElement(leg);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Unhook();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)DataSource; }
		}
	}
}
