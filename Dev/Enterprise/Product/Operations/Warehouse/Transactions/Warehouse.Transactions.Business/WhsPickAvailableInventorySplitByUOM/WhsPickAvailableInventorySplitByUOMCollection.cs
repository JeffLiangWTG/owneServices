using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventorySplitByUOMCollection : WhsPickAvailableInventorySplitBaseCollection<WhsPickAvailableInventorySplitByUOM>
	{
		public WhsPickAvailableInventorySplitByUOMCollection(BusinessObjectFactory factory, WhsPickAvailableInventory availableInventory)
			: base(factory, availableInventory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WhsPickAvailableInventorySplitByUOM(Factory);

		#region RebuildCollection

		protected override void RebuildCollectionCore(OrgSupplierPart part, IReadOnlyCollection<IPickLinePair> pickLinesToUse)
		{
			var unitConversions = new ConversionsToSKUTable(part);
			var refPackTypes = part.Lookups.PackTypes;

			var orderQuery = new ZQuery();
			orderQuery.AddToFilter(WhsDocketSchema.PK, pickLinesToUse.Select(p => p.PickLineOnOrder.DocketLine.WE_WD).ToArray());

			var orderExternalReferences = Factory
				.Load<WhsPickableDocket>(orderQuery)
				.ToDictionary(order => order.PK, order => order.WD_ExternalReference);

			var pickLinesByPackType = pickLinesToUse
				.GroupBy(x => new PickLinesKey(x.PickLineOnOrder.DocketLine.WE_WD, x.PickLineOnOrder.WZ_F3_NKAllocatedPackType, x.PickedDateTime, x.AssignedToCode))
				.ToDictionary(x => x.Key);

			foreach (var pickLinesKey in pickLinesByPackType.Keys.Where(pack => !pack.PackType.IsEmpty)) // don't create a row if pack type is not allocated - should never happen
			{
				var newItem = AddNew();
				var pickLines = pickLinesByPackType[pickLinesKey].ToArray();
				var unitsQty = pickLines.Sum(pl => pl.PickLineOnOrder.WZ_Units);
				var conversion = unitConversions[pickLinesKey.PackType];

				var packQty = 0m;
				if (conversion != null && unitsQty % conversion.QtySKU == 0) // unit conversion exists and still produces whole number
				{
					packQty = unitsQty / conversion.QtySKU;
				}

				ZString uomType = "";
				if (refPackTypes.ContainsCode(pickLinesKey.PackType))
				{
					uomType = refPackTypes.Single(r => r.F3_Code == pickLinesKey.PackType).F3_UOMType;
				}

				newItem.SetData(orderExternalReferences[pickLinesKey.OrderPk], packQty, pickLinesKey.PackType, uomType, pickLines, AvailableInventory);
			}
		}

		#region PickLinesKey

		sealed class PickLinesKey
		{
			internal PickLinesKey(ZGuid orderPk, ZString packType, ZDateTimeOffset pickedDateTime, ZString assignedTo)
			{
				OrderPk = orderPk;
				PackType = packType;
				PickedDateTime = pickedDateTime;
				AssignedTo = assignedTo;
			}

			public readonly ZGuid OrderPk;
			public readonly ZString PackType;
			readonly ZDateTimeOffset PickedDateTime;
			readonly ZString AssignedTo;

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				var pickLinesKeyObj = obj as PickLinesKey;

				return pickLinesKeyObj != null && Equals(pickLinesKeyObj);
			}

			bool Equals(PickLinesKey other)
			{
				return PackType.Equals(other.PackType)
					&& PickedDateTime.Equals(other.PickedDateTime)
					&& AssignedTo.Equals(other.AssignedTo)
					&& OrderPk.Equals(other.OrderPk);
			}

			public override int GetHashCode()
			{
				// we don't care about arithmetic overflow in calculation of a hashcode.
				unchecked
				{
					var hashCode = PackType.GetHashCode();
					hashCode = (hashCode * 397) ^ PickedDateTime.GetHashCode();
					hashCode = (hashCode * 397) ^ AssignedTo.GetHashCode();
					hashCode = (hashCode * 397) ^ OrderPk.GetHashCode();
					return hashCode;
				}
			}
		}

		#endregion

		#endregion
	}
}
