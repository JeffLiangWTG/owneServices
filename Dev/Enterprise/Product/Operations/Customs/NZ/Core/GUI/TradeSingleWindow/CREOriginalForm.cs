using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class CREOriginalForm : TSWOriginalForm
	{
		public CREOriginalForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public CREOriginalForm()
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
			get { return Res.GetString("F7C6F323-DF7B-4490-B28B-D96DABF59DF7", "Send ICR/CRE"); }
		}
	}
}
