namespace Enterprise.MasterFiles.GUI
{
	public partial class DocAutoDeliveryContactsForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.ZGrid DocContactsGrid;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox RelatedPartyGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton AutoDeliverButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox MenuItemGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox ContactTypeTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ForeignPortFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LocalPortFindBox;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DocContactsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RelatedPartyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AutoDeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ForeignPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MenuItemGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContactTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocContactsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(436);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(437);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer);
			// 
			// DocContactsGrid
			// 
			this.DocContactsGrid.AllowNavigation = false;
			this.DocContactsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocContactsGrid, "DocContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).DeliveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContacts)).SyncRoot)).Phone)));
			this.DocContactsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|736a51fd-1b14-429a-83ad-f7ca50c4543e", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|99130298-6c81-44f2-b2ec-e3c719c589f1", "Method");
			zTextBoxColumnStyleInfo2.ColumnName = "DeliveryMethod";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|0791b8e4-a2c1-4d97-848d-f1ccbbb3bdcd", "Type", "Attachment Type", "");
			zTextBoxColumnStyleInfo3.ColumnName = "AttachmentType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|1e347cea-23ab-44c2-a9dc-0ce01a345f39", "Company Name");
			zTextBoxColumnStyleInfo4.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|665dc8b3-d4ed-4c01-9798-e8f72c5224fd", "Email");
			zTextBoxColumnStyleInfo5.ColumnName = "Email";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|78e88ff7-778b-4aa2-b509-0868b5f29468", "Address 1");
			zTextBoxColumnStyleInfo6.ColumnName = "Address1";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|b07740c8-c8f6-4e80-b492-a0cab60c1488", "Address 2");
			zTextBoxColumnStyleInfo7.ColumnName = "Address2";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|69269f0f-e4d2-4f42-87c4-67ba8c6d2713", "City");
			zTextBoxColumnStyleInfo8.ColumnName = "City";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|49f3ec8f-63a5-4aa2-a307-70d9526a9944", "State");
			zTextBoxColumnStyleInfo9.ColumnName = "State";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|fe5e4216-7d62-4804-a2c5-75475c140774", "Post Code");
			zTextBoxColumnStyleInfo10.ColumnName = "PostCode";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|3daa983b-73f7-4f25-b806-b04692a6be44", "Fax");
			zTextBoxColumnStyleInfo11.ColumnName = "Fax";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|6b7bee3b-b4b3-48cc-9ef1-6759ba0b944b", "Phone");
			zTextBoxColumnStyleInfo12.ColumnName = "Phone";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DocContactsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.DocContactsGrid.GridId = "9266a65b-e667-4540-a04d-49edc8d3b786";
			this.DocContactsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocContactsGrid.LayoutKey = "DocContactsGrid";
			this.DocContactsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.DocContactsGrid.Name = "DocContactsGrid";
			this.DocContactsGrid.ReadOnly = true;
			this.DocContactsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DocContactsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 195, true);
			this.DocContactsGrid.TabIndex = 11;
			// 
			// RelatedPartyGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.RelatedPartyGuidFindBox, "RelatedPartyGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).RelatedPartyGuid)));
			this.RelatedPartyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 52, true);
			this.RelatedPartyGuidFindBox.Name = "RelatedPartyGuidFindBox";
			this.RelatedPartyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.RelatedPartyGuidFindBox.TabIndex = 10;
			// 
			// AutoDeliverButton
			// 
			this.AutoDeliverButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AutoDeliverButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|8f1f1eab-bdf5-4178-95ba-2c97f2cd71dd", "View Contacts");
			this.AutoDeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 284, true);
			this.AutoDeliverButton.Name = "AutoDeliverButton";
			this.AutoDeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 21, true);
			this.AutoDeliverButton.TabIndex = 12;
			this.AutoDeliverButton.Click += new System.EventHandler(this.AutoDeliverButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|e707a7c2-7d45-469e-ab6f-e64c875a23aa", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(808, 284, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.CloseButton.TabIndex = 14;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ForeignPortFindBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignPortFindBox, "ForeignPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).ForeignPort)));
			this.ForeignPortFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|2ba2f4af-43e6-4bc8-9dc4-e946743c7cce", "Foreign Port");
			this.ForeignPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 30, true);
			this.ForeignPortFindBox.Name = "ForeignPortFindBox";
			this.ForeignPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ForeignPortFindBox.TabIndex = 9;
			// 
			// MenuItemGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.MenuItemGuidFindBox, "MenuItemGuid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).MenuItemGuid)));
			this.MenuItemGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|2f4eae93-ae9c-48fb-8847-ca44a0c523be", "Document");
			this.MenuItemGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 7, true);
			this.MenuItemGuidFindBox.Name = "MenuItemGuidFindBox";
			this.MenuItemGuidFindBox.PreBoundMaxLength = 50;
			this.MenuItemGuidFindBox.ShowDescriptionBox = false;
			this.MenuItemGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.MenuItemGuidFindBox.TabIndex = 1;
			// 
			// TransportModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|cb82e75d-b1dc-4422-a163-cdfed14c3b3a", "Transport Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 30, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.ShowDescriptionBox = false;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TransportModeDropEdit.TabIndex = 3;
			// 
			// ContactTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactTypeTextBox, "DocContactType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).DocContactType)));
			this.ContactTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|cec80014-9518-45b2-be12-b96b8e64f588", "Document Group");
			this.ContactTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 30, true);
			this.ContactTypeTextBox.Name = "ContactTypeTextBox";
			this.ContactTypeTextBox.ReadOnly = true;
			this.ContactTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ContactTypeTextBox.TabIndex = 5;
			// 
			// LocalPortFindBox
			// 
			this.BindingSource.SetBindingMember(this.LocalPortFindBox, "LocalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer)(null)).LocalPort)));
			this.LocalPortFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|7442b18c-7ffe-4cd9-be51-9407fd451489", "Local Port");
			this.LocalPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 7, true);
			this.LocalPortFindBox.Name = "LocalPortFindBox";
			this.LocalPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.LocalPortFindBox.TabIndex = 7;
			// 
			// DocAutoDeliveryContactsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 334, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAutoDeliveryContactsForm|9afb0203-50b1-4e92-984b-963e3c534d16", "Auto-Delivery Query Tool");
			this.Controls.Add(this.LocalPortFindBox);
			this.Controls.Add(this.ContactTypeTextBox);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.MenuItemGuidFindBox);
			this.Controls.Add(this.ForeignPortFindBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.AutoDeliverButton);
			this.Controls.Add(this.RelatedPartyGuidFindBox);
			this.Controls.Add(this.DocContactsGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocAutoDeliveryContactViewer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 327, true);
			this.Name = "DocAutoDeliveryContactsForm";
			this.Controls.SetChildIndex(this.DocContactsGrid, 0);
			this.Controls.SetChildIndex(this.RelatedPartyGuidFindBox, 0);
			this.Controls.SetChildIndex(this.AutoDeliverButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ForeignPortFindBox, 0);
			this.Controls.SetChildIndex(this.MenuItemGuidFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TransportModeDropEdit, 0);
			this.Controls.SetChildIndex(this.ContactTypeTextBox, 0);
			this.Controls.SetChildIndex(this.LocalPortFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocContactsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
