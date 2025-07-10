using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class DraftHouseBillContainer : DocDataObject
	{
		#region Ctor

		public DraftHouseBillContainer(object identifier = default)
			: base(identifier)
		{
		}

		#endregion

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Seal

		public ZString Seal
		{
			get => seal;
			set
			{
				if (SetNonPersistentPropertyValue(SealInfo, ref seal, value))
				{
					Validate(SealInfo);
				}
			}
		}

		ZString seal;

		public ZPropertyInfo SealInfo => GetZPropertyInfo(nameof(Seal));

		#endregion

		#region SecondSeal

		public ZString SecondSeal
		{
			get => secondSeal;
			set
			{
				if (SetNonPersistentPropertyValue(SecondSealInfo, ref secondSeal, value))
				{
					Validate(SecondSealInfo);
				}
			}
		}

		ZString secondSeal;

		public ZPropertyInfo SecondSealInfo => GetZPropertyInfo(nameof(SecondSeal));

		#endregion

		#region ThirdSeal

		public ZString ThirdSeal
		{
			get => thirdSeal;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdSealInfo, ref thirdSeal, value))
				{
					Validate(ThirdSealInfo);
				}
			}
		}

		ZString thirdSeal;

		public ZPropertyInfo ThirdSealInfo => GetZPropertyInfo(nameof(ThirdSeal));

		#endregion

		#region Type

		public ContainerType Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ContainerType type;

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}

		IMeasurement grossWeight;

		#endregion

		#region VolumeCapacity

		public IMeasurement VolumeCapacity
		{
			get => volumeCapacity;
			set => volumeCapacity = SetChild(volumeCapacity, value);
		}

		IMeasurement volumeCapacity;

		#endregion

		#region PackageType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}

		ICodeDescription packType;

		#endregion

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
					Validate(PackCountInfo);
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

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
	}
}
