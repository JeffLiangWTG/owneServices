namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class TWManifestLayoutUserControl
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
			this.ImporterAddressUserControl = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.ImporterAddressUserControl();
			this.ExporterAddressUserControl = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.ExporterAddressUserControl();
			this.CustomsAgentCodeFindBox = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.CustomsAgentCodeFindBox();
			this.PersonGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DeclarationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BagNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNumberUserControl = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.EntryNumberUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterAddressUserControl.SuspendLayout();
			this.ExporterAddressUserControl.SuspendLayout();
			this.CustomsAgentCodeFindBox.SuspendLayout();
			this.PersonGuidFindBox.SuspendLayout();
			this.DeclarationDateEdit.SuspendLayout();
			this.EntryNumberUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader);
			// 
			// ImporterAddressUserControl
			// 
			this.ImporterAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterAddressUserControl, ".");
			this.ImporterAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("5CDC6C7F-D49B-4149-B29C-CA6AF870CF8B", "Importer");
			this.ImporterAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 70, true);
			this.ImporterAddressUserControl.Name = "ImporterAddressUserControl";
			this.ImporterAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 232, true);
			this.ImporterAddressUserControl.TabIndex = 1;
			// 
			// ExporterAddressUserControl
			// 
			this.ExporterAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterAddressUserControl, ".");
			this.ExporterAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("500F6D80-0B5D-4763-977F-88B870411451", "Exporter");
			this.ExporterAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 70, true);
			this.ExporterAddressUserControl.Name = "ExporterAddressUserControl";
			this.ExporterAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 232, true);
			this.ExporterAddressUserControl.TabIndex = 1;
			// 
			// CustomsAgentCodeFindBox
			// 
			this.CustomsAgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsAgentCodeFindBox, "AMA_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).AMA_GS_NKCustomsAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).CustomsAgentDescription)));
			this.CustomsAgentCodeFindBox.BindToForDescription = "CustomsAgentDescription";
			this.CustomsAgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 3, true);
			this.CustomsAgentCodeFindBox.Name = "CustomsAgentCodeFindBox";
			this.CustomsAgentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsAgentCodeFindBox.ParentType = null;
			this.CustomsAgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CustomsAgentCodeFindBox.TabIndex = 0;
			// 
			// PersonGuidFindBox
			// 
			this.PersonGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PersonGuidFindBox, "Person_CPN_PER_PersonPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).Person_CPN_PER_PersonPK)));
			this.PersonGuidFindBox.BindToForDescription = "Person+PersonDescription";
			this.PersonGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 29, true);
			this.PersonGuidFindBox.Name = "PersonGuidFindBox";
			this.PersonGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PersonGuidFindBox.ParentType = null;
			this.PersonGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PersonGuidFindBox.TabIndex = 1;
			// 
			// DeclarationDateEdit
			// 
			this.DeclarationDateEdit.AllowDrop = true;
			this.DeclarationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeclarationDateEdit, "DeclarationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).DeclarationDate)));
			this.DeclarationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 3, true);
			this.DeclarationDateEdit.Name = "DeclarationDateEdit";
			this.DeclarationDateEdit.TabIndex = 18;
			// 
			// BagNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BagNumberTextBox, "BagNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader)(null)).BagNumber)));
			this.BagNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.BagNumberTextBox.Name = "BagNumberTextBox";
			this.BagNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.BagNumberTextBox.TabIndex = 20;
			// 
			// EntryNumberUserControl
			// 
			this.EntryNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryNumberUserControl, ".");
			this.EntryNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 318, true);
			this.EntryNumberUserControl.Name = "EntryNumberUserControl";
			this.EntryNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 29, true);
			this.EntryNumberUserControl.TabIndex = 22;
			// 
			// TWManifestLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.EntryNumberUserControl);
			this.Controls.Add(this.BagNumberTextBox);
			this.Controls.Add(this.DeclarationDateEdit);
			this.Controls.Add(this.CustomsAgentCodeFindBox);
			this.Controls.Add(this.PersonGuidFindBox);
			this.Controls.Add(this.ImporterAddressUserControl);
			this.Controls.Add(this.ExporterAddressUserControl);
			this.Name = "TWManifestLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 388, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterAddressUserControl.ResumeLayout(true);
			this.ImporterAddressUserControl.PerformLayout();
			this.ExporterAddressUserControl.ResumeLayout(true);
			this.ExporterAddressUserControl.PerformLayout();
			this.CustomsAgentCodeFindBox.ResumeLayout(true);
			this.CustomsAgentCodeFindBox.PerformLayout();
			this.PersonGuidFindBox.ResumeLayout(true);
			this.PersonGuidFindBox.PerformLayout();
			this.DeclarationDateEdit.ResumeLayout(true);
			this.DeclarationDateEdit.PerformLayout();
			this.EntryNumberUserControl.ResumeLayout(true);
			this.EntryNumberUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.CustomsAgentCodeFindBox CustomsAgentCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox PersonGuidFindBox;
		internal ImporterAddressUserControl ImporterAddressUserControl;
		internal ExporterAddressUserControl ExporterAddressUserControl;
		internal ZArchitecture.GUI.ZDateEdit DeclarationDateEdit;
		internal ZArchitecture.ZTextBox BagNumberTextBox;
		internal EntryNumberUserControl EntryNumberUserControl;
	}
}
