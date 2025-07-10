using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.Module
{
	public partial class PalletTransactionFilterControl : ZFilterStripControl
	{
		public PalletTransactionFilterControl()
		{
			InitializeComponent();
		}

		public PalletTransactionFilterControl(IBusinessObjectCollection gridCollection, PalletTransactionFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
