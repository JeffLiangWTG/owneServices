using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.TransportBookings.Document
{
	public sealed class FFMMessagePackage : DocDataObject
	{
		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
					Validate(ContainerNumberInfo);
				}
			}
		}
		ZString containerNumber;
		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

		public ZString WayBillNumber
		{
			get => wayBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(WayBillNumberInfo, ref wayBillNumber, value))
				{
					Validate(WayBillNumberInfo);
				}
			}
		}
		ZString wayBillNumber;
		public ZPropertyInfo WayBillNumberInfo => GetZPropertyInfo(nameof(WayBillNumber));

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}
		IUnloco portOfDestination;

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

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

		public ZDecimal Weight
		{
			get => weight;
			set
			{
				if (SetNonPersistentPropertyValue(WeightInfo, ref weight, value))
				{
					Validate(WeightInfo);
				}
			}
		}
		ZDecimal weight;
		public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

		public ZDecimal Volume
		{
			get => volume;
			set
			{
				if (SetNonPersistentPropertyValue(VolumeInfo, ref volume, value))
				{
					Validate(VolumeInfo);
				}
			}
		}
		ZDecimal volume;
		public ZPropertyInfo VolumeInfo => GetZPropertyInfo(nameof(Volume));

		public ZString WeightMetric
		{
			get => weightMetric;
			set
			{
				if (SetNonPersistentPropertyValue(WeightMetricInfo, ref weightMetric, value))
				{
					Validate(WeightMetricInfo);
				}
			}
		}
		ZString weightMetric;
		public ZPropertyInfo WeightMetricInfo => GetZPropertyInfo(nameof(WeightMetric));

		public ZString VolumeMetric
		{
			get => volumeMetric;
			set
			{
				if (SetNonPersistentPropertyValue(VolumeMetricInfo, ref volumeMetric, value))
				{
					Validate(VolumeMetricInfo);
				}
			}
		}
		ZString volumeMetric;
		public ZPropertyInfo VolumeMetricInfo => GetZPropertyInfo(nameof(VolumeMetric));
	}
}
