using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class NXMDocumentForm : MessageSendingFormWithValidationDetails
	{
		LicensingMessageSendingObjectParent DeclarationWrapper { get; }

		public NXMDocumentForm(LicensingMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
		{
			DeclarationWrapper = Argument.NotNull(declarationWrapper, nameof(declarationWrapper));
			InitializeComponent();
			InitializeNewColumns();
		}

		protected override bool SendWithAdditionalWarningCheckBoxVisible => false;

		public new LicensingMessageSendingObjectParent BusinessEntity => (LicensingMessageSendingObjectParent)base.BusinessEntity;

		void InitializeNewColumns()
		{
			if (!DeclarationWrapper.IsSupportingDocumentsNeededMessage)
			{
				EDocsGroupBox.Visible = false;
				EDocsGroupBox.Enabled = false;
				messageSendingObjectsGroupBox.Dock = DockStyle.Fill;
			}

			var actionDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			actionDropEditColumnStyleInfo.ColumnName = "Action";
			actionDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			actionDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			actionDropEditColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(actionDropEditColumnStyleInfo);

			var messageTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageTypeTextBoxColumnStyleInfo.ColumnName = "MessageType";
			messageTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeTextBoxColumnStyleInfo);

			var descriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			descriptionTextBoxColumnStyleInfo.ColumnName = "Description";
			descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			MessageSendingObjectsGrid.ColumnStyles.Add(descriptionTextBoxColumnStyleInfo);

			var messageNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageNumberTextBoxColumnStyleInfo.ColumnName = "MessageNumber";
			messageNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			MessageSendingObjectsGrid.ColumnStyles.Add(messageNumberTextBoxColumnStyleInfo);

			if (DeclarationWrapper.MessageType == ControllingMessageTypeList.Codes.NX101)
			{
				var certificateTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				certificateTypeTextBoxColumnStyleInfo.ColumnName = "CertificateType";
				certificateTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
				MessageSendingObjectsGrid.ColumnStyles.Add(certificateTypeTextBoxColumnStyleInfo);
			}
			else
			{
				var businessTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				businessTypeTextBoxColumnStyleInfo.ColumnName = "BusinessType";
				businessTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
				MessageSendingObjectsGrid.ColumnStyles.Add(businessTypeTextBoxColumnStyleInfo);
			}

			var processingUnitTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			processingUnitTextBoxColumnStyleInfo.ColumnName = "ProcessingUnit";
			processingUnitTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			MessageSendingObjectsGrid.ColumnStyles.Add(processingUnitTextBoxColumnStyleInfo);

			if (BusinessEntity?.ShowReasonDescription ?? false)
			{
				var reasonDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				reasonDescriptionColumnStyleInfo.ColumnName = "ReasonDescription";
				reasonDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
				MessageSendingObjectsGrid.ColumnStyles.Add(reasonDescriptionColumnStyleInfo);

				var documentLineNumberColumnStyleInfo = new ZCalcEditColumnStyleInfo();
				documentLineNumberColumnStyleInfo.ColumnName = "LineNumber";
				documentLineNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				documentLineNumberColumnStyleInfo.MaxValue = 99;
				SupportingDocumentsGrid.ColumnStyles.Add(documentLineNumberColumnStyleInfo);
			}
		}
	}
}
