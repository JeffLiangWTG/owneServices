using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class PickManagerForWhsPick : PickManager
	{
		public PickManagerForWhsPick(ZForm form, WhsPick pick)
			: base(form)
		{
			this.pick = Argument.NotNull(pick, "WhsPick pick");
		}

		readonly WhsPick pick;

		protected override WhsPick Pick => pick;

		#region PickOrders

		protected override void PickOrdersCore()
		{
			using (new DisposableAction(() => UnhookEvents()))
			{
				HookEvents();
				Pick.PickOrders();
			}
		}

		#endregion
	}
}