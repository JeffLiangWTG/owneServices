using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Creates a filter for parts that should be grouped by distinct filter values and calculated separately.
	/// <see cref="IPartFilter"/>
	/// </summary>
	internal static class PartFilterProvider
	{
		/// <summary>
		/// Filter for warehouse jobs on docket only
		/// </summary>
		public static IPartFilter CreateForDocket()
		{
			return new WarehousePartFilter();
		}

		/// <summary>
		/// Filter for warehouse jobs on docket and product
		/// </summary>
		public static IPartFilter CreateForDocketProduct()
		{
			return new WarehousePartFilter()
			{
				IncludeProduct = true
			};
		}

		public static IPartFilter CreateForDocketProductLine()
		{
			return new WarehouseProductLineFilter();
		}

		public static IPartFilter CreateForDocketPackage()
			=> new WarehousePackageFilter()
			{
				IncludePackage = true
			};

		/// <summary>
		/// Filter for warehouse jobs on docket, product and package type
		/// </summary>
		public static IPartFilter CreateForDocketProductPackageType()
		{
			return new WarehousePartFilter()
			{
				IncludeProduct = true,
				IncludePackageType = true
			};
		}

		/// <summary>
		/// Filter for warehouse jobs on location only
		/// </summary>
		public static IPartFilter CreateForLocation()
		{
			return new LocationPartFilter();
		}

		/// <summary>
		/// Filter for local transport jobs
		/// </summary>
		public static IPartFilter CreateForLocalTransport()
		{
			return new LocalTransportPartFilter();
		}

		/// <summary>
		/// Filter for Container
		/// </summary>
		public static IPartFilter CreateForContainer()
			=> new ContainerPartFilter();

		abstract class PartFilterCommon : IHasPartDimensions
		{
			public bool HasPalletID => false;
			public bool HasCommodity => false;
			public bool HasWarehouse => false;
			public bool HasPalletized => false;
			public bool HasContainerOwnership => false;
			public bool HasChargeGroupToUse => false;

			public virtual bool HasProductAttributes => false;
			public virtual bool HasLocation => false;
			public virtual bool HasDocketReference => false;
			public virtual bool HasWarehouseLine => false;
			public virtual bool HasCartageLegPK => false;
			public virtual bool HasContainerNumber => false;
			public virtual bool HasContainerType => false;
			public virtual bool HasProduct => false;
			public virtual bool HasPackageType => false;
			public virtual bool HasYardUnitType => false;
			public virtual bool HasYardUnitLoad => false;
			public virtual bool HasYardUnitClient => false;

			public virtual string GetDocketReference(PartWithDimensions part)
				=> HasDocketReference && part.HasDimensions.HasDocketReference ? part.Part.DocketReference : string.Empty;

			public LocationMeasure GetLocation(PartWithDimensions part)
				=> HasLocation && part.HasDimensions.HasLocation ? part.Part.Location : LocationMeasure.Empty;

			public ProductAttributesMeasure GetProductAttributes(PartWithDimensions part)
				=> HasProductAttributes && part.HasDimensions.HasProductAttributes ? part.Part.ProductAttributes : ProductAttributesMeasure.Empty;

			public Guid? GetProductPk(PartWithDimensions part)
				=> HasProduct && part.HasDimensions.HasProduct ? part.Part.ProductPk : null;

			public Guid? GetCartageLegPk(PartWithDimensions part)
				=> HasCartageLegPK && part.HasDimensions.HasCartageLegPK ? part.Part.CartageLegPK : null;

			public Guid? GetContainerTypePk(PartWithDimensions part)
				=> HasContainerType && part.HasDimensions.HasContainerType ? part.Part.ContainerTypePk : null;

			public ZString? GetContainerNumber(PartWithDimensions part)
				=> HasContainerNumber && part.HasDimensions.HasContainerNumber ? part.Part.ContainerNumber : null;
		}

		sealed class WarehousePartFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasDocketReference => true;
			public override bool HasProduct => IncludeProduct;
			public override bool HasProductAttributes => IncludeProduct;
			public override bool HasPackageType => IncludePackageType;

			public bool IncludeProduct { get; set; }
			public bool IncludePackageType { get; set; }

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part, y.Part);

			public bool Equals(IRateablePart x, IRateablePart y)
				=> EqualDocket(x, y)
				&& EqualProduct(x, y)
				&& EqualPackageType(x, y);

			bool EqualDocket(IRateablePart x, IRateablePart y)
				=> x.DocketReference == y.DocketReference;
			bool EqualProduct(IRateablePart x, IRateablePart y)
				=> !IncludeProduct || (x.ProductPk == y.ProductPk && Equals(x.ProductAttributes, y.ProductAttributes));
			bool EqualPackageType(IRateablePart x, IRateablePart y)
				=> !IncludePackageType || x.PackageType == y.PackageType;

			public int GetHashCode(PartWithDimensions x)
			{
				return (x.Part.DocketReference?.GetHashCode() ?? 0)
					^ (IncludeProduct ? GetProductHashCode(x.Part) : 0)
					^ (IncludePackageType ? (x.Part.PackageType?.GetHashCode() ?? 0) : 0);
			}

			static int GetProductHashCode(IRateablePart x)
				=> x.ProductPk.GetHashCode()
				^ (x.ProductAttributes?.GetHashCode() ?? 0);

			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> dimensions.HasDocketReference
				|| (IncludeProduct && dimensions.HasProduct)
				|| (IncludePackageType && dimensions.HasPackageType);

			public override string GetDocketReference(PartWithDimensions keyPart)
				=> keyPart.Part.DocketReference;

			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
			{
				return (!partToCheck.HasDimensions.HasDocketReference || partToCheck.Part.DocketReference == keyPart.Part.DocketReference)
					&& (!IncludeProduct || SatisfiesProduct(keyPart, partToCheck))
					&& (!IncludePackageType || SatisfiesPackageType(keyPart, partToCheck));
			}

			bool SatisfiesProduct(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> !partToCheck.HasDimensions.HasProduct
				|| (partToCheck.Part.ProductPk == keyPart.Part.ProductPk && Equals(partToCheck.Part.ProductAttributes, keyPart.Part.ProductAttributes));

			bool SatisfiesPackageType(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> !partToCheck.HasDimensions.HasPackageType
				|| partToCheck.Part.PackageType == keyPart.Part.PackageType;
		}

		sealed class WarehouseProductLineFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasDocketReference => true;

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part, y.Part);

			bool Equals(IRateablePart x, IRateablePart y)
				=> EqualDocket(x, y)
				&& EqualWarehouseLine(x, y);

			bool EqualDocket(IRateablePart x, IRateablePart y)
				=> x.DocketReference == y.DocketReference;

			bool EqualWarehouseLine(IRateablePart x, IRateablePart y)
				=> x == y;

			public int GetHashCode(PartWithDimensions x)
				=> (x.Part.DocketReference?.GetHashCode() ?? 0)
				^ x.Part.GetHashCode();

			// To filter AutoRatingCalculatorParametersWithoutFilter.CreatedFilteredParametersIfNeeded
			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> dimensions.HasWarehouseLine;

			public override string GetDocketReference(PartWithDimensions keyPart)
				=> keyPart.Part.DocketReference;

			// To filter AmountByLineTable.PartWithDimensions
			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> (!partToCheck.HasDimensions.HasDocketReference || partToCheck.Part.DocketReference == keyPart.Part.DocketReference)
				&& (!partToCheck.HasDimensions.HasWarehouseLine || Equals(keyPart.Part, partToCheck.Part));
		}

		sealed class WarehousePackageFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasDocketReference => true;
			public override bool HasProduct => false;
			public override bool HasProductAttributes => false;
			public override bool HasPackageType => false;

			public bool IncludePackage { get; set; }

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part, y.Part);

			bool Equals(IRateablePart x, IRateablePart y)
				=> !IncludePackage || x == y;

			public int GetHashCode(PartWithDimensions x)
				=> IncludePackage ? x.Part.GetHashCode() : 0;

			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> true;

			public override string GetDocketReference(PartWithDimensions keyPart)
				=> keyPart.Part.DocketReference;

			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> Equals(keyPart.Part, partToCheck.Part);
		}

		sealed class LocationPartFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasLocation => true;

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part.Location, y.Part.Location);

			public int GetHashCode(PartWithDimensions x)
				=> x.Part.Location?.GetHashCode() ?? 0;

			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> dimensions.HasLocation;

			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> !partToCheck.HasDimensions.HasLocation || Equals(keyPart.Part.Location, partToCheck.Part.Location);
		}

		sealed class LocalTransportPartFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasCartageLegPK => true;
			public override bool HasContainerType => true;
			public override bool HasContainerNumber => true;

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> Leg(x) == Leg(y)
				&& ContainerTypePk(x) == ContainerTypePk(y)
				&& x.Part.ContainerNumber == y.Part.ContainerNumber;

			static Guid? Leg(PartWithDimensions x)
				=> x.HasDimensions.HasCartageLegPK ? x.Part.CartageLegPK : null;

			static Guid? ContainerTypePk(PartWithDimensions x)
				=> x.HasDimensions.HasContainerType ? x.Part.ContainerTypePk : null;

			public int GetHashCode(PartWithDimensions x)
				=> Leg(x).GetHashCode()
				 ^ ContainerTypePk(x).GetHashCode()
				 ^ (x.Part.ContainerNumber?.GetHashCode() ?? 0);

			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> dimensions.HasCartageLegPK
				|| dimensions.HasContainerType
				|| dimensions.HasContainerNumber;

			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> (!partToCheck.HasDimensions.HasCartageLegPK || keyPart.Part.CartageLegPK == partToCheck.Part.CartageLegPK)
				&& (!partToCheck.HasDimensions.HasContainerType || keyPart.Part.ContainerTypePk == partToCheck.Part.ContainerTypePk)
				&& (!partToCheck.HasDimensions.HasContainerNumber || keyPart.Part.ContainerNumber == partToCheck.Part.ContainerNumber);
		}

		sealed class ContainerPartFilter : PartFilterCommon, IPartFilter
		{
			public override bool HasContainerNumber => true;

			public override bool HasContainerType => true;

			public bool Equals(PartWithDimensions x, PartWithDimensions y)
				=> EqualsContainerType(x, y) && EqualsContainerNumber(x, y);

			bool EqualsContainerType(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part.ContainerTypePk, y.Part.ContainerTypePk);

			bool EqualsContainerNumber(PartWithDimensions x, PartWithDimensions y)
				=> Equals(x.Part.ContainerNumber, y.Part.ContainerNumber);

			public bool Satisfies(PartWithDimensions keyPart, PartWithDimensions partToCheck)
				=> (!partToCheck.HasDimensions.HasContainerType || EqualsContainerType(keyPart, partToCheck))
				&& (!partToCheck.HasDimensions.HasContainerNumber || EqualsContainerNumber(keyPart, partToCheck));

			public int GetHashCode(PartWithDimensions obj)
				=> obj.Part.ContainerTypePk.GetHashCode() ^ (obj.Part.ContainerNumber?.GetHashCode() ?? 0);

			public bool HasAnyDimensions(IHasPartDimensions dimensions)
				=> dimensions.HasContainerType
				|| dimensions.HasContainerNumber;
		}
	}
}

