using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExchangeRateWrapperForm : ZForm, IButtonNewTextOverride
	{
		internal Enterprise.ZArchitecture.ZGrid ExchangeRateWrapperGrid;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ExchangeRateWrapperGrid = new Enterprise.ZArchitecture.ZGrid(false);
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRateWrapperGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 348, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(356);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(357);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.BulkExchangeRateUpdater);
			// 
			// ExchangeRateWrapperGrid
			// 
			this.ExchangeRateWrapperGrid.AllowNavigation = false;
			this.ExchangeRateWrapperGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExchangeRateWrapperGrid, "ExchangeRateWrappers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).CurrencyNK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).BuyStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).BuyExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).BuyRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).SellStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).SellExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ExchangeRateWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.BulkExchangeRateUpdater)(null)).ExchangeRateWrappers)).SyncRoot)).SellRate)));
			this.ExchangeRateWrapperGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|d82561b8-d7a3-4636-8168-682353db2e74", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CurrencyNK";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|2db58144-c7c0-4ca0-a1a1-69ff068c315a", "Buy Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "BuyStartDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|9780f4d1-177b-45ba-82c4-c735d8f50eba", "Buy Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "BuyExpiryDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|5c284a72-a022-4c6a-958d-24b28dc29aab", "Buy Rate");
			zCalcEditColumnStyleInfo1.ColumnName = "BuyRate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|730a4ea5-469e-4751-94a2-d20886bee190", "Sell Start Date");
			zDateEditColumnStyleInfo3.ColumnName = "SellStartDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|b96a31a8-49ba-4d42-81d2-a344c3c7126f", "Sell Expiry Date");
			zDateEditColumnStyleInfo4.ColumnName = "SellExpiryDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|53b87518-dfc9-4978-b497-7790b2f2ebd2", "Sell Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "SellRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ExchangeRateWrapperGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ExchangeRateWrapperGrid.GridId = "2f305112-f223-4491-8564-0f16948e28dd";
			this.ExchangeRateWrapperGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExchangeRateWrapperGrid.LayoutKey = "ExchangeRateWrapperGrid";
			this.ExchangeRateWrapperGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.ExchangeRateWrapperGrid.Name = "ExchangeRateWrapperGrid";
			this.ExchangeRateWrapperGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.ExchangeRateWrapperGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 306, true);
			this.ExchangeRateWrapperGrid.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 320, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// ExchangeRateWrapperForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 371, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExchangeRateWrapperForm|ee19d600-8f55-4247-bd1e-0fedf11afc8f", "Exchange Rate Bulk Update");
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.ExchangeRateWrapperGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.BulkExchangeRateUpdater);
			this.Name = "ExchangeRateWrapperForm";
			this.Controls.SetChildIndex(this.ExchangeRateWrapperGrid, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExchangeRateWrapperGrid)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
