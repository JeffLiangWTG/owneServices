namespace Enterprise.Freight.Agency.GUI
{
	partial class SailingLevelAllocations
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			sailingBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(sailingBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyPrincipal);
			// 
			// sailingBoundGrid
			// 
			sailingBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(sailingBoundGrid, "Sailings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Sailing.JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Sailing.JX_JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Sailing.JX_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Sailing.JX_ReservedMasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).TEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).PowerPoints)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Tonnes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).Area)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedTEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedGP_TEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedReefer_TEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedPowerPoints)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedTonnes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocatedTEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocatedPowerPoints)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocatedTonnes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocatedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocatedArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverrideOverallocationPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).OverallocationPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).LoadedCargoWeight)));
			sailingBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|1b964159-bb48-41be-b3ef-7793f67ec523", "Load", "Port Of Loading", "The load port of the current sailing");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Sailing+JX_JA_RL_NKPortOfLoading";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|cc8e5c43-78b6-4f9a-8f51-961235fd8428", "Disch.", "Port Of Discharge", "The discharge port of the current sailing.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Sailing+JX_JB_RL_NKPortOfDischarge";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.ColumnName = "Sailing+JX_IsPublished";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "Sailing+JX_ReservedMasterBill";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|a6366f23-07dd-4674-87b2-29d5b9f5a585", "TEUs", "Allocated TEUs", "The total number of TEUs you have been allocated for shipments on this sailing.");
			zCalcEditColumnStyleInfo1.ColumnName = "TEU";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|e49eb5fd-22d9-4818-87af-0e7fe184906a", "Power", "Power Points", "Allocated Power Points", "The total number of power points you have been allocated for shipments on this sailing.");
			zCalcEditColumnStyleInfo2.ColumnName = "PowerPoints";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|5eeda939-ad11-447c-b37f-770b31a73369", "Tonnes", "Allocated Tonnes", "The total weight you have been allocated for shipments on this sailing.");
			zCalcEditColumnStyleInfo3.ColumnName = "Tonnes";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|389d195c-0d65-4da5-be28-ee85e9d26640", "Volume", "Allocated Volume", "The total volume of space you have been allocated in cubic meters for shipments on this sailing.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.");
			zCalcEditColumnStyleInfo4.ColumnName = "Volume";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|48db7a2b-4f22-4ea5-8570-efd8a7dc3002", "Area", "Allocated Area", "The total floor space in square meters you have been allocated for shipments on this sailing.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.");
			zCalcEditColumnStyleInfo5.ColumnName = "Area";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "UsedTEU";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "UsedGP_TEU";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|0c303111-9f01-4bc5-8761-db14bf285c5d", "Used Reefer TEUs", "Used Refrigerated TEUs", "The total number of reefer TEUs from confirmed shipments on this sailing.");
			zCalcEditColumnStyleInfo8.ColumnName = "UsedReefer_TEU";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "UsedPowerPoints";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "UsedTonnes";
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "UsedVolume";
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "UsedArea";
			zCalcEditColumnStyleInfo12.Decimals = 3;
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|26b359f4-9d00-46da-9447-7570cd932ef1", "Over. TEUs", "Over Allocated TEUs", "The upper limit on the number of TEUs that may be booked for shipments on this sailing.");
			zCalcEditColumnStyleInfo13.ColumnName = "OverallocatedTEU";
			zCalcEditColumnStyleInfo13.Decimals = 0;
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|dca441f0-a03f-4b8b-a35c-9f1266d1f19b", "Over. Power", "Over. Power Points", "Over Allocation Power Points", "The upper limit on the number of power points that may be booked for shipments on this sailing.");
			zCalcEditColumnStyleInfo14.ColumnName = "OverallocatedPowerPoints";
			zCalcEditColumnStyleInfo14.Decimals = 0;
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|f6a5e7a5-457b-4d2c-92b0-cbfa64f7a92c", "Over. Tonnes", "Over Allocation Tonnes", "The upper limit on the weight in tonnes that may be booked for shipments on this sailing.");
			zCalcEditColumnStyleInfo15.ColumnName = "OverallocatedTonnes";
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|3df3a0db-7603-4fc3-95d6-12bbe95599da", "Over. Volume", "Over Allocation Volume", "The upper limit on the volume in cubic meters that may be booked for on this sailing.\r\n\r\nContainerized shipments count towards the TEUs instead of volume.");
			zCalcEditColumnStyleInfo16.ColumnName = "OverallocatedVolume";
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|a415f8c7-00b4-4ca6-a513-dfa573f31f0f", "Over. Area", "Over Allocation Area", "The upper limit on the floor space in square meters that may be booked for shipments on this sailing.\r\n\r\nOnly break-bulk and roll-on roll-off count towards area.");
			zCalcEditColumnStyleInfo17.ColumnName = "OverallocatedArea";
			zCalcEditColumnStyleInfo17.Decimals = 3;
			zCheckBoxColumnStyleInfo2.ColumnName = "OverrideOverallocationPercent";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|42e5d954-6a08-4d7c-b9a2-5476f8f7606d", "Overallocation Percent");
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "OverallocationPercent";
			zCalcEditColumnStyleInfo18.Decimals = 0;
			zCalcEditColumnStyleInfo18.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("SailingLevelAllocations|42e5d954-6a08-4d7c-b9a2-5476f8f7606d", "Overallocation Percent");
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.ColumnName = "LoadedCargoWeight";
			zCalcEditColumnStyleInfo19.Decimals = 0;
			sailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			sailingBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			sailingBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			sailingBoundGrid.GridId = "bea6616a-4849-45a3-94b4-b2a846ddba83";
			sailingBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			sailingBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			sailingBoundGrid.LayoutKey = "zGrid2";
			sailingBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			sailingBoundGrid.Name = "sailingBoundGrid";
			sailingBoundGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			sailingBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 105, true);
			sailingBoundGrid.TabIndex = 1;
			// 
			// SailingLevelAllocations
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(sailingBoundGrid);
			this.Name = "SailingLevelAllocations";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(sailingBoundGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid sailingBoundGrid;
	}
}
