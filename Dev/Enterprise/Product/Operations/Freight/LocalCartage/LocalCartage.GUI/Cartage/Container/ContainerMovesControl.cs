using System;
using System.Windows.Forms;
using Enterprise.Freight.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class ContainerMovesControl : ZUserControl
	{
		public ContainerMovesControl()
		{
			InitializeComponent();

			ContainerCartageLegsGrid.ColorContextKey = LegGridColourScheme.LegColourKey;
			ContainerCartageLegsGrid.ShareActiveColorScheme = false;

			new UNDGDataItemFormManager(ContainerCartageLegsGrid, "BookedCtgMove").Initialize();
			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(ContainerCartageLegsGrid);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			Unhook();

			base.SetDataBinding(dataSource, dataMember);

			Hook();
		}

		void Hook()
		{
			if (Cartage != null)
			{
				workflowCustomFields = WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(ContainerCartageLegsGrid, Cartage.CartageLegs, true);
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

		void ContainerCartageLegsGrid_MouseDown(object sender, MouseEventArgs e)
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
			ContainerCartageLegsGrid.SelectSingleElement(leg);
		}
	}
}
