using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class PalletTransactionForm : ZTemplateForm
	{
		public PalletTransactionForm(PkgPalletTransaction transaction)
			: base(transaction)
		{
			InitializeComponent();
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}
	}
}
