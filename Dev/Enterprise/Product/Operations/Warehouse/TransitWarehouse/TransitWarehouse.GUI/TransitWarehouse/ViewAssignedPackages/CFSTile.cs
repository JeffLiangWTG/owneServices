using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class CFSTile : ZUserControl
	{
		public CFSTile()
		{
			InitializeComponent();
		}

		public CFSTile(ViewPackagesCFSInfo details) : this()
		{
			Details = details;
			this.WarehouseName.Text = details.WarehouseName;

			if (Details.TransitWarehouse != null)
			{
				this.Status.Text = details.WarehouseStatus;
				this.Status.BackColor = WarehouseStatusColor();
				this.Status.ForeColor = this.Status.BackColor != Color.Transparent ? Color.White : Color.Black;
				this.NumberOfPackages.Text = details.NumberOfPackagesDisplayText;
				this.Weight.Text = details.TotalWeightDisplayText;  // To show totals on view packages form warehosue tiles;
				this.Volume.Text = details.TotalVolumeDisplayText;  // To show totals on view packages form warehosue tiles;

				WireAllControls(this);
			}
			else
			{
				this.Enabled = false;
				this.zPanel1.Enabled = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (this.WarehouseAndStatusPanel.Width > QtyPanel.Width)
			{
				CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(QtyPanel, this.WarehouseAndStatusPanel.Width, false);
			}
			else
			{
				CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(this.WarehouseAndStatusPanel, this.QtyPanel.Width, false);
			}

			var numberOfPixelsToLeftOfWarehouseName = (this.WarehouseAndStatusPanel.Width - WarehouseName.Size.Width) / 2;
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetLeft(WarehouseName, numberOfPixelsToLeftOfWarehouseName, false);

			var halfControlSize = WarehouseAndStatusPanel.Size.Width / 2;
			SetLocationOfCaptionAndValueLabels(PackagesCaption, NumberOfPackages, halfControlSize);
			SetLocationOfCaptionAndValueLabels(WeightCaption, Weight, halfControlSize);
			SetLocationOfCaptionAndValueLabels(VolumeCaption, Volume, halfControlSize);
		}

		void SetLocationOfCaptionAndValueLabels(ZLabel captionLabel, ZLabel valueLabel, int halfControlSize)
		{
			var xCordinateOfCaption = halfControlSize - captionLabel.Size.Width - 1;
			var xCordinateOfValue = halfControlSize + 1;
			captionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(xCordinateOfCaption, captionLabel.Location.Y, false);
			valueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(xCordinateOfValue, valueLabel.Location.Y, false);
		}

		Color WarehouseStatusColor()
		{
			if (Details.WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Receiving || Details.WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatching)
			{
				return TransitWarehouseStatusColors.Red;
			}
			else if (Details.WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Dispatched || Details.WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Booked)
			{
				return TransitWarehouseStatusColors.Blue;
			}
			else if (Details.WarehouseStatus == TransitWarehouseJobsStatus.Descriptions.Received)
			{
				return TransitWarehouseStatusColors.Green;
			}
			return Color.Transparent;
		}

		#region Event Handlers

		void WireAllControls(Control cont)
		{
			foreach (Control ctl in cont.Controls)
			{
				ctl.Click += (s, e) => OnClick(e);
				ctl.MouseHover += (s, e) => OnMouseHover(e);
				ctl.MouseEnter += (s, e) => OnMouseEnter(e);
				ctl.MouseLeave += (s, e) => OnMouseLeave(e);
				if (ctl.HasChildren)
				{
					WireAllControls(ctl);
				}
			}
		}

		#endregion

		ViewPackagesCFSInfo Details { get; }
	}
}
