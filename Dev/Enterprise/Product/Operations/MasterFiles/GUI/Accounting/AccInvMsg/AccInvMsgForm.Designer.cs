namespace Enterprise.MasterFiles.GUI
{
	public partial class AccInvMsgForm
	{
		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.ZTextBox A9_CodeBoundText;
		private Enterprise.ZArchitecture.ZTextBox A9_DescriptionBoundText;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsShownOnDocumentCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.ZTextBox A9_LocalMsgBoundText;
		private Enterprise.ZArchitecture.ZTranslatableTextControl A9_EnglishMsgBoundText;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsTriggerExemptionMessageCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit A9_TaxGroupZDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox GovtCodeText;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;

		new void InitializeComponent()
		{
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 291, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(393);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccInvMsg);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 260, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.ButtonsUserControl.TabIndex = 18;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 248, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccInvMsgForm|7a4f6280-9c58-48be-b564-7bf8c263e6d4", "Invoice Tax Message");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 226, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_IsShownOnDocuments)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_LocalMsg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_EnglishMsg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_IsTriggerExemptionMessage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).A9_TaxGroupCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccInvMsg)(null)).TaxGroupGovtCode)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 226, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 226, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// AccInvMsgForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("62d57494-0176-46f6-997c-c0d5a99a7efa", "Invoice Tax Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 315, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccInvMsg);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 288, true);
			this.Name = "AccInvMsgForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.A9_CodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.A9_DescriptionBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.IsShownOnDocumentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.A9_LocalMsgBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.A9_EnglishMsgBoundText = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.IsTriggerExemptionMessageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.A9_TaxGroupZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GovtCodeText = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabPage.SuspendLayout();
			this.A9_EnglishMsgBoundText.SuspendLayout();
			this.A9_TaxGroupZDropEdit.SuspendLayout();
			this.MainTabPage.Controls.Add(this.A9_TaxGroupZDropEdit);
			this.MainTabPage.Controls.Add(this.IsTriggerExemptionMessageCheckBox);
			this.MainTabPage.Controls.Add(this.A9_LocalMsgBoundText);
			this.MainTabPage.Controls.Add(this.A9_EnglishMsgBoundText);
			this.MainTabPage.Controls.Add(this.IsShownOnDocumentCheckBox);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.A9_DescriptionBoundText);
			this.MainTabPage.Controls.Add(this.A9_CodeBoundText);
			this.MainTabPage.Controls.Add(this.GovtCodeText);
			// 
			// A9_CodeBoundText
			// 
			this.BindingSource.SetBindingMember(this.A9_CodeBoundText, "A9_Code");
			this.A9_CodeBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 16, true);
			this.A9_CodeBoundText.Name = "A9_CodeBoundText";
			this.A9_CodeBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.A9_CodeBoundText.TabIndex = 2;
			// 
			// A9_DescriptionBoundText
			// 
			this.BindingSource.SetBindingMember(this.A9_DescriptionBoundText, "A9_Description");
			this.A9_DescriptionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 42, true);
			this.A9_DescriptionBoundText.Name = "A9_DescriptionBoundText";
			this.A9_DescriptionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 17, true);
			this.A9_DescriptionBoundText.TabIndex = 3;
			// 
			// IsShownOnDocumentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsShownOnDocumentCheckBox, "A9_IsShownOnDocuments");
			this.IsShownOnDocumentCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccInvMsgForm|IsShownOnDocumentCheckBox", "Print Message on Documents");
			this.IsShownOnDocumentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsShownOnDocumentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 42, true);
			this.IsShownOnDocumentCheckBox.Name = "IsShownOnDocumentCheckBox";
			this.IsShownOnDocumentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.IsShownOnDocumentCheckBox.TabIndex = 6;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "A9_IsActive");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 13, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.IsActiveCheckBox.TabIndex = 5;
			// 
			// A9_LocalMsgBoundText
			// 
			this.BindingSource.SetBindingMember(this.A9_LocalMsgBoundText, "A9_LocalMsg");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.A9_LocalMsgBoundText, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.A9_LocalMsgBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 113, true);
			this.A9_LocalMsgBoundText.Multiline = true;
			this.A9_LocalMsgBoundText.Name = "A9_LocalMsgBoundText";
			this.A9_LocalMsgBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 101, true);
			this.A9_LocalMsgBoundText.TabIndex = 8;
			// 
			// A9_EnglishMsgBoundText
			// 
			this.A9_EnglishMsgBoundText.AcceptsReturn = false;
			this.A9_EnglishMsgBoundText.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.A9_EnglishMsgBoundText, "A9_EnglishMsg");
			this.A9_EnglishMsgBoundText.GridCurrent = null;
			this.A9_EnglishMsgBoundText.GridMember = null;
			this.A9_EnglishMsgBoundText.IsLanguageEditingEnabled = true;
			this.A9_EnglishMsgBoundText.IsMultiLine = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.A9_EnglishMsgBoundText, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.A9_EnglishMsgBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 113, true);
			this.A9_EnglishMsgBoundText.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
			this.A9_EnglishMsgBoundText.Name = "A9_EnglishMsgBoundText";
			this.A9_EnglishMsgBoundText.ReadOnly = false;
			this.A9_EnglishMsgBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 101, true);
			this.A9_EnglishMsgBoundText.TabIndex = 7;
			// 
			// IsTriggerExemptionMessageCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsTriggerExemptionMessageCheckBox, "A9_IsTriggerExemptionMessage");
			this.IsTriggerExemptionMessageCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccInvMsgForm|IsTriggerExemptionMessageCheckBox", "Print Receivables EXV Document Details");
			this.IsTriggerExemptionMessageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsTriggerExemptionMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 71, true);
			this.IsTriggerExemptionMessageCheckBox.Name = "IsTriggerExemptionMessageCheckBox";
			this.IsTriggerExemptionMessageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 23, true);
			this.IsTriggerExemptionMessageCheckBox.TabIndex = 9;
			// 
			// A9_TaxGroupZDropEdit
			// 
			this.A9_TaxGroupZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.A9_TaxGroupZDropEdit, "A9_TaxGroupCode");
			this.A9_TaxGroupZDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("62776f61-83d4-4b4a-bf4e-796cd2e5c497", "Tax Group Code");
			this.A9_TaxGroupZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 68, true);
			this.A9_TaxGroupZDropEdit.Name = "A9_TaxGroupZDropEdit";
			this.A9_TaxGroupZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 17, true);
			this.A9_TaxGroupZDropEdit.TabIndex = 4;
			// 
			// GovtCodeText
			//
			this.BindingSource.SetBindingMember(this.GovtCodeText, "TaxGroupGovtCode");
			this.GovtCodeText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D210EFE8-4B72-4A4E-9FA4-2338AC0D59D6", "Govt. Code", "The official Government Code that identifies this Tax Message Group");
			this.GovtCodeText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GovtCodeText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 90, true);
			this.GovtCodeText.Name = "GovtCodeText";
			this.GovtCodeText.ReadOnly = true;
			this.GovtCodeText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.GovtCodeText.TabIndex = 10;
			this.GovtCodeText.TabStop = false;

			this.MainTabPage.PerformLayout();
			this.A9_EnglishMsgBoundText.ResumeLayout(true);
			this.A9_EnglishMsgBoundText.PerformLayout();
			this.A9_TaxGroupZDropEdit.ResumeLayout(true);
			this.A9_TaxGroupZDropEdit.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void zStmNoteTabPage1_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}

		#endregion

	}
}
