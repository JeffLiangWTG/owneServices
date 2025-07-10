using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class OCROriginalForm : TSWOriginalForm
	{
		public OCROriginalForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public OCROriginalForm()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("B5B9A7B7-EBD9-4232-986E-93344A57BBB5", "Send OCR"); }
		}
	}
}
