using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class CREReplaceWithCommentsForm : CREOriginalForm
	{
		public CREReplaceWithCommentsForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public CREReplaceWithCommentsForm()
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
			get { return Res.GetString("BF8A2F2A-24F2-4339-930B-8DE033AA56A5", "Replace ICR/CRE"); }
		}
	}
}
