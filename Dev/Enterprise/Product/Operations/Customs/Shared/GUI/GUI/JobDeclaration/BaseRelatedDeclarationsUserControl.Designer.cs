namespace Enterprise.Customs.GUI
{
	partial class BaseRelatedDeclarationsUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RelatedDeclarationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RelatedDeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonNew = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonEdit = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).BeginInit();
			this.RelatedDeclarationsGrid.SuspendLayout();
			this.RelatedDeclarationsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// RelatedDeclarationsGrid
			// 
			this.RelatedDeclarationsGrid.AllowNavigation = false;
			this.RelatedDeclarationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RelatedDeclarationsGrid, "RelatedDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).RelatedDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).RelatedDeclarations)).SyncRoot)).JE_DeclarationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).RelatedDeclarations)).SyncRoot)).JE_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).RelatedDeclarations)).SyncRoot)).JE_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).RelatedDeclarations)).SyncRoot)).JE_EntryStatus)));
			this.RelatedDeclarationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JE_DeclarationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JE_MessageType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseRelatedDeclarationsUserControl|f1d09526-7fce-425d-92f8-f167a00ff63b", "Declaration Type");
			zTextBoxColumnStyleInfo3.ColumnName = "JE_MessageSubType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "JE_EntryStatus";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedDeclarationsGrid.GridId = "4d3654d8-9373-4521-a2bd-e648df5b8080";
			this.RelatedDeclarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedDeclarationsGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedDeclarationsGrid.LayoutKey = "zGrid1";
			this.RelatedDeclarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.RelatedDeclarationsGrid.Name = "RelatedDeclarationsGrid";
			this.RelatedDeclarationsGrid.ReadOnly = true;
			this.RelatedDeclarationsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RelatedDeclarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 212, true);
			this.RelatedDeclarationsGrid.TabIndex = 0;
			this.RelatedDeclarationsGrid.DoubleClick += new System.EventHandler(this.RelatedDeclarationsGrid_DoubleClick);
			// 
			// RelatedDeclarationsGroupBox
			// 
			this.RelatedDeclarationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedDeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseRelatedDeclarationsUserControl|11998496-3a96-480d-8b1f-f246cbebb997", "Related Declarations");
			this.RelatedDeclarationsGroupBox.Controls.Add(this.ParentButton);
			this.RelatedDeclarationsGroupBox.Controls.Add(this.ButtonNew);
			this.RelatedDeclarationsGroupBox.Controls.Add(this.RelatedDeclarationsGrid);
			this.RelatedDeclarationsGroupBox.Controls.Add(this.ButtonEdit);
			this.RelatedDeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedDeclarationsGroupBox.Name = "RelatedDeclarationsGroupBox";
			this.RelatedDeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 266, true);
			this.RelatedDeclarationsGroupBox.TabIndex = 0;
			this.RelatedDeclarationsGroupBox.TabStop = false;
			// 
			// ParentButton
			// 
			this.ParentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ParentButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("82a04733-2d08-4099-a577-690b07380a84", "Parent");
			this.ParentButton.EditableInViewMode = true;
			this.ParentButton.IsCaptionOverridden = false;
			this.ParentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 237, true);
			this.ParentButton.Name = "ParentButton";
			this.ParentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ParentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ParentButton.TabIndex = 3;
			this.ParentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ParentButton.ToolTipCaption = null;
			this.ParentButton.UseVisualStyleBackColor = true;
			this.ParentButton.Visible = false;
			this.ParentButton.Click += new System.EventHandler(this.ParentButton_Click);
			// 
			// ButtonNew
			// 
			this.ButtonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonNew.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("014cff5d-99d5-40d3-9545-7933bdb3366f", "New");
			this.ButtonNew.IsCaptionOverridden = false;
			this.ButtonNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 237, true);
			this.ButtonNew.Name = "ButtonNew";
			this.ButtonNew.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonNew.TabIndex = 2;
			this.ButtonNew.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonNew.ToolTipCaption = null;
			this.ButtonNew.UseVisualStyleBackColor = true;
			this.ButtonNew.Click += new System.EventHandler(this.ButtonNew_Click);
			// 
			// ButtonEdit
			// 
			this.ButtonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("55ca8751-2c23-4f04-8534-04a1b4162584", "Edit");
			this.ButtonEdit.IsCaptionOverridden = false;
			this.ButtonEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 237, true);
			this.ButtonEdit.Name = "ButtonEdit";
			this.ButtonEdit.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonEdit.TabIndex = 1;
			this.ButtonEdit.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonEdit.ToolTipCaption = null;
			this.ButtonEdit.UseVisualStyleBackColor = true;
			this.ButtonEdit.Click += new System.EventHandler(this.ButtonEdit_Click);
			// 
			// BaseRelatedDeclarationsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelatedDeclarationsGroupBox);
			this.Name = "BaseRelatedDeclarationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 271, true);
			this.Load += new System.EventHandler(this.BaseRelatedDeclarationsUserControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedDeclarationsGrid)).EndInit();
			this.RelatedDeclarationsGrid.ResumeLayout(false);
			this.RelatedDeclarationsGrid.PerformLayout();
			this.RelatedDeclarationsGroupBox.ResumeLayout(false);
			this.RelatedDeclarationsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZGrid RelatedDeclarationsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RelatedDeclarationsGroupBox;
		public Enterprise.ZArchitecture.GUI.ZButton ButtonEdit;
		public Enterprise.ZArchitecture.GUI.ZButton ButtonNew;
		public ZArchitecture.GUI.ZButton ParentButton;

	}
}
