using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentModuleButtonGrid
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (InnerGrid != null)
				{
					InnerGrid.MouseUp -= Grid_KeyUpOrMouseUp;
					InnerGrid.KeyUp -= Grid_KeyUpOrMouseUp;
				}

				this.OnAttach -= ShipmentModuleButtonGrid_OnAttachOrDetach;
				this.Detached -= ShipmentModuleButtonGrid_OnAttachOrDetach;
				this.Layout -= this.ShipmentModuleButtonGrid_Layout;
				this.Resize -= this.ShipmentModuleButtonGrid_Resize;
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.totalsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.totalsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.totalsValuesLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.totalsPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.TopLevelForwardingShipmentCollection);
			//
			// totalsPanel
			//
			this.totalsPanel.Controls.Add(this.totalsValuesLabel);
			this.totalsPanel.Controls.Add(this.totalsLabel);
			this.totalsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.totalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.totalsPanel.Name = "totalsPanel";
			this.totalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 25, true);
			this.totalsPanel.TabIndex = 0;
			//
			// totalsLabel
			//
			this.totalsLabel.Text = Enterprise.Freight.Forwarding.GUI.Res.GetString("c5db70c2-440c-47f2-9025-2b880c884d16", "Pack Line Totals");
			this.totalsLabel.Name = "totalsLabel";
			this.totalsLabel.Dock = DockStyle.Left;
			this.totalsLabel.Font = new System.Drawing.Font(this.totalsValuesLabel.Font, System.Drawing.FontStyle.Bold);
			this.totalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 25, true);
			//
			// totalsValuesLabel
			//
			this.totalsValuesLabel.Text = "";
			this.totalsValuesLabel.Name = "totalsValuesLabel";
			this.totalsValuesLabel.Dock = DockStyle.Left;
			this.totalsValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 25, true);
			//
			// ShipmentModuleButtonGrid
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.totalsPanel);
			this.Controls.SetChildIndex(this.totalsPanel, 0);
			this.Name = "ShipmentModuleButtonGrid";
			this.totalsPanel.ResumeLayout(false);
			this.totalsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void ShipmentModuleButtonGrid_Layout(object sender, LayoutEventArgs e)
		{
			this.totalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, this.toolStrip.Location.Y, false);
			var newWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.toolStrip.Left) - 5;
			this.totalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(newWidth, 25, true);
		}

		void ShipmentModuleButtonGrid_Resize(object sender, EventArgs e)
		{
			var newWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.toolStrip.Left) - 5;
			this.totalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(newWidth, 25, true);
		}

		Enterprise.ZArchitecture.GUI.ZPanel totalsPanel;
		Enterprise.ZArchitecture.ZLabel totalsLabel;
		Enterprise.ZArchitecture.ZLabel totalsValuesLabel;

		#endregion
	}
}
