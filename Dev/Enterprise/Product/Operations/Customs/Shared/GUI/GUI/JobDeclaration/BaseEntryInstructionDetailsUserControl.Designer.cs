namespace Enterprise.Customs.GUI
{
	partial class BaseEntryInstructionDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BondHolderRemoverPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NewOwnerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.RemoverOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.BondHolderOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.FromWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ToWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.AssessmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.MainLowerPanel.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.OtherPartiesGroupBox.SuspendLayout();
			this.BondHolderRemoverPanel.SuspendLayout();
			this.NewOwnerOrganisationControl.SuspendLayout();
			this.RemoverOrganisationControl.SuspendLayout();
			this.BondHolderOrganisationControl.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseGroupBox.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.AssessmentDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CEI_Style";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("613955FB-790A-4F0F-985D-1214CA7AEE03", "Assessment Date");
			zDateEditColumnStyleInfo1.ColumnName = "CEI_DateForDuty";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "ad340f7f-fd6b-4196-998c-ee4c975da660";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 80, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.EntryInstructionsGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.Controls.Add(this.MainLowerPanel);
			this.splitContainer1.Panel2.Controls.Add(this.DetailsPanel);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			this.splitContainer1.TabIndex = 3;
			// 
			// MainLowerPanel
			// 
			this.MainLowerPanel.Controls.Add(this.OtherPartiesDetailsPanel);
			this.MainLowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.MainLowerPanel.Name = "MainLowerPanel";
			this.MainLowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 259, true);
			this.MainLowerPanel.TabIndex = 1;
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.OtherPartiesGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 259, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// OtherPartiesGroupBox
			// 
			this.OtherPartiesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("79d1b01b-2842-422a-8f57-c526f9b002aa", "Other Parties");
			this.OtherPartiesGroupBox.Controls.Add(this.BondHolderRemoverPanel);
			this.OtherPartiesGroupBox.Controls.Add(this.FromWarehouseGroupBox);
			this.OtherPartiesGroupBox.Controls.Add(this.ToWarehouseGroupBox);
			this.OtherPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.OtherPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesGroupBox.Name = "OtherPartiesGroupBox";
			this.OtherPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 259, true);
			this.OtherPartiesGroupBox.TabIndex = 1;
			this.OtherPartiesGroupBox.TabStop = false;
			// 
			// BondHolderRemoverPanel
			// 
			this.BondHolderRemoverPanel.Controls.Add(this.NewOwnerOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.RemoverOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.BondHolderOrganisationControl);
			this.BondHolderRemoverPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BondHolderRemoverPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
			this.BondHolderRemoverPanel.Name = "BondHolderRemoverPanel";
			this.BondHolderRemoverPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 80, true);
			this.BondHolderRemoverPanel.TabIndex = 2;
			// 
			// NewOwnerOrganisationControl
			// 
			this.NewOwnerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CustomsEntryInstructions.CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Owner)));
			this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("82156dbf-c193-4b04-97df-2e8c8fb8cfa6", "New Owner");
			this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
			this.NewOwnerOrganisationControl.PopupCaption = "";
			this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.NewOwnerOrganisationControl.TabIndex = 3;
			// 
			// RemoverOrganisationControl
			// 
			this.RemoverOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CustomsEntryInstructions.CEI_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Carrier)));
			this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
			this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ff90ded1-6a45-4b2c-971e-e23c30810363", "Remover");
			this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.RemoverOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 0, true);
			this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
			this.RemoverOrganisationControl.PopupCaption = "";
			this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.RemoverOrganisationControl.TabIndex = 2;
			// 
			// BondHolderOrganisationControl
			// 
			this.BondHolderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CustomsEntryInstructions.CEI_OH_BondHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_BondHolder)));
			this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e10a8e47-2f22-489c-a473-f0de93b18614", "Bond Holder");
			this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.BondHolderOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
			this.BondHolderOrganisationControl.PopupCaption = "";
			this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.BondHolderOrganisationControl.TabIndex = 1;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("cc9fd95c-990f-4073-b8fb-c1416f696639", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 57, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 41, true);
			this.FromWarehouseGroupBox.TabIndex = 4;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("22a73331-b05d-4aaf-947d-2fc217daafc6", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.FromWarehouseCodeTextBox.TabIndex = 3;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FromWarehouseAddressControl, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 22, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// ToWarehouseGroupBox
			// 
			this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3D4E3B55-5AA1-47BD-A5BE-544092411E33", "To Warehouse");
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
			this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
			this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 41, true);
			this.ToWarehouseGroupBox.TabIndex = 5;
			this.ToWarehouseGroupBox.TabStop = false;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4BEFFE03-6E6A-4753-9C0D-549648C26B77", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 22, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Controls.Add(this.DetailsGroupBox);
			this.DetailsPanel.Controls.Add(this.DetailsUserControl);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 42, true);
			this.DetailsPanel.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("09EA707F-BDF6-49B7-923D-4FF70EA1FAE8", "Details");
			this.DetailsGroupBox.Controls.Add(this.CPCDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AssessmentDateEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 42, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "CustomsEntryInstructions.CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 16, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.ShouldResizeByMaxLength = true;
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CPCDropEdit.TabIndex = 0;
			// 
			// AssessmentDateEdit
			// 
			this.AssessmentDateEdit.AllowDrop = true;
			this.AssessmentDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AssessmentDateEdit, "CustomsEntryInstructions.CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			this.AssessmentDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("27F5ADA6-5596-4C6F-95D2-BD14D5966D0E", "Assessment Date");
			this.AssessmentDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AssessmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 16, true);
			this.AssessmentDateEdit.Name = "AssessmentDateEdit";
			this.AssessmentDateEdit.TabIndex = 1;
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 42, true);
			this.DetailsUserControl.TabIndex = 0;
			// 
			// BaseEntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "BaseEntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.MainLowerPanel.ResumeLayout(false);
			this.MainLowerPanel.PerformLayout();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.OtherPartiesGroupBox.ResumeLayout(false);
			this.OtherPartiesGroupBox.PerformLayout();
			this.BondHolderRemoverPanel.ResumeLayout(false);
			this.BondHolderRemoverPanel.PerformLayout();
			this.NewOwnerOrganisationControl.ResumeLayout(true);
			this.NewOwnerOrganisationControl.PerformLayout();
			this.RemoverOrganisationControl.ResumeLayout(true);
			this.RemoverOrganisationControl.PerformLayout();
			this.BondHolderOrganisationControl.ResumeLayout(true);
			this.BondHolderOrganisationControl.PerformLayout();
			this.FromWarehouseGroupBox.ResumeLayout(false);
			this.FromWarehouseGroupBox.PerformLayout();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ToWarehouseGroupBox.ResumeLayout(false);
			this.ToWarehouseGroupBox.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.AssessmentDateEdit.ResumeLayout(true);
			this.AssessmentDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.ZGrid EntryInstructionsGrid;
		protected CargoWise.Windows.UI.KSplitContainer splitContainer1;
		protected ZArchitecture.GUI.ZGroupBox OtherPartiesGroupBox;
		protected ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		protected ZArchitecture.GUI.ZGroupBox ToWarehouseGroupBox;
		protected ZArchitecture.GUI.ZAddressControl ToWarehouseAddressControl;
		protected ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		protected ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		protected ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;
		protected ZArchitecture.GUI.ZAddressControl FromWarehouseAddressControl;
		protected MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
		protected MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
		protected MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
		protected ZArchitecture.GUI.ZPanel BondHolderRemoverPanel;
		protected ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected ZArchitecture.GUI.ZDateEdit AssessmentDateEdit;
		protected ZArchitecture.GUI.ZPanel MainLowerPanel;
		protected ZArchitecture.GUI.ZPanel DetailsPanel;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth CPCDropEdit;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DetailsUserControl;
	}
}
