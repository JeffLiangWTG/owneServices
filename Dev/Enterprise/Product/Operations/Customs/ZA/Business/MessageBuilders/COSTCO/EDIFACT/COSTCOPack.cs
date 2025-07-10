using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class COSTCOPack : ICOSTCOPackLineInformation
	{
		public COSTCOPack(AsycudaPack pack, ZInt index)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
			this.index = index;
		}

		ZString IGID_GoodsItemDetails.GoodsLineNumber => (index + 1).ToString(CultureInfo.InvariantCulture);

		ZInt IGID_GoodsItemDetails.NumberOfPackages => pack.APA_PackQty;

		ZString IGID_GoodsItemDetails.TypeOfPackages => pack.APA_PackUQ;

		ZString IGID_GoodsItemDetails.CargoTypeIndicator => pack.Outturn?.C5_CargoType ?? ZString.Empty;

		ZInt IGID_GoodsItemDetails.NumberOfPackagesPackedUnpacked => pack.Outturn.C5_PackagesOutturned;

		ZString IFTX_Condition.PackageCondition => pack.Outturn.C5_PackageCondition;

		ZString IFTX_Condition.ConditionDescription => pack.Outturn.PackCondDesc;

		ZString IFTX_ContentsFound.ContentsFoundToBe => pack.Outturn.C5_GoodsDescription;

		ZString IFTX_ContentsShouldBe.ExcessShortIndicator => IsVOR || IsEOR
			? (ZString)ExcessShortIndicatorList.Codes.Unknown
			: pack.Outturn.ExcessShortInd;

		AsycudaManifestHeader Header => pack.Bill?.Header;

		ZBool IsVOR => Header?.IsVOR ?? false;

		ZBool IsEOR => Header?.IsEOR ?? false;

		ZString IFTX_ContentsShouldBe.ContentsShouldToBe => pack.Outturn.ContShouldBe;

		ZString IFTX_DescriptionOfGoods.DescriptionOfGoods => pack.APA_GoodsDescription;

		ZDecimal IMEA_GrossWeight.GrossWeightInKilograms => new ZWeight(pack.APA_Weight, pack.APA_WeightUQ).InKilogramsSafe;

		ZDecimal IMEA_GrossWeightFound.GrossWeightFoundInKilograms => new ZWeight(pack.Outturn.C5_WeightOutturned, pack.Outturn.C5_WeightOutturnedUQ).InKilogramsSafe;

		ZDecimal IMEA_GrossLitres.VolumeInLitres => new ZVolume(pack.APA_Volume, pack.APA_VolumeUQ).InCubicMetres * 1000;

		ZDecimal IMEA_GrossLitresFound.VolumeOutturnedInLitres => new ZVolume(pack.Outturn.C5_VolumeOutturned, pack.Outturn.C5_VolumeOutturnedUQ).InCubicMetres * 1000;

		ZString IPCI_PackageIdentification.MarksAndNumbers => pack.APA_MarksAndNumbers;

		ZString ISGP_SplitGoodsPlacement.ContainerNumber => pack.Container?.ACN_ContainerNumber ?? "1";

		ZInt ISGP_SplitGoodsPlacement.ContainerPackageContent => pack.Outturn.C5_PackagesOutturned;

		readonly AsycudaPack pack;
		readonly ZInt index;
	}
}
