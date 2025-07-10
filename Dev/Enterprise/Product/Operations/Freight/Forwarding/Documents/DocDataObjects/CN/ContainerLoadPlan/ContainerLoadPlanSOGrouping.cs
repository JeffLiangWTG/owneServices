using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	class ContainerLoadPlanSOGrouping : DocDataObject
	{
		public ContainerLoadPlanSOGrouping(string identifier) : base(identifier)
		{
		}

		#region SONumber

		public ZString SONumber
		{
			get => soNumber;
			set
			{
				if (SetNonPersistentPropertyValue(SONumberInfo, ref soNumber, value))
				{
					Validate(SONumberInfo);
				}
			}
		}

		ZString soNumber;

		public ZPropertyInfo SONumberInfo => GetZPropertyInfo(nameof(SONumber));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region Quantity

		public ZInt Quantity
		{
			get => quantity;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value))
				{
					Validate(QuantityInfo);
				}
			}
		}

		ZInt quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion

		#region PackageType

		public CodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		CodeDescription packageType;

		#endregion

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

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		#endregion

		#region Volume

		public Measurement Volume
		{
			get => volume;
			set => volume = SetChild(volume, value);
		}

		Measurement volume;

		#endregion

		#region DangerousGoods

		public IReadOnlyCollection<DangerousGood> DangerousGoods
		{
			get => dangerousGoods;
			set => dangerousGoods = SetChildCollection(dangerousGoods, value ?? System.Array.Empty<DangerousGood>());
		}

		IReadOnlyCollection<DangerousGood> dangerousGoods;

		#endregion

		#region DangerousGoodsDescription

		public ZString DangerousGoodsDescription
		{
			get => dangerousGoodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(DangerousGoodsDescriptionInfo, ref dangerousGoodsDescription, value))
				{
					Validate(DangerousGoodsDescriptionInfo);
				}
			}
		}

		ZString dangerousGoodsDescription;

		public ZPropertyInfo DangerousGoodsDescriptionInfo => GetZPropertyInfo(nameof(DangerousGoodsDescription));

		#endregion
	}
}
