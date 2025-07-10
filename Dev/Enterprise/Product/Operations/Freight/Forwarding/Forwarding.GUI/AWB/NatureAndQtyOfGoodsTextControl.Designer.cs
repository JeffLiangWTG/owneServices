using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsTextControl
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
			this.textBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// textBox
			// 
			this.BindingSource.SetBindingMember(this.textBox, "NatureAndQtyOfGoodsText.Text");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsText.Text)));
			this.textBox.CaptionResourceString = Res.GetData("77ddf2a7-bde9-45dd-901c-b72303d32552", "Nature and Quantity of Goods Description");
			this.textBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBox, false);
			this.textBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.textBox.Name = "textBox";
			this.textBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.textBox.TabIndex = 0;
			// 
			// NatureAndQtyOfGoodsTextControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBox);
			this.Name = "NatureAndQtyOfGoodsTextControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZTextBox textBox;
	}
}