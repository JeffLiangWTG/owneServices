using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	partial class GenerateOrderFromInventoryOrReceiveControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare ConsigneeOrganisationFindBox;

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
			this.ConsigneeOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.ConsigneeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverridingValuesLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
			this.OverridingNoteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CrossDockFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LocationLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeOrganisationFindBox.SuspendLayout();
			this.CrossDockFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ConsigneeOrganisationFindBox
			// 
			this.ConsigneeOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeOrganisationFindBox, "ConsigneePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Module.GenerateOrderFromExistingInventoryActionMethodApplicator)(null)).ConsigneePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Module.GenerateOrderFromExistingInventoryActionMethodApplicator)(null)).Consignees)));
			this.ConsigneeOrganisationFindBox.BindToList = "Consignees";
			this.ConsigneeOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 36, true);
			this.ConsigneeOrganisationFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.ConsigneeOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ConsigneeOrganisationFindBox.Name = "ConsigneeOrganisationFindBox";
			this.ConsigneeOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 17, true);
			this.ConsigneeOrganisationFindBox.TabIndex = 2;
			// 
			// ConsigneeLabel
			// 
			this.ConsigneeLabel.IsFontBold = true;
			this.ConsigneeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.ConsigneeLabel.Name = "ConsigneeLabel";
			this.ConsigneeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.ConsigneeLabel.TabIndex = 1;
			this.ConsigneeLabel.Text = Res.GetString("b25a155d-89f7-4a13-9a40-fd6bfb83c1f5", "Consignee:");
			// 
			// OverridingValuesLabel
			// 
			this.OverridingValuesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
			this.OverridingValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OverridingValuesLabel.Name = "OverridingValuesLabel";
			this.OverridingValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 23, true);
			this.OverridingValuesLabel.TabIndex = 0;
			this.OverridingValuesLabel.Text = Res.GetString("8fdd629f-0afe-4830-9fe2-c98beb7e294d", "Overriding Values");
			// 
			// OverridingNoteLabel
			// 
			this.OverridingNoteLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.OverridingNoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 60, true);
			this.OverridingNoteLabel.Name = "OverridingNoteLabel";
			this.OverridingNoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 33, true);
			this.OverridingNoteLabel.TabIndex = 3;
			this.OverridingNoteLabel.Text = Res.GetString("ca87843a-ba76-4d77-a9be-85747c416972", "If any selected Inventory or Receive Line does not have a valid Consignee value, then the overriding Consignee value above will be used when it is specified.");
			// 
			// CrossDockFindBox
			// 
			this.CrossDockFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CrossDockFindBox, "CrossDockLocationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Module.GenerateOrderFromExistingInventoryActionMethodApplicator)(null)).CrossDockLocationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Module.GenerateOrderFromExistingInventoryActionMethodApplicator)(null)).CrossDockLocations)));
			this.CrossDockFindBox.BindToList = "CrossDockLocations";
			this.CrossDockFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 118, true);
			this.CrossDockFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigLocation;
			this.CrossDockFindBox.Name = "CrossDockFindBox";
			this.CrossDockFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CrossDockFindBox.ParentType = null;
			this.CrossDockFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 17, true);
			this.CrossDockFindBox.TabIndex = 5;
			// 
			// LocationLabel
			// 
			this.LocationLabel.IsFontBold = true;
			this.LocationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 119, true);
			this.LocationLabel.Name = "LocationLabel";
			this.LocationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 17, true);
			this.LocationLabel.TabIndex = 4;
			this.LocationLabel.Text = Res.GetString("b7bdf461-f869-4e7f-b87c-8e1da4d85f2c", "Cross-Dock Location:");
			// 
			// GenerateOrderFromInventoryOrReceiveControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LocationLabel);
			this.Controls.Add(this.CrossDockFindBox);
			this.Controls.Add(this.OverridingNoteLabel);
			this.Controls.Add(this.OverridingValuesLabel);
			this.Controls.Add(this.ConsigneeLabel);
			this.Controls.Add(this.ConsigneeOrganisationFindBox);
			this.Name = "GenerateOrderFromInventoryOrReceiveControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 273, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeOrganisationFindBox.ResumeLayout(true);
			this.ConsigneeOrganisationFindBox.PerformLayout();
			this.CrossDockFindBox.ResumeLayout(true);
			this.CrossDockFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel ConsigneeLabel;
		private ZArchitecture.ZHeaderLabel OverridingValuesLabel;
		private ZArchitecture.ZLabel OverridingNoteLabel;
		private ZArchitecture.GUI.ZGuidFindBox CrossDockFindBox;
		private ZArchitecture.ZLabel LocationLabel;
	}
}
