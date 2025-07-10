using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class PackedOrderLine : DocDataObject
	{
		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region OrderLineNumber

		public ZString OrderLineNumber
		{
			get => orderLineNumber;
			set
			{
				if (SetNonPersistentPropertyValue(OrderLineNumberInfo, ref orderLineNumber, value))
				{
					Validate(OrderLineNumberInfo);
				}
			}
		}

		ZString orderLineNumber;

		public ZPropertyInfo OrderLineNumberInfo => GetZPropertyInfo(nameof(OrderLineNumber));

		#endregion

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

		#region ItemNumber

		public ZString ProductCode
		{
			get => productCode;
			set
			{
				if (SetNonPersistentPropertyValue(ItemNumberInfo, ref productCode, value))
				{
					Validate(ItemNumberInfo);
				}
			}
		}

		ZString productCode;

		public ZPropertyInfo ItemNumberInfo => GetZPropertyInfo(nameof(ProductCode));

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}
		IMeasurement grossWeight;

		#endregion

		#region GrossVolume

		public IMeasurement GrossVolume
		{
			get => grossVolume;
			set => grossVolume = SetChild(grossVolume, value);
		}

		IMeasurement grossVolume;

		#endregion

		#region MarksAndNos

		public ZString MarksAndNos
		{
			get => marksAndNos;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNosInfo, ref marksAndNos, value))
				{
					Validate(MarksAndNosInfo);
				}
			}
		}

		ZString marksAndNos;

		public ZPropertyInfo MarksAndNosInfo => GetZPropertyInfo(nameof(MarksAndNos));

		#endregion
	}
}
