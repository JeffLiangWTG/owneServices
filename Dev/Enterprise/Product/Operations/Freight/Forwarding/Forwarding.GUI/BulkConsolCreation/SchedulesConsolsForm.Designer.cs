namespace Enterprise.Freight.Forwarding.GUI
{
	partial class SchedulesConsolsForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Schedules = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SchedulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConsolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Schedules.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SchedulesGrid)).BeginInit();
			this.ConsolGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 449, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.MultiDaysSelection);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|D3F02C58-CC7F-4886-88F0-E25822FCE717", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(725, 420, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|DA99FD85-E814-4E7F-9D73-1E277A739543", "Save");
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
			this.Schedules.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|9763A637-BA74-4AA7-B2C5-8B28A1333F86", "Schedules");
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
			this.BindingSource.SetBindingMember(this.SchedulesGrid, "SailingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)).SyncRoot)).JX_JV_VoyageFlight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)).SyncRoot)).JX_JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)).SyncRoot)).JX_JB_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).SailingCollection)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			this.SchedulesGrid.CaptionVisible = false;

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|62E153CF-3710-43C1-8E08-AA234E594286", "Flight");
			zTextBoxColumnStyleInfo1.ColumnName = "JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|E75E757F-E502-4A88-8A09-A42A7ACB4C7D", "Departure Time");
			zDateEditColumnStyleInfo1.ColumnName = "JX_JA_E_DEP";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|8911A935-D359-41FE-AFE0-A7D78A59249E", "Arrival Time");
			zDateEditColumnStyleInfo2.ColumnName = "JX_JB_E_ARV";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|219E5455-358B-4FDE-BAE2-035C7E3839F2", "Origin");
			zTextBoxColumnStyleInfo2.ColumnName = "JX_JA_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;	
			
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|752838A5-F5BB-4011-A903-1A4BC5BA6676", "Destination");
			zTextBoxColumnStyleInfo3.ColumnName = "JX_JB_RL_NKPortOfDischarge";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);

			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SchedulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SchedulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SchedulesGrid.GridId = "2E065ED5-769A-4891-8FD8-CD7AB0191334";
			this.SchedulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SchedulesGrid.LayoutKey = "SchedulesGrid";
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
			this.ConsolGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|52A950A2-A464-47B5-9687-662EC864887D", "Consolidations");
			this.ConsolGroupBox.Controls.Add(this.ConsolsGrid);
			this.ConsolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 221, true);
			this.ConsolGroupBox.Name = "ConsolGroupBox";
			this.ConsolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 193, true);
			this.ConsolGroupBox.TabIndex = 7;
			this.ConsolGroupBox.TabStop = false;
			// 
			// ConsolsGrid
			// 
			this.ConsolsGrid.AllowNavigation = false;
			this.ConsolsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConsolsGrid, "CreatedConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)));			
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_AgentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_ConsolMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_MasterBillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_JX_JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).JK_JX_JB_E_LastARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).CreatedConsols)).SyncRoot)).TemplateRecord.STR_TemplateName)));
			this.ConsolsGrid.CaptionVisible = false;
			
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|FFCA5B7A-12ED-44E8-A6D1-3A080F71EA21", "Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);

			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|C19CE2CA-2D12-4627-821D-B63395080A99", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JK_AgentType";

			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|EE4AD36C-5B7E-4C3B-84AF-46916264D4CD", "Trans.");
			zTextBoxColumnStyleInfo6.ColumnName = "JK_TransportMode";

			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|BC93CF24-7952-4500-B57E-475D5367E99E", "Cont.");
			zTextBoxColumnStyleInfo7.ColumnName = "JK_ConsolMode";

			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|A6E85776-F9F0-45C0-A560-C5FF65C75BCD", "Master Bill");
			zTextBoxColumnStyleInfo8.ColumnName = "JK_MasterBillNum";

			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|BC77536F-C921-42E1-9BEB-EACE19AABEFF", "Load Port");
			zTextBoxColumnStyleInfo9.ColumnName = "JK_RL_NKLoadPort";

			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|9557D6A9-3249-4142-990B-E83174B41A5B", "Discharge Port");
			zTextBoxColumnStyleInfo10.ColumnName = "JK_RL_NKDischargePort";

			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|8A77A617-9C4F-4090-96E5-33F2F1AE45B8", "First ETD");
			zDateEditColumnStyleInfo3.ColumnName = "JK_JX_JA_E_DEP";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|89812622-0FFD-4859-A8A3-65800725F973", "Last ETA");
			zDateEditColumnStyleInfo4.ColumnName = "JK_JX_JB_E_LastARV";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|48b03cc9-78fb-487f-bbf7-1b0d17acb6e4", "Template Name");
			zTextBoxColumnStyleInfo11.ColumnName = "TemplateRecord+STR_TemplateName";

			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ConsolsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ConsolsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ConsolsGrid.GridId = "1A3427E4-F31E-42D2-9A7D-4A9477BBD372";
			this.ConsolsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolsGrid.LayoutKey = "ConsolsGrid";
			this.ConsolsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ConsolsGrid.Name = "ConsolsGrid";
			this.ConsolsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ConsolsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 168, true);
			this.ConsolsGrid.TabIndex = 0;
			// 
			// SchedulesConsolsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 473, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("SchedulesConsolsForm|DEE8A8D1-6316-4109-8989-6667827F98F5", "Schedules");
			this.Controls.Add(this.ConsolGroupBox);
			this.Controls.Add(this.Schedules);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SaveButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.GUI";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.MultiDaysSelection);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.MultiDaysSelection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 314, true);
			this.Name = "SchedulesConsolsForm";
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
			((System.ComponentModel.ISupportInitialize)(this.ConsolsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox Schedules;
		private Enterprise.ZArchitecture.ZGrid SchedulesGrid;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolGroupBox;
		private Enterprise.ZArchitecture.ZGrid ConsolsGrid;
	}
}
