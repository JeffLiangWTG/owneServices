using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class CRECancelForm : TSWOriginalForm
	{
		public CRECancelForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public CRECancelForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("3EB02BD5-35F2-4D4A-800B-E973C8BFCB36", "Cancel ICR/CRE"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
