using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.GUI
{
	partial class SendProductInformationRequestForm
	{
		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton DeliverButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox RecipientsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid RecipientsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MainPage;
		private System.ComponentModel.IContainer components;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoSupplier = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfoOrganization = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoName = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoDelivery = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.NonPersistentAddressOverrideColumnStyleInfo<Enterprise.Customs.Business.ProductInformationDeliveryContact> nonPersistentAddressOverrideColumnStyleInfoEmailAddress = new Enterprise.MasterFiles.GUI.NonPersistentAddressOverrideColumnStyleInfo<Enterprise.Customs.Business.ProductInformationDeliveryContact>();
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RecipientsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zDropEditSendFrom = new ZArchitecture.GUI.ZDropEdit();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zPanelDetails = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RecipientsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.RecipientsGrid.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainPage.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.zPanelDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 293, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 4, true);
			this.MainStatusBar.TabIndex = 2;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.ProductInformation);
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Deliver", "&Deliver");
			this.DeliverButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 0, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.DeliverButton.TabIndex = 3;
			this.DeliverButton.ToolTipCaption = null;
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Cancel", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 0, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// IndividualDocPackGroupBox
			// 
			this.RecipientsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Recipients", "Recipients");
			this.RecipientsGroupBox.Controls.Add(this.RecipientsGrid);
			this.RecipientsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RecipientsGroupBox.Name = "IndividualDocPackGroupBox";
			this.RecipientsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 191, true);
			this.RecipientsGroupBox.TabIndex = 0;
			this.RecipientsGroupBox.TabStop = false;
			// 
			// RecipientsGrid
			// 
			this.RecipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "DeliveryContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).OrgHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).DeliveryMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).DeliveryAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.FieldType)(((Enterprise.Customs.Business.ProductInformationDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.Business.ProductInformation)(null)).DeliveryContacts)).SyncRoot)).DeliveryMethodFieldType)));
			this.RecipientsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfoSupplier.ColumnName = "Supplier";
			zDropEditColumnStyleInfoSupplier.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfoSupplier.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfoOrganization.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Organization", "Organization");
			zGuidFindBoxColumnStyleInfoOrganization.ColumnName = "OrgHeaderPK";
			zGuidFindBoxColumnStyleInfoOrganization.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfoOrganization.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfoName.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Name", "Name");
			zDropEditColumnStyleInfoName.ColumnName = "Name";
			zDropEditColumnStyleInfoName.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfoName.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zDropEditColumnStyleInfoDelivery.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Delivery", "Delivery");
			zDropEditColumnStyleInfoDelivery.ColumnName = "DeliveryMethodDescription";
			zDropEditColumnStyleInfoDelivery.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfoDelivery.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.BindToDecimalPlaces = null;
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|EMailAddressFax", "E-Mail Address / Fax");
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.ColumnName = "DeliveryAddress";
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.DefaultCollectionIndex = 0;
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.EmailAddressPropertyName = "EmailAddress";
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.FieldTypeColumnName = "DeliveryMethodFieldType";
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.GetCopyRecipients = NonPersistentAddressOverrideColumnStyleInfoGetCopyRecipients;
			nonPersistentAddressOverrideColumnStyleInfoEmailAddress.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoSupplier);
			this.RecipientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfoOrganization);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoName);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoDelivery);
			this.RecipientsGrid.ColumnStyles.Add(nonPersistentAddressOverrideColumnStyleInfoEmailAddress);
			this.RecipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientsGrid.GridId = "5621944b-98e1-4440-bd23-1e3c69377537";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.LayoutKey = "RecipientsGrid";
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 174, true);
			this.RecipientsGrid.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MainPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 213, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainPage
			// 
			this.MainPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|Destination", "Destination");
			this.MainPage.Controls.Add(this.RecipientsGroupBox);
			this.MainPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainPage.Name = "MainPage";
			this.MainPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 191, true);
			this.MainPage.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.DeliverButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 297, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 22, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// zDropEditSendFrom
			// 
			this.BindingSource.SetBindingMember(this.zDropEditSendFrom, "SendFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ProductInformation)(null)).SendFrom)));
			this.zDropEditSendFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 8, true);
			this.zDropEditSendFrom.ShowDescriptionBox = false;
			this.zDropEditSendFrom.UseFullWidthForCodeBox = true;
			this.zDropEditSendFrom.Name = "zDropEditSendFrom";
			this.zDropEditSendFrom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.zDropEditSendFrom.TabIndex = 1;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.zPanelDetails);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.MainTabControl);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 297, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(78);
			this.kSplitContainer1.SplitterWidth = 9;
			this.kSplitContainer1.TabIndex = 4;
			// 
			// zPanelDetails
			// 
			this.zPanelDetails.Controls.Add(this.zDropEditSendFrom);
			this.zPanelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanelDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanelDetails.Name = "zPanelDetails";
			this.zPanelDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 78, true);
			this.zPanelDetails.TabIndex = 4;
			// 
			// SendProductInformationRequestForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("Enterprise.Customs.GUI.SendProductInformationRequestForm|SendProductInformationRequest", "Send Product Information Request");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 319, true);
			this.Controls.Add(this.kSplitContainer1);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.ProductInformation);
			this.DataSourceTypeName = "Enterprise.Customs.GUI.Declaration.ProductInformation";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 353, true);
			this.Name = "SendProductInformationRequestForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.kSplitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RecipientsGroupBox.ResumeLayout(false);
			this.RecipientsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.RecipientsGrid.ResumeLayout(false);
			this.RecipientsGrid.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPage.ResumeLayout(false);
			this.MainPage.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.zPanelDetails.ResumeLayout(false);
			this.zPanelDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private ZArchitecture.GUI.ZDropEdit zDropEditSendFrom;
		private KSplitContainer kSplitContainer1;
		private ZArchitecture.GUI.ZPanel zPanelDetails;
	}
}
