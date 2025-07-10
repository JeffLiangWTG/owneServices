using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefRateCodeForm : ZForm
	{
		public CusRefRateCodeForm(CusRefRateCode code) : base(code)
		{
			InitializeComponent();
			CusRefRateCode = code;
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
		}

		CusRefRateCode CusRefRateCode { get; }

		public override string FormCaption => CusRefRateCode.HumanReadableName;
	}
}
