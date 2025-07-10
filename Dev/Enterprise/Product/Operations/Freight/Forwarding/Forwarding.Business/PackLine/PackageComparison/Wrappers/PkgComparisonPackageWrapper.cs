using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	class PkgComparisonPackageWrapper : IComparisonPackageWrapper
	{
		public PkgComparisonPackageWrapper(PkgPackage package, bool requiresSecuredCargoFromWarehouse)
		{
			Argument.NotNull(package, nameof(package));

			this.package = package;
			this.requiresSecuredCargoFromWarehouse = requiresSecuredCargoFromWarehouse;
		}

		readonly PkgPackage package;
		readonly bool requiresSecuredCargoFromWarehouse;

		public ZString Name => package.PackageIDWithFallback;

		public ZString ContainerNumber => ZString.Empty;

		public ZString CommodityCode => package.KP_RH_NKCommodityCode;

		public ZString HarmonisedCode => package.KP_HSCode;

		public ZString PackTypeCode => package.KP_F3_NKPackType;

		public ZString InspectionTypeCode => package.ScreeningMethod.IsEmpty && requiresSecuredCargoFromWarehouse
			? FreightDataRegistry.AviationSecurity_Unknown_Code
			: package.ScreeningMethod;

		public ZBool IsHighRisk => package.IsHighRisk;

		public ZString AdditionalInspectionTypeCode => package.AdditionalScreeningMethod;

		public ZDecimal Length => package.KP_Length;

		public ZDecimal Width => package.KP_Width;

		public ZDecimal Height => package.KP_Height;

		public ZString UnitOfLength => package.KP_DimensionUQ;

		public ZString MarksAndNumbers => package.KP_MarksAndNumbers;

		public ZInt PackageCount => package.KP_PackageQty;

		public ZDecimal Volume => package.KP_Volume;

		public ZDecimal PkgPackageCollection_TotalVolume => package.KP_Volume;

		public ZString UnitOfVolume => package.KP_VolumeUQ;

		public ZDecimal Weight => package.KP_Weight;

		public ZDecimal PkgPackageCollection_TotalWeight => package.KP_Weight;

		public ZString UnitOfWeight => package.KP_WeightUQ;

		public ZString GoodsDescription => package.KP_GoodsDescription;

		public ZBool RequiresTemperatureControl => package.KP_RequiresTemperatureControl;

		public ZDecimal RequiredTemperatureMinimum => package.KP_RequiredTemperatureMinimum;

		public ZDecimal RequiredTemperatureMaximum => package.KP_RequiredTemperatureMaximum;

		public ZString RequiredTemperatureUnit => package.KP_RequiredTemperatureUnit;
	}
}
