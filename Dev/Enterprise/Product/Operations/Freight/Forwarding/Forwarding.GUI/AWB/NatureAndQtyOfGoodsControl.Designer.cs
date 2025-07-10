namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.natureAndQtyOfGoodsDetails = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// natureAndQtyOfGoodsDetails
			// 
			this.natureAndQtyOfGoodsDetails.AllowDrop = true;
			this.natureAndQtyOfGoodsDetails.CaptionResourceString = null;
			this.natureAndQtyOfGoodsDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.natureAndQtyOfGoodsDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.natureAndQtyOfGoodsDetails.Name = "natureAndQtyOfGoodsDetails";
			this.natureAndQtyOfGoodsDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.natureAndQtyOfGoodsDetails.TabIndex = 3;
			this.natureAndQtyOfGoodsDetails.UserControlType = typeof(Enterprise.Freight.Forwarding.GUI.AWB.NatureAndQtyOfGoodsTextControl);
			// 
			// NatureAndQtyOfGoodsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.natureAndQtyOfGoodsDetails);
			this.Name = "NatureAndQtyOfGoodsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl natureAndQtyOfGoodsDetails;
	}
}