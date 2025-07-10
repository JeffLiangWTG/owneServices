namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class DeclarationLockConfigControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.LockConfigLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TabLockInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TabLockInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EventLockInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EventLockInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LockConfigGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TabLockInfoGrid)).BeginInit();
			this.TabLockInfoGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventLockInfoGrid)).BeginInit();
			this.EventLockInfoGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LockConfigGrid)).BeginInit();
			this.LockConfigGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig);
			// 
			// LockConfigLabel
			// 
			this.LockConfigLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DeclarationLockConfigControl|b66e5c76-f0b3-4e8f-9bb7-93ff9293c0cb", "Declaration Types To Lock");
			this.LockConfigLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LockConfigLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LockConfigLabel.IsFontBold = true;
			this.LockConfigLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LockConfigLabel.Name = "LockConfigLabel";
			this.LockConfigLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 23, true);
			this.LockConfigLabel.TabIndex = 0;
			this.LockConfigLabel.UseMnemonic = false;
			// 
			// TabLockInfoLabel
			// 
			this.TabLockInfoLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5bd8ae91-1fe5-46fe-ab3d-e9eda50075ee", "Tabs that become Locked(Per Declaration Type)");
			this.TabLockInfoLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TabLockInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TabLockInfoLabel.IsFontBold = true;
			this.TabLockInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.TabLockInfoLabel.Name = "TabLockInfoLabel";
			this.TabLockInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 23, true);
			this.TabLockInfoLabel.TabIndex = 4;
			this.TabLockInfoLabel.UseMnemonic = false;
			// 
			// TabLockInfoGrid
			// 
			this.TabLockInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TabLockInfoGrid, "TabInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).TabInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationTabLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).TabInfos)).SyncRoot)).TabPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationTabLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).TabInfos)).SyncRoot)).TabPageDescription)));
			this.TabLockInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("82FE2D02-CD92-43EC-9D42-47DE78301AD6", "Tab To Lock");
			zDropEditColumnStyleInfo1.ColumnName = "TabPage";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("69AD5687-470B-4E17-954C-83AB85E1E421", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TabPageDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.TabLockInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TabLockInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TabLockInfoGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TabLockInfoGrid.GridId = "003738f5-e076-4bbe-8a4e-ae68dcd45a69";
			this.TabLockInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TabLockInfoGrid.LayoutKey = "TabLockInfoGrid";
			this.TabLockInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			this.TabLockInfoGrid.Name = "TabLockInfoGrid";
			this.TabLockInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 90, true);
			this.TabLockInfoGrid.TabIndex = 5;
			// 
			// EventLockInfoGrid
			// 
			this.EventLockInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EventLockInfoGrid, "EventInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).EventType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).Lookups.EventTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).EventReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).EventSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationEventLockInfo)(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).EventInfos)).SyncRoot)).Lookups.EntryTypeList)));
			this.EventLockInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "Lookups.EventTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("88D8992D-CA75-4DD5-81CD-8F10736DA816", "Event Type");
			zDropEditColumnStyleInfo2.ColumnName = "EventType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E5F51D56-1F92-4189-B68A-7FBE4CB91972", "Event Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "EventReference";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("48224C19-1258-4B23-B81F-3D4C7E8FA893", "Source");
			zDropEditColumnStyleInfo3.ColumnName = "EventSource";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.EntryTypeList";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("88C626D0-2130-49B8-AFAD-A10185D97779", "Entry Type");
			zDropEditColumnStyleInfo4.ColumnName = "EntryType";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EventLockInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EventLockInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EventLockInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EventLockInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EventLockInfoGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.EventLockInfoGrid.GridId = "b9d6f386-000d-4fe7-acad-46a6c3b96ade";
			this.EventLockInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EventLockInfoGrid.LayoutKey = "EventLockInfoGrid";
			this.EventLockInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 197, true);
			this.EventLockInfoGrid.Name = "EventLockInfoGrid";
			this.EventLockInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 90, true);
			this.EventLockInfoGrid.TabIndex = 3;
			// 
			// EventLockInfoLabel
			// 
			this.EventLockInfoLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6d24fd2f-2e21-432a-ab6d-e3b7897b6cc5", "Events that Lock(Per Declaration Type)");
			this.EventLockInfoLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.EventLockInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.EventLockInfoLabel.IsFontBold = true;
			this.EventLockInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.EventLockInfoLabel.Name = "EventLockInfoLabel";
			this.EventLockInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 23, true);
			this.EventLockInfoLabel.TabIndex = 2;
			this.EventLockInfoLabel.UseMnemonic = false;
			// 
			// LockConfigGrid
			// 
			this.LockConfigGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LockConfigGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).DeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).Lookups.DeclarationTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).DeclarationTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).LockMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.DeclarationLockConfig)(null)).Lookups.LockModeList)));
			this.LockConfigGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.BindToList = "Lookups.DeclarationTypeList";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0E65AE60-DEE4-49C2-9CB8-A3C9A0E18428", "Declaration Type");
			zDropEditColumnStyleInfo5.ColumnName = "DeclarationType";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("92BBF0B7-40BA-4F34-904A-7825CF5D0CB5", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "DeclarationTypeDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo6.BindToList = "Lookups.LockModeList";
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("004B3988-EB28-4FEA-86ED-C8364E36862D", "Entry Lock Mode");
			zDropEditColumnStyleInfo6.ColumnName = "LockMode";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.LockConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LockConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LockConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.LockConfigGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LockConfigGrid.GridId = "fab5f45d-f19f-491f-a441-9e53770d88e3";
			this.LockConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LockConfigGrid.LayoutKey = "LockConfigGrid";
			this.LockConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.LockConfigGrid.Name = "LockConfigGrid";
			this.LockConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 151, true);
			this.LockConfigGrid.TabIndex = 1;
			// 
			// DeclarationLockConfigControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LockConfigGrid);
			this.Controls.Add(this.EventLockInfoLabel);
			this.Controls.Add(this.EventLockInfoGrid);
			this.Controls.Add(this.TabLockInfoLabel);
			this.Controls.Add(this.TabLockInfoGrid);
			this.Controls.Add(this.LockConfigLabel);
			this.Name = "DeclarationLockConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TabLockInfoGrid)).EndInit();
			this.TabLockInfoGrid.ResumeLayout(false);
			this.TabLockInfoGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventLockInfoGrid)).EndInit();
			this.EventLockInfoGrid.ResumeLayout(false);
			this.EventLockInfoGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LockConfigGrid)).EndInit();
			this.LockConfigGrid.ResumeLayout(false);
			this.LockConfigGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel LockConfigLabel;
		ZArchitecture.ZLabel TabLockInfoLabel;
		ZArchitecture.ZGrid TabLockInfoGrid;
		ZArchitecture.ZGrid EventLockInfoGrid;
		ZArchitecture.ZLabel EventLockInfoLabel;
		ZArchitecture.ZGrid LockConfigGrid;
	}
}
