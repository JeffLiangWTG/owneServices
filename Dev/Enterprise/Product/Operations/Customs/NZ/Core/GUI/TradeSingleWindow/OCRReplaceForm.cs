using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class OCRReplaceForm : TSWOriginalForm
	{
		public OCRReplaceForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public OCRReplaceForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("81C9C43B-275A-4439-90D1-D8176F549E8F", "Replace OCR"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
