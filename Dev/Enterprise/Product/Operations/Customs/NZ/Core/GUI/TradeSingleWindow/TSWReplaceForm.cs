using Enterprise.Customs.NZ.Business.TradeSingleWindow;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWReplaceForm : TSWSendFormWithAttachments
	{
		public TSWReplaceForm()
		{
		}

		public TSWReplaceForm(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public override string FormCaption
		{
			get { return Res.GetString("283A6F4A-613D-4DC3-9EE3-0EBE3C882BE2", "Replace TSW Entry"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
