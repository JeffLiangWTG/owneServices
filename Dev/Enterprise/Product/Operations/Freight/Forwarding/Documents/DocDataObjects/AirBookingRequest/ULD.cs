using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ULD : DocDataObject
	{
		public ULD(object id)
			: base(id)
		{
		}

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

		#region ContainerCount

		public ZInt ContainerCount
		{
			get => containerCount;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerCountInfo, ref containerCount, value))
				{
					Validate(ContainerCountInfo);
				}
			}
		}

		ZInt containerCount;

		public ZPropertyInfo ContainerCountInfo => GetZPropertyInfo(nameof(ContainerCount));

		#endregion

		public UldContainerType Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		UldContainerType type;

		public Measurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}

		Measurement grossWeight;

		public Measurement TareWeight
		{
			get => tareWeight;
			set => tareWeight = SetChild(tareWeight, value);
		}

		Measurement tareWeight;

		public Measurement GoodsWeight
		{
			get => goodsWeight;
			set => goodsWeight = SetChild(goodsWeight, value);
		}

		Measurement goodsWeight;

		public Measurement ContainerVolume
		{
			get => containerVolume;
			set => containerVolume = SetChild(containerVolume, value);
		}

		Measurement containerVolume;

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion

	}
}
