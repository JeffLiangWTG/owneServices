namespace Enterprise.Customs.NO.GUI
{
	partial class ShipmentDetailsLayoutsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.ShipmentDetailsFinalDestinationUserControl = new Enterprise.Customs.NO.GUI.ShipmentDetailsFinalDestinationUserControl();
            this.ShipmentDetailsOriginUserControl = new Enterprise.Customs.NO.GUI.ShipmentDetailsOriginUserControl();
            this.ShipmentDetailsGoodsLocationUserControl = new Enterprise.Customs.NO.GUI.ShipmentDetailsGoodsLocationUserControl();
            this.ShipmentDetailsWeightAndVolumeUserControl = new Enterprise.Customs.NO.GUI.ShipmentDetailsWeightAndVolumeUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ShipmentDetailsFinalDestinationUserControl.SuspendLayout();
            this.ShipmentDetailsOriginUserControl.SuspendLayout();
            this.ShipmentDetailsGoodsLocationUserControl.SuspendLayout();
            this.ShipmentDetailsWeightAndVolumeUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
            // 
            // ShipmentDetailsFinalDestinationUserControl
            // 
            this.ShipmentDetailsFinalDestinationUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipmentDetailsFinalDestinationUserControl, ".");
            this.ShipmentDetailsFinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 20, true);
            this.ShipmentDetailsFinalDestinationUserControl.Name = "ShipmentDetailsFinalDestinationUserControl";
            this.ShipmentDetailsFinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            this.ShipmentDetailsFinalDestinationUserControl.TabIndex = 2;
            // 
            // ShipmentDetailsOriginUserControl
            // 
            this.ShipmentDetailsOriginUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipmentDetailsOriginUserControl, ".");
            this.ShipmentDetailsOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 49, true);
            this.ShipmentDetailsOriginUserControl.Name = "ShipmentDetailsOriginUserControl";
            this.ShipmentDetailsOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            this.ShipmentDetailsOriginUserControl.TabIndex = 1;
            // 
            // ShipmentDetailsGoodsLocationUserControl
            // 
            this.ShipmentDetailsGoodsLocationUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipmentDetailsGoodsLocationUserControl, ".");
            this.ShipmentDetailsGoodsLocationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 78, true);
            this.ShipmentDetailsGoodsLocationUserControl.Name = "ShipmentDetailsGoodsLocationUserControl";
            this.ShipmentDetailsGoodsLocationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            this.ShipmentDetailsGoodsLocationUserControl.TabIndex = 1;
            // 
            // ShipmentDetailsWeightAndVolumeUserControl
            // 
            this.ShipmentDetailsWeightAndVolumeUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipmentDetailsWeightAndVolumeUserControl, ".");
            this.ShipmentDetailsWeightAndVolumeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 107, true);
            this.ShipmentDetailsWeightAndVolumeUserControl.Name = "ShipmentDetailsWeightAndVolumeUserControl";
            this.ShipmentDetailsWeightAndVolumeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            this.ShipmentDetailsWeightAndVolumeUserControl.TabIndex = 1;
            // 
            // ShipmentDetailsLayoutsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ShipmentDetailsFinalDestinationUserControl);
            this.Controls.Add(this.ShipmentDetailsOriginUserControl);
            this.Controls.Add(this.ShipmentDetailsGoodsLocationUserControl);
            this.Controls.Add(this.ShipmentDetailsWeightAndVolumeUserControl);
            this.Name = "ShipmentDetailsLayoutsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 154, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ShipmentDetailsFinalDestinationUserControl.ResumeLayout(true);
            this.ShipmentDetailsFinalDestinationUserControl.PerformLayout();
            this.ShipmentDetailsOriginUserControl.ResumeLayout(true);
            this.ShipmentDetailsOriginUserControl.PerformLayout();
            this.ShipmentDetailsGoodsLocationUserControl.ResumeLayout(true);
            this.ShipmentDetailsGoodsLocationUserControl.PerformLayout();
            this.ShipmentDetailsWeightAndVolumeUserControl.ResumeLayout(true);
            this.ShipmentDetailsWeightAndVolumeUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ShipmentDetailsFinalDestinationUserControl ShipmentDetailsFinalDestinationUserControl;
		internal ShipmentDetailsOriginUserControl ShipmentDetailsOriginUserControl;
		internal ShipmentDetailsGoodsLocationUserControl ShipmentDetailsGoodsLocationUserControl;
		internal ShipmentDetailsWeightAndVolumeUserControl ShipmentDetailsWeightAndVolumeUserControl;
	}
}
