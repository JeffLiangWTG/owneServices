using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IComparisonPackageWrapper
	{
		ZString Name { get; }

		ZString ContainerNumber { get; }

		ZString CommodityCode { get; }

		ZString HarmonisedCode { get; }

		ZString PackTypeCode { get; }

		ZString InspectionTypeCode { get; }

		ZBool IsHighRisk { get; }

		ZString AdditionalInspectionTypeCode { get; }

		ZDecimal Length { get; }

		ZDecimal Width { get; }

		ZDecimal Height { get; }

		ZString UnitOfLength { get; }

		ZString MarksAndNumbers { get; }

		ZInt PackageCount { get; }

		ZDecimal Volume { get; }

		ZDecimal PkgPackageCollection_TotalVolume { get; }

		ZString UnitOfVolume { get; }

		ZDecimal Weight { get; }

		ZDecimal PkgPackageCollection_TotalWeight { get; }

		ZString UnitOfWeight { get; }

		ZString GoodsDescription { get; }

		ZBool RequiresTemperatureControl { get; }

		ZDecimal RequiredTemperatureMinimum { get; }

		ZDecimal RequiredTemperatureMaximum { get; }

		ZString RequiredTemperatureUnit { get; }
	}
}
