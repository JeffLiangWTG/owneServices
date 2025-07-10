using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class FindUserCodeControl<T> : ZUserControl
		where T : BusinessObject, IMasterStaffAssigner
	{
		public FindUserCodeControl()
		{
			InitializeComponent();
		}
	}
}
