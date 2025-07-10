using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsOperationalActionSupporter : OperationalActionSupporter
	{
		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Warehouse);
		}
	}
}
