using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class MessageSendingEntryLinesGridUserControl : ZUserControl
{
	public MessageSendingEntryLinesGridUserControl()
	{
		InitializeComponent();
		InitializeNewColumns();
	}

	void InitializeNewColumns()
	{
		var sendInfo = new ZCheckBoxColumnStyleInfo();
		var entryLineNoInfo = new ZCalcEditColumnStyleInfo();
		var tariffCodeInfo = new ZTextBoxColumnStyleInfo();
		var descriptionInfo = new ZTextBoxColumnStyleInfo();
		var quotaOrdNoInfo = new ZTextBoxColumnStyleInfo();
		var quotaQuantityInfo = new ZCalcEditColumnStyleInfo();
		var supUqInfo = new ZTextBoxColumnStyleInfo();

		sendInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.Send;
		sendInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
		EntryLinesGrid.ColumnStyles.Add(sendInfo);

		entryLineNoInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.LineNumber;
		entryLineNoInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		EntryLinesGrid.ColumnStyles.Add(entryLineNoInfo);

		tariffCodeInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.TariffCode;
		tariffCodeInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		EntryLinesGrid.ColumnStyles.Add(tariffCodeInfo);

		descriptionInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.Description;
		descriptionInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
		EntryLinesGrid.ColumnStyles.Add(descriptionInfo);

		quotaOrdNoInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.QuotaOrdNo;
		quotaOrdNoInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		EntryLinesGrid.ColumnStyles.Add(quotaOrdNoInfo);

		quotaQuantityInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.QuotaQuantity;
		quotaQuantityInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
		EntryLinesGrid.ColumnStyles.Add(quotaQuantityInfo);

		supUqInfo.ColumnName = AutoJobDeclarationMessageSendingEntryLine.Schema.SupUq;
		supUqInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
		EntryLinesGrid.ColumnStyles.Add(supUqInfo);
	}
}
