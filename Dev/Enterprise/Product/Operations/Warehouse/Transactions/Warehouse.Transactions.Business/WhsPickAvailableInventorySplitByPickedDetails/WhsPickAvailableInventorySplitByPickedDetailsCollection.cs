using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventorySplitByPickedDetailsCollection : WhsPickAvailableInventorySplitBaseCollection<WhsPickAvailableInventorySplitByPickedDetails>
	{
		public WhsPickAvailableInventorySplitByPickedDetailsCollection(BusinessObjectFactory factory, WhsPickAvailableInventory availableInventory)
			: base(factory, availableInventory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WhsPickAvailableInventorySplitByPickedDetails(Factory);

		#region LoadForAvailableInventory

		protected override void RebuildCollectionCore(OrgSupplierPart part, IReadOnlyCollection<IPickLinePair> pickLinesToUse)
		{
			var pickLinesSplitByPickedDetails = pickLinesToUse
				.GroupBy(x => new PickLinesKey(x.PickedDateTime, x.AssignedToCode))
				.ToDictionary(x => x.Key);

			foreach (var pickLinesSplit in pickLinesSplitByPickedDetails.Values)
			{
				var newItem = AddNew();
				var pickLines = pickLinesSplit.ToArray();

				newItem.SetData(pickLines, AvailableInventory);
			}
		}

		#region PickLinesKey

		sealed class PickLinesKey
		{
			internal PickLinesKey(ZDateTimeOffset pickedDateTime, ZString assignedTo)
			{
				PickedDateTime = pickedDateTime;
				AssignedTo = assignedTo;
			}

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

				var pickLineKeyObj = obj as PickLinesKey;
				return pickLineKeyObj != null && Equals(pickLineKeyObj);
			}

			bool Equals(PickLinesKey other)
			{
				return PickedDateTime.Equals(other.PickedDateTime)
					&& AssignedTo.Equals(other.AssignedTo);
			}

			public override int GetHashCode()
			{
				// we don't care about arithmetic overflow in calculation of a hashcode.
				unchecked
				{
					var hashCode = PickedDateTime.GetHashCode();
					hashCode = (hashCode * 397) ^ AssignedTo.GetHashCode();
					return hashCode;
				}
			}
		}

		#endregion

		#endregion
	}
}
