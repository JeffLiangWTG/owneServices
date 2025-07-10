using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module
{
	public partial class QuotationsFilterControl
	{
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo followUpDateColumnStyle = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|1a1af87f-84ba-4cd2-a1ef-568e17d445bc", "Client Code");
			zTextBoxColumnStyleInfo1.ColumnName = "TH_ClientCode";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|2006eec5-baec-4541-bee6-34159e6a1101", "Client Name");
			zTextBoxColumnStyleInfo2.ColumnName = "TH_ClientFullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|51852997-4825-4a88-8c93-c96f97fd4477", "Number");
			zTextBoxColumnStyleInfo3.ColumnName = "TH_QuoteNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|d6332963-4862-4caa-96a7-b909741e3094", "Quote Date");
			zDateEditColumnStyleInfo1.ColumnName = "TH_QuoteDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|f861f2c6-f112-4a87-95b7-cc015cc59807", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "TH_QuoteEndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|5c32d922-17da-4361-b73f-7f31dd9600ae", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "QuoteStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|c9ecb59a-aa85-4a9f-bbff-f2bfd348f659", "Status Date");
			zDateEditColumnStyleInfo3.ColumnName = "TH_StatusDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|9cef9b6c-212d-45e4-90f3-7b49c7473bea", "Sales Rep");
			zTextBoxColumnStyleInfo5.ColumnName = "Header+OverallSalesRepStaff";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|ad5a337b-00af-4e3c-902c-8cd84ce28668", "Client UNLOCO");
			zTextBoxColumnStyleInfo6.ColumnName = "Header+OH_RL_NKClosestPort";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("65b7ee7c-d1ad-4225-9219-ae112efc7954", "Cancellation Reason Code");
			zDropEditColumnStyleInfo1.ColumnName = "TH_QuoteCancellationReason";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("e6356244-66cc-471b-8cc2-5410b0518f38", "Cancellation Reason");
			zTextBoxColumnStyleInfo7.ColumnName = "QuoteCancellationReasonDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			followUpDateColumnStyle.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("QuotationsFilterControl|c79a15c0-4273-4e24-a903-b35d7bf4a4ce", "Follow Up Date");
			followUpDateColumnStyle.ColumnName = RatingHeader.Schema.TH_FollowUpDate;
			followUpDateColumnStyle.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			followUpDateColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(followUpDateColumnStyle);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 425, true);
			this.grid.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RatingHeader);
			// 
			// QuotationsFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "QuotationsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
