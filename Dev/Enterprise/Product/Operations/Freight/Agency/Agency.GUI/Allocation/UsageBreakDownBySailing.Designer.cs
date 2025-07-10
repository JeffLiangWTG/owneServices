namespace Enterprise.Freight.Agency.GUI
{
	partial class UsageBreakDownBySailing
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedTEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedGP_TEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedReefer_TEU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedPowerPoints)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedTonnes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencySailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyPrincipal)(null)).Sailings)).SyncRoot)).UsedArea)));
			sailingBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("UsageBreakDownBySailing|1b964159-bb48-41be-b3ef-7793f67ec523", "Load", "Port Of Loading", "The load port of the current sailing");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Sailing+JX_JA_RL_NKPortOfLoading";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("UsageBreakDownBySailing|cc8e5c43-78b6-4f9a-8f51-961235fd8428", "Disch.", "Port Of Discharge", "The discharge port of the current sailing.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Sailing+JX_JB_RL_NKPortOfDischarge";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "UsedTEU";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "UsedGP_TEU";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "UsedReefer_TEU";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "UsedPowerPoints";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "UsedTonnes";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "UsedVolume";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "UsedArea";
			zCalcEditColumnStyleInfo7.Decimals = 3;
			sailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			sailingBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			sailingBoundGrid.GridId = "dc6debc6-f67c-4731-a19a-44b1141e9891";
			sailingBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			sailingBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			sailingBoundGrid.LayoutKey = "zGrid2";
			sailingBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			sailingBoundGrid.Name = "sailingBoundGrid";
			sailingBoundGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			sailingBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 105, true);
			sailingBoundGrid.TabIndex = 2;
			// 
			// UsageBreakDownBySailing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(sailingBoundGrid);
			this.Name = "UsageBreakDownBySailing";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(sailingBoundGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZGrid sailingBoundGrid;
	}
}
