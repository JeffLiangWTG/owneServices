using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class LoadFilterControl : ZFilterStripControl
	{
		#region Constructors

		public LoadFilterControl()
		{
			InitializeComponent();
		}

		public LoadFilterControl(WhsLoadCollection collection, LoadFilterBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#endregion
	}
}
