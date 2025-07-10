using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class SendToCustomsMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		public SendToCustomsMessageSendingForm(MessageSendingObjectParent headerWrapper)
			: base(headerWrapper)
		{
			InitializeComponent();
			InitializeNewColumns();
		}

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;

		void InitializeNewColumns()
		{
			var actionDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			actionDropEditColumnStyleInfo.ColumnName = "Action";
			actionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			actionDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			actionDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(actionDropEditColumnStyleInfo);

			var messageTypeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageTypeTextBoxColumnStyleInfo.ColumnName = "MessageType";
			messageTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeTextBoxColumnStyleInfo);

			var descriptionTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			descriptionTextBoxColumnStyleInfo.ColumnName = "Description";
			descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			MessageSendingObjectsGrid.ColumnStyles.Add(descriptionTextBoxColumnStyleInfo);

			var entryNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			entryNumberTextBoxColumnStyleInfo.ColumnName = "EntryNumber";
			entryNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			MessageSendingObjectsGrid.ColumnStyles.Add(entryNumberTextBoxColumnStyleInfo);
		}
	}
}
