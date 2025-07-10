using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class LastKnownTransitWarehouseControl
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
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.AddressControl.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.StatusDateTimeEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupBox
			// 
			this.GroupBox.Controls.Add(this.AddressControl);
			this.GroupBox.Controls.Add(this.StatusDropEdit);
			this.GroupBox.Controls.Add(this.StatusDateTimeEdit);
			this.GroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b7ae729c-677d-4d90-8411-195713338c29", "Last Known Transit Warehouse");
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 59, true);
			this.GroupBox.TabIndex = 1;
			this.GroupBox.TabStop = false;
			// 
			// AddressControl
			// 
			this.AddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressControl, "OuterPackLines.JL_OA_LastKnownTransitWarehouseAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OA_LastKnownTransitWarehouseAddress)));
			this.AddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("312d3262-b404-4746-b201-8b8946df003a", "Transit Warehouse", "Transit Warehouse", "");
			this.AddressControl.BindToOrgList = "OuterPackLines.OrgHeader_List";
			this.AddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 14, true);
			this.AddressControl.Name = "AddressControl";
			this.AddressControl.PopupCaption = "";
			this.AddressControl.ReadOnly = false;
			this.AddressControl.ShowAddress = false;
			this.AddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 18, true);
			this.AddressControl.TabIndex = 1;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "OuterPackLines.LastKnownTransitWarehouseStatusForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).LastKnownTransitWarehouseStatusForBinding)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5e88edf0-fd94-45f8-8c2b-1d56732360d2", "Status", "Status", "");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 36, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 18, true);
			this.StatusDropEdit.TabIndex = 2;
			// 
			// StatusDateTimeEdit
			// 
			this.StatusDateTimeEdit.AllowDrop = true;
			this.StatusDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.StatusDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatusDateTimeEdit, "OuterPackLines.JL_LastKnownTransitWarehouseStatusDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_LastKnownTransitWarehouseStatusDateTime)));
			this.StatusDateTimeEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b1a922b3-b5bd-4852-b368-596aa5340164", "Date", "Date", "");
			this.StatusDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.StatusDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 36, true);
			this.StatusDateTimeEdit.Name = "StatusDateTimeEdit";
			this.StatusDateTimeEdit.TabIndex = 3;
			// 
			// LastKnownTransitWarehouseControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "LastKnownTransitWarehouseControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 59, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.AddressControl.ResumeLayout(true);
			this.AddressControl.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.StatusDateTimeEdit.ResumeLayout(true);
			this.StatusDateTimeEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZGroupBox GroupBox;
		private ZAddressControl AddressControl;
		private ZDropEdit StatusDropEdit;
		private ZDateEdit StatusDateTimeEdit;
	}
}
