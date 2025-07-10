using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class ForwardersCargoReceiptDetail : ForwardersCargoReceipt
	{
		#region ContainerPackingInfos

		public IReadOnlyCollection<ContainerPackingInfo> ContainerPackingInfos
		{
			get => containerPackingInfos;
			set => containerPackingInfos = SetChildCollection(containerPackingInfos, value);
		}

		IReadOnlyCollection<ContainerPackingInfo> containerPackingInfos;

		#region PackageCount

		public ZInt PackageCount
		{
			get => packageCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackageCountInfo, ref packageCount, value))
				{
					Validate(PackageCountInfo);
				}
			}
		}

		ZInt packageCount;

		public ZPropertyInfo PackageCountInfo => GetZPropertyInfo(nameof(PackageCount));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		#endregion

		#region OrderLineCount

		public ZInt OrderLineCount
		{
			get => orderLineCount;
			set
			{
				if (SetNonPersistentPropertyValue(OrderLineCountInfo, ref orderLineCount, value))
				{
					Validate(OrderLineCountInfo);
				}
			}
		}

		ZInt orderLineCount;

		public ZPropertyInfo OrderLineCountInfo => GetZPropertyInfo(nameof(OrderLineCount));

		#endregion

		#region ItemNumberCount

		public ZInt ItemNumberCount
		{
			get => itemNumberCount;
			set
			{
				if (SetNonPersistentPropertyValue(ItemNumberCountInfo, ref itemNumberCount, value))
				{
					Validate(ItemNumberCountInfo);
				}
			}
		}

		ZInt itemNumberCount;

		public ZPropertyInfo ItemNumberCountInfo => GetZPropertyInfo(nameof(ItemNumberCount));

		#endregion

		#region PackedQuantity

		public ZDecimal PackedQuantity
		{
			get => packedQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(PackedQuantityInfo, ref packedQuantity, value))
				{
					Validate(PackedQuantityInfo);
				}
			}
		}

		ZDecimal packedQuantity;

		public ZPropertyInfo PackedQuantityInfo => GetZPropertyInfo(nameof(PackedQuantity));

		#endregion

		#region UnitOfQuantity

		public ICodeDescription UnitOfQuantity
		{
			get => unitOfQuantity;
			set => unitOfQuantity = SetChild(unitOfQuantity, value);
		}

		ICodeDescription unitOfQuantity;

		#endregion

		#region GrossVolume

		public IMeasurement GrossVolume
		{
			get => grossVolume;
			set => grossVolume = SetChild(grossVolume, value);
		}

		IMeasurement grossVolume;

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}
		IMeasurement grossWeight;

		#endregion

		#endregion

		#region DescriptionOfGoods

		public ZString DescriptionOfGoods
		{
			get => descriptionOfGoods;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionOfGoodsInfo, ref descriptionOfGoods, value))
				{
					Validate(DescriptionOfGoodsInfo);
				}
			}
		}

		ZString descriptionOfGoods;

		public ZPropertyInfo DescriptionOfGoodsInfo => GetZPropertyInfo(nameof(DescriptionOfGoods));

		#endregion

		#region DetailedDescriptionOfGoods

		public ZString DetailedDescriptionOfGoods
		{
			get => detailedDescriptionOfGoods;
			set
			{
				if (SetNonPersistentPropertyValue(DetailedDescriptionOfGoodsInfo, ref detailedDescriptionOfGoods, value))
				{
					Validate(DetailedDescriptionOfGoodsInfo);
				}
			}
		}

		ZString detailedDescriptionOfGoods;

		public ZPropertyInfo DetailedDescriptionOfGoodsInfo => GetZPropertyInfo(nameof(DetailedDescriptionOfGoods));

		#endregion
	}
}
