using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CompanyTariffOrCostBasedCalculatorControl
	{
		private CompanyTariffOrCostBasedCalculatorPanel1 Panel1;
		private KToolStrip ToolStrip;
		private ZToolStripButton ByWeightBreakButton;
		private CompanyTariffOrCostBasedCalculatorPanel2 Panel2;
		private Container components = null;

		private void InitializeComponent()
		{
			ComponentResourceManager resources = new ComponentResourceManager(typeof(CompanyTariffOrCostBasedCalculatorControl));
			this.Panel1 = new CompanyTariffOrCostBasedCalculatorPanel1();
			this.ToolStrip = new ZToolStrip();
			this.ByWeightBreakButton = new ZToolStripButton();
			this.Panel2 = new CompanyTariffOrCostBasedCalculatorPanel2();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Calculator);
			// 
			// Panel1
			// 
			this.Panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.Panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 0, true);
			this.Panel1.Name = "Panel1";
			this.Panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 160, true);
			this.Panel1.TabIndex = 0;
			// 
			// ToolStrip
			// 
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ByWeightBreakButton });
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 160, true);
			this.ToolStrip.TabIndex = 0;
			this.ToolStrip.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical270;
			// 
			// ByWeightBreakButton
			// 
			this.ByWeightBreakButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("E130C955-ADA0-42C4-A6DE-E0E4208E6FA4", "By Weight Break");
			this.ByWeightBreakButton.CheckOnClick = true;
			this.ByWeightBreakButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.ByWeightBreakButton.Image = ((System.Drawing.Image)(resources.GetObject("ByWeightBreakButton.Image")));
			this.ByWeightBreakButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ByWeightBreakButton.Name = "ByWeightBreakButton";
			this.ByWeightBreakButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 97, true);
			this.ByWeightBreakButton.Tag = "";
			this.ByWeightBreakButton.CheckedChanged += new EventHandler(this.ByWeightBreakButton_CheckedChanged);
			// 
			// Panel2
			// 
			this.Panel2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.Panel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 0, true);
			this.Panel2.Name = "Panel2";
			this.Panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 160, true);
			this.Panel2.TabIndex = 1;
			// 
			// CompanyTariffOrCostBasedCalculatorControl
			// 
			this.Controls.Add(this.Panel2);
			this.Controls.Add(this.ToolStrip);
			this.Controls.Add(this.Panel1);
			this.Name = "CompanyTariffOrCostBasedCalculatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 160, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
