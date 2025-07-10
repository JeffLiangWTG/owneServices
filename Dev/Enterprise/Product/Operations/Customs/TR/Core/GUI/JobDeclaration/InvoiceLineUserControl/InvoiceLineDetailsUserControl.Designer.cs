namespace Enterprise.Customs.TR.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupplementaryCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartNoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplementaryCode2DropEdit.SuspendLayout();
			this.SupplementaryCode1DropEdit.SuspendLayout();
			this.PartNoCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine);
			// 
			// SupplementaryCode2DropEdit
			// 
			this.SupplementaryCode2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode2DropEdit, "JI_SupplementaryCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(null)).JI_SupplementaryCode2)));
			this.SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 89, true);
			this.SupplementaryCode2DropEdit.Name = "SupplementaryCode2DropEdit";
			this.SupplementaryCode2DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode2DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SupplementaryCode2DropEdit.TabIndex = 0;
			// 
			// SupplementaryCode1DropEdit
			// 
			this.SupplementaryCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode1DropEdit, "JI_SupplementaryCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(null)).JI_SupplementaryCode1)));
			this.SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 55, true);
			this.SupplementaryCode1DropEdit.Name = "SupplementaryCode1DropEdit";
			this.SupplementaryCode1DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode1DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SupplementaryCode1DropEdit.TabIndex = 1;
			// 
			// PartNoCodeFindBox
			// 
			this.PartNoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartNoCodeFindBox, "JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(null)).JI_PartNo)));
			this.PartNoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 21, true);
			this.PartNoCodeFindBox.Name = "PartNoCodeFindBox";
			this.PartNoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PartNoCodeFindBox.ParentType = null;
			this.PartNoCodeFindBox.PreBoundMaxLength = 35;
			this.PartNoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.PartNoCodeFindBox.TabIndex = 2;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(null)).JI_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = null;
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 123, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.BrandNameTextBox.TabIndex = 3;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplementaryCode1DropEdit);
			this.Controls.Add(this.SupplementaryCode2DropEdit);
			this.Controls.Add(this.PartNoCodeFindBox);
			this.Controls.Add(this.BrandNameTextBox);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplementaryCode2DropEdit.ResumeLayout(true);
			this.SupplementaryCode2DropEdit.PerformLayout();
			this.SupplementaryCode1DropEdit.ResumeLayout(true);
			this.SupplementaryCode1DropEdit.PerformLayout();
			this.PartNoCodeFindBox.ResumeLayout(true);
			this.PartNoCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode2DropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode1DropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox PartNoCodeFindBox;
		internal ZArchitecture.ZTextBox BrandNameTextBox;
	}
}
