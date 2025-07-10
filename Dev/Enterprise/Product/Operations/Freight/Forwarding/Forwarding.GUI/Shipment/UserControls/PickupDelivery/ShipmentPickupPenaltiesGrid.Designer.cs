using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ShipmentPickupPenaltiesGrid
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zCPY_CreditorTypeInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCPY_RL_NKLocationInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zCPY_TimeUnitInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCPY_RX_NKCurrencyInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo freeDayExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo durationExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PickupPenaltiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PickupPenaltiesGrid)).BeginInit();
			this.PickupPenaltiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// PickupPenaltiesGrid
			// 
			this.PickupPenaltiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PickupPenaltiesGrid, "PickupPenalties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_JC_Container)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).PenaltyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_RL_NKLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_FreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_PerUnitCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_TotalCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).CPY_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).FormattedFreeDayExclusions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).PickupPenalties)).SyncRoot)).FormattedDurationExclusions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).FirstFreeDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).LastFreeDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).ElapsedFreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).ElapsedDuration)));
			this.PickupPenaltiesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CPY_JC_Container";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "CPY_PenaltyType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "PenaltyTypeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCPY_CreditorTypeInfo.ColumnName = "CPY_CreditorType";
			zCPY_CreditorTypeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CPY_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCPY_RL_NKLocationInfo.ColumnName = "CPY_RL_NKLocation";
			zCPY_RL_NKLocationInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDayAndTimeEditColumnStyleInfo1.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo1.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo1.ColumnName = "CPY_FreeTime";
			zDayAndTimeEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDayAndTimeEditColumnStyleInfo2.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo2.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo2.ColumnName = "CPY_Duration";
			zDayAndTimeEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCPY_TimeUnitInfo.ColumnName = "CPY_TimeUnit";
			zCPY_TimeUnitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c9b3405d-6a68-4b57-85f6-c21e1df107b7", "Sell P/Unit", "Override Sell per Unit", "Charge amount per unit of period (day/hour) in storage, detention, waiting (if amount is fixed per unit)");
			zCalcEditColumnStyleInfo1.ColumnName = "CPY_PerUnitCost";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d46709b0-72dd-43e6-81ef-4d1fcb14dd4a", "Tot. Sell", "Override Total Sell", "Total penalty charge for storage, detention, truck wait time");
			zCalcEditColumnStyleInfo2.ColumnName = "CPY_TotalCost";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCPY_RX_NKCurrencyInfo.ColumnName = "CPY_RX_NKCurrency";
			zCPY_RX_NKCurrencyInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "FirstFreeDay";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "LastFreeDay";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDayAndTimeEditColumnStyleInfo3.ColumnName = "ElapsedFreeTime";
			zDayAndTimeEditColumnStyleInfo3.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDayAndTimeEditColumnStyleInfo4.ColumnName = "ElapsedDuration";
			zDayAndTimeEditColumnStyleInfo4.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			freeDayExclusionsColumnStyleInfo.ColumnName = "FormattedFreeDayExclusions";
			freeDayExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			freeDayExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("854a07ce-2d4e-e6a3-4665-981a440d8d9e", "Free Day Excl.", "Free Day Exclusions", "The days excluded when calculating Free Days.");
			durationExclusionsColumnStyleInfo.ColumnName = "FormattedDurationExclusions";
			durationExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			durationExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("14f6ee96-7e4e-db97-41bf-9ea60320bdaa", "Duration Excl.", "Duration Exclusions", "The days excluded when calculating Duration.");
			this.PickupPenaltiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCPY_CreditorTypeInfo);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCPY_RL_NKLocationInfo);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo2);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCPY_TimeUnitInfo);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zCPY_RX_NKCurrencyInfo);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo3);
			this.PickupPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo4);
			this.PickupPenaltiesGrid.ColumnStyles.Add(freeDayExclusionsColumnStyleInfo);
			this.PickupPenaltiesGrid.ColumnStyles.Add(durationExclusionsColumnStyleInfo);

			this.PickupPenaltiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickupPenaltiesGrid.GridId = "419b6319-f0c6-4231-b5fb-236a67336740";
			this.PickupPenaltiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PickupPenaltiesGrid.LayoutKey = "PickupPenaltiesGrid";
			this.PickupPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickupPenaltiesGrid.Name = "PickupPenaltiesGrid";
			this.PickupPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 363, true);
			this.PickupPenaltiesGrid.TabIndex = 0;
			// 
			// ShipmentPickupPenaltiesGrid
			// 
			this.Controls.Add(this.PickupPenaltiesGrid);
			this.Name = "ShipmentPickupPenaltiesGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 363, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PickupPenaltiesGrid)).EndInit();
			this.PickupPenaltiesGrid.ResumeLayout(false);
			this.PickupPenaltiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid PickupPenaltiesGrid;

		#endregion
	}
}
