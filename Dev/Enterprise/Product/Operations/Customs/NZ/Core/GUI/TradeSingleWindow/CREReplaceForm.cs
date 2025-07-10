using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class CREReplaceForm : TSWOriginalForm
	{
		public CREReplaceForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public CREReplaceForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("9391DC62-F342-4AB9-8304-4CE1495E60E1", "Replace ICR/CRE"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
