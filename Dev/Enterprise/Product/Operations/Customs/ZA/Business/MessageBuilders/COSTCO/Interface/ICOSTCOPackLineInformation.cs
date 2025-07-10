using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface ICOSTCOPackLineInformation : IGID_GoodsItemDetails
		, IFTX_Condition
		, IFTX_ContentsFound
		, IFTX_ContentsShouldBe
		, IFTX_DescriptionOfGoods
		, IMEA_GrossWeight
		, IMEA_GrossWeightFound
		, IMEA_GrossLitres
		, IMEA_GrossLitresFound
		, IPCI_PackageIdentification
		, ISGP_SplitGoodsPlacement
	{
	}

	public interface IGID_GoodsItemDetails
	{
		ZString GoodsLineNumber { get; }
		ZInt NumberOfPackages { get; }
		ZString TypeOfPackages { get; }
		ZString CargoTypeIndicator { get; }
		ZInt NumberOfPackagesPackedUnpacked { get; }
	}

	public interface IFTX_Condition
	{
		ZString PackageCondition { get; }
		ZString ConditionDescription { get; }
	}

	public interface IFTX_ContentsFound
	{
		ZString ContentsFoundToBe { get; }
	}

	public interface IFTX_ContentsShouldBe
	{
		ZString ExcessShortIndicator { get; }
		ZString ContentsShouldToBe { get; }
	}

	public interface IFTX_DescriptionOfGoods
	{
		ZString DescriptionOfGoods { get; }
	}

	public interface IMEA_GrossWeight
	{
		ZDecimal GrossWeightInKilograms { get; }
	}

	public interface IMEA_GrossWeightFound
	{
		ZDecimal GrossWeightFoundInKilograms { get; }
	}

	public interface IMEA_GrossLitres
	{
		ZDecimal VolumeInLitres { get; }
	}

	public interface IMEA_GrossLitresFound
	{
		ZDecimal VolumeOutturnedInLitres { get; }
	}

	public interface IPCI_PackageIdentification
	{
		ZString MarksAndNumbers { get; }
	}

	public interface ISGP_SplitGoodsPlacement
	{
		ZString ContainerNumber { get; }
		ZInt ContainerPackageContent { get; }
	}
}
