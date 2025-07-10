namespace Enterprise.Freight.LocalCartage.GUI.CartageLeg.GPS
{
	partial class GPSMessagesControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.GPSMessageActivitiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GPSVehicleIDGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GPSMessageActivitiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// GPSMessageActivitiesGrid
			// 
			this.GPSMessageActivitiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GPSMessageActivitiesGrid, "GPSActivities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).GPSActivities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).GPSActivities)).SyncRoot)).EN_ActivityTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).GPSActivities)).SyncRoot)).ActivityStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).GPSActivities)).SyncRoot)).EN_ActivityInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GPS.Business.GPSSupporterActivity)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).GPSActivities)).SyncRoot)).VehicleShortCode)));
			this.GPSMessageActivitiesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "EN_ActivityTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ce725fad-abe8-4698-a3cc-f0d96c3c2f7a", "Activity Status");
			zTextBoxColumnStyleInfo1.ColumnName = "ActivityStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("cab8d6be-608f-4582-827d-163c103aa1a1", "Activity Information");
			zTextBoxColumnStyleInfo2.ColumnName = "EN_ActivityInformation";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo3.Caption = "Vehicle";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("GPSMessagesControl|4f15e63c-7b18-428a-80f8-a28f999f56a1", "Vehicle");
			zTextBoxColumnStyleInfo3.ColumnName = "VehicleShortCode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.GPSMessageActivitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.GPSMessageActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GPSMessageActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GPSMessageActivitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GPSMessageActivitiesGrid.CopySelectedRowsAllowed = true;
			this.GPSMessageActivitiesGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.GPSMessageActivitiesGrid.GridId = "f1d7e3e8-dc92-44fb-933f-88fb1911dccf";
			this.GPSMessageActivitiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GPSMessageActivitiesGrid.LayoutKey = "zGrid1";
			this.GPSMessageActivitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GPSMessageActivitiesGrid.Name = "GPSMessageActivitiesGrid";
			this.GPSMessageActivitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 145, true);
			this.GPSMessageActivitiesGrid.TabIndex = 0;
			// 
			// GPSVehicleIDGuidFindBox
			// 
			this.GPSVehicleIDGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GPSVehicleIDGuidFindBox, "WorkSheet+EY_RQ_Truck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WorkSheet.EY_RQ_Truck)));
			this.GPSVehicleIDGuidFindBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("GPSMessagesControl|e23db607-8f9f-4184-8bfe-7bb07e4334b9", "Vehicle", "Vehicle", "");
			this.GPSVehicleIDGuidFindBox.Enabled = false;
			this.GPSVehicleIDGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 7, true);
			this.GPSVehicleIDGuidFindBox.Name = "GPSVehicleIDGuidFindBox";
			this.GPSVehicleIDGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.GPSVehicleIDGuidFindBox.TabIndex = 1;
			// 
			// GPSMessagesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GPSMessageActivitiesGrid);
			this.Controls.Add(this.GPSVehicleIDGuidFindBox);
			this.Name = "GPSMessagesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GPSMessageActivitiesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid GPSMessageActivitiesGrid;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GPSVehicleIDGuidFindBox;
	}
}
