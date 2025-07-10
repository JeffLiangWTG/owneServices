namespace Enterprise.MasterFiles.GUI
{
	public partial class CashAdvanceJobConfig
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.jobConfigGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ChargeCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargeGroupsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargeGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.chargeCodeMultiSelectUserControl1 = new Enterprise.MasterFiles.GUI.CashAdvanceChargeCodeMultiSelectUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.jobConfigGrid)).BeginInit();
			this.jobConfigGrid.SuspendLayout();
			this.TopGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.ChargeCodesGroupBox.SuspendLayout();
			this.ChargeGroupsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsGrid)).BeginInit();
			this.ChargeGroupsGrid.SuspendLayout();
			this.chargeCodeMultiSelectUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration);
			// 
			// jobConfigGrid
			// 
			this.jobConfigGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.jobConfigGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).LevelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).CAC_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).CAC_ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).CAC_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).CAC_DefaultingOption)));
			this.jobConfigGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e9befbf2-d7f4-41d3-b6c6-316f8441a360", "Source");
			zTextBoxColumnStyleInfo1.ColumnName = "LevelName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a97d2332-3573-4668-81bf-2b141ecc6a35", "Job Type");
			zDropEditColumnStyleInfo2.ColumnName = "CAC_JobType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("10d1e279-0688-45e8-a905-0b49104cbe60", "Direction");
			zDropEditColumnStyleInfo3.ColumnName = "CAC_ServiceDirection";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6f0792f7-aee6-4e7d-80e7-619cafab368d", "Transport Mode");
			zDropEditColumnStyleInfo4.ColumnName = "CAC_TransportMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e93a6fcd-44af-4bbd-8783-4ac33ebfc456", "Defaulting Charges");
			zDropEditColumnStyleInfo5.ColumnName = "CAC_DefaultingOption";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.jobConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.jobConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.jobConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.jobConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.jobConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.jobConfigGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobConfigGrid.GridId = "59b1f59e-29ed-44ae-be0d-a4c6a5992241";
			this.jobConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.jobConfigGrid.LayoutKey = "jobConfigGrid";
			this.jobConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobConfigGrid.Name = "jobConfigGrid";
			this.jobConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 217, true);
			this.jobConfigGrid.TabIndex = 0;
			// 
			// TopGroupBox
			//
			this.TopGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c0359801-e454-4f9c-a59d-3b218d83b910", "Receivables Advance Payment Required Defaults");
			this.TopGroupBox.Controls.Add(this.jobConfigGrid);
			this.TopGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopGroupBox.Name = "TopGroupBox";
			this.TopGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 217, true);
			this.TopGroupBox.TabIndex = 3;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ChargeCodesGroupBox);
			this.BottomPanel.Controls.Add(this.ChargeGroupsGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 226, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 261, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// ChargeCodesGroupBox
			// 
			this.ChargeCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4bfa92e5-0c3c-4ce0-96cb-5823540b7d85", "Charge Codes");
			this.ChargeCodesGroupBox.Controls.Add(this.chargeCodeMultiSelectUserControl1);
			this.ChargeCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ChargeCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 0, true);
			this.ChargeCodesGroupBox.Name = "ChargeCodesGroupBox";
			this.ChargeCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 261, true);
			this.ChargeCodesGroupBox.TabIndex = 1;
			this.ChargeCodesGroupBox.TabStop = false;
			// 
			// ChargeGroupsGroupBox
			// 
			this.ChargeGroupsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("719df355-45b5-471c-8f81-70e14a44d599", "Charge Groups");
			this.ChargeGroupsGroupBox.Controls.Add(this.ChargeGroupsGrid);
			this.ChargeGroupsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ChargeGroupsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupsGroupBox.Name = "ChargeGroupsGroupBox";
			this.ChargeGroupsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 261, true);
			this.ChargeGroupsGroupBox.TabIndex = 0;
			this.ChargeGroupsGroupBox.TabStop = false;
			// 
			// ChargeGroupsGrid
			// 
			this.ChargeGroupsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupsGrid, "ChargeGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).ChargeGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CashAdvanceDefaultingChargeGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).ChargeGroups)).SyncRoot)).JCT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CashAdvanceDefaultingChargeGroup)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).ChargeGroups)).SyncRoot)).ChargeGroupDescription)));
			this.ChargeGroupsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d29dbe85-2b21-457c-ab39-8a31afb6270e", "Code");
			zDropEditColumnStyleInfo6.ColumnName = "JCT_Code";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9a90bae1-0708-455f-a79e-cbd921a38b01", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeGroupDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargeGroupsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ChargeGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeGroupsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupsGrid.GridId = "d68ac27e-8c3f-41f1-8dfa-3a22212a8c27";
			this.ChargeGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupsGrid.LayoutKey = "ChargeGroupsGrid";
			this.ChargeGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.ChargeGroupsGrid.Name = "ChargeGroupsGrid";
			this.ChargeGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 615, true);
			this.ChargeGroupsGrid.TabIndex = 0;
			// 
			// chargeCodeMultiSelectUserControl1
			// 
			this.chargeCodeMultiSelectUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chargeCodeMultiSelectUserControl1, ".");
			this.chargeCodeMultiSelectUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chargeCodeMultiSelectUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.chargeCodeMultiSelectUserControl1.Name = "chargeCodeMultiSelectUserControl1";
			this.chargeCodeMultiSelectUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 615, true);
			this.chargeCodeMultiSelectUserControl1.TabIndex = 0;
			// 
			// CashAdvanceJobConfig
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Name = "CashAdvanceJobConfig";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 487, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.jobConfigGrid)).EndInit();
			this.jobConfigGrid.ResumeLayout(false);
			this.jobConfigGrid.PerformLayout();
			this.TopGroupBox.ResumeLayout(false);
			this.TopGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ChargeCodesGroupBox.ResumeLayout(false);
			this.ChargeCodesGroupBox.PerformLayout();
			this.ChargeGroupsGroupBox.ResumeLayout(false);
			this.ChargeGroupsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsGrid)).EndInit();
			this.ChargeGroupsGrid.ResumeLayout(false);
			this.ChargeGroupsGrid.PerformLayout();
			this.chargeCodeMultiSelectUserControl1.ResumeLayout(true);
			this.chargeCodeMultiSelectUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid jobConfigGrid;
		private ZArchitecture.GUI.ZGroupBox TopGroupBox;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZGroupBox ChargeGroupsGroupBox;
		private ZArchitecture.ZGrid ChargeGroupsGrid;
		private ZArchitecture.GUI.ZGroupBox ChargeCodesGroupBox;
		private CashAdvanceChargeCodeMultiSelectUserControl chargeCodeMultiSelectUserControl1;
	}
}
