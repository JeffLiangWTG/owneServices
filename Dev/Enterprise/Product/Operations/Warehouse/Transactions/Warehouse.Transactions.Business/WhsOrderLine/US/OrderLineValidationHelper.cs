using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsOrderLineValidationUS
	class OrderLineValidationHelperUS : WhsDocketLineForPickingValidationHelperUS<WhsOrderLine>
	{
		public OrderLineValidationHelperUS(WhsOrderLine orderLine)
			: base(orderLine)
		{
		}

		#region ChechOrderingNonExistingPackageGroupID

		public void ChechOrderingNonExistingPackageGroupID(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && Docket != null && !Docket.IsFinalisedOrCancelled && !PackageGroupID.IsEmpty && PackageGroupContent.Count == 0)
			{
				propertyInfo.AddError(Res.GetString("2673ec90-5c3f-4679-b191-b8b4d0b47b50",
					"No package with Package Group ID '{0}' exists in the warehouse.", PackageGroupID));
			}
		}

		#endregion

		#region GetRelatedPickingLines

		protected override WhsOrderLine[] GetRelatedPickingLines()
		{
			return GetSiblings().Where(ol => ol.WE_PackageGroupId == PackageGroupID).ToArray();
		}

		#endregion

		#region GetRelatedPutawayLines

		protected override WhsOrderLine[] GetRelatedPutawayLines()
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region Properties

		protected override ZGuid LocationPK => ZGuid.Empty;

		protected override IValidateParentWithLines ProductPackageTotalsParent => null;

		#region PerPackageQty

		protected override ZDecimal PerPackageQty
		{
			get
			{
				if (!perPackageQty.HasValue)
				{
					perPackageQty = PackageGroupContent.Where(i => IsMatching(LineAttributes, i.Key)).Sum(i => i.Value);
				}
				return perPackageQty.Value;
			}
		}

		protected override bool IsMatching(WhsOrderLine orderLine, ProductWithAttributes packedItem)
		{
			return Docket.WD_OH_Client == packedItem.ClientPK &&
				orderLine.WE_OP == packedItem.ProductPK &&
				(orderLine.WE_PackageGroupId.IsEmpty || orderLine.WE_PackageGroupId == PackageGroupID) &&
				(orderLine.WE_PartAttrib1.IsEmpty || orderLine.WE_PartAttrib1 == packedItem.PartAttrib1) &&
				(orderLine.WE_PartAttrib2.IsEmpty || orderLine.WE_PartAttrib2 == packedItem.PartAttrib2) &&
				(orderLine.WE_PartAttrib3.IsEmpty || orderLine.WE_PartAttrib3 == packedItem.PartAttrib3) &&
				(orderLine.WE_PackingDate.IsEmpty || orderLine.WE_PackingDate == packedItem.PackingDate) &&
				(orderLine.WE_ExpiryDate.IsEmpty || orderLine.WE_ExpiryDate == packedItem.ExpiryDate);
		}

		ZDecimal? perPackageQty;

		#endregion

		#region ErrorMessages

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage
		{
			get { return Res.GetString("74f7e994-192b-4907-b4d4-90c629ec67c1", "Units ordered must be divisible by the sum of Per Group Quantities of all matching inventory from the package."); }
		}

		protected override string LineDoesNotMatchAnyInventoryPackedIntoPackageGroupErrorMessage
		{
			get { return Res.GetString("e0926898-ee75-4f27-a47f-84e805bb4e63", "This order line does not match any inventory packed into Package Group ID '{0}'.", PackageGroupID); }
		}

		protected override string OnlyFullPackagesCanBeOrderedToBePickedErrorMessage
		{
			get { return Res.GetString("ecafa48f-2506-44d4-aef4-27f9a5504b1b", "Total Package Count inconsistent for Package Group ID. Check the Quantity ordered."); }
		}

		protected override string OnlyFullPackagesCanBePutawayInDestinationLocationErrorMessage
		{
			get { return ""; }
		}

		#endregion

		protected override WhsValidationHelperUS<WhsOrderLine> GetHelper(WhsOrderLine orderLine)
		{
			return new OrderLineValidationHelperUS(orderLine);
		}

		#endregion
	}
}
