using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.GUI
{
    partial class FreightRateControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.RateAndCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AutoratingModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// RateAndCurrencyCalcFindBox
			// 
			this.RateAndCurrencyCalcFindBox.AllowDrop = true;
			this.RateAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RateAndCurrencyCalcFindBox, false);
			this.RateAndCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateAndCurrencyCalcFindBox.Name = "RateAndCurrencyCalcFindBox";
			this.RateAndCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.RateAndCurrencyCalcFindBox.TabIndex = 0;
			// 
			// AutoratingModeDropEdit
			// 
			this.AutoratingModeDropEdit.AllowDrop = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AutoratingModeDropEdit, false);
			this.AutoratingModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 0, true);
			this.AutoratingModeDropEdit.Name = "AutoratingModeDropEdit";
			this.AutoratingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.AutoratingModeDropEdit.TabIndex = 1;
			// 
			// FreightRateControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateAndCurrencyCalcFindBox);
			this.Controls.Add(this.AutoratingModeDropEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this, true);
			this.Name = "FreightRateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private ZArchitecture.GUI.ZCalcFindBox RateAndCurrencyCalcFindBox;
		public ZArchitecture.GUI.ZDropEdit AutoratingModeDropEdit;
    }
}
