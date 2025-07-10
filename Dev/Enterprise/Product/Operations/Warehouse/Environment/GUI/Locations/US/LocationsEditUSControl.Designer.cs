using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.GUI.US
{
	partial class LocationsEditUSControl : Enterprise.Warehouse.Environment.GUI.LocationsEditBaseControl
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TSAStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectedUpdateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectedUpdateGroupBox
			// 
			this.SelectedUpdateGroupBox.Controls.Add(this.TSAStatusDropEdit);
			this.SelectedUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 166, true);
			this.SelectedUpdateGroupBox.Controls.SetChildIndex(this.TSAStatusDropEdit, 0);
			// 
			// LocationGrid
			// 
			zDropEditColumnStyleInfo1.BindToList = "Lookups+ApprovedKnownStatuses";
			zDropEditColumnStyleInfo1.Caption = "TSA Known";
			zDropEditColumnStyleInfo1.ColumnName = "WLV_ApprovedKnownLocation";
			this.LocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LocationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 140, true);
			this.LocationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 307, true);
			// 
			// TSAStatusDropEdit
			// 
			this.TSAStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TSAStatusDropEdit, "ApprovedKnownStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).ApprovedKnownStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsRow)(null)).Lookups.ApprovedKnownStatuses)));
			this.TSAStatusDropEdit.BindToList = "Lookups+ApprovedKnownStatuses";
			this.TSAStatusDropEdit.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationsEditUSControl|2ead80a5-3b75-45bc-9724-946f53c27f44", "TSA Status");
			this.TSAStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 118, true);
			this.TSAStatusDropEdit.Name = "TSAStatusDropEdit";
			this.TSAStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.TSAStatusDropEdit.TabIndex = 15;
			// 
			// LocationsEditUSControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "LocationsEditUSControl";
			this.SelectedUpdateGroupBox.ResumeLayout(false);
			this.SelectedUpdateGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocationGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit TSAStatusDropEdit;

		#endregion
	}
}
