using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CusLiquidationForm : ZTemplateForm, IPostingButtonsProvider
	{
		public CusLiquidationForm(CusLiquidation liquidation)
			: base(liquidation)
		{
		}

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}
	}
}
