using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class OCRReplaceWithCommentsForm : OCROriginalForm
	{
		public OCRReplaceWithCommentsForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public OCRReplaceWithCommentsForm()
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
			get { return Res.GetString("29D3E414-3388-49D7-86CF-CAB5FFE086C0", "Replace OCR"); }
		}
	}
}
