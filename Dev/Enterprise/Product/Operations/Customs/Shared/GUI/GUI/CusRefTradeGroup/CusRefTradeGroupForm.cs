using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefTradeGroupForm : ZForm
	{
		public CusRefTradeGroupForm(CusRefTradeGroup code) : base(code)
		{
			InitializeComponent();
			TradeGroup = code;
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
		}

		CusRefTradeGroup TradeGroup { get; }

		public override string FormCaption => TradeGroup.HumanReadableName;
	}
}
