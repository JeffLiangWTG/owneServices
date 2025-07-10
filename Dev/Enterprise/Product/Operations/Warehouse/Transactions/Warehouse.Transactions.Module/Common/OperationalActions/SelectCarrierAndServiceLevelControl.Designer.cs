using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	partial class SelectCarrierAndServiceLevelControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare CarrierOrganisationFindBox;

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
			this.CarrierOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.CarrierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CarrierServiceLevelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CarrierServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierOrganisationFindBox.SuspendLayout();
			this.CarrierServiceLevelDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CarrierOrganisationFindBox
			// 
			this.CarrierOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierOrganisationFindBox, "CarrierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Module.OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator)(null)).CarrierPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Module.OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator)(null)).Carriers)));
			this.CarrierOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 16, true);
			this.CarrierOrganisationFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.CarrierOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CarrierOrganisationFindBox.Name = "CarrierOrganisationFindBox";
			this.CarrierOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.CarrierOrganisationFindBox.TabIndex = 1;
			// 
			// CarrierLabel
			// 
			this.CarrierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CarrierLabel.IsFontBold = true;
			this.CarrierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CarrierLabel.Name = "CarrierLabel";
			this.CarrierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.CarrierLabel.TabIndex = 1;
			this.CarrierLabel.Text = "Carrier:";
			// 
			// CarrierServiceLevelLabel
			// 
			this.CarrierServiceLevelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CarrierServiceLevelLabel.IsFontBold = true;
			this.CarrierServiceLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 43, true);
			this.CarrierServiceLevelLabel.Name = "CarrierServiceLevelLabel";
			this.CarrierServiceLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
			this.CarrierServiceLevelLabel.TabIndex = 2;
			this.CarrierServiceLevelLabel.Text = "Carrier Service Level:";
			// 
			// OverridingCarrierServiceLevelWariningLabel
			// 
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 63, true);
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.Name = "OverridingCarrierAndCarrierServiceLevelWariningLabel";
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 33, true);
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.TabIndex = 1;
			this.OverridingCarrierAndCarrierServiceLevelWariningLabel.Text = "Not Specifying Carrier or Carrier Service Level will clear those field(s) from the Order.";
			// 
			// CarrierServiceLevelDropEdit
			// 
			this.CarrierServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceLevelDropEdit, "CarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Module.OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator)(null)).CarrierServiceLevel)));
			this.CarrierServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 40, true);
			this.CarrierServiceLevelDropEdit.Name = "CarrierServiceLevelDropEdit";
			this.CarrierServiceLevelDropEdit.PreBoundMaxLength = 4;
			this.CarrierServiceLevelDropEdit.ShouldResizeByMaxLength = true;
			this.CarrierServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.CarrierServiceLevelDropEdit.TabIndex = 2;
			// 
			// SelectCarrierAndServiceLevelControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CarrierServiceLevelDropEdit);
			this.Controls.Add(this.OverridingCarrierAndCarrierServiceLevelWariningLabel);
			this.Controls.Add(this.CarrierLabel);
			this.Controls.Add(this.CarrierServiceLevelLabel);
			this.Controls.Add(this.CarrierOrganisationFindBox);
			this.Name = "SelectCarrierAndServiceLevelControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierOrganisationFindBox.ResumeLayout(true);
			this.CarrierOrganisationFindBox.PerformLayout();
			this.CarrierServiceLevelDropEdit.ResumeLayout(true);
			this.CarrierServiceLevelDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel CarrierLabel;
		private ZArchitecture.ZLabel CarrierServiceLevelLabel;
		private ZArchitecture.ZLabel OverridingCarrierAndCarrierServiceLevelWariningLabel;
		private ZDropEdit CarrierServiceLevelDropEdit;
	}
}
