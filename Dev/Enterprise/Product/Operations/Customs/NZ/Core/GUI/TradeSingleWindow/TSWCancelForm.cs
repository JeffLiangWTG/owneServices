using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWCancelForm : TSWSendFormWithAttachments
	{
		public TSWCancelForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
			SetFormVisibility();
		}

		public TSWCancelForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("4E6B71E2-E55A-44F5-9A05-504AC104CF0A", "Cancel TSW Entry"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SetFormVisibility()
		{
			AdditionalInformationGroupBox.Controls.Remove(FreeTextTextBox);
			AdditionalInformationGroupBox.Controls.Remove(ManualProcessingTextBox);
			AdditionalInformationGroupBox.Controls.Remove(ManualProcessingLabel);
		}
	}
}
