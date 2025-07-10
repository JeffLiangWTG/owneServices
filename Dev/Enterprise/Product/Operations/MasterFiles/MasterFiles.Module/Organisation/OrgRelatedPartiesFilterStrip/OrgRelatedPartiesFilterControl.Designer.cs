
namespace Enterprise.MasterFiles.Module
{
	partial class OrgRelatedPartiesFilterControl
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
			this.PartyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedPartyOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter);
			// 
			// PartyTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PartyTypeDropEdit, "PartyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter)(null)).PartyType)));
			this.PartyTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgRelatedPartiesFilterControl|0254a56e-cea6-4e2a-a9cb-7bec1ffb207a", "Party Type");
			this.PartyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 1, true);
			this.PartyTypeDropEdit.Name = "PartyTypeDropEdit";
			this.PartyTypeDropEdit.PreBoundMaxLength = 2;
			this.PartyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PartyTypeDropEdit.TabIndex = 1;
			// 
			// DirectionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter)(null)).Direction)));
			this.DirectionDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgRelatedPartiesFilterControl|b8debbbc-3f29-41af-a686-ac5e98a6eda9", "Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 45, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.PreBoundMaxLength = 2;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.DirectionDropEdit.TabIndex = 7;
			// 
			// TransportModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter)(null)).TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgRelatedPartiesFilterControl|82a546b8-f020-4ed5-a97f-8480d6892202", "Transport Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 23, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 2;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.TransportModeDropEdit.TabIndex = 3;
			// 
			// ContainerModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "ContainerMode");;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter)(null)).ContainerMode)));
			this.ContainerModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgRelatedPartiesFilterControl|824d5933-3653-4f61-a058-5a448c1ec7c8", "Container Mode");
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 23, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 2;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.ContainerModeDropEdit.TabIndex = 5;
			// 
			// RelatedPartyOrganisationFindBox
			// 
			this.BindingSource.SetBindingMember(this.RelatedPartyOrganisationFindBox, "RelatedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgRelatedPartiesModuleFilter)(null)).RelatedParty)));
			this.RelatedPartyOrganisationFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgRelatedPartiesFilterControl|559c1e45-1b12-4716-8cfa-8b3810f63a30", "Related Party");
			this.RelatedPartyOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 67, true);
			this.RelatedPartyOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.RelatedPartyOrganisationFindBox.Name = "RelatedPartyOrganisationFindBox";
			this.RelatedPartyOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.RelatedPartyOrganisationFindBox.TabIndex = 9;
			// 
			// OrgRelatedPartiesFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelatedPartyOrganisationFindBox);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.ContainerModeDropEdit);
			this.Controls.Add(this.DirectionDropEdit);
			this.Controls.Add(this.PartyTypeDropEdit);
			this.Name = "OrgRelatedPartiesFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 87, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit PartyTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ContainerModeDropEdit;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox RelatedPartyOrganisationFindBox;
	}
}
