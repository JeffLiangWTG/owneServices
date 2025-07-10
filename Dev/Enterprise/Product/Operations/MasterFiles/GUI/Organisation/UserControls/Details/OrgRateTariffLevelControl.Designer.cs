namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgRateTariffLevelControl
	{

		#region Component Designer generated code

		private Enterprise.ZArchitecture.GUI.ZGroupBox GroupBox;
		internal Enterprise.ZArchitecture.ZGrid TariffLevelGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo ZDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo ZDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffLevelGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TariffLevelGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// GroupBox
			// 
			this.GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgRateTariffLevelControl|df78db7b-3eeb-42ef-859e-890767d1f2c4", "Company Tariff and Group Rate Usage");
			this.GroupBox.Controls.Add(this.TariffLevelGrid);
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 168, true);
			this.GroupBox.TabIndex = 7;
			this.GroupBox.TabStop = false;
			// 
			// TariffLevelGrid
			// 
			this.TariffLevelGrid.AllowNavigation = false;
			this.TariffLevelGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TariffLevelGrid, "CompanyData+RateTariffLevels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_TariffType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).TariffDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).TariffLevelAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_ApplyGroupRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgRateTariffLevel)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.RateTariffLevels)).SyncRoot)).P7_ExpiryDate)));
			this.TariffLevelGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "P7_TariffType";
			zDropEditColumnStyleInfo1.ToolTip = "Company Tariff Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgRateTariffLevelControl|35dd914d-2f04-4a0f-b880-278a865e048e", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TariffDescription";
			zTextBoxColumnStyleInfo1.ToolTip = "Company Tariff Type";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "P7_Mode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "P7_Direction";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgRateTariffLevelControl|ae745a05-a888-420c-a917-7c349ef8affe", "Level");
			zDropEditColumnStyleInfo4.ColumnName = "TariffLevelAsString";
			zDropEditColumnStyleInfo4.ToolTip = "Company Tariff Level to Use";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3070dad1-0036-4243-85be-759d9e9342d0", "Group Rate Applicable");
			zCheckBoxColumnStyleInfo1.ColumnName = "P7_ApplyGroupRate";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			ZDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e6fcccb1-1606-4ecd-ac6d-ef55404b16d1", "Start Date");
			ZDateEditColumnStyleInfo1.ColumnName = "P7_StartDate";
			ZDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ZDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			ZDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d07de1d7-f454-4b9f-86c2-796d2966a7ea", "Expiry Date");
			ZDateEditColumnStyleInfo2.ColumnName = "P7_ExpiryDate";
			ZDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ZDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TariffLevelGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TariffLevelGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TariffLevelGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TariffLevelGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TariffLevelGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TariffLevelGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TariffLevelGrid.ColumnStyles.Add(ZDateEditColumnStyleInfo1);
			this.TariffLevelGrid.ColumnStyles.Add(ZDateEditColumnStyleInfo2);
			this.TariffLevelGrid.GridId = "37ee027c-aced-4eea-b839-22aa164b7e64";
			this.TariffLevelGrid.CopySelectedRowsAllowed = true;
			this.TariffLevelGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TariffLevelGrid.LayoutKey = "TariffLevelGrid";
			this.TariffLevelGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.TariffLevelGrid.Name = "TariffLevelGrid";
			this.TariffLevelGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 144, true);
			this.TariffLevelGrid.TabIndex = 0;
			// 
			// OrgRateTariffLevelControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "OrgRateTariffLevelControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 168, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TariffLevelGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
