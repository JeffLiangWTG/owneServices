using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ShipmentDeliveryPenaltiesGrid
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zCPY_PenaltyTypeInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zTimeUnitInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zNKCurrencyInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo freeDayExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo durationExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DeliveryPenaltiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryPenaltiesGrid)).BeginInit();
			this.DeliveryPenaltiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// DeliveryPenaltiesGrid
			// 
			this.DeliveryPenaltiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeliveryPenaltiesGrid, "DeliveryPenalties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_JC_Container)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).PenaltyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_RL_NKLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_FreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_PerUnitCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_TotalCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).CPY_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).FirstFreeDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).LastFreeDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).ElapsedFreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).ElapsedDuration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).FormattedFreeDayExclusions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ShipmentContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).DeliveryPenalties)).SyncRoot)).FormattedDurationExclusions)));
			this.DeliveryPenaltiesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CPY_JC_Container";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCPY_PenaltyTypeInfo.ColumnName = "CPY_PenaltyType";
			zCPY_PenaltyTypeInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
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
			zTimeUnitInfo.ColumnName = "CPY_TimeUnit";
			zTimeUnitInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b1b1e4ae-f143-40c6-aa7a-e748e390cc0e", "Sell P/Unit", "Override Sell per Unit", "Charge amount per unit of period (day/hour) in storage, detention, waiting (if amount is fixed per unit)");
			zCalcEditColumnStyleInfo1.ColumnName = "CPY_PerUnitCost";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7562befd-94d9-4166-9e24-b2b64d2c5187", "Tot. Sell", "Override Total Sell", "Total penalty charge for storage, detention, truck wait time");
			zCalcEditColumnStyleInfo2.ColumnName = "CPY_TotalCost";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zNKCurrencyInfo.ColumnName = "CPY_RX_NKCurrency";
			zNKCurrencyInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
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
			freeDayExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("b631213e-d479-1a85-49fa-54fd55adbc8b", "Free Day Excl.", "Free Day Exclusions", "The days excluded when calculating Free Days.");
			durationExclusionsColumnStyleInfo.ColumnName = "FormattedDurationExclusions";
			durationExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			durationExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("0da71b6d-67f2-9e92-435f-1f7a98374270", "Duration Excl.", "Duration Exclusions", "The days excluded when calculating Duration.");
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zCPY_PenaltyTypeInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zCPY_CreditorTypeInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zCPY_RL_NKLocationInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo2);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zTimeUnitInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zNKCurrencyInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo3);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo4);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(freeDayExclusionsColumnStyleInfo);
			this.DeliveryPenaltiesGrid.ColumnStyles.Add(durationExclusionsColumnStyleInfo);
			this.DeliveryPenaltiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryPenaltiesGrid.GridId = "1256b104-cc9a-41e4-ac3e-2fe2f0533649";
			this.DeliveryPenaltiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveryPenaltiesGrid.LayoutKey = "DeliveryPenaltiesGrid";
			this.DeliveryPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryPenaltiesGrid.Name = "DeliveryPenaltiesGrid";
			this.DeliveryPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 248, true);
			this.DeliveryPenaltiesGrid.TabIndex = 0;
			// 
			// ShipmentDeliveryPenaltiesGrid
			// 
			this.Controls.Add(this.DeliveryPenaltiesGrid);
			this.Name = "ShipmentDeliveryPenaltiesGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 248, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryPenaltiesGrid)).EndInit();
			this.DeliveryPenaltiesGrid.ResumeLayout(false);
			this.DeliveryPenaltiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid DeliveryPenaltiesGrid;

		#endregion
	}
}
