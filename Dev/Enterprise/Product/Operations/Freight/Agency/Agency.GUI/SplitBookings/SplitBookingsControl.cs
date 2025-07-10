
namespace Enterprise.Freight.Agency.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using CargoWise.Windows.UI;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture.GUI;

	internal partial class SplitBookingsControl : ZUserControl
	{
		public SplitBookingsControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var header = CurrentDataItem as SplitBookingsHeader;
			if (header != null)
			{
				InitializeGrids(header);
			}
		}

		void InitializeGrids(SplitBookingsHeader header)
		{
			var grids = new List<SplitGrid>();

			if (header.ShowContainers)
			{
				grids.Add(new BookedContainersSplitGrid());
			}

			if (header.ShowActualContainers)
			{
				grids.Add(new ActualContainersSplitGrid());
			}

			if (header.ShowVehicles)
			{
				grids.Add(new VehiclesSplitGrid());
			}

			if (header.ShowTopLevelPacks)
			{
				grids.Add(new TopLevelPacksSplitGrid());
			}

			if (header.ShowPackLines)
			{
				grids.Add(new PacksSplitGrid());
			}

			foreach (var grid in grids)
			{
				BindingSource.SetBindingMember(grid, ".");
			}

			Render(grids);
		}

		void Render(IReadOnlyList<SplitGrid> grids)
		{
			this.gridsPlaceHolder.Controls.Clear();

			if (grids.Count > 0)
			{
				var gridHeight = gridsPlaceHolder.Height / grids.Count;
				var container = (Control)gridsPlaceHolder;

				for (var i = 0; i < grids.Count; i++)
				{
					grids[i].Dock = DockStyle.Fill;

					if (i < grids.Count - 1)
					{
						var splitter = new KSplitContainer();
						container.Controls.Add(splitter);
						container = splitter.Panel2;

						splitter.Dock = DockStyle.Fill;
						splitter.Orientation = Orientation.Horizontal;
						splitter.SplitterDistance = gridHeight;
						splitter.Panel1.Controls.Add(grids[i]);
					}
					else
					{
						container.Controls.Add(grids[i]);
					}
				}
			}
		}
	}
}
