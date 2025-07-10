using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class PromtAmendmentForm : PromptDeclarationForm
	{
		public PromtAmendmentForm(AdditionalMessageInformation additionalMessageInformation, bool isExtendingAmendmentReasonVisible)
			: base(additionalMessageInformation)
		{
			ReasonForExtendingTemporaryImportPeriodInfoTextBox.Visible = isExtendingAmendmentReasonVisible;
			ReasonForExtendingTemporaryImportPeriodInfoLabel.Visible = isExtendingAmendmentReasonVisible;
		}

		public PromtAmendmentForm()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
