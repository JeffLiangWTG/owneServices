namespace Enterprise.Customs.US.GUI
{
	partial class CargoManifestQueryGridForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ActionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsGroupQueriesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InbondLevelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LevelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ActionDropEdit.SuspendLayout();
			this.IsGroupQueriesCheckBox.SuspendLayout();
			this.InbondLevelGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LevelsGrid)).BeginInit();
			this.LevelsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CargoManifestQueryHeader);
			// 
			// ActionLabel
			// 
			this.ActionLabel.AutoSize = true;
			this.ActionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 23, true);
			this.ActionLabel.Name = "ActionLabel";
			this.ActionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			this.ActionLabel.TabIndex = 1;
			this.ActionLabel.Text = "Action:";
			// 
			// ActionDropEdit
			// 
			this.ActionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActionDropEdit, "ActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).ActionCode)));
			this.ActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 19, true);
			this.ActionDropEdit.MaxItemsToShowInDropDown = 15;
			this.ActionDropEdit.Name = "ActionDropEdit";
			this.ActionDropEdit.PreBoundMaxLength = 3;
			this.ActionDropEdit.ShouldResizeByMaxLength = true;
			this.ActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.ActionDropEdit.TabIndex = 0;
			// 
			// IsGroupQueriesCheckBox
			// 
			this.IsGroupQueriesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsGroupQueriesCheckBox, "IsGroupQueries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).IsGroupQueries)));
			this.IsGroupQueriesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsGroupQueriesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsGroupQueriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 21, true);
			this.IsGroupQueriesCheckBox.Name = "IsGroupQueriesCheckBox";
			this.IsGroupQueriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.IsGroupQueriesCheckBox.TabIndex = 1;
			this.IsGroupQueriesCheckBox.Text = "Group Queries";
			// 
			// InbondLevelGroupBox
			// 
			this.InbondLevelGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.InbondLevelGroupBox.Controls.Add(this.LevelsGrid);
			this.InbondLevelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.InbondLevelGroupBox.Name = "InbondLevelGroupBox";
			this.InbondLevelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 149, true);
			this.InbondLevelGroupBox.TabIndex = 2;
			this.InbondLevelGroupBox.TabStop = false;
			this.InbondLevelGroupBox.Text = "Related Records to Send Messages For";
			// 
			// LevelsGrid
			// 
			this.LevelsGrid.AllowNavigation = false;
			this.LevelsGrid.BackColor = System.Drawing.SystemColors.WindowFrame;
			this.BindingSource.SetBindingMember(this.LevelsGrid, "SendingObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).Issuer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).MasterBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).HouseBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).InBondNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).RequestForRelatedBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CargoManifestQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.CargoManifestQueryHeader)(null)).SendingObjects)).SyncRoot)).OutputOption)));
			this.LevelsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Issuer Code";
			zTextBoxColumnStyleInfo1.ColumnName = "Issuer";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Master Bill Number";
			zTextBoxColumnStyleInfo2.ColumnName = "MasterBillNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo3.Caption = "House Bill Number";
			zTextBoxColumnStyleInfo3.ColumnName = "HouseBillNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo4.Caption = "In-Bond Bill Number";
			zTextBoxColumnStyleInfo4.ColumnName = "InBondNumber";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.Caption = "Request For Related Bills";
			zCheckBoxColumnStyleInfo1.ColumnName = "RequestForRelatedBills";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.Caption = "Output Option";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo1.ColumnName = "OutputOption";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.LevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LevelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LevelsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LevelsGrid.GridId = "610D330E-6839-43BA-906F-77D4E7F3B921";
			this.LevelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LevelsGrid.LayoutKey = "LevelsGrid";
			this.LevelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LevelsGrid.Name = "LevelsGrid";
			this.LevelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 130, true);
			this.LevelsGrid.TabIndex = 0;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 203, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.Text = "Send";
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(602, 203, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 4;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CargoManifestQueryGridForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 262, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.ActionLabel);
			this.Controls.Add(this.InbondLevelGroupBox);
			this.Controls.Add(this.ActionDropEdit);
			this.Controls.Add(this.IsGroupQueriesCheckBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CargoManifestQueryHeader);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CargoManifestQueryHeader";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 290, true);
			this.Name = "CargoManifestQueryGridForm";
			this.Text = "Cargo Manifest Status Query";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ActionDropEdit, 0);
			this.Controls.SetChildIndex(this.IsGroupQueriesCheckBox, 0);
			this.Controls.SetChildIndex(this.InbondLevelGroupBox, 0);
			this.Controls.SetChildIndex(this.ActionLabel, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActionDropEdit.ResumeLayout(true);
			this.ActionDropEdit.PerformLayout();
			this.IsGroupQueriesCheckBox.ResumeLayout(true);
			this.IsGroupQueriesCheckBox.PerformLayout();
			this.InbondLevelGroupBox.ResumeLayout(false);
			this.InbondLevelGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LevelsGrid)).EndInit();
			this.LevelsGrid.ResumeLayout(false);
			this.LevelsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel ActionLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ActionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InbondLevelGroupBox;
		private Enterprise.ZArchitecture.ZGrid LevelsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsGroupQueriesCheckBox;
	}
}
