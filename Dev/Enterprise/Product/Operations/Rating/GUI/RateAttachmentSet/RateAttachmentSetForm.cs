using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateAttachmentSetForm : ZForm
	{
		public RateAttachmentSetForm(RateAttachmentSet rateAttachmentSet)
			: base(rateAttachmentSet)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public override string FormCaption
		{
			get { return Env.Registry.Rating.QuoteTitleText + " " + Res.GetString("Rating|RateAttachmentSetForm|FormCaptionSuffix", "Document Attachment"); }
		}
	}
}

