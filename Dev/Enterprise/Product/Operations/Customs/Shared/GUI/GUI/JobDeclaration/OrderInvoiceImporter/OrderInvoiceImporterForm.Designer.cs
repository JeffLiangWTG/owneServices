namespace Enterprise.Customs.GUI
{
	partial class OrderInvoiceImporterForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.OrderedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.InvoicedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ReceivedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OrdersToImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrdersToImportGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReconcileSelectedOrdersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoideDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceLinesTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrdersToImportGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersToImportGrid.InnerGrid)).BeginInit();
			this.OrdersToImportGrid.SuspendLayout();
			this.InvoideDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 386, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 0, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 11;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(357);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(357);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.OrderInvoiceImporter);
			// 
			// OrderedRadioButton
			// 
			this.OrderedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.OrderedRadioButton, "IsOrderedQuantityImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).IsOrderedQuantityImport)));
			this.OrderedRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B9C11B46-5DA6-4A78-8E55-63BD3AC4D3F6", "Ordered");
			this.OrderedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OrderedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 301, true);
			this.OrderedRadioButton.Name = "OrderedRadioButton";
			this.OrderedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.OrderedRadioButton.TabIndex = 2;
			// 
			// InvoicedRadioButton
			// 
			this.InvoicedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.InvoicedRadioButton, "IsInvoicedQuantityImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).IsInvoicedQuantityImport)));
			this.InvoicedRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1C9F14D1-0778-4E42-BDBF-0F15B8986F43", "Invoiced");
			this.InvoicedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InvoicedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 301, true);
			this.InvoicedRadioButton.Name = "InvoicedRadioButton";
			this.InvoicedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.InvoicedRadioButton.TabIndex = 3;
			// 
			// ReceivedRadioButton
			// 
			this.ReceivedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ReceivedRadioButton, "IsReceivedQuantityImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).IsReceivedQuantityImport)));
			this.ReceivedRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4ED26E77-2C49-4B66-A62D-CDDB647A74D4", "Received");
			this.ReceivedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReceivedRadioButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceivedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 301, true);
			this.ReceivedRadioButton.Name = "ReceivedRadioButton";
			this.ReceivedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.ReceivedRadioButton.TabIndex = 4;
			// 
			// OrdersToImportGroupBox
			// 
			this.OrdersToImportGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3B807A1D-32E0-40E0-9035-4EA147D10940", "Orders To Import");
			this.OrdersToImportGroupBox.Controls.Add(this.OrdersToImportGrid);
			this.OrdersToImportGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrdersToImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersToImportGroupBox.Name = "OrdersToImportGroupBox";
			this.OrdersToImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 289, true);
			this.OrdersToImportGroupBox.TabIndex = 0;
			this.OrdersToImportGroupBox.TabStop = false;
			// 
			// OrdersToImportGrid
			// 
			this.OrdersToImportGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrdersToImportGrid, "OrdersToImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).OrdersToImport)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("49652B8D-9D9B-44A7-868E-B0698909EE25", "Order Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B75A542C-CF8A-4DC6-99B7-B3C6259E76BE", "Invoice Number");
			zTextBoxColumnStyleInfo2.ColumnName = "JD_InvoiceNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("014B86F6-E6F5-4182-A12E-B9BC09501ACE", "Invoice Date");
			zDateEditColumnStyleInfo1.ColumnName = "JD_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "SupplierList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F6F92AF3-9625-462C-8158-903706461602", "Supplier");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SupplierPK";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo2.BindToList = "BuyerList";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("448C4019-4660-4E87-B37A-A572346BFBCC", "Buyer");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BuyerPK";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("74DF1EEB-A602-4289-BDA8-0E935687375F", "Order Status");
			zTextBoxColumnStyleInfo3.ColumnName = "JD_OrderStatus";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("450A1448-BBA9-4FC6-9D15-3A7BC3647755", "Mode");
			zTextBoxColumnStyleInfo4.ColumnName = "JD_TransportMode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("55998EBF-9F42-4BD1-BA6A-194AF19F1450", "Est. Arrival");
			zDateEditColumnStyleInfo2.ColumnName = "JD_Milestone_E_ARV";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrdersToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrdersToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrdersToImportGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrdersToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrdersToImportGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OrdersToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrdersToImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrdersToImportGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OrdersToImportGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersToImportGrid.GridId = "369e94e8-151f-4a19-9d68-f81298a8b98a";
			// 
			// 
			// 
			this.OrdersToImportGrid.InnerGrid.AllowCopyToNewRowMenuItem = false;
			this.OrdersToImportGrid.InnerGrid.AllowNavigation = false;
			this.OrdersToImportGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OrdersToImportGrid.InnerGrid.CaptionVisible = false;
			this.OrdersToImportGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdersToImportGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.OrdersToImportGrid.InnerGrid.LayoutKey = "Grid";
			this.OrdersToImportGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrdersToImportGrid.InnerGrid.Name = "Grid";
			this.OrdersToImportGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OrdersToImportGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 232, true);
			this.OrdersToImportGrid.InnerGrid.TabIndex = 0;
			this.OrdersToImportGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrdersToImportGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Orders;
			this.OrdersToImportGrid.Name = "OrdersToImportGrid";
			this.OrdersToImportGrid.NameOfAGridElement = Enterprise.Customs.GUI.Res.GetData("F1967BA6-C876-4824-85FB-77F222168546", "Order");
			this.OrdersToImportGrid.ReadOnly = false;
			this.OrdersToImportGrid.ShowAttachButton = false;
			this.OrdersToImportGrid.ShowDetachButton = false;
			this.OrdersToImportGrid.ShowNewButton = false;
			this.OrdersToImportGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 270, true);
			this.OrdersToImportGrid.TabIndex = 0;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8A887B2F-10F1-4C5E-AF7D-4F183341A1FA", "Select All");
			this.SelectAllButton.IsCaptionOverridden = false;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 349, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 24, true);
			this.SelectAllButton.TabIndex = 8;
			this.SelectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// CloseImportButton
			// 
			this.CloseImportButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F131AB07-69F0-44F9-8B36-C16B8F054051", "Close");
			this.CloseImportButton.IsCaptionOverridden = false;
			this.CloseImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 349, true);
			this.CloseImportButton.Name = "CloseImportButton";
			this.CloseImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 24, true);
			this.CloseImportButton.TabIndex = 10;
			this.CloseImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseImportButton.ToolTipCaption = null;
			this.CloseImportButton.Click += new System.EventHandler(this.CloseImportButton_Click);
			// 
			// ReconcileSelectedOrdersButton
			// 
			this.ReconcileSelectedOrdersButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("96C744C7-F763-4F3F-B9FE-A7B232B62DF1", "Reconcile Selected Orders");
			this.ReconcileSelectedOrdersButton.IsCaptionOverridden = false;
			this.ReconcileSelectedOrdersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 349, true);
			this.ReconcileSelectedOrdersButton.Name = "ReconcileSelectedOrdersButton";
			this.ReconcileSelectedOrdersButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ReconcileSelectedOrdersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 24, true);
			this.ReconcileSelectedOrdersButton.TabIndex = 9;
			this.ReconcileSelectedOrdersButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ReconcileSelectedOrdersButton.ToolTipCaption = null;
			this.ReconcileSelectedOrdersButton.Click += new System.EventHandler(this.ReconcileSelectedOrdersButton_Click);
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).InvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("09b1297f-40a5-46e9-b7ba-f196a68182eb", "Invoice Number");
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 327, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InvoiceNumberTextBox.TabIndex = 5;
			// 
			// InvoideDateDateEdit
			// 
			this.InvoideDateDateEdit.AllowDrop = true;
			this.InvoideDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoideDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoideDateDateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.OrderInvoiceImporter)(null)).InvoiceDate)));
			this.InvoideDateDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("80bbbd86-282f-4f46-983b-5c7467320b71", "Invoice Date");
			this.InvoideDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 353, true);
			this.InvoideDateDateEdit.Name = "InvoideDateDateEdit";
			this.InvoideDateDateEdit.TabIndex = 6;
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DB70864E-DED0-4BB4-B1EF-1F49BE50C946", "Update selected");
			this.UpdateButton.IsCaptionOverridden = false;
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 350, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 24, true);
			this.UpdateButton.TabIndex = 7;
			this.UpdateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UpdateButton.ToolTipCaption = null;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// InvoiceLinesTotalLabel
			// 
			this.InvoiceLinesTotalLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fcc0d38d-1cea-4a9e-ac4e-869efbbf091c", "Qty To Invoice");
			this.InvoiceLinesTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InvoiceLinesTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 301, true);
			this.InvoiceLinesTotalLabel.Name = "InvoiceLinesTotalLabel";
			this.InvoiceLinesTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.InvoiceLinesTotalLabel.TabIndex = 1;
			this.InvoiceLinesTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// OrderInvoiceImporterForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2E3245AB-0732-4AE4-97B4-F002F13552AB", "Import Order Lines");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 386, true);
			this.Controls.Add(this.InvoiceLinesTotalLabel);
			this.Controls.Add(this.OrderedRadioButton);
			this.Controls.Add(this.InvoicedRadioButton);
			this.Controls.Add(this.ReceivedRadioButton);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.InvoideDateDateEdit);
			this.Controls.Add(this.InvoiceNumberTextBox);
			this.Controls.Add(this.ReconcileSelectedOrdersButton);
			this.Controls.Add(this.CloseImportButton);
			this.Controls.Add(this.SelectAllButton);
			this.Controls.Add(this.OrdersToImportGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.OrderInvoiceImporter);
			this.DataSourceTypeName = "Enterprise.Customs.Business.OrderInvoiceImporter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "OrderInvoiceImporterForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OrdersToImportGroupBox, 0);
			this.Controls.SetChildIndex(this.SelectAllButton, 0);
			this.Controls.SetChildIndex(this.CloseImportButton, 0);
			this.Controls.SetChildIndex(this.ReconcileSelectedOrdersButton, 0);
			this.Controls.SetChildIndex(this.InvoiceNumberTextBox, 0);
			this.Controls.SetChildIndex(this.InvoideDateDateEdit, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.ReceivedRadioButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoicedRadioButton, 0);
			this.Controls.SetChildIndex(this.OrderedRadioButton, 0);
			this.Controls.SetChildIndex(this.InvoiceLinesTotalLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrdersToImportGroupBox.ResumeLayout(false);
			this.OrdersToImportGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersToImportGrid.InnerGrid)).EndInit();
			this.OrdersToImportGrid.ResumeLayout(true);
			this.OrdersToImportGrid.PerformLayout();
			this.InvoideDateDateEdit.ResumeLayout(true);
			this.InvoideDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox OrdersToImportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton SelectAllButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton InvoicedRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton OrderedRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ReceivedRadioButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseImportButton;
		protected Enterprise.ZArchitecture.GUI.ZModuleButtonGrid OrdersToImportGrid;
		private Enterprise.ZArchitecture.GUI.ZButton ReconcileSelectedOrdersButton;
		private Enterprise.ZArchitecture.ZTextBox InvoiceNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit InvoideDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateButton;
		private ZArchitecture.ZLabel InvoiceLinesTotalLabel;
	}
}
