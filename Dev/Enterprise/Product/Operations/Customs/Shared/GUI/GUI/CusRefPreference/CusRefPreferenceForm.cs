using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefPreferenceForm : ZForm
	{
		public CusRefPreferenceForm(CusRefPreference businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
		}
	}
}
