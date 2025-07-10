using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsAdjustmentLineValidation
	class AdjustmentLineValidationHelper : WhsDocketLineForPickingValidationHelperUS<WhsAdjustmentLine>
	{
		public AdjustmentLineValidationHelper(WhsAdjustmentLine adjustmentLine)
			: base(adjustmentLine)
		{
		}

		#region Properties

		protected override ZGuid ClientPK => LineAttributes.Docket.WD_OH_Client;

		protected override ZGuid ProductPK => LineAttributes.WE_OP;

		protected override ZGuid LocationPK => LineAttributes.WE_WL;

		protected override ZDecimal Units => LineAttributes.WE_TransactionQuantity;

		protected override ZDecimal PerPackageQty => LineAttributes.WE_PerPackageQty;

		protected override ZString PackageGroupID => LineAttributes.WE_PackageGroupId;

		protected override IValidateParentWithLines ProductPackageTotalsParent => null;

		#endregion

		#region Collections

		protected override WhsAdjustmentLine[] GetSiblings()
		{
			var adjustment = LineAttributes.Docket;
			return adjustment != null ? adjustment.Lines.Cast<WhsAdjustmentLine>().ToArray() : base.GetSiblings();
		}

		protected override WhsAdjustmentLine[] GetRelatedPickingLines()
		{
			return GetSiblings().Where(l => l.IsAdjustmentOut && l.WE_PackageGroupId == PackageGroupID && l.WE_WL == LineAttributes.WE_WL && l.WE_PalletID == LineAttributes.WE_PalletID).ToArray();
		}

		protected override WhsAdjustmentLine[] GetRelatedPutawayLines()
		{
			return GetSiblings().Where(l => l.IsAdjustmentIn && l.WE_PackageGroupId == PackageGroupID && l.WE_WL == LineAttributes.WE_WL && l.WE_PalletID == LineAttributes.WE_PalletID).ToArray();
		}

		#endregion

		#region IsMatching

		protected override bool IsMatching(WhsAdjustmentLine line, ProductWithAttributes packedItem)
		{
			return Docket.WD_OH_Client == packedItem.ClientPK &&
				line.WE_OP == packedItem.ProductPK &&
				line.WE_PackageGroupId == PackageGroupID &&
				line.WE_PartAttrib1 == packedItem.PartAttrib1 &&
				line.WE_PartAttrib2 == packedItem.PartAttrib2 &&
				line.WE_PartAttrib3 == packedItem.PartAttrib3 &&
				line.WE_SerialNumber == packedItem.SerialNumber &&
				line.WE_PackingDate == packedItem.PackingDate &&
				line.WE_ExpiryDate == packedItem.ExpiryDate &&
				line.WE_PerPackageQty == packedItem.PerPackageQty;
		}

		#endregion

		#region ErrorMessages

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage
		{
			get { return Res.GetString("3edbb0fa-4581-4a7f-9b6a-73c255f6b644", "Units to be adjusted must be divisible by Per Group Quantity."); }
		}

		protected override string LineDoesNotMatchAnyInventoryPackedIntoPackageGroupErrorMessage
		{
			get { return Res.GetString("7f8eb190-8424-468b-b22d-92991a42c693", "This adjustment line does not match any inventory packed into Package Group ID '{0}'.", PackageGroupID); }
		}

		protected override string OnlyFullPackagesCanBeOrderedToBePickedErrorMessage
		{
			get { return Res.GetString("7c93100d-ed00-484b-9cf1-bcec9b159597", "Only full packages (by Package Group ID) can be adjusted out from a location."); }
		}

		protected override string OnlyFullPackagesCanBePutawayInDestinationLocationErrorMessage
		{
			get { return Res.GetString("aae786af-127a-4e3f-b291-e728a5cc28be", "Only full packages (by Package Group ID) can be adjusted into a location."); }
		}

		#endregion

		#region GetHelper

		protected override WhsValidationHelperUS<WhsAdjustmentLine> GetHelper(WhsAdjustmentLine adjustmentLine)
		{
			return new AdjustmentLineValidationHelper(adjustmentLine);
		}

		#endregion
	}
}
