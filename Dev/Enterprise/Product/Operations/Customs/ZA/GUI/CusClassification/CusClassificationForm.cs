using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.GUI
{
	public class CusClassificationForm : BaseClassificationForm
	{
		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new CusClassificationUserControl();
		}
	}
}
