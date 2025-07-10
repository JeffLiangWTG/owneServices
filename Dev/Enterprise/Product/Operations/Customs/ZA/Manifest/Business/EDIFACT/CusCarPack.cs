using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarPack : ICusCarPackage
	{
		public CusCarPack(AsycudaPack asycudaPack)
		{
			this.asycudaPack = Argument.NotNull(asycudaPack, "asycudaPack");
		}

		readonly AsycudaPack asycudaPack;
		AsycudaBill Bill => asycudaPack.Bill;

		public AsycudaContainer Container => asycudaPack.Container;

		#region ICusCarPack

		ZString ICusCarPackage.ContainerNumber => asycudaPack.Container?.ACN_ContainerNumber ?? ZString.Empty;

		ZInt ICusCarPackage.NumberOfPacks => asycudaPack.APA_PackQty;

		ZString ICusCarPackage.PackUQ => asycudaPack.APA_PackUQ;

		ZString ICusCarPackage.Description => asycudaPack.APA_GoodsDescription.IsEmpty ? Bill.ABL_GoodsDescription : asycudaPack.APA_GoodsDescription;

		CargoStatusIndicator ICusCarPackage.CargoStatusIndicator => Bill.CargoStatusIndicator;

		ZDecimal ICusCarPackage.GrossVolumeInM3 => new ZVolume(asycudaPack.APA_Volume, asycudaPack.APA_VolumeUQ).InCubicMetres;

		ZDecimal ICusCarPackage.GrossVolume => IsVolumeInLitre ? asycudaPack.APA_Volume : ((ICusCarPackage)this).GrossVolumeInM3.Round(0);

		ZString ICusCarPackage.GrossVolumeUnitCode => IsVolumeInLitre ? Constants.VolumeUnitCode.Litre : Constants.VolumeUnitCode.CubicMetre;

		ZBool IsVolumeInLitre => asycudaPack.APA_VolumeUQ == Core.Constants.Volume.Litre;

		ZDecimal ICusCarPackage.GrossMassInKilos => new ZWeight(asycudaPack.APA_Weight, asycudaPack.APA_WeightUQ).InKilogramsSafe;

		ZString ICusCarPackage.UNDGClass => asycudaPack.UNDGs?.FirstItemForBinding[0]?.DI_IMOClass ?? ZString.Empty;

		ZString ICusCarPackage.UNDGNumber
		{
			get
			{
				return asycudaPack.UNDGs?.FirstItemForBinding[0]?.UNDGSubstance?.DG_UNNO ?? ZString.Empty;
			}
		}

		ZString ICusCarPackage.MarksAndNumbers => asycudaPack.APA_MarksAndNumbers.IsEmpty ? Bill.ABL_MarksAndNumbers : asycudaPack.APA_MarksAndNumbers;

		ZString ICusCarPackage.CommodityCode => asycudaPack.APA_CommodityCode;

		ZString ICusCarPackage.VINNumber => asycudaPack.APA_VINNumber;

		#endregion
	}
}
