using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class OCRCancelForm : TSWOriginalForm
	{
		public OCRCancelForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public OCRCancelForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("954A88D4-B240-4D56-B1BA-04FDA39BB2F8", "Cancel OCR"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
