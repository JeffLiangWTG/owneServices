using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLineLookups : WhsDocketLineLookups
	{
		public WhsAdjustmentLineLookups(WhsAdjustmentLine parent)
			: base(parent)
		{
		}

		#region AdjustmentReasonCodes

		public CodeDescriptionPairList AdjustmentReasonCodes
		{
			get { return GetReasonCodes(); }
		}

		CodeDescriptionPairList GetReasonCodes()
		{
			var reasonCodes = WarehouseDataRegistry.Instance.AdjustmentReasonCodes.Value.GetCodeDescriptionPairList();
			var docket = Parent.Docket;
			if (docket != null)
			{
				var adjustmentType = docket.WD_DocketSubType;
				if (adjustmentType == AdjustmentType.Codes.OwnershipAdjustment)
				{
					reasonCodes.Clear();
					reasonCodes.AddPair(AdjustmentReasonCodesCodeList.Codes.ChangeOwnership, AdjustmentReasonCodesCodeList.Descriptions.ChangeOwnership);
				}
				else
				{
					reasonCodes.RemoveCode(AdjustmentReasonCodesCodeList.Codes.ChangeOwnership);
				}
			}

			return reasonCodes;
		}

		#endregion

		#region OrgSupplierParts

		protected override void AddFiltersToResultCore(OrgSupplierPartCollection result)
		{
			if (Parent.WE_TransactionQuantity < 0)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Active Status", "Property", (ZString)(NoResString)"All"));
			}
		}

		protected override string GetCachedValueKeyString(OrgHeader client)
		{
			var isQuantityNegative = Parent.WE_TransactionQuantity < 0;
			return base.GetCachedValueKeyString(client) + isQuantityNegative.ToString();
		}

		protected override PartFilterOptions FilterOptionsForPartCollection
		{
			get
			{
				return Parent.WE_TransactionQuantity < 0
					? base.FilterOptionsForPartCollection | PartFilterOptions.IncludeInActive
					: base.FilterOptionsForPartCollection;
			}
		}

		#endregion

		#region InventoryStatuses

		protected override CodeDescriptionPairList GetInventoryStatusesCore()
		{
			var docket = Parent.Docket;
			return Factory.GetCachedValue(string.Format(Culture.Invariant, "WhsAdjustmentLineLookups|InventoryStatuses|{0}", Parent.WE_WD), // Key used in Factory Cache
				() =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(InventoryStatus.Codes.Available, InventoryStatus.Descriptions.Available);
					result.AddPair(InventoryStatus.Codes.Held, InventoryStatus.Descriptions.Held);

					if (docket != null && !docket.WD_WD_ParentDocket.IsEmpty && docket.WD_DocketSubType == AdjustmentType.Codes.Adjustment)
					{
						result.AddPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);
						result.AddPair(InventoryStatus.Codes.ReadyToPack, InventoryStatus.Descriptions.ReadyToPack);
					}

					return result;
				});
		}

		#endregion

		#region Locations

		protected override void AddLocationFiltersToResult(WhsLocationCollection result)
		{
			if (Parent.IsCustomsTransaction && ((WhsAdjustmentLine)Parent).IsAdjustmentIn)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(WhsLocationCollection.FilterSchema.BondedLocation, "Property0", ZBool.True));
			}
		}

		#endregion
	}
}
