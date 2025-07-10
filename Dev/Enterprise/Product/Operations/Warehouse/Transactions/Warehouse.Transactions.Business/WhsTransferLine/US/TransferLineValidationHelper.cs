using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsTransferLineValidation
	class TransferLineValidationHelper : WhsDocketLineForPickingValidationHelperUS<WhsTransferLine>
	{
		public TransferLineValidationHelper(WhsTransferLine transferLine)
			: base(transferLine)
		{
		}

		protected override ZGuid LocationPK => LineAttributes.WE_WL;

		protected override IValidateParentWithLines ProductPackageTotalsParent => null;

		#region IsMatching

		protected override bool IsMatching(WhsTransferLine transferLine, ProductWithAttributes packedItem)
			{
			return Docket.WD_OH_Client == packedItem.ClientPK &&
				transferLine.WE_OP == packedItem.ProductPK &&
				transferLine.WE_PackageGroupId == PackageGroupID &&
				transferLine.WE_PartAttrib1 == packedItem.PartAttrib1 &&
				transferLine.WE_PartAttrib2 == packedItem.PartAttrib2 &&
				transferLine.WE_PartAttrib3 == packedItem.PartAttrib3 &&
				transferLine.WE_SerialNumber == packedItem.SerialNumber &&
				transferLine.WE_PackingDate == packedItem.PackingDate &&
				transferLine.WE_ExpiryDate == packedItem.ExpiryDate &&
				transferLine.WE_PerPackageQty == packedItem.PerPackageQty;
			}

		#endregion

		#region GetRelatedPickingLines

		protected override WhsTransferLine[] GetRelatedPickingLines()
		{
			return GetSiblings().Where(tl =>
				tl.WE_PackageGroupId == PackageGroupID &&
				tl.WE_WL_TransferFrom == LineAttributes.WE_WL_TransferFrom &&
				tl.WE_TransferFromPalletId == LineAttributes.WE_TransferFromPalletId).ToArray();
		}

		#endregion

		#region GetRelatedPutawayLines

		protected override WhsTransferLine[] GetRelatedPutawayLines()
		{
			return GetSiblings().Where(tl =>
				tl.WE_PackageGroupId == PackageGroupID &&
				tl.WE_WL == LineAttributes.WE_WL &&
				tl.WE_PalletID == LineAttributes.WE_PalletID).ToArray();
		}

		#endregion

		#region GetSiblings

		protected override WhsTransferLine[] GetSiblings()
		{
			var transfer = (WhsTransfer)Docket;
			return transfer.LinesToValidateWhenFinalising.ToArray();
		}

		#endregion

		#region ErrorMessages

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage
		{
			get { return Res.GetString("d35aaf8d-6880-4617-ba50-490658938e54", "Units to transfer must be divisible by Per Group Quantity."); }
		}

		protected override string LineDoesNotMatchAnyInventoryPackedIntoPackageGroupErrorMessage
		{
			get { return Res.GetString("1f7f16b4-f8f4-4d0f-b50a-54a6f27d2a27", "This transfer line does not match any inventory packed into Package Group ID '{0}'.", PackageGroupID); }
		}

		protected override string OnlyFullPackagesCanBeOrderedToBePickedErrorMessage
		{
			get { return Res.GetString("e9b2161a-70dc-469e-b31e-4cbd086eabf0", "Only full packages (by Package Group ID) can be picked from source location."); }
		}

		protected override string OnlyFullPackagesCanBePutawayInDestinationLocationErrorMessage
		{
			get { return Res.GetString("f0d41dcc-2a7b-4b1d-9f13-e887507aa99c", "Only full packages (by Package Group ID) can be putaway into destination location."); }
		}

		#endregion

		protected override WhsValidationHelperUS<WhsTransferLine> GetHelper(WhsTransferLine transferLine)
		{
			return new TransferLineValidationHelper(transferLine);
		}
	}
}
