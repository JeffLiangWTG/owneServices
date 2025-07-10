namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCommodityCodeForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LocalCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RatingCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
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
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 24, true);
			this.MainStatusBar.SizingGrip = false;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(590);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCommodityCode);
			//
			// ButtonsUserControl
			//
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 328, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.LocalCodesTabPage);
			this.MainTabControl.Controls.Add(this.RatingCodesTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 330, true);
			this.MainTabControl.TabIndex = 0;
			//
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|70268b83-94c8-459e-8843-be20b2335545", "Commodity Details");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 233, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsTimber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsPerishable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsHazardous)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsFlammable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsSystem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_FN_NKNMFC)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_ReeferMaxTemperature)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_ReeferMinTemperature)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_ExpiryDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_ContainerVentRequired)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsForwarding)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsShipping)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsLandTransport)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_IsPersonalEffects)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RH_UniversalCommodityGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).Lookups.UniversalCommodityCodeBizoList)));
			//
			// zCodeMapTabPage
			//
			this.LocalCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|44fa631d-1d2f-4315-888c-491e52b7412d", "Local Codes");
			this.LocalCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LocalCodesTabPage.Name = "LocalCodesTabPage";
			this.LocalCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 276, true);
			this.LocalCodesTabPage.TabIndex = 1;
			this.LocalCodesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.LocalCodesTabPage_InitializeTab));
			//
			// RatingCodesTabPage
			//
			this.RatingCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|adce5152-c2db-438c-be39-b3cc8acf6fbe", "Rating Codes");
			this.RatingCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RatingCodesTabPage.Name = "RatingCodesTabPage";
			this.RatingCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 276, true);
			this.RatingCodesTabPage.TabIndex = 1;
			this.RatingCodesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RatingCodesTabPage_InitializeTab));
			//
			// zStmNoteTabPage1
			//
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 274, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			//
			// zLogsTabPage1
			//
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 253, true);
			this.zLogsTabPage1.TabIndex = 3;
			//
			// RefCommodityCodeForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|0012e881-f96b-4d8e-a07e-ca69a34647fa", "Commodity");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 393, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCommodityCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 320, true);
			this.Name = "RefCommodityCodeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
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
			this.RH_CodeBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.RH_DescriptionBoundText = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.RH_IsTimberBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RH_IsPerishableBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RH_IsHazardousBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RH_IsFlammableBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NMFCCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RH_ReeferMaxTemperatureBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RH_ReeferMinTemperatureBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RH_ExpiryDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DegCLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.DegCLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommodityTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RH_ContainerVentRequiredBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsForwardingchCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsShippingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsLandTransportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsPersonalEffectsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UniversalGroup = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IATACommodityItem = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MainTabPage.SuspendLayout();
			this.RH_DescriptionBoundText.SuspendLayout();
			this.NMFCCodeFindBox.SuspendLayout();
			this.RH_ExpiryDateBoundDateEdit.SuspendLayout();
			this.UniversalGroup.SuspendLayout();
			this.MainTabPage.Controls.Add(this.NMFCCodeFindBox);
			this.MainTabPage.Controls.Add(this.RH_ReeferMaxTemperatureBoundCalcEdit);
			this.MainTabPage.Controls.Add(this.RH_ReeferMinTemperatureBoundCalcEdit);
			this.MainTabPage.Controls.Add(this.RH_ExpiryDateBoundDateEdit);
			this.MainTabPage.Controls.Add(this.DegCLabel2);
			this.MainTabPage.Controls.Add(this.DegCLabel);
			this.MainTabPage.Controls.Add(this.CommodityTypeLabel);
			this.MainTabPage.Controls.Add(this.RH_ContainerVentRequiredBoundCheckEdit);
			this.MainTabPage.Controls.Add(this.UniversalGroup);
			this.MainTabPage.Controls.Add(this.IsForwardingchCheckBox);
			this.MainTabPage.Controls.Add(this.IsShippingCheckBox);
			this.MainTabPage.Controls.Add(this.IsLandTransportCheckBox);
			this.MainTabPage.Controls.Add(this.IsPersonalEffectsCheckBox);
			this.MainTabPage.Controls.Add(this.RH_IsHazardousBoundCheckEdit);
			this.MainTabPage.Controls.Add(this.RH_IsFlammableBoundCheckEdit);
			this.MainTabPage.Controls.Add(this.IsActiveCheckbox);
			this.MainTabPage.Controls.Add(this.IsSystemCheckbox);
			this.MainTabPage.Controls.Add(this.RH_CodeBoundText);
			this.MainTabPage.Controls.Add(this.IATACommodityItem);
			this.MainTabPage.Controls.Add(this.RH_DescriptionBoundText);
			this.MainTabPage.Controls.Add(this.RH_IsTimberBoundCheckEdit);
			this.MainTabPage.Controls.Add(this.RH_IsPerishableBoundCheckEdit);
			//
			// RH_CodeBoundText
			//
			this.RH_CodeBoundText.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RH_CodeBoundText, "RH_Code");
			this.RH_CodeBoundText.CaptionResourceString = null;
			this.RH_CodeBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 14, true);
			this.RH_CodeBoundText.Name = "RH_CodeBoundText";
			this.RH_CodeBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.RH_CodeBoundText.TabIndex = 0;
			//
			// RH_DescriptionBoundText
			//
			this.RH_DescriptionBoundText.AcceptsReturn = false;
			this.RH_DescriptionBoundText.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RH_DescriptionBoundText, "RH_Description");
			this.RH_DescriptionBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RH_DescriptionBoundText.GridCurrent = null;
			this.RH_DescriptionBoundText.GridMember = null;
			this.RH_DescriptionBoundText.IsLanguageEditingEnabled = true;
			this.RH_DescriptionBoundText.IsMultiLine = true;
			this.RH_DescriptionBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 66, true);
			this.RH_DescriptionBoundText.Name = "RH_DescriptionBoundText";
			this.RH_DescriptionBoundText.ReadOnly = false;
			this.RH_DescriptionBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 40, true);
			this.RH_DescriptionBoundText.TabIndex = 4;
			//
			// RH_IsTimberBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.RH_IsTimberBoundCheckEdit, "RH_IsTimber");
			this.RH_IsTimberBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RH_IsTimberBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RH_IsTimberBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 265, true);
			this.RH_IsTimberBoundCheckEdit.Name = "RH_IsTimberBoundCheckEdit";
			this.RH_IsTimberBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 21, true);
			this.RH_IsTimberBoundCheckEdit.TabIndex = 16;
			this.RH_IsTimberBoundCheckEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// RH_IsPerishableBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.RH_IsPerishableBoundCheckEdit, "RH_IsPerishable");
			this.RH_IsPerishableBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RH_IsPerishableBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RH_IsPerishableBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 233, true);
			this.RH_IsPerishableBoundCheckEdit.Name = "RH_IsPerishableBoundCheckEdit";
			this.RH_IsPerishableBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 21, true);
			this.RH_IsPerishableBoundCheckEdit.TabIndex = 14;
			this.RH_IsPerishableBoundCheckEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// RH_IsHazardousBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.RH_IsHazardousBoundCheckEdit, "RH_IsHazardous");
			this.RH_IsHazardousBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RH_IsHazardousBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RH_IsHazardousBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 205, true);
			this.RH_IsHazardousBoundCheckEdit.Name = "RH_IsHazardousBoundCheckEdit";
			this.RH_IsHazardousBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 21, true);
			this.RH_IsHazardousBoundCheckEdit.TabIndex = 13;
			this.RH_IsHazardousBoundCheckEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// RH_IsFlammableBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.RH_IsFlammableBoundCheckEdit, "RH_IsFlammable");
			this.RH_IsFlammableBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RH_IsFlammableBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RH_IsFlammableBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 235, true);
			this.RH_IsFlammableBoundCheckEdit.Name = "RH_IsFlammableBoundCheckEdit";
			this.RH_IsFlammableBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 21, true);
			this.RH_IsFlammableBoundCheckEdit.TabIndex = 15;
			this.RH_IsFlammableBoundCheckEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsActiveCheckbox
			//
			this.BindingSource.SetBindingMember(this.IsActiveCheckbox, "RH_IsActive");
			this.IsActiveCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 203, true);
			this.IsActiveCheckbox.Name = "IsActiveCheckbox";
			this.IsActiveCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 24, true);
			this.IsActiveCheckbox.TabIndex = 12;
			this.IsActiveCheckbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsSystemCheckbox
			//
			this.BindingSource.SetBindingMember(this.IsSystemCheckbox, "RH_IsSystem");
			this.IsSystemCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("79FA5B52-212E-4B0E-885F-B314DDAC6225", "Is System");
			this.IsSystemCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckbox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsSystemCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(612, 12, true);
			this.IsSystemCheckbox.Name = "IsSystemCheckbox";
			this.IsSystemCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsSystemCheckbox.TabIndex = 2;
			this.IsSystemCheckbox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// NMFCCodeFindBox
			//
			this.NMFCCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NMFCCodeFindBox, "RH_FN_NKNMFC");
			this.NMFCCodeFindBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NMFCCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 14, true);
			this.NMFCCodeFindBox.Name = "NMFCCodeFindBox";
			this.NMFCCodeFindBox.PopupCaption = null;
			this.NMFCCodeFindBox.ShouldAddFetchHints = false;
			this.NMFCCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.NMFCCodeFindBox.TabIndex = 1;
			//
			// RH_ReeferMaxTemperatureBoundCalcEdit
			//
			this.BindingSource.SetBindingMember(this.RH_ReeferMaxTemperatureBoundCalcEdit, "RH_ReeferMaxTemperature");
			this.RH_ReeferMaxTemperatureBoundCalcEdit.CaptionResourceString = null;
			this.RH_ReeferMaxTemperatureBoundCalcEdit.DecimalPlaces = 2;
			this.RH_ReeferMaxTemperatureBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 177, true);
			this.RH_ReeferMaxTemperatureBoundCalcEdit.Name = "RH_ReeferMaxTemperatureBoundCalcEdit";
			this.RH_ReeferMaxTemperatureBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 20, true);
			this.RH_ReeferMaxTemperatureBoundCalcEdit.TabIndex = 11;
			this.RH_ReeferMaxTemperatureBoundCalcEdit.Text = "0.00";
			this.RH_ReeferMaxTemperatureBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// RH_ReeferMinTemperatureBoundCalcEdit
			//
			this.BindingSource.SetBindingMember(this.RH_ReeferMinTemperatureBoundCalcEdit, "RH_ReeferMinTemperature");
			this.RH_ReeferMinTemperatureBoundCalcEdit.CaptionResourceString = null;
			this.RH_ReeferMinTemperatureBoundCalcEdit.DecimalPlaces = 2;
			this.RH_ReeferMinTemperatureBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 177, true);
			this.RH_ReeferMinTemperatureBoundCalcEdit.Name = "RH_ReeferMinTemperatureBoundCalcEdit";
			this.RH_ReeferMinTemperatureBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 20, true);
			this.RH_ReeferMinTemperatureBoundCalcEdit.TabIndex = 10;
			this.RH_ReeferMinTemperatureBoundCalcEdit.Text = "0.00";
			this.RH_ReeferMinTemperatureBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// RH_ExpiryDateBoundDateEdit
			//
			this.RH_ExpiryDateBoundDateEdit.AllowDrop = true;
			this.RH_ExpiryDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.RH_ExpiryDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RH_ExpiryDateBoundDateEdit, "RH_ExpiryDate");
			this.RH_ExpiryDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 145, true);
			this.RH_ExpiryDateBoundDateEdit.Name = "RH_ExpiryDateBoundDateEdit";
			this.RH_ExpiryDateBoundDateEdit.TabIndex = 9;
			//
			// DegCLabel2
			//
			this.DegCLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|47aed5c9-522d-4ec3-a413-d56026096ae0", "° C");
			this.DegCLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DegCLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 177, true);
			this.DegCLabel2.Name = "DegCLabel2";
			this.DegCLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 21, true);
			this.DegCLabel2.TabIndex = 17;
			//
			// DegCLabel
			//
			this.DegCLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|9bb05057-625c-4c47-9505-8b8080b57415", "° C");
			this.DegCLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DegCLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 177, true);
			this.DegCLabel.Name = "DegCLabel";
			this.DegCLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 21, true);
			this.DegCLabel.TabIndex = 15;
			//
			// CommodityTypeLabel
			//
			this.CommodityTypeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|559CA216-FEA7-4070-9904-8AEDD60DB471", "Commodity Type:");
			this.CommodityTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommodityTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 113, true);
			this.CommodityTypeLabel.Name = "CommodityTypeLabel";
			this.CommodityTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.CommodityTypeLabel.TabIndex = 2;
			//
			// RH_ContainerVentRequiredBoundCheckEdit
			//
			this.BindingSource.SetBindingMember(this.RH_ContainerVentRequiredBoundCheckEdit, "RH_ContainerVentRequired");
			this.RH_ContainerVentRequiredBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RH_ContainerVentRequiredBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RH_ContainerVentRequiredBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 265, true);
			this.RH_ContainerVentRequiredBoundCheckEdit.Name = "RH_ContainerVentRequiredBoundCheckEdit";
			this.RH_ContainerVentRequiredBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 21, true);
			this.RH_ContainerVentRequiredBoundCheckEdit.TabIndex = 17;
			this.RH_ContainerVentRequiredBoundCheckEdit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsForwardingchCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsForwardingchCheckBox, "RH_IsForwarding");
			this.IsForwardingchCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsForwardingchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsForwardingchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 115, true);
			this.IsForwardingchCheckBox.Name = "IsForwardingchCheckBox";
			this.IsForwardingchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.IsForwardingchCheckBox.TabIndex = 5;
			this.IsForwardingchCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsShippingCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsShippingCheckBox, "RH_IsShipping");
			this.IsShippingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsShippingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsShippingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 115, true);
			this.IsShippingCheckBox.Name = "IsShippingCheckBox";
			this.IsShippingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.IsShippingCheckBox.TabIndex = 6;
			this.IsShippingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsLandTransportCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsLandTransportCheckBox, "RH_IsLandTransport");
			this.IsLandTransportCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsLandTransportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsLandTransportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 115, true);
			this.IsLandTransportCheckBox.Name = "IsLandTransportCheckBox";
			this.IsLandTransportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.IsLandTransportCheckBox.TabIndex = 7;
			this.IsLandTransportCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// IsPersonalEffectsCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsPersonalEffectsCheckBox, "RH_IsPersonalEffects");
			this.IsPersonalEffectsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPersonalEffectsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPersonalEffectsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 115, true);
			this.IsPersonalEffectsCheckBox.Name = "IsPersonalEffectsCheckBox";
			this.IsPersonalEffectsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.IsPersonalEffectsCheckBox.TabIndex = 8;
			this.IsPersonalEffectsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsPersonalEffectsCheckBox.Visible = false;
			//
			// UniversalGroup
			//
			this.UniversalGroup.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UniversalGroup, "RH_UniversalCommodityGroup");
			this.UniversalGroup.BindToList = "Lookups.UniversalCommodityCodeBizoList";
			this.UniversalGroup.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("64AAD421-C548-40F8-8F14-1236E607F65A", "Universal Group");
			this.UniversalGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 40, true);
			this.UniversalGroup.Name = "UniversalGroup";
			this.UniversalGroup.ShowDescriptionBox = false;
			this.UniversalGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.UniversalGroup.TabIndex = 3;
			//
			// IATACommodityItem
			//
			this.IATACommodityItem.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IATACommodityItem, "RH_IATACommodityItem");
			this.IATACommodityItem.BindToList = "Lookups.IATACommodityItemList";
			this.IATACommodityItem.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("429322a4-8312-46e3-beb4-fb05b153e8a1", "IATA Commodity");
			this.IATACommodityItem.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 40, true);
			this.IATACommodityItem.Name = "IATACommodityItem";
			this.IATACommodityItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.IATACommodityItem.TabIndex = 3;

			this.MainTabPage.PerformLayout();
			this.RH_DescriptionBoundText.ResumeLayout(true);
			this.RH_DescriptionBoundText.PerformLayout();
			this.NMFCCodeFindBox.ResumeLayout(true);
			this.NMFCCodeFindBox.PerformLayout();
			this.RH_ExpiryDateBoundDateEdit.ResumeLayout(true);
			this.RH_ExpiryDateBoundDateEdit.PerformLayout();
			this.UniversalGroup.ResumeLayout(true);
			this.UniversalGroup.PerformLayout();
			this.IATACommodityItem.ResumeLayout(true);
			this.IATACommodityItem.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void LocalCodesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			this.RefCommodityCodeMapGrid = new Enterprise.ZArchitecture.ZGrid();

			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LocalCodesTabPage.SuspendLayout();
			this.LocalCodesTabPage.Controls.Add(this.RefCommodityCodeMapGrid);
			((System.ComponentModel.ISupportInitialize)(this.RefCommodityCodeMapGrid)).BeginInit();
			this.RefCommodityCodeMapGrid.SuspendLayout();
			this.RefCommodityCodeMapGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RefCommodityCodeMapGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RefCommodityCodeMapGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RefCommodityCodeMapGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			//
			// RefLocoMapsGrid
			//
			this.RefCommodityCodeMapGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RefCommodityCodeMapGrid, "RefCommodityCodeMaps");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityCodeMaps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityCodeMaps)).SyncRoot)).LC_RN_NKCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityCodeMaps)).SyncRoot)).LC_LocalCodeProvider)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityCodeMaps)).SyncRoot)).LC_LocalCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityCodeMaps)).SyncRoot)).LC_RH_NKCommodityCode)));

			this.RefCommodityCodeMapGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|ae4e3609-34af-4c55-9d28-46547823b833", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LC_RN_NKCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|725502b6-affe-4d6c-8978-bb0443e82965", "Usage");
			zDropEditColumnStyleInfo1.ColumnName = "LC_LocalCodeProvider";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|f91a5a5a-ebab-4397-8152-d009b8274629", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "LC_LocalCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|6b7ee2f7-e222-475a-ba93-6c90cc79d5ac", "Commodity code");
			zTextBoxColumnStyleInfo2.ColumnName = "LC_RH_NKCommodityCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;

			this.RefCommodityCodeMapGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefCommodityCodeMapGrid.GridId = "41a8dcde-c5b6-462b-b751-8199c7c60ace";
			this.RefCommodityCodeMapGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RefCommodityCodeMapGrid.LayoutKey = "RefCommodityCodeMapGrid";
			this.RefCommodityCodeMapGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefCommodityCodeMapGrid.Name = "RefCommodityCodeMapGrid";
			this.RefCommodityCodeMapGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 402, true);
			this.RefCommodityCodeMapGrid.TabIndex = 0;
			this.LocalCodesTabPage.ResumeLayout(false);
			this.LocalCodesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RefCommodityCodeMapGrid)).EndInit();
			this.RefCommodityCodeMapGrid.ResumeLayout(false);
			this.RefCommodityCodeMapGrid.PerformLayout();
		}

		private void RatingCodesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			this.RatingCodeMapGrid = new Enterprise.ZArchitecture.ZGrid();

			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RatingCodesTabPage.SuspendLayout();
			this.RatingCodesTabPage.Controls.Add(this.RatingCodeMapGrid);
			((System.ComponentModel.ISupportInitialize)(this.RatingCodeMapGrid)).BeginInit();
			this.RatingCodeMapGrid.SuspendLayout();
			this.RatingCodeMapGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.RatingCodeMapGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RatingCodeMapGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			//
			// RatingCodeMapGrid
			//
			this.RatingCodeMapGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RatingCodeMapGrid, "RefCommodityRatingCodeMaps");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityRatingCodeMaps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityRatingCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityRatingCodeMaps)).SyncRoot)).CommodityChild.RH_DescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCommodityRatingCodeMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCommodityCode)(null)).RefCommodityRatingCodeMaps)).SyncRoot)).CommodityChild.RatingLocalCode.LC_LocalCode)));
			this.RatingCodeMapGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|79db4058-3210-4756-8a58-b1f8b6cef416", "Code");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "RI_RH_NKCommodityChild";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|b18ae20e-7a2b-4330-a2b0-6a04650ac219", "Local Code");
			zTextBoxColumnStyleInfo3.ColumnName = "CommodityChild+RatingLocalCode+LC_LocalCode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCommodityCodeForm|fee41b4c-1bda-413f-b65a-4cc099f9a848", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "CommodityChild+RH_DescriptionMultilingual";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.RatingCodeMapGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RatingCodeMapGrid.GridId = "81675938-bf3f-4577-bd61-acb33c228fbd";
			this.RatingCodeMapGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatingCodeMapGrid.LayoutKey = "RatingCodeMapGrid";
			this.RatingCodeMapGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RatingCodeMapGrid.Name = "RatingCodeMapGrid";
			this.RatingCodeMapGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 402, true);
			this.RatingCodeMapGrid.TabIndex = 0;
			this.RatingCodesTabPage.ResumeLayout(false);
			this.RatingCodesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatingCodeMapGrid)).EndInit();
			this.RatingCodeMapGrid.ResumeLayout(false);
			this.RatingCodeMapGrid.PerformLayout();
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

		private Enterprise.ZArchitecture.ZTextBox RH_CodeBoundText;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RH_IsTimberBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RH_IsPerishableBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RH_IsHazardousBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RH_IsFlammableBoundCheckEdit;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTranslatableTextControl RH_DescriptionBoundText;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckbox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsSystemCheckbox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage LocalCodesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage RatingCodesTabPage;
		protected Enterprise.ZArchitecture.ZGrid RefCommodityCodeMapGrid;
		protected Enterprise.ZArchitecture.ZGrid RatingCodeMapGrid;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsForwardingchCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsShippingCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsLandTransportCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox IsPersonalEffectsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RH_ContainerVentRequiredBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox UniversalGroup;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox IATACommodityItem;
		protected Enterprise.ZArchitecture.ZLabel DegCLabel;
		protected Enterprise.ZArchitecture.ZLabel DegCLabel2;
		protected Enterprise.ZArchitecture.ZLabel CommodityTypeLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit RH_ExpiryDateBoundDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RH_ReeferMinTemperatureBoundCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RH_ReeferMaxTemperatureBoundCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox NMFCCodeFindBox;
	}
}
