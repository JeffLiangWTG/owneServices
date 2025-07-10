using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.OrderManager.Orders
{
	partial class OrderLineToleranceControl : ZUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ToleranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ToleranceModifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToleranceTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Types.INumericZType);
			// 
			// ToleranceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ToleranceCalcEdit, ".");
			this.ToleranceCalcEdit.BindToDecimalPlaces = null;
			this.ToleranceCalcEdit.CaptionResourceString = null;
			this.ToleranceCalcEdit.DecimalPlaces = 1;
			this.ToleranceCalcEdit.Decimals = 1;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToleranceCalcEdit, false);
			this.ToleranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 5, true);
			this.ToleranceCalcEdit.Name = "ToleranceCalcEdit";
			this.ToleranceCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ToleranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ToleranceCalcEdit.TabIndex = 1;
			this.ToleranceCalcEdit.Text = "0.0";
			this.ToleranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ToleranceCalcEdit.AllowNegative = false;
			// 
			// ToleranceModifierLabel
			// 
			this.ToleranceModifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToleranceModifierLabel, false);
			this.ToleranceModifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.ToleranceModifierLabel.Name = "ToleranceModifierLabel";
			this.ToleranceModifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.ToleranceModifierLabel.TabIndex = 0;
			// 
			// ToleranceTypeLabel
			// 
			this.ToleranceTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToleranceTypeLabel, false);
			this.ToleranceTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 5, true);
			this.ToleranceTypeLabel.Name = "ToleranceTypeLabel";
			this.ToleranceTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.ToleranceTypeLabel.TabIndex = 2;
			// 
			// OrderLineToleranceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToleranceModifierLabel);
			this.Controls.Add(this.ToleranceCalcEdit);
			this.Controls.Add(this.ToleranceTypeLabel);
			this.Name = "OrderLineToleranceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 30, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZCalcEdit ToleranceCalcEdit;
		private Enterprise.ZArchitecture.ZLabel ToleranceModifierLabel;
		private Enterprise.ZArchitecture.ZLabel ToleranceTypeLabel;
	}
}
