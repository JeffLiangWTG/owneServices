using CargoWise.Types;
namespace Enterprise.Freight.Module
{
	partial class SchedulesForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Schedules = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SchedulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Schedules.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SchedulesGrid)).BeginInit();
			this.ConsolGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 449, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.BulkCopyCriteria);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|ce23bf1c-06f6-42e8-8a2a-7850bb72c754", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 420, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|86c845e1-8385-48ea-a7d9-a006e960f49c", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 420, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 4;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// Schedules
			// 
			this.Schedules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.Schedules.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|af8751c8-bc7f-43ab-afd6-292d0164da1b", "Schedules");
			this.Schedules.Controls.Add(this.SchedulesGrid);
			this.Schedules.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.Schedules.Name = "Schedules";
			this.Schedules.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 201, true);
			this.Schedules.TabIndex = 6;
			this.Schedules.TabStop = false;
			this.Schedules.Text = "Schedules";
			// 
			// SchedulesGrid
			// 
			this.SchedulesGrid.AllowNavigation = false;
			this.SchedulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SchedulesGrid, "Schedules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JV_VoyageFlight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JB_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JA_DocumentaryCutoff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_DepotCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_JB_CTOStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_DepotReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_DepotStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.BaseJobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).Schedules)).SyncRoot)).JX_DepotAvailabilityDate)));
			this.SchedulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|a3937bf5-a19a-414f-b73b-212b9253cee1", "Flight");
			zTextBoxColumnStyleInfo1.ColumnName = "JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|94847d6c-8d72-4072-8350-c80cb83a1253", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JX_JA_E_DEP";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|d521a389-346b-4dee-8763-39bd5460a0bf", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "JX_JB_E_ARV";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo1.ColumnName = "JX_IsPublished";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|ef9962f4-ad73-45b7-babb-e42ecb6d5347", "Load");
			zTextBoxColumnStyleInfo2.ColumnName = "JX_JA_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|c773c774-f9ab-478d-aa85-fc86dc66012c", "Discharge");
			zTextBoxColumnStyleInfo3.ColumnName = "JX_JB_RL_NKPortOfDischarge";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|245cbeba-bcff-4144-97c8-c50881e3de1b", "Doc. Cut Off");
			zDateEditColumnStyleInfo3.ColumnName = "JX_JA_DocumentaryCutoff";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|36b69b06-d7da-4192-8802-5fbfb101a983", "CFS Cut Off");
			zDateEditColumnStyleInfo4.ColumnName = "JX_DepotCutOff";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|8dc2d069-cdd9-480c-9f93-d9aa47796c61", "Storage Date");
			zDateEditColumnStyleInfo5.ColumnName = "JX_JB_CTOStorageDate";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|c13a0232-28e1-4492-ad93-f2555f885c4a", "CFS Receival Start");
			zDateEditColumnStyleInfo6.ColumnName = "JX_DepotReceivalCommences";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|ce575f67-e2c5-414d-8b32-213f068aa360", "CFS Stor.", "CFS Storage Start");
			zDateEditColumnStyleInfo7.ColumnName = "JX_DepotStorageDate";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|785d81f2-ae54-420a-a702-f3fc6d9e6cc7", "CFS Avail.");
			zDateEditColumnStyleInfo8.ColumnName = "JX_DepotAvailabilityDate";
			zDateEditColumnStyleInfo8.IsReadOnly = true;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SchedulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.SchedulesGrid.GridId = "c0b7712a-82f2-486e-b152-7ecd16e3e803";
			this.SchedulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SchedulesGrid.LayoutKey = "zGrid1";
			this.SchedulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SchedulesGrid.Name = "SchedulesGrid";
			this.SchedulesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SchedulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 176, true);
			this.SchedulesGrid.TabIndex = 0;
			// 
			// ConsolGroupBox
			// 
			this.ConsolGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolGroupBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|677bed79-25cd-4a65-819f-75232b016a8c", "Consolidations");
			this.ConsolGroupBox.Controls.Add(this.zGrid1);
			this.ConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 221, true);
			this.ConsolGroupBox.Name = "ConsolGroupBox";
			this.ConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 193, true);
			this.ConsolGroupBox.TabIndex = 7;
			this.ConsolGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "ConsolDetails+CreatedConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_AgentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_ConsolMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_MasterBillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Freight.Business.BulkCopyCriteria)(null)).ConsolDetails.CreatedConsols)).SyncRoot)).JK_RL_NKDischargePort)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|0ac25839-796d-4dbe-80e2-b1c88677a009", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JK_AgentType";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|b0982553-5cbb-43dd-a866-909030acd20d", "Trans.");
			zTextBoxColumnStyleInfo6.ColumnName = "JK_TransportMode";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|abf5b712-57f6-444f-badf-f534dfa85925", "Cont.");
			zTextBoxColumnStyleInfo7.ColumnName = "JK_ConsolMode";
			zTextBoxColumnStyleInfo8.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|c7ed0a31-fa20-4df5-ba95-0be5c1d6b14c", "Load Port");
			zTextBoxColumnStyleInfo9.ColumnName = "JK_RL_NKLoadPort";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|19b8f821-bedd-42a4-88b7-31baa2256809", "Discharge Port");
			zTextBoxColumnStyleInfo10.ColumnName = "JK_RL_NKDischargePort";
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.zGrid1.GridId = "31deff40-58c2-4468-88a5-4b1207cff78f";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 168, true);
			this.zGrid1.TabIndex = 0;
			// 
			// SchedulesForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 473, true);
			this.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("SchedulesForm|9250db0f-5e92-4f83-8523-3de0b3d497f7", "Schedules");
			this.Controls.Add(this.ConsolGroupBox);
			this.Controls.Add(this.Schedules);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SaveButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.BulkCopyCriteria);
			this.DataSourceTypeName = "Enterprise.Freight.Business.BulkCopyCriteria";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 314, true);
			this.Name = "SchedulesForm";
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.Schedules, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConsolGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Schedules.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SchedulesGrid)).EndInit();
			this.ConsolGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);

		}

		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox Schedules;
		private Enterprise.ZArchitecture.ZGrid SchedulesGrid;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolGroupBox;
		private Enterprise.ZArchitecture.ZGrid zGrid1;
	}
}
