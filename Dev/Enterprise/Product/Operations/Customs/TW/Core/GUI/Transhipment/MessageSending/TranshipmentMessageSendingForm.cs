using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TranshipmentMessageSendingForm : TWMessageSendingForm
	{
		public TranshipmentMessageSendingForm()
		{
		}

		public TranshipmentMessageSendingForm(BaseMessageSendingObjectParent declarationWrapper)
		: base(declarationWrapper)
		{
		}

		protected override void InitializeNewColumns()
		{
			base.InitializeNewColumns();
			var actionDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			actionDropEditColumnStyleInfo.ColumnName = AutoTranshipmentMessageSendingObject.Schema.Action;
			actionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			actionDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			actionDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(actionDropEditColumnStyleInfo);

			var entryNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryNumberTextBoxColumnStyleInfo.ColumnName = AutoTranshipmentMessageSendingObject.Schema.EntryNumber;
			entryNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			entryNumberTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(entryNumberTextBoxColumnStyleInfo);

			var entryStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			entryStatusTextBoxColumnStyleInfo.ColumnName = AutoTranshipmentMessageSendingObject.Schema.EntryStatus;
			entryStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			MessageSendingObjectsGrid.ColumnStyles.Add(entryStatusTextBoxColumnStyleInfo);

			var statusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			statusTextBoxColumnStyleInfo.ColumnName = AutoTranshipmentMessageSendingObject.Schema.MessageStatus;
			statusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			MessageSendingObjectsGrid.ColumnStyles.Add(statusTextBoxColumnStyleInfo);
		}

		public override BusinessObject TopBusinessObject => ((TranshipmentMessageSendingObjectParent)BusinessEntity).Header;

		public override string FormVerb => TranshipmentMessageMenuItem.Constants.Caption.SendTranshipmentApplication;
	}
}
