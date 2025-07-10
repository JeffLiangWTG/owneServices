namespace Enterprise.MasterFiles.GUI
{
	public partial class UserDefinedUserControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl UserDefinedTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DataTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CustomisationTabPage;
		protected Enterprise.ZArchitecture.ZGrid CustomLabelsGrid;
		protected Enterprise.MasterFiles.GUI.CustomLabelsUserControl UserDefinedCustomLabels;
		internal protected Enterprise.ZArchitecture.GUI.ZButton OrderStatusListEditButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OrderLineStatusListEditButton;
		internal Enterprise.ZArchitecture.GUI.ZButton DocumentLogoButton;
		Enterprise.ZArchitecture.ZGrid CustomisedDocumentsGrid;
		System.ComponentModel.IContainer components;
		protected Enterprise.ZArchitecture.ZLabel CustomisedDocsLabel;
		protected Enterprise.ZArchitecture.ZLabel CustomLabelsLabel;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UserDefinedTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DataTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UserDefinedCustomLabels = new Enterprise.MasterFiles.GUI.CustomLabelsUserControl();
			this.CustomisationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomisedDocsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomLabelsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DocumentLogoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrderLineStatusListEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrderStatusListEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CustomLabelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CustomisedDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UserDefinedTabControl.SuspendLayout();
			this.DataTabPage.SuspendLayout();
			this.UserDefinedCustomLabels.SuspendLayout();
			this.CustomisationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomLabelsGrid)).BeginInit();
			this.CustomLabelsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomisedDocumentsGrid)).BeginInit();
			this.CustomisedDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// UserDefinedTabControl
			// 
			this.UserDefinedTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.UserDefinedTabControl.Controls.Add(this.DataTabPage);
			this.UserDefinedTabControl.Controls.Add(this.CustomisationTabPage);
			this.UserDefinedTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UserDefinedTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.UserDefinedTabControl.Name = "UserDefinedTabControl";
			this.UserDefinedTabControl.SelectedIndex = 0;
			this.UserDefinedTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 460, true);
			this.UserDefinedTabControl.TabIndex = 0;
			// 
			// DataTabPage
			// 
			this.DataTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|be65a400-2ea7-410b-aaef-bb272662b5b1", "Data");
			this.DataTabPage.Controls.Add(this.UserDefinedCustomLabels);
			this.DataTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DataTabPage.Name = "DataTabPage";
			this.DataTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 433, true);
			this.DataTabPage.TabIndex = 0;
			// 
			// UserDefinedCustomLabels
			// 
			this.UserDefinedCustomLabels.BindToMember = "";
			this.UserDefinedCustomLabels.CustomLabelsProvider = null;
			this.UserDefinedCustomLabels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UserDefinedCustomLabels.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UserDefinedCustomLabels.Name = "UserDefinedCustomLabels";
			this.UserDefinedCustomLabels.PropertyNamesToExclude = new string[0];
			this.UserDefinedCustomLabels.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 433, true);
			this.UserDefinedCustomLabels.TabIndex = 0;
			// 
			// CustomisationTabPage
			// 
			this.CustomisationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|03b54113-bb65-4ba8-b355-dc1138ca8091", "Customization");
			this.CustomisationTabPage.Controls.Add(this.CustomisedDocsLabel);
			this.CustomisationTabPage.Controls.Add(this.CustomLabelsLabel);
			this.CustomisationTabPage.Controls.Add(this.DocumentLogoButton);
			this.CustomisationTabPage.Controls.Add(this.OrderLineStatusListEditButton);
			this.CustomisationTabPage.Controls.Add(this.OrderStatusListEditButton);
			this.CustomisationTabPage.Controls.Add(this.CustomLabelsGrid);
			this.CustomisationTabPage.Controls.Add(this.CustomisedDocumentsGrid);
			this.CustomisationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomisationTabPage.Name = "CustomisationTabPage";
			this.CustomisationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 433, true);
			this.CustomisationTabPage.TabIndex = 1;
			// 
			// CustomisedDocsLabel
			// 
			this.CustomisedDocsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CustomisedDocsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|6f59a2b8-39f7-442a-880c-c45ff5c96f45", "Customized Documents:");
			this.CustomisedDocsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 183, true);
			this.CustomisedDocsLabel.Name = "CustomisedDocsLabel";
			this.CustomisedDocsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.CustomisedDocsLabel.TabIndex = 19;
			// 
			// CustomLabelsLabel
			// 
			this.CustomLabelsLabel.AutoSize = true;
			this.CustomLabelsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|0a52da3b-a00f-4b11-a73b-a447175a54cb", "Customized Labels:");
			this.CustomLabelsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.CustomLabelsLabel.Name = "CustomLabelsLabel";
			this.CustomLabelsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.CustomLabelsLabel.TabIndex = 17;
			// 
			// DocumentLogoButton
			// 
			this.DocumentLogoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DocumentLogoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|d4f67059-42bf-40b5-afcb-604d0ea62c6d", "Document Logo");
			this.DocumentLogoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 8, true);
			this.DocumentLogoButton.Name = "DocumentLogoButton";
			this.DocumentLogoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 23, true);
			this.DocumentLogoButton.TabIndex = 0;
			this.DocumentLogoButton.Click += new System.EventHandler(this.DocumentLogoButton_Click);
			// 
			// OrderLineStatusListEditButton
			// 
			this.OrderLineStatusListEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OrderLineStatusListEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|818ad087-9159-477a-96ad-f767ad472d8a", "Order Line Status List");
			this.OrderLineStatusListEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 8, true);
			this.OrderLineStatusListEditButton.Name = "OrderLineStatusListEditButton";
			this.OrderLineStatusListEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 23, true);
			this.OrderLineStatusListEditButton.TabIndex = 2;
			this.OrderLineStatusListEditButton.Click += new System.EventHandler(this.OrderLineStatusListEditButton_Click);
			// 
			// OrderStatusListEditButton
			// 
			this.OrderStatusListEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OrderStatusListEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UserDefinedUserControl|35729484-6a08-4f79-9857-5b5ec43008e2", "Order Status List");
			this.OrderStatusListEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 8, true);
			this.OrderStatusListEditButton.Name = "OrderStatusListEditButton";
			this.OrderStatusListEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.OrderStatusListEditButton.TabIndex = 1;
			this.OrderStatusListEditButton.Click += new System.EventHandler(this.OrderStatusListEditButton_Click);
			// 
			// CustomLabelsGrid
			// 
			this.CustomLabelsGrid.AllowNavigation = false;
			this.CustomLabelsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomLabelsGrid, "CustomFormLabels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)).SyncRoot)).OT_Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)).SyncRoot)).OT_IsMandatory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)).SyncRoot)).OT_FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)).SyncRoot)).OT_Caption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomFormLabels)).SyncRoot)).OT_Hint)));
			this.CustomLabelsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OT_Position";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo1.ColumnName = "OT_IsMandatory";
			zCheckBoxColumnStyleInfo1.ToolTip = "This field must be entered in the related form.";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.ColumnName = "OT_FieldName";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			zTextBoxColumnStyleInfo1.ColumnName = "OT_Caption";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.ColumnName = "OT_Hint";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.CustomLabelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CustomLabelsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CustomLabelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomLabelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomLabelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomLabelsGrid.GridId = "b0601b4b-631b-4eb8-9275-464e322112fd";
			this.CustomLabelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomLabelsGrid.LayoutKey = "CustomLabelsGrid";
			this.CustomLabelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.CustomLabelsGrid.Name = "CustomLabelsGrid";
			this.CustomLabelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 140, true);
			this.CustomLabelsGrid.TabIndex = 3;
			// 
			// CustomisedDocumentsGrid
			// 
			this.CustomisedDocumentsGrid.AllowNavigation = false;
			this.CustomisedDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomisedDocumentsGrid, "CustomDocumentLabels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomDocumentLabels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomDocumentLabels)).SyncRoot)).OT_FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomDocumentLabels)).SyncRoot)).OT_Caption)));
			this.CustomisedDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "OT_FieldName";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "OT_Caption";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CustomisedDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CustomisedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomisedDocumentsGrid.GridId = "3b7f5607-8b9a-4788-a4a8-27b69725a2bb";
			this.CustomisedDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomisedDocumentsGrid.LayoutKey = "zGrid1";
			this.CustomisedDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 204, true);
			this.CustomisedDocumentsGrid.Name = "CustomisedDocumentsGrid";
			this.CustomisedDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 224, true);
			this.CustomisedDocumentsGrid.TabIndex = 5;
			// 
			// UserDefinedUserControl
			// 
			this.Controls.Add(this.UserDefinedTabControl);
			this.IsModifyCustom = true;
			this.Name = "UserDefinedUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 484, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.UserDefinedTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UserDefinedTabControl.ResumeLayout(false);
			this.UserDefinedTabControl.PerformLayout();
			this.DataTabPage.ResumeLayout(false);
			this.DataTabPage.PerformLayout();
			this.UserDefinedCustomLabels.ResumeLayout(true);
			this.UserDefinedCustomLabels.PerformLayout();
			this.CustomisationTabPage.ResumeLayout(false);
			this.CustomisationTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomLabelsGrid)).EndInit();
			this.CustomLabelsGrid.ResumeLayout(false);
			this.CustomLabelsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomisedDocumentsGrid)).EndInit();
			this.CustomisedDocumentsGrid.ResumeLayout(false);
			this.CustomisedDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
