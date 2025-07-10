using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class MessageSendingForm : MessageSendingFormWithValidationDetails
	{
		[System.Obsolete("Do not call. Only for designer use.")]
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(DMOMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var zTextBoxCustomsLevelColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxCustomsLevelColumnStyleInfo.ColumnName = DMOMessageSendingObject.Schema.CustomsLevel;
			zTextBoxCustomsLevelColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxCustomsLevelColumnStyleInfo);

			var zTextBoxBillNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxBillNumberColumnStyleInfo.ColumnName = DMOMessageSendingObject.Schema.BillNumber;
			zTextBoxBillNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxBillNumberColumnStyleInfo);

			var zTextBoxRepresentativeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxRepresentativeColumnStyleInfo.ColumnName = DMOMessageSendingObject.Schema.Representative;
			zTextBoxRepresentativeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxRepresentativeColumnStyleInfo);

			var zTextBoxConsigneeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxConsigneeColumnStyleInfo.ColumnName = DMOMessageSendingObject.Schema.Consignee;
			zTextBoxConsigneeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxConsigneeColumnStyleInfo);
		}
	}
}
