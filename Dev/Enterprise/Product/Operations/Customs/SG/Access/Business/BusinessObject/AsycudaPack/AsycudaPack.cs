using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString APA_GoodsDescription
		{
			get => base.APA_GoodsDescription;
			set
			{
				var oldValue = APA_GoodsDescription;
				base.APA_GoodsDescription = value;
				if (!IsCopying && oldValue != value)
				{
					var packedItem = base.PackedItem;
					if (packedItem.API_GoodsDescription.IsEmpty)
					{
						packedItem.API_GoodsDescription = APA_GoodsDescription;
					}
				}
			}
		}

		public override ZInt APA_PackQty
		{
			get => base.APA_PackQty;
			set
			{
				var oldValue = APA_PackQty;
				base.APA_PackQty = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateCustomsQtyAndUnitQtyOnCountryPack();
				}
			}
		}

		public override ZDecimal LinePrice
		{
			get => base.LinePrice;
			set
			{
				var oldValue = LinePrice;
				base.LinePrice = value;
				if (!IsCopying && oldValue != LinePrice)
				{
					UpdateCustomsValueOnCountryPack();
				}
			}
		}

		public override ZString LinePriceCurrency
		{
			get => base.LinePriceCurrency;
			set
			{
				var oldValue = LinePriceCurrency;
				base.LinePriceCurrency = value;
				if (!IsCopying && oldValue != LinePriceCurrency)
				{
					UpdateCustomsValueOnCountryPack();
				}
			}
		}

		protected override bool LinePriceCurrency_ReadOnly => true;

		public override ZString APA_PackUQ
		{
			get => base.APA_PackUQ;
			set
			{
				var oldValue = APA_PackUQ;
				base.APA_PackUQ = value;
				if (!IsCopying && oldValue != value)
				{
					UpdateCustomsQtyAndUnitQtyOnCountryPack();
				}
			}
		}

		public bool HasValidTradeNetPermitNumber => Factory.GetValue(ref hasValidTradeNetPermitCached, () =>
		{
			return PackedItem?.CustomsEntryNumbers?.Cast<AsycudaPackedItemEntryNum>().Any(c => c.CE_EntryType == ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit && !c.CE_EntryNum.IsEmpty) ?? false;
		});

		CachedProperty<bool> hasValidTradeNetPermitCached;

		[ChildEditable]
		[ChildEditableTestExclude]
		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;
		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaContainer Container => (AsycudaContainer)base.Container;
		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		void UpdateCustomsValueOnCountryPack()
		{
			var packedItem = PackedItem;
			packedItem.DefaultCustomsValueFromPack(this);
		}

		void UpdateCustomsQtyAndUnitQtyOnCountryPack()
		{
			var packedItem = PackedItem;
			packedItem.DefaultCustomsQtyAndUnitQtyFromPack(this);
		}

		Integration.Customs.ASYCUDA.IAsycudaPackedItem Integration.Customs.ASYCUDA.IAsycudaPackWithOnePackedItemRelationship.PackedItem => base.PackedItem;
	}
}
