namespace Enterprise.Customs.US.InBond.GUI
{
	partial class CreateInBondForConsolForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.ShipmentsWithoutInBondGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentsForInBondGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewMovementHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewMovementHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AllocatedShipmentsForMovementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AllocatedShipmentsForMovementGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GaveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentsWithoutInBondGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsForInBondGrid)).BeginInit();
			this.ShipmentsForInBondGrid.SuspendLayout();
			this.NewMovementHeadersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NewMovementHeadersGrid)).BeginInit();
			this.NewMovementHeadersGrid.SuspendLayout();
			this.AllocatedShipmentsForMovementGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedShipmentsForMovementGrid)).BeginInit();
			this.AllocatedShipmentsForMovementGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 577, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper);
			// 
			// ShipmentsWithoutInBondGroupBox
			// 
			this.ShipmentsWithoutInBondGroupBox.Controls.Add(this.ShipmentsForInBondGrid);
			this.ShipmentsWithoutInBondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.ShipmentsWithoutInBondGroupBox.Name = "ShipmentsWithoutInBondGroupBox";
			this.ShipmentsWithoutInBondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 289, true);
			this.ShipmentsWithoutInBondGroupBox.TabIndex = 0;
			this.ShipmentsWithoutInBondGroupBox.TabStop = false;
			this.ShipmentsWithoutInBondGroupBox.Text = "Shipments Without In-Bond";
			// 
			// ShipmentsForInBondGrid
			// 
			this.ShipmentsForInBondGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipmentsForInBondGrid, "ShipmentsWithoutInBond");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).ShipmentsWithoutInBond)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).ShipmentsWithoutInBond)).SyncRoot)).ShipmentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).ShipmentsWithoutInBond)).SyncRoot)).HouseBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).ShipmentsWithoutInBond)).SyncRoot)).ConsigneeOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).ShipmentsWithoutInBond)).SyncRoot)).ConsigneeOrganizationAddress)));
			this.ShipmentsForInBondGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ShipmentNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "HouseBillNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ConsigneeOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("32345296-120B-42F4-A352-8242FF72187E", "Consignee Organization");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zAddressDropEditColumnStyleInfo1.ColumnName = "ConsigneeOrganizationAddress";
			zAddressDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("32345296-120B-42F4-A352-8242FF72187E", "Consignee Organization");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ShipmentsForInBondGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsForInBondGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentsForInBondGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ShipmentsForInBondGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.ShipmentsForInBondGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsForInBondGrid.GridId = "55c9dc12-5634-4b43-899e-261a9d854594";
			this.ShipmentsForInBondGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsForInBondGrid.LayoutKey = "ShipmentsForInBondGrid";
			this.ShipmentsForInBondGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ShipmentsForInBondGrid.Name = "ShipmentsForInBondGrid";
			this.ShipmentsForInBondGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 270, true);
			this.ShipmentsForInBondGrid.TabIndex = 0;
			// 
			// NewMovementHeadersGroupBox
			// 
			this.NewMovementHeadersGroupBox.Controls.Add(this.NewMovementHeadersGrid);
			this.NewMovementHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 311, true);
			this.NewMovementHeadersGroupBox.Name = "NewMovementHeadersGroupBox";
			this.NewMovementHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 236, true);
			this.NewMovementHeadersGroupBox.TabIndex = 4;
			this.NewMovementHeadersGroupBox.TabStop = false;
			this.NewMovementHeadersGroupBox.Text = "New Movement Headers";
			// 
			// NewMovementHeadersGrid
			// 
			this.NewMovementHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NewMovementHeadersGrid, "NewMovementHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).InBondNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).CarrierOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).InBondCarrierAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).Destination)));
			this.NewMovementHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "InBondNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo1.ColumnName = "EntryType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CarrierOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("C3826EC1-844C-4879-AE42-F73BAF01A0EB", "Carrier Organization");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zAddressDropEditColumnStyleInfo2.ColumnName = "InBondCarrierAddress";
			zAddressDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("C3826EC1-844C-4879-AE42-F73BAF01A0EB", "Carrier Organization");
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Destination";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.NewMovementHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NewMovementHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NewMovementHeadersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.NewMovementHeadersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.NewMovementHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.NewMovementHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewMovementHeadersGrid.GridId = "26AD49B2-A5CE-47BA-8324-60FB387226A4";
			this.NewMovementHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewMovementHeadersGrid.LayoutKey = "NewMovementHeadersGrid";
			this.NewMovementHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NewMovementHeadersGrid.Name = "NewMovementHeadersGrid";
			this.NewMovementHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 217, true);
			this.NewMovementHeadersGrid.TabIndex = 0;
			// 
			// AllocatedShipmentsForMovementGroupBox
			// 
			this.AllocatedShipmentsForMovementGroupBox.Controls.Add(this.AllocatedShipmentsForMovementGrid);
			this.AllocatedShipmentsForMovementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 2, true);
			this.AllocatedShipmentsForMovementGroupBox.Name = "AllocatedShipmentsForMovementGroupBox";
			this.AllocatedShipmentsForMovementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 289, true);
			this.AllocatedShipmentsForMovementGroupBox.TabIndex = 3;
			this.AllocatedShipmentsForMovementGroupBox.TabStop = false;
			// 
			// AllocatedShipmentsForMovementGrid
			// 
			this.AllocatedShipmentsForMovementGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AllocatedShipmentsForMovementGrid, "NewMovementHeaders.AllocatedShipmentsForMovement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).AllocatedShipmentsForMovement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).AllocatedShipmentsForMovement)).SyncRoot)).ShipmentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).AllocatedShipmentsForMovement)).SyncRoot)).HouseBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).AllocatedShipmentsForMovement)).SyncRoot)).ConsigneeOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.InBond.Business.CusInBondShipmentWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.MovementHeaderWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper)(null)).NewMovementHeaders)).SyncRoot)).AllocatedShipmentsForMovement)).SyncRoot)).ConsigneeOrganizationAddress)));
			this.AllocatedShipmentsForMovementGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "ShipmentNumber";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.ColumnName = "HouseBillNumber";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "ConsigneeOrgPK";
			zOrganisationFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("32345296-120B-42F4-A352-8242FF72187E", "Consignee Organization");
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zAddressDropEditColumnStyleInfo3.ColumnName = "ConsigneeOrganizationAddress";
			zAddressDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.InBond.GUI.Res.GetData("32345296-120B-42F4-A352-8242FF72187E", "Consignee Organization");
			zAddressDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.AllocatedShipmentsForMovementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AllocatedShipmentsForMovementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AllocatedShipmentsForMovementGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.AllocatedShipmentsForMovementGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo3);
			this.AllocatedShipmentsForMovementGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocatedShipmentsForMovementGrid.GridId = "6a8e4f40-77f4-4651-ab4a-58643a4e158d";
			this.AllocatedShipmentsForMovementGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllocatedShipmentsForMovementGrid.LayoutKey = "AllocatedShipmentsForMovementGrid";
			this.AllocatedShipmentsForMovementGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AllocatedShipmentsForMovementGrid.Name = "AllocatedShipmentsForMovementGrid";
			this.AllocatedShipmentsForMovementGrid.ReadOnly = true;
			this.AllocatedShipmentsForMovementGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 270, true);
			this.AllocatedShipmentsForMovementGrid.TabIndex = 0;
			// 
			// AddButton
			// 
			this.AddButton.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("B053D816-0F15-4DE5-A7F7-2A8E8A3B6DA9", "&Add →");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 106, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 30, true);
			this.AddButton.TabIndex = 1;
			this.AddButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddButton.ToolTipCaption = null;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("F85626C4-3F74-4AF5-99B2-5C9D9ECC5D34", "← &Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 165, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 30, true);
			this.RemoveButton.TabIndex = 2;
			this.RemoveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RemoveButton.ToolTipCaption = null;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// GaveUpButton
			// 
			this.GaveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GaveUpButton.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("DC81735F-309B-428A-A26F-E391E7E75769", "Cancel");
			this.GaveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GaveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1066, 549, true);
			this.GaveUpButton.Name = "GaveUpButton";
			this.GaveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GaveUpButton.TabIndex = 6;
			this.GaveUpButton.ToolTipCaption = null;
			this.GaveUpButton.UseVisualStyleBackColor = true;
			this.GaveUpButton.Click += new System.EventHandler(this.GaveUpButton_Click);
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateButton.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("FDF639D6-3326-4590-9FC0-F7C08CABD100", "Create");
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(967, 549, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CreateButton.TabIndex = 5;
			this.CreateButton.ToolTipCaption = null;
			this.CreateButton.UseVisualStyleBackColor = true;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// CreateInBondForConsolForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.GaveUpButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 601, true);
			this.Controls.Add(this.CreateButton);
			this.Controls.Add(this.GaveUpButton);
			this.Controls.Add(this.RemoveButton);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.AllocatedShipmentsForMovementGroupBox);
			this.Controls.Add(this.NewMovementHeadersGroupBox);
			this.Controls.Add(this.ShipmentsWithoutInBondGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CreateInBondForConsolWraper);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "CreateInBondForConsolForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ShipmentsWithoutInBondGroupBox, 0);
			this.Controls.SetChildIndex(this.NewMovementHeadersGroupBox, 0);
			this.Controls.SetChildIndex(this.AllocatedShipmentsForMovementGroupBox, 0);
			this.Controls.SetChildIndex(this.AddButton, 0);
			this.Controls.SetChildIndex(this.RemoveButton, 0);
			this.Controls.SetChildIndex(this.GaveUpButton, 0);
			this.Controls.SetChildIndex(this.CreateButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentsWithoutInBondGroupBox.ResumeLayout(false);
			this.ShipmentsWithoutInBondGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsForInBondGrid)).EndInit();
			this.ShipmentsForInBondGrid.ResumeLayout(false);
			this.ShipmentsForInBondGrid.PerformLayout();
			this.NewMovementHeadersGroupBox.ResumeLayout(false);
			this.NewMovementHeadersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NewMovementHeadersGrid)).EndInit();
			this.NewMovementHeadersGrid.ResumeLayout(false);
			this.NewMovementHeadersGrid.PerformLayout();
			this.AllocatedShipmentsForMovementGroupBox.ResumeLayout(false);
			this.AllocatedShipmentsForMovementGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedShipmentsForMovementGrid)).EndInit();
			this.AllocatedShipmentsForMovementGrid.ResumeLayout(false);
			this.AllocatedShipmentsForMovementGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ShipmentsWithoutInBondGroupBox;
		internal ZArchitecture.ZGrid ShipmentsForInBondGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox NewMovementHeadersGroupBox;
		internal ZArchitecture.ZGrid NewMovementHeadersGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AllocatedShipmentsForMovementGroupBox;
		internal ZArchitecture.ZGrid AllocatedShipmentsForMovementGrid;
		private ZArchitecture.GUI.ZButton GaveUpButton;
		private ZArchitecture.GUI.ZButton CreateButton;
		private ZArchitecture.GUI.ZButton AddButton;
		private ZArchitecture.GUI.ZButton RemoveButton;
	}
}
