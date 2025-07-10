using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class PackableItemsSplitter : NonPersistentBusinessObject, ISequenceNumberHeader
	{
		public PackableItemsSplitter(CusPackage package, CusPackageCusPackableItemRelation packItemRelation)
			: base(package.Factory)
		{
			Package = Argument.NotNull(package, nameof(package));
			PackItemRelation = Argument.NotNull(packItemRelation, nameof(packItemRelation));
			PackableItem = Argument.NotNull(PackItemRelation.PackableItem, nameof(PackItemRelation.PackableItem));
		}

		public readonly CusPackage Package;
		public readonly CusPackageCusPackableItemRelation PackItemRelation;
		public readonly CusPackableItem PackableItem;

		[ChildEditable]
		public NonPersistentSplitPackItemCollection PackableItemParts
		{
			get
			{
				if (fPackableItemParts == null)
				{
					fPackableItemParts = new NonPersistentSplitPackItemCollection(PackItemRelation, this);
					RegisterEditableChildObject(fPackableItemParts);
				}
				return fPackableItemParts;
			}
		}

		NonPersistentSplitPackItemCollection fPackableItemParts;

		public ZBool DoSplit()
		{
			var doResult = ZBool.False;
			var splitParts = PackableItemParts;
			if (splitParts.Any() && !PackItemRelation.IsPacked)
			{
				var currentItemIndex = PackableItem.CUI_Sequence;
				var packingList = PackableItem.PackingList;
				using (packingList.PackableItemSequenceNumberGenerator.GetLineNumberSuspender())
				{
					var sequenceOffset = splitParts.Count - 1;
					packingList.PackableItems.Where(x => x.CUI_Sequence > currentItemIndex).ForEach(x =>
					{
						x.CUI_Sequence = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(x.CUI_Sequence + sequenceOffset);
					});
					var cloneSplitItems = splitParts.Cast<NonPersistentSplitPackItem>().OrderBy(o => o.Sequence).Select((x, index) =>
					{
						var cloneArgs = new BusinessObjectCloneArgs(new[] { CusPackableItem.Schema.CUI_Sequence, CusPackableItem.Schema.CUI_GoodsDescription, CusPackableItem.Schema.CUI_PackableQty,
											CusPackableItem.Schema.CUI_PackableUQ, CusPackableItem.Schema.CUI_NetWeight, CusPackableItem.Schema.CUI_NetWeightUQ });
						var newPackableItem = (CusPackableItem)PackableItem.Clone(cloneArgs);
						newPackableItem.CUI_ClusterKey = packingList.CUL_ClusterKey;
						newPackableItem.CUI_GoodsDescription = x.GoodsDescription;
						newPackableItem.CUI_PackableQty = x.PackableQuantity;
						newPackableItem.CUI_PackableUQ = x.PackableUQ;
						newPackableItem.CUI_NetWeight = x.NetWeight;
						newPackableItem.CUI_NetWeightUQ = x.NetWeightUQ;
						newPackableItem.CUI_Sequence = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(currentItemIndex + index);
						return new CusPackageCusPackableItemRelation(Package, newPackableItem);
					});
					Package.PackableItemRelataions.AddRange(cloneSplitItems);
					PackableItem.Delete();
				}
				packingList.PackableItemSequenceNumberGenerator.ReCalculateAll();
				doResult = true;
			}
			return doResult;
		}

		#region ISequenceNumberHeader
		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(PackableItemParts);

		internal ShortSequenceNumberGenerator PackableItemPartSequenceNumberGenerator => fPackableItemPartSequenceNumberGenerator ?? (fPackableItemPartSequenceNumberGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator fPackableItemPartSequenceNumberGenerator;
		#endregion
	}
}
