using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class UpdateLastKnownTransitWarehouseUserControl
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
			this.TransitWarehouseAddressControl = new ZAddressControl();
			this.TransitWarehouseAddressLabel = new ZLabel();
			this.TransitWarehouseStatusDropEdit = new ZDropEdit();
			this.TransitWarehouseStatusLabel = new ZLabel();
			this.TransitWarehouseStatusDateTimeEdit = new ZDateEdit();
			this.TransitWarehouseStatusDateTimeLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateLastKnownTransitWarehouseApplicator);
			// 
			// TransitWarehouseAddressControl
			// 
			this.TransitWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitWarehouseAddressControl, "TransitWarehouseAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((UpdateLastKnownTransitWarehouseApplicator)(null)).TransitWarehouseAddressPK)));
			this.TransitWarehouseAddressControl.BindToOrgList = "BindToLists+OrgHeader_List";
			this.TransitWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 2, true);
			this.TransitWarehouseAddressControl.Name = "TransitWarehouseAddressControl";
			this.TransitWarehouseAddressControl.ReadOnly = false;
			this.TransitWarehouseAddressControl.ShowAddress = false;
			this.TransitWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 18, true);
			this.TransitWarehouseAddressControl.TabIndex = 0;
			// 
			// TransitWarehouseAddressLabel
			// 
			this.TransitWarehouseAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransitWarehouseAddressLabel.Name = "TransitWarehouseAddressLabel";
			this.TransitWarehouseAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.TransitWarehouseAddressLabel.TabIndex = 0;
			this.TransitWarehouseAddressLabel.Text = "Transit Warehouse";
			// 
			// TransitWarehouseStatusDropEdit
			// 
			this.TransitWarehouseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitWarehouseStatusDropEdit, "TransitWarehouseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((UpdateLastKnownTransitWarehouseApplicator)(null)).TransitWarehouseStatus)));
			this.TransitWarehouseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 25, true);
			this.TransitWarehouseStatusDropEdit.Name = "TransitWarehouseStatusDropEdit";
			this.TransitWarehouseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.TransitWarehouseStatusDropEdit.TabIndex = 1;
			// 
			// TransitWarehouseStatusLabel
			// 
			this.TransitWarehouseStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.TransitWarehouseStatusLabel.Name = "TransitWarehouseStatusLabel";
			this.TransitWarehouseStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.TransitWarehouseStatusLabel.TabIndex = 1;
			this.TransitWarehouseStatusLabel.Text = "Status";
			// 
			// TransitWarehouseStatusDateTimeEdit
			// 
			this.TransitWarehouseStatusDateTimeEdit.AllowDrop = true;
			this.TransitWarehouseStatusDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.TransitWarehouseStatusDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TransitWarehouseStatusDateTimeEdit, "TransitWarehouseStatusDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((UpdateLastKnownTransitWarehouseApplicator)(null)).TransitWarehouseStatusDateTime)));
			this.TransitWarehouseStatusDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.TransitWarehouseStatusDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 48, true);
			this.TransitWarehouseStatusDateTimeEdit.Name = "TransitWarehouseStatusDateTimeEdit";
			this.TransitWarehouseStatusDateTimeEdit.TabIndex = 2;
			// 
			// TransitWarehouseStatusDateTimeLabel
			// 
			this.TransitWarehouseStatusDateTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.TransitWarehouseStatusDateTimeLabel.Name = "TransitWarehouseStatusDateTimeLabel";
			this.TransitWarehouseStatusDateTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.TransitWarehouseStatusDateTimeLabel.TabIndex = 2;
			this.TransitWarehouseStatusDateTimeLabel.Text = "Date";
			// 
			// UpdateLastKnownTransitWarehouseUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransitWarehouseAddressControl);
			this.Controls.Add(this.TransitWarehouseAddressLabel);
			this.Controls.Add(this.TransitWarehouseStatusLabel);
			this.Controls.Add(this.TransitWarehouseStatusDropEdit);
			this.Controls.Add(this.TransitWarehouseStatusDateTimeEdit);
			this.Controls.Add(this.TransitWarehouseStatusDateTimeLabel);
			this.Name = "UpdateLastKnownTransitWarehouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZAddressControl TransitWarehouseAddressControl;
		private ZLabel TransitWarehouseAddressLabel;
		private ZDropEdit TransitWarehouseStatusDropEdit;
		private ZLabel TransitWarehouseStatusLabel;
		private ZDateEdit TransitWarehouseStatusDateTimeEdit;
		private ZLabel TransitWarehouseStatusDateTimeLabel;
	}
}
