using Enterprise.Freight.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class ContainerPenaltiesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo firstFreeDaysColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo lastFreeDaysColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo freeDayExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.FreeDayExclusionColumnStyleInfo();
			Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo durationExclusionColumnStyleInfo = new Enterprise.MasterFiles.GUI.DurationExclusionColumnStyleInfo();
			this.ContainerPenaltiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerPenaltiesGrid)).BeginInit();
			this.ContainerPenaltiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ContainerPenaltiesGrid
			// 
			this.ContainerPenaltiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainerPenaltiesGrid, "CarrierContainerPenalties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PenaltyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_DetentionPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_OriginPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_FirstFreeDayType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_LastFreeDayType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_OH_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_OH_CTO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgContainerDetention)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CarrierContainerPenalties)).SyncRoot)).PD_FreeDays)));
			this.ContainerPenaltiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PD_Direction";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.ColumnName = "PD_PenaltyType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "PenaltyDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PD_DetentionPortOrCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "PD_OriginPortOrCountry";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "PD_ContainerType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			firstFreeDaysColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			firstFreeDaysColumnStyleInfo.ColumnName = "PD_FirstFreeDayType";
			firstFreeDaysColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			lastFreeDaysColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			lastFreeDaysColumnStyleInfo.ColumnName = "PD_LastFreeDayType";
			lastFreeDaysColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "PD_OH_Client";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "PD_OH_CTO";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PD_FreeDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			freeDayExclusionColumnStyleInfo.ColumnName = "PD_CEX_FreeDayExclusion";
			freeDayExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			durationExclusionColumnStyleInfo.ColumnName = "PD_CEX_DurationExclusion";
			durationExclusionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(firstFreeDaysColumnStyleInfo);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(lastFreeDaysColumnStyleInfo);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(freeDayExclusionColumnStyleInfo);
			this.ContainerPenaltiesGrid.ColumnStyles.Add(durationExclusionColumnStyleInfo);

			this.ContainerPenaltiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerPenaltiesGrid.GridId = "e27fb61d-0176-4f8f-bd67-df33b003032e";
			this.ContainerPenaltiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerPenaltiesGrid.LayoutKey = "ContainerPenaltiesGrid";
			this.ContainerPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerPenaltiesGrid.Name = "ContainerPenaltiesGrid";
			this.ContainerPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 165, true);
			this.ContainerPenaltiesGrid.TabIndex = 0;
			// 
			// ContainerPenaltiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerPenaltiesGrid);
			this.Name = "ContainerPenaltiesUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 165, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainerPenaltiesGrid)).EndInit();
			this.ContainerPenaltiesGrid.ResumeLayout(false);
			this.ContainerPenaltiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZGrid ContainerPenaltiesGrid;

		#endregion
	}
}
