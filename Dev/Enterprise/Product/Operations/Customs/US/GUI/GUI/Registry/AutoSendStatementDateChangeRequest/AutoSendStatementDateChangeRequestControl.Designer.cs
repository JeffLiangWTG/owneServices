namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class AutoSendStatementDateChangeRequestControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.OverrideByOrganizationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OverrideByOrganizationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.AutoSendStatementDateChangeRequest);
			// 
			// OverrideByOrganizationDropEdit
			// 
			this.OverrideByOrganizationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideByOrganizationDropEdit, "OverrideAllOrByOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DataRegistry.Business.AutoSendStatementDateChangeRequest)(null)).OverrideAllOrByOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.AutoSendStatementDateChangeRequest)(null)).OverrideOrganisationList)));
			this.OverrideByOrganizationDropEdit.BindToList = "OverrideOrganisationList";
			this.OverrideByOrganizationDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AutoSendStatementDateChangeRequestControl|3a2e83f4-940f-43f2-a30b-eae4f4210ad4", "Override by Organization");
			this.OverrideByOrganizationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 16, true);
			this.OverrideByOrganizationDropEdit.Name = "OverrideByOrganizationDropEdit";
			this.OverrideByOrganizationDropEdit.ShowDescriptionBox = false;
			this.OverrideByOrganizationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OverrideByOrganizationDropEdit.TabIndex = 5;
			// 
			// AutoSendStatementDateChangeRequestControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OverrideByOrganizationDropEdit);
			this.Name = "AutoSendStatementDateChangeRequestControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OverrideByOrganizationDropEdit.ResumeLayout(true);
			this.OverrideByOrganizationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit OverrideByOrganizationDropEdit;
	}
}
