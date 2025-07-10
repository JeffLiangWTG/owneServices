using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class LinesUserAssignerActionMethod<T> : OperationalActionMethod
		where T : BusinessObject, IMasterStaffAssigner
	{
		protected LinesUserAssignerActionMethod(ZGuid guid)
			: base(guid)
		{ }

		public sealed override IComponent NewGuiControl()
		{
			return new FindUserCodeControl<T>();
		}

		public override bool HasControl => true;

		public override string Name => GetOperationalActionName();

		public override string Description => GetOperationalActionName();

		protected abstract string GetOperationalActionName();
	}
}
