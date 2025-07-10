using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in WhsReceiveLineValidationUS
	public abstract class WhsDocketLineValidationHelperUS<T> : WhsValidationHelperUS<T>
		where T : WhsDocketLine
	{
		#region Constructor

		protected WhsDocketLineValidationHelperUS(T docketLine)
			: base(docketLine)
		{
			Docket = docketLine.Docket;
		}
		protected readonly WhsDocket Docket;

		#endregion

		#region CheckHoldCodeChangeQtyIsDivisibleByPerPackageQty

		public void CheckHoldCodeChangeQtyIsDivisibleByPerPackageQty(T parent)
		{
			if (!parent.HeldCodeChangeQuantityInfo.HasErrors() && parent.WE_PerPackageQty > 0m && parent.HeldCodeChangeQuantity % parent.WE_PerPackageQty != 0m)
			{
				parent.HeldCodeChangeQuantityInfo.AddError(Res.GetString("e560dd2b-b043-4d58-9357-a322461df3a3", "Quantity must be divisible by Per Group Quantity."));
			}
		}

		#endregion

		#region CheckNotChangingHoldCodeOfInventoryInAPackageGroup

		public void CheckNotChangingHoldCodeOfInventoryInAPackageGroup(T parent)
		{
			if (!parent.HeldCodeToChangeToInfo.HasErrors() && !parent.HeldCodeToChangeToInfo.Value.IsEmpty && !parent.WE_PackageGroupId.IsEmpty)
			{
				parent.HeldCodeToChangeToInfo.AddError(Res.GetString("c8537885-f329-45ad-8522-1274c1abd429", "Cannot change the Hold Code of Inventory in a Package Group."));
			}
		}

		#endregion

		#region Properties

		protected override ZGuid ClientPK => (Docket != null) ? Docket.WD_OH_Client : ZGuid.Empty;

		protected override ZGuid ProductPK => LineAttributes.WE_OP;

		protected override ZGuid LocationPK => LineAttributes.WE_WL;

		protected override ZDecimal Units => LineAttributes.WE_TransactionQuantity;

		protected override ZDecimal PerPackageQty => LineAttributes.WE_PerPackageQty;

		protected override ZString PackageGroupID => LineAttributes.WE_PackageGroupId;

		#endregion

		#region Overrides

		protected override T[] GetSiblings()
		{
			var docket = LineAttributes.Docket;
			return docket != null ? docket.Lines.Cast<T>().ToArray() : base.GetSiblings();
		}

		#endregion
	}
}
