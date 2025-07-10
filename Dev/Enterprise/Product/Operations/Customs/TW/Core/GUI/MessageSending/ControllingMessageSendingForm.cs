using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ControllingMessageSendingForm : TWMessageSendingForm
	{
		public ControllingMessageSendingForm()
		{
		}

		public ControllingMessageSendingForm(BaseMessageSendingObjectParent declarationWrapper)
		: base(declarationWrapper)
		{
		}

		public new ControllingMessageSendingObjectParent BusinessEntity => (ControllingMessageSendingObjectParent)base.BusinessEntity;

		public override BusinessObject TopBusinessObject => BusinessEntity.Declaration;

		public override string FormVerb => EDIMenu.Constants.SendApplicationMessageForCertificateOfOrigin;

		protected override ZBool ShouldRunPreSendValidation => BusinessEntity.MessageType != ControllingMessageTypeList.Codes.X101;

		protected override void InitializeNewColumns()
		{
			base.InitializeNewColumns();
			var messageTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageTypeTextBoxColumnStyleInfo.ColumnName = AutoControllingMessageSendingObject.Schema.MessageType;
			messageTypeTextBoxColumnStyleInfo.GroupName = Res.GetData("70F67CC1-0B0D-40B1-8743-1D97112E9875", "Message Type");
			messageTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			messageTypeTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeTextBoxColumnStyleInfo);

			var messageTypeDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageTypeDescriptionTextBoxColumnStyleInfo.ColumnName = AutoControllingMessageSendingObject.Schema.Description;
			messageTypeDescriptionTextBoxColumnStyleInfo.GroupName = Res.GetData("70F67CC1-0B0D-40B1-8743-1D97112E9875", "Message Type");
			messageTypeDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeDescriptionTextBoxColumnStyleInfo);

			var messageNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageNumberTextBoxColumnStyleInfo.ColumnName = AutoControllingMessageSendingObject.Schema.MessageNumber;
			messageNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageNumberTextBoxColumnStyleInfo);
		}
	}
}
