
namespace Enterprise.Customs.US.Module
{
	partial class MQEDIMessageFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// Action Status
			//
			zTextBoxColumnStyleInfo1.Caption = "Action Status";
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ActionStatus";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// 
			// MessageStatusToShowInQueryModule
			// 
			zTextBoxColumnStyleInfo2.Caption = "Status";
			zTextBoxColumnStyleInfo2.ColumnName = "MessageStatusToShowInQueryModule";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			// 
			// FilteredGrid
			// 
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 268, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.MQEDIMessage);
			// 
			// MQEDIMessageFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MQEDIMessageFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 420, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
