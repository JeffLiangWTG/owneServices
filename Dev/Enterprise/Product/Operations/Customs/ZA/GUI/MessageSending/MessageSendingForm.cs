using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using JobDeclarationMessageSendingObjectParent = Enterprise.Customs.ZA.Business.JobDeclarationMessageSendingObjectParent;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class MessageSendingForm : MessageSendingObjectForm
	{
		ZTextBox vOCReasonTextBox;
		ZGroupBox vOCReasonGroupBox;
		ZLabel amountDueLabel;
		ZLabel valuAddedTaxLabel;
		ZLabel sch1Pt2BDutyLabel;
		ZLabel customsDutyLabel;
		ZLabel customsValueLabel;
		ZLabel cIFValueLabel;
		ZLabel vOCDifferenceLabel;
		ZLabel vOCBeforeLabel;
		ZLabel vOCAfterLabel;
		ZCalcEdit amountDueDifferenceCalcEdit;
		ZCalcEdit valueAddedTaxDifferenceCalcEdit;
		ZCalcEdit amountDueBeforeCalcEdit;
		ZCalcEdit amountDueAfterCalcEdit;
		ZCalcEdit valueAddedTaxBeforeCalcEdit;
		ZCalcEdit valueAddedTaxCalcEdit;
		ZCalcEdit s1P2BDutyDifferenceCalcEdit;
		ZCalcEdit customsDutyNoS1P2BDifferenceCalcEdit;
		ZCalcEdit s1P2BDutyBeforeCalcEdit;
		ZCalcEdit s1P2BDutyAfterCalcEdit;
		ZCalcEdit customsDutyNoS1P2BBeforeCalcEdit;
		ZCalcEdit customsDutyNoS1P2BAfterCalcEdit;
		ZCalcEdit customsValueDifferenceCalcEdit;
		ZCalcEdit cIFValueDifferenceCalcEdit;
		ZCalcEdit customsValueBeforeCalcEdit;
		ZCalcEdit customsValueAfterCalcEdit;
		ZCalcEdit cIFValueBeforeCalcEdit;
		ZCalcEdit cIFValueAfterCalcEdit;
		ZTextBoxColumnStyleInfo vocReasonzTextBoxColumnStyleInfo;
		ZGroupBox vOCValuesGroupBox;
		ZLabel provisionalPaymentLabel;
		ZCalcEdit provisionalPaymentDifferenceCalcEdit;
		ZCalcEdit provisionalPaymentBeforeCalcEdit;
		ZCalcEdit provisionalPaymentAfterCalcEdit;
		ZCalcEdit penaltyAfterCalcEdit;
		ZCalcEdit penaltyBeforeCalcEdit;
		ZCalcEdit penaltyDefferenceCalcEdit;
		ZLabel penaltyLabel;
		ZPanel vOCPanel;

		public MessageSendingForm()
		{
		}

		public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper)
			: base(declarationWrapper)
		{
			InitializeComponent();
			penaltyLabel.AllowOverlap(provisionalPaymentLabel);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			ZDropEditColumnStyleInfo messageTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			messageTypeDropEditColumnStyleInfo.ColumnName = MessageSendingObject.Schema.MessageType;
			messageTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			messageTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			messageTypeDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(messageTypeDropEditColumnStyleInfo);

			ZTextBoxColumnStyleInfo cPCTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			cPCTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.CustomsProcedureCode;
			cPCTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			cPCTextBoxColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(cPCTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo lRNTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			lRNTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.LocalReferenceNumber;
			lRNTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(lRNTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo paymentCodeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			paymentCodeTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.PaymentMethod;
			paymentCodeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(paymentCodeTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo mRNTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			mRNTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.MovementReferenceNumber;
			mRNTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(mRNTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo ucrTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ucrTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.UniqueConsignmentReferenceNumber;
			ucrTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(ucrTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo statusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			statusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.EntryStatus;
			statusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(statusTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo messageStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageStatusTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.MessageStatus;
			messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(messageStatusTextBoxColumnStyleInfo);

			ZDropEditColumnStyleInfo caseNumberDropEditStyleInfo = new ZDropEditColumnStyleInfo();
			caseNumberDropEditStyleInfo.ColumnName = MessageSendingObject.Schema.CaseNumber;
			caseNumberDropEditStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(caseNumberDropEditStyleInfo);

			ZDropEditColumnStyleInfo changeIndicatorDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			changeIndicatorDropEditColumnStyleInfo.ColumnName = MessageSendingObject.Schema.ChangeAcknowledgementIndicator;
			changeIndicatorDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(changeIndicatorDropEditColumnStyleInfo);

			vocReasonzTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			vocReasonzTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.VOCReason;
			vocReasonzTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(vocReasonzTextBoxColumnStyleInfo);

			var declarationTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			declarationTypeDropEditColumnStyleInfo.ColumnName = MessageSendingObject.Schema.DeclarationType;
			declarationTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			declarationTypeDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			declarationTypeDropEditColumnStyleInfo.IsMandatory = true;
			this.MessageSendingObjectsGrid.ColumnStyles.Add(declarationTypeDropEditColumnStyleInfo);
		}

		public new JobDeclarationMessageSendingObjectParent BusinessEntity => (JobDeclarationMessageSendingObjectParent)base.BusinessEntity;

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			MessageSendingNotificationCollection result;
			var businessEntity = this.BusinessEntity;
			var declaration = businessEntity.ParentDeclaration;
			if (declaration == null)
			{
				result = base.RunPreSendValidation();
			}
			else
			{
				var decIsTopLevel = declaration.IsTopLevel;
				try
				{
					declaration.IsTopLevel = false;
					businessEntity.RegisterEditableChildObject(declaration);
					result = base.RunPreSendValidation();
				}
				finally
				{
					declaration.IsTopLevel = decIsTopLevel;
					businessEntity.UnRegisterEditableChildObject(declaration);
				}
			}
			return result;
		}

		protected override bool CheckIsOKToSend()
		{
			if (BusinessEntity.IsMessagingPOC)
			{
				return true;
			}
			else
			{
				return base.CheckIsOKToSend();
			}
		}
	}
}

