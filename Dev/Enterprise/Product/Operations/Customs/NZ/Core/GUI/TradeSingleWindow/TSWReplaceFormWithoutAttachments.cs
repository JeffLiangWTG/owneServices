using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWReplaceFormWithoutAttachments : TSWSendFormWithoutAttachments
	{
		public TSWReplaceFormWithoutAttachments()
		{
		}

		public TSWReplaceFormWithoutAttachments(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("8AF3AED7-399F-450C-82A3-3C44DE77A2F1", "Replace TSW Entry"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
