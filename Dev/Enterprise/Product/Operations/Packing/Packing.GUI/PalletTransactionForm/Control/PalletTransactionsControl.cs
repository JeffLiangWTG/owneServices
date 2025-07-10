using System.ComponentModel;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.GUI
{
	public partial class PalletTransactionsControl : ZUserControl
	{
		public PalletTransactionsControl()
		{
			InitializeComponent();
			PalletTransactionsGrid.HookDoubleClickToOpenJob<PkgPalletTransaction>(ControllerIDs.PalletTransaction);
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return PalletTransactionsGrid.BindTo; }
			set { PalletTransactionsGrid.BindTo = value; }
		}
	}
}
