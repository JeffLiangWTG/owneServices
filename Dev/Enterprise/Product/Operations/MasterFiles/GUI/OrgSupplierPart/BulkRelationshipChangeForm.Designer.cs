using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BulkRelationshipChangeForm : ZChildForm
	{
		private ZButton CancelBtn;
		private ZButton ContinueBtn;
		private ZGroupBox ToDetailsGroupBox;
		private ZDropEdit ToRelationshipDropEdit;
		private ZGuidFindBox ToOrganisationGuidFindBox;
		private ZGroupBox FromDetailsGroupBox;
		private ZDropEdit FromRelationshipDropEdit;
		private ZGuidFindBox FromOrganisationGuidFindBox;
		private ZLabel zLabel1;
		private ZCalcEdit EstimatedProductCountCalcEdit;
		private ZLabel EstimatedProductCountLabel;

		protected override void InitializeComponent()
		{
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContinueBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ToRelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FromDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromRelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.EstimatedProductCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstimatedProductCountLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToDetailsGroupBox.SuspendLayout();
			this.FromDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 241, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 10, true);
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|423c116d-359c-430d-bf87-3d1b780a491d", "Cancel");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 228, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 6;
			this.CancelBtn.UseVisualStyleBackColor = true;
			// 
			// ContinueBtn
			// 
			this.ContinueBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueBtn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|5ba1669f-156b-4763-a5b5-1331b6f34f5c", "Process");
			this.ContinueBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 228, true);
			this.ContinueBtn.Name = "ContinueBtn";
			this.ContinueBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ContinueBtn.TabIndex = 5;
			this.ContinueBtn.UseVisualStyleBackColor = true;
			this.ContinueBtn.Click += new System.EventHandler(this.ContinueBtn_Click);
			// 
			// ToDetailsGroupBox
			// 
			this.ToDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|e84d250f-bcb7-4c34-94be-2e891baa133f", "To Details");
			this.ToDetailsGroupBox.Controls.Add(this.ToRelationshipDropEdit);
			this.ToDetailsGroupBox.Controls.Add(this.ToOrganisationGuidFindBox);
			this.ToDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 146, true);
			this.ToDetailsGroupBox.Name = "ToDetailsGroupBox";
			this.ToDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 56, true);
			this.ToDetailsGroupBox.TabIndex = 2;
			this.ToDetailsGroupBox.TabStop = false;
			// 
			// ToRelationshipDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ToRelationshipDropEdit, "ToRelationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger)(null)).ToRelationship)));
			this.ToRelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|0585a1a9-de9e-48ed-ae20-c6cd5a12043f", "Relationship", "Relationship", "");
			this.ToRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 22, true);
			this.ToRelationshipDropEdit.Name = "ToRelationshipDropEdit";
			this.ToRelationshipDropEdit.ShowDescriptionBox = false;
			this.ToRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ToRelationshipDropEdit.TabIndex = 1;
			// 
			// ToOrganisationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ToOrganisationGuidFindBox, "ToOrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger)(null)).ToOrganisationPK)));
			this.ToOrganisationGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|ec38fe3d-a8ae-4f05-975f-e4be2f97e260", "Organization", "Organization", "");
			this.ToOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 22, true);
			this.ToOrganisationGuidFindBox.Name = "ToOrganisationGuidFindBox";
			this.ToOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ToOrganisationGuidFindBox.TabIndex = 0;
			// 
			// FromDetailsGroupBox
			// 
			this.FromDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|f4167dce-b140-4f75-9dfd-88de6acee045", "From Details");
			this.FromDetailsGroupBox.Controls.Add(this.FromRelationshipDropEdit);
			this.FromDetailsGroupBox.Controls.Add(this.FromOrganisationGuidFindBox);
			this.FromDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 90, true);
			this.FromDetailsGroupBox.Name = "FromDetailsGroupBox";
			this.FromDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 56, true);
			this.FromDetailsGroupBox.TabIndex = 1;
			this.FromDetailsGroupBox.TabStop = false;
			// 
			// FromRelationshipDropEdit
			// 
			this.BindingSource.SetBindingMember(this.FromRelationshipDropEdit, "FromRelationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger)(null)).FromRelationship)));
			this.FromRelationshipDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|a4b0281e-4104-4f8e-bab4-da8eebc4d7e5", "Relationship", "Relationship", "");
			this.FromRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 22, true);
			this.FromRelationshipDropEdit.Name = "FromRelationshipDropEdit";
			this.FromRelationshipDropEdit.ShowDescriptionBox = false;
			this.FromRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.FromRelationshipDropEdit.TabIndex = 1;
			// 
			// FromOrganisationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.FromOrganisationGuidFindBox, "FromOrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger)(null)).FromOrganisationPK)));
			this.FromOrganisationGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|d5d2f049-02f9-4206-9b1d-813bd1f7b3d6", "Organization", "Organization", "");
			this.FromOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 22, true);
			this.FromOrganisationGuidFindBox.Name = "FromOrganisationGuidFindBox";
			this.FromOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FromOrganisationGuidFindBox.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.zLabel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 77, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// EstimatedProductCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EstimatedProductCountCalcEdit, "EstimatedProductCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger)(null)).EstimatedProductCount)));
			this.EstimatedProductCountCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.EstimatedProductCountCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EstimatedProductCountCalcEdit, false);
			this.EstimatedProductCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 215, true);
			this.EstimatedProductCountCalcEdit.Name = "EstimatedProductCountCalcEdit";
			this.EstimatedProductCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.EstimatedProductCountCalcEdit.TabIndex = 3;
			this.EstimatedProductCountCalcEdit.Text = "0";
			this.EstimatedProductCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstimatedProductCountLabel
			// 
			this.EstimatedProductCountLabel.AutoSize = true;
			this.EstimatedProductCountLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|e77ad25d-dd5d-41fd-895d-3c2a875bcc1b", "Products selected.");
			this.EstimatedProductCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 215, true);
			this.EstimatedProductCountLabel.Name = "EstimatedProductCountLabel";
			this.EstimatedProductCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.EstimatedProductCountLabel.TabIndex = 4;
			// 
			// BulkRelationshipChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 264, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BulkRelationshipChangeForm|47c2372a-4405-46cf-81fc-18d3cda9a6c9", "Bulk Relationship Change");
			this.Controls.Add(this.EstimatedProductCountLabel);
			this.Controls.Add(this.EstimatedProductCountCalcEdit);
			this.Controls.Add(this.ToDetailsGroupBox);
			this.Controls.Add(this.FromDetailsGroupBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.ContinueBtn);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.OrgSupplierBulkRelationshipChanger";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 300, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 285, true);
			this.Name = "BulkRelationshipChangeForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, true);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContinueBtn, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.FromDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ToDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.EstimatedProductCountCalcEdit, 0);
			this.Controls.SetChildIndex(this.EstimatedProductCountLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToDetailsGroupBox.ResumeLayout(false);
			this.FromDetailsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
