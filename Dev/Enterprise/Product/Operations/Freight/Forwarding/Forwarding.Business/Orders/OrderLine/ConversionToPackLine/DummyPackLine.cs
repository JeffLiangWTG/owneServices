using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class DummyPackLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DummyPackLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public List<OrderLine> MergedOrderLines
		{
			get { return mergedOrderLines ?? (mergedOrderLines = new List<OrderLine>()); }
		}
		List<OrderLine> mergedOrderLines;

		#region Properties

		#region OuterPack

		[ReadOnly(true)]
		public ZInt OuterPacks
		{
			get { return fOuterPacks; }
			set
			{
				SetNonPersistentPropertyValue(OuterPacksInfo, ref fOuterPacks, value);
			}
		}

		ZInt fOuterPacks;

		public ZPropertyInfo OuterPacksInfo
		{
			get { return GetZPropertyInfo(nameof(OuterPacks)); }
		}

		#endregion

		#region OuterPackType

		[MaxLength(AutoJobPackLines.Schema.JL_F3_NKPackTypeMaxLength)]
		[ReadOnly(true)]
		public ZString OuterPackType
		{
			get { return outerPackType; }
			set
			{
				SetNonPersistentPropertyValue(OuterPackTypeInfo, ref outerPackType, value);
			}
		}

		ZString outerPackType;

		public ZPropertyInfo OuterPackTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OuterPackType)); }
		}

		#endregion

		#region InnerPack

		[ReadOnly(true)]
		public ZInt InnerPacks
		{
			get { return innerPacks; }
			set
			{
				SetNonPersistentPropertyValue(InnerPacksInfo, ref innerPacks, value);
			}
		}

		ZInt innerPacks;

		public ZPropertyInfo InnerPacksInfo
		{
			get { return GetZPropertyInfo(nameof(InnerPacks)); }
		}

		#endregion

		#region InnerPackType

		[MaxLength(AutoJobPackLines.Schema.JL_F3_NKPackTypeMaxLength)]
		[ReadOnly(true)]
		public ZString InnerPackType
		{
			get { return innerPackType; }
			set
			{
				SetNonPersistentPropertyValue(InnerPackTypeInfo, ref innerPackType, value);
			}
		}

		ZString innerPackType;

		public ZPropertyInfo InnerPackTypeInfo
		{
			get { return GetZPropertyInfo(nameof(InnerPackType)); }
		}

		#endregion

		#region UnitOfQuantity

		[MaxLength(AutoJobPackLines.Schema.JL_F3_NKPackTypeMaxLength)]
		[ReadOnly(true)]
		public ZString UnitOfQuantity
		{
			get { return unitOfQuantity; }
			set
			{
				SetNonPersistentPropertyValue(UnitOfQuantityInfo, ref unitOfQuantity, value);
			}
		}

		ZString unitOfQuantity;

		public ZPropertyInfo UnitOfQuantityInfo
		{
			get { return GetZPropertyInfo(nameof(UnitOfQuantity)); }
		}

		#endregion

		#region Volume

		[ReadOnly(true)]
		public ZDecimal Volume
		{
			get { return fVolume; }
			set
			{
				SetNonPersistentPropertyValue(VolumeInfo, ref fVolume, value);
			}
		}

		ZDecimal fVolume;

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(nameof(Volume)); }
		}

		#endregion

		#region VolumeUnit

		[MaxLength(AutoJobPackLines.Schema.JL_ActualVolumeUQMaxLength)]
		[ReadOnly(true)]
		public ZString VolumeUnit
		{
			get { return fVolumeUnit; }
			set
			{
				SetNonPersistentPropertyValue(VolumeUnitInfo, ref fVolumeUnit, value);
			}
		}

		ZString fVolumeUnit;

		public ZPropertyInfo VolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(VolumeUnit)); }
		}

		#endregion

		#region Weight

		[ReadOnly(true)]
		public ZDecimal Weight
		{
			get { return fWeight; }
			set
			{
				SetNonPersistentPropertyValue(WeightInfo, ref fWeight, value);
			}
		}

		ZDecimal fWeight;

		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(nameof(Weight)); }
		}

		#endregion

		#region WeightUnit

		[MaxLength(AutoJobPackLines.Schema.JL_ActualWeightUQMaxLength)]
		[ReadOnly(true)]
		public ZString WeightUnit
		{
			get { return fWeightUnit; }
			set
			{
				SetNonPersistentPropertyValue(WeightUnitInfo, ref fWeightUnit, value);
			}
		}

		ZString fWeightUnit;

		public ZPropertyInfo WeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(WeightUnit)); }
		}

		#endregion

		#region Description

		[MaxLength(AutoJobPackLines.Schema.JL_DescriptionMaxLength)]
		[ReadOnly(true)]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);
			}
		}

		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Origin

		[MaxLength(AutoJobPackLines.Schema.JL_RN_NKOriginMaxLength)]
		[ReadOnly(true)]
		public ZString Origin
		{
			get { return fOrigin; }
			set
			{
				SetNonPersistentPropertyValue(OriginInfo, ref fOrigin, value);
			}
		}

		ZString fOrigin;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		#endregion

		#region LinePrice

		[ReadOnly(true)]
		public ZDecimal LinePrice
		{
			get { return fLinePrice; }
			set
			{
				SetNonPersistentPropertyValue(LinePriceInfo, ref fLinePrice, value);
			}
		}

		ZDecimal fLinePrice;

		public ZPropertyInfo LinePriceInfo
		{
			get { return GetZPropertyInfo(nameof(LinePrice)); }
		}

		#endregion

		#region Products

		[ReadOnly(true)]
		[MaxLength(ProductsMaxLength)]
		public ZString Products
		{
			get { return fProducts; }
			set
			{
				SetNonPersistentPropertyValue(ProductsInfo, ref fProducts, value);
			}
		}

		ZString fProducts;

		public ZPropertyInfo ProductsInfo
		{
			get { return GetZPropertyInfo(nameof(Products)); }
		}

		public const int ProductsMaxLength = 2000;

		#endregion

		#region ContainerNumber

		[MaxLength(AutoJobOrderLine.Schema.JO_ContainerNumberMaxLength)]
		[ReadOnly(true)]
		public ZString ContainerNumber
		{
			get { return fContainerNumber; }
			set
			{
				SetNonPersistentPropertyValue(ContainerNumberInfo, ref fContainerNumber, value);
			}
		}

		ZString fContainerNumber;

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerNumber)); }
		}

		#endregion

		#endregion

		#region PK's

		public ZGuid ContainerPK { get; set; }

		#endregion
	}
}
