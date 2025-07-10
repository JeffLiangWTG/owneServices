using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AccExRateConfigs : ZUserControl
	{
		CargoWise.Windows.UI.KSplitContainer AccExRateConfigsSplitContainer;
		internal ZGroupBox CurrencyGroupBox;
		internal ZGrid CurrencyGrid;
		internal ZGrid MasterGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zJCT_CodeFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCT_ExRateTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zLevelNameTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_InvoiceCurrencyTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_JobTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_LedgerDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_TransportModeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_ServiceDirectionDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zJCE_PreferenceDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zJCE_OffsetCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zJCE_PromptCheckBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zJCT_StartDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zJCT_ExpiryDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.AccExRateConfigsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CurrencyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CurrencyGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MasterGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrencyGrid)).BeginInit();
			this.CurrencyGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccExRateConfigsSplitContainer)).BeginInit();
			this.AccExRateConfigsSplitContainer.Panel1.SuspendLayout();
			this.AccExRateConfigsSplitContainer.Panel2.SuspendLayout();
			this.AccExRateConfigsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MasterGrid)).BeginInit();
			this.MasterGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection);
			// 
			// AccExRateConfigsSplitContainer
			// 
			this.AccExRateConfigsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccExRateConfigsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccExRateConfigsSplitContainer.Name = "AccExRateConfigsSplitContainer";
			// 
			// AccExRateConfigsSplitContainer.Panel1
			// 
			this.AccExRateConfigsSplitContainer.Panel1.Controls.Add(this.MasterGrid);
			// 
			// AccExRateConfigsSplitContainer.Panel2
			// 
			this.AccExRateConfigsSplitContainer.Panel2.Controls.Add(this.CurrencyGroupBox);
			this.AccExRateConfigsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2336, 973, true);
			this.AccExRateConfigsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(798);
			this.AccExRateConfigsSplitContainer.SplitterWidth = 10;
			this.AccExRateConfigsSplitContainer.TabIndex = 0;
			// 
			// CurrencyGroupBox
			// 
			this.CurrencyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D8AFEBA4-2F59-4FB7-B1C3-99DD2DA81655", "Exchange Rate Source");
			this.CurrencyGroupBox.Controls.Add(this.CurrencyGrid);
			this.CurrencyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrencyGroupBox.Name = "CurrencyGroupBox";
			this.CurrencyGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 2, 7, 2, true);
			this.CurrencyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 442, true);
			this.CurrencyGroupBox.TabIndex = 11;
			this.CurrencyGroupBox.TabStop = false;
			// 
			// CurrencyGrid
			// 
			this.CurrencyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CurrencyGrid, "CurrencyConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).CurrencyConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExchangeRateCurrencyConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).CurrencyConfigurations)).SyncRoot)).JCT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExchangeRateCurrencyConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).CurrencyConfigurations)).SyncRoot)).JCT_ExRateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.ExchangeRateCurrencyConfiguration)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).CurrencyConfigurations)).SyncRoot)).JCT_StartDate)));
			this.CurrencyGrid.CaptionVisible = false;
			zJCT_CodeFindBoxColumnStyleInfo.ColumnName = "JCT_Code";
			zJCT_CodeFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCT_CodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCT_ExRateTypeDropEditColumnStyleInfo.ColumnName = "JCT_ExRateType";
			zJCT_ExRateTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 1;
			zJCT_ExRateTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCT_StartDateEditColumnStyleInfo.ColumnName = "JCT_StartDate";
			zJCT_StartDateEditColumnStyleInfo.DefaultCollectionIndex = 2;
			zJCT_StartDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCT_StartDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zJCT_ExpiryDateEditColumnStyleInfo.ColumnName = "JCT_ExpiryDate";
			zJCT_ExpiryDateEditColumnStyleInfo.DefaultCollectionIndex = 3;
			zJCT_ExpiryDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCT_ExpiryDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.CurrencyGrid.ColumnStyles.Add(zJCT_CodeFindBoxColumnStyleInfo);
			this.CurrencyGrid.ColumnStyles.Add(zJCT_ExRateTypeDropEditColumnStyleInfo);
			this.CurrencyGrid.ColumnStyles.Add(zJCT_StartDateEditColumnStyleInfo);
			this.CurrencyGrid.ColumnStyles.Add(zJCT_ExpiryDateEditColumnStyleInfo);
			this.CurrencyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencyGrid.GridId = "6EC68DDA-035B-4747-8CA9-AC1B37EF75C0";
			this.CurrencyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrencyGrid.LayoutKey = "CurrencyGrid";
			this.CurrencyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 14, true);
			this.CurrencyGrid.Name = "CurrencyGrid";
			this.CurrencyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 426, true);
			this.CurrencyGrid.TabIndex = 12;
			// 
			// MasterGrid
			// 
			this.MasterGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MasterGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).LevelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_InvoiceCurrencyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_Calc_CurrencyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_Preference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_Offset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccExchangeRateConfiguration)(null)).JCE_Prompt)));
			this.MasterGrid.CaptionVisible = false;
			zLevelNameTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("143299e7-218b-423d-9d8c-33e34bd41405", "Source");
			zLevelNameTextBoxColumnStyleInfo.ColumnName = "LevelName";
			zLevelNameTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			zLevelNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_InvoiceCurrencyTypeDropEditColumnStyleInfo.ColumnName = "JCE_InvoiceCurrencyType";
			zJCE_InvoiceCurrencyTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_InvoiceCurrencyTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_JobTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f2fe313e-7cd3-4085-802e-7e1b23cdf795", "Job Type");
			zJCE_JobTypeDropEditColumnStyleInfo.ColumnName = "JCE_JobType";
			zJCE_JobTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_JobTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_LedgerDropEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9c27af5a-6652-4cf4-ba62-a015b4e5a42c", "Ledger");
			zJCE_LedgerDropEditColumnStyleInfo.ColumnName = "JCE_Ledger";
			zJCE_LedgerDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_LedgerDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_TransportModeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("17911b89-ce4d-4ef2-86c2-ade45a7c04ac", "Transport");
			zJCE_TransportModeDropEditColumnStyleInfo.ColumnName = "JCE_TransportMode";
			zJCE_TransportModeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_TransportModeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_ServiceDirectionDropEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("183ea94b-981d-41d1-a08b-b5f8b7e0b6df", "Direction");
			zJCE_ServiceDirectionDropEditColumnStyleInfo.ColumnName = "JCE_ServiceDirection";
			zJCE_ServiceDirectionDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_ServiceDirectionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo.ColumnName = "JCE_Calc_CurrencyType";
			zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo.WordWrap = true;
			zJCE_PreferenceDropEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("af5dd204-62e7-49d7-b4ac-7ea46efcfb29", "Preference");
			zJCE_PreferenceDropEditColumnStyleInfo.ColumnName = "JCE_Preference";
			zJCE_PreferenceDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_PreferenceDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_OffsetCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			zJCE_OffsetCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cee00cce-fd9f-4d3e-b3c3-e470cdd594a4", "Offset");
			zJCE_OffsetCalcEditColumnStyleInfo.ColumnName = "JCE_Offset";
			zJCE_OffsetCalcEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_OffsetCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zJCE_PromptCheckBoxColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42b71f7c-e35c-4805-b35b-436e29697064", "Prompt");
			zJCE_PromptCheckBoxColumnStyleInfo.ColumnName = "JCE_Prompt";
			zJCE_PromptCheckBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			zJCE_PromptCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MasterGrid.ColumnStyles.Add(zLevelNameTextBoxColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_InvoiceCurrencyTypeDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_JobTypeDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_LedgerDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_TransportModeDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_ServiceDirectionDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_Calc_CurrencyTypeDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_PreferenceDropEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_OffsetCalcEditColumnStyleInfo);
			this.MasterGrid.ColumnStyles.Add(zJCE_PromptCheckBoxColumnStyleInfo);
			this.MasterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterGrid.GridId = "A723C2FC-0D91-457B-BDCE-EF5C1C0B37C4";
			this.MasterGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MasterGrid.LayoutKey = "MasterGrid";
			this.MasterGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterGrid.Name = "MasterGrid";
			this.MasterGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 442, true);
			this.MasterGrid.TabIndex = 10;
			// 
			// AccExRateConfigs
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccExRateConfigsSplitContainer);
			this.Name = "AccExRateConfigs";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2336, 973, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccExRateConfigsSplitContainer.Panel1.ResumeLayout(false);
			this.AccExRateConfigsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AccExRateConfigsSplitContainer)).EndInit();
			this.AccExRateConfigsSplitContainer.ResumeLayout(false);
			this.AccExRateConfigsSplitContainer.PerformLayout();
			this.CurrencyGroupBox.ResumeLayout(false);
			this.CurrencyGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CurrencyGrid)).EndInit();
			this.CurrencyGrid.ResumeLayout(false);
			this.CurrencyGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MasterGrid)).EndInit();
			this.MasterGrid.ResumeLayout(false);
			this.MasterGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
