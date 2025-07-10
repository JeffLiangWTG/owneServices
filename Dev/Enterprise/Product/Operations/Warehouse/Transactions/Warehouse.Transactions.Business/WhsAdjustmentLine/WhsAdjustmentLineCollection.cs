using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLineCollection : WhsDocketLineCollection
	{
		public WhsAdjustmentLineCollection(WhsAdjustment master)
			: base(master)
		{
		}

		public WhsAdjustmentLineCollection(WhsAdjustment master, ZQuery filter)
			: base(master, filter)
		{
		}

		public new WhsAdjustmentLine this[int index]
		{
			get { return (WhsAdjustmentLine)(base[index]); }
		}

		#region AddNew

		public virtual new WhsAdjustmentLine AddNew()
		{
			return (WhsAdjustmentLine)base.AddNew();
		}

		#endregion

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(WhsDocketLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var line = (WhsAdjustmentLine)newElement;
			var docket = Docket;
			if (docket != null)
			{
				var warehouse = docket.Warehouse;
				if (warehouse != null && warehouse.WW_IsVirtualWarehouse && warehouse.DefaultLocation != null)
				{
					line.WE_WL = warehouse.DefaultLocation.PK;
				}

				if (docket.WD_DocketSubType == AdjustmentType.Codes.OwnershipAdjustment)
				{
					line.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.ChangeOwnership;
				}
			}
		}

		#endregion

		#region AllowNew

		protected override bool AllowNew
		{
			get { return base.AllowNew && Adjustment != null && !Adjustment.IsNewOwnershipAdjustmentChild; }
		}

		#endregion

		#region Adjustment

		WhsAdjustment Adjustment
		{
			get { return (WhsAdjustment)Docket; }
		}

		#endregion
	}
}
