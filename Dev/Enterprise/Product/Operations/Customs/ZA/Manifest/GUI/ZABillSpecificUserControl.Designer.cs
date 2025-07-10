namespace Enterprise.Customs.ZA.Manifest.GUI
{
	partial class ZABillSpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CargoReleaseStatusOtherDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CargoReleaseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CaseNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CaseNumberBillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CargoReleaseStatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaseNumberGrid)).BeginInit();
			this.CaseNumberGrid.SuspendLayout();
			this.CaseNumberBillsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Manifest.Business.AsycudaBill);
			// 
			// CargoReleaseStatusOtherDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoReleaseStatusOtherDescriptionTextBox, "CargoReleaseStatusOtherDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CargoReleaseStatusOtherDescription)));
			this.CargoReleaseStatusOtherDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CargoReleaseStatusOtherDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.CargoReleaseStatusOtherDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 25, true);
			this.CargoReleaseStatusOtherDescriptionTextBox.Name = "CargoReleaseStatusOtherDescriptionTextBox";
			this.CargoReleaseStatusOtherDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CargoReleaseStatusOtherDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.CargoReleaseStatusOtherDescriptionTextBox.TabIndex = 1;
			// 
			// CargoReleaseStatusDropEdit
			// 
			this.CargoReleaseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoReleaseStatusDropEdit, "CargoReleaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CargoReleaseStatus)));
			this.CargoReleaseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.CargoReleaseStatusDropEdit.Name = "CargoReleaseStatusDropEdit";
			this.CargoReleaseStatusDropEdit.PreBoundMaxLength = 2;
			this.CargoReleaseStatusDropEdit.ShouldResizeByMaxLength = true;
			this.CargoReleaseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.CargoReleaseStatusDropEdit.TabIndex = 0;
			// 
			// CaseNumberGrid
			// 
			this.CaseNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CaseNumberGrid, "CaseNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CaseNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CaseNumbers)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CaseNumbers)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaBill)(null)).CaseNumbers)).SyncRoot)).Description)));
			this.CaseNumberGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("2D3C6F63-509F-4E4A-BE05-BC43A0B464F7", "Case Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("774EE0D9-DE7C-491F-AB2E-61BE8CA6AB8A", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("9B0C850F-3FC1-4FE0-968E-117674E013CA", "Status Description");
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CaseNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CaseNumberGrid.GridId = "9EFE5B55-F331-49C3-91CA-0695E4501ECB";
			this.CaseNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CaseNumberGrid.LayoutKey = "CaseNumberGrid";
			this.CaseNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CaseNumberGrid.Name = "CaseNumberGrid";
			this.CaseNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 169, true);
			this.CaseNumberGrid.TabIndex = 4;
			// 
			// CaseNumberBillsGroupBox
			// 
			this.CaseNumberBillsGroupBox.Controls.Add(this.CaseNumberGrid);
			this.CaseNumberBillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 61, true);
			this.CaseNumberBillsGroupBox.Name = "CaseNumberBillsGroupBox";
			this.CaseNumberBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 188, true);
			this.CaseNumberBillsGroupBox.TabIndex = 3;
			this.CaseNumberBillsGroupBox.TabStop = false;
			this.CaseNumberBillsGroupBox.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("1A22827B-9D92-48CE-8427-595EB5FBE782", "Supporting Documents Cases");
			// 
			// ZABillSpecificUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CargoReleaseStatusOtherDescriptionTextBox);
			this.Controls.Add(this.CargoReleaseStatusDropEdit);
			this.Controls.Add(this.CaseNumberBillsGroupBox);
			this.Name = "ZABillSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CargoReleaseStatusDropEdit.ResumeLayout(true);
			this.CargoReleaseStatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaseNumberGrid)).EndInit();
			this.CaseNumberGrid.ResumeLayout(false);
			this.CaseNumberGrid.PerformLayout();
			this.CaseNumberBillsGroupBox.ResumeLayout(false);
			this.CaseNumberBillsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox CargoReleaseStatusOtherDescriptionTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CargoReleaseStatusDropEdit;
		ZArchitecture.ZGrid CaseNumberGrid;
		internal ZArchitecture.GUI.ZGroupBox CaseNumberBillsGroupBox;
	}
}
